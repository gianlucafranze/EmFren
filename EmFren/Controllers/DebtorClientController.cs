using EmFren.Data;
using EmFren.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmFren.Controllers
{
    public class DebtorClientController : Controller
    {
        private readonly EmFrenDbContext _context;

        public DebtorClientController(EmFrenDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            DateTime? dateFrom,
            DateTime? dateTo,
            string debtorFilter = "all",
            int page = 1)
        {
            // ============================================
            // ENTERPRISE SESSION CHECK
            // ============================================

            int? userId = HttpContext.Session.GetInt32("Id");
            string? username = HttpContext.Session.GetString("Username");
            string? email = HttpContext.Session.GetString("Email");
            string? fromEnterprise = HttpContext.Session.GetString("FromEnterprise");

            if (!userId.HasValue ||
                string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(email) ||
                fromEnterprise != "True")
            {
                return RedirectToAction("Login", "Users");
            }


            // ============================================
            // PAGINATION
            // ============================================

            const int pageSize = 10;

            if (page < 1)
            {
                page = 1;
            }


            // ============================================
            // NORMALIZE DEBTOR FILTER
            // ============================================

            debtorFilter = (debtorFilter ?? "all").Trim().ToLowerInvariant();

            if (debtorFilter != "all" &&
                debtorFilter != "debtor" &&
                debtorFilter != "nondebtor")
            {
                debtorFilter = "all";
            }


            // ============================================
            // BASE QUERY
            // ============================================

            var query =
                from o in _context.Orders

                join u in _context.Users
                    on o.UserId equals u.Id

                join cp in _context.CoinPayments
                    on o.CoinPaymentId equals cp.Id

                join oa in _context.OrderArticles
                    on o.Id equals oa.Order.Id

                join a in _context.Articles
                    on oa.IdArticle equals a.Id

                join v in _context.Vehicles
                    on a.VehicleId equals v.Id into vehicleJoin

                from v in vehicleJoin.DefaultIfEmpty()

                select new ViewShopUser
                {
                    // ====================================
                    // ORDER
                    // ====================================

                    OrderId = o.Id,

                    DateOrder = o.DateOrder,

                    TotalPurchase = o.TotalPurchase,

                    IsDebtorClient = o.IsDebtorClient,


                    // ====================================
                    // USER
                    // ====================================

                    UserId = u.Id,

                    Username = u.Username,


                    // ====================================
                    // PAYMENT
                    // ====================================

                    PaymentCurrency = cp.CoinName,


                    // ====================================
                    // ORDER ARTICLE
                    // ====================================

                    Quantity = oa.Quantity,


                    // ====================================
                    // ARTICLE
                    // ====================================

                    ArticleCode = a.ArticleId,

                    Measure = a.Measure,

                    Type = a.Type,

                    Friction = a.Friction,

                    UnitPrice = a.Price,

                    Discount = a.Discount,

                    Stock = a.Stock,


                    // ====================================
                    // VEHICLE
                    // ====================================

                    VehicleBrand = v != null
                        ? v.Brand
                        : null
                };


            // ============================================
            // SEARCH BY CLIENT / USERNAME
            // ============================================

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(x =>
                    x.Username != null &&
                    x.Username.Contains(search));
            }


            // ============================================
            // DATE FROM
            // ============================================

            if (dateFrom.HasValue)
            {
                DateTime from = dateFrom.Value.Date;

                query = query.Where(x =>
                    x.DateOrder.HasValue &&
                    x.DateOrder.Value >= from);
            }


            // ============================================
            // DATE TO
            // ============================================

            if (dateTo.HasValue)
            {
                DateTime toExclusive =
                    dateTo.Value.Date.AddDays(1);

                query = query.Where(x =>
                    x.DateOrder.HasValue &&
                    x.DateOrder.Value < toExclusive);
            }


            // ============================================
            // DEBTOR CLIENT FILTER
            // ============================================

            if (debtorFilter == "debtor")
            {
                // Only clients marked as debtors

                query = query.Where(x =>
                    x.IsDebtorClient == true);
            }
            else if (debtorFilter == "nondebtor")
            {
                // Clients that are NOT debtors.
                // NULL is also treated as "No deudor".

                query = query.Where(x =>
                    x.IsDebtorClient == false ||
                    x.IsDebtorClient == null);
            }
            else
            {
                // "all"
                // No debtor filter is applied.

                debtorFilter = "all";
            }


            // ============================================
            // ORDER
            // ============================================

            query = query
                .OrderByDescending(x => x.OrderId)
                .ThenByDescending(x => x.DateOrder);


            // ============================================
            // TOTAL RECORDS
            // ============================================

            int totalRecords =
                await query.CountAsync();

            int totalPages =
                (int)Math.Ceiling(
                    totalRecords / (double)pageSize);


            // ============================================
            // FIX PAGE IF OUT OF RANGE
            // ============================================

            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
            }


            // ============================================
            // PAGED RESULTS
            // ============================================

            List<ViewShopUser> purchases =
                await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .AsNoTracking()
                    .ToListAsync();


            // ============================================
            // VIEWBAGS
            // ============================================

            ViewBag.Username = username;

            ViewBag.Search =
                search ?? "";

            ViewBag.DateFrom =
                dateFrom?.ToString("yyyy-MM-dd") ?? "";

            ViewBag.DateTo =
                dateTo?.ToString("yyyy-MM-dd") ?? "";

            // IMPORTANT:
            // This value is used by the SELECT in the view
            // to keep the selected option after filtering.

            ViewBag.DebtorFilter =
                debtorFilter;

            ViewBag.CurrentPage =
                page;

            ViewBag.TotalPages =
                totalPages;

            ViewBag.TotalResults =
                totalRecords;

            ViewBag.PageSize =
                pageSize;


            // ============================================
            // RETURN VIEW
            // ============================================

            return View(purchases);
        }

        //set client debtor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetClientDebtor(int orderId, int userId)
        {
            string? sessionId =
                HttpContext.Session.GetString("Id");

            string? fromEnterprise =
                HttpContext.Session.GetString("FromEnterprise");


            if (string.IsNullOrEmpty(sessionId) ||
                string.IsNullOrEmpty(fromEnterprise) ||
                fromEnterprise != "True")
            {
                return RedirectToAction(
                    "Login",
                    "Users"
                );
            }

            // Search for the order
            var order =
                await _context.Orders
                    .FirstOrDefaultAsync(x => x.Id == orderId);

            if (order == null)
            {
                return RedirectToAction(
                    "Index"
                );
            }

            // Mark this order as debtor client
            order.IsDebtorClient = true;


            var notificationClient = new NotificationClient
            {
                UserId = userId,
                OrderId = order.Id,
                Title = "Compra marcada como deudora",
                Message = $"La compra #{order.Id} ha sido marcada como pendiente de pago.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.NotificationsClient.Add(
                notificationClient
            );

            // Save first to generate notification ID
            await _context.SaveChangesAsync();

            // Create relationship between notification and client
            var notificationUserClient =
                new NotificationUserClient
                {
                    NotificationId = notificationClient.Id,
                    UserId = order.UserId,
                    IsRead = false
                };

            _context.NotificationUsersClient.Add(
                notificationUserClient
            );

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Index"
            );
        }

        //unset client debtor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnsetClientDebtor(int orderId, int userId)
        {
            string? sessionId =
                HttpContext.Session.GetString("Id");

            string? fromEnterprise =
                HttpContext.Session.GetString("FromEnterprise");


            if (string.IsNullOrEmpty(sessionId) ||
                string.IsNullOrEmpty(fromEnterprise) ||
                fromEnterprise != "True")
            {
                return RedirectToAction(
                    "Login",
                    "Users"
                );
            }

            // Search for the order
            var order =
                await _context.Orders
                    .FirstOrDefaultAsync(x => x.Id == orderId);

            if (order == null)
            {
                return RedirectToAction(
                    "Index"
                );
            }

            // Mark this order as debtor client
            order.IsDebtorClient = false;

            var notificationClient = new NotificationClient
            {
                UserId = userId,
                OrderId = order.Id,
                Title = "Compra marcada como paga",
                Message = $"La compra #{order.Id} ha sido marcada como pagado.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.NotificationsClient.Add(
                notificationClient
            );

            // Save first to generate notification ID
            await _context.SaveChangesAsync();

            // Create relationship between notification and client
            var notificationUserClient =
                new NotificationUserClient
                {
                    NotificationId = notificationClient.Id,
                    UserId = order.UserId,
                    IsRead = false
                };

            _context.NotificationUsersClient.Add(
                notificationUserClient
            );

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Index"
            );
        }
    }
}