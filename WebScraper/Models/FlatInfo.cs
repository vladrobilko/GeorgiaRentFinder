using System.ComponentModel.DataAnnotations;

namespace WebScraper.Models
{
    public class FlatInfoModel
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public int Cost { get; set; }

        public DateTime Date { get; set; }

        public string Description { get; set; }

        public FlatPhoneTracker FlatPhoneTracker { get; set; }

        public List<string> LinksOfImages { get; set; }

        public string PageLink { get; set; }

        public int ViewsOnSite { get; set; }

        public Coordinate Coordinate { get; set; }

        public FlatInfoModel(string title, int cost, DateTime date, string description, FlatPhoneTracker flatPhoneTracker, List<string> linksOfImage, string adLink, int viewsOnSite, Coordinate coordinate)
        {
            Title = title;
            Cost = cost;
            Date = date;
            Description = description ?? "No description";
            FlatPhoneTracker = flatPhoneTracker ?? new FlatPhoneTracker() { PhoneNumber = "No phone number", CountMentionsOnSites = 0 };
            LinksOfImages = linksOfImage;
            PageLink = adLink ?? "No link";
            ViewsOnSite = viewsOnSite;
            Coordinate = coordinate ?? new Coordinate().GetDefaultCoordinate();
        }

        public FlatInfoModel() { }

        public FlatInfoModel GetDefaultFlatInfoModel()
        {
            return new FlatInfoModel(
                "Default",
                int.MaxValue,
                DateTime.MaxValue,
                "No description",
                new FlatPhoneTracker(){PhoneNumber = "No phone number", CountMentionsOnSites = 0},
                null,
                "No link",
                int.MaxValue,
                new Coordinate().GetDefaultCoordinate());
        }
    }
}