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
    /// Engine which takes the country code properties from IpCountries element
    /// and translates them to country names. The country names are always in
    /// English. Results are stored using the
    /// <see cref="Constants.CountryNamesKey"/> key.
    /// 
    /// The element data used by this engine is shared with the
    /// <see cref="CountryCodeTranslationEngine" />.
    /// 
    /// All translation files are stored as embedded resources, so no
    /// configuration is required.
    /// </summary>
    public class CountryCodeAllTranslationEngine
        : TranslationEngineBase<ICountryCodeTranslationData>
    {
        /// <summary>
        /// Static translations passed to the base constructor.
        /// </summary>
        private static readonly IReadOnlyCollection<TranslationProperty>
            _translations = new List<TranslationProperty>
            {
                new TranslationProperty(
                    "CountryCodesGeographicalAll",
                    nameof(ICountryCodeTranslationData.CountryNamesGeographicalAll)),
                new TranslationProperty(
                    "CountryCodesPopulationAll",
                    nameof(ICountryCodeTranslationData.CountryNamesPopulationAll))
            };

        /// <inheritdoc/>
        public override string ElementDataKey => Constants.CountryNamesKey;

        /// <summary>
        /// Countructor which is only accessible by the builder.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="elementDataFactory"></param>
        internal CountryCodeAllTranslationEngine(
            ILogger<FlowElementBase<
                ICountryCodeTranslationData,
                IElementPropertyMetaData>> logger,
            Func<
                IPipeline,
                FlowElementBase<
                    ICountryCodeTranslationData,
                    IElementPropertyMetaData>,
                ICountryCodeTranslationData> elementDataFactory)
            : base(
                  "ipcountries",
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
