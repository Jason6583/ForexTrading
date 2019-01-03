using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ForexTradingDatabase;
using System.IO;
using System.Data.Entity;

namespace Generator
{
    /// <summary>
    /// Generator for filling forex data into database
    /// </summary>
    class Program
    {
        static private ForexTradingContext forexTradingContex;
        static void Main(string[] args)
        {
            forexTradingContex = new ForexTradingContext();

           AddData("C:/Users/Roman Pecho/Desktop/DAT_MT_EURGBP_M1_2017.csv", "EUR", "GBP");


        }
        /// <summary>
        /// Adds new asset to dabase
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Asset AddAsset(string name)
        {
            Asset asset = new Asset() { Name = name };
            forexTradingContex.Assets.Add(asset);
            forexTradingContex.SaveChanges();

            return asset;
        }

        /// <summary>
        /// Adds new trading pair to dabase
        /// </summary>
        /// <param name="idFIrst"></param>
        /// <param name="idSecond"></param>
        public static void AddTradingPair(string idFIrst, string idSecond)
        {
            forexTradingContex.TraidingPairs.Add(new TradingPair()
            {
                FirstAssetName = idFIrst,
                SecondAssetName = idSecond
            });
            forexTradingContex.SaveChanges();
        }

        /// <summary>
        /// Adds trading pair data to dabase
        /// </summary>
        /// <param name="nameOfFIle"></param>
        /// <param name="idFirst"></param>
        /// <param name="idSecond"></param>
        public static void AddData(string nameOfFIle, string idFirst, string idSecond)
        {

            //var first = AddAsset(idFirst);
            var second = AddAsset(idSecond); ;

            var asset1 = (from x in forexTradingContex.Assets where x.Name == "EUR" select x).SingleOrDefault();
            //var asset2 = (from x in forexTradingContex.Assets where x.Name == "USD" select x).SingleOrDefault();

            AddTradingPair(idFirst, idSecond);

            int i = 0;
            TradingPair tradingPair =
                (from x in forexTradingContex.TraidingPairs
                 where x.FirstAssetName == idFirst
                 where x.SecondAssetName == idSecond
                 select x).ToList()[0];

            StreamReader streamReader = new StreamReader(nameOfFIle);
            while (i != 5000)
            {

                string[] line = streamReader.ReadLine().Split(',');
                DateTime date = Convert.ToDateTime(line[0]);
                DateTime time = Convert.ToDateTime(line[1]);
                date = date.AddHours(time.Hour);
                date = date.AddMinutes(time.Minute);


                TradingPairData traidingPairData = new TradingPairData()
                {
                    Date = date,
                    TradingPairId = tradingPair.Id,
                    Value = Convert.ToDouble(line[2].Replace('.', ','))
                };
                forexTradingContex.TraidingPairDatas.Add(traidingPairData);

                Console.WriteLine(i);
                i++;
            }

            forexTradingContex.SaveChanges();
        }
    }
}
