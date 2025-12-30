using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Auth.Dtos.Billing
{
    public class CreatePlanRequestDto
    {
        public string code { get; set; } = default!;

        public string name { get; set; } = default!;

        public string? description { get; set; }

        public bool isActive { get; set; } = true;
        public int roleID { get; set; }
    }
    public class UpdatePlanRequestDto : CreatePlanRequestDto
    {
        [Required]
        public int planID { get; set; }
    }

    public class CreatePriceRequestDto
    {
        public int planID { get; set; }
        public string currency { get; set; } = "VND";
        public decimal amount { get; set; }
        public string intervalUnit { get; set; } = "month";

        public int intervalCount { get; set; } // 1 | 3 | 6
        public int? trialDays { get; set; }
        public bool isActive { get; set; } = true;
    }
    public class UpdatePriceRequestDto : CreatePriceRequestDto
    {
        public int priceID { get; set; }
    }


}
