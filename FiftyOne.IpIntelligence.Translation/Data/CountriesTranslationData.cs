using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines.Data;
using FiftyOne.Pipeline.Translation.Data;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace FiftyOne.IpIntelligence.Translation.Data
{
    public class CountriesTranslationData : TranslationData, ICountriesTranslationData
    {
        public CountriesTranslationData(
            ILogger<TranslationData> logger,
            IPipeline pipeline)
            : base(logger, pipeline)
        {
        }

        public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>
            CountryNamesGeographicTranslated =>
            GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>(
                nameof(CountryNamesGeographicTranslated));

        public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>
            CountryNamesPopulationTranslated =>
            GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>(
                nameof(CountryNamesPopulationTranslated));
    }
}
