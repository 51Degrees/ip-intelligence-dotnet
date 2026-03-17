using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Engines.Data;
using FiftyOne.Pipeline.Translation.Data;
using System.Collections.Generic;

namespace FiftyOne.IpIntelligence.Translation.Data
{
    public interface ICountriesTranslationData : ITranslationData
    {
        IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>
            CountryNamesGeographicTranslated { get; }
        IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>
            CountryNamesPopulationTranslated { get; }
    }
}
