using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FiftyOne.IpIntelligence.Translation.Resources
{
    internal class Resources
    {
        internal static IReadOnlyDictionary<string, string> GetCountryResources()
        {
            return GetResources()
                .Where(i => i.Key.StartsWith("countries"))
                .ToDictionary(i => i.Key, i => i.Value);
        }

        internal static IReadOnlyDictionary<string, string> GetCountryCodeResources()
        {
            return GetResources()
                .Where(i => i.Key.StartsWith("countrycodes"))
                .ToDictionary(i => i.Key, i => i.Value);
        }

        private static IReadOnlyDictionary<string, string> GetResources()
        {
            var result = new Dictionary<string, string>();
            var assembly = typeof(Resources).Assembly;
            var resourceNames = assembly.GetManifestResourceNames();
            foreach (var name in resourceNames
                .Where(i => i.StartsWith(typeof(Resources).Namespace)))
            {
                using (var reader = new StreamReader(assembly.GetManifestResourceStream(name)))
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
