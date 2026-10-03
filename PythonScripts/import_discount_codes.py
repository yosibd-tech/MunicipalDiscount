"""
Loads the two rebuilt workbooks into tbDiscountsCodes + tbDiscountCodeRates.

    python import_discount_codes.py <codes.xlsx> <rates.xlsx> --dry-run   validate only
    python import_discount_codes.py <codes.xlsx> <rates.xlsx> --replace   load for real

    codes.xlsx  -> tbDiscountsCodes    one row per discount code
    rates.xlsx  -> tbDiscountCodeRates     one row per dated rate version

A rate line finds its code through קוד עירוני + קוד חוקי — the same pair that
identifies the row in the codes file. The database ids are assigned here, so
neither workbook contains one.

Nothing is written unless --replace is given, and --replace empties both tables
first: the workbooks are the complete picture, not a delta. It all happens in one
transaction, so a failure partway through leaves the database untouched.
"""

import sys
from datetime import date, datetime

import pyodbc
from openpyxl import load_workbook

# מחרוזת החיבור מגיעה מ-appsettings.json ומפוענחת ב-DPAPI —
# אין סיסמה בקוד. ראה db_config.py.
from db_config import CONN_STR

CONN_STR = CONN_STR
# Hebrew header -> field. Must match export_discount_codes.py.
CODE_HEADERS = {
    "קוד עירוני":      "MunicipalDiscountCode",
    "קוד חוקי":        "LegallDiscountCode",
    "קטגוריה":         "Categories",
    "קוד קבוצה":       "GroupCode",
    "שם הנחה":         "DiscountName",
    "חובה":            "Mandatory",
    "חוק בתוקף":       "LawInForce",
    "מסמכים נדרשים":   "Documents",
    "הערה":            "Remark",
    "כללים":           "Rules",
    "תנאים":           "Conditions",
}

RATE_HEADERS = {
    "קוד עירוני":       "MunicipalDiscountCode",
    "קוד חוקי":         "LegallDiscountCode",
    # Written by the export for the reader's benefit; mapped to nothing so it
    # is skipped silently instead of being reported as an unknown column.
    "שם הנחה":          None,
    "תאריך תחילת תוקף": "ValidityDate",
    "אחוז הנחה":        "DiscountPercent",
    "אחוז הנחה קודם":   "PreviousDiscountPercent",
    "מקסימום הנחה":     "MaximumDiscount",
    "מקסימום שטח":      "MaximumArea",
}

TEXT_FIELDS = ["Categories", "DiscountName", "Mandatory", "LawInForce",
               "Documents", "Remark", "Rules", "Conditions"]

PERCENT_FIELDS = ["DiscountPercent", "PreviousDiscountPercent", "MaximumDiscount"]
NUMBER_FIELDS = PERCENT_FIELDS + ["MaximumArea"]

TEXT_LIMIT = 255          # every nvarchar column in tbDiscountsCodes


class SheetError(Exception):
    """A problem the user has to fix in Excel."""


def read_sheet(path, headers, required, label):
    """Workbook -> list of dicts, one per non-empty line, with the Excel row number."""
    book = load_workbook(path, data_only=True)
    sheet = book.worksheets[0]

    header_cells = next(sheet.iter_rows(min_row=1, max_row=1, values_only=True))
    found = [str(h).strip() if h is not None else "" for h in header_cells]

    missing = [h for h in required if h not in found]
    if missing:
        raise SheetError(f"{label}: חסרות עמודות חובה — {', '.join(missing)}")

    unknown = [h for h in found if h and h not in headers]
    if unknown:
        print(f"WARNING: {label}: ignoring unrecognised columns: {', '.join(unknown)}")

    rows = []
    for excel_row, values in enumerate(sheet.iter_rows(min_row=2, values_only=True), start=2):
        if all(v is None or str(v).strip() == "" for v in values):
            continue

        record = {"_row": excel_row}
        for header, value in zip(found, values):
            field = headers.get(header)
            if field:
                record[field] = value
        rows.append(record)

    print(f"{label}: {len(rows)} data rows")
    return rows


def as_int(value, field, excel_row, label):
    if value is None or str(value).strip() == "":
        return None
    try:
        return int(float(str(value).strip()))
    except ValueError:
        raise SheetError(f"{label} שורה {excel_row}: '{value}' בעמודה {field} אינו מספר")


def as_date(value, excel_row, label):
    if value is None or str(value).strip() == "":
        raise SheetError(f"{label} שורה {excel_row}: חסר תאריך תחילת תוקף")

    if isinstance(value, datetime):
        return value.date()
    if isinstance(value, date):
        return value

    text = str(value).strip()
    for pattern in ("%d/%m/%Y", "%Y-%m-%d", "%d.%m.%Y", "%d/%m/%y"):
        try:
            return datetime.strptime(text, pattern).date()
        except ValueError:
            continue
    raise SheetError(f"{label} שורה {excel_row}: '{value}' אינו תאריך תקין (dd/mm/yyyy)")


def as_number(value, field, excel_row, label, warnings):
    if value is None or str(value).strip() == "":
        return None

    text = str(value).strip().replace("%", "")
    try:
        number = float(text)
    except ValueError:
        raise SheetError(f"{label} שורה {excel_row}: '{value}' בעמודה {field} אינו מספר")

    # Percent columns are fractions in the database (0.40 = 40%). A cell that was
    # never formatted as percent arrives as 40 rather than 0.4 — accept it, but
    # say so, because both readings are plausible.
    if field in PERCENT_FIELDS and number > 1:
        if number <= 100:
            warnings.append(
                f"{label} שורה {excel_row}: {field}={number:g} התקבל כ-{number / 100:.0%}")
            number = number / 100
        else:
            raise SheetError(f"{label} שורה {excel_row}: {field}={number:g} גדול מ-100%")

    return number


def as_text(value, field, excel_row, label):
    if value is None:
        return None
    text = str(value).strip()
    if text == "":
        return None
    if len(text) > TEXT_LIMIT:
        raise SheetError(
            f"{label} שורה {excel_row}: העמודה {field} ארוכה מ-{TEXT_LIMIT} תווים ({len(text)})")
    return text


def code_key(record, excel_row, label):
    """The one number that identifies a code in both workbooks.

    MunicipalDiscountCode where the municipality has adopted the discount,
    LegallDiscountCode where it has not - those rows carry no municipal
    number and exist only to hold the law in the catalogue.

    Deliberately NOT the pair of them. Carrying the workbooks to another
    municipality means renumbering the municipal column, and a pair key
    would have to be kept in step across two columns in two sheets; one
    column in two sheets is half the editing and half the ways to get it
    wrong. The legal number is the catalogue's own and never changes.
    """
    municipal = as_int(record.get("MunicipalDiscountCode"), "קוד עירוני", excel_row, label)
    legal = as_int(record.get("LegallDiscountCode"), "קוד חוקי", excel_row, label)

    if municipal is None and legal is None:
        raise SheetError(
            f"{label} שורה {excel_row}: חייב להופיע קוד עירוני או קוד חוקי (לפחות אחד)")

    return municipal if municipal is not None else legal


def describe(key):
    """The number as the operator sees it in the sheet."""
    return str(key)


def parse_codes(rows, label):
    codes = {}

    for record in rows:
        excel_row = record["_row"]
        key = code_key(record, excel_row, label)

        if key in codes:
            raise SheetError(
                f"{label}: הקוד {describe(key)} מופיע פעמיים "
                f"(שורות {codes[key]['_row']} ו-{excel_row})")

        entry = {
            "_row": excel_row,
            "MunicipalDiscountCode": as_int(record.get("MunicipalDiscountCode"), "קוד עירוני", excel_row, label),
            "LegallDiscountCode": as_int(record.get("LegallDiscountCode"), "קוד חוקי", excel_row, label),
            "GroupCode": as_int(record.get("GroupCode"), "קוד קבוצה", excel_row, label),
        }
        for field in TEXT_FIELDS:
            entry[field] = as_text(record.get(field), field, excel_row, label)

        codes[key] = entry

    return codes


def parse_rates(rows, codes, label, warnings):
    rates = {}

    for record in rows:
        excel_row = record["_row"]
        key = code_key(record, excel_row, label)

        if key not in codes:
            raise SheetError(
                f"{label} שורה {excel_row}: הקוד {describe(key)} אינו קיים בקובץ קודי ההנחה")

        when = as_date(record.get("ValidityDate"), excel_row, label)
        versions = rates.setdefault(key, {})

        if when in versions:
            raise SheetError(
                f"{label}: לקוד {describe(key)} שתי שורות עם התאריך {when:%d/%m/%Y} "
                f"(שורות {versions[when]['_row']} ו-{excel_row})")

        entry = {"_row": excel_row, "ValidityDate": when}
        for field in NUMBER_FIELDS:
            entry[field] = as_number(record.get(field), field, excel_row, label, warnings)

        versions[when] = entry

    orphans = [describe(key) for key in codes if key not in rates]
    if orphans:
        warnings.append(
            f"{len(orphans)} קודים ללא אף שורת תעריף ולא יימצאו בחישוב: "
            f"{', '.join(orphans[:10])}{' ...' if len(orphans) > 10 else ''}")

    return rates


def load(codes, rates, replace):
    conn = pyodbc.connect(CONN_STR, autocommit=False)
    cursor = conn.cursor()

    try:
        cursor.execute("SELECT COUNT(*) FROM tbDiscountsCodes")
        existing = cursor.fetchone()[0]

        if existing and not replace:
            raise SheetError(
                f"tbDiscountsCodes מכיל כבר {existing} שורות. "
                "יש להריץ עם --replace כדי להחליף אותן.")

        if replace and existing:
            # The FK cascades, so the rate versions go with their codes.
            cursor.execute("DELETE FROM tbDiscountsCodes")
            print(f"Cleared {existing} existing code(s).")

        code_count = 0
        rate_count = 0

        for key in sorted(codes):
            entry = codes[key]

            cursor.execute(
                "INSERT INTO tbDiscountsCodes "
                "(MunicipalDiscountCode, LegallDiscountCode, Categories, GroupCode, "
                " DiscountName, Mandatory, LawInForce, Documents, Remark, Rules, Conditions) "
                "OUTPUT INSERTED.ID "
                "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                entry["MunicipalDiscountCode"], entry["LegallDiscountCode"],
                entry["Categories"], entry["GroupCode"], entry["DiscountName"],
                entry["Mandatory"], entry["LawInForce"], entry["Documents"],
                entry["Remark"], entry["Rules"], entry["Conditions"])

            code_id = cursor.fetchone()[0]
            code_count += 1

            for when in sorted(rates.get(key, {})):
                version = rates[key][when]
                cursor.execute(
                    "INSERT INTO tbDiscountCodeRates "
                    "(DiscountCodeId, ValidityDate, DiscountPercent, "
                    " PreviousDiscountPercent, MaximumDiscount, MaximumArea) "
                    "VALUES (?, ?, ?, ?, ?, ?)",
                    code_id, when, version["DiscountPercent"],
                    version["PreviousDiscountPercent"], version["MaximumDiscount"],
                    version["MaximumArea"])
                rate_count += 1

        conn.commit()
        print(f"Loaded {code_count} code(s), {rate_count} rate version(s).")

    except Exception:
        conn.rollback()
        raise
    finally:
        conn.close()


def main():
    if len(sys.argv) < 3:
        print(__doc__)
        sys.exit(1)

    codes_path, rates_path = sys.argv[1], sys.argv[2]
    flags = sys.argv[3:]
    dry_run = "--dry-run" in flags
    replace = "--replace" in flags

    if not dry_run and not replace:
        print("ERROR: pass --dry-run to validate, or --replace to load.")
        sys.exit(1)

    warnings = []
    try:
        code_rows = read_sheet(codes_path, CODE_HEADERS,
                               ("קוד עירוני", "קוד חוקי"), "קודי הנחה")
        rate_rows = read_sheet(rates_path, RATE_HEADERS,
                               ("קוד עירוני", "קוד חוקי", "תאריך תחילת תוקף"), "תעריפי הנחה")

        codes = parse_codes(code_rows, "קודי הנחה")
        rates = parse_rates(rate_rows, codes, "תעריפי הנחה", warnings)
    except SheetError as error:
        print(f"ERROR: {error}")
        sys.exit(1)

    for warning in warnings:
        print(f"WARNING: {warning}")

    total_versions = sum(len(v) for v in rates.values())
    print(f"Parsed {len(codes)} code(s), {total_versions} rate version(s).")

    versioned = {key: v for key, v in rates.items() if len(v) > 1}
    if versioned:
        print(f"{len(versioned)} code(s) change rate over time:")
        for key in sorted(versioned):
            history = " -> ".join(
                f"{when:%d/%m/%Y}=" + (
                    f"{versioned[key][when]['DiscountPercent']:.0%}"
                    if versioned[key][when]["DiscountPercent"] is not None else "?")
                for when in sorted(versioned[key]))
            print(f"  {describe(key)}: {history}")

    if dry_run:
        print("Dry run — nothing was written.")
        return

    try:
        load(codes, rates, replace)
    except SheetError as error:
        print(f"ERROR: {error}")
        sys.exit(1)


if __name__ == "__main__":
    main()
