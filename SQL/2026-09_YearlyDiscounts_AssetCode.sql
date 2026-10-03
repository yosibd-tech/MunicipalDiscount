/* =====================================================================
   tbYearlyDiscounts.AssetCode — סוג הנכס, לצורך סינון "מגורים"
   ---------------------------------------------------------------------
   The eligibility page applies to מגורים assets only, and nothing on
   tbYearlyDiscounts said what kind of property a row was. The kind lives
   in tbGAssetKinds (Code -> Name / Category / MainCategory), and until
   now the only way to reach it was through tbGOutstandingBalances — an
   outstanding-balances snapshot that simply does not list a property with
   nothing owing on it. 1,636 of the 12,883 rows for 2026 (₪10M of
   billing) were unreachable that way.

   So the code is carried on the year row itself, filled by the billing
   import (barButImportBilling) from the file's own kind column.

   STEP 1 adds the column. STEP 2 backfills what can be backfilled today,
   so the filter works before the next import runs. STEP 3 reports.
   ===================================================================== */

SET NOCOUNT ON;
GO

/* ---------- STEP 1: the column ---------- */

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'tbYearlyDiscounts' AND COLUMN_NAME = 'AssetCode')
BEGIN
    ALTER TABLE dbo.tbYearlyDiscounts ADD AssetCode int NULL;
    PRINT 'Added tbYearlyDiscounts.AssetCode';
END
ELSE
    PRINT 'tbYearlyDiscounts.AssetCode already exists — skipped';
GO

/* Lookups for the eligibility filter and for the audit queries. */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_tbYearlyDiscounts_AssetCode')
BEGIN
    CREATE INDEX IX_tbYearlyDiscounts_AssetCode
        ON dbo.tbYearlyDiscounts (AssetCode) INCLUDE (ClientId, Year_, AnnualBilling, AssetSize);
    PRINT 'Created IX_tbYearlyDiscounts_AssetCode';
END
GO

/* ---------- STEP 2: backfill from the balances snapshot ----------
   Only where the code is still unknown, so a value the import has
   already written is never overwritten. AssetNumber is nvarchar(20)
   here and bigint there, hence TRY_CAST.
   Rows the snapshot does not list stay NULL — they are exactly the ones
   the import is meant to fill in. */

UPDATE y
SET    y.AssetCode = b.AssetType
FROM   dbo.tbYearlyDiscounts y
CROSS  APPLY (SELECT TOP 1 b2.AssetType
              FROM   dbo.tbGOutstandingBalances b2
              WHERE  b2.AssetNumber = TRY_CAST(y.AssetNumber AS bigint)) b
WHERE  y.AssetCode IS NULL
  AND  b.AssetType IS NOT NULL;

PRINT CONCAT('Backfilled ', @@ROWCOUNT, ' rows from tbGOutstandingBalances');
GO

/* ---------- STEP 3: what we ended up with ---------- */

SELECT  ISNULL(k.Category, N'(לא ידוע)')                       AS [קטגוריה],
        COUNT(*)                                               AS [שורות],
        SUM(CASE WHEN y.Year_ = 2026 THEN 1 ELSE 0 END)        AS [מתוכן 2026],
        SUM(CASE WHEN y.AnnualBilling > 0 THEN 1 ELSE 0 END)   AS [עם חיוב]
FROM        dbo.tbYearlyDiscounts y
LEFT   JOIN dbo.tbGAssetKinds k ON k.Code = y.AssetCode
GROUP BY k.Category
ORDER BY COUNT(*) DESC;
GO

/* ---------- The filter this exists for ----------

   SELECT ...
   FROM       tbYearlyDiscounts y
   INNER JOIN tbGAssetKinds     k ON k.Code = y.AssetCode
   WHERE      k.Category = N'מגורים'     -- codes 100 (מגורים) and 110 (מחסן)

   ------------------------------------------------------------------- */
