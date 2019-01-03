using System;
using System.Collections.Generic;
using System.Data.Entity.Core.EntityClient;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Timers;
using ForexTradingDatabase;

namespace ForexTradingWcfServiceLibrary
{
    /// <summary>
    /// Trading service for server
    /// </summary>
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple,
        InstanceContextMode = InstanceContextMode.Single,
        UseSynchronizationContext = false)]
    public class TradingForexService : ITradingForexService
    {
        private ITradingForexClient _user;
        private string _selectedPair;
        private static ForexTradingContext _forexTradingContext;
        private static DateTime _serverTime = new DateTime(2017, 1, 2, 11, 1, 0);
        private TradingPair _actualTradingPair;
        User _actualUser;
        Queue<string> logs;
        /// <summary>
        /// Constructor for service
        /// </summary>
        public TradingForexService()
        {
            _forexTradingContext = new ForexTradingContext();
            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 500;
            aTimer.Enabled = true;
            logs = new Queue<string>();

        }
        /// <summary>
        /// Tries login user into database
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool LoginUser(string email, string password)
        {
            _actualTradingPair = (from x in _forexTradingContext.TraidingPairs select x).FirstOrDefault();
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
        /// <summary>
        /// Tries register into database
        /// </summary>
        /// <param name="name"></param>
        /// <param name="surename"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool RegisterUser(string name, string surename, string email, string password)
        {
            var pass = BCrypt.Net.BCrypt.HashPassword(password);
            User user = new User(email, name, surename, pass);

            _forexTradingContext.Users.Add(user);
            _forexTradingContext.SaveChanges();
            return true;
        }
        /// <summary>
        /// Returns most recent forext data according to servertime
        /// </summary>
        /// <param name="count"></param>
        /// <param name="tradingPair"></param>
        /// <returns></returns>
        public ForexData GetData(int count, string tradingPair)
        {
            ForexTradingContext _forexTradingContext = new ForexTradingContext();

            _actualTradingPair = (from x in _forexTradingContext.TraidingPairs where x.FullName == tradingPair select x).FirstOrDefault();

            var data = (from x in _forexTradingContext.TraidingPairDatas
                        where x.Date < _serverTime
                        where x.TradingPairId == _actualTradingPair.Id
                        select x).ToList();

            var dataF = data.OrderByDescending(x => x.Date).ToList().Take(count);
            var dataP = dataF.OrderBy(x => x.Date).ToList();

            var convertedData = ConvertData(dataP, count);
            return convertedData;

        }
        /// <summary>
        /// Returns all avaible trading pairs
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllTradingPairs()
        {
            return (from x in _forexTradingContext.TraidingPairs select x.FullName)
                .ToList();
        }
        /// <summary>
        /// Add new asset to users portfolio
        /// </summary>
        /// <param name="tradingPair"></param>
        /// <param name="dateOfBuy"></param>
        /// <param name="investment"></param>
        public void AddAsset(string tradingPair, long dateOfBuy, double investment)
        {
            DateTime timeOfBuy = new DateTime(dateOfBuy, DateTimeKind.Utc);
            ForexTradingContext forexTradingContext = new ForexTradingContext();

            var tradingPairs = (from x in forexTradingContext.TraidingPairs where x.FullName == tradingPair select x).FirstOrDefault();

            var value = (from x in forexTradingContext.TraidingPairDatas                 
                         where x.TradingPairId == tradingPairs.Id
                         where x.Date <= _serverTime
                         orderby x.Date descending
                         select x).First();

            forexTradingContext.PortFolioDatas.Add(new PortFolioData()
            {
                TradingPairId = value.TradingPairId,
                DateOfBuy = timeOfBuy,
                UserEmail = _actualUser.Email,
                Price = value.Value,
                Investment = investment
            });

            forexTradingContext.SaveChanges();

        }

        /// <summary>
        /// Set users asset as sold
        /// </summary>
        /// <param name="id"></param>
        public void SellAsset(int id)
        {
            ForexTradingContext forexTradingContext = new ForexTradingContext();
            PortFolioData asset = (from x in forexTradingContext.PortFolioDatas
                                   where x.Id == id
                                   select x).SingleOrDefault();

            asset.DateOfSold = _serverTime;

            forexTradingContext.SaveChanges();

        }
        /// <summary>
        /// Returns actual server time
        /// </summary>
        /// <returns></returns>
        public long GetServerTime()
        {
            return _serverTime.Ticks;
        }

        /// <summary>
        /// Helper class for query from database
        /// </summary>
        private class TradingDataFoo
        {
            public int Id { get; set; }
            public TradingPair TradingPair { get; set; }
            public PortFolioData Data { get; set; }

            public double Perc { get; set; }

            public double Investment { get; set; }
        }
        /// <summary>
        /// Returns actual porfolio of user 
        /// </summary>
        /// <returns></returns>
        public KeyValuePair<string[], List<string[]>> GetPortFolio()
        {
            try
            {
                var portfolio = (from x in
                        _forexTradingContext.PortFolioDatas
                                 join y in _forexTradingContext.TraidingPairs
                                 on x.TradingPairId equals y.Id
                                 where x.UserEmail == _actualUser.Email
                                 where x.DateOfSold == null
                                 select new TradingDataFoo
                                 {
                                     Data = x,
                                     TradingPair = y,
                                     Perc = ((from v in _forexTradingContext.TraidingPairDatas
                                              where v.Date <= _serverTime
                                              where v.TradingPairId == y.Id
                                              orderby v.Date descending
                                              select v.Value).FirstOrDefault() * 100 / x.Price) - 100,
                                     Investment = x.Investment,
                                     Id = x.Id
                                 }).ToList();


                List<string[]> portofolioList = new List<string[]>();
                foreach (var item in portfolio)
                {

                    portofolioList.Add(new string[]
                        {
                            item.Data.DateOfBuy.Value.ToString("MM/dd/yyyy "),
                            item.Data.DateOfBuy.Value.ToString("HH:mm"),
                            item.TradingPair.FullName,
                            item.Data.Price.ToString(),
                            item.Investment.ToString("N2"),
                            (item.Perc * item.Investment  / 100).ToString("N2"),
                            item.Perc.ToString("N2"),
                            item.Id.ToString()
                        }
                    );
                }

                var portfolioSummary = (from x in
                    _forexTradingContext.PortFolioDatas
                                        join y in _forexTradingContext.TraidingPairs
                                        on x.TradingPairId equals y.Id
                                        where x.UserEmail == _actualUser.Email
                                        where x.DateOfSold == null
                                        let value = ((((from v in _forexTradingContext.TraidingPairDatas
                                                        where v.Date <= _serverTime
                                                        where v.TradingPairId == y.Id
                                                        orderby v.Date descending
                                                        select v.Value).FirstOrDefault() * 100 / x.Price) - 100) / 100 * x.Investment)

                                        let perc = ((((from v in _forexTradingContext.TraidingPairDatas
                                                       where v.Date <= _serverTime
                                                       where v.TradingPairId == y.Id
                                                       orderby v.Date descending
                                                       select v.Value).FirstOrDefault() * 100 / x.Price) - 100))
                                        select new
                                        {
                                            values = value,
                                            perces = perc,
                                            investmets = x.Investment
                                        }

                );


                var totalSum = portfolioSummary.Sum(x => x.values);
                var totalPerc = portfolioSummary.Sum(x => x.perces) / portfolioSummary.Count();
                var totalInvestment = portfolioSummary.Sum(x => x.investmets);

                var summary = new string[4];
                summary[0] = portfolioSummary.Count().ToString();
                summary[1] = totalInvestment.ToString("N2");
                summary[2] = totalSum.ToString("N2");
                summary[3] = totalPerc.ToString("N2");


                KeyValuePair<string[], List<string[]>> keyValuePair = new KeyValuePair<string[], List<string[]>>(summary, portofolioList);

                return keyValuePair;
            }
            catch (Exception ex)
            {
                return new KeyValuePair<string[], List<string[]>>();
            }
        }
        /// <summary>
        /// Returns portfolio history of user
        /// </summary>
        /// <returns></returns>
        public KeyValuePair<string[], List<string[]>> GetPortFolioHistory()
        {
            try
            {

                var portfolio = (from x in
                        _forexTradingContext.PortFolioDatas
                                 join y in _forexTradingContext.TraidingPairs
                                 on x.TradingPairId equals y.Id
                                 where x.UserEmail == _actualUser.Email
                                 where x.DateOfSold != null
                                 select new TradingDataFoo
                                 {
                                     Data = x,
                                     TradingPair = y,
                                     Perc = ((from v in _forexTradingContext.TraidingPairDatas
                                              where v.Date <= x.DateOfSold
                                              where v.TradingPairId == y.Id
                                              orderby v.Date descending
                                              select v.Value).FirstOrDefault() * 100 / x.Price) - 100,
                                     Investment = x.Investment,
                                     Id = x.Id
                                 }).ToList();


                List<string[]> portofolioList = new List<string[]>();
                foreach (var item in portfolio)
                {

                    portofolioList.Add(new string[]
                        {
                            item.Data.DateOfBuy.Value.ToString("MM/dd/yyyy "),
                            item.Data.DateOfBuy.Value.ToString("HH:mm"),
                            item.TradingPair.FullName,
                            item.Data.Price.ToString(),
                            item.Investment.ToString("N2"),
                            (item.Perc * item.Investment  / 100).ToString("N2"),
                            item.Perc.ToString("N2"),
                            item.Id.ToString()
                        }
                    );
                }

                portofolioList = (portofolioList.OrderBy(x => x[0] + x[1])).ToList();

                var portfolioSummary = (from x in
                    _forexTradingContext.PortFolioDatas
                                        join y in _forexTradingContext.TraidingPairs
                                       on x.TradingPairId equals y.Id
                                        where x.UserEmail == _actualUser.Email
                                        where x.DateOfSold != null
                                        let value = ((((from v in _forexTradingContext.TraidingPairDatas
                                                        where v.Date <= x.DateOfSold
                                                        where v.TradingPairId == y.Id
                                                        orderby v.Date descending
                                                        select v.Value).FirstOrDefault() * 100 / x.Price) - 100) / 100 * x.Investment)

                                        let perc = ((((from v in _forexTradingContext.TraidingPairDatas
                                                       where v.Date <= x.DateOfSold
                                                       where v.TradingPairId == y.Id
                                                       orderby v.Date descending
                                                       select v.Value).FirstOrDefault() * 100 / x.Price) - 100))
                                        select new
                                        {
                                            values = value,
                                            perces = perc,
                                            investmets = x.Investment
                                        }

                );


                var totalSum = portfolioSummary.Sum(x => x.values);
                var totalPerc = portfolioSummary.Sum(x => x.perces) / portfolioSummary.Count();
                var totalInvestment = portfolioSummary.Sum(x => x.investmets);

                var summary = new string[4];
                summary[0] = portfolioSummary.Count().ToString();
                summary[1] = totalInvestment.ToString("N2");
                summary[2] = totalSum.ToString("N2");
                summary[3] = totalPerc.ToString("N2");


                KeyValuePair<string[], List<string[]>> keyValuePair = new KeyValuePair<string[], List<string[]>>(summary, portofolioList);

                return keyValuePair;
            }
            catch (Exception ex)
            {
                return new KeyValuePair<string[], List<string[]>>();
            }
        }

        /// <summary>
        /// Returns actual value of trading pair
        /// </summary>
        /// <param name="tradingPair"></param>
        /// <returns></returns>
        public double GetActualValue(string tradingPair)
        {
            var tradingPairF = (from x in _forexTradingContext.TraidingPairs where x.FullName == tradingPair select x).FirstOrDefault();

            return (from x in _forexTradingContext.TraidingPairDatas
                    where x.Date <= _serverTime
                    where x.TradingPairId == tradingPairF.Id
                    orderby x.Date descending
                    select x.Value).First();
        }
        /// <summary>
        /// Convert database Forexdata to service ForexData
        /// </summary>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private ForexData ConvertData(List<ForexTradingDatabase.TradingPairData> data, int count)
        {
            if (data.Count != 0)
            {
                List<KeyValuePair<long, double>> listOfData = new List<KeyValuePair<long, double>>();
                foreach (var item in data)
                {
                    listOfData.Add(new KeyValuePair<long, double>(item.Date.Ticks, item.Value));
                }

                return new ForexData(_actualTradingPair.FullName, listOfData);
            }

            return null;
        }
        /// <summary>
        /// Writes logs to server
        /// </summary>
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
        /// <summary>
        /// Handling event when server sends trading pair data to user
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            try
            {
                if (_user != null)
                {
                    var data = (from x in _forexTradingContext.TraidingPairDatas
                                where x.Date <= _serverTime
                                where x.TradingPairId == _actualTradingPair.Id
                                orderby x.Date descending
                                select x).First();

                    var pom = data;

                    _user.ReceiveData(new ForexData(_actualTradingPair.FullName, new List<KeyValuePair<long, double>>()
                    {
                        new KeyValuePair<long, double>(data.Date.Ticks,data.Value)
                    }));
                }

                _serverTime = _serverTime.AddMinutes(1);
                WriteLogs();
            }
            catch(ObjectDisposedException ex)
            {
                string message = $"{_serverTime} {_actualUser.Email} was disconeted";
                Console.WriteLine(message);
                logs.Enqueue(message);
                _user = null;
                _actualUser = null;
            }
            catch (Exception ex)
            {
                logs.Enqueue(ex.Message);
            }
        }
    }
}
