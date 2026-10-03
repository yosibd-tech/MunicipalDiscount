/* ============================================================================
   2026-08h — ValidityDate on tbIncomeDiscountCriteria and tbMinimumWage

   The same move the discount codes made: a bracket table or a minimum wage does
   not belong to a year, it takes effect on a date. משרד הפנים can publish new
   brackets in the middle of a year, and the old Year_ column cannot say when
   they start to apply.

   Year_ is kept for now — every screen and report still reads it. Once the
   callers select by date it can be retired, exactly as Year_ was retired from
   tbDiscountsCodes.

   Existing rows are backfilled to the first of January of their year, which is
   what Year_ meant.

   Run once, against MunicipalDiscount. Safe to re-run.
   ============================================================================ */

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('dbo.tbIncomeDiscountCriteria')
                 AND name = 'ValidityDate')
BEGIN
    ALTER TABLE dbo.tbIncomeDiscountCriteria ADD ValidityDate date NULL;   -- בתוקף מתאריך
    PRINT 'tbIncomeDiscountCriteria.ValidityDate added.';
END
ELSE
    PRINT 'tbIncomeDiscountCriteria.ValidityDate already exists — skipped.';
GO

UPDATE dbo.tbIncomeDiscountCriteria
SET    ValidityDate = DATEFROMPARTS(Year_, 1, 1)
WHERE  ValidityDate IS NULL;
PRINT CONCAT('tbIncomeDiscountCriteria backfilled: ', @@ROWCOUNT, ' row(s).');
GO

IF EXISTS (SELECT 1 FROM sys.columns
           WHERE object_id = OBJECT_ID('dbo.tbIncomeDiscountCriteria')
             AND name = 'ValidityDate' AND is_nullable = 1)
   AND NOT EXISTS (SELECT 1 FROM dbo.tbIncomeDiscountCriteria WHERE ValidityDate IS NULL)
BEGIN
    ALTER TABLE dbo.tbIncomeDiscountCriteria ALTER COLUMN ValidityDate date NOT NULL;
    PRINT 'tbIncomeDiscountCriteria.ValidityDate is now NOT NULL.';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('dbo.tbMinimumWage')
                 AND name = 'ValidityDate')
BEGIN
    ALTER TABLE dbo.tbMinimumWage ADD ValidityDate date NULL;   -- בתוקף מתאריך
    PRINT 'tbMinimumWage.ValidityDate added.';
END
ELSE
    PRINT 'tbMinimumWage.ValidityDate already exists — skipped.';
GO

UPDATE dbo.tbMinimumWage
SET    ValidityDate = DATEFROMPARTS(Year_, 1, 1)
WHERE  ValidityDate IS NULL;
PRINT CONCAT('tbMinimumWage backfilled: ', @@ROWCOUNT, ' row(s).');
GO

IF EXISTS (SELECT 1 FROM sys.columns
           WHERE object_id = OBJECT_ID('dbo.tbMinimumWage')
             AND name = 'ValidityDate' AND is_nullable = 1)
   AND NOT EXISTS (SELECT 1 FROM dbo.tbMinimumWage WHERE ValidityDate IS NULL)
BEGIN
    ALTER TABLE dbo.tbMinimumWage ALTER COLUMN ValidityDate date NOT NULL;
    PRINT 'tbMinimumWage.ValidityDate is now NOT NULL.';
END
GO
