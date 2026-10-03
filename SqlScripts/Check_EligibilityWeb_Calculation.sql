/* =====================================================================
   בדיקת תוצאות מסך חישוב ההנחה בדפדפן  (MunicipalDiscountWeb)
   ---------------------------------------------------------------------
   Reproduces, in SSMS, exactly what EligibilityService.CalculateAsync
   computes for the "בדיקת זכאות" page, so a number the page shows can be
   checked against the database by hand.

   The household does NOT come from the database — the page reads it from
   the population-registry Excel file (רישום אוכלוסין). So paste the rows
   the page is showing into @Members in SECTION 1, in the same order the
   page lists them (ראש משפחה, בן/בת זוג, then the children), and the rest
   of the script is the page's own logic.

   Rules mirrored from EligibilityService / FamilyRegistryService:
     * IsAdult is Age > @AdultAge — STRICTLY over, so 18 is a minor.
     * Only ADULT incomes are summed. A minor's income never counts.
     * Exactly ONE adult child is counted at half: the FIRST adult child
       in file order, not the highest earner.
     * The divisor is the WHOLE household, minors included.
     * The page always calculates with today's date. It does NOT read
       RequestDate from tbDiscountRequests.
     * The cap belongs to the CODE (מבחן הכנסה 80% -> 780), not to the
       citizen, and is read from the version in force on that date.

   Safe to run: reads only, writes nothing.
   ===================================================================== */

SET NOCOUNT ON;

/* =====================================================================
   SECTION 0 — INPUTS. Set these to match what the page had on screen.
   ===================================================================== */

DECLARE @Year        int  = 2026;              -- the page's year picker
DECLARE @RequestDate date = CAST(GETDATE() AS date);  -- the page always uses today
DECLARE @AdultAge    int  = 18;                -- tbMainParameters: FamilyRegistryAdultAge
                                               -- (adult = Age > this, so 19+ by default)

-- Built from character codes so the script works no matter what encoding
-- the file is opened with. This is N'מבחן הכנסה'.
DECLARE @Category nvarchar(100) =
    NCHAR(1502)+NCHAR(1489)+NCHAR(1495)+NCHAR(1503)+NCHAR(32)+
    NCHAR(1492)+NCHAR(1499)+NCHAR(1504)+NCHAR(1505)+NCHAR(1492);

DECLARE @KindHead   nvarchar(20) = N'HEAD';
DECLARE @KindSpouse nvarchar(20) = N'SPOUSE';
DECLARE @KindChild  nvarchar(20) = N'CHILD';


/* =====================================================================
   SECTION 1 — THE HOUSEHOLD. Paste what the page shows.
   Seq must follow the page's row order: the first adult CHILD by Seq is
   the one whose income gets halved.
   Income for a minor may be left at 0 — the page gives minors no income
   box, and this script ignores it either way.
   ===================================================================== */

DECLARE @Members TABLE (
    Seq     int            NOT NULL PRIMARY KEY,
    FullName nvarchar(200) NULL,
    Kind    nvarchar(20)   NOT NULL,   -- HEAD / SPOUSE / CHILD
    Age     int            NOT NULL,
    Income  decimal(18,2)  NOT NULL
);

INSERT INTO @Members (Seq, FullName, Kind, Age, Income) VALUES
    (1, N'ראש משפחה', @KindHead,   45, 9000),
    (2, N'בן/בת זוג',  @KindSpouse, 43, 4000),
    (3, N'ילד ראשון',  @KindChild,  21, 6000),
    (4, N'ילד שני',    @KindChild,  19, 3000),
    (5, N'ילד שלישי',  @KindChild,  12,    0);


/* =====================================================================
   SECTION 2 — WHAT THE PAGE DERIVES FROM THE HOUSEHOLD
   ===================================================================== */

DECLARE @FamilySize int = (SELECT COUNT(*) FROM @Members);
DECLARE @AdultCount int = (SELECT COUNT(*) FROM @Members WHERE Age > @AdultAge);

-- The FIRST adult child in row order — this is the page's rule.
DECLARE @HalvedSeq  int;
SELECT TOP 1 @HalvedSeq = Seq
FROM   @Members
WHERE  Kind = @KindChild AND Age > @AdultAge
ORDER  BY Seq;

DECLARE @HalvedName nvarchar(200) =
    (SELECT FullName FROM @Members WHERE Seq = @HalvedSeq);

-- Adults only; the halved child at half.
DECLARE @CountedIncome decimal(18,4) =
    ISNULL((SELECT SUM(CASE WHEN Seq = @HalvedSeq THEN Income / 2.0 ELSE Income END)
            FROM @Members
            WHERE Age > @AdultAge), 0);

-- Divisor is the whole household, minors included.
DECLARE @AverageIncome decimal(18,4) =
    CASE WHEN @FamilySize = 0 THEN 0
         ELSE @CountedIncome / @FamilySize END;

SELECT N'--- שלב 1: המשפחה ---' AS [שלב];

SELECT  m.Seq                                      AS [#],
        m.FullName                                 AS [שם],
        m.Kind                                     AS [קשר],
        m.Age                                      AS [גיל],
        CASE WHEN m.Age > @AdultAge THEN N'בגיר' ELSE N'קטין' END AS [בגירות],
        m.Income                                   AS [הכנסה],
        CASE WHEN m.Age <= @AdultAge      THEN 0
             WHEN m.Seq  = @HalvedSeq     THEN m.Income / 2.0
             ELSE m.Income END                     AS [הכנסה נספרת],
        CASE WHEN m.Seq = @HalvedSeq THEN N'<<< מחצית' ELSE N'' END AS [הערה]
FROM    @Members m
ORDER BY m.Seq;


/* =====================================================================
   SECTION 3 — THE INCOME BRACKET  (EligibilityService.GetDiscountRateAsync)
   ===================================================================== */

-- Guard the page applies first: IsYearInCriteriaAsync.
DECLARE @YearHasCriteria bit =
    CASE WHEN EXISTS (SELECT 1 FROM tbIncomeDiscountCriteria WHERE Year_ = @Year)
         THEN 1 ELSE 0 END;

-- The criteria version in force on the request date.
DECLARE @CriteriaVersion date =
    (SELECT MAX(v.ValidityDate)
     FROM   tbIncomeDiscountCriteria v
     WHERE  v.Year_ = @Year AND v.ValidityDate <= @RequestDate);

DECLARE @CriteriaId   int;
DECLARE @IncomeRate   decimal(9,4) = 0;
DECLARE @FromAmount   decimal(12,2);
DECLARE @ToAmount     decimal(12,2);

SELECT TOP 1
        @CriteriaId = c.ID,
        @IncomeRate = c.DiscountRate,
        @FromAmount = c.FromAmount,
        @ToAmount   = c.ToAmount
FROM    tbIncomeDiscountCriteria c
WHERE   c.NumberOfPeople = @FamilySize
  AND   c.FromAmount    <= @AverageIncome
  AND   c.ToAmount      >= @AverageIncome
  AND   c.Year_          = @Year
  AND   c.ValidityDate   = @CriteriaVersion
ORDER BY c.DiscountRate DESC;

SET @IncomeRate = ISNULL(@IncomeRate, 0);

SELECT N'--- שלב 2: מדרגת ההכנסה ---' AS [שלב];

SELECT  @RequestDate                AS [תאריך חישוב],
        @Year                       AS [שנה],
        CASE WHEN @YearHasCriteria = 1 THEN N'כן' ELSE N'לא - המסך יציג שגיאה' END
                                    AS [יש קריטריונים לשנה],
        @CriteriaVersion            AS [גרסת קריטריונים בתוקף],
        @CriteriaId                 AS [ID שורת קריטריון],
        @FromAmount                 AS [מדרגה מ],
        @ToAmount                   AS [מדרגה עד],
        @IncomeRate                 AS [אחוז לפי הכנסה];


/* =====================================================================
   SECTION 4 — THE CAP  (EligibilityService.GetMaximumDiscountAsync)
   ===================================================================== */

DECLARE @MunicipalCode   int;
DECLARE @DiscountName    nvarchar(200);
DECLARE @RateFrom        date;
DECLARE @RateUntil       date;
DECLARE @MaximumDiscount decimal(9,4) = 0;

IF @IncomeRate > 0
BEGIN
    SELECT TOP 1
            @MunicipalCode   = v.MunicipalDiscountCode,
            @DiscountName    = v.DiscountName,
            @RateFrom        = v.RateFrom,
            @RateUntil       = v.RateUntil,
            @MaximumDiscount = v.MaximumDiscount
    FROM    vwDiscountRateAsOf v
    WHERE   v.Categories      = @Category
      AND   v.DiscountPercent = @IncomeRate
      AND   @RequestDate     >= v.RateFrom
      AND  (v.RateUntil IS NULL OR @RequestDate < v.RateUntil)
    ORDER BY v.RateFrom DESC;
END

SET @MaximumDiscount = ISNULL(@MaximumDiscount, 0);

SELECT N'--- שלב 3: תקרת ראש העיר ---' AS [שלב];

SELECT  @MunicipalCode      AS [קוד הנחה],
        @DiscountName       AS [שם ההנחה],
        @RateFrom           AS [גרסת תעריף מ],
        @RateUntil          AS [גרסת תעריף עד],
        @MaximumDiscount    AS [תקרה],
        CASE WHEN @IncomeRate > 0 AND @MunicipalCode IS NULL
             THEN N'לא נמצא קוד לאחוז זה - לא תוחל תקרה'
             ELSE N'' END   AS [אזהרה];


/* =====================================================================
   SECTION 5 — THE RESULT THE PAGE SHOULD BE SHOWING
   Compare these to the card on screen.
   ===================================================================== */

DECLARE @FinalRate decimal(9,4) = @IncomeRate;
IF @MaximumDiscount > 0 AND @IncomeRate > @MaximumDiscount
    SET @FinalRate = @MaximumDiscount;

SELECT N'=== תוצאה ===' AS [שלב];

SELECT  @FamilySize     AS [סה"כ נפשות],
        @AdultCount     AS [בגירים],
        @CountedIncome  AS [הכנסה נספרת],
        @AverageIncome  AS [הכנסה ממוצעת],
        ISNULL(@HalvedName, N'אין ילד בגיר') AS [חצי הכנסה],
        @IncomeRate     AS [אחוז לפני תקרה],
        @FinalRate      AS [אחוז סופי],
        CASE WHEN @FinalRate > 0
             THEN CAST(CAST(@FinalRate * 100 AS decimal(9,2)) AS nvarchar(20)) + N'%'
             ELSE N'לא זכאי להנחה' END        AS [תצוגה במסך],
        CASE WHEN @FinalRate < @IncomeRate
             THEN N'שיעור ההנחה הופחת ל-' +
                  CAST(CAST(@FinalRate * 100 AS decimal(9,2)) AS nvarchar(20)) +
                  N'% בהתאם למגבלת ראש העיר.'
             ELSE N'' END                     AS [הודעת אזהרה];


/* =====================================================================
   SECTION 6 — DIAGNOSTICS. Run these when the page's number looks wrong.
   ===================================================================== */

SELECT N'--- כל גרסאות הקריטריונים לשנה ---' AS [אבחון];

SELECT  c.ValidityDate                       AS [תוקף מ],
        COUNT(*)                             AS [שורות],
        MIN(c.NumberOfPeople)                AS [נפשות מ],
        MAX(c.NumberOfPeople)                AS [נפשות עד],
        CASE WHEN c.ValidityDate = @CriteriaVersion THEN N'<<< בתוקף' ELSE N'' END AS [הערה]
FROM    tbIncomeDiscountCriteria c
WHERE   c.Year_ = @Year
GROUP BY c.ValidityDate
ORDER BY c.ValidityDate;

SELECT N'--- כל המדרגות לגודל המשפחה הזה, בגרסה שבתוקף ---' AS [אבחון];

SELECT  c.ID                                 AS [ID],
        c.NumberOfPeople                     AS [נפשות],
        c.FromAmount                         AS [מ],
        c.ToAmount                           AS [עד],
        c.DiscountRate                       AS [אחוז],
        CASE WHEN @AverageIncome BETWEEN c.FromAmount AND c.ToAmount
             THEN N'<<< מתאים' ELSE N'' END  AS [הערה]
FROM    tbIncomeDiscountCriteria c
WHERE   c.Year_          = @Year
  AND   c.ValidityDate   = @CriteriaVersion
  AND   c.NumberOfPeople = @FamilySize
ORDER BY c.FromAmount;

SELECT N'--- קודי מבחן הכנסה והתקרות שלהם בתאריך החישוב ---' AS [אבחון];

SELECT  v.MunicipalDiscountCode              AS [קוד],
        v.DiscountName                       AS [שם],
        v.DiscountPercent                    AS [אחוז],
        v.MaximumDiscount                    AS [תקרה],
        v.MaximumArea                        AS [שטח מרבי],
        v.RateFrom                           AS [מ],
        v.RateUntil                          AS [עד],
        CASE WHEN v.DiscountPercent = @IncomeRate THEN N'<<< נבחר' ELSE N'' END AS [הערה]
FROM    vwDiscountRateAsOf v
WHERE   v.Categories  = @Category
  AND   @RequestDate >= v.RateFrom
  AND  (v.RateUntil IS NULL OR @RequestDate < v.RateUntil)
ORDER BY v.DiscountPercent;

SELECT N'--- הבדל מול המסך בשולחן העבודה ---' AS [אבחון];

-- The desktop halves the HIGHEST-earning adult child and counts minors'
-- income too. When the two screens disagree on the same household, this is
-- usually why. Both numbers are shown so the gap is visible.
DECLARE @DesktopHalvedSeq int;
SELECT TOP 1 @DesktopHalvedSeq = Seq
FROM   @Members
WHERE  Kind = @KindChild AND Age >= @AdultAge
ORDER  BY Income DESC, Seq;

DECLARE @DesktopCounted decimal(18,4) =
    ISNULL((SELECT SUM(CASE WHEN Seq = @DesktopHalvedSeq THEN Income / 2.0 ELSE Income END)
            FROM @Members), 0);

SELECT  @CountedIncome                                   AS [הכנסה נספרת - דפדפן],
        @DesktopCounted                                  AS [הכנסה נספרת - שולחן עבודה],
        (SELECT FullName FROM @Members WHERE Seq = @HalvedSeq)        AS [חצי - דפדפן],
        (SELECT FullName FROM @Members WHERE Seq = @DesktopHalvedSeq) AS [חצי - שולחן עבודה],
        CASE WHEN @CountedIncome = @DesktopCounted
             THEN N'זהה'
             ELSE N'שונה - המסכים יציגו אחוזים שונים' END AS [השוואה];
