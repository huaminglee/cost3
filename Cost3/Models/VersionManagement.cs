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
    
    public partial class VersionManagement
    {
        public int Id { get; set; }
        public string ProductVersion { get; set; }
        public string PNumber { get; set; }
        public string CNumber { get; set; }
        public Nullable<int> LabourVersion { get; set; }
        public Nullable<int> RawStockVersion { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
    }
}
