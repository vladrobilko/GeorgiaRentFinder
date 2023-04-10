using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using HtmlAgilityPack;

namespace WebScraperApp
{
    public class ScraperSsDotGe
    {
        public List<Info> GetInfos(string url)
        {
            List<Info> infos = new List<Info>();

            HtmlWeb web = new HtmlWeb();

            var doc = web.Load(url);

            for (int i = 0, j = 0; i < 20; j++)
            {
                var inputTitle = doc.DocumentNode.SelectSingleNode($"//*[@id=\"list\"]/div[{j}]/div[1]/div[1]/div[1]/div[2]/div[1]/a/div/span")?.InnerText;
                if (inputTitle != null)
                {
                    var inputCost = doc.DocumentNode.SelectSingleNode($"//*[@id=\"list\"]/div[{j}]/div[1]/div[1]/div[2]/div[2]/div[1]/text()")?.InnerText ?? "Zero";
                    var cost = int.TryParse(inputCost.Replace(" ", ""), out int number) == true ? number : 0;

                    var inputDate = doc.DocumentNode.SelectSingleNode($"//*[@id=\"list\"]/div[{j}]/div[1]/div[1]/div[1]/div[2]/div[2]/div/div[2]/div/div[1]/text()")?.InnerText.Replace(" ", "").Replace("\r\n","") ?? "Zero";

                    var date = DateTime.ParseExact(inputDate, "dd.MM.yyyy/HH:mm", CultureInfo.InvariantCulture);

                    infos.Add(new Info() { Title = inputTitle, Cost = cost, Data = date });
                    i++;
                    continue;
                }
            }
 

            return infos;
        }

        public List<string> GetImages(HtmlDocument doc)
        {
            var images = new List<string>();

            var input = doc.DocumentNode.SelectSingleNode("")?.InnerText;


            return images;
        }
        //////*[@id="list"]/div[5]/div[1]/div[1]/div[1]/div[1]/div[2]/div[1]/div/div[1]/a - href
        //1 //*[@id="list"]/div[5]/div[1]/div[1]/div[2]/div[2]/div[1]/text()
        //////*[@id="list"]/div[5]/div[1]/div[1]/div[1]/div[2]/div[1]/a/div/span - title
        //////*[@id="list"]/div[5]/div[1]/div[1]/div[1]/div[2]/div[2]/div/div[2]/div/div[1]/text() - date
        //
        //////*[@id="list"]/div[6]/div[1]/div[1]/div[1]/div[1]/div/div[1]/div/div[1]/a/img
        //2 //*[@id="list"]/div[6]/div[1]/div[1]/div[2]/div[2]/div[1]/text()
        //////*[@id="list"]/div[6]/div[1]/div[1]/div[1]/div[2]/div[1]/a/div/span
        //////*[@id="list"]/div[6]/div[1]/div[1]/div[1]/div[2]/div[2]/div/div[2]/div/div[1]/text() - date
        //3 //*[@id="list"]/div[7]/div[1]/div[1]/div[2]/div[2]/div[1]/text()
        //4 //*[@id="list"]/div[10]/div[1]/div[1]/div[2]/div[2]/div[1]/text()
        //////*[@id="list"]/div[10]/div[1]/div[1]/div[1]/div[1]/div/a/img
        /// 
        //5 //*[@id="list"]/div[11]/div[1]/div[1]/div[2]/div[1]/div - по соглашению
        /// //*[@id="list"]/div[12]/div[1]/div[1]/div[1]/div[2]/div[1]/a/div/span
        //6 //*[@id="list"]/div[12]/div[1]/div[1]/div[2]/div[2]/div - по соглашению
        //7 //*[@id="list"]/div[15]/div[1]/div[1]/div[2]/div[2]/div[1]/text()
        //8 //*[@id="list"]/div[16]/div[1]/div[1]/div[2]/div[2]/div[1]/text()
        //9 //*[@id="list"]/div[17]/div[1]/div[1]/div[2]/div[2]/div[1]/text()
        //10//*[@id="list"]/div[20]/div[1]/div[1]/div[2]/div[2]/div[1]/text()
        //11//*[@id="list"]/div[21]/div[1]/div[1]/div[2]/div[2]/div[1]/text()
        //12//*[@id="list"]/div[22]/div[1]/div[1]/div[2]/div[2]/div[1]/text()
        //13//*[@id="list"]/div[25]/div[1]/div[1]/div[2]/div[2]/div[1]/text()
        //14//*[@id="list"]/div[26]/div[1]/div[1]/div[2]/div[2]/div[1]/text()
        //15//*[@id="list"]/div[27]/div[1]/div[1]/div[2]/div[2]/div[1]/text()
        //16//*[@id="list"]/div[28]/div[1]/div[1]/div[2]/div[2]/div[1]/text()
        //17//*[@id="list"]/div[29]/div[1]/div[1]/div[2]/div[2]/div[1]/text()
        //18//*[@id="list"]/div[30]/div[1]/div[1]/div[2]/div[2]/div - по соглашению
        //19//*[@id="list"]/div[31]/div[1]/div[1]/div[2]/div[2]/div[1]/text()
        //20//*[@id="list"]/div[32]/div[1]/div[1]/div[2]/div[2]/div[1]/text()
    }
}
