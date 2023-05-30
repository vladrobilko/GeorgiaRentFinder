namespace Application.Models
{
    public class ComfortStuffClientModel
    {
        public string BedRooms { get; set; }

        public string Floor { get; set; }

        public string TotalArea { get; set; }

        public bool? IsThereGas { get; set; }

        public bool? IsThereHotWater { get; set; }

        public bool? IsThereConditioner { get; set; } 
    }
}
