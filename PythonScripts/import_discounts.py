"""
קליטת קובץ הנחות שנה קודמת — tbClients / tbYearlyDiscounts / tbDiscountRequests

Called from MainForm.barButImportPreviouseYearData with the file the user picked:

    python import_discounts.py <file_path> <user_identity_card> <header_row> [--year=NNNN]

The file is the municipality's own export, unchanged: tab-separated, cp1255,
column positions configured in tbDiscountHeaders (1-based):

    Column1 ClientNumber   Column2 ClientName    Column3 AssetNumber
    Column4 ClieneWifeNumber  Column5 DiscountCode  Column6 DiscountName
    Column7 PhoneNumber    Column8 Year_          Column9 DiscountAmount

One column is found by its header text instead: "אזור" ->
tbYearlyDiscounts.Area, an int (the file numbers the areas 1, 2, 3 …). It sits
beside AssetNumber because the area describes the property, not the citizen. tbDiscountHeaders has nine fixed
slots and its form nine dropdowns, so a tenth mapping would mean changing both;
the area column is named the same in every file we have seen. If that stops
being true, add Column10 and map it here.

Every line of the file is a discount the municipality ALREADY GRANTED, so each
line becomes one row in tbDiscountRequests. It arrives without a date —
RequestDate stays NULL and is filled later through the backfill web form.

What changed with the three-table split
---------------------------------------
* No TRUNCATE. It existed only because nothing identified a row, so re-running
  a file would have doubled every citizen. tbYearlyDiscounts now has
  UQ_tbYearlyDiscounts_ClientYearAsset, so the import updates the row it finds
  and inserts only what is new. Re-importing a corrected file no longer wipes
  the table.
* Instead, the import removes the requests IT wrote for that year before
  writing them again — but only the untouched ones (no RequestDate, no
  percent, no computed amount, Status = 0). Anything a worker has decided
  survives a re-import. That is the narrow replacement for the TRUNCATE.
* No join to tbDiscountsCodes. It used to look the percent up by code+year to
  fill PreviousDiscountPercent, but a code no longer has a year — its rate is
  selected by RequestDate, which these rows do not have yet. The percent was
  never a decision anyway, so it is simply not written.
* Column8 is the row's own year now (Year_), not "the previous year".
"""

import sys
import pandas as pd
import pyodbc

# מחרוזת החיבור מגיעה מ-appsettings.json ומפוענחת ב-DPAPI —
# אין סיסמה בקוד. ראה db_config.py.
from db_config import CONN_STR


def main():
    if len(sys.argv) < 3:
        print("Usage: python import_discounts.py <file_path> <user_identity_card> "
              "[header_row] [--year=NNNN]")
        sys.exit(1)

    file_path = sys.argv[1]
    user_identity_card = sys.argv[2]

    # --year=NNNN is the year the operator selected beside the import button.
    # The file still decides which year each row belongs to; this only refuses
    # a file that is not the year he thinks he is loading - the mistake that is
    # waiting to happen every January.
    expected_year = None
    positional = []
    for arg in sys.argv[3:]:
        if arg.startswith('--year='):
            try:
                expected_year = int(arg.split('=', 1)[1])
            except ValueError:
                print(f"ERROR: bad --year value: {arg}")
                sys.exit(1)
        else:
            positional.append(arg)
    header_row = int(positional[0]) if positional else 1

    conn_str = CONN_STR
    conn = pyodbc.connect(conn_str, autocommit=False)
    cursor = conn.cursor()

    # ── 1. Column mappings from tbDiscountHeaders ──
    cursor.execute(
        "SELECT Column1, Column2, Column3, Column4, Column5, Column6, Column7, Column8, Column9 "
        "FROM tbDiscountHeaders")
    row = cursor.fetchone()
    if not row:
        print("ERROR: No header mappings found in tbDiscountHeaders")
        sys.exit(1)

    field_names = [
        'ClientNumber', 'ClientName', 'AssetNumber', 'ClieneWifeNumber',
        'DiscountCode', 'DiscountName', 'PhoneNumber', 'Year_', 'DiscountAmount'
    ]
    col_map = {}
    for i, field in enumerate(field_names):
        excel_col = row[i]
        if excel_col and int(excel_col) > 0:
            col_map[field] = int(excel_col) - 1

    print(f"Column mapping: {col_map}")

    for required in ('ClientNumber', 'Year_'):
        if required not in col_map:
            print(f"ERROR: {required} is not mapped in tbDiscountHeaders")
            sys.exit(1)

    # ── 2. UserId from tbUsers ──
    cursor.execute(
        "SELECT ID FROM tbUsers WHERE IdentityCard = ? AND IsActive = 1",
        user_identity_card)
    user_row = cursor.fetchone()
    if not user_row:
        print(f"ERROR: User not found for IdentityCard={user_identity_card}")
        sys.exit(1)
    user_db_id = user_row[0]
    print(f"UserId: {user_db_id}")

    # ── 3. File → df ──
    df = pd.read_csv(file_path, sep='\t', encoding='windows-1255',
                     skiprows=header_row - 1, header=0, dtype=str, on_bad_lines='skip')
    df.columns = [c.strip() for c in df.columns]
    df = df.dropna(how='all')
    print(f"File -> df: {len(df)} rows")

    # ── 4. Select and rename the mapped columns ──
    rename_map = {}
    for field, col_idx in col_map.items():
        if col_idx < len(df.columns):
            rename_map[df.columns[col_idx]] = field

    # "אזור" is matched on its header text, not on a position in
    # tbDiscountHeaders.
    #
    # Exact match first, and that matters: the file carries "תיאור אזור"
    # right next to "אזור" ("איזור 2"), and a loose match would take
    # whichever came first. The loose pass is only the fallback, for a stray
    # nbsp or a wider heading such as "אזור גביה".
    def clean(name):
        return str(name).replace('\u00a0', ' ').strip()

    area_col = next((c for c in df.columns if clean(c) == 'אזור'), None)
    if area_col is None:
        area_col = next((c for c in df.columns
                         if 'אזור' in clean(c) and 'תיאור' not in clean(c)),
                        None)

    if area_col is not None:
        rename_map[area_col] = 'Area'
        print(f"Area column: {area_col!r}")
    else:
        print("Area column 'אזור' not found in the file — Area stays NULL")

    # "שנה מקורית" is found the same way, and for the same reason: it sits
    # beside "שנה נוכחית", which is the column tbDiscountHeaders already maps
    # as Year_. Exact match only — a loose one would match both.
    orig_col = next((c for c in df.columns if clean(c) == 'שנה מקורית'), None)

    if orig_col is not None:
        rename_map[orig_col] = 'OriginalYear'
        print(f"Original-year column: {orig_col!r}")
    else:
        print("WARNING: 'שנה מקורית' not found — retroactive grants cannot be "
              "filtered out and may be imported as this year's discounts")

    df = df.rename(columns=rename_map)[list(rename_map.values())].copy()

    df['ClientNumber'] = df['ClientNumber'].astype(str).str.strip()
    df = df[df['ClientNumber'].notna() & (df['ClientNumber'] != '') & (df['ClientNumber'] != 'nan')]

    df['Year_'] = pd.to_numeric(df['Year_'], errors='coerce')
    df = df[df['Year_'].notna()]
    df['Year_'] = df['Year_'].astype(int)

    if 'DiscountCode' in df.columns:
        df['DiscountCode'] = pd.to_numeric(df['DiscountCode'], errors='coerce')
    if 'DiscountAmount' in df.columns:
        df['DiscountAmount'] = pd.to_numeric(df['DiscountAmount'], errors='coerce')
    if 'Area' in df.columns:
        df['Area'] = pd.to_numeric(df['Area'], errors='coerce')

    print(f"After column mapping: {len(df)} rows")
    if df.empty:
        print("Nothing to import.")
        sys.exit(1)

    # שנה מקורית שאינה שנת השורה — הנחה רטרואקטיבית, ואינה הנחה של השנה הזו.
    #
    # The municipality grants discounts for earlier years during the current
    # one, and every such line carries the current year in "שנה נוכחית".
    # Imported as they are, one property collects several full-year discounts
    # at once: in the 2026 file, 87 lines for 2019 through 2025, one citizen
    # holding five grants on a single asset totalling 5.4x its annual charge.
    #
    # Compared against the row's own Year_, so a file holding more than one
    # year is judged line by line rather than as a whole.
    if 'OriginalYear' in df.columns:
        original = pd.to_numeric(df['OriginalYear'], errors='coerce')
        retroactive = original.notna() & (original != df['Year_'])

        if retroactive.any():
            dropped = sorted(original[retroactive].astype(int).unique().tolist())
            print(f"Retroactive grants skipped: {int(retroactive.sum())} "
                  f"(original year {', '.join(str(y) for y in dropped)})")
            df = df[~retroactive]

        df = df.drop(columns=['OriginalYear'])

    if df.empty:
        print("Nothing to import after removing retroactive grants.")
        sys.exit(1)

    years = sorted(df['Year_'].unique().tolist())
    print(f"Years in file: {years}")

    if expected_year is not None and years != [expected_year]:
        found = ', '.join(str(y) for y in years)
        print(f"ERROR: the year selected for this import is {expected_year}, "
              f"but the file holds {found}. Nothing was imported. "
              f"Change the year beside the import button, or pick the right file.")
        conn.close()
        sys.exit(1)

    def text(r, field, limit):
        """Trimmed string for a mapped column, or None when absent/blank."""
        if field not in df.columns:
            return None
        value = r.get(field)
        if value is None or pd.isna(value):
            return None
        value = str(value).strip()
        return value[:limit] if value else None

    def number(r, field):
        value = r.get(field)
        return None if value is None or pd.isna(value) else float(value)

    def whole(r, field):
        """int for a numeric column, or None when absent/blank/not a number."""
        if field not in df.columns:
            return None
        value = r.get(field)
        return None if value is None or pd.isna(value) else int(value)

    clients_added = 0
    unplaced = []
    requests_added = 0
    skipped = 0

    try:
        # ── 5. Clear the untouched imported requests for these years ──
        #      so re-importing a corrected file replaces them instead of
        #      adding a second copy. Worker decisions are left alone.
        #
        # RequestDate is deliberately not tested here - it is written by
        # import_request_dates.py, not by a worker, and testing it left
        # every dated row undeletable, so re-importing duplicated it.
        # Status / EntitledDiscountPercent / ComputedDiscount still
        # protect a row a worker has actually decided.
        removed = 0
        for year in years:
            cursor.execute(
                """
                DELETE r
                FROM   tbDiscountRequests r
                INNER  JOIN tbYearlyDiscounts y ON y.ID = r.YearlyDiscountId
                WHERE  y.Year_ = ?
                  AND  r.EntitledDiscountPercent IS NULL
                  AND  r.ComputedDiscount        IS NULL
                  AND  r.Status = 0
                """, year)
            removed += cursor.rowcount
        print(f"Removed {removed} untouched requests from a previous import of {years}")

        # ── 6. One row of the file at a time: client, year row, request ──
        for _, r in df.iterrows():
            client_number = '?'
            try:
                client_number = str(r['ClientNumber']).strip()[:20]
                year = int(r['Year_'])
                asset_number = text(r, 'AssetNumber', 20)

                # the citizen — insert once, then keep the details fresh
                cursor.execute("SELECT ID FROM tbClients WHERE ClientNumber = ?", client_number)
                client_row = cursor.fetchone()
                if client_row:
                    client_id = client_row[0]
                    cursor.execute(
                        """
                        UPDATE tbClients
                        SET    ClientName       = COALESCE(?, ClientName),
                               ClieneWifeNumber = COALESCE(?, ClieneWifeNumber),
                               PhoneNumber      = COALESCE(?, PhoneNumber)
                        WHERE  ID = ?
                        """,
                        text(r, 'ClientName', 255), text(r, 'ClieneWifeNumber', 20),
                        text(r, 'PhoneNumber', 50), client_id)
                else:
                    cursor.execute(
                        "INSERT INTO tbClients (ClientNumber, ClientName, ClieneWifeNumber, PhoneNumber) "
                        "OUTPUT INSERTED.ID VALUES (?, ?, ?, ?)",
                        client_number, text(r, 'ClientName', 255),
                        text(r, 'ClieneWifeNumber', 20), text(r, 'PhoneNumber', 50))
                    client_id = cursor.fetchone()[0]
                    clients_added += 1

                # The year row must already exist. The billing import runs
                # FIRST and is the property register: it is the only import that
                # sets AssetCode, and AssetCode is what puts a row in scope
                # (tbGAssetKinds.Category = 'מגורים'). This import therefore
                # attaches grants to properties that are already there and
                # creates none of its own.
                #
                # It used to insert a row here with ClientId / Year_ /
                # AssetNumber / Area and nothing else. Two things went wrong with
                # that. The rows carried no billing, no size and no code, so they
                # were inert for every calculation - that is exactly the shape of
                # the 2025 data, 4,634 rows with zero billing. And once billing
                # was restricted to מגורים, a grant on a shop would create a row
                # the billing import can never reach, so its columns would stay
                # NULL for good.
                #
                # A grant that cannot be placed is reported instead. It means the
                # property is not residential, or the billing file has not been
                # imported for this year yet.
                cursor.execute(
                    "SELECT ID FROM tbYearlyDiscounts "
                    "WHERE ClientId = ? AND Year_ = ? "
                    "  AND (AssetNumber = ? OR (AssetNumber IS NULL AND ? IS NULL))",
                    client_id, year, asset_number, asset_number)
                year_row = cursor.fetchone()
                if not year_row:
                    unplaced.append((client_number, asset_number))
                    continue

                yearly_id = year_row[0]
                # the area can be corrected in a later file, but never blanked
                cursor.execute(
                    "UPDATE tbYearlyDiscounts SET Area = COALESCE(?, Area) WHERE ID = ?",
                    whole(r, 'Area'), yearly_id)

                # the grant itself — one request per line of the file
                # MunicipalDiscountCodeName is the description the file gave
                # the code, and it is not decoration: MissingDiscountCodesReport
                # reads MIN(MunicipalDiscountCodeName) to say WHICH discount an
                # unregistered number is, and that text becomes DiscountName on
                # the row the import offers to create in tbDiscountsCodes. Left
                # out here, a code first seen through this file would be
                # reported - and registered - as a bare number.
                # Column6 of tbDiscountHeaders, already mapped as DiscountName.
                discount_code = number(r, 'DiscountCode')
                cursor.execute(
                    """
                    INSERT INTO tbDiscountRequests
                        (YearlyDiscountId, RequestDate, MunicipalDiscountCode,
                         MunicipalDiscountCodeName, ActualDiscountAmount,
                         Status, UserId)
                    VALUES (?, NULL, ?, ?, ?, 0, ?)
                    """,
                    yearly_id,
                    int(discount_code) if discount_code else None,
                    text(r, 'DiscountName', 255),
                    number(r, 'DiscountAmount'),
                    user_db_id)
                requests_added += 1

            except Exception as e:
                print(f"ERROR on client {client_number}: {e}")
                skipped += 1

        # All or nothing: a half-loaded year is worse than none, and re-running
        # a corrected file is now cheap — the import replaces what it wrote.
        if skipped:
            raise RuntimeError(f"{skipped} rows failed — nothing was written")

        conn.commit()
        print(f"Import complete: {clients_added} new citizens "
              f"(citizens already on file were kept and their details refreshed), "
              f"{requests_added} requests.")

        # Grants with no property to hang on. Named rather than counted: each one
        # is either a non-residential property - correctly out of scope - or a
        # sign the billing file for this year has not been imported yet, and the
        # two need telling apart.
        if unplaced:
            print(f"NOT IMPORTED: {len(unplaced)} grants have no matching property "
                  f"for {years}. The billing import creates the properties and must "
                  f"run first; a grant still missing afterwards is on a "
                  f"non-residential property and is out of scope.")
            for client_number, asset_number in unplaced[:20]:
                print(f"   client {client_number}, asset {asset_number}")
            if len(unplaced) > 20:
                print(f"   ... and {len(unplaced) - 20} more")

    except Exception as e:
        conn.rollback()
        print(f"ERROR: import rolled back — {e}")
        conn.close()
        sys.exit(1)

    # ── 7. Validate against what is now in the database ──
    placeholders = ','.join('?' * len(years))
    cursor.execute(
        f"""
        SELECT COUNT(*)
        FROM   tbDiscountRequests r
        INNER  JOIN tbYearlyDiscounts y ON y.ID = r.YearlyDiscountId
        WHERE  y.Year_ IN ({placeholders})
        """, *years)
    db_count = cursor.fetchone()[0]
    conn.close()

    print(f"Validation: {db_count} requests in the database for {years} "
          f"({requests_added} written by this import)")

    if db_count < requests_added:
        print("WARNING: fewer rows in the database than were written!")
        sys.exit(1)

    print("Validation passed!")


if __name__ == "__main__":
    main()
