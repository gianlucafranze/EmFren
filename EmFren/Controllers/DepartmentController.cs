using EmFren.Data;
using EmFren.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmFren.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly EmFrenDbContext _context;

        public DepartmentController(EmFrenDbContext context)
        {
            _context = context;
        }


        // ============================================================
        // INDEX
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string sort = "id_asc",
            int page = 1)
        {
            // --------------------------------------------------------
            // ENTERPRISE SESSION VALIDATION
            // --------------------------------------------------------

            int? id = HttpContext.Session.GetInt32("Id");

            string? username =
                HttpContext.Session.GetString("Username");

            string? email =
                HttpContext.Session.GetString("Email");

            string? fromEnterprise =
                HttpContext.Session.GetString("FromEnterprise");


            if (id == null ||
                string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(fromEnterprise))
            {
                return RedirectToAction("Login", "Users");
            }


            if (!bool.TryParse(
                    fromEnterprise,
                    out bool isEnterprise) ||
                !isEnterprise)
            {
                return RedirectToAction("Login", "Users");
            }


            // --------------------------------------------------------
            // PAGINATION
            // --------------------------------------------------------

            const int pageSize = 10;

            if (page < 1)
            {
                page = 1;
            }


            var query = _context.Departments
                .AsQueryable();


            // --------------------------------------------------------
            // SEARCH
            // --------------------------------------------------------

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(d =>
                    d.DepartmentName.Contains(search) ||
                    d.Id.ToString().Contains(search));
            }


            // --------------------------------------------------------
            // SORT
            // --------------------------------------------------------

            switch (sort)
            {
                case "id_asc":

                    query = query.OrderBy(d => d.Id);

                    break;


                case "id_desc":

                    query = query.OrderByDescending(d => d.Id);

                    break;


                case "name_asc":

                    query = query.OrderBy(d => d.DepartmentName);

                    break;


                case "name_desc":

                    query = query.OrderByDescending(d => d.DepartmentName);

                    break;


                default:

                    sort = "id_asc";

                    query = query.OrderBy(d => d.Id);

                    break;
            }


            // --------------------------------------------------------
            // TOTAL
            // --------------------------------------------------------

            int totalDepartments =
                await query.CountAsync();


            int totalPages =
                (int)Math.Ceiling(
                    totalDepartments /
                    (double)pageSize);


            if (totalPages > 0 &&
                page > totalPages)
            {
                page = totalPages;
            }


            // --------------------------------------------------------
            // GET DATA
            // --------------------------------------------------------

            var departments = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            // --------------------------------------------------------
            // VIEWBAG
            // --------------------------------------------------------

            ViewBag.CurrentPage = page;

            ViewBag.TotalPages = totalPages;

            ViewBag.TotalDepartments =
                totalDepartments;

            ViewBag.Search = search;

            ViewBag.Sort = sort;

            ViewBag.Id = id;

            ViewBag.Username = username;

            ViewBag.Email = email;


            return View("Index", departments);
        }


        // ============================================================
        // CREATE
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Department model,
            string? search,
            string? sort,
            int page = 1)
        {
            if (!IsEnterpriseUser())
            {
                return RedirectToAction(
                    "Login",
                    "Users");
            }


            if (!ModelState.IsValid)
            {
                TempData["Error"] =
                    "Los datos del departamento no son válidos.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        search,
                        sort,
                        page
                    });
            }


            string departmentName =
                model.DepartmentName.Trim();


            // --------------------------------------------------------
            // CHECK DUPLICATE
            // --------------------------------------------------------

            bool exists =
                await _context.Departments.AnyAsync(d =>
                    d.DepartmentName.ToLower()
                    == departmentName.ToLower());


            if (exists)
            {
                TempData["Error"] =
                    "Este departamento ya está registrado.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        search,
                        sort,
                        page
                    });
            }


            model.DepartmentName =
                departmentName;


            _context.Departments.Add(model);

            await _context.SaveChangesAsync();


            TempData["Success"] =
                "Departamento agregado correctamente.";


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
            Department model,
            string? search,
            string? sort,
            int page = 1)
        {
            if (!IsEnterpriseUser())
            {
                return RedirectToAction(
                    "Login",
                    "Users");
            }


            if (!ModelState.IsValid)
            {
                TempData["Error"] =
                    "Los datos del departamento no son válidos.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        search,
                        sort,
                        page
                    });
            }


            var department =
                await _context.Departments
                    .FirstOrDefaultAsync(d =>
                        d.Id == model.Id);


            if (department == null)
            {
                TempData["Error"] =
                    "El departamento no existe.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        search,
                        sort,
                        page
                    });
            }


            string departmentName =
                model.DepartmentName.Trim();


            // --------------------------------------------------------
            // CHECK DUPLICATE
            // --------------------------------------------------------

            bool exists =
                await _context.Departments.AnyAsync(d =>
                    d.Id != model.Id &&
                    d.DepartmentName.ToLower()
                    == departmentName.ToLower());


            if (exists)
            {
                TempData["Error"] =
                    "Este departamento ya está registrado.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        search,
                        sort,
                        page
                    });
            }


            department.DepartmentName =
                departmentName;


            await _context.SaveChangesAsync();


            TempData["Success"] =
                "Departamento actualizado correctamente.";


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
            if (!IsEnterpriseUser())
            {
                return RedirectToAction(
                    "Login",
                    "Users");
            }


            var department =
                await _context.Departments
                    .FirstOrDefaultAsync(d =>
                        d.Id == id);


            if (department == null)
            {
                TempData["Error"] =
                    "El departamento no existe.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        search,
                        sort,
                        page
                    });
            }


            _context.Departments.Remove(
                department);


            try
            {
                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Departamento eliminado correctamente.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "No se puede eliminar este departamento porque está siendo utilizado en otros registros.";
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
        // ENTERPRISE USER CHECK
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


            if (id == null ||
                string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(fromEnterprise))
            {
                return false;
            }


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