using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FZ.Auth.Domain.User;

namespace FZ.Auth.Domain.Billing
{
    [Table(nameof(Plan), Schema = Constant.Database.DbSchema.Auth)]
    public class Plan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int planID { get; set; }

        [Required, MaxLength(64)]
        public string code { get; set; } = default!;

        [Required, MaxLength(128)]
        public string name { get; set; } = default!;

        [MaxLength(512)]
        public string? description { get; set; }

        public bool isActive { get; set; } = true;

        public virtual ICollection<Price> prices { get; set; } = new List<Price>();
        public virtual ICollection<Order> orders { get; set; } = new List<Order>(); // inverse cho Order.plan
    }

    [Table(nameof(Price), Schema = Constant.Database.DbSchema.Auth)]
    public class Price
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int priceID { get; set; }

        public int planID { get; set; }

        [Required, MaxLength(8)]
        public string currency { get; set; } = "VND";

        [Column(TypeName = "decimal(18,2)")]
        public decimal amount { get; set; }

        [Required, MaxLength(16)]
        public string intervalUnit { get; set; } = "month";

        public int intervalCount { get; set; } // 1 | 3 | 6

        public int? trialDays { get; set; }

        public bool isActive { get; set; } = true;

        public virtual Plan plan { get; set; } = default!;
        public virtual ICollection<Order> orders { get; set; } = new List<Order>(); // inverse cho Order.price
    }

    [Table(nameof(UserSubscription), Schema = Constant.Database.DbSchema.Auth)]
    public class UserSubscription
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int subscriptionID { get; set; }

        public int userID { get; set; }
        public int planID { get; set; }
        public int? priceID { get; set; } // optional để audit

        [Required, MaxLength(24)]
        public string status { get; set; } = "active"; // active|trialing|past_due|canceled|expired|paused

        public bool autoRenew { get; set; }

        public DateTime startAt { get; set; }
        public DateTime? trialEndAt { get; set; }

        public DateTime currentPeriodStart { get; set; }
        public DateTime currentPeriodEnd { get; set; }

        public DateTime? cancelAt { get; set; }
        public bool cancelAtPeriodEnd { get; set; }

        // Navigations
        public virtual AuthUser user { get; set; } = default!;
        public virtual Plan plan { get; set; } = default!;
        public virtual Price? price { get; set; }
        public virtual ICollection<Invoice> invoices { get; set; } = new List<Invoice>();
    }
}
