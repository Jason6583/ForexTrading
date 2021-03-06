﻿using System;
using System.ComponentModel.DataAnnotations;

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
