/* *********************************************************************
 * This Original Work is copyright of 51 Degrees Mobile Experts Limited.
 * Copyright 2025 51 Degrees Mobile Experts Limited, Davidson House,
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
using FiftyOne.Pipeline.CloudRequestEngine.FlowElements;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines.Data;
using FiftyOne.Pipeline.Engines.FlowElements;
using FiftyOne.Pipeline.Engines.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace FiftyOne.IpIntelligence.Cloud.FlowElements
{
    /// <summary>
    /// Fluent builder used to create a cloud-based IP intelligence
    /// engine.
    /// </summary>
    public class IpiCloudEngineBuilder : AspectEngineBuilderBase<IpiCloudEngineBuilder, IpiCloudEngine>
    {
        private ILoggerFactory _loggerFactory;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="loggerFactory">
        /// The factory to use when creating a logger.
        /// </param>
        public IpiCloudEngineBuilder(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        /// <summary>
        /// Build a new engine using the configured values.
        /// </summary>
        /// <returns>
        /// A new <see cref="IpiCloudEngine"/>
        /// </returns>
        public IpiCloudEngine Build()
        {
            return BuildEngine();
        }

        /// <summary>
        /// This method is called by the base class to create a new
        /// <see cref="IpiCloudEngine"/> instance before 
        /// additional configuration is applied.
        /// </summary>
        /// <param name="properties">
        /// A string list of the properties that the engine should populate.
        /// In this case, this list is ignored as the resource key 
        /// defines the properties that are returned by the cloud service.
        /// </param>
        /// <returns>
        /// A new <see cref="IpiCloudEngine"/> instance.
        /// </returns>
        protected override IpiCloudEngine NewEngine(List<string> properties)
        {
            return new IpiCloudEngine(
                _loggerFactory.CreateLogger<IpiCloudEngine>(),
                CreateData);
        }

        private IpDataCloud CreateData(IPipeline pipeline, FlowElementBase<IpDataCloud, IAspectPropertyMetaData> engine)
        {
            return new IpDataCloud(
                _loggerFactory.CreateLogger<IpDataCloud>(),
                pipeline,
                (IAspectEngine)engine,
                MissingPropertyService.Instance);
        }
    }
}
