using EmFren.Data;
using EmFren.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;

namespace EmFren.Controllers
{
    public class ShopController : Controller
    {
        private readonly EmFrenDbContext _context;

        public ShopController(EmFrenDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // VIEW SHOP ENTERPRISE
        // =========================================================


        [HttpGet]
        public async Task<IActionResult> ViewShopEnterprise(
        int page = 1,
        string? searchUser = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null)
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

            const int pageSize = 10;

            if (page < 1)
            {
                page = 1;
            }

            var query =
                from o in _context.Orders

                join u in _context.Users
                    on o.UserId equals u.Id

                join cp in _context.CoinPayments
                    on o.CoinPaymentId equals cp.Id

                join oa in _context.OrderArticles
                    on o.Id equals oa.IdOrder

                join a in _context.Articles
                    on oa.IdArticle equals a.Id

                join v in _context.Vehicles
                    on a.VehicleId equals v.Id
                    into vehicleGroup

                from v in vehicleGroup.DefaultIfEmpty()

                select new ViewShopUser
                {
                    OrderId = o.Id,
                    DateOrder = o.DateOrder,
                    TotalPurchase = o.TotalPurchase,

                    UserId = u.Id,
                    Username = u.Username,

                    PaymentCurrency = cp.CoinName,

                    Quantity = oa.Quantity,

                    ArticleCode = a.ArticleId,
                    Measure = a.Measure,
                    Type = a.Type,
                    Friction = a.Friction,

                    UnitPrice = a.Price,
                    Discount = a.Discount,
                    Stock = a.Stock,

                    VehicleBrand =
                        v != null
                            ? v.Brand
                            : null
                };


            // =========================================================
            // BUSCAR POR USUARIO O ID
            // =========================================================

            if (!string.IsNullOrWhiteSpace(searchUser))
            {
                string search =
                    searchUser.Trim();

                if (int.TryParse(search, out int userId))
                {
                    query =
                        query.Where(x =>
                            x.UserId == userId ||
                            x.Username.Contains(search));
                }
                else
                {
                    query =
                        query.Where(x =>
                            x.Username.Contains(search));
                }
            }


            // =========================================================
            // FECHA DESDE
            // =========================================================

            if (dateFrom.HasValue)
            {
                DateTime fromDate =
                    dateFrom.Value.Date;

                query =
                    query.Where(x =>
                        x.DateOrder.HasValue &&
                        x.DateOrder.Value >= fromDate);
            }


            // =========================================================
            // FECHA HASTA
            // =========================================================

            if (dateTo.HasValue)
            {
                DateTime toDateExclusive =
                    dateTo.Value.Date.AddDays(1);

                query =
                    query.Where(x =>
                        x.DateOrder.HasValue &&
                        x.DateOrder.Value < toDateExclusive);
            }


            // =========================================================
            // IMPORTANTE:
            // CONTAR PEDIDOS, NO ARTÍCULOS
            // =========================================================

            int totalRecords =
                await query
                    .Select(x => x.OrderId)
                    .Distinct()
                    .CountAsync();


            int totalPages =
                (int)Math.Ceiling(
                    totalRecords /
                    (double)pageSize
                );


            if (totalPages > 0 &&
                page > totalPages)
            {
                page = totalPages;
            }


            // =========================================================
            // OBTENER LOS IDs DE LOS PEDIDOS DE ESTA PÁGINA
            // =========================================================

            var orderIds =
                await query
                    .Select(x => new
                    {
                        x.OrderId,
                        x.DateOrder
                    })
                    .Distinct()
                    .OrderByDescending(x => x.DateOrder)
                    .ThenByDescending(x => x.OrderId)
                    .Skip(
                        (page - 1) *
                        pageSize
                    )
                    .Take(pageSize)
                    .Select(x => x.OrderId)
                    .ToListAsync();


            // =========================================================
            // OBTENER TODOS LOS ARTÍCULOS DE ESOS PEDIDOS
            // =========================================================

            var purchases =
                await query
                    .Where(x =>
                        orderIds.Contains(x.OrderId))
                    .OrderByDescending(x => x.DateOrder)
                    .ThenByDescending(x => x.OrderId)
                    .AsNoTracking()
                    .ToListAsync();


            // =========================================================
            // PAGINATION VIEWBAGS
            // =========================================================

            ViewBag.CurrentPage =
                page;

            ViewBag.TotalPages =
                totalPages;

            ViewBag.TotalRecords =
                totalRecords;

            ViewBag.PageSize =
                pageSize;


            // =========================================================
            // FILTERS
            // =========================================================

            ViewBag.SearchUser =
                searchUser ?? "";

            ViewBag.DateFrom =
                dateFrom?.ToString("yyyy-MM-dd");

            ViewBag.DateTo =
                dateTo?.ToString("yyyy-MM-dd");


            ViewBag.Username =
                HttpContext.Session.GetString(
                    "Username"
                ) ?? "U";


            return View(
                "~/Views/Shop/ViewShopEnterprise.cshtml",
                purchases
            );
        }

        // =========================================================
        // ARTICLES / SHOP
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Articles(
            string? search,
            int page = 1)
        {
            // -----------------------------------------------------
            // CHECK LOGIN
            // -----------------------------------------------------

            int? userId =
                HttpContext.Session.GetInt32("Id");

            if (userId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Users"
                );
            }


            // -----------------------------------------------------
            // BUYER ONLY
            // -----------------------------------------------------

            bool isEnterprise =
                HttpContext.Session.GetString(
                    "FromEnterprise"
                ) == "True";

            if (isEnterprise)
            {
                return RedirectToAction(
                    "Login",
                    "Users"
                );
            }


            // -----------------------------------------------------
            // PAGINATION SETTINGS
            // -----------------------------------------------------

            const int pageSize = 10;

            if (page < 1)
            {
                page = 1;
            }


            // -----------------------------------------------------
            // QUERY
            // ONLY ARTICLES WITH STOCK = TRUE
            // -----------------------------------------------------

            var query = _context.Articles
                .Include(a => a.Vehicle)
                .Where(a => a.Stock == true)
                .AsNoTracking()
                .AsQueryable();


            // -----------------------------------------------------
            // SEARCH
            // -----------------------------------------------------

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                string searchLower =
                    search.ToLower();

                query = query.Where(a =>

                    // Article ID
                    (a.ArticleId != null &&
                     a.ArticleId
                        .ToLower()
                        .Contains(searchLower))

                    ||

                    // Vehicle
                    (a.Vehicle != null &&
                     a.Vehicle.Brand != null &&
                     a.Vehicle.Brand
                        .ToLower()
                        .Contains(searchLower))

                    ||

                    // Measure
                    (a.Measure != null &&
                     a.Measure
                        .ToLower()
                        .Contains(searchLower))

                    ||

                    // Type
                    (a.Type != null &&
                     a.Type
                        .ToLower()
                        .Contains(searchLower))

                    ||

                    // Friction
                    (a.Friction != null &&
                     a.Friction
                        .ToLower()
                        .Contains(searchLower))

                    ||

                    // Database ID
                    a.Id
                        .ToString()
                        .Contains(search)
                );
            }


            // -----------------------------------------------------
            // TOTAL ARTICLES
            // -----------------------------------------------------

            int totalArticles =
                await query.CountAsync();


            int totalPages =
                (int)Math.Ceiling(
                    totalArticles /
                    (double)pageSize
                );


            if (totalPages > 0 &&
                page > totalPages)
            {
                page = totalPages;
            }


            // -----------------------------------------------------
            // GET PAGE
            // -----------------------------------------------------

            var articles =
                await query
                    .OrderBy(a => a.Id)
                    .Skip(
                        (page - 1) *
                        pageSize
                    )
                    .Take(pageSize)
                    .ToListAsync();


            // -----------------------------------------------------
            // COIN PAYMENT
            // -----------------------------------------------------

            var coinPayments =
                await _context.CoinPayments
                    .AsNoTracking()
                    .OrderBy(c => c.Id)
                    .ToListAsync();


            // -----------------------------------------------------
            // VIEWBAGS
            // -----------------------------------------------------

            ViewBag.Search =
                search;

            ViewBag.CurrentPage =
                page;

            ViewBag.TotalPages =
                totalPages;

            ViewBag.TotalArticles =
                totalArticles;

            ViewBag.PageSize =
                pageSize;


            ViewBag.Username =
                HttpContext.Session.GetString(
                    "Username"
                ) ?? "U";


            // -----------------------------------------------------
            // COIN PAYMENT FOR VIEW
            // -----------------------------------------------------

            ViewBag.CoinPayments =
                coinPayments;


            // -----------------------------------------------------
            // RETURN VIEW
            // -----------------------------------------------------

            return View(
                "~/Views/Shop/Articles.cshtml",
                articles
            );
        }


        // =========================================================
        // VIEW USER PURCHASES
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> ViewShopUser(
         int page = 1,
         DateTime? dateFrom = null,
         DateTime? dateTo = null)
        {
            // =========================================================
            // SESSION
            // =========================================================

            int? userId =
                HttpContext.Session.GetInt32("Id");

            if (userId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Users"
                );
            }

            // =========================================================
            // BLOCK ENTERPRISE USERS
            // =========================================================

            bool isEnterprise =
                HttpContext.Session.GetString(
                    "FromEnterprise"
                ) == "True";

            if (isEnterprise)
            {
                return RedirectToAction(
                    "Login",
                    "Users"
                );
            }

            // =========================================================
            // PAGINATION
            // =========================================================

            const int pageSize = 10;

            if (page < 1)
            {
                page = 1;
            }

            // =========================================================
            // BASE QUERY
            // =========================================================

            var query =
                from o in _context.Orders

                join u in _context.Users
                    on o.UserId equals u.Id

                join cp in _context.CoinPayments
                    on o.CoinPaymentId equals cp.Id

                join oa in _context.OrderArticles
                    on o.Id equals oa.IdOrder

                join a in _context.Articles
                    on oa.IdArticle equals a.Id

                join v in _context.Vehicles
                    on a.VehicleId equals v.Id
                    into vehicleGroup

                from v in vehicleGroup.DefaultIfEmpty()

                where o.UserId == userId.Value

                select new ViewShopUser
                {
                    OrderId = o.Id,
                    DateOrder = o.DateOrder,
                    TotalPurchase = o.TotalPurchase,

                    UserId = u.Id,
                    Username = u.Username,

                    PaymentCurrency = cp.CoinName,

                    Quantity = oa.Quantity,

                    ArticleCode = a.ArticleId,
                    Measure = a.Measure,
                    Type = a.Type,
                    Friction = a.Friction,

                    UnitPrice = a.Price,
                    Discount = a.Discount,
                    Stock = a.Stock,

                    VehicleBrand =
                        v != null
                            ? v.Brand
                            : null
                };

            // =========================================================
            // DATE RANGE FILTER
            // =========================================================

            if (dateFrom.HasValue)
            {
                DateTime fromDate =
                    dateFrom.Value.Date;

                query = query.Where(x =>
                    x.DateOrder.HasValue &&
                    x.DateOrder.Value >= fromDate
                );
            }

            if (dateTo.HasValue)
            {
                // Include the entire "dateTo" day.
                DateTime toDateExclusive =
                    dateTo.Value.Date.AddDays(1);

                query = query.Where(x =>
                    x.DateOrder.HasValue &&
                    x.DateOrder.Value < toDateExclusive
                );
            }

            // =========================================================
            // TOTAL RECORDS
            // =========================================================

            int totalRecords =
                await query.CountAsync();

            // =========================================================
            // TOTAL PAGES
            // =========================================================

            int totalPages =
                (int)Math.Ceiling(
                    totalRecords /
                    (double)pageSize
                );

            if (totalPages > 0 &&
                page > totalPages)
            {
                page = totalPages;
            }

            // =========================================================
            // GET PURCHASES
            // =========================================================

            var purchases =
                await query
                    .OrderByDescending(x => x.DateOrder)
                    .ThenByDescending(x => x.OrderId)
                    .Skip(
                        (page - 1) *
                        pageSize
                    )
                    .Take(pageSize)
                    .AsNoTracking()
                    .ToListAsync();

            // =========================================================
            // VIEWBAGS
            // =========================================================

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.PageSize = pageSize;

            ViewBag.Username =
                HttpContext.Session.GetString(
                    "Username"
                ) ?? "U";

            ViewBag.DateFrom =
                dateFrom?.ToString("yyyy-MM-dd");

            ViewBag.DateTo =
                dateTo?.ToString("yyyy-MM-dd");

            // =========================================================
            // VIEW
            // =========================================================

            return View(
                "~/Views/Shop/ViewShopUser.cshtml",
                purchases
            );
        }



        // =========================================================
        // CREATE ORDER
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrder(
            int coinPaymentId,
            List<OrderItemRequest> items)
        {
            // =====================================================
            // VERIFY USER
            // =====================================================

            int? userId =
                HttpContext.Session.GetInt32("Id");


            if (userId == null)
            {
                return Json(new
                {
                    success = false,
                    message = "La sesión ha expirado."
                });
            }


            // =====================================================
            // VERIFY BUYER
            // =====================================================

            bool isEnterprise =
                HttpContext.Session.GetString(
                    "FromEnterprise"
                ) == "True";


            if (isEnterprise)
            {
                return Json(new
                {
                    success = false,
                    message =
                        "Los usuarios de empresa no pueden realizar compras."
                });
            }


            // =====================================================
            // VERIFY ITEMS
            // =====================================================

            if (items == null ||
                items.Count == 0)
            {
                return Json(new
                {
                    success = false,
                    message =
                        "El carrito está vacío."
                });
            }


            // =====================================================
            // VERIFY PAYMENT METHOD
            // =====================================================

            var paymentMethod =
                await _context.CoinPayments
                    .FirstOrDefaultAsync(
                        x => x.Id == coinPaymentId);


            if (paymentMethod == null)
            {
                return Json(new
                {
                    success = false,
                    message =
                        "El método de pago seleccionado no existe."
                });
            }


            // =====================================================
            // LOAD ARTICLES FROM DATABASE
            // =====================================================

            var articleIds =
                items
                    .Select(x => x.ArticleId)
                    .Distinct()
                    .ToList();


            var articles =
                await _context.Articles
                    .Where(
                        a => articleIds.Contains(a.Id)
                    )
                    .ToListAsync();


            if (articles.Count != articleIds.Count)
            {
                return Json(new
                {
                    success = false,
                    message =
                        "Uno o más artículos ya no existen."
                });
            }


            // =====================================================
            // CREATE ORDER
            // =====================================================

            var order = new Order
            {
                UserId =
                    userId.Value,

                DateOrder =
                    DateTime.UtcNow.Date,

                CoinPaymentId =
                    coinPaymentId,

                TotalPurchase =
                    0m
            };


            _context.Orders.Add(order);

            await _context.SaveChangesAsync();


            // =====================================================
            // CREATE ORDER ARTICLES
            // =====================================================

            decimal totalPurchase = 0m;


            foreach (var item in items)
            {
                // -------------------------------------------------
                // VALIDATE QUANTITY
                // -------------------------------------------------

                if (item.Quantity <= 0)
                {
                    continue;
                }


                // -------------------------------------------------
                // FIND ARTICLE
                // -------------------------------------------------

                var article =
                    articles.First(
                        a => a.Id == item.ArticleId
                    );


                // -------------------------------------------------
                // CHECK STOCK
                // -------------------------------------------------

                if (article.Stock != true)
                {
                    await DeleteEmptyOrder(
                        order.Id
                    );

                    return Json(new
                    {
                        success = false,

                        message =
                            $"El artículo {article.ArticleId ?? article.Id.ToString()} no tiene stock."
                    });
                }


                // -------------------------------------------------
                // CALCULATE PRICE
                // -------------------------------------------------

                decimal price =
                    article.Price ?? 0m;


                decimal discount =
                    article.Discount ?? 0m;


                decimal finalPrice =
                    price -
                    (price * discount / 100m);


                // -------------------------------------------------
                // CALCULATE SUBTOTAL
                // -------------------------------------------------

                decimal subtotal =
                    finalPrice *
                    item.Quantity;


                totalPurchase +=
                    subtotal;


                // -------------------------------------------------
                // CREATE ORDER ARTICLE
                // -------------------------------------------------

                var orderArticle =
                    new OrderArticle
                    {
                        IdOrder =
                            order.Id,

                        IdArticle =
                            article.Id,

                        Quantity =
                            item.Quantity
                    };


                _context.OrderArticles.Add(
                    orderArticle
                );
            }


            // =====================================================
            // VERIFY TOTAL
            // =====================================================

            if (totalPurchase <= 0)
            {
                await DeleteEmptyOrder(
                    order.Id
                );

                return Json(new
                {
                    success = false,
                    message =
                        "No se pudo calcular el total de la compra."
                });
            }


            // =====================================================
            // UPDATE ORDER TOTAL
            // =====================================================

            order.TotalPurchase =
                totalPurchase;


            _context.Orders.Update(
                order
            );


            // =====================================================
            // SAVE ORDER + ORDER ARTICLES
            // =====================================================

            await _context.SaveChangesAsync();

            // =====================================================
            // CREATE NOTIFICATION
            // =====================================================

            var notification =
                new Notification
                {
                    UserId =
                        userId.Value,

                    OrderId =
                        order.Id,

                    Title =
                        "Nueva compra",

                    Message =
                        $"Nueva compra registrada. Orden #{order.Id}. Total: {totalPurchase:N2}",

                    IsRead =
                        false,

                    CreatedAt =
                        DateTime.UtcNow
                };


            _context.Notifications.Add(
                notification
            );



            // Save notification first so notification.Id
            // is generated by the database.
            await _context.SaveChangesAsync();


            // =====================================================
            // GET ENTERPRISE USERS
            // =====================================================

            var enterpriseUsers =
                await _context.Users
                    .Where(
                        u => u.FromEnterprise == true
                    )
                    .Select(
                        u => u.Id
                    )
                    .ToListAsync();


            // =====================================================
            // CREATE NOTIFICATION USERS
            // =====================================================

            foreach (var enterpriseUserId in enterpriseUsers)
            {
                var notificationUser =
                    new NotificationUser
                    {
                        NotificationId =
                            notification.Id,

                        UserId =
                            userId.Value,

                        IsRead =
                            false
                    };


                _context.NotificationUsers.Add(
                    notificationUser
                );
            }


            // =====================================================
            // SAVE NOTIFICATION USERS
            // =====================================================

            await _context.SaveChangesAsync();


            // =====================================================
            // SUCCESS
            // =====================================================

            return Json(new
            {
                success = true,

                orderId =
                    order.Id,

                total =
                    totalPurchase,

                message =
                    "La compra fue registrada correctamente."
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePurchase(int orderId)
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

            var orderArticles =
                await _context.OrderArticles
                    .Where(x => x.IdOrder == orderId)
                    .ToListAsync();

            if (orderArticles.Any())
            {
                _context.OrderArticles.RemoveRange(
                    orderArticles
                );
            }

            var order =
                await _context.Orders
                    .FirstOrDefaultAsync(x => x.Id == orderId);

            if (order != null)
            {
                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "ViewShopEnterprise"
            );
        }


        // =========================================================
        // DELETE EMPTY ORDER
        // =========================================================

        private async Task DeleteEmptyOrder(
            int orderId)
        {
            var order =
                await _context.Orders
                    .FirstOrDefaultAsync(
                        x => x.Id == orderId
                    );


            if (order != null)
            {
                _context.Orders.Remove(
                    order
                );

                await _context.SaveChangesAsync();
            }
        }

        // SetClientDebtor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetClientDebtor(int orderId, int userId)
        {
            string? sessionId =
                HttpContext.Session.GetString("Id");

            string? fromEnterprise =
                HttpContext.Session.GetString("FromEnterprise");


            // =========================================================
            // SESSION VALIDATION
            // =========================================================

            if (string.IsNullOrEmpty(sessionId) ||
                string.IsNullOrEmpty(fromEnterprise) ||
                fromEnterprise != "True")
            {
                return RedirectToAction(
                    "Login",
                    "Users"
                );
            }


            // =========================================================
            // SEARCH ORDER
            // =========================================================

            var order =
                await _context.Orders
                    .FirstOrDefaultAsync(x => x.Id == orderId);

            if (order == null)
            {
                return RedirectToAction(
                    "ViewShopEnterprise"
                );
            }


            // =========================================================
            // MARK ORDER AS DEBTOR CLIENT
            // =========================================================

            order.IsDebtorClient = true;


            // =========================================================
            // CREATE CLIENT NOTIFICATION
            // =========================================================

            var notification = new NotificationClient
            {
                UserId = order.UserId,
                OrderId = order.Id,

                Title = "Compra marcada como deudora",

                Message =
                    $"La compra #{order.Id} ha sido marcada como pendiente de pago.",

                IsRead = false,

                CreatedAt = DateTime.UtcNow
            };

            _context.NotificationsClient.Add(notification);

            // Save first so the notification receives its ID
            await _context.SaveChangesAsync();


            // =========================================================
            // CREATE NOTIFICATION USER CLIENT
            // =========================================================

            var notificationUser =
                new NotificationUserClient
                {
                    NotificationId = notification.Id,

                    UserId = order.UserId,

                    IsRead = false
                };

            _context.NotificationUsersClient.Add(notificationUser);


            // =========================================================
            // SAVE
            // =========================================================

            await _context.SaveChangesAsync();


            // =========================================================
            // REDIRECT
            // =========================================================

            return RedirectToAction(
                "ViewShopEnterprise"
            );
        }
    }


    // =============================================================
    // ORDER ITEM REQUEST
    // =============================================================

    public class OrderItemRequest
    {
        public int ArticleId { get; set; }

        public int Quantity { get; set; }
    }
}