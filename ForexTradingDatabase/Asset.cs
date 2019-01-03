using System;
using System.ComponentModel.DataAnnotations;

namespace ForexTradingDatabase
{  
    /// <summary>
    /// Asset class in database
    /// </summary>
    [Serializable]
    public class Asset
    {
        public Asset()
        {
        }

        [Key]
        public string Name { get; set; }
    }
}
