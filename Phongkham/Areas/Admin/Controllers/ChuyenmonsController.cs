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

namespace Phongkham.Areas.Admin.Controllers {
    [Authorize(Roles = "Admin")]
    public class ChuyenmonsController : Controller
    {
        private readonly ApplicationDBcontext _context;

        public ChuyenmonsController(ApplicationDBcontext context)
        {
            _context = context;
        }

        // GET: Chuyenmons
        public async Task<IActionResult> Index()
        {
            return View(await _context.Chuyenmons.ToListAsync());
        }

        // GET: Chuyenmons/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chuyenmon = await _context.Chuyenmons
                .FirstOrDefaultAsync(m => m.Id == id);
            if (chuyenmon == null)
            {
                return NotFound();
            }

            return View(chuyenmon);
        }

        // GET: Chuyenmons/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Chuyenmons/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?linkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Chuyenmon chuyenmon)
        {
            if (ModelState.IsValid)
            {
                _context.Add(chuyenmon);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(chuyenmon);
        }

        // GET: Chuyenmons/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chuyenmon = await _context.Chuyenmons.FindAsync(id);
            if (chuyenmon == null)
            {
                return NotFound();
            }
            return View(chuyenmon);
        }

        // POST: Chuyenmons/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?linkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Chuyenmon chuyenmon)
        {
            if (id != chuyenmon.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chuyenmon);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChuyenmonExists(chuyenmon.Id))
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
            return View(chuyenmon);
        }

        // GET: Chuyenmons/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chuyenmon = await _context.Chuyenmons
                .FirstOrDefaultAsync(m => m.Id == id);
            if (chuyenmon == null)
            {
                return NotFound();
            }

            return View(chuyenmon);
        }

        // POST: Chuyenmons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var chuyenmon = await _context.Chuyenmons.FindAsync(id);
            if (chuyenmon != null)
            {
                _context.Chuyenmons.Remove(chuyenmon);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ChuyenmonExists(int id)
        {
            return _context.Chuyenmons.Any(e => e.Id == id);
        }
    }
}
