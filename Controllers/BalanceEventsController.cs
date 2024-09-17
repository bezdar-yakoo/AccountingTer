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
    public class BalanceEventsController : Controller
    {
        private readonly ApplicationContext _context;

        public BalanceEventsController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: BalanceEvents
        public async Task<IActionResult> Index()
        {
              return View(await _context.BalanceEvents.ToListAsync());
        }

        // GET: BalanceEvents/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.BalanceEvents == null)
            {
                return NotFound();
            }

            var balanceEvent = await _context.BalanceEvents
                .FirstOrDefaultAsync(m => m.Id == id);
            if (balanceEvent == null)
            {
                return NotFound();
            }

            return View(balanceEvent);
        }

        // GET: BalanceEvents/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BalanceEvents/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Value,Description,OwnerBalanceId,IsAdded,OwnerId,DateTime")] BalanceEvent balanceEvent)
        {
            if (ModelState.IsValid)
            {
                _context.Add(balanceEvent);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(balanceEvent);
        }

        // GET: BalanceEvents/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.BalanceEvents == null)
            {
                return NotFound();
            }

            var balanceEvent = await _context.BalanceEvents.FindAsync(id);
            if (balanceEvent == null)
            {
                return NotFound();
            }
            return View(balanceEvent);
        }

        // POST: BalanceEvents/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Value,Description,OwnerBalanceId,IsAdded,OwnerId,DateTime")] BalanceEvent balanceEvent)
        {
            if (id != balanceEvent.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(balanceEvent);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BalanceEventExists(balanceEvent.Id))
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
            return View(balanceEvent);
        }

        // GET: BalanceEvents/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.BalanceEvents == null)
            {
                return NotFound();
            }

            var balanceEvent = await _context.BalanceEvents
                .FirstOrDefaultAsync(m => m.Id == id);
            if (balanceEvent == null)
            {
                return NotFound();
            }

            return View(balanceEvent);
        }

        // POST: BalanceEvents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.BalanceEvents == null)
            {
                return Problem("Entity set 'ApplicationContext.BalanceEvents'  is null.");
            }
            var balanceEvent = await _context.BalanceEvents.FindAsync(id);
            if (balanceEvent != null)
            {
                _context.BalanceEvents.Remove(balanceEvent);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BalanceEventExists(int id)
        {
          return _context.BalanceEvents.Any(e => e.Id == id);
        }
    }
}
