/* *********************************************************************
 * This Original Work is copyright of 51 Degrees Mobile Experts Limited.
 * Copyright 2026 51 Degrees Mobile Experts Limited, Davidson House,
 * Forbury Square, Reading, Berkshire, United Kingdom RG1 3EU.
 *
 * This Original Work is licensed under the European Union Public Licence
 * (EUPL) v.1.2 and is subject to its terms as set out below.
 *
 * If a copy of the EUPL was not distributed with this file, You can obtain
 * one at https://opensource.org/licenses/EUPL-1.2.
 *
 * The 'Compatible Licences' set out in the Appendix to the EUPL (as may be
 * amended by the European Commission) shall be deemed incompatible for
 * the purposes of the Work and the provisions of the compatibility
 * clause in Article 5 of the EUPL shall not apply.
 *
 * If using the Work as, or as part of, a network application, by
 * including the attribution notice(s) required under Article 5 of the EUPL
 * in the end user terms of the application under an appropriate heading,
 * such notice(s) shall fulfill the requirements of that article.
 * ********************************************************************* */

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
    /// <summary>
    /// Engine which takes the country name properties from
    /// <see cref="CountryCodeTranslationEngine"/> and translates them into the
    /// browser language determined from the evidence. Results are stored using
    /// the <see cref="Constants.CountryNamesTranslatedKey"/> key.
    /// 
    /// All translation files are stored as embedded resources, so no
    /// configuration is required.
    /// </summary>
    public class CountriesTranslationEngine
        : TranslationEngineBase<ICountriesTranslationData>
    {
        /// <summary>
        /// Static translations passed to the base constructor.
        /// </summary>
        private static readonly IReadOnlyCollection<TranslationProperty>
            _translations = new List<TranslationProperty>
            {
                new TranslationProperty(
                    nameof(ICountryCodeTranslationData.CountryNamesGeographical),
                    nameof(ICountriesTranslationData.CountryNamesGeographicalTranslated)),
                new TranslationProperty(
                    nameof(ICountryCodeTranslationData.CountryNamesPopulation),
                    nameof(ICountriesTranslationData.CountryNamesPopulationTranslated))
            };

        public override string ElementDataKey =>
            Constants.CountryNamesTranslatedKey;

        public CountriesTranslationEngine(
            ILogger<FlowElementBase<ICountriesTranslationData, IElementPropertyMetaData>> logger,
            Func<IPipeline, FlowElementBase<ICountriesTranslationData, IElementPropertyMetaData>, ICountriesTranslationData> elementDataFactory)
            : base(
                  "countrynames",
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
