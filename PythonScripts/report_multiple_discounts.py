"""
דוח כפל הנחות — לקוחות שקיבלו יותר מהנחה אחת באותה שנה.

    python report_multiple_discounts.py <year> [output.xlsx]

בנכסי מגורים לקוח זכאי להנחה אחת בלבד. לכן כל לקוח שיש לו יותר מקוד הנחה
אחד נכנס לדוח.

השורות מקובצות לפי לקוח וקוד הנחה: עשר שורות של אותו קוד הן הנחה אחת
ומצטמצמות לשורה אחת. מה שמכניס לקוח לדוח הוא מספר הקודים השונים שלו, ולא
מספר השורות.

Standalone on purpose: it reads, it writes one workbook, and it needs neither
the application running nor a button pressed. Without an output path it writes
into the reports folder from tbMainParameters, named by year and timestamp.

Counted per CITIZEN, not per property: the entitlement belongs to the person,
so two dwellings with one discount each is still two discounts.

Residential only, joined through tbGAssetKinds.Category — the same filter the
other reports and the billing import use. A discount on a shop is outside what
this application judges.

Requires: pyodbc, openpyxl
"""

import datetime
import os
import sys

import pyodbc
from openpyxl import Workbook
from openpyxl.styles import Alignment, Border, Font, PatternFill, Side
from openpyxl.utils import get_column_letter

# אין סיסמה בקוד. ראה db_config.py.
from db_config import CONN_STR


SUMMARY_SQL = """
    SELECT c.ClientNumber                AS [מספר לקוח],
           c.ClientName                  AS [שם לקוח],
           c.PhoneNumber                 AS [טלפון],
           r.MunicipalDiscountCode       AS [קוד הנחה],
           MAX(r.MunicipalDiscountCodeName) AS [שם ההנחה],
           CASE WHEN MAX(inc.Code) IS NOT NULL THEN N'כן' ELSE N'' END
                                         AS [מבחן הכנסה],
           COUNT(*)                      AS [שורות],
           COUNT(DISTINCT y.ID)          AS [נכסים],
           MIN(r.RequestDate)            AS [תאריך ראשון],
           MAX(r.RequestDate)            AS [תאריך אחרון],
           SUM(CASE WHEN r.ActualDiscountAmount < 0
                    THEN -r.ActualDiscountAmount ELSE 0 END) AS [סך שניתן],
           SUM(CASE WHEN r.ActualDiscountAmount > 0
                    THEN  r.ActualDiscountAmount ELSE 0 END) AS [סך שזוכה]
    FROM        tbDiscountRequests r
    INNER  JOIN tbYearlyDiscounts  y ON y.ID   = r.YearlyDiscountId
    INNER  JOIN tbClients          c ON c.ID   = y.ClientId
    INNER  JOIN tbGAssetKinds      k ON k.Code = y.AssetCode
                                    AND k.Category = N'מגורים'
    -- קודי מבחן ההכנסה של אותה שנה, כטבלה נגזרת ולא כתת-שאילתה בתוך SUM:
    -- SQL Server אינו מאפשר צבירה על ביטוי שמכיל תת-שאילתה.
    LEFT   JOIN (SELECT DISTINCT ic.MunicipalDiscountCode AS Code
                 FROM   tbIncomeDiscountCriteria ic
                 WHERE  ic.Year_ = ?
                   AND  ic.MunicipalDiscountCode IS NOT NULL) inc
                                   ON inc.Code = r.MunicipalDiscountCode
    WHERE  y.Year_ = ?
      AND  y.ClientId IN (
               SELECT y2.ClientId
               FROM   tbDiscountRequests r2
               INNER  JOIN tbYearlyDiscounts y2 ON y2.ID   = r2.YearlyDiscountId
               INNER  JOIN tbGAssetKinds     k2 ON k2.Code = y2.AssetCode
                                               AND k2.Category = N'מגורים'
               WHERE  y2.Year_ = ?
               GROUP BY y2.ClientId
               -- קוד שאינו ידוע הוא קוד בפני עצמו, ולא "אין קוד": שתי הנחות
               -- שאחת מהן חסרת קוד הן עדיין שתי הנחות.
               HAVING COUNT(DISTINCT ISNULL(r2.MunicipalDiscountCode, -1)) > 1)
    GROUP BY c.ClientNumber, c.ClientName, c.PhoneNumber, r.MunicipalDiscountCode
    ORDER BY c.ClientNumber, r.MunicipalDiscountCode
"""

DETAIL_SQL = """
    SELECT c.ClientNumber                AS [מספר לקוח],
           c.ClientName                  AS [שם לקוח],
           y.AssetNumber                 AS [מספר נכס],
           k.Name                        AS [סוג נכס],
           y.AssetSize                   AS [שטח],
           y.AnnualBilling               AS [חיוב שנתי],
           r.RequestDate                 AS [תאריך],
           r.MunicipalDiscountCode       AS [קוד הנחה],
           r.MunicipalDiscountCodeName   AS [שם ההנחה],
           CASE WHEN r.ActualDiscountAmount < 0
                THEN -r.ActualDiscountAmount END AS [ניתן],
           CASE WHEN r.ActualDiscountAmount > 0
                THEN  r.ActualDiscountAmount END AS [זיכוי],
           r.EntitledDiscountPercent     AS [אחוז זכאות],
           r.ComputedDiscount            AS [הנחה מחושבת],
           CASE WHEN r.Status = 1 THEN N'הוחלט' ELSE N'לא הוחלט' END AS [סטטוס]
    FROM        tbDiscountRequests r
    INNER  JOIN tbYearlyDiscounts  y ON y.ID   = r.YearlyDiscountId
    INNER  JOIN tbClients          c ON c.ID   = y.ClientId
    INNER  JOIN tbGAssetKinds      k ON k.Code = y.AssetCode
                                    AND k.Category = N'מגורים'
    WHERE  y.Year_ = ?
      AND  y.ClientId IN (
               SELECT y2.ClientId
               FROM   tbDiscountRequests r2
               INNER  JOIN tbYearlyDiscounts y2 ON y2.ID   = r2.YearlyDiscountId
               INNER  JOIN tbGAssetKinds     k2 ON k2.Code = y2.AssetCode
                                               AND k2.Category = N'מגורים'
               WHERE  y2.Year_ = ?
               GROUP BY y2.ClientId
               HAVING COUNT(DISTINCT ISNULL(r2.MunicipalDiscountCode, -1)) > 1)
    ORDER BY c.ClientNumber, y.AssetNumber, r.RequestDate, r.ID
"""

HEADER_FILL = PatternFill("solid", fgColor="2A5298")
HEADER_FONT = Font(bold=True, color="FFFFFF")
THIN = Side(style="thin")


def fetch(cursor, sql, params):
    cursor.execute(sql, *params)
    columns = [d[0] for d in cursor.description]
    return columns, cursor.fetchall()


def reports_folder(cursor):
    """תיקיית הדוחות מ-tbMainParameters, כמו ביישום — לא נתיב קשיח."""
    cursor.execute("SELECT [Value] FROM tbMainParameters WHERE [Key] = 'ReportsPath'")
    row = cursor.fetchone()
    folder = (row[0] or "").strip() if row else ""
    return folder or os.getcwd()


def write_sheet(sheet, columns, rows, title, subtitle):
    sheet.sheet_view.rightToLeft = True

    sheet.cell(row=1, column=1, value=title).font = Font(bold=True, size=14)
    sheet.cell(row=2, column=1, value=subtitle).font = Font(color="6E6E6E")

    header_row = 4

    for index, name in enumerate(columns, start=1):
        cell = sheet.cell(row=header_row, column=index, value=name)
        cell.fill = HEADER_FILL
        cell.font = HEADER_FONT
        cell.alignment = Alignment(horizontal="center")
        cell.border = Border(top=THIN, bottom=THIN, left=THIN, right=THIN)

    for r, record in enumerate(rows, start=header_row + 1):
        for c, value in enumerate(record, start=1):
            cell = sheet.cell(row=r, column=c, value=value)

            if isinstance(value, datetime.date):
                cell.number_format = "DD/MM/YYYY"
            elif isinstance(value, float):
                cell.number_format = "#,##0.00"

    if rows:
        sheet.auto_filter.ref = (
            f"A{header_row}:"
            f"{get_column_letter(len(columns))}{header_row + len(rows)}")

    sheet.freeze_panes = sheet.cell(row=header_row + 1, column=1)

    # רוחב לפי התוכן, עם תקרה — שם לקוח ארוך היה מייצר עמודה ברוחב מסך.
    for index, name in enumerate(columns, start=1):
        widest = max([len(str(name))] +
                     [len(str(rec[index - 1])) for rec in rows if rec[index - 1] is not None]
                     or [0])
        sheet.column_dimensions[get_column_letter(index)].width = min(widest + 2, 42)


def main():
    if len(sys.argv) < 2:
        print(__doc__)
        sys.exit(1)

    try:
        year = int(sys.argv[1])
    except ValueError:
        print(f"ERROR: bad year: {sys.argv[1]}")
        sys.exit(1)

    output = sys.argv[2] if len(sys.argv) > 2 else None

    conn = pyodbc.connect(CONN_STR)
    try:
        cursor = conn.cursor()

        summary_columns, summary = fetch(cursor, SUMMARY_SQL, (year, year, year))

        if not summary:
            print(f"No citizen holds more than one residential discount code "
                  f"in {year}. Nothing written.")
            return

        detail_columns, detail = fetch(cursor, DETAIL_SQL, (year, year))

        if output is None:
            folder = reports_folder(cursor)
            os.makedirs(folder, exist_ok=True)
            stamp = datetime.datetime.now().strftime("%Y%m%d_%H%M%S")
            output = os.path.join(folder, f"כפל_הנחות_{year}_{stamp}.xlsx")
    finally:
        conn.close()

    stamp_text = datetime.datetime.now().strftime("%d/%m/%Y %H:%M")

    workbook = Workbook()
    workbook.remove(workbook.active)

    clients = len({row[0] for row in summary})

    write_sheet(workbook.create_sheet("סיכום"), summary_columns, summary,
                f"לקוחות עם יותר מקוד הנחה אחד (מגורים) — {year}",
                f"{clients} לקוחות · {len(summary)} שורות קוד · {stamp_text}")

    write_sheet(workbook.create_sheet("פירוט"), detail_columns, detail,
                f"כל שורות ההנחה של אותם לקוחות — {year}",
                f"{len(detail)} שורות · {stamp_text}")

    workbook.save(output)

    print(f"Citizens holding more than one discount code: {clients}")
    print(f"Client/code lines: {len(summary)}")
    print(f"Rows listed: {len(detail)}")
    print(f"Written: {output}")


if __name__ == "__main__":
    main()
