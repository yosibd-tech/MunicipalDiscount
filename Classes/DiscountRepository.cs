using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace MunicipalDiscount.Classes
{
    /// <summary>
    /// כתיבה לשלוש הטבלאות שבהן חיים נתוני ההנחות:
    /// tbClients (הלקוח), tbYearlyDiscounts (שורה אחת ללקוח לנכס לשנה)
    /// ו-tbDiscountRequests (שורה אחת לכל הנחה שניתנה).
    ///
    /// Every importer and every form goes through here, so the rule "one year
    /// row per citizen per property per year, many requests under it" is
    /// written once instead of in each caller. The old table had no such key,
    /// which is how the imports produced 263 duplicate rows.
    /// </summary>
    public static class DiscountRepository
    {
        /// <summary>
        /// מחזיר את מזהה הלקוח, ויוצר אותו אם אינו קיים.
        /// Details already on file are kept when the incoming value is empty —
        /// an import that carries no phone number must not erase one.
        /// </summary>
        public static int EnsureClient(SqlConnection conn, SqlTransaction? tx,
            string clientNumber, string? clientName, string? wifeNumber, string? phone)
            => EnsureClient(conn, tx, clientNumber, clientName, wifeNumber, phone, out _);

        /// <summary>
        /// כמו EnsureClient, ומדווח האם הלקוח נוצר עכשיו — כדי שקליטה תוכל
        /// לדווח כמה לקוחות חדשים הגיעו בקובץ.
        /// </summary>
        public static int EnsureClient(SqlConnection conn, SqlTransaction? tx,
            string clientNumber, string? clientName, string? wifeNumber, string? phone,
            out bool created)
        {
            created = false;
            if (string.IsNullOrWhiteSpace(clientNumber))
                throw new ArgumentException("מספר לקוח ריק", nameof(clientNumber));

            clientNumber = Clip(clientNumber, 20)!;

            using (var find = new SqlCommand(
                "SELECT ID FROM tbClients WHERE ClientNumber = @clientNumber", conn, tx))
            {
                find.Parameters.Add("@clientNumber", SqlDbType.NVarChar, 20).Value = clientNumber;
                object? existing = find.ExecuteScalar();

                if (existing != null && existing != DBNull.Value)
                {
                    int clientId = Convert.ToInt32(existing);

                    using (var update = new SqlCommand(@"
                        UPDATE tbClients
                        SET    ClientName       = COALESCE(@clientName, ClientName),
                               ClieneWifeNumber = COALESCE(@wifeNumber, ClieneWifeNumber),
                               PhoneNumber      = COALESCE(@phone,      PhoneNumber)
                        WHERE  ID = @id", conn, tx))
                    {
                        update.Parameters.Add("@clientName", SqlDbType.NVarChar, 255).Value = Db(Clip(clientName, 255));
                        update.Parameters.Add("@wifeNumber", SqlDbType.NVarChar, 20).Value = Db(Clip(wifeNumber, 20));
                        update.Parameters.Add("@phone", SqlDbType.NVarChar, 50).Value = Db(Clip(phone, 50));
                        update.Parameters.Add("@id", SqlDbType.Int).Value = clientId;
                        update.ExecuteNonQuery();
                    }

                    return clientId;
                }
            }

            using (var insert = new SqlCommand(@"
                INSERT INTO tbClients (ClientNumber, ClientName, ClieneWifeNumber, PhoneNumber)
                OUTPUT INSERTED.ID
                VALUES (@clientNumber, @clientName, @wifeNumber, @phone)", conn, tx))
            {
                insert.Parameters.Add("@clientNumber", SqlDbType.NVarChar, 20).Value = clientNumber;
                insert.Parameters.Add("@clientName", SqlDbType.NVarChar, 255).Value = Db(Clip(clientName, 255));
                insert.Parameters.Add("@wifeNumber", SqlDbType.NVarChar, 20).Value = Db(Clip(wifeNumber, 20));
                insert.Parameters.Add("@phone", SqlDbType.NVarChar, 50).Value = Db(Clip(phone, 50));

                int newId = Convert.ToInt32(insert.ExecuteScalar());
                created = true;
                return newId;
            }
        }

        /// <summary>
        /// קוד סוג הנכס של "מגורים" מתוך tbGAssetKinds — לא מספר קבוע בקוד.
        /// מחזיר null אם אין קטגוריית מגורים בטבלה.
        /// </summary>
        public static int? GetResidentialAssetCode(SqlConnection conn, SqlTransaction? tx = null)
        {
            using var cmd = new SqlCommand(
                "SELECT MIN(Code) FROM tbGAssetKinds WHERE Category = N'מגורים'", conn, tx);

            object? result = cmd.ExecuteScalar();
            return result is null || result == DBNull.Value ? null : Convert.ToInt32(result);
        }

        /// <summary>
        /// מחזיר את מזהה שורת השנה של הלקוח, ויוצר אותה אם אינה קיימת.
        /// The billing figures are not set here — they arrive separately from
        /// the yearly billing file, which knows nothing about citizens.
        /// </summary>
        /// <param name="createIfMissing">
        /// כשהוא false — מחזיר 0 במקום ליצור שורה, והקורא מדלג.
        ///
        /// קליטת ההנחות משתמשת בזה: קובץ החיוב השנתי הוא מרשם הנכסים, ושורה
        /// שנוצרת מתוך קובץ ההנחות נולדת בלי חיוב ובלי שטח. נכס שאינו מגורים
        /// שנוצר כך לא ייגע בקליטת החיוב לעולם — היא קולטת מגורים בלבד —
        /// והעמודות יישארו ריקות לתמיד.
        ///
        /// מסך הוספת לקוח כן יוצר, וזו הכוונה: העובד מוסיף אזרח ביודעין, מזין
        /// את מספר הנכס מתוך שובר הארנונה, ומעביר assetCode של מגורים.
        /// </param>
        public static int EnsureYearRow(SqlConnection conn, SqlTransaction? tx,
            int clientId, int year, string? assetNumber, int? area = null,
            bool createIfMissing = true, int? assetCode = null)
        {
            assetNumber = Clip(assetNumber, 20);

            using (var find = new SqlCommand(@"
                SELECT ID FROM tbYearlyDiscounts
                WHERE  ClientId = @clientId
                  AND  Year_    = @year
                  AND (AssetNumber = @assetNumber
                       OR (AssetNumber IS NULL AND @assetNumber IS NULL))", conn, tx))
            {
                find.Parameters.Add("@clientId", SqlDbType.Int).Value = clientId;
                find.Parameters.Add("@year", SqlDbType.Int).Value = year;
                find.Parameters.Add("@assetNumber", SqlDbType.NVarChar, 20).Value = Db(assetNumber);

                object? existing = find.ExecuteScalar();
                if (existing != null && existing != DBNull.Value)
                {
                    int yearlyId = Convert.ToInt32(existing);

                    // האזור מתעדכן אם הגיע בקובץ, ולעולם אינו נמחק על ידי קובץ
                    // שאינו נושא אותו.
                    if (area.HasValue)
                    {
                        using var setArea = new SqlCommand(
                            "UPDATE tbYearlyDiscounts SET Area = @area WHERE ID = @id", conn, tx);
                        setArea.Parameters.Add("@area", SqlDbType.Int).Value = area.Value;
                        setArea.Parameters.Add("@id", SqlDbType.Int).Value = yearlyId;
                        setArea.ExecuteNonQuery();
                    }

                    return yearlyId;
                }
            }

            if (!createIfMissing)
                return 0;

            using (var insert = new SqlCommand(@"
                INSERT INTO tbYearlyDiscounts (ClientId, Year_, AssetNumber, Area, AssetCode)
                OUTPUT INSERTED.ID
                VALUES (@clientId, @year, @assetNumber, @area, @assetCode)", conn, tx))
            {
                insert.Parameters.Add("@clientId", SqlDbType.Int).Value = clientId;
                insert.Parameters.Add("@year", SqlDbType.Int).Value = year;
                insert.Parameters.Add("@assetNumber", SqlDbType.NVarChar, 20).Value = Db(assetNumber);
                insert.Parameters.Add("@area", SqlDbType.Int).Value =
                    area.HasValue ? (object)area.Value : DBNull.Value;
                insert.Parameters.Add("@assetCode", SqlDbType.Int).Value =
                    assetCode.HasValue ? (object)assetCode.Value : DBNull.Value;

                return Convert.ToInt32(insert.ExecuteScalar());
            }
        }

        /// <summary>
        /// מוסיף בקשת הנחה. כל שורה בקובץ המוניציפלי היא הנחה שכבר ניתנה,
        /// ולכן היא בקשה בפני עצמה — גם כשהיא מגיעה ללא תאריך.
        /// </summary>
        public static int InsertRequest(SqlConnection conn, SqlTransaction? tx,
            int yearlyDiscountId, DateTime? requestDate, int? municipalDiscountCode,
            decimal? actualDiscountAmount, int? userId, string? codeName = null)
        {
            using (var insert = new SqlCommand(@"
                INSERT INTO tbDiscountRequests
                    (YearlyDiscountId, RequestDate, MunicipalDiscountCode,
                     MunicipalDiscountCodeName, ActualDiscountAmount, Status, UserId)
                OUTPUT INSERTED.ID
                VALUES (@yearlyId, @requestDate, @code, @codeName, @amount, 0, @userId)", conn, tx))
            {
                insert.Parameters.Add("@yearlyId", SqlDbType.Int).Value = yearlyDiscountId;
                insert.Parameters.Add("@requestDate", SqlDbType.Date).Value =
                    requestDate.HasValue ? (object)requestDate.Value.Date : DBNull.Value;
                insert.Parameters.Add("@code", SqlDbType.Int).Value =
                    municipalDiscountCode.HasValue && municipalDiscountCode.Value != 0
                        ? (object)municipalDiscountCode.Value : DBNull.Value;
                insert.Parameters.Add("@codeName", SqlDbType.NVarChar, 255).Value =
                    Db(Clip(codeName, 255));
                insert.Parameters.Add("@amount", SqlDbType.Decimal).Value =
                    actualDiscountAmount.HasValue ? (object)actualDiscountAmount.Value : DBNull.Value;
                insert.Parameters.Add("@userId", SqlDbType.Int).Value =
                    userId.HasValue ? (object)userId.Value : DBNull.Value;

                return Convert.ToInt32(insert.ExecuteScalar());
            }
        }

        /// <summary>
        /// שומר החלטת זכאות של עובד על גבי בקשת ההנחה של אותה שנה.
        ///
        /// A citizen's request row normally arrives from the import — the line
        /// says a discount was granted, but not on what date or by which
        /// entitlement. The check fills that same row in: the date it was
        /// decided, the percent it produced, the money that percent works out
        /// to, and the rate version used. Status then turns 1, which is exactly
        /// what it means: an imported row a worker has since ruled on.
        ///
        /// A new row is inserted only when the citizen has no request at all for
        /// that year — a check run on someone the municipality granted nothing.
        ///
        /// ComputedDiscount is stored at decision time so that a later mid-year
        /// rate change cannot make this approval look illegal. The percent is
        /// applied to the year's billing with the code's area cap — the same
        /// formula the audit report computes at run time.
        ///
        /// Returns false when the citizen has no row for that year.
        /// </summary>
        public static bool SaveEligibilityDecision(SqlConnection conn, SqlTransaction? tx,
            string clientNumber, int year, DateTime requestDate, decimal percent, int? userId)
        {
            // An untouched imported request is taken first, then a request that
            // was already ruled on (a correction re-writes it rather than piling
            // up a second decision), and only then the year row on its own.
            using (var save = new SqlCommand(@"
                DECLARE @requestId int, @yearlyId int;

                SELECT TOP 1 @requestId = r.ID, @yearlyId = y.ID
                FROM   tbYearlyDiscounts y
                INNER  JOIN tbClients c ON c.ID = y.ClientId
                LEFT   JOIN tbDiscountRequests r ON r.YearlyDiscountId = y.ID
                WHERE  c.ClientNumber = @clientNumber
                  AND  y.Year_ = @year
                ORDER BY CASE WHEN r.ID IS NULL  THEN 2
                              WHEN r.Status = 0  THEN 0
                              ELSE 1 END,
                         r.ID DESC, y.ID;

                IF @requestId IS NOT NULL
                BEGIN
                    UPDATE r
                    SET    RequestDate             = @requestDate,
                           EntitledDiscountPercent = @percent,
                           ComputedDiscount        =
                               CASE
                                   WHEN y.AnnualBilling IS NULL THEN NULL
                                   WHEN y.AssetSize > ISNULL(rate.MaximumArea, 0) AND ISNULL(rate.MaximumArea, 0) > 0
                                   THEN (y.AnnualBilling / y.AssetSize) * rate.MaximumArea * @percent
                                   ELSE y.AnnualBilling * @percent
                               END,
                           DiscountRateId          = rate.RateId,
                           Status                  = 1,
                           UserId                  = @userId
                    FROM   tbDiscountRequests r
                    INNER  JOIN tbYearlyDiscounts y ON y.ID = r.YearlyDiscountId
                    OUTER  APPLY (
                        SELECT TOP 1 v.RateId, v.MaximumArea
                        FROM   vwDiscountRateAsOf v
                        WHERE  v.MunicipalDiscountCode = r.MunicipalDiscountCode
                          AND  @requestDate >= v.RateFrom
                          AND (v.RateUntil IS NULL OR @requestDate < v.RateUntil)
                        ORDER BY v.RateFrom DESC
                    ) rate
                    WHERE  r.ID = @requestId;
                END
                ELSE IF @yearlyId IS NOT NULL
                BEGIN
                    INSERT INTO tbDiscountRequests
                        (YearlyDiscountId, RequestDate, MunicipalDiscountCode,
                         EntitledDiscountPercent, ComputedDiscount, DiscountRateId, Status, UserId)
                    SELECT y.ID,
                           @requestDate,
                           NULL,
                           @percent,
                           CASE
                               WHEN y.AnnualBilling IS NULL THEN NULL
                               ELSE y.AnnualBilling * @percent
                           END,
                           NULL,
                           1,
                           @userId
                    FROM   tbYearlyDiscounts y
                    WHERE  y.ID = @yearlyId;
                END", conn, tx))
            {
                save.Parameters.Add("@clientNumber", SqlDbType.NVarChar, 20).Value = Clip(clientNumber, 20);
                save.Parameters.Add("@year", SqlDbType.Int).Value = year;
                save.Parameters.Add("@requestDate", SqlDbType.Date).Value = requestDate.Date;
                // Precision/scale are explicit: an inferred scale of 0 would
                // round 0.40 down to 0 and silently store no discount.
                save.Parameters.Add(new SqlParameter("@percent", SqlDbType.Decimal)
                    { Precision = 9, Scale = 4, Value = percent });
                save.Parameters.Add("@userId", SqlDbType.Int).Value =
                    userId.HasValue ? (object)userId.Value : DBNull.Value;

                return save.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// מוחק בקשות שנקלטו מקובץ ואיש לא נגע בהן, לפני קליטה חוזרת של אותה שנה.
        ///
        /// This is what replaced the TRUNCATE. The old import emptied the whole
        /// table because nothing identified a row, so re-running a file would
        /// have doubled every citizen. Now only rows that are purely imported —
        /// no request date, no decided percent, no computed amount, not
        /// approved — are cleared, so re-importing a corrected file cannot
        /// destroy a worker's decision.
        /// </summary>
        // RequestDate is deliberately NOT part of this test.
        //
        // It reads like a sign someone touched the row, but it is written by
        // import_request_dates.py, not by a worker. While it was tested here,
        // any row that import had dated stopped counting as untouched, so the
        // next discounts import could not clear it and inserted a second copy
        // instead - 2026 reached 9,730 rows for 5,009 real requests that way.
        // A worker's decision is Status / EntitledDiscountPercent /
        // ComputedDiscount, and those three still protect it.
        public static int DeleteUntouchedImportedRequests(SqlConnection conn, SqlTransaction? tx, int year)
        {
            using (var delete = new SqlCommand(@"
                DELETE r
                FROM   tbDiscountRequests r
                INNER  JOIN tbYearlyDiscounts y ON y.ID = r.YearlyDiscountId
                WHERE  y.Year_ = @year
                  AND  r.EntitledDiscountPercent IS NULL
                  AND  r.ComputedDiscount        IS NULL
                  AND  r.Status = 0", conn, tx))
            {
                delete.Parameters.Add("@year", SqlDbType.Int).Value = year;
                return delete.ExecuteNonQuery();
            }
        }

        private static string? Clip(string? value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            value = value.Trim();
            return value.Length > maxLength ? value.Substring(0, maxLength) : value;
        }

        private static object Db(string? value) => (object?)value ?? DBNull.Value;
    }
}
