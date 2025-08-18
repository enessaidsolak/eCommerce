using System;
using System.Collections.Generic;

namespace eCommerce.DATA.Entity;

public partial class EmailSetting
{
    public int Id { get; set; }

    public string? Host { get; set; }

    public int Port { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string? FromEmail { get; set; }

    public string? FromName { get; set; }
}
