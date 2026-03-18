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
using Microsoft.Extensions.Logging;

namespace FiftyOne.IpIntelligence.Translation.FlowElements
{
    /// <summary>
    /// Builder for the <see cref="CountriesTranslationEngine"/> element.
    /// This requires no configuration, as all the configuration for the base
    /// translation engine is taken care of.
    /// </summary>
    public class CountriesTranslationEngineBuilder
    {
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="loggerFactory">
        /// Logger factory used by the engine and any element data created.
        /// </param>
        public CountriesTranslationEngineBuilder(ILoggerFactory loggerFactory) 
        {
            _loggerFactory = loggerFactory;
        }

        /// <summary>
        /// Build a new instance of <see cref="CountryCodeTranslationEngine"/>.
        /// </summary>
        /// <returns></returns>
        public CountriesTranslationEngine Build()
        {
            return new CountriesTranslationEngine(
                _loggerFactory.CreateLogger<CountriesTranslationEngine>(),
                CreateData);
        }

        /// <summary>
        /// Creates an instance of <see cref="CountriesTranslationData"/>.
        /// </summary>
        /// <param name="pipeline"></param>
        /// <param name="flowElement"></param>
        /// <returns></returns>
        private ICountriesTranslationData CreateData(
            IPipeline pipeline,
            FlowElementBase<
                ICountriesTranslationData,
                IElementPropertyMetaData> flowElement)
        {
            return new CountriesTranslationData(
                _loggerFactory.CreateLogger<CountriesTranslationData>(),
                pipeline);
        }
    }
}
