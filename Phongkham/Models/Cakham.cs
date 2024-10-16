using System.ComponentModel.DataAnnotations;

namespace Phongkham.Models
{
    public class Cakham
    {
        public int Id { get; set; }
        public int KhungGioId { get; set; }
        public decimal Gia { get; set; }
        public DateTime NgayDang { get; set; } = DateTime.Now;
        public KhungGio? KhungGio { get; set; }
        [EnumDataType(typeof(TrangThaiCaKham))]
        public TrangThaiCaKham TrangThai { get; set; }

        public string DentistId { get; set; }
        public ApplicationUser? Dentist { get; set; }
    }
}