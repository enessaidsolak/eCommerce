using System;
using System.Collections.Generic;

namespace eCommerce.DATA.Entity;

public partial class ProductComment
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int ProductId { get; set; }

    public string? Comment { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? Rating { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
