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

using FiftyOne.IpIntelligence.Cloud.Data;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines.Data;
using FiftyOne.Pipeline.Engines.FlowElements;
using FiftyOne.Pipeline.Engines.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace FiftyOne.IpIntelligence.Cloud.FlowElements
{
    /// <summary>
    /// Fluent builder used to create a cloud-based translation engine
    /// for IP intelligence country name translations.
    /// </summary>
    public class IpiCloudTranslationEngineBuilder : AspectEngineBuilderBase<IpiCloudTranslationEngineBuilder, IpiCloudTranslationEngine>
    {
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="loggerFactory">
        /// The factory to use when creating a logger.
        /// </param>
        public IpiCloudTranslationEngineBuilder(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        /// <summary>
        /// Build a new engine using the configured values.
        /// </summary>
        /// <returns>
        /// A new <see cref="IpiCloudTranslationEngine"/>
        /// </returns>
        public IpiCloudTranslationEngine Build()
        {
            return BuildEngine();
        }

        /// <summary>
        /// This method is called by the base class to create a new
        /// <see cref="IpiCloudTranslationEngine"/> instance.
        /// </summary>
        /// <param name="properties">
        /// A string list of the properties that the engine should populate.
        /// </param>
        /// <returns>
        /// A new <see cref="IpiCloudTranslationEngine"/> instance.
        /// </returns>
        protected override IpiCloudTranslationEngine NewEngine(List<string> properties)
        {
            return new IpiCloudTranslationEngine(
                _loggerFactory.CreateLogger<IpiCloudTranslationEngine>(),
                CreateData);
        }

        private CloudCountryCodeTranslationData CreateData(
            IPipeline pipeline,
            FlowElementBase<CloudCountryCodeTranslationData, IAspectPropertyMetaData> engine)
        {
            return new CloudCountryCodeTranslationData(
                _loggerFactory.CreateLogger<CloudCountryCodeTranslationData>(),
                pipeline,
                (IAspectEngine)engine);
        }
    }
}
