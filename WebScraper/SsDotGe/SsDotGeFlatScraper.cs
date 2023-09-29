using HtmlAgilityPack;
using System.Globalization;
using System.Text.RegularExpressions;
using WebScraper.Interfaces;
using WebScraper.Models;

namespace WebScraper.SsDotGe
{
    public class SsDotGeFlatScraper : IFlatScraper
    {
        public DateTime GetFlatCreationOrMinDate(HtmlDocument mainPage, int htmlDivNumber)
        {
            var formatInputDate = "dd.MM.yyyy/HH:mm";
            var minDateInFormat = DateTime.MinValue.ToString(formatInputDate);
            var inputDate = mainPage.DocumentNode.SelectSingleNode(
                    $"//*[@id=\"list\"]/div[{htmlDivNumber}]/div[1]/div[1]/div[1]/div[2]/div[2]/div/div[2]/div/div[1]/text()")?
                .InnerText.Replace(" ", "")
                .Replace("\r\n", "");
            if (inputDate == null)
            {
                inputDate = mainPage.DocumentNode.SelectSingleNode(
                        $"//*[@id=\"list\"]/div[{htmlDivNumber}]/div[1]/div[1]/div[1]/div[2]/div[2]/div/div/div[1]/text()")?
                    .InnerText.Replace(" ", "")
                    .Replace("\r\n", "") ?? minDateInFormat;
            }

            return DateTime.ParseExact(inputDate, formatInputDate, CultureInfo.InvariantCulture);
        }

        public string GetFlatTitle(HtmlDocument mainPage, int htmlDivNumber)
        {
            return mainPage.DocumentNode
                .SelectSingleNode($"//*[@id=\"list\"]/div[{htmlDivNumber}]/div[1]/div[1]/div[1]/div[2]/div[1]/a/div/span")
                ?.InnerText ?? "No title";
        }

        public int GetFlatCost(HtmlDocument mainPage, int htmlDivNumber)
        {
            var inputCost = mainPage.DocumentNode.SelectSingleNode(
                $"//*[@id=\"list\"]/div[{htmlDivNumber}]/div[1]/div[1]/div[2]/div[2]/div[1]/text()")?.InnerText;

            if (inputCost != null) inputCost = Regex.Replace(inputCost, @"[^\d\s]+", string.Empty);

            return int.TryParse(inputCost, out var result) ? result : int.MaxValue;
        }

        public string GetFLatLink(HtmlDocument mainPage, string url, int htmlDivNumber)
        {
            HtmlNode nextPage = mainPage.DocumentNode.SelectSingleNode($"//*[@id=\"list\"]/div[{htmlDivNumber}]/div[1]/div[1]/div[1]/div[2]/div[1]/a");
            var uri = new Uri(url);
            return nextPage.GetAttributeValue<string>("href", null);
        }

        public string GetFlatDescription(HtmlDocument flatPage, int descriptionLength)
        {
            var input = flatPage.DocumentNode.SelectNodes(
                    "//span[contains(@class,'details_text')]")?
                .FirstOrDefault()?
                .InnerText.Replace("\r\n", "");

            if (string.IsNullOrWhiteSpace(input)) return "No description";

            var removeWhitespace = Regex.Replace(input, @"\s{2,}", " ");

            if (removeWhitespace.Length <= descriptionLength) return removeWhitespace;

            var description = removeWhitespace.Substring(0, descriptionLength - 3) + "...";

            return description;
        }

        public string GetFlatOwnerPhoneNumber(HtmlDocument flatPage)
        {
            var number = flatPage.DocumentNode.SelectSingleNode(
                    "//*[@id=\"main-body\"]/div[2]/div[2]/div[1]/div[1]/div[7]/div/div/div/div[5]/div/div/div[3]/div/div[1]/div[3]/a/span")
                ?.InnerText;

            if (number == null)
            {
                number = flatPage.DocumentNode
                    .SelectNodes(
                        "//div[contains(@class,'UserMObileNumbersBlock')]//a")?
                    .FirstOrDefault()?.InnerText.Replace("\r\n", "") ?? "No number";
            }

            return number.Replace(" ", "");
        }

        public List<string> GetFlatImages(HtmlDocument flatPage)
        {
            var imagesUrl = flatPage.DocumentNode.Descendants("img")
                .Where(e => e.Attributes["class"]?.Value == "img-responsive")
                .Select(e => e.GetAttributeValue("src", null))
                .Where(s => !String.IsNullOrEmpty(s) && !s.Contains("Thumb") && s.Contains("static.ss.ge"))
                .Select(s => Regex.Replace(s, @"(?<!/)/(?!/)", "//"))
                .ToList() ?? new List<string>();

            if (imagesUrl.Count == 0)
            {
                var linkBlurredImageIfFlatNotHaveImages =
                    "https://media.istockphoto.com/id/955951212/photo/blurred-background-modern-kitchen-and-dinning-room-in-house-with-bokeh-light-lifestyle-backdrop.jpg?s=612x612&w=0&k=20&c=THHBhrRhOCnD0DdfLj42JNsDuzZpC0oqp7K0EIO4B8U=";
                imagesUrl.Add(linkBlurredImageIfFlatNotHaveImages);
            }

            return imagesUrl;
        }

        public int GetPageViews(HtmlDocument flatPage)
        {
            var viewsFromPage = flatPage.DocumentNode
                .SelectSingleNode("//*[@id=\"main-body\"]/div[2]/div[2]/div[1]/div[1]/div[6]/div/div[1]/div[2]/div[1]/span")
                ?.InnerText;

            if (viewsFromPage != null) Regex.Replace(viewsFromPage, @"[^\d\s]+", string.Empty);

            return int.TryParse(viewsFromPage, out var result) ? result : int.MaxValue;
        }

        public FlatCoordinateModel GetFlatCoordinate(HtmlDocument flatPage)
        {
            return new FlatCoordinateModel().GetDefaultFlatCoordinate();
        }

        public ComfortStuffModel GetComfortStuff(HtmlDocument flatPage)
        {
            var paramsBotBlk = flatPage.DocumentNode
                .SelectNodes("//div[contains(@class,'ParamsBotBlk')]")?
                .ToList();

            var paramsHdBlk = flatPage.DocumentNode
                .SelectNodes("//div[contains(@class,'ParamsHdBlk')]//text")?
                .ToList();

            var indexBedrooms = paramsBotBlk?.FindIndex(e => e.InnerText == "Bedrooms") ?? 0;
            var bedrooms = paramsHdBlk?.ElementAtOrDefault(indexBedrooms)?.InnerText ?? "No bedrooms";

            var indexFloors = paramsBotBlk?.FindIndex(e => e.InnerText == "Floor") ?? 0;
            var floor = paramsHdBlk?.ElementAtOrDefault(indexFloors)?.InnerText.Replace(" ", "").Replace("\r\n", "") ?? "No floors";

            var indexTotalArea = paramsBotBlk?.FindIndex(e => e.InnerText == "Total Area") ?? 0;
            var totalArea = paramsHdBlk?.ElementAtOrDefault(indexTotalArea)?.InnerText ?? "No total area";

            var additionalInfo = flatPage.DocumentNode
                .SelectNodes("//div[contains(@class,'col-md-6 col-xs-6 parameteres_item_each')]")?
                .ToList();

            var indexGas = additionalInfo?.FindIndex(e => e.InnerText.Contains("Natural gas")) ?? 0;
            var indexHotWater = additionalInfo?.FindIndex(e => e.InnerText.Contains("Hot water")) ?? 0;
            var indexConditioner = additionalInfo?.FindIndex(e => e.InnerText.Contains("Air conditioning")) ?? 0;


            var spans = flatPage.DocumentNode
                .SelectNodes("//div[contains(@class,'col-md-6 col-xs-6 parameteres_item_each')]//span")?
                .ToList();
            var isThereGas = spans?.ElementAtOrDefault(indexGas)?.GetAttributeValue("class", "UnCheckedParam") == "CheckedParam";

            var isThereHotWater = spans?.ElementAtOrDefault(indexHotWater)?.GetAttributeValue("class", "UnCheckedParam") == "CheckedParam";

            var isThereConditioner = spans?.ElementAtOrDefault(indexConditioner)?.GetAttributeValue("class", "UnCheckedParam") == "CheckedParam";

            return new ComfortStuffModel(bedrooms, floor, totalArea, isThereGas, isThereHotWater, isThereConditioner);
        }
    }
}