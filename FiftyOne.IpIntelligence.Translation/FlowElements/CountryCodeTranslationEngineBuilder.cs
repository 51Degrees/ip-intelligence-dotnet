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
using System;

namespace FiftyOne.IpIntelligence.Translation.FlowElements
{
    /// <summary>
    /// Builder for the <see cref="CountryCodeTranslationEngine"/> element.
    /// This requires no configuration, as all the configuration for the base
    /// translation engine is taken care of.
    /// </summary>
    public class CountryCodeTranslationEngineBuilder
    {
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="loggerFactory">
        /// Logger factory used by the engine and any element data created.
        /// </param>
        public CountryCodeTranslationEngineBuilder(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        /// <summary>
        /// Build a new instance of <see cref="CountryCodeTranslationEngine"/>.
        /// </summary>
        /// <returns></returns>
        public CountryCodeTranslationEngine Build()
        {
            return CreateEngine(
                _loggerFactory.CreateLogger<CountryCodeTranslationEngine>(),
                CreateData);
        }

        /// <summary>
        /// Construct the engine instance. Subclasses can override this to
        /// return a derived engine type while reusing the rest of
        /// <see cref="Build"/>.
        /// </summary>
        protected virtual CountryCodeTranslationEngine CreateEngine(
            ILogger<FlowElementBase<
                ICountryCodeTranslationData,
                IElementPropertyMetaData>> logger,
            Func<
                IPipeline,
                FlowElementBase<
                    ICountryCodeTranslationData,
                    IElementPropertyMetaData>,
                ICountryCodeTranslationData> elementDataFactory)
        {
            return new CountryCodeTranslationEngine(
                logger,
                elementDataFactory);
        }

        /// <summary>
        /// Creates an instance of CountryCodeTranslationData
        /// </summary>
        /// <param name="pipeline"></param>
        /// <param name="flowElement"></param>
        /// <returns></returns>
        private ICountryCodeTranslationData CreateData(
            IPipeline pipeline,
            FlowElementBase<
                ICountryCodeTranslationData,
                IElementPropertyMetaData> flowElement)
        {
            return new CountryCodeTranslationData(
                _loggerFactory.CreateLogger<CountryCodeTranslationData>(),
                pipeline);
        }
    }
}
