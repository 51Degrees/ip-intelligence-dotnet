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

using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines.Data;
using FiftyOne.Pipeline.Translation.Data;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace FiftyOne.IpIntelligence.Translation.Data
{
    /// <summary>
    /// Concrete implementation of <see cref="ICountryCodeTranslationData"/>.
    /// </summary>
    public class CountryCodeTranslationData : TranslationData, ICountryCodeTranslationData
    {
        internal CountryCodeTranslationData(
            ILogger<TranslationData> logger,
            IPipeline pipeline)
            : base(logger, pipeline)
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
    }
}
