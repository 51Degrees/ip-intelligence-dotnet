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
