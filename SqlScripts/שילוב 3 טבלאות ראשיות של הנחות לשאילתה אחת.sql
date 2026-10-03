USE MunicipalDiscount;
GO
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
    y.AssetCode                 AS y_AssetCode,
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
--where RequestDate is null and ActualDiscountAmount is not null--and AssetCode = 100 and MunicipalDiscountCode in (720,730,740,750,760,770,780,790)

--WHERE c.ClientNumber = 33619503--24739989--36433258
--WHERE c.ClientNumber 86969813
--WHERE c.ClientNumber N'66047689'
where c.ClientNumber=200661650
--ORDER BY y.Year_ DESC, r.ID; where [ClientNumber]=40706601
  --where EntitledDiscountPercent is  null
  --where AssetCode is null
 -- where y.Year_=2025 --and AnnualBilling is not null