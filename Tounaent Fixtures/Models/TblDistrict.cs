using System;
using System.Collections.Generic;

namespace Tounaent_Fixtures.Models;

public partial class TblDistrict
{
    public int DistictId { get; set; }

    public string DistictName { get; set; } = null!;

    public bool IsActive { get; set; }

    public int StateId { get; set; }

    public DateTime? AddedDt { get; set; }

    public string? AddedBy { get; set; }

    public DateTime? ModifyDt { get; set; }

    public string? ModifyBy { get; set; }
}
