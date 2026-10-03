/* ============================================================================
   2026-08g — tbDiscountRequests.MunicipalDiscountCodeName (תאור ההנחה)

   The granted-discounts file carries the discount's description beside its
   code (column F, headed "תאור"). Until now the import read the field and
   dropped it: the name was taken to belong to the code, in tbDiscountsCodes.

   It is worth keeping on the request itself. The row records what the
   municipality granted, in its own words, on the day it granted it — and a
   code's description can be edited later, or the code retired, without that
   changing what the paperwork said at the time.

   nvarchar(255), nullable: a request created by the eligibility check inherits
   it from the citizen's previous request, and may have none.

   Run once, against MunicipalDiscount. Safe to re-run.
   ============================================================================ */

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('dbo.tbDiscountRequests', 'U') IS NULL
BEGIN
    RAISERROR('tbDiscountRequests not found — wrong database?', 16, 1);
    RETURN;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('dbo.tbDiscountRequests')
                 AND name = 'MunicipalDiscountCodeName')
BEGIN
    ALTER TABLE dbo.tbDiscountRequests
        ADD MunicipalDiscountCodeName nvarchar(255) NULL;   -- תאור ההנחה
    PRINT 'tbDiscountRequests.MunicipalDiscountCodeName added.';
END
ELSE
    PRINT 'MunicipalDiscountCodeName already exists — skipped.';
GO

PRINT '';
PRINT 'Existing rows keep it NULL until the current-year file is imported again.';
GO
