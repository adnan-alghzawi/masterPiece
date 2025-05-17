using System;
using System.Collections.Generic;

namespace masterPiece.Models;

public partial class Payment
{
    public int ID { get; set; }

    public int OrderID { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public DateTime? PaymentDate { get; set; }

    public string Status { get; set; } = null!;

    public string? TransactionID { get; set; }

    public virtual Order Order { get; set; } = null!;
}
