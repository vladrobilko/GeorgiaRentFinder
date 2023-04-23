using System;
using System.Collections.Generic;

namespace DataManagement.Models;

public partial class FlatCoordinateDto
{
    public long FlatInfoId { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }
}
