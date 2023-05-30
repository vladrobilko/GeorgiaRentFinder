using HtmlAgilityPack;
using WebScraper.Models;

namespace WebScraper.Interfaces
{
    public interface IFlatScraper
    {
        DateTime GetFlatCreationOrMinDate(HtmlDocument mainPage, int htmlDivNumber);

        string GetFlatTitle(HtmlDocument mainPage, int htmlDivNumber);

        int GetFlatCost(HtmlDocument mainPage, int htmlDivNumber);

        string GetFLatLink(HtmlDocument mainPage, string url, int htmlDivNumber);

        string GetFlatDescription(HtmlDocument flatPage, int descriptionLength);

        string GetFlatOwnerPhoneNumber(HtmlDocument flatPage);

        List<string> GetFlatImages(HtmlDocument flatPage);

        int GetPageViews(HtmlDocument flatPage);

        FlatCoordinateModel GetFlatCoordinate(HtmlDocument flatPage);

        ComfortStuffModel GetComfortStuff(HtmlDocument flatPage);
    }
}
