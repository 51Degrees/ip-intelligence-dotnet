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

using FiftyOne.IpIntelligence.Countries.Data;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.FlowElements;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace FiftyOne.IpIntelligence.Countries.FlowElements
{
    /// <summary>
    /// Builder for <see cref="IpCountriesElement"/>.
    /// </summary>
    public class IpCountriesElementBuilder
    {
        private readonly ILoggerFactory _loggerFactory;
        private string _countryCodesFile;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="loggerFactory">Logger factory to use.</param>
        public IpCountriesElementBuilder(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        /// <summary>
        /// Optionally set the path to a custom CountryCodes.json file.
        /// If not set, the embedded resource shipped with this package
        /// will be used.
        /// </summary>
        /// <param name="filePath">
        /// Path to a JSON file containing an array of country code strings.
        /// </param>
        /// <returns>This builder.</returns>
        public IpCountriesElementBuilder SetCountryCodesFile(string filePath)
        {
            _countryCodesFile = filePath;
            return this;
        }

        /// <summary>
        /// Build a new <see cref="IpCountriesElement"/> instance.
        /// </summary>
        /// <returns>A new element instance.</returns>
        public IpCountriesElement Build()
        {
            var logger = _loggerFactory.CreateLogger<IpCountriesElement>();
            List<string> codes;

            if (!string.IsNullOrEmpty(_countryCodesFile))
            {
                var json = File.ReadAllText(_countryCodesFile);
                codes = JsonSerializer.Deserialize<List<string>>(json);
                codes.Sort(StringComparer.OrdinalIgnoreCase);
                logger.LogInformation(
                    "Loaded {0} country codes from '{1}'.",
                    codes.Count,
                    _countryCodesFile);
            }
            else
            {
                codes = LoadFromEmbeddedResource();
                logger.LogInformation(
                    "Loaded {0} country codes from embedded resource.",
                    codes.Count);
            }

            return new IpCountriesElement(logger, codes,
                (pipeline, element) =>
                    new IpCountriesData(
                        _loggerFactory.CreateLogger<IpCountriesData>(),
                        pipeline));
        }

        private static List<string> LoadFromEmbeddedResource()
        {
            var assembly = typeof(IpCountriesElementBuilder).Assembly;
            var resourceName = "CountryCodes.json";

            // Find the resource by suffix match
            foreach (var name in assembly.GetManifestResourceNames())
            {
                if (name.EndsWith(resourceName, StringComparison.OrdinalIgnoreCase))
                {
                    resourceName = name;
                    break;
                }
            }

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException(
                        "Embedded resource 'CountryCodes.json' not found.");
                }

                using (var reader = new StreamReader(stream))
                {
                    var json = reader.ReadToEnd();
                    var codes = JsonSerializer.Deserialize<List<string>>(json);
                    codes.Sort(StringComparer.OrdinalIgnoreCase);
                    return codes;
                }
            }
        }
    }
}
