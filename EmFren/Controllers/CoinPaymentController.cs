using EmFren.Data;
using EmFren.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Utilities.Zlib;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;

namespace EmFren.Controllers
{
    public class CoinPaymentController : Controller
    {
        private readonly EmFrenDbContext _context;

        public CoinPaymentController(EmFrenDbContext context)
        {
            _context = context;
        }


        // ============================================================
        // INDEX
        // SEARCH + SORT + PAGINATION + ENTERPRISE AUTHORIZATION
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string sort = "id_asc",
            int page = 1)
        {
            // ========================================================
            // CHECK SESSION
            // ========================================================

            int? id = HttpContext.Session.GetInt32("Id");

            string? username =
                HttpContext.Session.GetString("Username");

            string? email =
                HttpContext.Session.GetString("Email");

            string? fromEnterprise =
                HttpContext.Session.GetString("FromEnterprise");


            // ========================================================
            // NO VALID SESSION
            // ========================================================

            if (id == null ||
                string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(fromEnterprise))
            {
                return RedirectToAction("Login", "Users");
            }


            // ========================================================
            // VERIFY ENTERPRISE ACCOUNT
            // ========================================================

            if (!bool.TryParse(fromEnterprise, out bool isEnterprise) ||
                !isEnterprise)
            {
                return RedirectToAction("Login", "Users");
            }


            // ========================================================
            // PAGINATION
            // ========================================================

            const int pageSize = 10;

            if (page < 1)
            {
                page = 1;
            }


            // ========================================================
            // BASE QUERY
            // ========================================================

            var query = _context.CoinPayments
                .AsQueryable();


            // ========================================================
            // SEARCH BY ID OR COIN NAME
            // ========================================================

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(c =>
                    c.CoinName.Contains(search) ||
                    c.Id.ToString().Contains(search));
            }


            // ========================================================
            // SORT
            // ========================================================

            switch (sort)
            {
                // ID ASCENDING
                case "id_asc":

                    query = query.OrderBy(c => c.Id);

                    break;


                // ID DESCENDING
                case "id_desc":

                    query = query.OrderByDescending(c => c.Id);

                    break;


                // COIN NAME ASCENDING
                case "name_asc":

                    query = query.OrderBy(c => c.CoinName);

                    break;


                // COIN NAME DESCENDING
                case "name_desc":

                    query = query.OrderByDescending(c => c.CoinName);

                    break;


                // DEFAULT
                default:

                    sort = "id_asc";

                    query = query.OrderBy(c => c.Id);

                    break;
            }


            // ========================================================
            // TOTAL RECORDS
            // ========================================================

            var totalCoins = await query.CountAsync();


            // ========================================================
            // TOTAL PAGES
            // ========================================================

            var totalPages = (int)Math.Ceiling(
                totalCoins / (double)pageSize
            );


            // ========================================================
            // MAKE SURE PAGE EXISTS
            // ========================================================

            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
            }


            // ========================================================
            // GET CURRENT PAGE
            // ========================================================

            var coins = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            // ========================================================
            // VIEWBAG - PAGINATION
            // ========================================================

            ViewBag.CurrentPage = page;

            ViewBag.TotalPages = totalPages;

            ViewBag.TotalCoins = totalCoins;


            // ========================================================
            // VIEWBAG - SEARCH AND SORT
            // ========================================================

            ViewBag.Search = search;

            ViewBag.Sort = sort;


            // ========================================================
            // VIEWBAG - LOGGED USER
            // ========================================================

            ViewBag.Id = id;

            ViewBag.Username = username;

            ViewBag.Email = email;


            // ========================================================
            // RETURN VIEW
            // ========================================================

            return View("Index", coins);
        }


        // ============================================================
        // CREATE
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CoinPayment model,
            string? search,
            string? sort,
            int page = 1)
        {
            // ========================================================
            // CHECK ENTERPRISE SESSION
            // ========================================================

            if (!IsEnterpriseUser())
            {
                return RedirectToAction("Login", "Users");
            }


            // ========================================================
            // VALIDATE MODEL
            // ========================================================

            if (!ModelState.IsValid)
            {
                TempData["Error"] =
                    "Los datos de la moneda no son válidos.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        search,
                        sort,
                        page
                    });
            }


            // ========================================================
            // CHECK DUPLICATE COIN NAME
            // ========================================================

            string coinName = model.CoinName.Trim();

            bool exists = await _context.CoinPayments
                .AnyAsync(c =>
                    c.CoinName.ToLower() ==
                    coinName.ToLower());

            if (exists)
            {
                TempData["Error"] =
                    "Esta moneda ya está registrada.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        search,
                        sort,
                        page
                    });
            }


            // ========================================================
            // CREATE
            // ========================================================

            model.CoinName = coinName;

            _context.CoinPayments.Add(model);

            await _context.SaveChangesAsync();


            TempData["Success"] =
                "Moneda agregada correctamente.";


            return RedirectToAction(
                nameof(Index),
                new
                {
                    search,
                    sort,
                    page
                });
        }


        // ============================================================
        // EDIT
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            CoinPayment model,
            string? search,
            string? sort,
            int page = 1)
        {
            // ========================================================
            // CHECK ENTERPRISE SESSION
            // ========================================================

            if (!IsEnterpriseUser())
            {
                return RedirectToAction("Login", "Users");
            }


            // ========================================================
            // VALIDATE MODEL
            // ========================================================

            if (!ModelState.IsValid)
            {
                TempData["Error"] =
                    "Los datos de la moneda no son válidos.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        search,
                        sort,
                        page
                    });
            }


            // ========================================================
            // FIND COIN
            // ========================================================

            var coin = await _context.CoinPayments
                .FirstOrDefaultAsync(c => c.Id == model.Id);


            if (coin == null)
            {
                TempData["Error"] =
                    "La moneda no existe.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        search,
                        sort,
                        page
                    });
            }


            // ========================================================
            // CHECK DUPLICATE NAME
            // ========================================================

            string coinName = model.CoinName.Trim();

            bool exists = await _context.CoinPayments
                .AnyAsync(c =>
                    c.Id != model.Id &&
                    c.CoinName.ToLower() ==
                    coinName.ToLower());


            if (exists)
            {
                TempData["Error"] =
                    "Esta moneda ya está registrada.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        search,
                        sort,
                        page
                    });
            }


            // ========================================================
            // UPDATE
            // ========================================================

            coin.CoinName = coinName;

            await _context.SaveChangesAsync();


            TempData["Success"] =
                "Moneda actualizada correctamente.";


            return RedirectToAction(
                nameof(Index),
                new
                {
                    search,
                    sort,
                    page
                });
        }


        // ============================================================
        // DELETE
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id,
            string? search,
            string? sort,
            int page = 1)
        {
            // ========================================================
            // CHECK ENTERPRISE SESSION
            // ========================================================

            if (!IsEnterpriseUser())
            {
                return RedirectToAction("Login", "Users");
            }


            // ========================================================
            // FIND COIN
            // ========================================================

            var coin = await _context.CoinPayments
                .FirstOrDefaultAsync(c => c.Id == id);


            if (coin == null)
            {
                TempData["Error"] =
                    "La moneda no existe.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        search,
                        sort,
                        page
                    });
            }


            // ========================================================
            // DELETE
            // ========================================================

            _context.CoinPayments.Remove(coin);


            try
            {
                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Moneda eliminada correctamente.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "No se puede eliminar esta moneda porque está siendo utilizada en otros registros.";
            }


            return RedirectToAction(
                nameof(Index),
                new
                {
                    search,
                    sort,
                    page
                });
        }


        // ============================================================
        // ENTERPRISE SESSION CHECK
        // ============================================================

        private bool IsEnterpriseUser()
        {
            int? id =
                HttpContext.Session.GetInt32("Id");

            string? username =
                HttpContext.Session.GetString("Username");

            string? email =
                HttpContext.Session.GetString("Email");

            string? fromEnterprise =
                HttpContext.Session.GetString("FromEnterprise");


            // No session
            if (id == null ||
                string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(fromEnterprise))
            {
                return false;
            }


            // Check FromEnterprise
            if (!bool.TryParse(
                    fromEnterprise,
                    out bool isEnterprise))
            {
                return false;
            }


            return isEnterprise;
        }
    }
}