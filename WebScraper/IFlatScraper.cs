﻿using HtmlAgilityPack;
using WebScraper.Models;

namespace WebScraper
{
    public interface IFlatScraper
    {
        DateTime GetFlatCreationOrMinDate(HtmlDocument mainPage, int htmlDivNumber);

        string GetFlatTitle(HtmlDocument mainPage, int htmlDivNumber);

        int GetFlatCost(HtmlDocument mainPage, int htmlDivNumber);

        string GetFLatLink(HtmlDocument mainPage, string url, int htmlDivNumber);

        string GetFlatDescription(HtmlDocument flatPage, int descriptionLength);

        string GetFlatOwnerPhoneNumber(HtmlDocument flatPage);

        List<string> GetFirstTenImages(HtmlDocument flatPage);

        int GetPageViews(HtmlDocument flatPage);

        FlatCoordinate GetFlatCoordinate(HtmlDocument flatPage);
    }
}
