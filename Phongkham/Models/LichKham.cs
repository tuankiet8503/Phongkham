using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Phongkham.Models
{
    public class lichKham
    {
        public int Id { get; set; }
        public string PatientId { get; set; }
        public string UserName { get; set; }
        public DateTime NgayDat { get; set; } = DateTime.Now;
        public string phone { get; set; }
        public int CakhamId { get; set; }
        public Cakham? cakham { get; set; }
        [ForeignKey("PatientId")]
        public ApplicationUser Patient { get; set; }
        public List<CTlichkham> Clichkham { get; set; }
    }
}
