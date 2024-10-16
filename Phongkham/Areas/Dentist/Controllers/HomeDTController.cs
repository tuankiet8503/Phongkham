using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Phongkham.Data;

using Phongkham.Models;
using System.Diagnostics;

namespace Phongkham.Areas.Dentist.Controllers
{
    [Authorize(Roles = "Dentist")]
    public class HomeDTController : Controller
    {
        // GET: HomeDTController
        private readonly ApplicationDBcontext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public HomeDTController(ApplicationDBcontext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound("Người dùng không tồn tại hoặc không đăng nhập.");
            }

            var dentistId = currentUser.Id;
            // Lấy danh sách lịch khám của nha sĩ đang đăng nhập
            var lichKham = await _context.lichKhams
                                         .Where(lk => lk.cakham.DentistId == dentistId)
                                         .ToListAsync();
            var chuyenMon = _context.Chuyenmons.ToList(); // Lấy danh sách chuyên môn
            var tinTuc = _context.Tintucs.ToList(); // Lấy danh sách tin tức
            var questions = _context.Questions.ToList(); // Lấy danh sách câu hỏi từ bệnh nhân
            var loaiTinTuc = _context.Loaitintucs.ToList();
            // Đưa thông tin lấy được vào view
            ViewData["lichKham"] = lichKham;
            ViewData["ChuyenMon"] = chuyenMon;
            ViewData["TinTuc"] = tinTuc;
            ViewData["Questions"] = questions;
            ViewData["LoaiTinTuc"] = loaiTinTuc;
            return View();
        }
        public async Task<IActionResult> IndexTT(int? loaiTinTucId)
        {
            var loaiTinTuc = _context.Loaitintucs.ToList();
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound("Người dùng không tồn tại hoặc không đăng nhập.");
            }

            // Lấy tin tức thuộc loại tin tức được chọn (nếu có)
            var tinTucQuery = _context.Tintucs.AsQueryable();
            if (loaiTinTucId.HasValue)
            {
                tinTucQuery = tinTucQuery.Where(tt => tt.LoaitintucId == loaiTinTucId);
            }
            var tinTuc = await tinTucQuery.ToListAsync();

            ViewData["SelectedLoaiTinTucId"] = loaiTinTucId;
            ViewData["LoaiTinTuc"] = loaiTinTuc;
            return View(tinTuc);
        }

        public async Task<IActionResult> IndexLK()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound("Người dùng không tồn tại hoặc không đăng nhập.");
            }
            var loaiTinTuc = _context.Loaitintucs.ToList();
            var dentistId = currentUser.Id;
            // Lấy danh sách lịch khám của nha sĩ đang đăng nhập
            var lichKham = await _context.lichKhams
                                  .Include(lk => lk.cakham)
                                  .ThenInclude(ck => ck.KhungGio)
                                  .Where(lk => lk.cakham.DentistId == dentistId)
                                  .ToListAsync();
            // Đưa thông tin lấy được vào view
            ViewData["lichKham"] = lichKham;
            ViewData["LoaiTinTuc"] = loaiTinTuc;
            return View();
        }
        // GET: HomeDTController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: HomeDTController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: HomeDTController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: HomeDTController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: HomeDTController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: HomeDTController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: HomeDTController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
