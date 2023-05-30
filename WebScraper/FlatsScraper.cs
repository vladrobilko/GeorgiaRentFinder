using HtmlAgilityPack;
using WebScraper.Interfaces;
using WebScraper.Models;

namespace WebScraper
{
    public class FlatsScraper
    {
        private readonly IFlatScraper _flatScraper;
        private readonly int _flatsOnPage;
        private readonly int _flatLowestPrice;
        private readonly int _flatHighestPrice;

        public FlatsScraper(IFlatScraper flatScraper, int flatOnPage, int flatLowestPrice, int flatHighestPrice)
        {
            _flatsOnPage = flatOnPage;
            _flatLowestPrice = flatLowestPrice;
            _flatHighestPrice = flatHighestPrice;
            _flatScraper = flatScraper;
        }

        public List<FlatInfoModel> ScrapAllPagesWithAllFlats(string url, DateTime lastCheckDate)
        {
            var flats = new List<FlatInfoModel>();

            HtmlDocument mainPage = GetHtmlDocumentForPage(url);

            ScrapPage(url, lastCheckDate, mainPage, flats);

            return flats;
        }

        private void ScrapPage(string url, DateTime lastCheckDate, HtmlDocument mainPage, List<FlatInfoModel> flats)
        {
            for (int i = 0, j = 0; i < _flatsOnPage; j++)
            {
                if (j > _flatsOnPage * 2) break;

                var flatCreationDate = _flatScraper.GetFlatCreationOrMinDate(mainPage, j);

                if (flatCreationDate < lastCheckDate) continue;

                var flatTitle = _flatScraper.GetFlatTitle(mainPage, j);

                var flatCost = _flatScraper.GetFlatCost(mainPage, j);

                if (IsFlatSuit(flatTitle, flatCost))
                {
                    var flatLink = _flatScraper.GetFLatLink(mainPage, url, j);

                    flats.Add(GetFlatInfoModel(flatLink, flatTitle, flatCost, flatCreationDate));
                    i++;
                }

                if (flatTitle != "No title" && !IsFlatSuit(flatTitle, flatCost)) i++;
            }
        }

        private FlatInfoModel GetFlatInfoModel(string flatLink, string flatTitle, int flatCost, DateTime flatCreationDate)
        {
            HtmlDocument flatPage = GetHtmlDocumentForPage(flatLink);

            var flatDescription = _flatScraper.GetFlatDescription(flatPage, 200);

            var flatOwnerPhoneNumber = _flatScraper.GetFlatOwnerPhoneNumber(flatPage);

            var firstTenImagesFromFlatPage = _flatScraper.GetFlatImages(flatPage);

            var pageViews = _flatScraper.GetPageViews(flatPage);

            return new FlatInfoModel(
                flatTitle,
                flatCost,
                flatCreationDate,
                flatDescription,
                flatOwnerPhoneNumber,
                firstTenImagesFromFlatPage,
                flatLink,
                pageViews,
                _flatScraper.GetFlatCoordinate(flatPage),
                _flatScraper.GetComfortStuff(flatPage)
            );
        }

        private HtmlDocument GetHtmlDocumentForPage(string pageUrl)
        {
            var webForMainPage = new HtmlWeb();
            HtmlDocument mainPage = webForMainPage.Load(pageUrl);
            return mainPage;
        }

        private bool IsFlatSuit(string flatTitle, int flatCost)
        {
            return flatTitle != "No title" && flatCost > _flatLowestPrice && flatCost < _flatHighestPrice;
        }
    }
}