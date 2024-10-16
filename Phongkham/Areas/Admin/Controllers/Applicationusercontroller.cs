using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Phongkham.Data;
using Phongkham.Models;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Phongkham.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ApplicationUserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDBcontext _context;

        public ApplicationUserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDBcontext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<IActionResult> Index(string role)
        {
            var users = await _userManager.Users.ToListAsync();
            var userRoles = new Dictionary<string, IList<string>>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles;
            }

            if (!string.IsNullOrEmpty(role))
            {
                users = users.Where(u => userRoles[u.Id].Contains(role)).ToList();
            }

            ViewBag.UserRoles = userRoles;
            ViewBag.SelectedRole = role;

            return View(users);
        }
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.UserRoles = roles;

            return View(user);
        }
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = _roleManager.Roles.Select(r => r.Name).ToList();
            var userRoles = await _userManager.GetRolesAsync(user);
            var specialties = _context.Chuyenmons.ToList(); // Assuming _context is your DbContext

            ViewBag.Roles = new SelectList(roles, userRoles.FirstOrDefault());
            ViewBag.UserRoles = userRoles;
            ViewBag.Specialties = new SelectList(specialties, "Id", "Name", user.ChuyenMonId);

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Email,FullName,Address,Age,ImageUrl,SelectedRole,ChuyenMonId")] ApplicationUser user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            var existingUser = await _userManager.FindByIdAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            // Không cập nhật UserName
            user.UserName = existingUser.UserName;

            // Cập nhật các thuộc tính khác
            existingUser.Email = user.Email;
            existingUser.FullName = user.FullName;
            existingUser.Address = user.Address;
            existingUser.Age = user.Age;
            existingUser.ImageUrl = user.ImageUrl;
            existingUser.ChuyenMonId = user.ChuyenMonId;

            var result = await _userManager.UpdateAsync(existingUser);
            if (result.Succeeded)
            {
                var existingRoles = await _userManager.GetRolesAsync(existingUser);
                await _userManager.RemoveFromRolesAsync(existingUser, existingRoles);
                await _userManager.AddToRoleAsync(existingUser, user.SelectedRole);

                // Lấy vai trò mới của người dùng
                var newRoles = await _userManager.GetRolesAsync(existingUser);

                // Nếu người dùng từ vai trò Dentist sang vai trò khác
                if (existingRoles.Contains("Dentist") && !newRoles.Contains("Dentist"))
                {
                    // Xóa bản ghi tương ứng trong bảng CTnhasi
                    var ctnhasi = _context.cTnhasis.SingleOrDefault(c => c.UserId == user.Id);
                    if (ctnhasi != null)
                    {
                        _context.cTnhasis.Remove(ctnhasi);
                        await _context.SaveChangesAsync();
                    }
                }
                // Nếu ngược lại, người dùng đổi từ vai trò khác sang Dentist
                else if (!existingRoles.Contains("Dentist") && newRoles.Contains("Dentist"))
                {
                    // Thêm bản ghi mới vào bảng CTnhasi
                    var ctnhasi = new CTnhasi
                    {
                        chuyenmonId = user.ChuyenMonId.Value,
                        UserId = user.Id
                    };

                    _context.cTnhasis.Add(ctnhasi);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            ViewBag.Roles = new SelectList(_roleManager.Roles.Select(r => r.Name).ToList());
            ViewBag.Specialties = new SelectList(_context.Chuyenmons.ToList(), "Id", "Name");

            return View(user);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                // Khóa tài khoản người dùng
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.MaxValue; // Khóa vĩnh viễn

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(user);
        }

    }
}
