using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using ForexTradingWcfServiceLibrary;
namespace ForextTradingWCFServer
{
    /// <summary>
    /// Server for wcf service
    /// </summary>
    class Program
    {
        /// <summary>
        /// Main class for server
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            using (var host = new ServiceHost(typeof(TradingForexService)))
            {
                host.Open();
                Console.WriteLine("Server stared");

                Console.Read();
            }
        }
    }
}
