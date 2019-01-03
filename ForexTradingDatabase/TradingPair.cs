using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForexTradingDatabase
{
    /// <summary>
    /// Class for trading pair in database
    /// </summary>
    [Serializable]
    public class TradingPair
    {
        public TradingPair()
        {
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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
