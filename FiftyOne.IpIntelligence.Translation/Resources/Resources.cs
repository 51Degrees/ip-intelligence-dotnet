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

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FiftyOne.IpIntelligence.Translation.Resources
{
    /// <summary>
    /// Resources class used to retrieve the translation files stored
    /// as embedded resources. Public so that subclasses living in other
    /// assemblies (e.g. cloud-side gating wrappers) can read the same
    /// YAML files used by the package's built-in builders.
    /// </summary>
    public class Resources
    {
        /// <summary>
        /// Get the country name translation YAML files.
        /// </summary>
        /// <returns>
        /// Dictionary of file contents keyed on file name.
        /// </returns>
        public static IReadOnlyDictionary<string, string>
            GetCountryResources()
        {
            return GetResources()
                .Where(i => i.Key.StartsWith("countries"))
                .ToDictionary(i => i.Key, i => i.Value);
        }

        /// <summary>
        /// Get the country code translation YAML files.
        /// </summary>
        /// <returns>
        /// Dictionary of file contents keyed on file name.
        /// </returns>
        public static IReadOnlyDictionary<string, string>
            GetCountryCodeResources()
        {
            return GetResources()
                .Where(i => i.Key.StartsWith("countrycodes"))
                .ToDictionary(i => i.Key, i => i.Value);
        }

        /// <summary>
        /// Get all the translation YAML files stored as embedded resources.
        /// </summary>
        /// <returns>
        /// Dictionary of file contents keyed on file name.
        /// </returns>
        private static IReadOnlyDictionary<string, string> GetResources()
        {
            var result = new Dictionary<string, string>();
            var assembly = typeof(Resources).Assembly;
            var resourceNames = assembly.GetManifestResourceNames();
            foreach (var name in resourceNames
                // Only return the resources which are under this resources
                // namespace, and are YAML files.
                .Where(i => i.StartsWith(typeof(Resources).Namespace) &&
                    (i.EndsWith(".yml") || i.EndsWith(".yaml"))))
            {
                using (var reader = new StreamReader(
                    assembly.GetManifestResourceStream(name)))
                {
                    result.Add(
                        name.Replace(typeof(Resources).Namespace + ".", ""),
                        reader.ReadToEnd());
                }
            }
            return result;
        }
    }
}
