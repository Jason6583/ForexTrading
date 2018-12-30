using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForexTradingDatabase
{
    [Serializable]
    public class TradingPair
    {
        public TradingPair()
        {
        }

        [Key]
        public int Id { get; set; }
        public string FirstAssetName { get; set; }
        public string SecondAssetName { get; set; }
        public string FullName
        {
            get { return FirstAssetName + "/" + SecondAssetName; }
            set { value = FirstAssetName + "/" + SecondAssetName; }
        }
    }
}
