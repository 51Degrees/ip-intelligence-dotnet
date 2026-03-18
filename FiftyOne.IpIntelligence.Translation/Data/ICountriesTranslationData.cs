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
using FiftyOne.Pipeline.Engines.Data;
using FiftyOne.Pipeline.Translation.Data;
using System.Collections.Generic;

namespace FiftyOne.IpIntelligence.Translation.Data
{
    /// <summary>
    /// Contains trawnslated country names for both the geographical and
    /// population weighted lists from <see cref="ICountryCodeTranslationData"/>.
    /// </summary>
    public interface ICountriesTranslationData : ITranslationData
    {
        /// <summary>
        /// Translated list of country names based on the geographical weighted
        /// list from IP Intelligence. This is translated to the browser
        /// language determined from the evidence.
        /// </summary>
        IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>
            CountryNamesGeographicalTranslated { get; }

        /// <summary>
        /// Translated list of country names based on the population weighted
        /// list from IP Intelligence. This is translated to the browser
        /// language determined from the evidence.
        /// </summary>
        IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>
            CountryNamesPopulationTranslated { get; }

        /// <summary>
        /// Translated list of country names based on the geographical list of
        /// all countries from the IpCountriesElement. This is translated to
        /// the browser language determined from the evidence.
        /// </summary>
        IAspectPropertyValue<IReadOnlyList<string>>
            CountryNamesGeographicalAllTranslated { get; }

        /// <summary>
        /// Translated list of country names based on the population list of
        /// all countries from the IpCountriesElement. This is translated to
        /// the browser language determined from the evidence.
        /// </summary>
        IAspectPropertyValue<IReadOnlyList<string>>
            CountryNamesPopulationAllTranslated { get; }
    }
}
