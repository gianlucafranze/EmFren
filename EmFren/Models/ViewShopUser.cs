using System;

namespace EmFren.Models
{
    public class ViewShopUser
    {
        // =========================================================
        // ORDER
        // =========================================================

        public int OrderId { get; set; }

        public DateTime? DateOrder { get; set; }

        public decimal? TotalPurchase { get; set; }

        public bool? IsDebtorClient { get; set; }


        // =========================================================
        // USER
        // =========================================================

        public int UserId { get; set; }

        public string? Username { get; set; }


        // =========================================================
        // PAYMENT
        // =========================================================

        public string? PaymentCurrency { get; set; }


        // =========================================================
        // ORDER ARTICLE
        // =========================================================

        public int? Quantity { get; set; }


        // =========================================================
        // ARTICLE
        // =========================================================

        public string? ArticleCode { get; set; }

        public string? Measure { get; set; }

        public string? Type { get; set; }

        public string? Friction { get; set; }

        public decimal? UnitPrice { get; set; }

        public decimal? Discount { get; set; }

        public bool? Stock { get; set; }


        // =========================================================
        // VEHICLE
        // =========================================================

        public string? VehicleBrand { get; set; }
    }
}