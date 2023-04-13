﻿using System;
using System.Collections.Generic;

namespace WebScraperApp
{
    public class FlatInfo
    {
        public string Title { get; set; }

        public int Cost { get; set; }

        public DateTime Date { get; set; }

        public string Description { get; set; }

        public string PhoneNumber { get; set; }

        public List<string> LinksOfImages { get; set; }

        public string AdLink { get; set; }

        public int PageViews { get; set; }

        public FlatInfo(string title, int cost, DateTime date, string description, string phoneNumber, List<string> linksOfImage, string adLink, int pageViews)
        {
            Title = title ?? "No title";
            Cost = cost;
            Date = date;
            Description = description ?? "No description";
            PhoneNumber = phoneNumber ?? "No phone number";
            LinksOfImages = linksOfImage;
            AdLink = adLink ?? "No link";
            PageViews = pageViews;
        }

        public FlatInfo GetDefaultAd()
        {
            return new FlatInfo(
                "No title",
                int.MaxValue,
                DateTime.MaxValue, 
                "No description",
                "No phone number",
                null,
                "No link",
                int.MaxValue);
        }
    }
}