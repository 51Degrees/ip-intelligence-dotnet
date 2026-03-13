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

using FiftyOne.IpIntelligence.Engine.OnPremise.FlowElements;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines;
using FiftyOne.Pipeline.Engines.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace FiftyOne.IpIntelligence.Countries.FlowElements
{
    /// <summary>
    /// A post-processing flow element that produces flat (non-weighted) country
    /// code lists by combining weighted results from the IPI engine with the
    /// full set of possible country codes from metadata.
    ///
    /// The results are written directly to the <see cref="IIpIntelligenceData"/>
    /// object's <c>CountryCodesGeographicalAll</c> and
    /// <c>CountryCodesPopulationAll</c> properties.
    ///
    /// Must be added to the pipeline after <see cref="IpiOnPremiseEngine"/>.
    /// </summary>
    public class IpCountriesElement : FlowElementBase<IElementData, IElementPropertyMetaData>
    {
        private IpiOnPremiseEngine _ipiEngine;
        private List<string> _allCountryCodes;
        private bool _initialized;
        private readonly object _initLock = new object();
        private readonly ILogger<IpCountriesElement> _logger;

        /// <inheritdoc/>
        public override string ElementDataKey => "ipcountriesall";

        /// <inheritdoc/>
        public override IEvidenceKeyFilter EvidenceKeyFilter { get; } =
            new EvidenceKeyFilterWhitelist(new List<string>());

        /// <inheritdoc/>
        public override IList<IElementPropertyMetaData> Properties { get; } =
            new List<IElementPropertyMetaData>
            {
                new ElementPropertyMetaData(
                    null,
                    "CountryCodesGeographicalAll",
                    typeof(IReadOnlyList<string>),
                    true,
                    "Countries"),
                new ElementPropertyMetaData(
                    null,
                    "CountryCodesPopulationAll",
                    typeof(IReadOnlyList<string>),
                    true,
                    "Countries")
            };

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        internal IpCountriesElement(
            ILogger<IpCountriesElement> logger)
            : base(logger)
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
                        nameof(IpCountriesElement)));
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
        protected override void ProcessInternal(IFlowData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (_ipiEngine == null || _allCountryCodes == null || _allCountryCodes.Count == 0)
            {
                return;
            }

            IIpIntelligenceData ipData;
            try
            {
                ipData = data.Get<IIpIntelligenceData>();
            }
            catch (KeyNotFoundException)
            {
                return;
            }
            catch (InvalidOperationException)
            {
                return;
            }

            // Build the flat lists and write them directly to the IPI data object
            var geoAll = BuildAllList(ipData, "CountryCodesGeographical");
            var popAll = BuildAllList(ipData, "CountryCodesPopulation");

            ((IData)ipData)["CountryCodesGeographicalAll"] = geoAll;
            ((IData)ipData)["CountryCodesPopulationAll"] = popAll;
        }

        private IAspectPropertyValue<IReadOnlyList<string>> BuildAllList(
            IIpIntelligenceData ipData, string propertyName)
        {
            // Try to get the weighted property from the IPI data.
            // If not available, we still return all codes alphabetically.
            List<string> weightedCodes = null;
            object rawValue = null;
            try
            {
                rawValue = ((IElementData)ipData)[propertyName];
            }
            catch (PropertyMissingException)
            {
            }
            catch (KeyNotFoundException)
            {
            }

            if (rawValue is IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> weightedProperty
                && weightedProperty.HasValue)
            {
                weightedCodes = weightedProperty.Value
                    .OrderByDescending(w => w.RawWeighting)
                    .Select(w => w.Value)
                    .ToList();
            }

            if (weightedCodes == null || weightedCodes.Count == 0)
            {
                // No weighted data — return all codes alphabetically
                return new AspectPropertyValue<IReadOnlyList<string>>(
                    new List<string>(_allCountryCodes));
            }

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
