using System;
using System.Collections.Generic;

namespace WebScraperApp
{
    public class RealtyInfo
    {
        public string Title { get; set; } = "No title";

        public int Cost { get; set; } = 0;

        public DateTime? Data { get; set; } = null;

        public string Description { get; set; } = "No description";

        public string PhoneNumber { get; set; } = "No phone number";

        public List<string> LinkOfImages { get; set; }
    }
}
