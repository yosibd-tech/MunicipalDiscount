# תרגום מחרוזות DevExpress לעברית — איך עושים את זה

כשמסך מציג טקסט באנגלית (למשל חלון הסינון של עמודה: `Numeric Filters`,
`Values`, `Equals`, `Close`), זו מחרוזת פנימית של DevExpress ולא טקסט שכתבנו.
המסמך הזה מסביר איך למצוא מי אחראי עליה ואיך לתרגם.

---

## 1. הרעיון: אין מתרגם אחד, יש הרבה

DevExpress מחלקת את המחרוזות בין **מתרגמים נפרדים לפי מודול**. כל אחד הוא מחלקה
עם `GetLocalizedString(enum id)` ותכונה סטטית `Active`. **תרגום של אחד לא משפיע
על האחרים** — וזו בדיוק הסיבה שהרשת הייתה בעברית וחלון הסינון נשאר באנגלית.

| מה מוצג | המתרגם | ה-enum |
|---|---|---|
| הרשת: כותרת קיבוץ, שורה חדשה, תפריט העמודה | `DevExpress.XtraGrid.Localization.GridLocalizer` | `GridStringId` |
| **חלון הסינון של העמודה (סגנון אקסל)** | `DevExpress.Utils.Filtering.Internal.FilterUIElementLocalizer` | `FilterUIElementLocalizerStringId` |
| עורכים: תפריטי טקסט, לוח שנה, כפתורים | `DevExpress.XtraEditors.Controls.Localizer` | `StringId` |
| סרגלים ותפריטים (Bars/Ribbon) | `DevExpress.XtraBars.Localization.BarLocalizer` | `BarString` |
| תצוגה מקדימה והדפסה | `DevExpress.XtraPrinting.Localization.PreviewLocalizer` | `PreviewStringId` |
| עורך הסינון (Filter Editor) | `DevExpress.XtraEditors.Filtering.FilterUILocalizer` | — |

בפרויקט הזה שני הראשונים כבר מתורגמים ב-`Classes/HebrewLocalizers.cs`.

---

## 2. איך מוצאים למי שייכת מחרוזת מסוימת

### הדרך הבטוחה — לשאול את ה-DLL עצמו

פרויקט קונסולה קטן, חד-פעמי, שסורק את כל ה-assemblies של DevExpress ומחפש את
הטקסט האנגלי שראית על המסך:

```bash
mkdir probe && cd probe
dotnet new console
```

`probe.csproj` — אותה גרסה שהאפליקציה משתמשת בה:

```xml
<PropertyGroup>
  <TargetFramework>net8.0-windows</TargetFramework>
  <UseWindowsForms>true</UseWindowsForms>
</PropertyGroup>
<ItemGroup>
  <PackageReference Include="DevExpress.Win.Grid" Version="25.2.3" />
</ItemGroup>
```

`Program.cs`:

```csharp
using System; using System.IO; using System.Linq;
using System.Reflection; using System.Text;

class P {
    static void Main() {
        // נגיעה בפקד טוענת את כל ה-assemblies שהוא צריך
        var g = new DevExpress.XtraGrid.GridControl();
        var v = new DevExpress.XtraGrid.Views.Grid.GridView(g);

        // כאן מכניסים את הטקסט האנגלי שרואים על המסך
        var wanted = new[] { "Numeric Filters", "Values", "Equals", "Close" };
        var sb = new StringBuilder();

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()
                     .Where(a => a.FullName.StartsWith("DevExpress"))) {
            Type[] types;
            try { types = asm.GetTypes(); } catch { continue; }

            foreach (var t in types.Where(t => !t.IsAbstract && t.Name.EndsWith("Localizer"))) {
                MethodInfo get = t.GetMethod("GetLocalizedString",
                    BindingFlags.Public | BindingFlags.Instance);
                if (get == null) continue;
                var ps = get.GetParameters();
                if (ps.Length != 1 || !ps[0].ParameterType.IsEnum) continue;

                object inst;
                try { inst = Activator.CreateInstance(t); } catch { continue; }

                foreach (var id in Enum.GetValues(ps[0].ParameterType)) {
                    string text;
                    try { text = get.Invoke(inst, new[] { id }) as string; } catch { continue; }
                    if (!string.IsNullOrEmpty(text) && wanted.Contains(text))
                        sb.AppendLine($"{t.FullName}\t{ps[0].ParameterType.Name}.{id}\t{text}");
                }
            }
        }
        File.WriteAllText("found.txt", sb.ToString(), Encoding.UTF8);
    }
}
```

`dotnet run`, ואז `found.txt` נותן בדיוק את שם המתרגם ואת שם ה-id. אפשר גם
להוריד את תנאי `wanted` ולקבל את **כל** המחרוזות של מתרגם מסוים — נוח כשרוצים
לתרגם חלון שלם ולא מחרוזת אחת.

חשוב: להשתמש **באותה גרסת DevExpress** של האפליקציה (כאן 25.2.3), אחרת שמות
ה-id עלולים להיות שונים.

### הדרך המהירה

לפי הטבלה שלמעלה — לרוב ברור לאיזה מודול שייך החלון. אם ניחשת לא נכון, ה-`case`
פשוט לא יתפוס והטקסט יישאר באנגלית; אין נזק.

---

## 3. איך מתרגמים

מוסיפים `case` למחלקה המתאימה ב-`Classes/HebrewLocalizers.cs`:

```csharp
public class HebrewFilterLocalizer : FilterUIElementLocalizer
{
    public override string GetLocalizedString(FilterUIElementLocalizerStringId id)
    {
        switch (id)
        {
            case FilterUIElementLocalizerStringId.FilteringUIClose: return "סגור";
            // ...
            default: return base.GetLocalizedString(id);
        }
    }
}
```

`default: return base...` חשוב — כל מה שלא תרגמנו נשאר כמו שהיה במקום להיעלם.

אם מדובר במתרגם חדש שעדיין אין לו מחלקה, מוסיפים מחלקה באותו קובץ באותה תבנית.

---

## 4. איפה מפעילים — פעם אחת, ב-Program.cs

```csharp
DevExpress.XtraGrid.Localization.GridLocalizer.Active = new HebrewGridLocalizer();
DevExpress.Utils.Filtering.Internal.FilterUIElementLocalizer.Active = new HebrewFilterLocalizer();
```

**לא בקונסטרוקטור של טופס.** ההגדרה גלובלית לכל האפליקציה, ואם היא יושבת בטופס
מסוים — כל שאר המסכים נשארים באנגלית עד שמישהו פותח דווקא אותו. זה מה שקרה כאן:
`HebrewGridLocalizer` הוגדר בתוך `frDiscountsCodes`, ולכן הרשתות בשאר המסכים לא
היו מתורגמות. עכשיו זה ב-`Program.cs`, לפני שנפתח טופס כלשהו.

לכן גם התשובה ל"הבעיה קיימת בכמה טפסים" היא בדרך כלל: **לא צריך לתקן כל טופס** —
צריך להוסיף `case` אחד למתרגם המתאים, וזה חל על כל הטפסים בבת אחת.

---

## 5. חלופה: קבצי התרגום הרשמיים של DevExpress

ל-DevExpress יש Localization Service שממנו מורידים satellite assemblies
(`he/DevExpress.*.resources.dll`). זה מתרגם הכול בבת אחת, אבל התרגום העברי הוא
תרומת קהילה ולעיתים חלקי או לא במינוח שאנחנו רוצים. השיטה שכאן — `case` לכל
מחרוזת שבאמת מופיעה על המסך — נותנת שליטה מלאה על הניסוח, ולכן היא זו שנבחרה.
