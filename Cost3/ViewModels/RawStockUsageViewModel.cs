using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Cost.ViewModels
{
    public class RawStockUsageViewModel
    {
        public int Id { get; set; }
        public string MatNumber { get; set; }
        public string MatNR { get; set; }
        public string MatDB { get; set; }
        public decimal Qty { get; set; }
        public string Unit { get; set; }
        public int Version { get; set; }
        public string FactoryCode { get; set; }
    }
}