using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines.Data;
using FiftyOne.Pipeline.Translation.Data;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace FiftyOne.IpIntelligence.Translation.Data
{
    public class CountryCodeTranslationData : TranslationData, ICountryCodeTranslationData
    {
        public CountryCodeTranslationData(
            ILogger<TranslationData> logger,
            IPipeline pipeline)
            : base(logger, pipeline)
        {
        }

        public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>
            CountryNamesGeographic =>
            GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>(
                nameof(CountryNamesGeographic));

        public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>
            CountryNamesPopulation =>
            GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>(
                nameof(CountryNamesPopulation));

    }
}
