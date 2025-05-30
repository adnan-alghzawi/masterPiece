﻿using System;
using System.Collections.Generic;

namespace masterPiece.Models;

public partial class OrderDetail
{
    public int ID { get; set; }

    public int OrderID { get; set; }

    public int ProductID { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
