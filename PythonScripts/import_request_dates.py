"""
עידכון תאריך בקשת הנחה מקובץ התנועות — tbDiscountRequests.RequestDate

Called from MainForm.barButImportRequestDates with the file the user picked:

    python import_request_dates.py <file_path> <year> [--force] [--dry-run]

The file is the municipality's movements export ("כל התנועות"), a real .xlsx —
not the tab-separated report 203 file that import_discounts.py reads. Report 203
carries no date at all, which is why RequestDate is inserted NULL there and
filled here.

Where the date comes from
-------------------------
A movement row carries a date for a discount request when its client, asset,
year and פעולה (the transaction action code) match a request already
recorded in tbDiscountRequests. For those rows the date is תאריך גביה.

The action code is deliberately NOT checked against tbDiscountsCodes first.
That table is a classification list with nothing keeping it complete, and
filtering on it silently discarded every movement whose code nobody had
registered yet - 280 of 2026's requests were left dateless that way by codes
370, 435, 505, 543, 567 and 581. The join below is the real gate and a
stricter one: a movement can only ever date a request that already exists,
so a code that is not a discount cannot match anything. Codes missing from
tbDiscountsCodes are now reported instead of silently changing the outcome.

The other two date columns are deliberately not used: תאריך עדכון is the clerk's
booking timestamp, and יום ערך is a billing value date that runs into the future.

A discount is not one movement. It is booked once per תקופה, and a correction
re-books every period, so one grant can carry several תאריך גביה values weeks
apart. The grant date is the FIRST of them — MIN(תאריך גביה) — because that is
the date the entitlement began, and therefore the date that selects the
tbDiscountRates version.

Matching into the database
--------------------------
    מספר לקוח -> tbClients.ClientNumber
    מספר נכס  -> tbYearlyDiscounts.AssetNumber
    שנה       -> tbYearlyDiscounts.Year_        (the year argument, not שנה מקורית)
    פעולה     -> tbDiscountRequests.MunicipalDiscountCode

Only requests whose RequestDate is still NULL are filled. A date a worker
already entered through RequestDateForm survives the import; --force overwrites
it. --dry-run reports every number below and writes nothing.
"""

import sys

import openpyxl
import pyodbc

# מחרוזת החיבור מגיעה מ-appsettings.json ומפוענחת ב-DPAPI —
# אין סיסמה בקוד. ראה db_config.py.
from db_config import CONN_STR

# The messages travel back to MainForm through stdout and are shown in a
# message box, so they must survive the console's own code page.
sys.stdout.reconfigure(encoding='utf-8')
sys.stderr.reconfigure(encoding='utf-8')


CONN_STR = CONN_STR
# The five columns the import needs, by their header text in the export.
COL_CLIENT = 'מספר לקוח'
COL_ASSET = 'מספר נכס'
COL_ACTION = 'פעולה'
COL_YEAR = 'שנה'
COL_DATE = 'תאריך גביה'


def cell_text(value):
    """A cell as the database stores it: text, no trailing .0, no padding."""
    if value is None:
        return None
    if isinstance(value, float) and value.is_integer():
        value = int(value)
    text = str(value).strip()
    return text or None


def find_sheet(workbook):
    """The movements sheet is the one whose header row has פעולה and תאריך גביה.

    The export names it after its run time (RM_ENTRIES30_08_051324), so it
    cannot be found by name, and the workbook also holds pivot sheets.
    """
    for name in workbook.sheetnames:
        sheet = workbook[name]
        for row in sheet.iter_rows(min_row=1, max_row=1, values_only=True):
            headers = [(h or '').strip() for h in row]
            if COL_ACTION in headers and COL_DATE in headers:
                return name, sheet, headers
        # a read-only sheet is a one-shot iterator; re-open it to read again
        workbook[name]
    return None, None, None


def main():
    args = [a for a in sys.argv[1:] if not a.startswith('--')]
    flags = {a for a in sys.argv[1:] if a.startswith('--')}
    if len(args) < 2:
        print("Usage: python import_request_dates.py <file_path> <year> [--force] [--dry-run]")
        sys.exit(1)

    file_path = args[0]
    try:
        year = int(args[1])
    except ValueError:
        print(f"ERROR: '{args[1]}' is not a year")
        sys.exit(1)
    force = '--force' in flags
    dry_run = '--dry-run' in flags

    conn = pyodbc.connect(CONN_STR, autocommit=False)
    cursor = conn.cursor()

    # ── 1. The classification list — for reporting only, see the docstring ──
    cursor.execute(
        "SELECT DISTINCT MunicipalDiscountCode FROM tbDiscountsCodes "
        "WHERE MunicipalDiscountCode IS NOT NULL")
    codes = {int(r[0]) for r in cursor.fetchall()}
    if not codes:
        print("WARNING: tbDiscountsCodes holds no municipal discount codes")
    print(f"Codes registered in tbDiscountsCodes: {len(codes)}")

    # ── 2. The movements sheet ──
    workbook = openpyxl.load_workbook(file_path, read_only=True, data_only=True)
    sheet_name, sheet, headers = find_sheet(workbook)
    if sheet is None:
        print(f"ERROR: no sheet in the file has both '{COL_ACTION}' and '{COL_DATE}' columns")
        sys.exit(1)

    try:
        idx = {name: headers.index(name)
               for name in (COL_CLIENT, COL_ASSET, COL_ACTION, COL_YEAR, COL_DATE)}
    except ValueError as e:
        print(f"ERROR: a required column is missing from sheet '{sheet_name}' — {e}")
        sys.exit(1)
    print(f"Sheet '{sheet_name}', columns {idx}")

    # ── 3. Dated movements -> MIN(תאריך גביה) per client + asset + code ──
    rows_read = 0
    no_action_code = 0
    other_year = 0
    no_date = 0
    first_date = {}

    for row in sheet.iter_rows(min_row=2, values_only=True):
        if not any(v is not None for v in row):
            continue
        rows_read += 1

        action = cell_text(row[idx[COL_ACTION]])
        if action is None or not action.isdigit():
            no_action_code += 1
            continue
        action_code = int(action)

        row_year = cell_text(row[idx[COL_YEAR]])
        if row_year is None or not row_year.isdigit() or int(row_year) != year:
            other_year += 1
            continue

        collected = row[idx[COL_DATE]]
        if collected is None:
            no_date += 1
            continue
        collected = collected.date() if hasattr(collected, 'date') else collected

        client = cell_text(row[idx[COL_CLIENT]])
        asset = cell_text(row[idx[COL_ASSET]])
        if client is None or asset is None:
            no_date += 1
            continue

        key = (client, asset, action_code)
        if key not in first_date or collected < first_date[key]:
            first_date[key] = collected

    workbook.close()
    print(f"File: {rows_read} rows — {no_action_code} with no numeric action code, "
          f"{other_year} of another year, {no_date} unusable")
    print(f"Dated movements for {year}: {len(first_date)} (client + asset + code), "
          f"dates {min(first_date.values())} .. {max(first_date.values())}"
          if first_date else f"Dated movements for {year}: none")

    if not first_date:
        print(f"Nothing to update — the file holds no discount movements for {year}")
        conn.close()
        return

    # ── 4. Into a temp table, then one set-based update ──
    try:
        cursor.execute("""
            CREATE TABLE #RequestDates (
                ClientNumber  NVARCHAR(20),
                AssetNumber   NVARCHAR(20),
                DiscountCode  INT,
                RequestDate   DATE)
        """)
        cursor.fast_executemany = True
        cursor.executemany(
            "INSERT INTO #RequestDates (ClientNumber, AssetNumber, DiscountCode, RequestDate) "
            "VALUES (?, ?, ?, ?)",
            [(c, a, code, d) for (c, a, code), d in first_date.items()])

        # What the file reaches, before anything is written
        cursor.execute("""
            SELECT COUNT(*)                                              AS matched,
                   SUM(CASE WHEN r.RequestDate IS NULL THEN 1 ELSE 0 END) AS to_fill
            FROM   #RequestDates t
            INNER  JOIN tbClients          c ON c.ClientNumber     = t.ClientNumber
            INNER  JOIN tbYearlyDiscounts  y ON y.ClientId         = c.ID
                                            AND y.AssetNumber      = t.AssetNumber
                                            AND y.Year_            = ?
            INNER  JOIN tbDiscountRequests r ON r.YearlyDiscountId = y.ID
                                            AND r.MunicipalDiscountCode = t.DiscountCode
        """, year)
        matched, to_fill = cursor.fetchone()
        matched = matched or 0
        to_fill = to_fill or 0
        print(f"Matched to a discount request: {matched} "
              f"(the other {len(first_date) - matched} dated movements are not requests)")
        print(f"Of those, {to_fill} have no date yet and "
              f"{matched - to_fill} already have one"
              + (" — --force will overwrite them" if force else " — left as they are"))

        cursor.execute("""
            UPDATE r
            SET    r.RequestDate = t.RequestDate
            FROM   tbDiscountRequests r
            INNER  JOIN tbYearlyDiscounts y ON y.ID              = r.YearlyDiscountId
            INNER  JOIN tbClients         c ON c.ID              = y.ClientId
            INNER  JOIN #RequestDates     t ON t.ClientNumber    = c.ClientNumber
                                           AND t.AssetNumber     = y.AssetNumber
                                           AND t.DiscountCode    = r.MunicipalDiscountCode
            WHERE  y.Year_ = ?
              AND (r.RequestDate IS NULL OR ? = 1)
        """, year, 1 if force else 0)
        updated = cursor.rowcount

        # ── 5. What is left, and why ──
        #
        # A bare count of leftovers is what sent someone exporting them to
        # Excel by hand to find out what they had in common. Each one is
        # named here instead: whether the movements file even reached it,
        # and whether its code is registered.
        cursor.execute("""
            SELECT r.MunicipalDiscountCode, c.ClientNumber, y.AssetNumber
            FROM   tbDiscountRequests r
            INNER  JOIN tbYearlyDiscounts y ON y.ID = r.YearlyDiscountId
            INNER  JOIN tbClients        c ON c.ID = y.ClientId
            WHERE  y.Year_ = ? AND r.RequestDate IS NULL
        """, year)
        leftovers = cursor.fetchall()

        cursor.execute("""
            SELECT COUNT(*),
                   SUM(CASE WHEN r.RequestDate IS NULL THEN 1 ELSE 0 END)
            FROM   tbDiscountRequests r
            INNER  JOIN tbYearlyDiscounts y ON y.ID = r.YearlyDiscountId
            WHERE  y.Year_ = ?
        """, year)
        total, still_null = cursor.fetchone()

        if dry_run:
            conn.rollback()
            print(f"DRY RUN — would have updated {updated} requests; nothing was written")
        else:
            conn.commit()
            print(f"Updated {updated} requests with a RequestDate")
        print(f"{year}: {total} requests, {still_null} still without a date")

        # Grouped by code: in the file but undated is an anomaly worth
        # chasing; absent from the file just means the export did not cover
        # it (a code below the export's own range, or a later movement).
        if leftovers:
            by_code = {}
            for code, client, asset in leftovers:
                seen, absent = by_code.setdefault(int(code), [0, 0])
                if (client, asset, int(code)) in first_date:
                    by_code[int(code)][0] = seen + 1
                else:
                    by_code[int(code)][1] = absent + 1
            print("Still without a date, by code:")
            for code in sorted(by_code, key=lambda c: -sum(by_code[c])):
                seen, absent = by_code[code]
                note = "" if code in codes else "  [code not in tbDiscountsCodes]"
                parts = []
                if seen:
                    parts.append(f"{seen} matched a dated movement")
                if absent:
                    parts.append(f"{absent} not in the movements file")
                print(f"   code {code}: " + ", ".join(parts) + note)

        # Not fatal any more, but it means categories and reports built on
        # tbDiscountsCodes do not know these codes.
        # Codes that actually back a request for this year but are absent
        # from the classification list. Not fatal to the dates any more, but
        # categories and reports built on tbDiscountsCodes do not know them.
        cursor.execute("""
            SELECT DISTINCT r.MunicipalDiscountCode
            FROM   tbDiscountRequests r
            INNER  JOIN tbYearlyDiscounts y ON y.ID = r.YearlyDiscountId
            WHERE  y.Year_ = ?
              AND  r.MunicipalDiscountCode NOT IN (
                       SELECT MunicipalDiscountCode FROM tbDiscountsCodes
                       WHERE  MunicipalDiscountCode IS NOT NULL)
            ORDER  BY r.MunicipalDiscountCode
        """, year)
        missing = [int(r[0]) for r in cursor.fetchall()]
        if missing:
            listed = ", ".join(str(c) for c in missing)
            print(f"Note: {len(missing)} code(s) used by {year} requests are missing "
                  f"from tbDiscountsCodes: {listed}")
            print("      Dates were imported for them anyway; add them so the "
                  "classification is complete.")

    except Exception as e:
        conn.rollback()
        print(f"ERROR: update rolled back — {e}")
        conn.close()
        sys.exit(1)

    conn.close()


if __name__ == '__main__':
    main()
