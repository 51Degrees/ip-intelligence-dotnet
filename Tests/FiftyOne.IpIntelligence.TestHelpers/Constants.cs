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

using FiftyOne.Pipeline.Engines;
using System;
using System.Collections.Generic;
using System.Text;

namespace FiftyOne.IpIntelligence.TestHelpers
{
    public static class Constants
    {
        public const int IPS_TO_TEST = 10;
        public const string IPI_DATA_FILE_NAME = "51Degrees-LiteV41.ipi";
        public const string IP_FILE_NAME = "evidence.yml";

        public static string Ipv4Address = "8.8.8.8";
        public static string Ipv6Address = "2001:4860:4860::8888";

        public static IEnumerable<PerformanceProfiles> TestableProfiles
        {
            get
            {
                foreach(var x in Enum.GetValues(typeof(PerformanceProfiles)))
                    if (x is PerformanceProfiles profile)
                        yield return profile;
            }
        }
    }
}
