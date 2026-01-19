# Trang CV Cá Nhân - ASP.NET MVC

Ứng dụng web theo mô hình **MVC** để tạo, lưu trữ và hiển thị CV cá nhân. Dữ liệu lưu file CSV với encoding UTF-8 BOM.

---

## 📁 Cấu Trúc

```
trangcv/
├── Controllers/HomeController.cs   # Điều phối logic
├── Models/CVModel.cs               # Cấu trúc dữ liệu
├── Services/CVService.cs           # Read/Write CSV
├── Views/Home/
│   ├── Index.cshtml                # Hiển thị CV
│   └── Edit.cshtml                 # Form nhập liệu
├── Data/cv_data.csv                # Lưu trữ dữ liệu
└── wwwroot/css/site.css            # Dark Theme
```

---

## 🔧 Logic Code Chi Tiết

### 1. CVModel.cs - Cấu Trúc Dữ Liệu

**Chiến lược lưu trữ flat-file:**
- Thuộc tính đơn → String trực tiếp
- Danh sách đơn giản → Pipe-delimited: `"C#|Python|JavaScript"`
- Đối tượng phức tạp (NCKH, DuAn) → JSON string

```csharp
public class CVModel
{
    // Thuộc tính đơn
    public string HoTen { get; set; } = "";
    public string MSSV { get; set; } = "";
    
    // Danh sách pipe-delimited
    public string KyNangChuyenMon { get; set; } = "";  // "C#|Python|JS"
    
    // JSON string cho đối tượng phức tạp
    public string NCKH { get; set; } = "";   // JSON array
    public string DuAn { get; set; } = "";   // JSON array

    // Helper method: Parse pipe-delimited → List<string>
    public List<string> GetKyNangChuyenMonList() => ParsePipeDelimited(KyNangChuyenMon);
    
    private List<string> ParsePipeDelimited(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return new List<string>();
        return value.Split('|', StringSplitOptions.RemoveEmptyEntries)
                   .Select(s => s.Trim()).ToList();
    }
}

// Sub-models cho NCKH và Dự án
public class ResearchProject
{
    public string TenNhiemVu { get; set; } = "";
    public string MoTa { get; set; } = "";
    public string ThoiGian { get; set; } = "";
    public string CongViec { get; set; } = "";
    public string VaiTro { get; set; } = "";
    public string KetQua { get; set; } = "";
}

public class Project
{
    public string TenDuAn { get; set; } = "";
    public string MoTa { get; set; } = "";
    public string ThoiGian { get; set; } = "";
    public string CongNghe { get; set; } = "";
    public string VaiTro { get; set; } = "";
}
```

---

### 2. CVService.cs - CSV Read/Write Logic

**Cấu trúc CSV (Key-Value):**
```csv
Field,Value
HoTen,"Nguyễn Văn A"
KyNangChuyenMon,"C#|ASP.NET|Python"
NCKH,"[{""TenNhiemVu"":""AI Research"",""MoTa"":""...""}]"
```

**Xử lý encoding tiếng Việt:**
```csharp
// UTF-8 với BOM để Excel nhận diện đúng
var utf8WithBom = new UTF8Encoding(true);
File.WriteAllText(_csvFilePath, sb.ToString(), utf8WithBom);
```

**Escape CSV đúng chuẩn RFC 4180:**
```csharp
private string EscapeCsv(string value)
{
    if (string.IsNullOrEmpty(value)) return "";
    
    // Wrap trong "" nếu chứa ký tự đặc biệt
    if (value.Contains(',') || value.Contains('\n') || value.Contains('"'))
    {
        // Escape dấu " thành ""
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
    return value;
}
```

**Parse CSV xử lý quoted fields:**
```csharp
private string[] ParseCsvLine(string line)
{
    var result = new List<string>();
    var current = new StringBuilder();
    bool inQuotes = false;

    for (int i = 0; i < line.Length; i++)
    {
        char c = line[i];
        if (c == '"')
        {
            // Escape sequence "" → "
            if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
            {
                current.Append('"');
                i++;
            }
            else inQuotes = !inQuotes;
        }
        else if (c == ',' && !inQuotes)
        {
            result.Add(current.ToString());
            current.Clear();
        }
        else current.Append(c);
    }
    result.Add(current.ToString());
    return result.ToArray();
}
```

**JSON Serialization cho NCKH/DuAn:**
```csharp
public List<ResearchProject> GetNCKHList(string json)
{
    if (string.IsNullOrWhiteSpace(json)) return new List<ResearchProject>();
    try { return JsonSerializer.Deserialize<List<ResearchProject>>(json) ?? new(); }
    catch { return new List<ResearchProject>(); }
}

public List<Project> GetProjectList(string json)
{
    if (string.IsNullOrWhiteSpace(json)) return new List<Project>();
    try { return JsonSerializer.Deserialize<List<Project>>(json) ?? new(); }
    catch { return new List<Project>(); }
}
```

---

### 3. HomeController.cs - MVC Flow

**Dependency Injection:**
```csharp
private readonly CVService _cvService;

public HomeController(CVService cvService)
{
    _cvService = cvService;  // Inject từ DI Container
}
```

**Index Action - Read Flow:**
```csharp
public IActionResult Index()
{
    var cv = _cvService.GetCV();  // Đọc từ CSV
    
    // Parse JSON → Objects cho View sử dụng
    ViewBag.NCKHList = _cvService.GetNCKHList(cv.NCKH);
    ViewBag.ProjectList = _cvService.GetProjectList(cv.DuAn);
    
    return View(cv);
}
```

**Edit POST Action - Write Flow:**
```csharp
[HttpPost]
[ValidateAntiForgeryToken]  // CSRF Protection
public IActionResult Edit(CVModel cv, string NCKHJson, string DuAnJson)
{
    // Nhận JSON từ hidden input (do JavaScript serialize)
    cv.NCKH = NCKHJson ?? "[]";
    cv.DuAn = DuAnJson ?? "[]";
    
    _cvService.SaveCV(cv);  // Ghi vào CSV
    TempData["Message"] = "Lưu CV thành công!";
    
    return RedirectToAction("Index");  // PRG Pattern
}
```

---

### 4. View Logic

**Index.cshtml - Data Binding:**
```razor
@model trangcv.Models.CVModel
@{
    var nckh = ViewBag.NCKHList as List<ResearchProject>;
    var projects = ViewBag.ProjectList as List<Project>;
}

<!-- Razor syntax binding -->
<h1>@Model.HoTen</h1>

<!-- Helper method call trong View -->
@foreach (var skill in Model.GetKyNangChuyenMonList())
{
    <span class="skill-badge">@skill</span>
}

<!-- ViewBag for complex objects -->
@foreach (var proj in projects)
{
    <div class="project-card">@proj.TenDuAn</div>
}
```

**Edit.cshtml - Dynamic Form với JavaScript:**
```razor
<form asp-action="Edit" method="post">
    @Html.AntiForgeryToken()
    
    <!-- Tag Helper binding -->
    <input asp-for="HoTen" class="form-control" />
    
    <!-- Dynamic NCKH container -->
    <div id="nckh-container">
        @for (int i = 0; i < nckh.Count; i++)
        {
            <div class="dynamic-item">
                <input class="nckh-ten" value="@nckh[i].TenNhiemVu" />
                <input class="nckh-mota" value="@nckh[i].MoTa" />
                <!-- ... other fields -->
            </div>
        }
    </div>
    
    <!-- Hidden inputs nhận JSON từ JS -->
    <input type="hidden" name="NCKHJson" id="NCKHJson" />
    <input type="hidden" name="DuAnJson" id="DuAnJson" />
    
    <button type="submit" onclick="prepareSubmit()">Lưu</button>
</form>
```

**JavaScript - Serialize Dynamic Items:**
```javascript
function prepareSubmit() {
    // Thu thập NCKH từ DOM → JSON
    const nckhList = [];
    document.querySelectorAll('#nckh-container .dynamic-item').forEach(item => {
        nckhList.push({
            TenNhiemVu: item.querySelector('.nckh-ten').value,
            MoTa: item.querySelector('.nckh-mota').value,
            ThoiGian: item.querySelector('.nckh-thoigian').value,
            CongViec: item.querySelector('.nckh-congviec').value,
            VaiTro: item.querySelector('.nckh-vaitro').value,
            KetQua: item.querySelector('.nckh-ketqua').value
        });
    });
    document.getElementById('NCKHJson').value = JSON.stringify(nckhList);
    
    // Tương tự cho DuAn...
}

function addNCKH() {
    const container = document.getElementById('nckh-container');
    container.insertAdjacentHTML('beforeend', `
        <div class="dynamic-item">
            <input class="nckh-ten" placeholder="Tên nhiệm vụ" />
            <!-- ... other inputs -->
        </div>
    `);
}

function removeNCKH(btn) {
    btn.closest('.dynamic-item').remove();
}
```

---

### 5. Program.cs - DI Registration

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

// Scoped: 1 instance per HTTP request
builder.Services.AddScoped<CVService>();

var app = builder.Build();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
```

---

## 🎨 CSS Architecture

**CSS Variables (Dark Theme):**
```css
:root {
    --bg-primary: #0d1117;        /* Main background */
    --bg-secondary: #161b22;      /* Cards, footer */
    --bg-card: #21262d;           /* Elevated cards */
    --text-primary: #e6edf3;      /* Main text */
    --text-secondary: #8b949e;    /* Secondary text */
    --border-color: #30363d;      /* Borders */
    --gradient-primary: linear-gradient(135deg, #4a5568 0%, #2d3748 100%);  /* Cool Grey */
}
```

**Sticky Footer (Flexbox):**
```css
body {
    min-height: 100vh;
    display: flex;
    flex-direction: column;
}

main.flex-grow-1 { flex: 1 0 auto; }  /* Expand to fill */
.footer { flex-shrink: 0; margin-top: auto !important; }  /* Stick to bottom */
```

---

## 📊 Data Flow Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                        HTTP Request                              │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌───────────────────────────────────────────────────────────────────┐
│  HomeController                                                   │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐               │
│  │  Index()    │  │  Edit()GET  │  │ Edit()POST  │               │
│  │  Read CV    │  │  Read CV    │  │  Write CV   │               │
│  └──────┬──────┘  └──────┬──────┘  └──────┬──────┘               │
└─────────┼────────────────┼────────────────┼──────────────────────┘
          │                │                │
          ▼                ▼                ▼
┌───────────────────────────────────────────────────────────────────┐
│  CVService                                                        │
│  ┌──────────────────────────────────────────────────────────┐    │
│  │  GetCV()           │  SaveCV()                           │    │
│  │  - ReadAllLines    │  - StringBuilder                    │    │
│  │  - ParseCsvLine    │  - EscapeCsv                        │    │
│  │  - Map to Model    │  - WriteAllText (UTF8+BOM)          │    │
│  └──────────────────────────────────────────────────────────┘    │
└───────────────────────────┬──────────────────────────────────────┘
                            │
                            ▼
┌───────────────────────────────────────────────────────────────────┐
│  Data/cv_data.csv                                                 │
│  ┌──────────────────────────────────────────────────────────┐    │
│  │  Field,Value                                              │    │
│  │  HoTen,"Nguyễn Văn A"                                     │    │
│  │  KyNangChuyenMon,"C#|Python|JavaScript"                   │    │
│  │  NCKH,"[{""TenNhiemVu"":""...""}]"                         │    │
│  └──────────────────────────────────────────────────────────┘    │
└───────────────────────────────────────────────────────────────────┘
```

---

## 📄 License

MIT License
