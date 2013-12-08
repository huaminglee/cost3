using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Cost.ViewModels
{
    public class LabourViewModel
    {
        public int Id { get; set; }
        public string MatNumber { get; set; }
        public string WorkCenterCode { get; set; }
        public string WorkCenterName { get; set; }
        public decimal LabourHour { get; set; }
        public int Version { get; set; }
        public string Remark { get; set; }
        public string FactoryCode { get; set; }
    }
}