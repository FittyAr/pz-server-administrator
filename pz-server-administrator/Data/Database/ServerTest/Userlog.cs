using System;
using System.Collections.Generic;

namespace pz_server_administrator.Data.Database.ServerTest;

public partial class Userlog
{
    public int Id { get; set; }

    public string? Username { get; set; }

    public string? Type { get; set; }

    public string? Text { get; set; }

    public string? IssuedBy { get; set; }

    public int? Amount { get; set; }

    public string? LastUpdate { get; set; }
}
