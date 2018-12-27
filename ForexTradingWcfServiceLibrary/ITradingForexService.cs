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
        ForexData GetData(int count, string tradingPair);
        [OperationContract]
        List<string> GetAllTradingPairs();

        [OperationContract]
        void AddAsset(string tradingPair, long dateOfBuy);

        [OperationContract]
        long GetServerTime();

        [OperationContract]
        List<string[]> GetPortFolio();

        [OperationContract]
        double GetActualValue(string firstAssetName, string secondAssetNamer);

    }
    
    [DataContract]
    public class ForexData
    {
        public ForexData(string name, List<KeyValuePair<long, double>> data)
        {
            Name = name;
            Data = data;
        }

        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public List<KeyValuePair<long, double>> Data { get; set; }
    }
}
