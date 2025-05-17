using System;
using System.Collections.Generic;

namespace masterPiece.Models;

public partial class Review
{
    public int ID { get; set; }

    public int OrderID { get; set; }

    public int ProductID { get; set; }

    public int UserID { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
