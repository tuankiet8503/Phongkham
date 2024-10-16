using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Phongkham.Models
{
    public class CTlichkham
    {
        public int Id { get; set; }
        [Required]
        public string TongGiaTien { get; set; }
        public int LichkhamId { get; set; }
        public lichKham? LichKham { get; set; }
        public string TenNhaSi { get; set; }
        public string TenBenhNhan { get; set; }
    }
}
