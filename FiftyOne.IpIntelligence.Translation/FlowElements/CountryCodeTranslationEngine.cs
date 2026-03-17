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

    public class CountryCodeTranslationEngine : TranslationEngineBase<ICountryCodeTranslationData>
    {
        private static readonly IReadOnlyCollection<TranslationProperty> _translations =
            new List<TranslationProperty>
        {
            new TranslationProperty(
                "CountriesGeographic",
                nameof(ICountryCodeTranslationData.CountryNamesGeographic)),
            new TranslationProperty(
                "CountriesPopulation",
                nameof(ICountryCodeTranslationData.CountryNamesPopulation))
        };

        public override string ElementDataKey => "countries";

        public CountryCodeTranslationEngine(
            ILogger<FlowElementBase<ICountryCodeTranslationData, IElementPropertyMetaData>> logger,
            Func<IPipeline, FlowElementBase<ICountryCodeTranslationData, IElementPropertyMetaData>, ICountryCodeTranslationData> elementDataFactory)
            : base(
                  "ip",
                  _translations,
                  Resources.Resources.GetCountryCodeResources(),
                  "en_GB",
                  MissingTranslationBehavior.Original,
                  logger,
                  elementDataFactory)
        {
        }
    }
}
