using EmFren.Data;
using EmFren.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmFren.Controllers
{
    public class MyAccountController : Controller
    {
        private readonly EmFrenDbContext _context;

        public MyAccountController(EmFrenDbContext context)
        {
            _context = context;
        }


        // =========================================================
        // GET: MyAccount/Edit
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            // Get logged-in user ID from session
            int? userId = HttpContext.Session.GetInt32("Id");

            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            // Get ONLY the logged-in user
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId.Value);

            if (user == null)
            {
                HttpContext.Session.Clear();

                return RedirectToAction("Login", "Users");
            }

            // ONLY users who are NOT from enterprise
            if (user.FromEnterprise)
            {
                return RedirectToAction("Login", "Users");
            }

            return View(user);
        }


        // =========================================================
        // POST: MyAccount/Edit
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string username, string email)
        {
            // =====================================================
            // GET LOGGED-IN USER ID FROM SESSION
            // =====================================================

            int? userId = HttpContext.Session.GetInt32("Id");

            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }


            // =====================================================
            // GET ONLY THE LOGGED-IN USER
            // =====================================================

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId.Value);

            if (user == null)
            {
                HttpContext.Session.Clear();

                return RedirectToAction("Login", "Users");
            }


            // =====================================================
            // ONLY NON-ENTERPRISE USERS CAN EDIT
            // =====================================================

            if (user.FromEnterprise)
            {
                return RedirectToAction("Login", "Users");
            }


            // =====================================================
            // VALIDATION
            // =====================================================

            if (string.IsNullOrWhiteSpace(username))
            {
                ModelState.AddModelError(
                    "Username",
                    "El nombre de usuario es obligatorio."
                );
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError(
                    "Email",
                    "El correo electrónico es obligatorio."
                );
            }


            // =====================================================
            // CHECK EMAIL
            // =====================================================

            if (!string.IsNullOrWhiteSpace(email))
            {
                bool emailExists = await _context.Users
                    .AnyAsync(u =>
                        u.Email == email.Trim() &&
                        u.Id != userId.Value
                    );

                if (emailExists)
                {
                    ModelState.AddModelError(
                        "Email",
                        "Este correo electrónico ya está utilizado por otro usuario."
                    );
                }
            }


            // =====================================================
            // RETURN VIEW IF VALIDATION FAILS
            // =====================================================

            if (!ModelState.IsValid)
            {
                user.Username = username;
                user.Email = email;

                return View(user);
            }


            // =====================================================
            // UPDATE ONLY CURRENT USER
            // =====================================================

            user.Username = username.Trim();
            user.Email = email.Trim();

            await _context.SaveChangesAsync();


            // =====================================================
            // UPDATE SESSION
            // =====================================================

            HttpContext.Session.SetString(
                "Username",
                user.Username
            );

            HttpContext.Session.SetString(
                "Email",
                user.Email
            );


            // =====================================================
            // SUCCESS MESSAGE
            // =====================================================

            TempData["SuccessMessage"] =
                "Tu cuenta ha sido actualizada correctamente.";


            return RedirectToAction("Edit");
        }
    }
}