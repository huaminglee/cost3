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
    
    public partial class DetailRawStock
    {
        public long Id { get; set; }
        public string ProductVersion { get; set; }
        public string PNumber { get; set; }
        public string CNumber { get; set; }
        public string MatNR { get; set; }
        public string MatDB { get; set; }
        public Nullable<int> RawStockVersion { get; set; }
        public Nullable<decimal> Qty { get; set; }
        public Nullable<decimal> CQty { get; set; }
        public Nullable<decimal> RawStockTotal { get; set; }
        public string Unit { get; set; }
        public Nullable<decimal> RawStockCost { get; set; }
    }
}
