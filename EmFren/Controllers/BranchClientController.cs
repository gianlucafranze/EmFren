using EmFren.Data;
using EmFren.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmFren.Controllers
{
    public class BranchClientController : Controller
    {
        private readonly EmFrenDbContext _context;

        public BranchClientController(EmFrenDbContext context)
        {
            _context = context;
        }


        // =========================================================
        // INDEX
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string sort = "id_asc",
            int page = 1)
        {
            if (!IsEnterpriseUser())
                return RedirectToAction("Login", "Users");

            const int pageSize = 10;

            IQueryable<BranchClient> query =
                _context.BranchClients.AsNoTracking();


            // =====================================================
            // SEARCH
            // =====================================================

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(b =>
                    b.CountryName.Contains(search) ||
                    b.City.Contains(search) ||
                    b.Id.ToString().Contains(search));
            }


            // =====================================================
            // SORT
            // =====================================================

            query = sort switch
            {
                "id_desc" =>
                    query.OrderByDescending(b => b.Id),

                "country_asc" =>
                    query.OrderBy(b => b.CountryName),

                "country_desc" =>
                    query.OrderByDescending(b => b.CountryName),

                "city_asc" =>
                    query.OrderBy(b => b.City),

                "city_desc" =>
                    query.OrderByDescending(b => b.City),

                _ =>
                    query.OrderBy(b => b.Id)
            };


            // =====================================================
            // PAGINATION
            // =====================================================

            int totalBranches = await query.CountAsync();

            int totalPages =
                (int)Math.Ceiling(totalBranches / (double)pageSize);

            if (totalPages == 0)
                totalPages = 1;

            if (page < 1)
                page = 1;

            if (page > totalPages)
                page = totalPages;


            var branches = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            // =====================================================
            // VIEWBAG
            // =====================================================

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalBranches = totalBranches;

            ViewBag.Search = search;
            ViewBag.Sort = sort;

            ViewBag.Id = HttpContext.Session.GetString("Id");
            ViewBag.Username =
                HttpContext.Session.GetString("Username");
            ViewBag.Email =
                HttpContext.Session.GetString("Email");


            return View("Index", branches);
        }


        // =========================================================
        // CREATE
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            BranchClient model,
            string? search,
            string sort = "id_asc",
            int page = 1)
        {
            if (!IsEnterpriseUser())
                return RedirectToAction("Login", "Users");


            model.CountryName =
                model.CountryName?.Trim() ?? string.Empty;

            model.City =
                model.City?.Trim() ?? string.Empty;


            if (string.IsNullOrWhiteSpace(model.CountryName))
            {
                TempData["Error"] =
                    "El país es obligatorio.";

                return RedirectToAction(
                    "Index",
                    new { search, sort, page });
            }


            if (string.IsNullOrWhiteSpace(model.City))
            {
                TempData["Error"] =
                    "La ciudad es obligatoria.";

                return RedirectToAction(
                    "Index",
                    new { search, sort, page });
            }


            // =====================================================
            // DUPLICATE CITY
            // =====================================================

            bool cityExists =
                await _context.BranchClients
                    .AnyAsync(b =>
                        b.City.ToLower() ==
                        model.City.ToLower());


            if (cityExists)
            {
                TempData["Error"] =
                    "Ya existe una sucursal para esa ciudad.";

                return RedirectToAction(
                    "Index",
                    new { search, sort, page });
            }


            _context.BranchClients.Add(model);

            await _context.SaveChangesAsync();


            TempData["Success"] =
                "Sucursal creada correctamente.";


            return RedirectToAction(
                "Index",
                new { search, sort, page });
        }


        // =========================================================
        // EDIT
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            BranchClient model,
            string? search,
            string sort = "id_asc",
            int page = 1)
        {
            if (!IsEnterpriseUser())
                return RedirectToAction("Login", "Users");


            model.CountryName =
                model.CountryName?.Trim() ?? string.Empty;

            model.City =
                model.City?.Trim() ?? string.Empty;


            if (string.IsNullOrWhiteSpace(model.CountryName))
            {
                TempData["Error"] =
                    "El país es obligatorio.";

                return RedirectToAction(
                    "Index",
                    new { search, sort, page });
            }


            if (string.IsNullOrWhiteSpace(model.City))
            {
                TempData["Error"] =
                    "La ciudad es obligatoria.";

                return RedirectToAction(
                    "Index",
                    new { search, sort, page });
            }


            // =====================================================
            // DUPLICATE CITY
            // =====================================================

            bool cityExists =
                await _context.BranchClients
                    .AnyAsync(b =>
                        b.Id != model.Id &&
                        b.City.ToLower() ==
                        model.City.ToLower());


            if (cityExists)
            {
                TempData["Error"] =
                    "Ya existe otra sucursal para esa ciudad.";

                return RedirectToAction(
                    "Index",
                    new { search, sort, page });
            }


            var branch =
                await _context.BranchClients
                    .FirstOrDefaultAsync(b =>
                        b.Id == model.Id);


            if (branch == null)
            {
                TempData["Error"] =
                    "La sucursal no fue encontrada.";

                return RedirectToAction(
                    "Index",
                    new { search, sort, page });
            }


            branch.CountryName = model.CountryName;
            branch.City = model.City;


            await _context.SaveChangesAsync();


            TempData["Success"] =
                "Sucursal actualizada correctamente.";


            return RedirectToAction(
                "Index",
                new { search, sort, page });
        }


        // =========================================================
        // DELETE
        // =========================================================

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


            var branch =
                await _context.BranchClients
                    .FirstOrDefaultAsync(b =>
                        b.Id == id);


            if (branch == null)
            {
                TempData["Error"] =
                    "La sucursal no fue encontrada.";

                return RedirectToAction(
                    "Index",
                    new { search, sort, page });
            }


            _context.BranchClients.Remove(branch);


            try
            {
                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Sucursal eliminada correctamente.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "No se puede eliminar esta sucursal porque está siendo utilizada por otros registros.";
            }


            return RedirectToAction(
                "Index",
                new { search, sort, page });
        }


        // =========================================================
        // ENTERPRISE USER CHECK
        // =========================================================

        private bool IsEnterpriseUser()
        {
            string? fromEnterprise =
                HttpContext.Session
                    .GetString("FromEnterprise");

            return bool.TryParse(
                       fromEnterprise,
                       out bool result)
                   && result;
        }
    }
}