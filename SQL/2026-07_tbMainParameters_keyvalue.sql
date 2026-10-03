-- Converts MunicipalDiscount's private tbMainParameters from a single wide row
-- into a key/value settings table (the shape edited by ManParametersForm's
-- grid). This table is NOT shared with MonicipalYBD (which uses tbGAppSettings).
--
--   [Key]              English identifier read by ManParameters.
--   [Value]            The real value the app uses (English folder path,
--                      Python path, header-row number, or message text).
--   [Description]      Free-text note shown in the parameters grid.
--   [HebrewFolderName] Hebrew alias — the desktop.ini display name for folder
--                      rows (group 'סיפריות קבצים'); NULL for non-folder rows.
--   [ParametersGroup]  Grid grouping.
--
-- The original wide row is preserved in tbMainParameters_OLD.

IF OBJECT_ID('dbo.tbMainParameters') IS NOT NULL
   AND OBJECT_ID('dbo.tbMainParameters_OLD') IS NULL
   AND COL_LENGTH('dbo.tbMainParameters', 'Key') IS NULL   -- only the OLD wide table lacks [Key]
    EXEC sp_rename 'dbo.tbMainParameters', 'tbMainParameters_OLD';
GO

IF OBJECT_ID('dbo.tbMainParameters') IS NULL
    CREATE TABLE dbo.tbMainParameters
    (
        [Key]              nvarchar(200)  NOT NULL PRIMARY KEY,
        [Value]            nvarchar(2000) NULL,
        [Description]      nvarchar(500)  NULL,
        [HebrewFolderName] nvarchar(200)  NULL,
        [ParametersGroup]  nvarchar(200)  NULL
    );
GO

MERGE dbo.tbMainParameters AS t
USING (VALUES
    -- Managed folders (group 'סיפריות קבצים'): Value = English path on disk,
    -- HebrewFolderName = Hebrew alias shown in Explorer via desktop.ini.
    (N'ImportFolder',                  N'C:\MunicipalDiscount\Import',                                     N'תיקיית יבוא ראשית',               N'יבוא',                          N'סיפריות קבצים'),
    (N'HelperFilesFolder',             N'C:\MunicipalDiscount\Import\HelperFiles',                         N'תיקיית קבצי עזר',                 N'קובצי עזר',                     N'סיפריות קבצים'),
    (N'PreviousDiscountPath',          N'C:\MunicipalDiscount\Import\PreviouseYearDiscount',               N'נתיב קובץ הנחות ש.קודמת',          N'קובץ הנחות שניתנו בשנה הקודמת',  N'סיפריות קבצים'),
    (N'ActualCurrentYearDiscountPath', N'C:\MunicipalDiscount\Import\CurrentYearDiscount',                 N'נתיב קובץ הנחות ש.שוטפת',          N'קובץ הנחות שניתנו בשנה השוטפת',  N'סיפריות קבצים'),
    (N'CurrentYearBillingPath',        N'C:\MunicipalDiscount\Import\CurrentYearBilling',                  N'נתיב קובץ חיוב ארנונה',            N'קובץ חיוב ארנונה בשנה השוטפת',   N'סיפריות קבצים'),
    (N'IncomeCriteriaPath',            N'C:\MunicipalDiscount\Import\HelperFiles\IncomeTestCreteriasFile', N'נתיב קובץ קריטריונים למבחן הכנסה', N'קובץ קריטריונים למבחן הכנסה',    N'סיפריות קבצים'),
    -- Non-folder settings
    (N'PythonScriptsPath',             N'C:\Users\User\source\repos\MunicipalDiscount\PythonScripts',      N'תיקיית סקריפטים Python',           NULL,                             N'הגדרות שונות'),
    (N'DiscountExcelHeaderRowNum',     N'11',                                                             N'שורת כותרות קובץ הנחות',           NULL,                             N'הגדרות שונות'),
    (N'BllingExcelHeaderRowNum',       N'3',                                                              N'שורת כותרות קובץ חיוב',            NULL,                             N'הגדרות שונות'),
    (N'IllegalDiscountMessage',        N'קבלת הנחה לא חוקית בסך {סכום}, נא לבוא לסדר את החוב שנוצר עקב כך ולמנוע הליכי אכיפה, תודה.', N'נוסח הודעה להנחה לא חוקית',       NULL, N'הודעות'),
    (N'IllegalDiscountMessageHeb',     N'קבלת הנחה לא חוקית בסך {סכום}, נא לבוא לסדר את החוב שנוצר עקב כך ולמנוע הליכי אכיפה, תודה.', N'נוסח הודעה להנחה לא חוקית (עברית)', NULL, N'הודעות')
) AS s([Key],[Value],[Description],[HebrewFolderName],[ParametersGroup])
ON t.[Key] = s.[Key]
WHEN MATCHED THEN UPDATE SET
    [Value] = s.[Value], [Description] = s.[Description],
    [HebrewFolderName] = s.[HebrewFolderName], [ParametersGroup] = s.[ParametersGroup]
WHEN NOT MATCHED THEN
    INSERT ([Key],[Value],[Description],[HebrewFolderName],[ParametersGroup])
    VALUES (s.[Key], s.[Value], s.[Description], s.[HebrewFolderName], s.[ParametersGroup]);
GO
