using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace ForexTradingWcfServiceLibrary
{
    [ServiceContract(CallbackContract = typeof(ITradingForexClient))]
    public interface ITradingForexService
    {
        [OperationContract]
        bool LoginUser(string email, string password);
        [OperationContract]
        bool RegisterUser(string name, string surename,string email, string password);
        [OperationContract]
        void SelectTradingPair(string name);
        [OperationContract]
        ForexData GetData(int count, DateTime time);
    }

    [DataContract]
    public class ForexData
    {
        public ForexData(string name, List<KeyValuePair<DateTime, double>> data)
        {
            Name = name;
            Data = data;
        }

        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public List<KeyValuePair<DateTime, double>> Data { get; set; }
    }
}
