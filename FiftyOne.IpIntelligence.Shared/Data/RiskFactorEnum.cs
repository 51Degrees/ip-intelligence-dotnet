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

using System;
using System.Collections.Generic;
using System.Text;

namespace FiftyOne.IpIntelligence.Shared.Data
{
    /// <summary>
    /// Enum indicating the confidence associated with a network result.
    /// </summary>
    public enum RiskFactorEnum
    {
        /// <summary>
        /// A standard wired or cellular IP with over 99% country accuracy and better than 60% combined accuracy within a 25 km radius globally.
        /// </summary>
        LowRisk = 0,
        /// <summary>
        /// A hosting environment or ASN (like cloud servers, VPNs, proxies, TOR, etc.) but where we are confident the location represents over 60% accuracy at the country level.
        /// </summary>
        ModerateRisk = 1,
        /// <summary>
        /// A hosting network where the location we have is likely valid for less than 50% of the traffic.
        /// </summary>
        HighRisk = 2
    }
}
