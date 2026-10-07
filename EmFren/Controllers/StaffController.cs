using EmFren.Data;
using EmFren.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmFren.Controllers
{
    public class StaffController : Controller
    {
        private readonly EmFrenDbContext _context;

        public StaffController(EmFrenDbContext context)
        {
            _context = context;
        }


        // =========================================================
        // INDEX
        // =========================================================

        public async Task<IActionResult> Index(
            string? search,
            string sort = "id_asc",
            int page = 1)
        {
            // =====================================================
            // ENTERPRISE ACCESS
            // =====================================================

            var sessionId = HttpContext.Session.GetString("Id");
            var sessionUsername = HttpContext.Session.GetString("Username");
            var sessionEmail = HttpContext.Session.GetString("Email");
            var sessionFromEnterprise =
                HttpContext.Session.GetString("FromEnterprise");

            if (string.IsNullOrEmpty(sessionId) ||
                string.IsNullOrEmpty(sessionUsername) ||
                string.IsNullOrEmpty(sessionEmail) ||
                sessionFromEnterprise != "True")
            {
                return RedirectToAction("Login", "Users");
            }


            // =====================================================
            // QUERY
            // =====================================================

            var query = _context.Staff
                .Include(s => s.Department)
                .AsQueryable();


            // =====================================================
            // SEARCH
            // =====================================================

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(s =>
                    // STAFF NAME
                    s.Name.Contains(search)

                    // DOCUMENT
                    || (s.DocumentNumber != null &&
                        s.DocumentNumber.Contains(search))

                    // EMAIL
                    || (s.Email != null &&
                        s.Email.Contains(search))

                    // CELLPHONE
                    || (s.CellphoneNumber != null &&
                        s.CellphoneNumber.Contains(search))

                    // DEPARTMENT NAME
                    || (s.Department != null &&
                        s.Department.DepartmentName.Contains(search))

                    // STAFF ID
                    || s.Id.ToString().Contains(search)
                );
            }


            // =====================================================
            // SORT
            // =====================================================

            query = sort switch
            {
                // ID
                "id_desc" =>
                    query.OrderByDescending(s => s.Id),

                // NAME
                "name_asc" =>
                    query.OrderBy(s => s.Name),

                "name_desc" =>
                    query.OrderByDescending(s => s.Name),

                // DEPARTMENT
                "department_asc" =>
                    query.OrderBy(s =>
                        s.Department != null
                            ? s.Department.DepartmentName
                            : ""),

                "department_desc" =>
                    query.OrderByDescending(s =>
                        s.Department != null
                            ? s.Department.DepartmentName
                            : ""),

                // DEFAULT
                _ =>
                    query.OrderBy(s => s.Id)
            };


            // =====================================================
            // PAGINATION
            // =====================================================

            int pageSize = 10;

            int totalStaff = await query.CountAsync();

            int totalPages =
                (int)Math.Ceiling(totalStaff / (double)pageSize);

            if (page < 1)
                page = 1;

            if (totalPages > 0 && page > totalPages)
                page = totalPages;


            // =====================================================
            // GET STAFF
            // =====================================================

            var staff = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            // =====================================================
            // VIEWBAGS
            // =====================================================

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalStaff = totalStaff;
            ViewBag.Search = search;
            ViewBag.Sort = sort;


            // =====================================================
            // LOAD DEPARTMENTS
            // =====================================================
            // Required by the searchable department selector
            // in the Create Staff modal.

            ViewBag.Departments = await _context.Departments
                .OrderBy(d => d.DepartmentName)
                .ToListAsync();


            return View(staff);
        }


        // =========================================================
        // CREATE - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!IsEnterpriseUser())
                return RedirectToAction("Login", "Users");

            ViewBag.Departments = await _context.Departments
                .OrderBy(d => d.DepartmentName)
                .ToListAsync();

            return View();
        }


        // =========================================================
        // CREATE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Staff staff)
        {
            if (!IsEnterpriseUser())
                return RedirectToAction("Login", "Users");


            // =====================================================
            // VALIDATION
            // =====================================================

            if (!ModelState.IsValid)
            {
                ViewBag.Departments = await _context.Departments
                    .OrderBy(d => d.DepartmentName)
                    .ToListAsync();

                return View(staff);
            }


            // =====================================================
            // CREATE STAFF
            // =====================================================

            _context.Staff.Add(staff);

            await _context.SaveChangesAsync();


            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // EDIT - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsEnterpriseUser())
                return RedirectToAction("Login", "Users");


            var staff = await _context.Staff
                .FirstOrDefaultAsync(s => s.Id == id);


            if (staff == null)
                return NotFound();


            ViewBag.Departments = await _context.Departments
                .OrderBy(d => d.DepartmentName)
                .ToListAsync();


            return View(staff);
        }


        // =========================================================
        // EDIT - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Staff staff)
        {
            if (!IsEnterpriseUser())
                return RedirectToAction("Login", "Users");


            // =====================================================
            // ID VALIDATION
            // =====================================================

            if (id != staff.Id)
                return NotFound();


            // =====================================================
            // MODEL VALIDATION
            // =====================================================

            if (!ModelState.IsValid)
            {
                ViewBag.Departments = await _context.Departments
                    .OrderBy(d => d.DepartmentName)
                    .ToListAsync();

                return View(staff);
            }


            // =====================================================
            // UPDATE
            // =====================================================

            try
            {
                _context.Staff.Update(staff);

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StaffExists(staff.Id))
                    return NotFound();

                throw;
            }


            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // DETAILS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (!IsEnterpriseUser())
                return RedirectToAction("Login", "Users");


            var staff = await _context.Staff
                .Include(s => s.Department)
                .FirstOrDefaultAsync(s => s.Id == id);


            if (staff == null)
                return NotFound();


            return View(staff);
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


            var staff = await _context.Staff
                .FirstOrDefaultAsync(s => s.Id == id);


            if (staff == null)
                return NotFound();


            _context.Staff.Remove(staff);

            await _context.SaveChangesAsync();


            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // CHANGE ACTIVE STATUS
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            if (!IsEnterpriseUser())
                return RedirectToAction("Login", "Users");


            var staff = await _context.Staff
                .FirstOrDefaultAsync(s => s.Id == id);


            if (staff == null)
                return NotFound();


            staff.StaffActive =
                !(staff.StaffActive ?? false);


            await _context.SaveChangesAsync();


            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // HELPERS
        // =========================================================

        private bool StaffExists(int id)
        {
            return _context.Staff.Any(s => s.Id == id);
        }


        // =========================================================
        // ENTERPRISE USER CHECK
        // =========================================================

        private bool IsEnterpriseUser()
        {
            var sessionId =
                HttpContext.Session.GetString("Id");

            var sessionUsername =
                HttpContext.Session.GetString("Username");

            var sessionEmail =
                HttpContext.Session.GetString("Email");

            var sessionFromEnterprise =
                HttpContext.Session.GetString("FromEnterprise");


            return !string.IsNullOrEmpty(sessionId) &&
                   !string.IsNullOrEmpty(sessionUsername) &&
                   !string.IsNullOrEmpty(sessionEmail) &&
                   sessionFromEnterprise == "True";
        }
    }
}