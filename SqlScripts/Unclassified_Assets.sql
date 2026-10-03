/* נכסים שלא קיבלו קטגוריה — "(no kind row)" ו-"(empty category)"
   ---------------------------------------------------------------
   השרשרת: tbYearlyDiscounts -> tbGOutstandingBalances (AssetNumber)
            -> tbGAssetKinds (Code = AssetType) -> Category

   כל שלב בשרשרת יכול להישבר, והעמודה [סיבה] אומרת היכן.
   קריאה בלבד. */

DECLARE @Year int = 2026;

WITH a AS (
    SELECT  y.ID                                   AS YearlyId,
            y.AssetNumber,
            y.AnnualBilling,
            y.AssetSize,
            y.Area,
            c.ClientNumber,
            c.ClientName,
            TRY_CAST(y.AssetNumber AS bigint)      AS AssetNum,
            b.AssetType,
            k.Code                                 AS KindCode,
            k.Name                                 AS KindName,
            k.Category
    FROM        tbYearlyDiscounts y
    INNER  JOIN tbClients c ON c.ID = y.ClientId
    OUTER  APPLY (SELECT TOP 1 b2.AssetType
                  FROM   tbGOutstandingBalances b2
                  WHERE  b2.AssetNumber = TRY_CAST(y.AssetNumber AS bigint)) b
    LEFT   JOIN tbGAssetKinds k ON k.Code = b.AssetType
    WHERE  y.Year_ = @Year
)
SELECT  a.ClientNumber                    AS [מספר לקוח],
        a.ClientName                      AS [שם לקוח],
        a.AssetNumber                     AS [מספר נכס],
        a.AnnualBilling                   AS [חיוב שנתי],
        a.AssetSize                       AS [שטח],
        a.Area                            AS [אזור],
        a.AssetType                       AS [קוד סוג נכס],
        a.KindName                        AS [שם סוג נכס],
        a.Category                        AS [קטגוריה],
        CASE
            WHEN a.AssetNumber IS NULL     THEN N'מספר נכס ריק'
            WHEN a.AssetNum    IS NULL     THEN N'מספר נכס פגום (רישום מדעי מאקסל)'
            WHEN a.AssetType   IS NULL     THEN N'הנכס לא קיים בטבלת היתרות'
            WHEN a.KindCode    IS NULL     THEN N'קוד סוג הנכס לא מוגדר בטבלת סוגי הנכסים'
            WHEN a.Category    IS NULL     THEN N'קטגוריה ריקה'
            WHEN LTRIM(RTRIM(a.Category)) = N'' THEN N'קטגוריה ריקה'
            ELSE N'?'
        END                               AS [סיבה]
FROM    a
WHERE   a.Category IS NULL
   OR   LTRIM(RTRIM(a.Category)) = N''
ORDER BY a.AnnualBilling DESC, a.ClientNumber;


/* ---------- סיכום לפי סיבה ---------- */

WITH a AS (
    SELECT  y.AnnualBilling,
            TRY_CAST(y.AssetNumber AS bigint) AS AssetNum,
            y.AssetNumber,
            b.AssetType,
            k.Code AS KindCode,
            k.Category
    FROM        tbYearlyDiscounts y
    OUTER  APPLY (SELECT TOP 1 b2.AssetType
                  FROM   tbGOutstandingBalances b2
                  WHERE  b2.AssetNumber = TRY_CAST(y.AssetNumber AS bigint)) b
    LEFT   JOIN tbGAssetKinds k ON k.Code = b.AssetType
    WHERE  y.Year_ = @Year
)
SELECT  CASE
            WHEN a.AssetNumber IS NULL THEN N'מספר נכס ריק'
            WHEN a.AssetNum    IS NULL THEN N'מספר נכס פגום (רישום מדעי מאקסל)'
            WHEN a.AssetType   IS NULL THEN N'הנכס לא קיים בטבלת היתרות'
            WHEN a.KindCode    IS NULL THEN N'קוד סוג הנכס לא מוגדר בטבלת סוגי הנכסים'
            ELSE N'קטגוריה ריקה'
        END                          AS [סיבה],
        COUNT(*)                     AS [נכסים],
        SUM(a.AnnualBilling)         AS [חיוב שנתי],
        SUM(CASE WHEN a.AnnualBilling > 0 THEN 1 ELSE 0 END) AS [מתוכם עם חיוב]
FROM    a
WHERE   a.Category IS NULL
   OR   LTRIM(RTRIM(a.Category)) = N''
GROUP BY CASE
            WHEN a.AssetNumber IS NULL THEN N'מספר נכס ריק'
            WHEN a.AssetNum    IS NULL THEN N'מספר נכס פגום (רישום מדעי מאקסל)'
            WHEN a.AssetType   IS NULL THEN N'הנכס לא קיים בטבלת היתרות'
            WHEN a.KindCode    IS NULL THEN N'קוד סוג הנכס לא מוגדר בטבלת סוגי הנכסים'
            ELSE N'קטגוריה ריקה'
         END
ORDER BY COUNT(*) DESC;
