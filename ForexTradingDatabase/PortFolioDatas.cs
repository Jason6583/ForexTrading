namespace ForexTradingDatabase
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PortFolioDatas
    {
        public int Id { get; set; }

        public DateTime? DateOfBuy { get; set; }

        public DateTime? DateOfSold { get; set; }

        [StringLength(128)]
        public string User_Email { get; set; }

        public int? TradingPair_Id { get; set; }

        public double Price { get; set; }

        public double Investment { get; set; }

        public virtual TradingPair TradingPair { get; set; }

        public virtual User User { get; set; }
    }
}
