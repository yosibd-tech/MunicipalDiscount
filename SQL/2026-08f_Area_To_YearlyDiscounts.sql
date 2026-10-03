/* ============================================================================
   2026-08f — Area (אזור) moves from tbClients to tbYearlyDiscounts

   2026-08e put Area on tbClients, because that is where it was asked for. It
   belongs next to AssetNumber instead: the area is a property of the PROPERTY,
   not of the citizen. A citizen holding two properties can hold them in two
   different areas, and only the year row knows which asset a request is about.

   Nothing is lost: Area was NULL in every row — it is filled by the next import
   of the previous-year file, and the citizen tables hold test data only.

   Run once, against MunicipalDiscount. Safe to re-run.
   ============================================================================ */

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('dbo.tbYearlyDiscounts', 'U') IS NULL
BEGIN
    RAISERROR('tbYearlyDiscounts not found — wrong database?', 16, 1);
    RETURN;
END
GO

/* 1. The new home. */
IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('dbo.tbYearlyDiscounts')
                 AND name = 'Area')
BEGIN
    ALTER TABLE dbo.tbYearlyDiscounts ADD Area int NULL;   -- אזור
    PRINT 'tbYearlyDiscounts.Area added.';
END
ELSE
    PRINT 'tbYearlyDiscounts.Area already exists — skipped.';
GO

/* 2. The backfill screen lists and sorts a whole area at a time. */
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'IX_tbYearlyDiscounts_Area'
                 AND object_id = OBJECT_ID('dbo.tbYearlyDiscounts'))
    CREATE NONCLUSTERED INDEX IX_tbYearlyDiscounts_Area
        ON dbo.tbYearlyDiscounts (Area) INCLUDE (ClientId, Year_, AssetNumber);
GO

/* 3. The old home goes away, index first. */
IF EXISTS (SELECT 1 FROM sys.indexes
           WHERE name = 'IX_tbClients_Area'
             AND object_id = OBJECT_ID('dbo.tbClients'))
BEGIN
    DROP INDEX IX_tbClients_Area ON dbo.tbClients;
    PRINT 'IX_tbClients_Area dropped.';
END
GO

IF EXISTS (SELECT 1 FROM sys.columns
           WHERE object_id = OBJECT_ID('dbo.tbClients')
             AND name = 'Area')
BEGIN
    ALTER TABLE dbo.tbClients DROP COLUMN Area;
    PRINT 'tbClients.Area dropped.';
END
ELSE
    PRINT 'tbClients.Area is already gone — skipped.';
GO

PRINT '';
PRINT 'Area now lives on the year row, beside AssetNumber.';
PRINT 'It stays NULL until the previous-year file is imported again.';
GO
