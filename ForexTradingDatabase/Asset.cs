using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
