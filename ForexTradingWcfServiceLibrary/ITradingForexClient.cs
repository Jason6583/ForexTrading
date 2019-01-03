using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

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
