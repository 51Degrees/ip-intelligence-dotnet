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

using FiftyOne.IpIntelligence.Engine.OnPremise.Data;
using FiftyOne.IpIntelligence.Engine.OnPremise.Interop;
using FiftyOne.IpIntelligence.Shared.FlowElements;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.Exceptions;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines;
using FiftyOne.Pipeline.Engines.FiftyOne.Data;
using FiftyOne.Pipeline.Engines.FlowElements;
using FiftyOne.Pipeline.Engines.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using FiftyOne.IpIntelligence.Engine.OnPremise.Wrappers;

namespace FiftyOne.IpIntelligence.Engine.OnPremise.FlowElements
{
    /// <summary>
    /// Builder for the <see cref="IpiOnPremiseEngine"/>. All options
    /// for the engine should be set here.
    /// </summary>
    public class IpiOnPremiseEngineBuilder
       : IpiOnPremiseEngineBuilderBase<IpiOnPremiseEngine>
    {
        #region Constructor

        /// <summary>
        /// Construct a new instance of the builder.
        /// </summary>
        /// <param name="loggerFactory">
        /// Factory used to create loggers for the engine
        /// </param>
        public IpiOnPremiseEngineBuilder(
            ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
        }

        /// <summary>
        /// Construct a new instance of the builder.
        /// </summary>
        /// <param name="loggerFactory">
        /// Factory used to create loggers for the engine
        /// </param>
        /// <param name="dataUpdateService">
        /// Data update service used to keep the engine's data up to date.
        /// </param>
        public IpiOnPremiseEngineBuilder(
            ILoggerFactory loggerFactory,
            IDataUpdateService dataUpdateService)
            : base(loggerFactory, dataUpdateService)
        {
        }

        #endregion

        #region Protected Overrides

        /// <summary>
        /// Creates a new instance of <see cref="DeviceDetectionHashEngine"/>.
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="deviceDataFactory"></param>
        /// <param name="tempDataFilePath"></param>
        /// <returns></returns>
        protected override IpiOnPremiseEngine CreateEngine(
            ILoggerFactory loggerFactory,
            Func<IPipeline, FlowElementBase<
                IIpDataOnPremise,
                IFiftyOneAspectPropertyMetaData>,
                IIpDataOnPremise> deviceDataFactory,
            string tempDataFilePath)
        {

            return new IpiOnPremiseEngine(
                loggerFactory,
                deviceDataFactory,
                tempDataFilePath);
        }

        #endregion
    }
}
