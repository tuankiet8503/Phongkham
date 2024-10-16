using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Phongkham.Data;
using Phongkham.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Phongkham.Controllers
{
    public class DatlichKhamsController : Controller
    {
        private readonly ApplicationDBcontext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DatlichKhamsController> _logger;

        public DatlichKhamsController(ApplicationDBcontext context, UserManager<ApplicationUser> userManager, ILogger<DatlichKhamsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> XemChiTiet(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lichKham = await _context.lichKhams
                                         .Include(lk => lk.Clichkham)
                                         .FirstOrDefaultAsync(lk => lk.Id == id);

            if (lichKham == null)
            {
                return NotFound();
            }

            return View(lichKham);
        }

        public IActionResult Index()
        {
            var lichKhams = _context.lichKhams
                                    .Include(lk => lk.cakham)
                                        .ThenInclude(ck => ck.KhungGio)
                                    .ToList();
            return View(lichKhams);
        }

        public IActionResult TaolichKham()
        {
            var chuyenMons = _context.Chuyenmons.ToList();
            ViewBag.ChuyenMons = chuyenMons;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaolichKham(lichKham model, int? selectedChuyenMonId, string selectedNhaSiId)
        {
            ViewBag.ChuyenMons = _context.Chuyenmons.ToList();
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                TempData["ErrorMessage"] = "Không thể tìm thấy thông tin người dùng.";
                return View(model);
            }

            model.UserName = currentUser.FullName;
            model.PatientId = currentUser.Id;

            if (model.NgayDat == null)
            {
                TempData["ErrorMessage"] = "Không thể đặt lịch. Vui lòng chọn ngày và giờ khám.";
                return View(model);
            }

            model.NgayDat = model.NgayDat.ToLocalTime();

            try
            {
                if (!selectedChuyenMonId.HasValue)
                {
                    TempData["ErrorMessage"] = "Vui lòng chọn chuyên môn.";
                    return View(model);
                }
                if (string.IsNullOrEmpty(selectedNhaSiId))
                {
                    TempData["ErrorMessage"] = "Vui lòng chọn nha sĩ.";
                    return View(model);
                }

                var caKham = await _context.Cakhams
                                            .Include(ck => ck.KhungGio)
                                            .Include(ck => ck.Dentist)
                                            .FirstOrDefaultAsync(ck => ck.Id == model.CakhamId);

                if (caKham == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin về ca khám.";
                    return View(model);
                }
                caKham.TrangThai = TrangThaiCaKham.Đã_Đặt;
                _context.Update(caKham);

                var tongGiaTien = caKham.Gia;
                var ctlichKham = new CTlichkham
                {
                    TongGiaTien = tongGiaTien.ToString(),
                    LichkhamId = model.Id,
                    TenNhaSi = caKham.Dentist.FullName,
                    TenBenhNhan = currentUser.FullName
                };

                model.Clichkham = new List<CTlichkham> { ctlichKham };
                _context.lichKhams.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đặt lịch thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving appointment.");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi trong quá trình đặt lịch. Vui lòng thử lại sau.";
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult GetNhaSisByChuyenMon(int? selectedChuyenMonId)
        {
            if (selectedChuyenMonId.HasValue)
            {
                var nhaSis = _context.cTnhasis
                                     .Include(ns => ns.User)
                                     .Where(ns => ns.chuyenmonId == selectedChuyenMonId.Value)
                                     .Select(ns => new {
                                         UserId = ns.UserId,
                                         FullName = ns.User.FullName
                                     })
                                     .ToList();
                return Json(nhaSis);
            }
            return NotFound();
        }

        [HttpGet]
        public IActionResult GetCaKhamsByNhaSi(string selectedNhaSiId)
        {
            if (!string.IsNullOrEmpty(selectedNhaSiId))
            {
                var caKhams = _context.Cakhams
                                      .Where(ck => ck.DentistId == selectedNhaSiId && ck.TrangThai == TrangThaiCaKham.Chưa_Đặt)
                                      .Join(_context.KhungGios,
                                            ck => ck.KhungGioId,
                                            kg => kg.Id,
                                            (ck, kg) => new
                                            {
                                                Id = ck.Id,
                                                CaKham = kg.TimeSlot
                                            })
                                      .ToList();

                if (caKhams.Any())
                {
                    return Json(caKhams);
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                return NotFound();
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetGiaByCaKham(int? selectedCaKhamId)
        {
            if (selectedCaKhamId.HasValue)
            {
                var caKham = await _context.Cakhams.FindAsync(selectedCaKhamId.Value);
                if (caKham != null)
                {
                    return Json(caKham.Gia);
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                return NotFound();
            }
        }
    }
}
