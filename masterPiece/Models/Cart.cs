using System;
using System.Collections.Generic;

namespace masterPiece.Models;

public partial class Cart
{
    public int ID { get; set; }

    public int UserID { get; set; }

    public virtual ICollection<CartDetail> CartDetails { get; set; } = new List<CartDetail>();

    public virtual User User { get; set; } = null!;
}
