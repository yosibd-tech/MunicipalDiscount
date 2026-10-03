// שלב א מודל הנתונים
// Models/FamilyMember.cs
public class FamilyMember
{
    public string IdNumber { get; set; }      // ת.ז.
    public string FirstName { get; set; }     // שם פרטי
    public string LastName { get; set; }      // שם משפחה
    public int Age { get; set; }              // גיל
    public string Status { get; set; }        // סטטוס (ראש משפחה/בן זוג/ילד)
    public string Relation { get; set; }      // קרבה
}

// Models/FamilyData.cs
public class FamilyData
{
    public string HeadOfFamilyId { get; set; }
    public List<FamilyMember> Members { get; set; }
}

// שלב ב שירות API
// Services/PopulationRegistryService.cs
public class PopulationRegistryService
{
    private readonly HttpClient _client;
    private readonly string _apiKey;
    private readonly string _baseUrl;

    public PopulationRegistryService(string apiKey)
    {
        _client = new HttpClient();
        _apiKey = apiKey;
        _baseUrl = "https://api.gov.il/population";  // כתובת לדוגמה
        
        // הוספת הרשאות לכל בקשה
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<FamilyData> GetFamilyByIdAsync(string headOfFamilyId)
    {
        try
        {
            // וידוא ת.ז. תקינה
            if (!IsValidIsraeliId(headOfFamilyId))
                throw new ArgumentException("ת.ז. לא תקינה");

            // קריאה ל-API
            var response = await _client.GetAsync($"{_baseUrl}/family/{headOfFamilyId}");
            
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"שגיאה בקריאה ל-API: {response.StatusCode}");
            }

            // פרסור התשובה
            string json = await response.Content.ReadAsStringAsync();
            var family = JsonConvert.DeserializeObject<FamilyData>(json);
            
            return family;
        }
        catch (HttpRequestException ex)
        {
            throw new Exception("שגיאת תקשורת עם מרשם האוכלוסין", ex);
        }
    }

    // וידוא ת.ז. ישראלית
    private bool IsValidIsraeliId(string id)
    {
        if (string.IsNullOrEmpty(id) || id.Length > 9)
            return false;
            
        id = id.PadLeft(9, '0');
        int sum = 0;
        
        for (int i = 0; i < 9; i++)
        {
            int digit = id[i] - '0';
            int step = digit * ((i % 2) + 1);
            sum += step > 9 ? step - 9 : step;
        }
        
        return sum % 10 == 0;
    }
}

// שלב ג שימוש בטופס

// בטופס הראשי
private PopulationRegistryService _registryService;

private void Form_Load(object sender, EventArgs e)
{
    string apiKey = ConfigurationManager.AppSettings["PopulationApiKey"];
    _registryService = new PopulationRegistryService(apiKey);
}

private async void btnGetFamily_Click(object sender, EventArgs e)
{
    string idNumber = txtIdNumber.Text.Trim();
    
    try
    {
        btnGetFamily.Enabled = false;
        lblStatus.Text = "מושך נתונים...";

        var family = await _registryService.GetFamilyByIdAsync(idNumber);

        // הצגה ב-DataGridView
        dgvFamily.DataSource = family.Members;
        
        lblStatus.Text = $"נמצאו {family.Members.Count} בני משפחה";
    }
    catch (Exception ex)
    {
        MessageBox.Show(ex.Message, "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
    finally
    {
        btnGetFamily.Enabled = true;
    }
}

//שלב 4: הגדרות ב-App.config

<appSettings>
    <add key="PopulationApiKey" value="YOUR_API_KEY_HERE"/>
    <add key="PopulationApiUrl" value="https://api.gov.il/population"/>
</appSettings>

//הסבר
//מה קורה בפועל
//שלב 1 - אתה שולח בקשה:

var response = await _client.GetAsync("https://api.gov.il/population/family/123456789");

//שלב 2 - ה-API מחזיר טקסט (JSON):

{
    "headOfFamilyId": "123456789",
    "members": [
        {"idNumber": "123456789", "firstName": "יוסי", "age": 45},
        {"idNumber": "234567890", "firstName": "רונית", "age": 42}
    ]
}

//שלב 3 - אתה ממיר (מפרסר) לאובייקט C#:
string json = await response.Content.ReadAsStringAsync();  // קיבלת טקסט
var family = JsonConvert.DeserializeObject<FamilyData>(json);  // הפך לאובייקט

// עכשיו אפשר לגשת לנתונים:
string name = family.Members[0].FirstName;  // "יוסי"

// עד כאן קוד
/* מימוש
1. פנייה לרשות התקשוב הממשלתי
   https://www.gov.il/he/departments/digital_israel

2. הגשת בקשה + הסבר למה צריך את המידע

3. חתימה על הסכם שמירת סודיות

4. קבלת:
   - כתובת API אמיתית
   - API Key / הרשאות
   - תיעוד טכני*/
   
   /*חלופות לפיתוח ובדיקות
עד שמקבלים גישה אמיתית:
אפשרות 1: Mock API מקומי*/

