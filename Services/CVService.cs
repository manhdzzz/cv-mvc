using System.Text;
using System.Text.Json;
using trangcv.Models;

namespace trangcv.Services
{
    /// <summary>
    /// Service để đọc/ghi dữ liệu CV vào file CSV
    /// </summary>
    public class CVService
    {
        private readonly string _dataPath;
        private readonly string _csvFilePath;

        public CVService(IWebHostEnvironment env)
        {
            _dataPath = Path.Combine(env.ContentRootPath, "Data");
            _csvFilePath = Path.Combine(_dataPath, "cv_data.csv");

            // Đảm bảo thư mục Data tồn tại
            if (!Directory.Exists(_dataPath))
            {
                Directory.CreateDirectory(_dataPath);
            }
        }

        /// <summary>
        /// Lưu CV vào file CSV
        /// </summary>
        public void SaveCV(CVModel cv)
        {
            var sb = new StringBuilder();

            // Header row
            sb.AppendLine("Field,Value");

            // Personal Info
            sb.AppendLine($"HoTen,{EscapeCsv(cv.HoTen)}");
            sb.AppendLine($"MSSV,{EscapeCsv(cv.MSSV)}");
            sb.AppendLine($"NgaySinh,{EscapeCsv(cv.NgaySinh)}");
            sb.AppendLine($"Lop,{EscapeCsv(cv.Lop)}");
            sb.AppendLine($"Khoa,{EscapeCsv(cv.Khoa)}");
            sb.AppendLine($"AnhDaiDien,{EscapeCsv(cv.AnhDaiDien)}");
            sb.AppendLine($"Email,{EscapeCsv(cv.Email)}");
            sb.AppendLine($"SoDienThoai,{EscapeCsv(cv.SoDienThoai)}");
            sb.AppendLine($"DiaChi,{EscapeCsv(cv.DiaChi)}");
            sb.AppendLine($"GioiThieu,{EscapeCsv(cv.GioiThieu)}");

            // Education
            sb.AppendLine($"TruongDaoTao,{EscapeCsv(cv.TruongDaoTao)}");
            sb.AppendLine($"NganhHoc,{EscapeCsv(cv.NganhHoc)}");
            sb.AppendLine($"GPA,{EscapeCsv(cv.GPA)}");
            sb.AppendLine($"ChungChi,{EscapeCsv(cv.ChungChi)}");

            // Skills
            sb.AppendLine($"KyNangChuyenMon,{EscapeCsv(cv.KyNangChuyenMon)}");
            sb.AppendLine($"KyNangMem,{EscapeCsv(cv.KyNangMem)}");
            sb.AppendLine($"NgoaiNgu,{EscapeCsv(cv.NgoaiNgu)}");

            // Projects (JSON strings)
            sb.AppendLine($"NCKH,{EscapeCsv(cv.NCKH)}");
            sb.AppendLine($"DuAn,{EscapeCsv(cv.DuAn)}");

            // Goals
            sb.AppendLine($"SoThich,{EscapeCsv(cv.SoThich)}");
            sb.AppendLine($"MucTieuNgheNghiep,{EscapeCsv(cv.MucTieuNgheNghiep)}");

            // Sử dụng UTF-8 với BOM để đảm bảo tiếng Việt hiển thị đúng trong Excel
            var utf8WithBom = new UTF8Encoding(true);
            File.WriteAllText(_csvFilePath, sb.ToString(), utf8WithBom);
        }

        /// <summary>
        /// Đọc CV từ file CSV
        /// Dữ liệu được đọc từ file Data/cv_data.csv
        /// </summary>
        public CVModel GetCV()
        {
            var cv = new CVModel();

            // Nếu file không tồn tại, trả về CV trống
            if (!File.Exists(_csvFilePath))
            {
                return cv;
            }

            try
            {
                // Đọc file với UTF-8 (tự động detect BOM)
                var utf8WithBom = new UTF8Encoding(true);
                var lines = File.ReadAllLines(_csvFilePath, utf8WithBom);
                var data = new Dictionary<string, string>();

                foreach (var line in lines.Skip(1)) // Skip header
                {
                    var parts = ParseCsvLine(line);
                    if (parts.Length >= 2)
                    {
                        data[parts[0]] = parts[1];
                    }
                }

                // Map to CVModel - tất cả dữ liệu từ file CSV
                cv.HoTen = data.GetValueOrDefault("HoTen", "");
                cv.MSSV = data.GetValueOrDefault("MSSV", "");
                cv.NgaySinh = data.GetValueOrDefault("NgaySinh", "");
                cv.Lop = data.GetValueOrDefault("Lop", "");
                cv.Khoa = data.GetValueOrDefault("Khoa", "");
                cv.AnhDaiDien = data.GetValueOrDefault("AnhDaiDien", "/images/avatar.jpg");
                cv.Email = data.GetValueOrDefault("Email", "");
                cv.SoDienThoai = data.GetValueOrDefault("SoDienThoai", "");
                cv.DiaChi = data.GetValueOrDefault("DiaChi", "");
                cv.GioiThieu = data.GetValueOrDefault("GioiThieu", "");

                cv.TruongDaoTao = data.GetValueOrDefault("TruongDaoTao", "");
                cv.NganhHoc = data.GetValueOrDefault("NganhHoc", "");
                cv.GPA = data.GetValueOrDefault("GPA", "");
                cv.ChungChi = data.GetValueOrDefault("ChungChi", "");

                cv.KyNangChuyenMon = data.GetValueOrDefault("KyNangChuyenMon", "");
                cv.KyNangMem = data.GetValueOrDefault("KyNangMem", "");
                cv.NgoaiNgu = data.GetValueOrDefault("NgoaiNgu", "");

                cv.NCKH = data.GetValueOrDefault("NCKH", "");
                cv.DuAn = data.GetValueOrDefault("DuAn", "");

                cv.SoThich = data.GetValueOrDefault("SoThich", "");
                cv.MucTieuNgheNghiep = data.GetValueOrDefault("MucTieuNgheNghiep", "");

                return cv;
            }
            catch
            {
                // Nếu lỗi đọc file, trả về CV trống
                return cv;
            }
        }

        /// <summary>
        /// Lấy danh sách NCKH từ JSON string
        /// </summary>
        public List<ResearchProject> GetNCKHList(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<ResearchProject>();

            try
            {
                return JsonSerializer.Deserialize<List<ResearchProject>>(json) ?? new List<ResearchProject>();
            }
            catch
            {
                return new List<ResearchProject>();
            }
        }

        /// <summary>
        /// Lấy danh sách Dự án từ JSON string
        /// </summary>
        public List<Project> GetProjectList(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<Project>();

            try
            {
                return JsonSerializer.Deserialize<List<Project>>(json) ?? new List<Project>();
            }
            catch
            {
                return new List<Project>();
            }
        }

        /// <summary>
        /// Chuyển danh sách NCKH thành JSON string
        /// </summary>
        public string SerializeNCKH(List<ResearchProject> list)
        {
            return JsonSerializer.Serialize(list);
        }

        /// <summary>
        /// Chuyển danh sách Dự án thành JSON string
        /// </summary>
        public string SerializeProjects(List<Project> list)
        {
            return JsonSerializer.Serialize(list);
        }

        /// <summary>
        /// Escape các ký tự đặc biệt trong CSV
        /// </summary>
        private string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            // Nếu chứa dấu phẩy, xuống dòng hoặc dấu nháy kép thì wrap trong dấu nháy kép
            if (value.Contains(',') || value.Contains('\n') || value.Contains('"'))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }
            return value;
        }

        /// <summary>
        /// Parse một dòng CSV
        /// </summary>
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
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            result.Add(current.ToString());
            return result.ToArray();
        }
    }
}
