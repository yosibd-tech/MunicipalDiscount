/* ============================================================================
   2026-08 — תיקון מספרי נכס שהגיעו פגומים מקובץ החיוב

   Why:
     The municipality's billing export writes long property numbers in
     scientific notation — 192670530010 comes out as 1.92671E+11, i.e. rounded
     to six significant digits. Excel hands that to the import as
     192671000000, and the import created a year row under that wrong number.

     Where the same citizen already had the property under its true number
     (from the discounts file, which is not mangled), they now hold the same
     property twice for the same year: the true row with no billing, and the
     rounded row carrying the billing figures.

   What this script does:
     Moves the billing figures onto the true row and deletes the rounded one.
     A row is only touched when ALL of these hold:
       * the corrupt row carries no discount request (nothing would be lost);
       * exactly one row of that citizen and year rounds to the same value;
       * the true row has no billing of its own to overwrite.

   It does NOT invent the other cases: a citizen who receives no discount was
   never in the discounts file, so their true property number is nowhere in the
   database and cannot be recovered from it. Those rows keep the rounded number
   and are listed at the end — the only real fix is a clean export from the
   municipality.

   Safe to re-run: after the first run nothing matches.

   Going forward MainForm's billing import repairs this while loading, so the
   duplicate rows are not created in the first place.
   ============================================================================ */

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
GO

/* The pairs, before anything is changed. */
PRINT '--- pairs to merge ---';
GO

SELECT c.ClientNumber, bad.AssetNumber AS corrupt_asset, good.AssetNumber AS true_asset,
       bad.AnnualBilling, bad.AssetSize
FROM   dbo.tbYearlyDiscounts bad
INNER  JOIN dbo.tbClients c ON c.ID = bad.ClientId
INNER  JOIN dbo.tbYearlyDiscounts good
        ON  good.ClientId = bad.ClientId
       AND  good.Year_    = bad.Year_
       AND  good.ID      <> bad.ID
       AND  good.AssetNumber NOT LIKE '%[^0-9]%'
       AND  LEN(good.AssetNumber) = LEN(bad.AssetNumber)
       AND  ROUND(CAST(good.AssetNumber AS float) / 1000000.0, 0) * 1000000
            = CAST(bad.AssetNumber AS float)
WHERE  LEN(bad.AssetNumber) >= 12
  AND  bad.AssetNumber LIKE '%000000'
  AND  bad.AssetNumber NOT LIKE '%[^0-9]%'
  AND  good.AnnualBilling IS NULL
  AND  NOT EXISTS (SELECT 1 FROM dbo.tbDiscountRequests r WHERE r.YearlyDiscountId = bad.ID)
ORDER  BY c.ClientNumber;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    /* The rows to merge, resolved once so the UPDATE and the DELETE agree. */
    SELECT bad.ID AS BadId, good.ID AS GoodId,
           bad.AnnualBilling, bad.AssetSize
    INTO   #merge
    FROM   dbo.tbYearlyDiscounts bad
    INNER  JOIN dbo.tbYearlyDiscounts good
            ON  good.ClientId = bad.ClientId
           AND  good.Year_    = bad.Year_
           AND  good.ID      <> bad.ID
           AND  good.AssetNumber NOT LIKE '%[^0-9]%'
           AND  LEN(good.AssetNumber) = LEN(bad.AssetNumber)
           AND  ROUND(CAST(good.AssetNumber AS float) / 1000000.0, 0) * 1000000
                = CAST(bad.AssetNumber AS float)
    WHERE  LEN(bad.AssetNumber) >= 12
      AND  bad.AssetNumber LIKE '%000000'
      AND  bad.AssetNumber NOT LIKE '%[^0-9]%'
      AND  good.AnnualBilling IS NULL
      AND  NOT EXISTS (SELECT 1 FROM dbo.tbDiscountRequests r WHERE r.YearlyDiscountId = bad.ID);

    /* Refuse to act if a corrupt row somehow matches two true rows. */
    IF EXISTS (SELECT 1 FROM #merge GROUP BY BadId HAVING COUNT(*) > 1)
    BEGIN
        RAISERROR('A corrupt row matches more than one property — resolve by hand.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    UPDATE g
    SET    g.AnnualBilling = m.AnnualBilling,
           g.AssetSize     = m.AssetSize
    FROM   dbo.tbYearlyDiscounts g
    INNER  JOIN #merge m ON m.GoodId = g.ID;

    PRINT 'billing moved onto the correct property: ' + CAST(@@ROWCOUNT AS varchar(10));

    DELETE y
    FROM   dbo.tbYearlyDiscounts y
    INNER  JOIN #merge m ON m.BadId = y.ID;

    PRINT 'duplicate rows removed: ' + CAST(@@ROWCOUNT AS varchar(10));

    DROP TABLE #merge;
    COMMIT TRANSACTION;
    PRINT 'Committed.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    PRINT 'Rolled back — nothing was changed.';
    THROW;
END CATCH
GO

/* ---------------------------------------------------------------------------
   What is left: properties whose true number is not in the database, because
   their owner receives no discount and so was never in the discounts file.
   These are not duplicates — just a property recorded under a rounded number.
   --------------------------------------------------------------------------- */
PRINT '';
PRINT '--- still carrying a rounded asset number (not recoverable from our data) ---';
GO

SELECT COUNT(*) AS unrecoverable
FROM   dbo.tbYearlyDiscounts
WHERE  LEN(AssetNumber) >= 12
  AND  AssetNumber LIKE '%000000'
  AND  AssetNumber NOT LIKE '%[^0-9]%';
GO
