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
using FiftyOne.Pipeline.Engines.Data;
using FiftyOne.Pipeline.Engines.FiftyOne.Data;
using FiftyOne.Pipeline.Engines.FlowElements;
using FiftyOne.Pipeline.Engines.Services;
using FiftyOne.Pipeline.Translation.Data;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace FiftyOne.IpIntelligence.Cloud.Data
{
    /// <summary>
    /// A cloud-specific data class that implements IAspectData for use
    /// with <see cref="FlowElements.IpiCloudTranslationEngine"/>.
    /// </summary>
    public class CloudCountryCodeTranslationData : AspectDataBase, ICountryCodeTranslationData
    {
        /// <summary>
        /// Construct a new instance.
        /// </summary>
        /// <param name="logger">
        /// The logger instance to use.
        /// </param>
        /// <param name="pipeline">
        /// The Pipeline that created this data instance.
        /// </param>
        /// <param name="engine">
        /// The engine that created this instance.
        /// </param>
        public CloudCountryCodeTranslationData(
            ILogger<AspectDataBase> logger,
            IPipeline pipeline,
            IAspectEngine engine)
            : base(logger, pipeline, engine, FiftyOne.Pipeline.Engines.Services.MissingPropertyService.Instance)
        {
        }

        /// <inheritdoc/>
        public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>
            CountryNamesGeographical =>
                GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>(
                    nameof(CountryNamesGeographical));

        /// <inheritdoc/>
        public IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>
            CountryNamesPopulation =>
                GetAs<IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>(
                    nameof(CountryNamesPopulation));

        /// <inheritdoc/>
        public IAspectPropertyValue<IReadOnlyList<string>>
            CountryNamesGeographicalAll =>
                GetAs<IAspectPropertyValue<IReadOnlyList<string>>>(
                    nameof(CountryNamesGeographicalAll));

        /// <inheritdoc/>
        public IAspectPropertyValue<IReadOnlyList<string>>
            CountryNamesPopulationAll =>
                GetAs<IAspectPropertyValue<IReadOnlyList<string>>>(
                    nameof(CountryNamesPopulationAll));
    }
}
