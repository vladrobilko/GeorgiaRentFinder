using System.ComponentModel.DataAnnotations;

namespace WebScraper.Models
{
    public class FlatInfoModel
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public int Cost { get; set; }

        [Required]
        public DateTime SitePublication { get; set; }

        public string Description { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        public List<string> LinksOfImages { get; set; }

        public string PageLink { get; set; }

        public int ViewsOnSite { get; set; }

        public FlatCoordinate FlatCoordinate { get; set; }

        public FlatInfoModel(string title, int cost, DateTime sitePublication, string description, string phoneNumber, List<string> linksOfImage, string adLink, int viewsOnSite, FlatCoordinate flatCoordinate)
        {
            Title = title;
            Cost = cost;
            SitePublication = sitePublication;
            Description = description;
            PhoneNumber = phoneNumber;
            LinksOfImages = linksOfImage;
            PageLink = adLink;
            ViewsOnSite = viewsOnSite;
            FlatCoordinate = flatCoordinate;
        }

        public FlatInfoModel() { }
    }
}