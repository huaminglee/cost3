using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Cost.Models
{
    [MetadataType(typeof(WorkCenterMetadata))]
    public partial class WorkCenter
    {
        private class WorkCenterMetadata
        {
            [JsonIgnore]
            //public virtual Factory Factory { get; set; }
            public virtual ICollection<Labour> Labour { get; set; }
        }
    }
}