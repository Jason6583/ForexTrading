using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Timers;
using ForexTradingDatabase;

namespace ForexTradingWcfServiceLibrary
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
    public class TradingForexService : ITradingForexService
    {
        private ITradingForexClient _user;
        private string _selectedPair;
        private ForexTradingContext _forexTradingContext = new ForexTradingContext();
        private DateTime _serverTime = new DateTime(2017, 1, 2, 2, 1, 0);
        public TradingForexService()
        {

        }

        public bool LoginUser(string email, string password)
        {
            User pexesoPlayer = _forexTradingContext.Users.SingleOrDefault(x => x.Email == email);
            var conn = OperationContext.Current.GetCallbackChannel<ITradingForexClient>();

            if (pexesoPlayer != null)
            {
                if (BCrypt.Net.BCrypt.Verify(password, pexesoPlayer.Password))
                {
                    _user = conn;
                    System.Timers.Timer aTimer = new System.Timers.Timer();
                    aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                    aTimer.Interval = 1000;
                    aTimer.Enabled = true;

                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }

        public bool RegisterUser(string name, string surename, string email, string password)
        {
            throw new NotImplementedException();
        }


        public void SelectTradingPair(string name)
        {
            Console.WriteLine("");
        }

        public ForexData GetData(int count, DateTime time)
        {
            var data = (from x in _forexTradingContext.TraidingPairDatas
                        let value = x.TradingPair.FirstAsset.Name + x.TradingPair.SecondAsset.Name
                        where x.Date <= time
                        where x.TradingPair.FirstAsset.Name + "/" + x.TradingPair.SecondAsset.Name == "EUR/USD"
                        group x.Value by value
                into y
                        select y);

            var convertedData = ConvertData(data);
            return convertedData;
        }

        private ForexData ConvertData(IEnumerable<IGrouping<string, double>> data)
        {
            

            if (data.Count() != 0)
            {
                var pom = data.First();
                var pom1 = pom.Reverse().Take(80);

                List<KeyValuePair<DateTime, double>> listOfData = new List<KeyValuePair<DateTime, double>>();
                ForexData forexData = new ForexData(data.First().Key, listOfData);

                foreach (var pomData in pom1)
                {
                    listOfData.Add(new KeyValuePair<DateTime, double>(_serverTime, pomData));
                }

                return forexData;
            }

            return null;
        }


        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            try
            {
                var data = (from x in _forexTradingContext.TraidingPairDatas
                            let value = x.TradingPair.FirstAsset.Name + x.TradingPair.SecondAsset.Name
                            where x.Date == _serverTime
                            where x.TradingPair.FirstAsset.Name + "/" + x.TradingPair.SecondAsset.Name == "EUR/USD"
                            group x.Value by value
                    into y
                            select y);


                var convertedData = ConvertData(data);

                if (convertedData != null)
                    _user.ReceiveData(convertedData);

                _serverTime = _serverTime.AddMinutes(1);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
