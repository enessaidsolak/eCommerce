using System;
using System.Collections.Generic;

namespace eCommerce.DATA.Entity;

public partial class Coupon
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public decimal DiscountRate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public bool IsActive { get; set; }

    public int UsageLimit { get; set; }

    public int UsageCount { get; set; }
}
