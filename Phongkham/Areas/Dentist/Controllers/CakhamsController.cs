using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Phongkham.Data;
using Phongkham.Models;

namespace Phongkham.Areas.Dentist.Controllers
{
    [Authorize(Roles = "Dentist")]
    public class CakhamsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDBcontext _context;

        public CakhamsController(ApplicationDBcontext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Cakhams
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound(); // Xử lý khi không tìm thấy người dùng
            }

            // Truy vấn các ca khám của nha sĩ đó
            var cakhams = _context.Cakhams
                .Where(c => c.DentistId == currentUser.Id)
                .Include(c => c.Dentist)
                .Include(c => c.KhungGio);
            var loaiTinTuc = _context.Loaitintucs.ToList();
            ViewData["LoaiTinTuc"] = loaiTinTuc;
            return View(await cakhams.ToListAsync());
        }

        // GET: Cakhams/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cakham = await _context.Cakhams
                .Include(c => c.Dentist)
                .Include(c => c.KhungGio)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cakham == null)
            {
                return NotFound();
            }
            var loaiTinTuc = _context.Loaitintucs.ToList();
            ViewData["LoaiTinTuc"] = loaiTinTuc;
            return View(cakham);
        }

        // GET: Cakhams/Create
        public async Task<IActionResult> CreateAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var dentistId = currentUser.Id;

            ViewData["DentistId"] = new SelectList(new List<SelectListItem> { new SelectListItem { Value = dentistId, Text = dentistId } }, "Value", "Text");
            ViewData["KhungGioId"] = new SelectList(_context.KhungGios, "Id", "TimeSlot");
            var cakham = new Cakham
            {
                TrangThai = TrangThaiCaKham.Chưa_Đặt
            };
            var loaiTinTuc = _context.Loaitintucs.ToList();
            ViewData["LoaiTinTuc"] = loaiTinTuc;
            return View(cakham);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,KhungGioId,Gia,NgayDang,TrangThai,DentistId")] Cakham cakham)
        {
            if (ModelState.IsValid)
            {
                var exists = await _context.Cakhams
                    .AnyAsync(c => c.KhungGioId == cakham.KhungGioId && c.NgayDang == cakham.NgayDang && c.DentistId == cakham.DentistId);

                if (exists)
                {
                    ModelState.AddModelError(string.Empty, "Khung giờ và ngày đã tồn tại.");
                    ViewData["DentistId"] = new SelectList(_context.Users, "Id", "Id", cakham.DentistId);
                    ViewData["KhungGioId"] = new SelectList(_context.KhungGios, "Id", "TimeSlot", cakham.KhungGioId);
                    return View(cakham);
                }

                cakham.TrangThai = TrangThaiCaKham.Chưa_Đặt;
                _context.Add(cakham);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tạo ca khám thành công!";
                return RedirectToAction(nameof(Index));
            }
            ViewData["DentistId"] = new SelectList(_context.Users, "Id", "Id", cakham.DentistId);
            ViewData["KhungGioId"] = new SelectList(_context.KhungGios, "Id", "TimeSlot", cakham.KhungGioId);
            return View(cakham);
        }

        // GET: Cakhams/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cakham = await _context.Cakhams.FindAsync(id);
            if (cakham == null)
            {
                return NotFound();
            }
            var currentUser = await _userManager.GetUserAsync(User);
            var dentistId = currentUser.Id;

            ViewData["DentistId"] = new SelectList(new List<SelectListItem> { new SelectListItem { Value = dentistId, Text = dentistId } }, "Value", "Text");
            ViewData["KhungGioId"] = new SelectList(_context.KhungGios, "Id", "TimeSlot");

            var trangThaiList = Enum.GetValues(typeof(TrangThaiCaKham))
                                    .Cast<TrangThaiCaKham>()
                                    .Select(e => new SelectListItem
                                    {
                                        Value = e.ToString(),
                                        Text = e.ToString()
                                    });
            ViewBag.TrangThaiList = trangThaiList;
            var loaiTinTuc = _context.Loaitintucs.ToList();
            ViewData["LoaiTinTuc"] = loaiTinTuc;
            return View(cakham);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,KhungGioId,Gia,NgayDang,TrangThai,DentistId")] Cakham cakham)
        {
            if (id != cakham.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingCakham = await _context.Cakhams.FindAsync(id);
                    if (existingCakham == null)
                    {
                        return NotFound();
                    }

                    // Check if the status is changing to "Chưa đặt"
                    if (cakham.TrangThai == TrangThaiCaKham.Chưa_Đặt && existingCakham.TrangThai == TrangThaiCaKham.Đã_Đặt)
                    {
                        // Find the associated appointments
                        var lichKhams = await _context.lichKhams
                            .Where(lk => lk.CakhamId == cakham.Id)
                            .ToListAsync();

                        foreach (var lichKham in lichKhams)
                        {
                            // Remove the appointment
                            _context.lichKhams.Remove(lichKham);

                            // Optionally: Send a notification to the patient
                            var patient = await _userManager.FindByIdAsync(lichKham.PatientId);
                            if (patient != null)
                            {
                                TempData["Notification"] = $"Lịch khám ngày {lichKham.NgayDat} đã bị hủy.";
                            }
                        }
                    }

                    // Update the existing Cakham entity
                    existingCakham.KhungGioId = cakham.KhungGioId;
                    existingCakham.Gia = cakham.Gia;
                    existingCakham.NgayDang = cakham.NgayDang;
                    existingCakham.TrangThai = cakham.TrangThai;
                    existingCakham.DentistId = cakham.DentistId;

                    _context.Update(existingCakham);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CakhamExists(cakham.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["DentistId"] = new SelectList(_context.Users, "Id", "Id", cakham.DentistId);
            ViewData["KhungGioId"] = new SelectList(_context.KhungGios, "Id", "Id", cakham.KhungGioId);
            var trangThaiList = Enum.GetValues(typeof(TrangThaiCaKham))
                            .Cast<TrangThaiCaKham>()
                            .Select(e => new SelectListItem
                            {
                                Value = e.ToString(),
                                Text = e.ToString()
                            });
            ViewBag.TrangThaiList = trangThaiList;

            return View(cakham);
        }

        // GET: Cakhams/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cakham = await _context.Cakhams
                .Include(c => c.Dentist)
                .Include(c => c.KhungGio)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cakham == null)
            {
                return NotFound();
            }
            var loaiTinTuc = _context.Loaitintucs.ToList();
            ViewData["LoaiTinTuc"] = loaiTinTuc;
            return View(cakham);
        }

        // POST: Cakhams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cakham = await _context.Cakhams.FindAsync(id);
            if (cakham != null)
            {
                if (cakham.TrangThai == TrangThaiCaKham.Chưa_Đặt)
                {
                    _context.Cakhams.Remove(cakham);
                    TempData["SuccessMessage"] = "Xóa ca khám thành công.";
                    await _context.SaveChangesAsync();
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể xóa ca khám đã được đặt.";
                    return RedirectToAction(nameof(Index), new { id });
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy ca khám.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CakhamExists(int id)
        {
            return _context.Cakhams.Any(e => e.Id == id);
        }
    }
}
