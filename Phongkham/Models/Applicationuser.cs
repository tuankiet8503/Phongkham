using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;

namespace Phongkham.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FullName { get; set; }
        public string? Address { get; set; }
        public string? Age { get; set; }
        public string? ImageUrl { get; set; }
        public List<UserImage> Images { get; set; }
        [NotMapped]
        public string SelectedRole { get; set; }
        public int? ChuyenMonId { get; set; }
        [EnumDataType(typeof(TrangThaiTaiKhoan))]
        public TrangThaiTaiKhoan TrangThai { get; set; }

        public ICollection<Questions> Questions { get; set; } = new List<Questions>();
        public ICollection<Answers> Answers { get; set; } = new List<Answers>();

        public static implicit operator ApplicationUser(ClaimsPrincipal v)
        {
            throw new NotImplementedException();
        }
    }
}
