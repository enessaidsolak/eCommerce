using System;
using System.Collections.Generic;

namespace eCommerce.Entity;

public partial class Address
{
    public int AddressId { get; set; }

    public int? UserId { get; set; }

    public string? AddressLine { get; set; }

    public string? City { get; set; }

    public string? PostalCode { get; set; }

    public string? Country { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual User? User { get; set; }
}
