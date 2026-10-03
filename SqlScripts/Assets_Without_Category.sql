/* נכסים ללא קטגוריה — לפי לקוח
   ---------------------------------------------------------------
   אחרי 2026-09_YearlyDiscounts_AssetCode.sql הקטגוריה מגיעה ישירות:
        tbYearlyDiscounts.AssetCode -> tbGAssetKinds.Code -> Category

   הקטגוריה חסרה כאשר:
     * AssetCode ריק           — הנכס לא נמצא בטבלת היתרות ועדיין לא נקלט מהקובץ
     * AssetCode לא מוגדר      — קוד שאין לו שורה ב-tbGAssetKinds
     * Category ריקה           — יש שורת סוג נכס, אך בלי קטגוריה

   קריאה בלבד. */

DECLARE @Year int = 2026;   -- NULL = כל השנים

/* ---------- פירוט ---------- */

SELECT  c.ClientNumber                        AS [מספר לקוח],
        c.ClientName                          AS [שם לקוח],
        y.Year_                               AS [שנה],
        y.AssetNumber                         AS [מספר נכס],
        y.AssetCode                           AS [קוד סוג נכס],
        k.Name                                AS [שם סוג נכס],
        k.MainCategory                        AS [קטגוריה ראשית],
        y.AnnualBilling                       AS [חיוב שנתי],
        y.AssetSize                           AS [שטח],
        y.Area                                AS [אזור],
        CASE
            WHEN y.AssetCode IS NULL THEN N'אין קוד סוג נכס'
            WHEN k.Code      IS NULL THEN N'קוד סוג הנכס לא מוגדר בטבלת סוגי הנכסים'
            ELSE                          N'לסוג הנכס אין קטגוריה'
        END                                   AS [סיבה]
FROM        dbo.tbYearlyDiscounts y
INNER  JOIN dbo.tbClients        c ON c.ID   = y.ClientId
LEFT   JOIN dbo.tbGAssetKinds    k ON k.Code = y.AssetCode
WHERE  (k.Category IS NULL OR LTRIM(RTRIM(k.Category)) = N'')
  AND  (@Year IS NULL OR y.Year_ = @Year)
ORDER BY c.ClientNumber, y.AssetNumber;


/* ---------- סיכום לפי לקוח ---------- */

SELECT  c.ClientNumber                                        AS [מספר לקוח],
        c.ClientName                                          AS [שם לקוח],
        COUNT(*)                                              AS [נכסים ללא קטגוריה],
        SUM(CASE WHEN y.AnnualBilling > 0 THEN 1 ELSE 0 END)  AS [מתוכם עם חיוב],
        SUM(y.AnnualBilling)                                  AS [חיוב שנתי],
        (SELECT COUNT(*) FROM dbo.tbYearlyDiscounts y2
         WHERE y2.ClientId = y.ClientId
           AND (@Year IS NULL OR y2.Year_ = @Year))           AS [סה"כ נכסים ללקוח]
FROM        dbo.tbYearlyDiscounts y
INNER  JOIN dbo.tbClients     c ON c.ID   = y.ClientId
LEFT   JOIN dbo.tbGAssetKinds k ON k.Code = y.AssetCode
WHERE  (k.Category IS NULL OR LTRIM(RTRIM(k.Category)) = N'')
  AND  (@Year IS NULL OR y.Year_ = @Year)
GROUP BY c.ClientNumber, c.ClientName, y.ClientId
ORDER BY SUM(y.AnnualBilling) DESC, COUNT(*) DESC;


/* ---------- סיכום לפי סיבה ---------- */

SELECT  CASE
            WHEN y.AssetCode IS NULL THEN N'אין קוד סוג נכס'
            WHEN k.Code      IS NULL THEN N'קוד סוג הנכס לא מוגדר בטבלת סוגי הנכסים'
            ELSE                          N'לסוג הנכס אין קטגוריה'
        END                                                   AS [סיבה],
        COUNT(*)                                              AS [נכסים],
        COUNT(DISTINCT y.ClientId)                            AS [לקוחות],
        SUM(y.AnnualBilling)                                  AS [חיוב שנתי]
FROM        dbo.tbYearlyDiscounts y
LEFT   JOIN dbo.tbGAssetKinds k ON k.Code = y.AssetCode
WHERE  (k.Category IS NULL OR LTRIM(RTRIM(k.Category)) = N'')
  AND  (@Year IS NULL OR y.Year_ = @Year)
GROUP BY CASE
            WHEN y.AssetCode IS NULL THEN N'אין קוד סוג נכס'
            WHEN k.Code      IS NULL THEN N'קוד סוג הנכס לא מוגדר בטבלת סוגי הנכסים'
            ELSE                          N'לסוג הנכס אין קטגוריה'
         END
ORDER BY COUNT(*) DESC;
