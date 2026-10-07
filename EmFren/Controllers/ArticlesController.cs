using EmFren.Data;
using EmFren.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmFren.Controllers
{
    public class ArticlesController : Controller
    {
        private readonly EmFrenDbContext _context;

        public ArticlesController(EmFrenDbContext context)
        {
            _context = context;
        }


        // =========================================================
        // INDEX
        // =========================================================

        public async Task<IActionResult> Index(
            string search = "",
            string sort = "id_asc",
            int page = 1)
        {
            // -----------------------------------------------------
            // ENTERPRISE SESSION CHECK
            // -----------------------------------------------------

            var id = HttpContext.Session.GetString("Id");
            var username = HttpContext.Session.GetString("Username");
            var email = HttpContext.Session.GetString("Email");
            var fromEnterprise = HttpContext.Session.GetString("FromEnterprise");

            if (string.IsNullOrEmpty(id) ||
                string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(email) ||
                fromEnterprise != "True")
            {
                return RedirectToAction("Login", "Users");
            }


            // -----------------------------------------------------
            // QUERY
            // -----------------------------------------------------

            IQueryable<Article> query = _context.Articles
                .Include(a => a.Vehicle);


            // -----------------------------------------------------
            // SEARCH
            // -----------------------------------------------------

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(a =>
                    (a.ArticleId != null &&
                     a.ArticleId.Contains(search)) ||

                    (a.Vehicle != null &&
                     a.Vehicle.Brand != null &&
                     a.Vehicle.Brand.Contains(search)) ||

                    (a.Measure != null &&
                     a.Measure.Contains(search)) ||

                    (a.Type != null &&
                     a.Type.Contains(search)) ||

                    (a.Friction != null &&
                     a.Friction.Contains(search)) ||

                    a.Id.ToString().Contains(search)
                );
            }


            // -----------------------------------------------------
            // SORT
            // -----------------------------------------------------

            query = sort switch
            {
                "id_desc" =>
                    query.OrderByDescending(a => a.Id),

                "article_id_asc" =>
                    query.OrderBy(a => a.ArticleId),

                "article_id_desc" =>
                    query.OrderByDescending(a => a.ArticleId),

                "vehicle_asc" =>
                    query.OrderBy(a => a.Vehicle!.Brand),

                "vehicle_desc" =>
                    query.OrderByDescending(a => a.Vehicle!.Brand),

                "price_asc" =>
                    query.OrderBy(a => a.Price),

                "price_desc" =>
                    query.OrderByDescending(a => a.Price),

                "stock_asc" =>
                    query.OrderBy(a => a.Stock),

                "stock_desc" =>
                    query.OrderByDescending(a => a.Stock),

                "discount_asc" =>
                    query.OrderBy(a => a.Discount),

                "discount_desc" =>
                    query.OrderByDescending(a => a.Discount),

                _ =>
                    query.OrderBy(a => a.Id)
            };


            // -----------------------------------------------------
            // PAGINATION
            // -----------------------------------------------------

            int pageSize = 10;

            int totalArticles = await query.CountAsync();

            int totalPages = (int)Math.Ceiling(
                totalArticles / (double)pageSize
            );

            if (page < 1)
                page = 1;

            if (totalPages > 0 && page > totalPages)
                page = totalPages;


            var articles = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            // -----------------------------------------------------
            // VEHICLES FOR CREATE / EDIT
            // -----------------------------------------------------

            ViewBag.Vehicles = await _context.Vehicles
                .OrderBy(v => v.Brand)
                .ToListAsync();


            // -----------------------------------------------------
            // VIEWBAGS
            // -----------------------------------------------------

            ViewBag.Username = username;

            ViewBag.Search = search;
            ViewBag.Sort = sort;

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            ViewBag.TotalArticles = totalArticles;


            return View(articles);
        }


        // =========================================================
        // CREATE
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Article article)
        {
            if (!IsEnterpriseUser())
                return RedirectToAction("Login", "Users");


            if (!ModelState.IsValid)
            {
                TempData["Error"] =
                    "Los datos ingresados no son válidos.";

                return RedirectToAction(nameof(Index));
            }


            // -----------------------------------------------------
            // DEFAULT VALUES
            // -----------------------------------------------------

            if (article.Stock == null)
                article.Stock = true;

            if (article.Discount == null)
                article.Discount = 0m;


            // -----------------------------------------------------
            // CHECK VEHICLE
            // -----------------------------------------------------

            if (article.VehicleId.HasValue)
            {
                var vehicleExists = await _context.Vehicles
                    .AnyAsync(v => v.Id == article.VehicleId.Value);

                if (!vehicleExists)
                {
                    TempData["Error"] =
                        "El vehículo seleccionado no existe.";

                    return RedirectToAction(nameof(Index));
                }
            }


            // -----------------------------------------------------
            // CREATE
            // -----------------------------------------------------

            _context.Articles.Add(article);

            await _context.SaveChangesAsync();


            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // EDIT
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Article article)
        {
            if (!IsEnterpriseUser())
                return RedirectToAction("Login", "Users");


            // -----------------------------------------------------
            // FIND EXISTING ARTICLE
            // -----------------------------------------------------

            var existingArticle = await _context.Articles
                .FirstOrDefaultAsync(a => a.Id == article.Id);


            if (existingArticle == null)
                return NotFound();


            // -----------------------------------------------------
            // CHECK VEHICLE
            // -----------------------------------------------------

            if (article.VehicleId.HasValue)
            {
                var vehicleExists = await _context.Vehicles
                    .AnyAsync(v => v.Id == article.VehicleId.Value);

                if (!vehicleExists)
                {
                    TempData["Error"] =
                        "El vehículo seleccionado no existe.";

                    return RedirectToAction(nameof(Index));
                }
            }


            // -----------------------------------------------------
            // UPDATE
            // -----------------------------------------------------

            existingArticle.ArticleId = article.ArticleId;
            existingArticle.VehicleId = article.VehicleId;
            existingArticle.Measure = article.Measure;
            existingArticle.Type = article.Type;
            existingArticle.Friction = article.Friction;
            existingArticle.Price = article.Price;
            existingArticle.Stock = article.Stock;
            existingArticle.Discount = article.Discount;


            await _context.SaveChangesAsync();


            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // DELETE
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsEnterpriseUser())
                return RedirectToAction("Login", "Users");


            var article = await _context.Articles
                .FirstOrDefaultAsync(a => a.Id == id);


            if (article == null)
                return NotFound();


            _context.Articles.Remove(article);

            await _context.SaveChangesAsync();


            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // TOGGLE STOCK
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStock(int id)
        {
            if (!IsEnterpriseUser())
                return RedirectToAction("Login", "Users");


            var article = await _context.Articles
                .FirstOrDefaultAsync(a => a.Id == id);


            if (article == null)
                return NotFound();


            article.Stock = !(article.Stock ?? false);


            await _context.SaveChangesAsync();


            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // ENTERPRISE SESSION CHECK
        // =========================================================

        private bool IsEnterpriseUser()
        {
            var id = HttpContext.Session.GetString("Id");
            var username = HttpContext.Session.GetString("Username");
            var email = HttpContext.Session.GetString("Email");
            var fromEnterprise = HttpContext.Session.GetString("FromEnterprise");

            return !string.IsNullOrEmpty(id) &&
                   !string.IsNullOrEmpty(username) &&
                   !string.IsNullOrEmpty(email) &&
                   fromEnterprise == "True";
        }
    }
}