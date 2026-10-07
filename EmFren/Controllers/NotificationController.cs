using EmFren.Controllers;
using EmFren.Data;
using EmFren.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using static System.Net.Mime.MediaTypeNames;

namespace EmFren.Controllers
{
    public class NotificationController : Controller
    {
        private readonly EmFrenDbContext _context;

        public NotificationController(EmFrenDbContext context)
        {
            _context = context;
        }


        // =========================================================
        // NOTIFICATION ENTERPRISE
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> NotificationEnterprise()
        {
            int? userId = HttpContext.Session.GetInt32("Id");

            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            bool isEnterprise =
                HttpContext.Session.GetString("FromEnterprise") == "True";

            if (!isEnterprise)
            {
                return RedirectToAction("Login", "Users");
            }


            // =====================================================
            // USERNAME
            // =====================================================

            string username =
                HttpContext.Session.GetString("Username") ?? "Usuario";


            // =====================================================
            // GET NOTIFICATIONS FOR CURRENT ENTERPRISE USER
            // =====================================================
            var notifications = await _context.NotificationUsers
                .Include(x => x.Notification)
                .Include(x => x.User)
                .OrderByDescending(x => x.Notification!.CreatedAt)
                .Select(x => new
                {
                    Id = x.Notification!.Id,

                    Title = x.Notification.Title,

                    Message = x.Notification.Message,

                    OrderId = x.Notification.OrderId,

                    CreatedAt = x.Notification.CreatedAt,

                    IsRead = x.IsRead,

                    UsernameNotification = x.User!.Username
                })
                .ToListAsync();


            // =====================================================
            // SEND DATA TO VIEW
            // =====================================================

            ViewBag.Username = username;

            ViewBag.Notifications = notifications;


            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            int? userId = HttpContext.Session.GetInt32("Id");

            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            bool isEnterprise =
                HttpContext.Session.GetString("FromEnterprise") == "True";

            if (!isEnterprise)
            {
                return RedirectToAction("Login", "Users");
            }

            var notificationUser = await _context.NotificationUsers
                .FirstOrDefaultAsync(x =>
                    x.NotificationId == notificationId);

            if (notificationUser == null)
            {
                return NotFound();
            }

            notificationUser.IsRead = true;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(NotificationEnterprise));
        }

        [HttpGet]
        public async Task<IActionResult> NotificationBuyer()
        {
            int? userId = HttpContext.Session.GetInt32("Id");

            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            bool isEnterprise =
                HttpContext.Session.GetString("FromEnterprise") == "True";

            if (isEnterprise)
            {
                return RedirectToAction("Login", "Users");
            }


            // =====================================================
            // USERNAME
            // =====================================================

            string username =
                HttpContext.Session.GetString("Username") ?? "Usuario";


            // =====================================================
            // GET NOTIFICATIONS FOR CURRENT ENTERPRISE USER
            // =====================================================
            var notifications =
                await _context.NotificationUsersClient
                    .Where(x =>
                        x.UserId == userId)
                    .Include(x => x.Notification)
                    .Include(x => x.User)
                    .OrderByDescending(x =>
                        x.Notification!.CreatedAt)
                    .Select(x => new
                    {
                        Id = x.Notification!.Id,
                        Title = x.Notification.Title,
                        Message = x.Notification.Message,
                        OrderId = x.Notification.OrderId,
                        CreatedAt = x.Notification.CreatedAt,
                        IsRead = x.IsRead,
                        UsernameNotification = x.User!.Username
                    })
                    .ToListAsync();


            // =====================================================
            // SEND DATA TO VIEW
            // =====================================================

            ViewBag.Username = username;

            ViewBag.Notifications = notifications;


            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsReadBuyer(int notificationId)
        {
            int? userId = HttpContext.Session.GetInt32("Id");

            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            bool isEnterprise =
                HttpContext.Session.GetString("FromEnterprise") == "True";

            if (isEnterprise)
            {
                return RedirectToAction("Login", "Users");
            }

            var notificationUser = await _context.NotificationUsersClient
                .FirstOrDefaultAsync(x =>
                    x.NotificationId == notificationId);

            if (notificationUser == null)
            {
                return NotFound();
            }

            notificationUser.IsRead = true;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(NotificationBuyer));
        }
    }
}