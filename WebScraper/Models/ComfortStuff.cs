namespace WebScraper.Models
{
    public class ComfortStuff
    {
        public string BedRooms { get; set; } = "No bedrooms";

        public string Floor { get; set; } = "No floors";

        public string TotalArea { get; set; } = "No total area";

        public bool? IsThereGas { get; set; } = null;

        public bool? IsThereHotWater { get; set; } = null;

        public bool? IsThereConditioner { get; set; } = null;

        public ComfortStuff(string bedRooms,string floor, string totalArea, bool? isThereGas, bool? isThereHotWater, bool? isThereConditioner)
        {
            BedRooms = bedRooms;
            Floor = floor;
            TotalArea = totalArea;
            IsThereGas = isThereGas;
            IsThereHotWater = isThereHotWater;
            IsThereConditioner = isThereConditioner;
        }

        public ComfortStuff() { }
    }
}