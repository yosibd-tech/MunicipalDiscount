/* כל העמודות של הלקוח, שורות השנה והבקשות — לפי מספר לקוח */

SELECT
    -- tbClients
    c.ID                        AS c_ID,
    c.ClientNumber              AS c_ClientNumber,
    c.ClientName                AS c_ClientName,
    c.ClieneWifeNumber          AS c_ClieneWifeNumber,
    c.PhoneNumber               AS c_PhoneNumber,

    -- tbYearlyDiscounts
    y.ID                        AS y_ID,
    y.ClientId                  AS y_ClientId,
    y.Year_                     AS y_Year_,
    y.AssetNumber               AS y_AssetNumber,
    y.AnnualBilling             AS y_AnnualBilling,
    y.AssetSize                 AS y_AssetSize,
    y.Area                      AS y_Area,

    -- tbDiscountRequests
    r.ID                        AS r_ID,
    r.YearlyDiscountId          AS r_YearlyDiscountId,
    r.RequestDate               AS r_RequestDate,
    r.MunicipalDiscountCode     AS r_MunicipalDiscountCode,
    r.MunicipalDiscountCodeName AS r_MunicipalDiscountCodeName,
    r.ActualDiscountAmount      AS r_ActualDiscountAmount,
    r.EntitledDiscountPercent   AS r_EntitledDiscountPercent,
    r.ComputedDiscount          AS r_ComputedDiscount,
    r.DiscountRateId            AS r_DiscountRateId,
    r.Status                    AS r_Status,
    r.UserId                    AS r_UserId,
    r.CreateDate                AS r_CreateDate
FROM       tbClients          c
LEFT  JOIN tbYearlyDiscounts  y ON y.ClientId         = c.ID
LEFT  JOIN tbDiscountRequests r ON r.YearlyDiscountId = y.ID
WHERE c.ClientNumber = N'28264182'
ORDER BY y.Year_ DESC, r.ID;
