using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FZ.Auth.Domain.User;

namespace FZ.Auth.Domain.Billing
{
    [Table(nameof(Order), Schema = Constant.Database.DbSchema.Auth)]
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int orderID { get; set; }

        public int userID { get; set; }
        public int planID { get; set; }
        public int priceID { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal amount { get; set; }

        [Required, MaxLength(8)]
        public string currency { get; set; } = "VND";

        [Required, MaxLength(24)]
        public string status { get; set; } = "pending"; // pending|paid|failed|expired

        [Required, MaxLength(32)]
        public string provider { get; set; } = default!; // momo|zalopay|vnpay|stripe|...

        [Required, MaxLength(128)]
        public string providerSessionId { get; set; } = default!; // UNIQUE theo provider

        public DateTime createdAt { get; set; }
        public DateTime? expiresAt { get; set; }

        // Navigations
        public virtual AuthUser user { get; set; } = default!;
        public virtual Plan plan { get; set; } = default!;
        public virtual Price price { get; set; } = default!;
        public virtual ICollection<Invoice> invoices { get; set; } = new List<Invoice>();
    }

    [Table(nameof(Invoice), Schema = Constant.Database.DbSchema.Auth)]
    public class Invoice
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int invoiceID { get; set; }

        public int userID { get; set; }
        public int? subscriptionID { get; set; }   // SetNull
        public int? orderID { get; set; }          // SetNull

        [Column(TypeName = "decimal(18,2)")]
        public decimal subtotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal discount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal tax { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal total { get; set; }

        public DateTime issuedAt { get; set; }
        public DateTime? dueAt { get; set; }

        [MaxLength(512)]
        public string? pdfUrl { get; set; }

        // Navigations
        public virtual AuthUser user { get; set; } = default!;
        public virtual UserSubscription? subscription { get; set; }
        public virtual Order? order { get; set; }
        public virtual ICollection<Payment> payments { get; set; } = new List<Payment>();
    }

    [Table(nameof(Payment), Schema = Constant.Database.DbSchema.Auth)]
    public class Payment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int paymentID { get; set; }

        public int invoiceID { get; set; }

        [Required, MaxLength(32)]
        public string provider { get; set; } = default!;

        [MaxLength(128)]
        public string? providerPaymentId { get; set; } // UNIQUE khi có

        [Required, MaxLength(24)]
        public string status { get; set; } = "pending"; // pending|succeeded|failed|refunded

        public DateTime createdAt { get; set; }
        public DateTime? paidAt { get; set; }

        [MaxLength(256)]
        public string? failureReason { get; set; }

        // Navigation
        public virtual Invoice invoice { get; set; } = default!;
    }
}
