using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AccountingTer.Models;
using AccountingTer.Services;

namespace AccountingTer.Controllers
{
    public class StringPropertiesController : Controller
    {
        private readonly ApplicationContext _context;

        public StringPropertiesController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: StringProperties
        public async Task<IActionResult> Index()
        {
              return View(await _context.StringProperties.ToListAsync());
        }

        // GET: StringProperties/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.StringProperties == null)
            {
                return NotFound();
            }

            var stringProperties = await _context.StringProperties
                .FirstOrDefaultAsync(m => m.Id == id);
            if (stringProperties == null)
            {
                return NotFound();
            }

            return View(stringProperties);
        }

        // GET: StringProperties/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: StringProperties/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Key,Value,Description")] StringProperties stringProperties)
        {
            if (ModelState.IsValid)
            {
                _context.Add(stringProperties);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(stringProperties);
        }

        // GET: StringProperties/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.StringProperties == null)
            {
                return NotFound();
            }

            var stringProperties = await _context.StringProperties.FindAsync(id);
            if (stringProperties == null)
            {
                return NotFound();
            }
            return View(stringProperties);
        }

        // POST: StringProperties/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Key,Value,Description")] StringProperties stringProperties)
        {
            if (id != stringProperties.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(stringProperties);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StringPropertiesExists(stringProperties.Id))
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
            return View(stringProperties);
        }

        // GET: StringProperties/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.StringProperties == null)
            {
                return NotFound();
            }

            var stringProperties = await _context.StringProperties
                .FirstOrDefaultAsync(m => m.Id == id);
            if (stringProperties == null)
            {
                return NotFound();
            }

            return View(stringProperties);
        }

        // POST: StringProperties/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.StringProperties == null)
            {
                return Problem("Entity set 'ApplicationContext.StringProperties'  is null.");
            }
            var stringProperties = await _context.StringProperties.FindAsync(id);
            if (stringProperties != null)
            {
                _context.StringProperties.Remove(stringProperties);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StringPropertiesExists(int id)
        {
          return _context.StringProperties.Any(e => e.Id == id);
        }
    }
}
