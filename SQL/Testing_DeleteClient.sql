/* =====================================================================
   מחיקת לקוח אחד מכל שלוש הטבלאות — לבדיקות בלבד
   ---------------------------------------------------------------------
   Removes every trace of ONE client number, so the same citizen can be
   added again from the web page and the whole flow tested from the start:
   הוספת לקוח -> חשב הנחה -> אישור הנחה -> הדפסה.

   Deleted, child first, because each points at the one above it:

     tbDiscountRequests   -> the decisions, by YearlyDiscountId
     tbYearlyDiscounts    -> the year/property rows, by ClientId
     tbClients            -> the citizen

   FOR A TEST DATABASE. On the live database this erases imported billing
   and decisions that cannot be recovered - there is no undo.

   HOW TO RUN
     1. Put the client number in @ClientNumber below.
     2. Run it. It prints what it is about to delete and stops (@WhatIf = 1).
     3. Read the counts. If they are what you expect, set @WhatIf = 0 and
        run again to delete.

   Everything runs in one transaction: either all three tables lose the
   client, or none of them does.
   ===================================================================== */

SET NOCOUNT ON;

DECLARE @ClientNumber nvarchar(20) = N'123456789';   -- <<< מספר הלקוח
DECLARE @WhatIf       bit          = 1;              -- 1 = הצג בלבד, 0 = מחק

/* ---------- the client ---------- */

DECLARE @ClientId int =
    (SELECT ID FROM tbClients WHERE ClientNumber = @ClientNumber);

IF @ClientId IS NULL
BEGIN
    PRINT N'לא נמצא לקוח ' + @ClientNumber + N' — אין מה למחוק.';
    RETURN;
END

/* ---------- what is about to go ---------- */

SELECT  c.ID AS ClientId, c.ClientNumber, c.ClientName
FROM    tbClients c
WHERE   c.ID = @ClientId;

SELECT  y.ID AS YearlyId, y.Year_, y.AssetNumber, y.AssetCode,
        y.AnnualBilling, y.AssetSize
FROM    tbYearlyDiscounts y
WHERE   y.ClientId = @ClientId
ORDER BY y.Year_ DESC, y.AssetNumber;

SELECT  r.ID AS RequestId, y.Year_, y.AssetNumber, r.RequestDate,
        r.MunicipalDiscountCode, r.EntitledDiscountPercent,
        r.ComputedDiscount, r.ActualDiscountAmount, r.Status
FROM    tbDiscountRequests r
INNER   JOIN tbYearlyDiscounts y ON y.ID = r.YearlyDiscountId
WHERE   y.ClientId = @ClientId
ORDER BY y.Year_ DESC, y.AssetNumber, r.ID;

DECLARE @Requests int =
    (SELECT COUNT(*)
     FROM   tbDiscountRequests r
     INNER  JOIN tbYearlyDiscounts y ON y.ID = r.YearlyDiscountId
     WHERE  y.ClientId = @ClientId);

DECLARE @YearRows int =
    (SELECT COUNT(*) FROM tbYearlyDiscounts WHERE ClientId = @ClientId);

PRINT N'לקוח ' + @ClientNumber + N' (ID ' + CAST(@ClientId AS nvarchar(20)) + N'): '
    + CAST(@Requests AS nvarchar(20)) + N' בקשות, '
    + CAST(@YearRows AS nvarchar(20)) + N' שורות שנה.';

IF @WhatIf = 1
BEGIN
    PRINT N'הצגה בלבד — לא נמחק דבר. יש לקבוע @WhatIf = 0 כדי למחוק.';
    RETURN;
END

/* ---------- the deletion ---------- */

BEGIN TRY
    BEGIN TRANSACTION;

        DELETE  r
        FROM    tbDiscountRequests r
        INNER   JOIN tbYearlyDiscounts y ON y.ID = r.YearlyDiscountId
        WHERE   y.ClientId = @ClientId;

        PRINT N'נמחקו ' + CAST(@@ROWCOUNT AS nvarchar(20)) + N' בקשות הנחה.';

        DELETE  FROM tbYearlyDiscounts WHERE ClientId = @ClientId;
        PRINT N'נמחקו ' + CAST(@@ROWCOUNT AS nvarchar(20)) + N' שורות שנה.';

        DELETE  FROM tbClients WHERE ID = @ClientId;
        PRINT N'נמחקו ' + CAST(@@ROWCOUNT AS nvarchar(20)) + N' לקוחות.';

    COMMIT TRANSACTION;
    PRINT N'הלקוח נמחק. ניתן להוסיף אותו מחדש מהדף.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

    PRINT N'שגיאה — לא נמחק דבר:';
    THROW;
END CATCH
