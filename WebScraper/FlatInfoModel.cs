using System.ComponentModel.DataAnnotations;

namespace WebScraper
{
    public class FlatInfoModel
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public int Cost { get; set; }

        public DateTime Date { get; set; }

        public string Description { get; set; }

        public string PhoneNumber { get; set; }

        public List<string> LinksOfImages { get; set; }

        public string PageLink { get; set; }

        public int ViewsOnSite { get; set; }

        public Coordinate Coordinate { get; set; }

        public FlatInfoModel(string title, int cost, DateTime date, string description, string phoneNumber, List<string> linksOfImage, string adLink, int viewsOnSite, Coordinate coordinate)
        {
            Title = title;
            Cost = cost;
            Date = date;
            Description = description ?? "No description";
            PhoneNumber = phoneNumber ?? "No phone number";
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
                "No phone number",
                null,
                "No link",
                int.MaxValue,
                new Coordinate().GetDefaultCoordinate());
        }
    }
}