using System;
using System.ComponentModel.DataAnnotations;

namespace ForexTradingDatabase
{

    /// <summary>
    /// Class for portfolio data in database
    /// </summary>
    [Serializable]
   public class PortFolioData
    {
        public PortFolioData()
        {
            
        }

        [Key]
        public int Id { get; set; }
        public int TradingPairId { get; set; }

        public DateTime? DateOfBuy { get; set; }
        public DateTime? DateOfSold { get; set; }

        public string UserEmail { get; set; }

        public double Price { get; set; }
        public double Investment { get; set; }

    }
}
