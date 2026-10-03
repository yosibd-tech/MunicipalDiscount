/* ============================================================================
   2026-08b — tbDiscountRates renamed to tbDiscountCodeRates

   Follow-up to 2026-08_DiscountCodes_Rates.sql, which created the table under
   the shorter name. The table is still empty (the codes are loaded from Excel
   afterwards), so it is dropped and recreated rather than renamed with
   sp_rename — that way the primary key, the foreign key, the unique constraint
   and the index all carry the new name too, instead of keeping the old one.

   Run once, after 2026-08_DiscountCodes_Rates.sql. Guarded: does nothing if the
   rename already happened, and refuses to run if any rate rows exist.
   ============================================================================ */

SET NOCOUNT ON;
GO

IF OBJECT_ID('dbo.tbDiscountRates', 'U') IS NOT NULL
BEGIN
    DECLARE @rows int;
    SELECT @rows = COUNT(*) FROM dbo.tbDiscountRates;

    IF @rows > 0
    BEGIN
        RAISERROR('tbDiscountRates holds %d row(s) — load them again after the rename, or migrate them by hand.', 16, 1, @rows);
        RETURN;
    END

    IF OBJECT_ID('dbo.vwDiscountRateAsOf', 'V') IS NOT NULL
        DROP VIEW dbo.vwDiscountRateAsOf;

    DROP TABLE dbo.tbDiscountRates;
    PRINT 'tbDiscountRates dropped (was empty).';
END
ELSE
    PRINT 'tbDiscountRates does not exist — nothing to rename.';
GO

IF OBJECT_ID('dbo.tbDiscountCodeRates', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbDiscountCodeRates
    (
        ID                      int IDENTITY(1,1)  NOT NULL,
        DiscountCodeId          int                NOT NULL,    -- FK -> tbDiscountsCodes.ID
        ValidityDate            date               NOT NULL,    -- תאריך תחילת תוקף
        DiscountPercent         decimal(9,4)       NULL,        -- אחוז הנחה
        PreviousDiscountPercent decimal(9,4)       NULL,        -- אחוז הנחה קודם
        MaximumDiscount         decimal(9,4)       NULL,        -- מקסימום הנחה
        MaximumArea             decimal(10,2)      NULL,        -- מקסימום שטח

        CONSTRAINT PK_tbDiscountCodeRates PRIMARY KEY CLUSTERED (ID),

        CONSTRAINT FK_tbDiscountCodeRates_Code FOREIGN KEY (DiscountCodeId)
            REFERENCES dbo.tbDiscountsCodes (ID) ON DELETE CASCADE,

        -- A code cannot have two different rates starting the same day.
        CONSTRAINT UQ_tbDiscountCodeRates_CodeDate UNIQUE (DiscountCodeId, ValidityDate)
    );

    PRINT 'tbDiscountCodeRates created.';
END
ELSE
    PRINT 'tbDiscountCodeRates already exists — skipped.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'IX_tbDiscountCodeRates_CodeDate'
                 AND object_id = OBJECT_ID('dbo.tbDiscountCodeRates'))
    CREATE NONCLUSTERED INDEX IX_tbDiscountCodeRates_CodeDate
        ON dbo.tbDiscountCodeRates (DiscountCodeId, ValidityDate DESC)
        INCLUDE (DiscountPercent, MaximumDiscount, MaximumArea);
GO

/* The view is recreated here because it names the table. See section 6 of
   2026-08_DiscountCodes_Rates.sql for how callers are meant to use it. */
IF OBJECT_ID('dbo.vwDiscountRateAsOf', 'V') IS NOT NULL
    DROP VIEW dbo.vwDiscountRateAsOf;
GO

CREATE VIEW dbo.vwDiscountRateAsOf
AS
    SELECT
        c.ID                        AS DiscountCodeId,
        c.MunicipalDiscountCode,
        c.LegallDiscountCode,
        c.Categories,
        c.DiscountName,
        c.GroupCode,
        c.Mandatory,
        c.LawInForce,
        r.ID                        AS RateId,
        r.ValidityDate              AS RateFrom,
        LEAD(r.ValidityDate) OVER (PARTITION BY c.ID ORDER BY r.ValidityDate) AS RateUntil,
        r.DiscountPercent,
        r.PreviousDiscountPercent,
        r.MaximumDiscount,
        r.MaximumArea
    FROM dbo.tbDiscountsCodes c
    INNER JOIN dbo.tbDiscountCodeRates r ON r.DiscountCodeId = c.ID;
GO

PRINT 'Done.';
GO
