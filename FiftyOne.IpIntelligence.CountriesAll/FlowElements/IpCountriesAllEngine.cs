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

using FiftyOne.IpIntelligence.CountriesAll.Data;
using FiftyOne.IpIntelligence.Engine.OnPremise.FlowElements;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines;
using FiftyOne.Pipeline.Engines.Data;
using FiftyOne.Pipeline.Engines.FlowElements;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace FiftyOne.IpIntelligence.CountriesAll.FlowElements
{
    /// <summary>
    /// A post-processing engine that produces flat (non-weighted) country
    /// code lists by combining weighted results from the IPI engine with the
    /// full set of possible country codes from metadata.
    ///
    /// Must be added to the pipeline after <see cref="IpiOnPremiseEngine"/>.
    /// </summary>
    public class IpCountriesAllEngine : AspectEngineBase<IpCountriesAllData, IAspectPropertyMetaData>
    {
        private IpiOnPremiseEngine _ipiEngine;
        private List<string> _allCountryCodes;
        private bool _initialized;
        private readonly object _initLock = new object();
        private readonly ILogger<IpCountriesAllEngine> _logger;

        private const string NoIpiEngineMessage =
            "IPI engine or CountryCode metadata not available.";
        private const string NoIpDataMessage =
            "IP Intelligence data not available in flow data.";

        /// <inheritdoc/>
        public override string ElementDataKey => "ipcountriesall";

        /// <inheritdoc/>
        public override IEvidenceKeyFilter EvidenceKeyFilter { get; } =
            new EvidenceKeyFilterWhitelist(new List<string>());

        /// <inheritdoc/>
        public override string DataSourceTier => "n/a";

        /// <inheritdoc/>
        public override IList<IAspectPropertyMetaData> Properties { get; } =
            new List<IAspectPropertyMetaData>
            {
                new AspectPropertyMetaData(
                    null,
                    "CountryCodesGeographicalAll",
                    typeof(IReadOnlyList<string>),
                    "Countries",
                    new List<string> { "n/a" },
                    true,
                    "All country codes ordered by geographical weighting " +
                    "descending, followed by remaining codes alphabetically."),
                new AspectPropertyMetaData(
                    null,
                    "CountryCodesPopulationAll",
                    typeof(IReadOnlyList<string>),
                    "Countries",
                    new List<string> { "n/a" },
                    true,
                    "All country codes ordered by population weighting " +
                    "descending, followed by remaining codes alphabetically.")
            };

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        /// <param name="aspectDataFactory">Factory to create data instances.</param>
        internal IpCountriesAllEngine(
            ILogger<IpCountriesAllEngine> logger,
            Func<IPipeline, FlowElementBase<IpCountriesAllData, IAspectPropertyMetaData>,
                IpCountriesAllData> aspectDataFactory)
            : base(logger, aspectDataFactory)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public override void AddPipeline(IPipeline pipeline)
        {
            if (pipeline == null)
            {
                throw new ArgumentNullException(nameof(pipeline));
            }
            base.AddPipeline(pipeline);
            EnsureInitialized(pipeline);
        }

        private void EnsureInitialized(IPipeline pipeline)
        {
            if (_initialized)
            {
                return;
            }

            lock (_initLock)
            {
                if (_initialized)
                {
                    return;
                }

                _ipiEngine = pipeline.GetElement<IpiOnPremiseEngine>();

                if (_ipiEngine == null)
                {
                    _logger.LogWarning(string.Format(
                        CultureInfo.InvariantCulture,
                        "{0} not found in the pipeline. {1} will not have values.",
                        nameof(IpiOnPremiseEngine),
                        nameof(IpCountriesAllEngine)));
                }
                else
                {
                    LoadAllCountryCodes();
                }

                _initialized = true;
            }
        }

        private void LoadAllCountryCodes()
        {
            var countryCodeProperty = _ipiEngine.Properties
                .FirstOrDefault(p => string.Equals(
                    p.Name, "CountryCode", StringComparison.OrdinalIgnoreCase));

            if (countryCodeProperty == null)
            {
                _logger.LogWarning(string.Format(
                    CultureInfo.InvariantCulture,
                    "CountryCode property not found in {0} metadata.",
                    nameof(IpiOnPremiseEngine)));
                _allCountryCodes = new List<string>();
                return;
            }

            _allCountryCodes = countryCodeProperty.GetValues()
                .Select(v => v.Name)
                .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
                .ToList();

            _logger.LogInformation(string.Format(
                CultureInfo.InvariantCulture,
                "Loaded {0} country codes from IPI engine metadata.",
                _allCountryCodes.Count));
        }

        /// <inheritdoc/>
        protected override void ProcessEngine(IFlowData data, IpCountriesAllData aspectData)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (aspectData == null)
            {
                throw new ArgumentNullException(nameof(aspectData));
            }

            if (_ipiEngine == null || _allCountryCodes == null || _allCountryCodes.Count == 0)
            {
                SetNoValue(aspectData, NoIpiEngineMessage);
                return;
            }

            IIpIntelligenceData ipData;
            try
            {
                ipData = data.Get<IIpIntelligenceData>();
            }
            catch (KeyNotFoundException)
            {
                SetNoValue(aspectData, NoIpDataMessage);
                return;
            }
            catch (InvalidOperationException)
            {
                SetNoValue(aspectData, NoIpDataMessage);
                return;
            }

            aspectData.CountryCodesGeographicalAll = BuildAllList(
                ipData, "CountryCodesGeographical");
            aspectData.CountryCodesPopulationAll = BuildAllList(
                ipData, "CountryCodesPopulation");
        }

        private static void SetNoValue(IpCountriesAllData aspectData, string message)
        {
            aspectData.CountryCodesGeographicalAll =
                new AspectPropertyValue<IReadOnlyList<string>>()
                {
                    NoValueMessage = message
                };
            aspectData.CountryCodesPopulationAll =
                new AspectPropertyValue<IReadOnlyList<string>>()
                {
                    NoValueMessage = message
                };
        }

        private IAspectPropertyValue<IReadOnlyList<string>> BuildAllList(
            IIpIntelligenceData ipData, string propertyName)
        {
            object rawValue;
            try
            {
                rawValue = ((IElementData)ipData)[propertyName];
            }
            catch (PropertyMissingException)
            {
                return new AspectPropertyValue<IReadOnlyList<string>>()
                {
                    NoValueMessage = string.Format(
                        CultureInfo.InvariantCulture,
                        "Property '{0}' not available.",
                        propertyName)
                };
            }
            catch (KeyNotFoundException)
            {
                return new AspectPropertyValue<IReadOnlyList<string>>()
                {
                    NoValueMessage = string.Format(
                        CultureInfo.InvariantCulture,
                        "Property '{0}' not available.",
                        propertyName)
                };
            }

            var weightedProperty =
                rawValue as IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>;

            if (weightedProperty == null || !weightedProperty.HasValue)
            {
                var noValueMsg = weightedProperty?.NoValueMessage
                    ?? string.Format(
                        CultureInfo.InvariantCulture,
                        "Property '{0}' has no value.",
                        propertyName);
                return new AspectPropertyValue<IReadOnlyList<string>>()
                {
                    NoValueMessage = noValueMsg
                };
            }

            // Sort weighted codes by weight descending, strip weights
            var weightedCodes = weightedProperty.Value
                .OrderByDescending(w => w.RawWeighting)
                .Select(w => w.Value)
                .ToList();

            // Build set for fast lookup of already-included codes
            var includedCodes = new HashSet<string>(
                weightedCodes, StringComparer.OrdinalIgnoreCase);

            // Remaining codes from the master list, excluding already-included ones
            // Master list is already sorted alphabetically
            var remainingCodes = _allCountryCodes
                .Where(code => !includedCodes.Contains(code));

            // Concatenate: weighted (desc) + remaining (alpha)
            var result = new List<string>(weightedCodes.Count + _allCountryCodes.Count);
            result.AddRange(weightedCodes);
            result.AddRange(remainingCodes);

            return new AspectPropertyValue<IReadOnlyList<string>>(result);
        }

        /// <inheritdoc/>
        protected override void ManagedResourcesCleanup()
        {
        }

        /// <inheritdoc/>
        protected override void UnmanagedResourcesCleanup()
        {
        }
    }
}
