/* ============================================================================
   2026-08e — tbClients.Area  (אזור)

   The municipality's own export carries a column headed "אזור" — the area the
   citizen belongs to, numbered 1, 2, 3 … It was read past until now; the
   backfill screen needs it, because a worker filling in request dates works
   through the paper requests of one area at a time.

   int, not text: every value in the file is a number, so a numeric column
   sorts and filters correctly and the screen's area picker is a list of
   numbers rather than of strings that happen to look like numbers.

   Filled by PythonScripts/import_discounts.py (the file behind
   barButImportPreviouseYearData), which finds the column by its header text
   rather than by a position in tbDiscountHeaders — the mapping table has nine
   fixed slots and its form has nine dropdowns.

   Note on placement: the area describes where the citizen is, so it sits on
   tbClients as asked. If it ever turns out that one citizen holds properties
   in two different areas, it belongs on tbYearlyDiscounts instead — that is
   the row that knows the AssetNumber.

   Run once, against MunicipalDiscount. Safe to re-run: an Area column of the
   wrong type is dropped and rebuilt, which costs nothing while the citizen
   tables still hold test data only.
   ============================================================================ */

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('dbo.tbClients', 'U') IS NULL
BEGIN
    RAISERROR('tbClients not found — wrong database?', 16, 1);
    RETURN;
END
GO

/* An earlier run of this script created Area as nvarchar(100). Rebuild it. */
IF EXISTS (SELECT 1 FROM sys.columns c
           INNER JOIN sys.types t ON t.user_type_id = c.user_type_id
           WHERE c.object_id = OBJECT_ID('dbo.tbClients')
             AND c.name = 'Area'
             AND t.name <> 'int')
BEGIN
    IF EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'IX_tbClients_Area'
                 AND object_id = OBJECT_ID('dbo.tbClients'))
        DROP INDEX IX_tbClients_Area ON dbo.tbClients;

    ALTER TABLE dbo.tbClients DROP COLUMN Area;
    PRINT 'tbClients.Area dropped — it was not int.';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('dbo.tbClients')
                 AND name = 'Area')
BEGIN
    ALTER TABLE dbo.tbClients ADD Area int NULL;   -- אזור
    PRINT 'tbClients.Area added (int).';
END
ELSE
    PRINT 'tbClients.Area already exists as int — skipped.';
GO

-- The backfill screen lists a whole area at a time.
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'IX_tbClients_Area'
                 AND object_id = OBJECT_ID('dbo.tbClients'))
    CREATE NONCLUSTERED INDEX IX_tbClients_Area
        ON dbo.tbClients (Area) INCLUDE (ClientNumber, ClientName);
GO

PRINT '';
PRINT 'Existing rows keep Area NULL until the next import of the previous-year file.';
GO
