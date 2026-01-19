namespace trangcv.Models
{
    /// <summary>
    /// Model chứa toàn bộ thông tin CV cá nhân
    /// </summary>
    public class CVModel
    {
        // 1. Thông tin cá nhân
        public string HoTen { get; set; } = "";
        public string MSSV { get; set; } = "";
        public string NgaySinh { get; set; } = "";
        public string Lop { get; set; } = "";
        public string Khoa { get; set; } = "";
        public string AnhDaiDien { get; set; } = "/images/avatar.jpg";
        public string Email { get; set; } = "";
        public string SoDienThoai { get; set; } = "";
        public string DiaChi { get; set; } = "";
        public string GioiThieu { get; set; } = "";

        // 2. Thông tin học vấn
        public string TruongDaoTao { get; set; } = "";
        public string NganhHoc { get; set; } = "";
        public string GPA { get; set; } = "";
        public string ChungChi { get; set; } = ""; // Phân cách bằng dấu |

        // 3. Thông tin kỹ năng
        public string KyNangChuyenMon { get; set; } = ""; // Phân cách bằng dấu |
        public string KyNangMem { get; set; } = ""; // Phân cách bằng dấu |
        public string NgoaiNgu { get; set; } = ""; // Phân cách bằng dấu |

        // 4. NCKH & Dự án (JSON string để lưu trong CSV)
        public string NCKH { get; set; } = ""; // JSON array string
        public string DuAn { get; set; } = ""; // JSON array string

        // 5. Sở thích & Mục tiêu
        public string SoThich { get; set; } = ""; // Phân cách bằng dấu |
        public string MucTieuNgheNghiep { get; set; } = "";

        // Helper methods để chuyển đổi danh sách
        public List<string> GetChungChiList() => ParsePipeDelimited(ChungChi);
        public List<string> GetKyNangChuyenMonList() => ParsePipeDelimited(KyNangChuyenMon);
        public List<string> GetKyNangMemList() => ParsePipeDelimited(KyNangMem);
        public List<string> GetNgoaiNguList() => ParsePipeDelimited(NgoaiNgu);
        public List<string> GetSoThichList() => ParsePipeDelimited(SoThich);

        private List<string> ParsePipeDelimited(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new List<string>();
            return value.Split('|', StringSplitOptions.RemoveEmptyEntries)
                       .Select(s => s.Trim())
                       .ToList();
        }
    }

    /// <summary>
    /// Model cho nhiệm vụ NCKH
    /// </summary>
    public class ResearchProject
    {
        public string TenNhiemVu { get; set; } = "";
        public string MoTa { get; set; } = "";
        public string ThoiGian { get; set; } = "";
        public string CongViec { get; set; } = "";
        public string VaiTro { get; set; } = "";
        public string KetQua { get; set; } = "";
    }

    /// <summary>
    /// Model cho dự án
    /// </summary>
    public class Project
    {
        public string TenDuAn { get; set; } = "";
        public string MoTa { get; set; } = "";
        public string ThoiGian { get; set; } = "";
        public string CongNghe { get; set; } = "";
        public string VaiTro { get; set; } = "";
    }
}
