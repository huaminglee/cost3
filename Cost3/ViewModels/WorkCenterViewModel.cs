using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Cost.Models;

namespace Cost.ViewModels
{
    public class WorkCenterViewModel
    {
        public int Id { get; set; }
        public string WorkCenterCode { get; set; }
        public string WorkCenterName { get; set; }
        public string CostCenter { get; set; }
        public decimal WorkRate { get; set; }
        public string FactoryCode { get; set; }
        public string FactoryName { get; set; }//From Factory
    }
}