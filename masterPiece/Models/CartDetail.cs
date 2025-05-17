using System;
using System.Collections.Generic;

namespace masterPiece.Models;

public partial class CartDetail
{
    public int ID { get; set; }

    public int CartID { get; set; }

    public int ProductID { get; set; }

    public int Quantity { get; set; }

    public virtual Cart Cart { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
