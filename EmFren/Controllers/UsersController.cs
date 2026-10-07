using EmFren.Data;
using EmFren.Models;
using EmFren.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace EmFren.Controllers
{
    public class UsersController : Controller
    {
        private readonly EmFrenDbContext _context;
        private readonly EmailService _emailService;

        public UsersController(
            EmFrenDbContext context,
            EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        /* public async Task<IActionResult> User(string? search)
         {
             // ==============================
             // CHECK SESSION
             // ==============================

             int? id = HttpContext.Session.GetInt32("Id");

             string? username =
                 HttpContext.Session.GetString("Username");

             string? email =
                 HttpContext.Session.GetString("Email");

             string? fromEnterprise =
                 HttpContext.Session.GetString("FromEnterprise");


             // ==============================
             // NO VALID SESSION
             // ==============================

             if (id == null ||
                 string.IsNullOrEmpty(username) ||
                 string.IsNullOrEmpty(email) ||
                 string.IsNullOrEmpty(fromEnterprise))
             {
                 return RedirectToAction("Login", "Users");
             }


             // ==============================
             // VERIFY ENTERPRISE ACCOUNT
             // ==============================

             if (!bool.TryParse(fromEnterprise, out bool isEnterprise) ||
                 !isEnterprise)
             {
                 return RedirectToAction("Login", "Users");
             }


             // ==============================
             // CURRENT USER INFORMATION
             // ==============================

             ViewBag.Id = id;
             ViewBag.Username = username;
             ViewBag.Email = email;


             // ==============================
             // GET USERS FROM DATABASE
             // ==============================

             IQueryable<User> query = _context.Users;


             // ==============================
             // SEARCH
             // ==============================

             if (!string.IsNullOrWhiteSpace(search))
             {
                 search = search.Trim();

                 if (int.TryParse(search, out int searchId))
                 {
                     query = query.Where(u =>
                         u.Id == searchId ||
                         u.Username.ToLower().Contains(search.ToLower()) ||
                         u.Email.ToLower().Contains(search.ToLower()));
                 }
                 else
                 {
                     query = query.Where(u =>
                         u.Username.ToLower().Contains(search.ToLower()) ||
                         u.Email.ToLower().Contains(search.ToLower()));
                 }
             }


             // ==============================
             // GET USERS
             // ==============================

             List<User> users = await query
                 .OrderBy(u => u.Username)
                 .ToListAsync();


             // ==============================
             // SEND USERS TO VIEW
             // ==============================

             return View(users);
         }*/

        [HttpGet]
        public async Task<IActionResult> Users(string? search, int page = 1)
        {
            // ==========================================
            // CHECK SESSION
            // ==========================================

            int? id = HttpContext.Session.GetInt32("Id");

            string? username =
                HttpContext.Session.GetString("Username");

            string? email =
                HttpContext.Session.GetString("Email");

            string? fromEnterprise =
                HttpContext.Session.GetString("FromEnterprise");


            // ==========================================
            // NO VALID SESSION
            // ==========================================

            if (id == null ||
                string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(fromEnterprise))
            {
                return RedirectToAction("Login", "Users");
            }


            // ==========================================
            // VERIFY ENTERPRISE ACCOUNT
            // ==========================================

            if (!bool.TryParse(fromEnterprise, out bool isEnterprise) ||
                !isEnterprise)
            {
                return RedirectToAction("Login", "Users");
            }


            // ==========================================
            // SEARCH AND PAGINATION
            // ==========================================

            const int pageSize = 10;

            if (page < 1)
            {
                page = 1;
            }


            var query = _context.Users.AsQueryable();


            // ==========================================
            // SEARCH
            // ==========================================

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(u =>
                    u.Username.Contains(search) ||
                    u.Email.Contains(search) ||
                    u.Id.ToString().Contains(search));
            }


            // ==========================================
            // TOTAL USERS
            // ==========================================

            var totalUsers = await query.CountAsync();


            // ==========================================
            // TOTAL PAGES
            // ==========================================

            var totalPages = (int)Math.Ceiling(
                totalUsers / (double)pageSize
            );


            // If page is greater than the last page
            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
            }


            // ==========================================
            // USERS FOR CURRENT PAGE
            // ==========================================

            var users = await query
                .OrderBy(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            // ==========================================
            // VIEWBAG
            // ==========================================

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.Search = search;

            // Logged-in enterprise user information
            ViewBag.Id = id;
            ViewBag.Username = username;
            ViewBag.Email = email;


            return View(users);
        }

        // =========================================================
        // EDIT USER
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditUser(
            int Id,
            string Username,
            string Email)
        {
            // Find user
            var user = _context.Users
                .FirstOrDefault(u => u.Id == Id);

            if (user == null)
            {
                TempData["Error"] = "Usuario no encontrado.";

                return RedirectToAction(nameof(Users));
            }


            // Validate username
            if (string.IsNullOrWhiteSpace(Username))
            {
                TempData["Error"] =
                    "El nombre de usuario es obligatorio.";

                return RedirectToAction(nameof(Users));
            }


            // Validate email
            if (string.IsNullOrWhiteSpace(Email))
            {
                TempData["Error"] =
                    "El email es obligatorio.";

                return RedirectToAction(nameof(Users));
            }


            // Check if email is already being used
            bool emailExists = _context.Users.Any(u =>
                u.Email == Email.Trim() &&
                u.Id != Id);

            if (emailExists)
            {
                TempData["Error"] =
                    "Ya existe otro usuario con este email.";

                return RedirectToAction(nameof(Users));
            }


            // Update only Username and Email
            user.Username = Username.Trim();

            user.Email = Email.Trim();


            // Save changes
            _context.SaveChanges();


            TempData["Success"] =
                "Usuario actualizado correctamente.";


            return RedirectToAction(nameof(Users));
        }

        // SET / REMOVE FROM ENTERPRISE
        /*[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetFromEnterprise(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            user.FromEnterprise = !user.FromEnterprise;

            _context.Entry(user)
                .Property(u => u.FromEnterprise)
                .IsModified = true;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Users));
        }*/


        // set or not from enterprise
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetFromEnterprise(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            // Toggle FromEnterprise
            user.FromEnterprise = !user.FromEnterprise;

            // Explicitly tell Entity Framework that this property changed
            _context.Entry(user)
                    .Property(u => u.FromEnterprise)
                    .IsModified = true;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Users));
        }

        // delete user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Users));
        }

        // GET: /Users/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Users/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Find user by email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            // User doesn't exist
            if (user == null)
            {
                ModelState.AddModelError(
                    "",
                    "Email o contraseña incorrectos."
                );

                return View(model);
            }

            // Verify BCrypt password
            if (!BCrypt.Net.BCrypt.Verify(
                    model.Password,
                    user.PasswordHash))
            {
                ModelState.AddModelError(
                    "",
                    "Email o contraseña incorrectos."
                );

                return View(model);
            }

            // Store user information in Session
            HttpContext.Session.SetInt32(
                "Id",
                user.Id
            );

            HttpContext.Session.SetString(
                "Username",
                user.Username
            );

            HttpContext.Session.SetString(
                "Email",
                user.Email
            );

            HttpContext.Session.SetString(
                "FromEnterprise",
                user.FromEnterprise.ToString()
            );

            // Redirect according to account type
            if (user.FromEnterprise)
            {
                // Enterprise user
                return RedirectToAction(
                    "HomeEnterprise",
                    "Home"
                );
            }
            else
            {
                // Normal buyer/user
                return RedirectToAction(
                    "HomeBuyer",
                    "Home"
                );
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login", "Users");
        }


        // GET: /Users/CreateAccount
        [HttpGet]
        public IActionResult CreateAccount()
        {
            return View();
        }


        // GET: /Users/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }


        // POST: /Users/CreateAccount
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAccount(RegisterUser model)
        {
            /*
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingEmail = _context.Users
                .FirstOrDefault(u => u.Email == model.Email);

            if (existingEmail != null)
            {
                ModelState.AddModelError(
                    "Email",
                    "Este email ya está registrado."
                );

                return View(model);
            }

            var existingUsername = _context.Users
                .FirstOrDefault(u => u.Username == model.Username);

            if (existingUsername != null)
            {
                ModelState.AddModelError(
                    "Username",
                    "Este nombre de usuario ya está registrado."
                );

                return View(model);
            }
            */

            string passwordHash =
                BCrypt.Net.BCrypt.HashPassword(model.Password);

            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = passwordHash,
                FromEnterprise = false
            };

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            return RedirectToAction("Login", "Users");
        }


        // POST: /Users/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            // Find the user using User.Id as the primary key
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            // Don't reveal whether the email exists
            if (user == null)
            {
                return RedirectToAction("ForgotPasswordConfirmation");
            }

            // Generate a secure random token
            var tokenBytes = RandomNumberGenerator.GetBytes(64);

            var token = Convert.ToBase64String(tokenBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");

            // Invalidate previous unused tokens belonging to this user
            // user.Id is the primary key from the User model
            var oldTokens = await _context.PasswordResetTokens
                .Where(x => x.UserId == user.Id && !x.Used)
                .ToListAsync();

            foreach (var oldToken in oldTokens)
            {
                oldToken.Used = true;
            }

            // Create a new password reset token
            var resetToken = new PasswordResetToken
            {
                // PasswordResetToken.UserId is the foreign key
                // that points to User.Id
                UserId = user.Id,

                Token = token,

                ExpiresAt = DateTime.UtcNow.AddMinutes(60),

                Used = false
            };

            _context.PasswordResetTokens.Add(resetToken);

            await _context.SaveChangesAsync();

            // Create password reset link
            var resetLink =
                $"{Request.Scheme}://{Request.Host}/Users/ResetPassword?token={token}";

            // Send email
            await _emailService.SendPasswordResetEmail(
                user.Email,
                user.Username,
                resetLink);

            return RedirectToAction("ForgotPasswordConfirmation");
        }


        // GET: /Users/ResetPassword?token=...
        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token)
        {
            var resetToken = await _context.PasswordResetTokens
                .FirstOrDefaultAsync(x =>
                    x.Token == token &&
                    !x.Used);

            if (resetToken == null ||
                resetToken.ExpiresAt < DateTime.UtcNow)
            {
                return View("InvalidResetToken");
            }

            var model = new ResetPasswordViewModel
            {
                Token = token
            };

            return View(model);
        }


        // POST: /Users/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(
            ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var resetToken = await _context.PasswordResetTokens
                .FirstOrDefaultAsync(x =>
                    x.Token == model.Token &&
                    !x.Used);

            if (resetToken == null ||
                resetToken.ExpiresAt < DateTime.UtcNow)
            {
                return View("InvalidResetToken");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(x =>
                    x.Id == resetToken.UserId);

            if (user == null)
            {
                return View("InvalidResetToken");
            }

            // Hash the new password with BCrypt
            string passwordHash =
                BCrypt.Net.BCrypt.HashPassword(
                    model.NewPassword);

            user.PasswordHash = passwordHash;

            // Token can only be used once
            resetToken.Used = true;

            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }


        // GET: /Users/ForgotPasswordConfirmation
        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }
    }
}