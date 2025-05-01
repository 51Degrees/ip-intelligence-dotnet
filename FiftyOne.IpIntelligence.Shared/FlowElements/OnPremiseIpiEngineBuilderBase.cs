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

using FiftyOne.Pipeline.Engines.FiftyOne.FlowElements;
using FiftyOne.Pipeline.Engines.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace FiftyOne.IpIntelligence.Shared.FlowElements
{
    /// <summary>
    /// Fluent builder that is used to create an on-premise IP intelligence 
    /// aspect engine.
    /// </summary>
    /// <typeparam name="TBuilder">
    /// The type of engine builder for fluent methods to return.
    /// </typeparam>
    /// <typeparam name="TEngine">
    /// The type of engine that will be built. 
    /// </typeparam>
    public abstract class OnPremiseIpiEngineBuilderBase<TBuilder, TEngine>
        : FiftyOneOnPremiseAspectEngineBuilderBase<TBuilder, TEngine>
        where TBuilder : OnPremiseIpiEngineBuilderBase<TBuilder, TEngine>
        where TEngine : IFiftyOneAspectEngine
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataUpdateService">
        /// The <see cref="IDataUpdateService"/> to use when registering
        /// data files to check for for automatic updates.
        /// </param>
        public OnPremiseIpiEngineBuilderBase(
            IDataUpdateService dataUpdateService)
            : base(dataUpdateService)
        {
        }

        /// <summary>
        /// Set the expected number of concurrent operations using the engine.
        /// This sets the concurrency of the internal caches to mitigate
        /// excessive contention.
        /// </summary>
        /// <param name="concurrency">Expected concurrent accesses</param>
        /// <returns>This builder</returns>
        public abstract TBuilder SetConcurrency(ushort concurrency);
    }
}
