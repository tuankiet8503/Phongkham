using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Phongkham.Data;
using Phongkham.Models;

namespace Phongkham.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TintucsController : Controller
    {

        private readonly ApplicationDBcontext _context;

        public TintucsController(ApplicationDBcontext context)
        {
            _context = context;
        }

        // GET: Tintucs
        public async Task<IActionResult> Index()
        {
            var applicationDBcontext = _context.Tintucs.Include(t => t.Loaitintuc);
            return View(await applicationDBcontext.ToListAsync());
        }

        // GET: Tintucs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tintuc = await _context.Tintucs
                .Include(t => t.Loaitintuc)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tintuc == null)
            {
                return NotFound();
            }

            return View(tintuc);
        }

        // GET: Tintucs/Create
        public IActionResult Create()
        {
            ViewData["LoaitintucId"] = new SelectList(_context.Loaitintucs, "Id", "Name");
            return View();
        }

        // POST: Tintucs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?linkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,tieude,Noidung,NgayDang,ImageUrl,LoaitintucId")] Tintuc tintuc, IFormFile imageUrl)
        {
            if (ModelState.IsValid)
            {
                if (imageUrl != null)
                {
                    // Lưu hình ảnh đại diện tham khảo bài 02 hàm SaveImage
                    tintuc.ImageUrl = await SaveImage(imageUrl);
                }

                _context.Add(tintuc);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["LoaitintucId"] = new SelectList(_context.Loaitintucs, "Id", "Name", tintuc.LoaitintucId);
            return View(tintuc);
        }
        private async Task<string> SaveImage(IFormFile image)
        {
            var savePath = Path.Combine("wwwroot/images", image.FileName); //

            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return "/images/" + image.FileName; // Trả về đường dẫn tương đối
        }
        // GET: Tintucs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tintuc = await _context.Tintucs.FindAsync(id);
            if (tintuc == null)
            {
                return NotFound();
            }
            ViewData["LoaitintucId"] = new SelectList(_context.Loaitintucs, "Id", "Name", tintuc.LoaitintucId);
            return View(tintuc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,tieude,Noidung,NgayDang,ImageUrl,LoaitintucId")] Tintuc tintuc, IFormFile imageUrl)
        {
            if (id != tintuc.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageUrl != null)
                    {

                        tintuc.ImageUrl = await SaveImage(imageUrl);
                    }
                    _context.Update(tintuc);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TintucExists(tintuc.Id))
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
            ViewData["LoaitintucId"] = new SelectList(_context.Loaitintucs, "Id", "Name", tintuc.LoaitintucId);
            return View(tintuc);
        }

        // GET: Tintucs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tintuc = await _context.Tintucs
                .Include(t => t.Loaitintuc)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tintuc == null)
            {
                return NotFound();
            }

            return View(tintuc);
        }

        // POST: Tintucs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tintuc = await _context.Tintucs.FindAsync(id);
            if (tintuc != null)
            {
                _context.Tintucs.Remove(tintuc);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TintucExists(int id)
        {
            return _context.Tintucs.Any(e => e.Id == id);
        }
    }
}
