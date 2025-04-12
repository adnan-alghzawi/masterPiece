using System;
using System.Collections.Generic;

namespace masterPiece.Models;

public partial class Cart
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public virtual ICollection<CartDetail> CartDetails { get; set; } = new List<CartDetail>();

    public virtual User User { get; set; } = null!;
}
