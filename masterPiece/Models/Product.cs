using System;
using System.Collections.Generic;

namespace masterPiece.Models;

public partial class Product
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int QuantityAvailable { get; set; }

    public string Category { get; set; } = null!;

    public bool? IsActive { get; set; }

    public string? ImagePath { get; set; } // صورة المنتج


    public virtual ICollection<CartDetail> CartDetails { get; set; } = new List<CartDetail>();

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual User User { get; set; } = null!;
}
