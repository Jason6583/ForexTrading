namespace ForexTradingDatabase
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TraidingPairDatas
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public double Value { get; set; }

        public int? TradingPair_Id { get; set; }

        public virtual TradingPair TradingPair { get; set; }
    }
}
