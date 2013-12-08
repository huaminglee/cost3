using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

//namespace Cost.Models.Metadata
namespace Cost.Models
{
    [MetadataType(typeof(RawStockMetadata))]
    public partial class RawStock
    {
        private class RawStockMetadata
        {
            [JsonIgnore]
            public virtual ICollection<RawStockQty> RawStockQty { get; set; }
        }
    }
}