using System;
using System.Collections.Generic;

namespace eCommerce.DATA.Entity;

public partial class CouponUsage
{
    public int Id { get; set; }

    public int CouponId { get; set; }

    public string UserId { get; set; } = null!;

    public DateTime UsedAt { get; set; }
}
