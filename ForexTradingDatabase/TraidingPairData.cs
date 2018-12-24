using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForexTradingDatabase
{
    [Serializable]
    public class TraidingPairData
    {
        public TraidingPairData()
        {
        }


        [Key]
        public int Id { get; set; }
        public TradingPair TradingPair { get; set; }
        public DateTime Date { get; set; }
        public double Value { get; set; }
    }
}
