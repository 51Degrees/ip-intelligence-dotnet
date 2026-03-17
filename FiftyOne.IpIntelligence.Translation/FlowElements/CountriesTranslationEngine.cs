using FiftyOne.IpIntelligence.Translation.Data;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Translation.Data;
using FiftyOne.Pipeline.Translation.FlowElements;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace FiftyOne.IpIntelligence.Translation.FlowElements
{
    public class CountriesTranslationEngine : TranslationEngineBase<ICountriesTranslationData>
    {
        private static readonly IReadOnlyCollection<TranslationProperty> _translations =
            new List<TranslationProperty>
        {
            new TranslationProperty(
                nameof(ICountryCodeTranslationData.CountryNamesGeographic),
                nameof(ICountriesTranslationData.CountryNamesGeographicTranslated)),
            new TranslationProperty(
                nameof(ICountryCodeTranslationData.CountryNamesPopulation),
                nameof(ICountriesTranslationData.CountryNamesPopulationTranslated))
        };

        public override string ElementDataKey => "countriestranslated";

        public CountriesTranslationEngine(
            ILogger<FlowElementBase<ICountriesTranslationData, IElementPropertyMetaData>> logger,
            Func<IPipeline, FlowElementBase<ICountriesTranslationData, IElementPropertyMetaData>, ICountriesTranslationData> elementDataFactory)
            : base(
                  "countries",
                  _translations,
                  Resources.Resources.GetCountryResources(),
                  null,
                  MissingTranslationBehavior.Original,
                  logger,
                  elementDataFactory)
        {
        }
    }
}
