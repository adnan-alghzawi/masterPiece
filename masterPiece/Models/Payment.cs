﻿using System;
using System.Collections.Generic;

namespace masterPiece.Models;

public partial class Payment
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public DateTime? PaymentDate { get; set; }

    public string Status { get; set; } = null!;

    public string? TransactionId { get; set; }

    public virtual Order Order { get; set; } = null!;
}
