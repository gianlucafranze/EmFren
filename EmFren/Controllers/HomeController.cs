using EmFren.Data;
using EmFren.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmFren.Controllers
{
    public class HomeController : Controller
    {
        private readonly EmFrenDbContext _context;

        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public HomeController(EmFrenDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // HOME ENTERPRISE
        // =========================================================

        public async Task<IActionResult> HomeEnterprise()
        {
            // =========================================================
            // SESSION
            // =========================================================

            int? id = HttpContext.Session.GetInt32("Id");

            string? username =
                HttpContext.Session.GetString("Username");

            string? email =
                HttpContext.Session.GetString("Email");

            string? fromEnterprise =
                HttpContext.Session.GetString("FromEnterprise");


            // =========================================================
            // VALIDATE SESSION
            // =========================================================

            if (id == null ||
                string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(fromEnterprise))
            {
                return RedirectToAction("Login", "Users");
            }


            // =========================================================
            // VERIFY ENTERPRISE ACCOUNT
            // =========================================================

            if (!bool.TryParse(fromEnterprise, out bool isEnterprise) ||
                !isEnterprise)
            {
                return RedirectToAction("Login", "Users");
            }


            // =========================================================
            // UNREAD NOTIFICATION COUNT
            // =========================================================

            int unreadNotificationCount =
                await _context.NotificationUsers
                    .CountAsync(nu =>
                        nu.UserId == id.Value &&
                        !nu.IsRead);


            // =========================================================
            // CLIENT COUNT
            // =========================================================

            int clientCount =
                await _context.Clients
                    .CountAsync();


            // =========================================================
            // ARTICLE COUNT
            // =========================================================

            int articleCount =
                await _context.Articles
                    .CountAsync();


            // =========================================================
            // PURCHASE COUNT
            // =========================================================

            int purchaseCount =
                await _context.Orders
                    .CountAsync();

            // =========================================================
            // DEBTOR CLIENT COUNT
            // =========================================================

            int debtorClientCount =
                await _context.Orders
                    .CountAsync(o =>
                        o.IsDebtorClient == true);


            // =========================================================
            // VIEW DATA
            // =========================================================

            ViewBag.Id = id;
            ViewBag.Username = username;
            ViewBag.Email = email;

            ViewBag.UnreadNotificationCount =
                unreadNotificationCount;

            ViewBag.ClientCount =
                clientCount;

            ViewBag.ArticleCount =
                articleCount;

            ViewBag.PurchaseCount =
                purchaseCount;

            ViewBag.DebtorClientCount =
                debtorClientCount;


            // =========================================================
            // VIEW
            // =========================================================

            return View();
        }


        // =========================================================
        // HOME BUYER
        // =========================================================

        public async Task<IActionResult> HomeBuyer()
        {
            // =========================================================
            // SESSION
            // =========================================================

            int? id =
                HttpContext.Session.GetInt32("Id");

            string? username =
                HttpContext.Session.GetString("Username");

            string? email =
                HttpContext.Session.GetString("Email");

            string? fromEnterprise =
                HttpContext.Session.GetString("FromEnterprise");


            // =========================================================
            // VALIDATE SESSION
            // =========================================================

            if (id == null ||
                string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(fromEnterprise))
            {
                return RedirectToAction("Login", "Users");
            }


            // =========================================================
            // VERIFY THAT THIS IS NOT AN ENTERPRISE ACCOUNT
            // =========================================================

            if (!bool.TryParse(fromEnterprise, out bool isEnterprise) ||
                isEnterprise)
            {
                return RedirectToAction("Login", "Users");
            }

            // =========================================================
            // UNREAD NOTIFICATION COUNT
            // =========================================================

            int unreadNotificationCount =
                await _context.NotificationUsersClient
                    .CountAsync(nu =>
                        nu.UserId == id.Value &&
                        !nu.IsRead);

            // =========================================================
            // VIEW DATA
            // =========================================================

            ViewBag.Id = id;
            ViewBag.Username = username;
            ViewBag.Email = email;
            ViewBag.UnreadNotificationCount = unreadNotificationCount;


            // =========================================================
            // VIEW
            // =========================================================

            return View();
        }
    }
}