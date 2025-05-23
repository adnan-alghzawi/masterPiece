﻿using System;
using System.Collections.Generic;

namespace masterPiece.Models;

public partial class Subscription
{
    public int ID { get; set; }

    public int UserID { get; set; }

    public string PlanName { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool? IsActive { get; set; }

    public virtual User User { get; set; } = null!;
}
