using Application.Models;
using Newtonsoft.Json;
using WebScraper.Models;

namespace DataManagement.Converters
{
    public static class StringConverter
    {
        public static ComfortStuffClientModel ToComfortStuffClientModel(this string comfortStuffJson)
        {
            var comfortStuffDto = JsonConvert.DeserializeObject<ComfortStuffClientModel>(comfortStuffJson);

            if (comfortStuffDto == null)
            {
                var defaultComfortStuff = new ComfortStuffModel();

                return new ComfortStuffClientModel()
                {
                    BedRooms = defaultComfortStuff.BedRooms,
                    Floor = defaultComfortStuff.Floor,
                    TotalArea = defaultComfortStuff.TotalArea,
                    IsThereGas = defaultComfortStuff.IsThereGas,
                    IsThereHotWater = defaultComfortStuff.IsThereHotWater,
                    IsThereConditioner = defaultComfortStuff.IsThereConditioner
                };
            }

            return comfortStuffDto;
        }
    }
}
