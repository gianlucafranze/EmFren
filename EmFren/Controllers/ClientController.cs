using EmFren.Data;
using EmFren.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmFren.Controllers
{
    public class ClientController : Controller
    {
        private readonly EmFrenDbContext _context;

        public ClientController(EmFrenDbContext context)
        {
            _context = context;
        }

        private bool IsEnterpriseUser()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("Id"))
                && !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"))
                && !string.IsNullOrEmpty(HttpContext.Session.GetString("Email"))
                && HttpContext.Session.GetString("FromEnterprise") == "True";
        }

        public async Task<IActionResult> Index(
            string? search,
            string sort = "id_asc",
            int page = 1)
        {
            if (!IsEnterpriseUser())
                return RedirectToAction("Login", "Users");

            const int pageSize = 10;

            var query = _context.Clients
                .Include(c => c.BranchClient)
                .AsQueryable();

            // SEARCH
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(c =>
                    c.Id.ToString().Contains(search) ||
                    c.Name.Contains(search) ||
                    (c.DocumentNumber != null &&
                     c.DocumentNumber.Contains(search)) ||
                    (c.Email != null &&
                     c.Email.Contains(search)) ||
                    (c.CellphoneNumber != null &&
                     c.CellphoneNumber.Contains(search)) ||
                    (c.BranchClient != null &&
                     c.BranchClient.CountryName.Contains(search)) ||
                    (c.BranchClient != null &&
                     c.BranchClient.City.Contains(search))
                );
            }

            // SORT
            query = sort switch
            {
                "id_desc" => query.OrderByDescending(c => c.Id),

                "name_asc" => query.OrderBy(c => c.Name),

                "name_desc" => query.OrderByDescending(c => c.Name),

                "branch_asc" => query.OrderBy(c =>
                    c.BranchClient != null
                        ? c.BranchClient.CountryName
                        : ""),

                "branch_desc" => query.OrderByDescending(c =>
                    c.BranchClient != null
                        ? c.BranchClient.CountryName
                        : ""),

                _ => query.OrderBy(c => c.Id)
            };

            var totalClients = await query.CountAsync();

            var totalPages = (int)Math.Ceiling(
                totalClients / (double)pageSize);

            if (totalPages > 0 && page > totalPages)
                page = totalPages;

            if (page < 1)
                page = 1;

            var clients = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalClients = totalClients;
            ViewBag.Search = search;
            ViewBag.Sort = sort;

            ViewBag.BranchClients = await _context.BranchClients
                .OrderBy(b => b.CountryName)
                .ThenBy(b => b.City)
                .ToListAsync();

            ViewBag.Username =
                HttpContext.Session.GetString("Username");

            return View(clients);
        }

        // CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Client client,
            string? search,
            string sort = "id_asc",
            int page = 1)
        {
            if (!IsEnterpriseUser())
                return RedirectToAction("Login", "Users");

            if (ModelState.IsValid)
            {
                _context.Clients.Add(client);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new
            {
                search,
                sort,
                page
            });
        }

        // EDIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            Client client,
            string? search,
            string sort = "id_asc",
            int page = 1)
        {
            if (!IsEnterpriseUser())
                return RedirectToAction("Login", "Users");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Clients.Update(client);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Clients
                        .AnyAsync(c => c.Id == client.Id))
                    {
                        return NotFound();
                    }

                    throw;
                }
            }

            return RedirectToAction(nameof(Index), new
            {
                search,
                sort,
                page
            });
        }

        // DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id,
            string? search,
            string sort = "id_asc",
            int page = 1)
        {
            if (!IsEnterpriseUser())
                return RedirectToAction("Login", "Users");

            var client = await _context.Clients
                .FindAsync(id);

            if (client != null)
            {
                _context.Clients.Remove(client);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new
            {
                search,
                sort,
                page
            });
        }

        // TOGGLE ACTIVE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(
            int id,
            string? search,
            string sort = "id_asc",
            int page = 1)
        {
            if (!IsEnterpriseUser())
                return RedirectToAction("Login", "Users");

            var client = await _context.Clients
                .FindAsync(id);

            if (client != null)
            {
                client.ClientActive =
                    !(client.ClientActive ?? false);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new
            {
                search,
                sort,
                page
            });
        }
    }
}