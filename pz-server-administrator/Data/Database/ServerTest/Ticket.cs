using System;
using System.Collections.Generic;

namespace pz_server_administrator.Data.Database.ServerTest;

public partial class Ticket
{
    public int Id { get; set; }

    public string Message { get; set; } = null!;

    public string Author { get; set; } = null!;

    public int? AnsweredId { get; set; }

    public bool? Viewed { get; set; }
}
