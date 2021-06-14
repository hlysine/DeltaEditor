using MoreLinq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DeltaQuestionEditor_WPF.Helpers
{
    public class DynamicContractResolver : DefaultContractResolver
    {
        private readonly IEnumerable<string> _propertyNamesToExclude;

        public DynamicContractResolver(IEnumerable<string> propertyNamesToExclude)
        {
            _propertyNamesToExclude = propertyNamesToExclude;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);

            properties.ForEach(x => x.Ignored = x.Ignored || _propertyNamesToExclude.Contains(x.PropertyName));

            return properties;
        }
    }
}
