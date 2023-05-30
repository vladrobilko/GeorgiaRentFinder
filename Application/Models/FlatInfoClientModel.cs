namespace Application.Models
{
    public class FlatInfoClientModel
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public long Cost { get; set; }

        public DateTime SitePublication { get; set; }

        public string Description { get; set; }

        public FlatPhoneClientModel FlatPhoneClientModel { get; set; }  

        public List<string> LinksOfImages { get; set; }

        public string PageLink { get; set; }

        public long ViewsOnSite { get; set; }

        public FlatCoordinateClientModel FlatCoordinateClientModel { get; set; }

        public ComfortStuffClientModel ComfortStuffClientModel { get; set; }
    }
}
