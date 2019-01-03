using System.ServiceModel;

namespace ForexTradingWcfServiceLibrary
{
    /// <summary>
    /// Interface for callback client 
    /// </summary>
    public interface ITradingForexClient
    {
        [OperationContract(IsOneWay = true)]
         void ReceiveData(ForexData data);
    }
}
