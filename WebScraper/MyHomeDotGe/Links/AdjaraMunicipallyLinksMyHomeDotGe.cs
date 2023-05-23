namespace WebScraper.MyHomeDotGe.Links
{
    public class AdjaraMunicipallyLinksMyHomeDotGe
    {
        public static string GetBatumiLink(int pageNumber)
        {
            return $"https://www.myhome.ge/en/s/Apartment-for-rent-House-for-rent-Commercial-for-rent-Hotel-for-rent-Batumi?Keyword=Batumi&AdTypeID=3&PrTypeID=1.2.4.7&Page={pageNumber}&mapC=41.8113601%2C41.7793202&districts=776481390.776472116.776471185.777654897.776734274.776998491.776460995.776458944.776463102.776465448&cities=8742159&GID=8742159";
        }

        public static string GetKhelvachauriLink(int pageNumber = 0)
        {
            return $"https://www.myhome.ge/en/s/Apartment-for-rent-House-for-rent-Commercial-for-rent-Hotel-for-rent-Khelvachauri?Keyword=Khelvachauri&AdTypeID=3&PrTypeID=1.2.4.7&mapC=41.8113601%2C41.7793202&districts=79605316&cities=79605316&GID=79605316";
        }

        public static string GetKobuletiLink(int pageNumber)
        {
            return $"https://www.myhome.ge/en/s/Apartment-for-rent-House-for-rent-Commercial-for-rent-Hotel-for-rent-Kobuleti?Keyword=Kobuleti&AdTypeID=3&PrTypeID=1.2.4.7&Page={pageNumber}&mapC=41.8113601%2C41.7793202&districts=8738685&cities=8738685&GID=8738685";
        }

        public static string GetKhuloLink(int pageNumber = 0)
        {
            return $"";
        }

        public static string GetKedaLink(int pageNumber = 0)
        {
            return $"";
        }

        public static string GetShuakheviLink(int pageNumber = 0)
        {
            return $"";
        }
    }
}