/* =====================================================================
   tbIncomeDiscountCriteria.MunicipalDiscountCode
   ---------------------------------------------------------------------
   Until now the eligibility check worked the code out backwards: it had a
   percentage (0.70) and searched tbDiscountCodeRates for whichever
   מבחן הכנסה code carried that DiscountPercent, arriving at 770.

   That inference is fragile. Codes exist only for 20%-90%, so a bracket of
   100% would find nothing and the decision would be saved with no code at
   all; and if two codes ever carried the same percent the match would be
   ambiguous with nothing to break the tie.

   The bracket now names its own code. MaximumDiscount and MaximumArea are
   still read from tbDiscountCodeRates — found BY the code instead of by the
   percentage.

   STEP 1 adds the column. STEP 2 fills the existing rows from the rate that
   matches their percent — the same inference, done once, under review,
   instead of on every calculation. STEP 3 reports what is left.
   ===================================================================== */

SET NOCOUNT ON;
GO

/* ---------- STEP 1: the column ---------- */

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'tbIncomeDiscountCriteria'
                 AND COLUMN_NAME = 'MunicipalDiscountCode')
BEGIN
    ALTER TABLE dbo.tbIncomeDiscountCriteria ADD MunicipalDiscountCode int NULL;
    PRINT 'Added tbIncomeDiscountCriteria.MunicipalDiscountCode';
END
ELSE
    PRINT 'Column already exists — skipped';
GO

/* ---------- STEP 2: fill from the matching rate ----------
   Only rows that have no code yet, so a value set by hand in
   IncomeDiscountCriteriaForm is never overwritten.

   The rate version is the one in force on the criteria row's own
   ValidityDate: a bracket that came into force in July belongs with the
   code as it stood in July. */

DECLARE @Category nvarchar(100) =
    NCHAR(1502)+NCHAR(1489)+NCHAR(1495)+NCHAR(1503)+NCHAR(32)+
    NCHAR(1492)+NCHAR(1499)+NCHAR(1504)+NCHAR(1505)+NCHAR(1492);   -- מבחן הכנסה

UPDATE c
SET    c.MunicipalDiscountCode = v.MunicipalDiscountCode
FROM   dbo.tbIncomeDiscountCriteria c
CROSS  APPLY (
    SELECT TOP 1 r.MunicipalDiscountCode
    FROM   dbo.vwDiscountRateAsOf r
    WHERE  r.Categories      = @Category
      AND  r.DiscountPercent = c.DiscountRate
      AND  c.ValidityDate   >= r.RateFrom
      AND (r.RateUntil IS NULL OR c.ValidityDate < r.RateUntil)
    ORDER BY r.RateFrom DESC
) v
WHERE  c.MunicipalDiscountCode IS NULL;

PRINT CONCAT('Filled ', @@ROWCOUNT, ' criteria rows with a discount code');
GO

/* ---------- STEP 3: what the table looks like now ---------- */

SELECT  c.Year_                        AS [שנה],
        c.ValidityDate                 AS [תוקף מ],
        c.DiscountRate                 AS [אחוז],
        c.MunicipalDiscountCode        AS [קוד הנחה],
        d.DiscountName                 AS [שם ההנחה],
        COUNT(*)                       AS [מדרגות]
FROM        dbo.tbIncomeDiscountCriteria c
LEFT   JOIN dbo.tbDiscountsCodes d ON d.MunicipalDiscountCode = c.MunicipalDiscountCode
GROUP BY c.Year_, c.ValidityDate, c.DiscountRate, c.MunicipalDiscountCode, d.DiscountName
ORDER BY c.Year_, c.ValidityDate, c.DiscountRate;
GO

/* Rows the fill could not reach — a percent no code carries. These must be
   given a code by hand in מסך קריטריונים להנחה, or the check will save a
   decision with no code on it. */

SELECT  c.ID        AS [שורה],
        c.Year_     AS [שנה],
        c.ValidityDate AS [תוקף מ],
        c.NumberOfPeople AS [נפשות],
        c.FromAmount AS [מ],
        c.ToAmount   AS [עד],
        c.DiscountRate AS [אחוז]
FROM    dbo.tbIncomeDiscountCriteria c
WHERE   c.MunicipalDiscountCode IS NULL
ORDER BY c.Year_, c.ValidityDate, c.DiscountRate;
GO
