using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Shared.Models
{
    [DataContract]
    public class AquaticSpecies
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string Name { get; set; } = string.Empty;

        [DataMember]
        public string ScientificName { get; set; } = String.Empty;

        [DataMember]
        public string Habitat { get; set; } = String.Empty;

        [DataMember]
        public string WaterType { get; set; } = String.Empty;

        [DataMember]
        public int AverageLifespand { get; set; }
    }
}
