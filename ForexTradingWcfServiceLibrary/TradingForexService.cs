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
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple,
        InstanceContextMode = InstanceContextMode.Single,
        UseSynchronizationContext = false)]
    public class TradingForexService : ITradingForexService
    {
        private ITradingForexClient _user;
        private string _selectedPair;
        private ForexTradingContext _forexTradingContext = new ForexTradingContext();
        private DateTime _serverTime = new DateTime(2017, 1, 2, 8, 1, 0);
        private string _actualTradingPair = "EUR/USD";
        User _actualUser;
        Queue<string> logs;
        public TradingForexService()
        {
            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 800;
            aTimer.Enabled = true;
            logs = new Queue<string>();
        }

        public bool LoginUser(string email, string password)
        {
            User user = _forexTradingContext.Users.SingleOrDefault(x => x.Email == email);
            var conn = OperationContext.Current.GetCallbackChannel<ITradingForexClient>();
           
            if (user != null)
            {
                if (BCrypt.Net.BCrypt.Verify(password, user.Password))
                {
                    _actualUser = user;
                    _user = conn;

                    string message = $"{_serverTime} {_actualUser.Email} was connected";
                    Console.WriteLine(message);
                    logs.Enqueue(message);
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

        public ForexData GetData(int count, string tradingPair)
        {

            ForexTradingContext _forexTradingContext = new ForexTradingContext();
            _actualTradingPair = tradingPair;

            var data = (from x in _forexTradingContext.TraidingPairDatas
                        where x.Date < _serverTime
                        where x.TradingPair.FirstAsset.Name + "/" + x.TradingPair.SecondAsset.Name == _actualTradingPair
                        select x);

            var pom = data.ToList();
            var convertedData = ConvertData(pom, count);
            return convertedData;

        }

        public List<string> GetAllTradingPairs()
        {
            return (from x in _forexTradingContext.TraidingPairs select x.FirstAsset.Name + "/" + x.SecondAsset.Name)
                .ToList();
        }

        public void AddAsset(string tradingPair, long dateOfBuy)
        {

            DateTime timeOfBuy = new DateTime(dateOfBuy, DateTimeKind.Utc);

            TradingPair tradingPairId = (from x in _forexTradingContext.TraidingPairs
                                         where x.FirstAsset.Name + "/" + x.SecondAsset.Name == tradingPair
                                         select x).SingleOrDefault();

            double value = (from x in _forexTradingContext.TraidingPairDatas
                            join y in _forexTradingContext.TraidingPairs.Include("FirstAsset").Include("SecondAsset")
                            on x.TradingPair.Id equals y.Id
                            where x.Date <= _serverTime
                            where y.FirstAsset.Name == tradingPairId.FirstAsset.Name
                            where y.SecondAsset.Name == tradingPairId.SecondAsset.Name
                            orderby x.Date descending
                            select x.Value).First();

            _forexTradingContext.PortFolioDatas.Add(new PortFolioData()
            { TradingPair = tradingPairId, DateOfBuy = timeOfBuy, User = _actualUser, Price = value });

            _forexTradingContext.SaveChanges();

        }

        public long GetServerTime()
        {
            return _serverTime.Ticks;
        }

        private class TradingDataFoo
        {
            public TradingPair TradingPair { get; set; }
            public PortFolioData Data { get; set; }

            public double Perc { get; set; }
        }

        public List<string[]> GetPortFolio()
        {

            //Foreing keys are not add without this one
            var pom = (from x in _forexTradingContext.TraidingPairs.Include("FirstAsset").Include("SecondAsset")
                       select x).ToList();

            var portfolio = (from x in
                    _forexTradingContext.PortFolioDatas
                             join y in _forexTradingContext.TraidingPairs.Include("FirstAsset").Include("SecondAsset")
                             on x.TradingPair.Id equals y.Id
                             where x.User.Email == _actualUser.Email
                             where x.DateOfSold == null
                             select new TradingDataFoo
                             {
                                 Data = x,
                                 TradingPair = y,
                                 Perc = ((from v in _forexTradingContext.TraidingPairDatas
                                          where v.Date <= _serverTime
                                          where v.TradingPair.FirstAsset.Name == y.FirstAsset.Name
                                          where v.TradingPair.SecondAsset.Name == y.SecondAsset.Name
                                          orderby v.Date descending
                                          select v.Value).FirstOrDefault() * 100 / x.Price) - 100
                             }).ToList();


            List<string[]> portofolioList = new List<string[]>();
            foreach (var item in portfolio)
            {

                portofolioList.Add(new string[] {
                    item.Data.DateOfBuy.Value.ToString("MM/dd/yyyy HH:mm"),
                    item.TradingPair.FirstAsset.Name + "/" +
                    item.TradingPair.SecondAsset.Name,
                    item.Data.Price.ToString(),
                    item.Perc.ToString("N2")
                    }
                );
            }

            return portofolioList;
        }

        public double GetActualValue(string firstAssetName, string secondAssetName)
        {

            return (from x in _forexTradingContext.TraidingPairDatas
                    where x.Date <= _serverTime
                    where x.TradingPair.FirstAsset.Name == firstAssetName
                    where x.TradingPair.SecondAsset.Name == secondAssetName
                    orderby x.Date descending
                    select x.Value).First();
        }

        private ForexData ConvertData(List<TraidingPairData> data, int count)
        {
            if (data.Count != 0)
            {
                List<KeyValuePair<long, double>> listOfData = new List<KeyValuePair<long, double>>();

                for (int i = data.Count - 1; i >= data.Count - count; i--)
                {
                    listOfData.Add(new KeyValuePair<long, double>(data[i].Date.Ticks, data[i].Value));
                }

                listOfData = listOfData.OrderBy(x => x.Key).ToList();

                return new ForexData(_actualTradingPair, listOfData);
            }

            return null;
        }

        private void WriteLogs()
        {
            Console.Clear();
            Console.WriteLine($"SERVER TIME: {_serverTime}");
            Console.WriteLine("--------------LOGS--------------");
            Console.WriteLine();
            foreach (var log in logs)
            {
                Console.WriteLine(log);
            }
        }
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            try
            {
                if (_user != null)
                {

                    var data = (from x in _forexTradingContext.TraidingPairDatas
                                where x.Date == _serverTime
                                where x.TradingPair.FirstAsset.Name + "/" + x.TradingPair.SecondAsset.Name == _actualTradingPair
                                select x).ToList();


                    var convertedData = ConvertData(data, 1);

                    if (convertedData != null)
                        _user.ReceiveData(convertedData);
                }

                _serverTime = _serverTime.AddMinutes(1);
                WriteLogs();
            }
            catch (Exception ex)
            {
                string message = $"{_serverTime} {_actualUser.Email} was dissconeted";
                Console.WriteLine(message);
                logs.Enqueue(message);
                _user = null;
                _actualUser = null;
            }
        }
    }
}
