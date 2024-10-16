using System.ComponentModel.DataAnnotations;

namespace Phongkham.Models
{
    public class Chuyenmon
    {
        public int Id { get; set; }
        [Required, StringLength(100)]
        public string Name { get; set; }
        public List<CTnhasi>? cTnhasis { get; set; }
    }
}
