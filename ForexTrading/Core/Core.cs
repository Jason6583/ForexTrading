using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ForexTradingWcfServiceLibrary;

namespace ForexTrading.Core
{
    /// <summary>
    /// Class for handling callback from wcf service
    /// </summary>
    public class Client : ITradingForexClient
    {
        public class ReceiveDataArgs : EventArgs
        {
            public ReceiveDataArgs(string name, List<KeyValuePair<DateTime, double>> data)
            {
                Data = data;
                Name = name;
            }

            public List<KeyValuePair<DateTime, double>> Data { get; set; }
            public string Name { get; set; }
        }

        public delegate void UpdateFileEventHandler(object source, ReceiveDataArgs args);
        public event UpdateFileEventHandler ReceiveDataEvent;
        public void ReceiveData(ForexData data)
        {
            List<KeyValuePair<DateTime, double>> list = new List<KeyValuePair<DateTime, double>>();
            foreach (var item in data.Data)
            {
                list.Add(new KeyValuePair<DateTime, double>(new DateTime(item.Key, DateTimeKind.Utc), item.Value));
            }

            OnReceiveDataEvent(this, new ReceiveDataArgs(data.Name, list));
        }

        protected virtual void OnReceiveDataEvent(object source, ReceiveDataArgs args)
        {
            ReceiveDataEvent?.Invoke(source, args);
        }
    }
    /// <summary>
    /// Core class for operating with service
    /// </summary>
    public class Core
    {
        public string UserEmail { get; set; }
        private static Client _client;
        public Client Client
        {
            get { return _client; }

        }

        private static InstanceContext instanceContext;
        private ITradingForexService _tradingServiceClient;
        public Core()
        {
            _client = new Client();
            instanceContext = new InstanceContext(_client);

            CreateConnection();
            ThreadPool.QueueUserWorkItem(new WaitCallback(
                (obj) =>
                {
                    _tradingServiceClient = proxy.CreateChannel();
                }));

        }
        private static DuplexChannelFactory<ITradingForexService> proxy;
        public void CreateConnection()
        {
            proxy = new DuplexChannelFactory<ITradingForexService>(instanceContext, "TradingService");
        }

        public void CloseConnection()
        {
            proxy.Close();
            instanceContext = null;
        }

        public List<KeyValuePair<DateTime, double>> GetData(int count, string tradingPair)
        {

            ForexData forexData = _tradingServiceClient.GetData(count, tradingPair);
            List<KeyValuePair<DateTime, double>> list = new List<KeyValuePair<DateTime, double>>();
            foreach (var item in forexData.Data)
            {
                list.Add(new KeyValuePair<DateTime, double>(new DateTime(item.Key, DateTimeKind.Utc), item.Value));
            }
            return list;

        }

        /// <summary>
        /// Login user to database
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        public void LoginUser(string email, string password)
        {
            if (_tradingServiceClient.LoginUser("pecho4@gmail.com", "1111"))
            {
                UserEmail = email;
            }
        }


        /// <summary>
        /// Register user to database
        /// </summary>
        /// <param name="name"></param>
        /// <param name="surename"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        public void RegisterUser(string name, string surename, string email, string password)
        {
            try
            {
                _tradingServiceClient.RegisterUser(name, surename, email, password);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<string> GetAllTradingPairs()
        {
            List<string> foo = _tradingServiceClient.GetAllTradingPairs();
            return foo;
        }

        public void AddAsset(string tradingPair, DateTime timeOfBuy, double investment)
        {
            _tradingServiceClient.AddAsset(tradingPair, timeOfBuy.Ticks, investment);
        }

        public DateTime GetServerTime()
        {
            DateTime dateTime = new DateTime(_tradingServiceClient.GetServerTime(), DateTimeKind.Utc);
            return dateTime;

        }

        public KeyValuePair<string[], List<string[]>> GetPortFolio()
        {
            //Entity returns sometimes error with query, only sometimes with same data, thats weird
            KeyValuePair<string[], List<string[]>> foo = new KeyValuePair<string[], List<string[]>>();

            foo = _tradingServiceClient.GetPortFolio();


            return foo;
        }

        public double GetActualValue(string firstAssetName, string secondAssetName)
        {
            var foo = _tradingServiceClient.GetActualValue(firstAssetName, secondAssetName);
            return foo;

        }
    }
}
