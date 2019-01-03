using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForexTradingDatabase
{
    /// <summary>
    /// Class for trading pair in database
    /// </summary>
    [Serializable]
    public class TradingPairData
    {
        public TradingPairData()
        {
        }


        [Key]
        public int Id { get; set; }
        public int TradingPairId { get; set; }
        public DateTime Date { get; set; }
        public double Value { get; set; }
    }
}
