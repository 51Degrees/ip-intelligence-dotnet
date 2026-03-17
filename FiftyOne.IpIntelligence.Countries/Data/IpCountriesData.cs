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
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace FiftyOne.IpIntelligence.Countries.Data
{
    /// <summary>
    /// Element data for the <see cref="FlowElements.IpCountriesElement"/>.
    /// Contains flat (non-weighted) country code lists.
    /// </summary>
    public class IpCountriesData : ElementDataBase, IIpCountriesData
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        /// <param name="pipeline">The pipeline this data belongs to.</param>
        public IpCountriesData(
            ILogger<IpCountriesData> logger,
            IPipeline pipeline)
            : base(logger, pipeline)
        {
        }

        /// <inheritdoc/>
        public IAspectPropertyValue<IReadOnlyList<string>> CountryCodesGeographicalAll
        {
            get { return (IAspectPropertyValue<IReadOnlyList<string>>)this["CountryCodesGeographicalAll"]; }
        }

        /// <inheritdoc/>
        public IAspectPropertyValue<IReadOnlyList<string>> CountryCodesPopulationAll
        {
            get { return (IAspectPropertyValue<IReadOnlyList<string>>)this["CountryCodesPopulationAll"]; }
        }

        internal void SetCountryCodesGeographicalAll(
            IAspectPropertyValue<IReadOnlyList<string>> value)
        {
            this["CountryCodesGeographicalAll"] = value;
        }

        internal void SetCountryCodesPopulationAll(
            IAspectPropertyValue<IReadOnlyList<string>> value)
        {
            this["CountryCodesPopulationAll"] = value;
        }
    }
}
