using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
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
            public ReceiveDataArgs(string name, List<KeyValuePair<DateTime,double>> data)
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
            OnReceiveDataEvent(this, new ReceiveDataArgs(data.Name, data.Data));
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
        private static Client _client;
        public Client Client
        {
            get { return _client; }
            
        }

        private InstanceContext instanceContext;
        private ITradingForexService _tradingServiceClient;
        public Core()
        {
            _client = new Client();
            instanceContext = new InstanceContext(_client);
            
            var client = new DuplexChannelFactory<ITradingForexService>(instanceContext, "TradingService");
            _tradingServiceClient = client.CreateChannel();
            LoginUser("pecho4@gmail.com", "1111");



        }

        /// <summary>
        /// Login user to database
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        public void LoginUser(string email, string password)
        {
            _tradingServiceClient.LoginUser("pecho4@gmail.com", "1111");
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

        //public List<TradingPair> GetAllTradingPairs()
        //{
        //   return _database.GetAllTradingPairs();
        //}
    }
}
