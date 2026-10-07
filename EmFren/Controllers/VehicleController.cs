using EmFren.Data;
using EmFren.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmFren.Controllers
{
    public class VehicleController : Controller
    {
        private readonly EmFrenDbContext _context;

        public VehicleController(EmFrenDbContext context)
        {
            _context = context;
        }


        // =========================================================
        // INDEX
        // =========================================================

        public IActionResult Index(
            string? search,
            string sort = "id_asc",
            int page = 1)
        {
            // Check enterprise session
            var userId = HttpContext.Session.GetString("Id");
            var fromEnterprise = HttpContext.Session.GetString("FromEnterprise");

            if (string.IsNullOrEmpty(userId) ||
                fromEnterprise != "True")
            {
                return RedirectToAction("Login", "Users");
            }

            // Get username for the header
            ViewBag.Username = HttpContext.Session.GetString("Username");

            // Your existing vehicle code...

            // Example:
            var vehicles = _context.Vehicles.AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                if (int.TryParse(search, out int id))
                {
                    vehicles = vehicles.Where(v =>
                        v.Id == id ||
                        v.Brand.Contains(search));
                }
                else
                {
                    vehicles = vehicles.Where(v =>
                        v.Brand.Contains(search));
                }
            }

            // Sorting
            vehicles = sort switch
            {
                "id_desc" => vehicles.OrderByDescending(v => v.Id),

                "brand_asc" => vehicles.OrderBy(v => v.Brand),

                "brand_desc" => vehicles.OrderByDescending(v => v.Brand),

                _ => vehicles.OrderBy(v => v.Id)
            };

            // Total
            int totalVehicles = vehicles.Count();

            // Pagination
            int pageSize = 10;

            int totalPages =
                (int)Math.Ceiling(
                    totalVehicles / (double)pageSize);

            if (page < 1)
                page = 1;

            if (totalPages > 0 && page > totalPages)
                page = totalPages;

            var result = vehicles
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Search = search;
            ViewBag.Sort = sort;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalVehicles = totalVehicles;

            return View(result);
        }


        // =========================================================
        // CREATE
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Vehicle vehicle)
        {
            // ENTERPRISE ACCESS
            var userId = HttpContext.Session.GetString("Id");
            var fromEnterprise = HttpContext.Session.GetString("FromEnterprise");

            if (string.IsNullOrEmpty(userId) ||
                fromEnterprise != "True")
            {
                return RedirectToAction("Login", "Users");
            }


            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }


            _context.Vehicles.Add(vehicle);

            await _context.SaveChangesAsync();


            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // EDIT
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Vehicle vehicle)
        {
            // ENTERPRISE ACCESS
            var userId = HttpContext.Session.GetString("Id");
            var fromEnterprise = HttpContext.Session.GetString("FromEnterprise");

            if (string.IsNullOrEmpty(userId) ||
                fromEnterprise != "True")
            {
                return RedirectToAction("Login", "Users");
            }


            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }


            var existingVehicle =
                await _context.Vehicles.FindAsync(vehicle.Id);


            if (existingVehicle == null)
            {
                return NotFound();
            }


            existingVehicle.Brand = vehicle.Brand;


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
            // ENTERPRISE ACCESS
            var userId = HttpContext.Session.GetString("Id");
            var fromEnterprise = HttpContext.Session.GetString("FromEnterprise");

            if (string.IsNullOrEmpty(userId) ||
                fromEnterprise != "True")
            {
                return RedirectToAction("Login", "Users");
            }


            var vehicle =
                await _context.Vehicles.FindAsync(id);


            if (vehicle == null)
            {
                return NotFound();
            }


            _context.Vehicles.Remove(vehicle);

            await _context.SaveChangesAsync();


            return RedirectToAction(nameof(Index));
        }
    }
}