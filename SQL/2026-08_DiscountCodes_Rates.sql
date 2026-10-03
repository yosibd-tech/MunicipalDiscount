/* ============================================================================
   2026-08 — קודי הנחה: מעבר משורה לכל קוד לכל שנה
             לטבלת הגדרות + טבלת בת של גרסאות תעריף לפי תאריך

   Why:
     Until now a discount code was stored once per year (Year_), so a new year
     meant cloning all 82 rows through the "copy year" feature. That model
     cannot express a rate that changes DURING the year, which is what the
     municipalities started doing.

     From here on the definition of a code lives once, and every rate change is
     one row in tbDiscountCodeRates carrying the date it takes effect. The rate that
     applies to a citizen is the version with the greatest ValidityDate that is
     not later than the citizen's RequestDate.

   What this script does:
     1. Adds RequestDate to tbYearlyDiscounts.
     2. Renames the current tbDiscountsCodes to tbDiscountsCodes_OLD (nothing is
        deleted — same convention as tbMainParameters_OLD).
     3. Creates the new tbDiscountsCodes (definition, no Year_).
     4. Creates tbDiscountCodeRates (the dated versions).

   It deliberately does NOT migrate the old rows: the codes table is being
   rebuilt from Excel and loaded with PythonScripts/import_discount_codes.py.

   Run once, against MunicipalDiscount. Safe to re-run — every step is guarded.
   ============================================================================ */

SET NOCOUNT ON;
-- Required for the filtered indexes in section 5. sqlcmd defaults this OFF,
-- and CREATE INDEX with a WHERE clause fails without it.
SET QUOTED_IDENTIFIER ON;
GO

/* ---------------------------------------------------------------------------
   1. RequestDate — the date the citizen asked for the discount.
      This is what selects the rate version. Filled either by the eligibility
      page, or by the new web form for requests already imported this year.

      SUPERSEDED by 2026-08c_YearlyDiscounts_Split.sql: once the table is split,
      RequestDate lives on tbDiscountRequests, not here. The ClientId test is how
      this step recognises the new shape and stays out of its way.
   --------------------------------------------------------------------------- */
IF COL_LENGTH('dbo.tbYearlyDiscounts', 'ClientId') IS NOT NULL
    PRINT 'tbYearlyDiscounts is already split — RequestDate belongs to tbDiscountRequests, skipped.';
ELSE IF COL_LENGTH('dbo.tbYearlyDiscounts', 'RequestDate') IS NULL
BEGIN
    ALTER TABLE dbo.tbYearlyDiscounts ADD RequestDate date NULL;
    PRINT 'tbYearlyDiscounts.RequestDate added.';
END
ELSE
    PRINT 'tbYearlyDiscounts.RequestDate already exists — skipped.';
GO

/* ---------------------------------------------------------------------------
   2. Keep the current table under _OLD.
      Reference data only (164 rows), and the new one is loaded from Excel, so
      there is no migration step — but the old rows stay available for checking
      the rebuilt data against what was there before.
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.tbDiscountsCodes', 'U') IS NOT NULL
   AND OBJECT_ID('dbo.tbDiscountCodeRates', 'U') IS NULL
BEGIN
    IF OBJECT_ID('dbo.tbDiscountsCodes_OLD', 'U') IS NOT NULL
    BEGIN
        RAISERROR('tbDiscountsCodes_OLD already exists — rename it or drop it before running this script.', 16, 1);
        RETURN;
    END

    EXEC sp_rename 'dbo.tbDiscountsCodes', 'tbDiscountsCodes_OLD';
    PRINT 'tbDiscountsCodes renamed to tbDiscountsCodes_OLD.';
END
GO

/* ---------------------------------------------------------------------------
   3. The definition of a discount code — one row per code, no year.

      Both code columns are nullable on purpose: 23 of the existing rows are
      statutory discounts that carry only a LegallDiscountCode, and municipal
      discounts carry a MunicipalDiscountCode. At least one must be present —
      that is what CK_tbDiscountsCodes_HasCode enforces.

      Widened from smallint to int so the codes match tbYearlyDiscounts
      .MunicipalDiscountCode, which is already int.
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.tbDiscountsCodes', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbDiscountsCodes
    (
        ID                      int IDENTITY(1,1)  NOT NULL,
        MunicipalDiscountCode   int                NULL,        -- קוד עירוני
        LegallDiscountCode      int                NULL,        -- קוד חוקי
        Categories              nvarchar(255)      NULL,        -- קטגוריה
        GroupCode               int                NULL,        -- קוד קבוצה
        DiscountName            nvarchar(255)      NULL,        -- שם הנחה
        Mandatory               nvarchar(255)      NULL,        -- חובה
        LawInForce              nvarchar(255)      NULL,        -- חוק בתוקף
        Documents               nvarchar(255)      NULL,        -- מסמכים נדרשים
        Remark                  nvarchar(255)      NULL,        -- הערה
        Rules                   nvarchar(255)      NULL,        -- כללים
        Conditions              nvarchar(255)      NULL,        -- תנאים

        CONSTRAINT PK_tbDiscountsCodes PRIMARY KEY CLUSTERED (ID),

        -- A code with neither number cannot be matched to a citizen row.
        CONSTRAINT CK_tbDiscountsCodes_HasCode CHECK
            (MunicipalDiscountCode IS NOT NULL OR LegallDiscountCode IS NOT NULL),

        -- One definition per code pair. NULLs compare as equal here, which is
        -- what we want: it also blocks a second (NULL, NULL) row.
        CONSTRAINT UQ_tbDiscountsCodes_Codes UNIQUE (MunicipalDiscountCode, LegallDiscountCode)
    );

    PRINT 'tbDiscountsCodes created.';
END
ELSE
    PRINT 'tbDiscountsCodes already exists — skipped.';
GO

/* ---------------------------------------------------------------------------
   4. The rate versions. One row per change the municipality makes.

      A code that never changes has exactly one row. A code that went from 40%
      to 60% in March 2026 has two:
          ValidityDate 2026-01-01  DiscountPercent 0.40
          ValidityDate 2026-03-15  DiscountPercent 0.60

      Percentages are fractions (0.40 = 40%), as they were before. Stored as
      decimal rather than float — the old float column held 0.7 as
      0.69999999999999996, which shows up in reports.
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.tbDiscountCodeRates', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbDiscountCodeRates
    (
        ID                      int IDENTITY(1,1)  NOT NULL,
        DiscountCodeId          int                NOT NULL,    -- FK -> tbDiscountsCodes.ID
        ValidityDate            date               NOT NULL,    -- תאריך תחילת תוקף
        DiscountPercent         decimal(9,4)       NULL,        -- אחוז הנחה
        PreviousDiscountPercent decimal(9,4)       NULL,        -- אחוז הנחה קודם
        MaximumDiscount         decimal(9,4)       NULL,        -- מקסימום הנחה
        MaximumArea             decimal(10,2)      NULL,        -- מקסימום שטח

        CONSTRAINT PK_tbDiscountCodeRates PRIMARY KEY CLUSTERED (ID),

        CONSTRAINT FK_tbDiscountCodeRates_Code FOREIGN KEY (DiscountCodeId)
            REFERENCES dbo.tbDiscountsCodes (ID) ON DELETE CASCADE,

        -- A code cannot have two different rates starting the same day.
        CONSTRAINT UQ_tbDiscountCodeRates_CodeDate UNIQUE (DiscountCodeId, ValidityDate)
    );

    PRINT 'tbDiscountCodeRates created.';
END
ELSE
    PRINT 'tbDiscountCodeRates already exists — skipped.';
GO

/* ---------------------------------------------------------------------------
   5. Indexes for the two lookup paths.
      Citizens carry a code number, so every lookup starts by finding the
      definition by number, then walks back through its rate versions.
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'IX_tbDiscountsCodes_Municipal'
                 AND object_id = OBJECT_ID('dbo.tbDiscountsCodes'))
    CREATE NONCLUSTERED INDEX IX_tbDiscountsCodes_Municipal
        ON dbo.tbDiscountsCodes (MunicipalDiscountCode) WHERE MunicipalDiscountCode IS NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'IX_tbDiscountsCodes_Legal'
                 AND object_id = OBJECT_ID('dbo.tbDiscountsCodes'))
    CREATE NONCLUSTERED INDEX IX_tbDiscountsCodes_Legal
        ON dbo.tbDiscountsCodes (LegallDiscountCode) WHERE LegallDiscountCode IS NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'IX_tbDiscountCodeRates_CodeDate'
                 AND object_id = OBJECT_ID('dbo.tbDiscountCodeRates'))
    CREATE NONCLUSTERED INDEX IX_tbDiscountCodeRates_CodeDate
        ON dbo.tbDiscountCodeRates (DiscountCodeId, ValidityDate DESC)
        INCLUDE (DiscountPercent, MaximumDiscount, MaximumArea);
GO

/* ---------------------------------------------------------------------------
   6. The lookup, as a view — the rate in force for a code on a given date.

      Every caller (the Python import, the two reports, both eligibility
      screens) uses the same rule through this view, so it lives in one place
      instead of being retyped in six queries. RateFrom/RateUntil are the window
      each version was in force, which makes the lookup a plain range test:

          SELECT DiscountPercent, MaximumDiscount
          FROM   dbo.vwDiscountRateAsOf
          WHERE  MunicipalDiscountCode = @code
            AND  @requestDate >= RateFrom
            AND (@requestDate <  RateUntil OR RateUntil IS NULL)

      Exactly one row matches any date on or after the code's first version.
      A date EARLIER than the first version matches nothing — callers decide
      whether that is an error or falls back to the earliest version.
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.vwDiscountRateAsOf', 'V') IS NOT NULL
    DROP VIEW dbo.vwDiscountRateAsOf;
GO

CREATE VIEW dbo.vwDiscountRateAsOf
AS
    SELECT
        c.ID                        AS DiscountCodeId,
        c.MunicipalDiscountCode,
        c.LegallDiscountCode,
        c.Categories,
        c.DiscountName,
        c.GroupCode,
        c.Mandatory,
        c.LawInForce,
        r.ID                        AS RateId,
        r.ValidityDate              AS RateFrom,
        LEAD(r.ValidityDate) OVER (PARTITION BY c.ID ORDER BY r.ValidityDate) AS RateUntil,
        r.DiscountPercent,
        r.PreviousDiscountPercent,
        r.MaximumDiscount,
        r.MaximumArea
    FROM dbo.tbDiscountsCodes c
    INNER JOIN dbo.tbDiscountCodeRates r ON r.DiscountCodeId = c.ID;
GO

PRINT 'Done. Next: rebuild the codes in Excel and load them with PythonScripts/import_discount_codes.py';
GO

/* ============================================================================
   Undo (only while the new tables are still empty):

       DROP VIEW  dbo.vwDiscountRateAsOf;
       DROP TABLE dbo.tbDiscountCodeRates;
       DROP TABLE dbo.tbDiscountsCodes;
       EXEC sp_rename 'dbo.tbDiscountsCodes_OLD', 'tbDiscountsCodes';
       ALTER TABLE dbo.tbYearlyDiscounts DROP COLUMN RequestDate;
   ============================================================================ */
