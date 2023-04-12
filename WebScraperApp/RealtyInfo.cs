using System;
using System.CodeDom;
using System.Collections.Generic;

namespace WebScraperApp
{
    public class RealtyInfo
    {
        public string Title { get; set; }

        public int Cost { get; set; }

        public DateTime Date { get; set; }

        public string Description { get; set; }

        public string PhoneNumber { get; set; }

        public List<string> LinksOfImages { get; set; }

        public string AdLink { get; set; }

        public RealtyInfo(string title, int cost, DateTime date, string description, string phoneNumber, List<string> linksOfImage, string adLink)
        {
            Title = title ?? "No title";
            Cost = cost;
            Date = date;
            Description = description ?? "No description";
            PhoneNumber = phoneNumber ?? "No phone number";
            LinksOfImages = linksOfImage;
            AdLink = adLink ?? "No link";

        }

        public RealtyInfo GetDefaultAd()
        {
            return new RealtyInfo(
                "No title",
                default,
                default,
                "No description",
                "No phone number",
                null,
                "No link");
        }
    }
}
