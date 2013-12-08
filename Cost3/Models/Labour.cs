//------------------------------------------------------------------------------
// <auto-generated>
//    此代码是根据模板生成的。
//
//    手动更改此文件可能会导致应用程序中发生异常行为。
//    如果重新生成代码，则将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Cost.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Labour
    {
        public int Id { get; set; }
        public string MatNumber { get; set; }
        public string WorkCenterCode { get; set; }
        public Nullable<decimal> LabourHour { get; set; }
        public Nullable<int> Version { get; set; }
        public string FactoryCode { get; set; }
        public string Remark { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
    
        public virtual WorkCenter WorkCenter { get; set; }
    }
}