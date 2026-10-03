/* =====================================================================
   איפוס נתוני לקוחות — מחיקה וקליטה מחדש בסדר הנכון
   ---------------------------------------------------------------------
   מוחק את tbDiscountRequests, tbYearlyDiscounts ו-tbClients בלבד —
   את כל השורות, כולל החלטות זכאות שאושרו במסך (Status = 1). אין עצירה.

   מה שנשאר ואסור למחוק:
     tbDiscountsCodes, tbDiscountCodeRates   — קודי ההנחה והתעריפים
     tbIncomeDiscountCriteria                — מדרגות ההכנסה
     tbDiscountHeaders                       — מיפוי עמודות קובץ ההנחות
     tbMainParameters, tbUsers               — הגדרות ומשתמשים
     tbGAssetKinds, tbGOutstandingBalances   — של MonicipalYBD

   סדר המחיקה כפוי: שלושת מפתחות הזרים הם NO_ACTION, בלי CASCADE.

   !! הרצה אחרי גיבוי בלבד !!
   ===================================================================== */

/* מסד הנתונים נבחר כאן ולא בתיבה שבסרגל של SSMS. חיבור שנפתח על master
   נכשל בכל משפט עם Invalid object name, והסקריפט נראה כאילו רץ ולא מחק
   דבר. אם המסד נקרא אחרת בהתקנה הזו — לשנות כאן. */

USE MunicipalDiscount;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/* ---------- מה עומד להימחק ---------- */

SELECT 'לפני המחיקה' AS [שלב],
       (SELECT COUNT(*) FROM dbo.tbDiscountRequests) AS [בקשות],
       (SELECT COUNT(*) FROM dbo.tbYearlyDiscounts)  AS [שורות שנה],
       (SELECT COUNT(*) FROM dbo.tbClients)          AS [לקוחות];
GO

/* ---------- דיווח: כמה החלטות זכאות עומדות להימחק ---------- */
/* כאן עמדה עצירה כשנמצאו החלטות שעובד אישר. היא הוסרה ביודעין: איפוס
   פירושו איפוס, והסקריפט מוחק גם אותן. המספר עדיין מודפס — מי שמריץ
   צריך לראות מה נמחק בזמן אמת, ולא לגלות זאת אחר כך. */

DECLARE @decisions int =
    (SELECT COUNT(*) FROM dbo.tbDiscountRequests
     WHERE EntitledDiscountPercent IS NOT NULL
        OR ComputedDiscount IS NOT NULL
        OR Status = 1);

IF @decisions > 0
    PRINT CONCAT(N'שים לב: נמחקות גם ', @decisions, N' החלטות זכאות שנשמרו.');
ELSE
    PRINT N'אין החלטות זכאות שנשמרו.';
GO

/* ---------- המחיקה ---------- */

BEGIN TRANSACTION;

    DELETE FROM dbo.tbDiscountRequests;
    PRINT CONCAT('tbDiscountRequests: ', @@ROWCOUNT, ' rows deleted');

    DELETE FROM dbo.tbYearlyDiscounts;
    PRINT CONCAT('tbYearlyDiscounts:  ', @@ROWCOUNT, ' rows deleted');

    DELETE FROM dbo.tbClients;
    PRINT CONCAT('tbClients:          ', @@ROWCOUNT, ' rows deleted');

COMMIT TRANSACTION;
GO

/* ---------- אתחול מונה הזהות ----------
   לא הכרחי — המזהים פנימיים — אבל מתחילים מ-1 ומקלים על קריאת הנתונים
   בבדיקה הראשונה. TRUNCATE לא היה אפשרי כאן: הטבלאות מפתח זר. */

DBCC CHECKIDENT ('dbo.tbDiscountRequests', RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.tbYearlyDiscounts',  RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.tbClients',          RESEED, 0) WITH NO_INFOMSGS;
PRINT N'מוני הזהות אופסו.';
GO

SELECT 'אחרי המחיקה' AS [שלב],
       (SELECT COUNT(*) FROM dbo.tbDiscountRequests) AS [בקשות],
       (SELECT COUNT(*) FROM dbo.tbYearlyDiscounts)  AS [שורות שנה],
       (SELECT COUNT(*) FROM dbo.tbClients)          AS [לקוחות];
GO

/* =====================================================================
   הקליטה מחדש — בסדר הזה בלבד
   ---------------------------------------------------------------------
   1. יבוא חיוב שנתי      — יוצר לקוחות, שורות שנה, AssetCode.

      הקובץ מכיל את כל סוגי הנכסים — מגורים, מסחר, תעשיה, קרקע וכו'.
      אין לסנן אותו ידנית. הקליטה מסננת בעצמה לפי "סוג נכס" מול
      tbGAssetKinds ושומרת רק Category = 'מגורים' (קודים 100 ו-110),
      ומדווחת בסיכום כמה שורות לא נקלטו ומאיזו סיבה.

      חובה שהקובץ יכלול את העמודה "סוג נכס" — בלעדיה הקליטה נעצרת,
      כי אי אפשר לדעת אילו שורות הן מגורים.
   2. יבוא נתוני שנה שוטפת — מצמיד הנחות לנכסים שכבר קיימים.
                             לא יוצר שורות שנה. מדווח על הנחות שלא ניתן
                             להצמיד — אלו נכסים שאינם מגורים.
   3. קליטת תאריכים מקובץ תנועות — ממלא RequestDate.

   הרצה בסדר אחר תיצור נתונים חסרים: קליטת ההנחות לפני החיוב תדווח שאף
   הנחה לא נמצאה, כי אין עדיין נכסים להצמיד אליהם.
   ===================================================================== */
