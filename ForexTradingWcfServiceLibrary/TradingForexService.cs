using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
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
        private static DateTime _serverTime = new DateTime(2017, 1, 2, 11, 1, 0);
        Queue<string> logs;
        private Dictionary<ITradingForexClient, KeyValuePair<string, TradingPair>> _clients;
        private TradingPair _defaultTradingPair;
        /// <summary>
        /// Constructor for service
        /// </summary>
        public TradingForexService()
        {
            ForexTradingContext forexTradingContext = new ForexTradingContext();
            _clients = new Dictionary<ITradingForexClient, KeyValuePair<string, TradingPair>>();
            logs = new Queue<string>();

            _defaultTradingPair = (from x in forexTradingContext.TraidingPairs select x).FirstOrDefault();

            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 500;
            aTimer.Enabled = true;

        }
        /// <summary>
        /// Tries login user into database
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool LoginUser(string email, string password)
        {
            ForexTradingContext forexTradingContext = new ForexTradingContext();
            User user = forexTradingContext.Users.SingleOrDefault(x => x.Email == email);
            var conn = OperationContext.Current.GetCallbackChannel<ITradingForexClient>();

            _clients.TryGetValue(conn, out var connectedUser);

            if (connectedUser.Key == null)
            {
                if (user != null)
                {
                    if (BCrypt.Net.BCrypt.Verify(password, user.Password))
                    {
                        _clients.Add(conn, new KeyValuePair<string, TradingPair>(email, _defaultTradingPair));

                        string message = $"{_serverTime} {email} was connected";
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
            ForexTradingContext _forexTradingContext = new ForexTradingContext();

            var userFoo = (from x in _forexTradingContext.Users where x.Email == email select x).SingleOrDefault();
            if (userFoo == null)
                try
                {
                    var pass = BCrypt.Net.BCrypt.HashPassword(password);
                    User user = new User(email, name, surename, pass);

                    _forexTradingContext.Users.Add(user);
                    _forexTradingContext.SaveChanges();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }

            return false;
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

            var conn = OperationContext.Current.GetCallbackChannel<ITradingForexClient>();
            _clients.TryGetValue(conn, out var user);


            var tradingPairFoo = (from x in _forexTradingContext.TraidingPairs where x.FullName == tradingPair select x).FirstOrDefault();
            _clients[conn] = new KeyValuePair<string, TradingPair>(user.Key, tradingPairFoo);

            var data = (from x in _forexTradingContext.TraidingPairDatas
                        where x.Date < _serverTime
                        where x.TradingPairId == tradingPairFoo.Id
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
            ForexTradingContext _forexTradingContext = new ForexTradingContext();
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


            _clients.TryGetValue(OperationContext.Current.GetCallbackChannel<ITradingForexClient>(), out var user);


            forexTradingContext.PortFolioDatas.Add(new PortFolioData()
            {
                TradingPairId = value.TradingPairId,
                DateOfBuy = timeOfBuy,
                UserEmail = user.Key,
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
            _clients.TryGetValue(OperationContext.Current.GetCallbackChannel<ITradingForexClient>(), out var user);
            ForexTradingContext _forexTradingContext = new ForexTradingContext();

            try
            {
                var portfolio = (from x in
                        _forexTradingContext.PortFolioDatas
                                 join y in _forexTradingContext.TraidingPairs
                                 on x.TradingPairId equals y.Id
                                 where x.UserEmail == user.Key
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
                                        where x.UserEmail == user.Key
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
               
                var totalInvestment = portfolioSummary.Sum(x => x.investmets);
                var totalPerc = totalSum * 100 / totalInvestment;

                var summary = new string[4];
                summary[0] = portfolioSummary.Count().ToString();
                summary[1] = totalInvestment.ToString("N2");
                summary[2] = totalSum.ToString("N2");
                summary[3] = totalPerc.ToString("N2");


                KeyValuePair<string[], List<string[]>> keyValuePair = new KeyValuePair<string[], List<string[]>>(summary, portofolioList);

                return keyValuePair;
            }
            catch (Exception)
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
            _clients.TryGetValue(OperationContext.Current.GetCallbackChannel<ITradingForexClient>(), out var user);
            ForexTradingContext _forexTradingContext = new ForexTradingContext();

            try
            {

                var portfolio = (from x in
                        _forexTradingContext.PortFolioDatas
                                 join y in _forexTradingContext.TraidingPairs
                                 on x.TradingPairId equals y.Id
                                 where x.UserEmail == user.Key
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
                                        where x.UserEmail == user.Key
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
                var totalInvestment = portfolioSummary.Sum(x => x.investmets);
                var totalPerc = totalSum * 100 / totalInvestment;

                var summary = new string[4];
                summary[0] = portfolioSummary.Count().ToString();
                summary[1] = totalInvestment.ToString("N2");
                summary[2] = totalSum.ToString("N2");
                summary[3] = totalPerc.ToString("N2");


                KeyValuePair<string[], List<string[]>> keyValuePair = new KeyValuePair<string[], List<string[]>>(summary, portofolioList);

                return keyValuePair;
            }
            catch (Exception)
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
            ForexTradingContext forexTradingContext = new ForexTradingContext();
            var tradingPairF = (from x in forexTradingContext.TraidingPairs where x.FullName == tradingPair select x).FirstOrDefault();



            return (from x in forexTradingContext.TraidingPairDatas
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

                return new ForexData(_defaultTradingPair.FullName, listOfData);
            }
            else
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
            ForexTradingContext forexTradingContext = new ForexTradingContext();
            try
            {
                foreach (var user in _clients)
                {
                    try
                    {
                        var data = (from x in forexTradingContext.TraidingPairDatas
                                    where x.Date <= _serverTime
                                    where x.TradingPairId == user.Value.Value.Id
                                    orderby x.Date descending
                                    select x).First();


                        user.Key.ReceiveData(new ForexData(user.Value.Value.FullName, new List<KeyValuePair<long, double>>()
                        {
                            new KeyValuePair<long, double>(data.Date.Ticks,data.Value)
                        }));
                       
                    }
                    catch (ObjectDisposedException)
                    {
                        string message = $"{_serverTime} {user.Value.Key} was disconeted";
                        Console.WriteLine(message);
                        logs.Enqueue(message);
                        _clients.Remove(user.Key);
                    }

                }
            }
            catch (Exception ex)
            {
                
                logs.Enqueue(ex.Message);
                Console.WriteLine(ex.Message);
            }


            _serverTime = _serverTime.AddMinutes(1);
            WriteLogs();
        }
    }
}
