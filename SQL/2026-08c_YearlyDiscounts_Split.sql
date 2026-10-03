/* ============================================================================
   2026-08 — הנחות שנתיות: פיצול טבלה אחת לשלוש
             לקוחות / שורת שנה / בקשות הנחה

   Why:
     tbYearlyDiscounts held two years in one row — PreviesYear with
     PreviousDiscountPercent/PreviousYearDiscount, and CurrentYear with
     ActualCurrentYearDiscount plus the billing figures. Those are the same
     three facts written twice under different names, by two importers:

         import_discounts.py                    -> the Previous... columns
         MainForm.ImportCurrentYearDiscountFile -> the Current... columns

     Same problem the codes table had: a fact that repeats per year is a row,
     not a column. Consequences of the old shape:

       * a one-year horizon — two years back is unreachable;
       * opening a new year meant copying this year's figures forward, and the
         only mechanism for that is import_discounts.py's TRUNCATE (line 119),
         which would erase every eligibility decision the workers had made;
       * no key to upsert against, so MainForm's not-found INSERT loop produced
         263 duplicate rows (197 from the billing screen, 66 from Python);
       * the citizen's row mixed imported data with worker decisions.

     From here on a row belongs to exactly one year, and every discount the
     citizen was granted — imported or decided by a worker — is one row in
     tbDiscountRequests carrying its own RequestDate. "Previous year" stops
     being a schema concept and becomes Year_ = @year - 1.

   What this script does:
     1. Renames tbYearlyDiscounts to tbYearlyDiscounts_OLD (nothing deleted).
     2. Creates tbClients, the new tbYearlyDiscounts, and tbDiscountRequests.
     3. Prints what the old table held, for review before it is dropped.

   It deliberately does NOT migrate the 5280 old rows: that data is being
   re-imported through the new process, so it has no value beyond review —
   same decision as the codes table, which is rebuilt from Excel. Section 6
   has the review queries and the DROP to run once you are satisfied.

   Run once, against MunicipalDiscount, AFTER 2026-08_DiscountCodes_Rates.sql
   and 2026-08b_Rename_tbDiscountCodeRates.sql. Safe to re-run — every step is
   guarded.

   Code that must change before the application runs again — the old table is
   gone, so every one of these breaks:
     Forms/MainForm.cs:491, 525, 625, 899, 1209
     Forms/DiscountEligibilityChecks.cs:118, 347, 435
     Forms/AddNewClient .cs:90, 106
     Classes/CurrentYearDiscountAuditReport.cs:110, 230
     Classes/DiscountChecksReport.cs:64
     Classes/PreviousYearDiscountsReport.cs:48
     PythonScripts/import_discounts.py:119-152
     MunicipalDiscountWeb/Services/EligibilityService.cs, Models/ClientInfo.cs
   ============================================================================ */

SET NOCOUNT ON;
-- Required for the filtered index in section 5. sqlcmd defaults this OFF,
-- and CREATE INDEX with a WHERE clause fails without it.
SET QUOTED_IDENTIFIER ON;
GO

/* ---------------------------------------------------------------------------
   0. Precondition.
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.tbYearlyDiscounts', 'U') IS NULL
   AND OBJECT_ID('dbo.tbYearlyDiscounts_OLD', 'U') IS NULL
BEGIN
    RAISERROR('tbYearlyDiscounts not found — wrong database?', 16, 1);
    RETURN;
END
GO

/* ---------------------------------------------------------------------------
   1. Keep the current table under _OLD, same convention as
      tbDiscountsCodes_OLD and tbMainParameters_OLD.

      Its 5280 rows are not migrated — they are kept only so the re-imported
      data can be checked against what was there before.
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.tbYearlyDiscounts', 'U') IS NOT NULL
   AND OBJECT_ID('dbo.tbDiscountRequests', 'U') IS NULL
BEGIN
    IF OBJECT_ID('dbo.tbYearlyDiscounts_OLD', 'U') IS NOT NULL
    BEGIN
        RAISERROR('tbYearlyDiscounts_OLD already exists — rename it or drop it before running this script.', 16, 1);
        RETURN;
    END

    EXEC sp_rename 'dbo.tbYearlyDiscounts', 'tbYearlyDiscounts_OLD';
    PRINT 'tbYearlyDiscounts renamed to tbYearlyDiscounts_OLD.';
END
GO

/* ---------------------------------------------------------------------------
   2. The citizen — one row per person, not per person per year.

      ClientName stays nullable because AddNewClient can create a citizen
      before the name is known.
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.tbClients', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbClients
    (
        ID                int IDENTITY(1,1) NOT NULL,
        ClientNumber      nvarchar(20)      NOT NULL,   -- מספר זהות
        ClientName        nvarchar(255)     NULL,       -- שם הלקוח
        ClieneWifeNumber  nvarchar(20)      NULL,       -- מספר זהות בן/בת הזוג
        PhoneNumber       nvarchar(50)      NULL,       -- טלפון

        CONSTRAINT PK_tbClients PRIMARY KEY CLUSTERED (ID),
        CONSTRAINT UQ_tbClients_ClientNumber UNIQUE (ClientNumber)
    );

    PRINT 'tbClients created.';
END
ELSE
    PRINT 'tbClients already exists — skipped.';
GO

/* ---------------------------------------------------------------------------
   3. The year row — one per citizen per property per year.

      Carries only what describes the WHOLE year and arrives from the billing
      import (barButImportBilling, MainForm.cs:625 and :1209). AssetNumber is
      part of the key: 22 citizens in the old data held two properties, and
      re-stating it each year means a change of property is visible in that
      year's record.

      Renamed from the old columns, which no longer make sense on a row that
      already knows its year — and CurrentYearBlling was a typo:
          CurrentYearBlling    -> AnnualBilling    חיוב שנתי
          CurrentYearAssetSize -> AssetSize        שטח הנכס
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.tbYearlyDiscounts', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbYearlyDiscounts
    (
        ID            int IDENTITY(1,1) NOT NULL,
        ClientId      int               NOT NULL,   -- FK -> tbClients.ID
        Year_         int               NOT NULL,   -- שנת המס
        AssetNumber   nvarchar(20)      NULL,       -- מספר נכס
        AnnualBilling decimal(18,2)     NULL,       -- חיוב שנתי
        AssetSize     decimal(10,2)     NULL,       -- שטח הנכס

        CONSTRAINT PK_tbYearlyDiscounts PRIMARY KEY CLUSTERED (ID),

        CONSTRAINT FK_tbYearlyDiscounts_Client FOREIGN KEY (ClientId)
            REFERENCES dbo.tbClients (ID),

        -- The key the imports upsert against — this is what makes the
        -- duplicate-row bug impossible to repeat.
        CONSTRAINT UQ_tbYearlyDiscounts_ClientYearAsset
            UNIQUE (ClientId, Year_, AssetNumber)
    );

    PRINT 'tbYearlyDiscounts created (new shape).';
END
ELSE
    PRINT 'tbYearlyDiscounts already exists — skipped.';
GO

/* ---------------------------------------------------------------------------
   4. The request — one row per discount the citizen was granted.

      Three money/rate columns that look similar and are not derivable from
      each other after the fact:

        ActualDiscountAmount     ₪ the municipality actually granted, straight
                                 from barButImportCurrentYearData
        EntitledDiscountPercent  % the eligibility check produced
        ComputedDiscount         ₪ that percent works out to, stored at the
                                 moment of the decision

      ComputedDiscount is the point of the whole exercise: once a code's rate
      can change mid-year, recomputing an old approval from today's codes would
      judge a February decision by June's rate and flag a legitimate discount as
      illegal. The audit report becomes a subtraction instead:

          IllegalDiscount = ActualDiscountAmount - ComputedDiscount

      RequestDate is nullable because imported grants arrive without one —
      filling them is the backfill web form's job. It is also what selects the
      rate version through vwDiscountRateAsOf.

      MunicipalDiscountCode sits here rather than on the year row: it arrives on
      the same imported line as the amount, and that is what lets a citizen's
      code change mid-year with no special handling. It stays a plain number
      (not an FK) because statutory discounts carry only a LegallDiscountCode.
   --------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.tbDiscountRequests', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbDiscountRequests
    (
        ID                      int IDENTITY(1,1) NOT NULL,
        YearlyDiscountId        int               NOT NULL,   -- FK -> tbYearlyDiscounts.ID
        RequestDate             date              NULL,       -- תאריך הבקשה
        MunicipalDiscountCode   int               NULL,       -- קוד הנחה עירוני
        ActualDiscountAmount    decimal(18,2)     NULL,       -- הנחה בפועל בש"ח
        EntitledDiscountPercent decimal(9,4)      NULL,       -- אחוז הזכאות שנקבע
        ComputedDiscount        decimal(18,2)     NULL,       -- הנחה מחושבת בש"ח
        DiscountRateId          int               NULL,       -- גרסת התעריף ששימשה לחישוב
        Status                  bit               NOT NULL
            CONSTRAINT DF_tbDiscountRequests_Status DEFAULT (0),
        UserId                  int               NULL,
        CreateDate              datetime2(7)      NOT NULL
            CONSTRAINT DF_tbDiscountRequests_CreateDate DEFAULT (SYSDATETIME()),

        CONSTRAINT PK_tbDiscountRequests PRIMARY KEY CLUSTERED (ID),

        CONSTRAINT FK_tbDiscountRequests_Yearly FOREIGN KEY (YearlyDiscountId)
            REFERENCES dbo.tbYearlyDiscounts (ID) ON DELETE CASCADE
    );

    PRINT 'tbDiscountRequests created.';
END
ELSE
    PRINT 'tbDiscountRequests already exists — skipped.';
GO

-- DiscountRateId records WHICH rate version produced ComputedDiscount. Not
-- strictly needed to reproduce the money, but without it you cannot tell, a
-- year later, which version was applied. Nullable: imported grants have none.
IF OBJECT_ID('dbo.tbDiscountCodeRates', 'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys
                   WHERE name = 'FK_tbDiscountRequests_Rate')
BEGIN
    ALTER TABLE dbo.tbDiscountRequests
        ADD CONSTRAINT FK_tbDiscountRequests_Rate FOREIGN KEY (DiscountRateId)
            REFERENCES dbo.tbDiscountCodeRates (ID);
    PRINT 'FK_tbDiscountRequests_Rate added.';
END
GO

/* ---------------------------------------------------------------------------
   5. Indexes for the paths the screens and reports actually take.
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'IX_tbYearlyDiscounts_Year'
                 AND object_id = OBJECT_ID('dbo.tbYearlyDiscounts'))
    CREATE NONCLUSTERED INDEX IX_tbYearlyDiscounts_Year
        ON dbo.tbYearlyDiscounts (Year_) INCLUDE (ClientId, AnnualBilling, AssetSize);
GO

-- The billing import matches on AssetNumber alone (MainForm.cs:628).
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'IX_tbYearlyDiscounts_Asset'
                 AND object_id = OBJECT_ID('dbo.tbYearlyDiscounts'))
    CREATE NONCLUSTERED INDEX IX_tbYearlyDiscounts_Asset
        ON dbo.tbYearlyDiscounts (AssetNumber, Year_);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'IX_tbDiscountRequests_Yearly'
                 AND object_id = OBJECT_ID('dbo.tbDiscountRequests'))
    CREATE NONCLUSTERED INDEX IX_tbDiscountRequests_Yearly
        ON dbo.tbDiscountRequests (YearlyDiscountId)
        INCLUDE (RequestDate, MunicipalDiscountCode, ActualDiscountAmount,
                 EntitledDiscountPercent, ComputedDiscount, Status);
GO

-- The backfill form's worklist: requests still missing a date.
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'IX_tbDiscountRequests_NoDate'
                 AND object_id = OBJECT_ID('dbo.tbDiscountRequests'))
    CREATE NONCLUSTERED INDEX IX_tbDiscountRequests_NoDate
        ON dbo.tbDiscountRequests (YearlyDiscountId)
        WHERE RequestDate IS NULL;
GO

PRINT '';
PRINT 'Schema ready. The new tables are EMPTY — fill them by re-running the';
PRINT 'imports once import_discounts.py and MainForm have been rewritten.';
GO

/* ===========================================================================
   6. Review, then drop.

      Run these against tbYearlyDiscounts_OLD to check the re-imported data
      against what was there before. Once they agree, drop the old table.
      Nothing in the application references it.
   =========================================================================== */

-- What the old table held, per year:
--     SELECT PreviesYear, CurrentYear, COUNT(*) AS rows_,
--            COUNT(DISTINCT ClientNumber) AS citizens,
--            SUM(PreviousYearDiscount)      AS prev_granted,
--            SUM(ActualCurrentYearDiscount) AS curr_granted,
--            SUM(CurrentYearBlling)         AS billing
--     FROM dbo.tbYearlyDiscounts_OLD
--     GROUP BY PreviesYear, CurrentYear;
--
-- The same figures out of the new tables, for comparison:
--     SELECT y.Year_, COUNT(DISTINCT y.ID) AS year_rows,
--            COUNT(DISTINCT y.ClientId)    AS citizens,
--            COUNT(r.ID)                   AS requests,
--            SUM(r.ActualDiscountAmount)   AS granted,
--            SUM(y.AnnualBilling)          AS billing
--     FROM dbo.tbYearlyDiscounts y
--     LEFT JOIN dbo.tbDiscountRequests r ON r.YearlyDiscountId = y.ID
--     GROUP BY y.Year_;
--
-- A citizen who was in the old data and did not come back:
--     SELECT DISTINCT o.ClientNumber, o.ClientName
--     FROM   dbo.tbYearlyDiscounts_OLD o
--     WHERE  NOT EXISTS (SELECT 1 FROM dbo.tbClients c
--                        WHERE c.ClientNumber = LTRIM(RTRIM(o.ClientNumber)));
--
-- When satisfied:
--     DROP TABLE dbo.tbYearlyDiscounts_OLD;

/* ============================================================================
   Undo (while the new tables are still empty):

       DROP TABLE dbo.tbDiscountRequests;
       DROP TABLE dbo.tbYearlyDiscounts;
       DROP TABLE dbo.tbClients;
       EXEC sp_rename 'dbo.tbYearlyDiscounts_OLD', 'tbYearlyDiscounts';
   ============================================================================ */
