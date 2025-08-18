using System;
using System.Collections.Generic;

namespace eCommerce.DATA.Entity;

public partial class CouponUsage
{
    public int Id { get; set; }

    public int CouponId { get; set; }

    public int UserId { get; set; }

    public DateTime UsedAt { get; set; }
}
