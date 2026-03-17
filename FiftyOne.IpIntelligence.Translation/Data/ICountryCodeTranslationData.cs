using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Engines.Data;
using FiftyOne.Pipeline.Translation.Data;
using System.Collections.Generic;

namespace FiftyOne.IpIntelligence.Translation.Data
{
    public interface ICountryCodeTranslationData : ITranslationData
    {
        IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>
            CountryNamesGeographic
        { get; }
        IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>
            CountryNamesPopulation
        { get; }
    }
}
