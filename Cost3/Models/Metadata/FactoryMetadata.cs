using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Cost.Models
{
    [MetadataType(typeof(FactoryMetadata))]
    public partial class Factory
    {
        private class FactoryMetadata
        {
            [JsonIgnore]
            public virtual ICollection<WorkCenter> WorkCenter { get; set; }
        }
    }
}