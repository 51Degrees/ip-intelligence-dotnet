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
using FiftyOne.Pipeline.Engines;
using FiftyOne.Pipeline.Engines.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FiftyOne.IpIntelligence.Countries.FlowElements
{
    /// <summary>
    /// A post-processing flow element that produces flat (non-weighted) country
    /// code lists by combining weighted results from the IPI engine with the
    /// full set of possible country codes loaded from a JSON file.
    ///
    /// The results are stored in the element's own <see cref="IIpCountriesData"/>
    /// object, accessible via <c>flowData.Get&lt;IIpCountriesData&gt;()</c>.
    ///
    /// Must be added to the pipeline after the IPI engine.
    /// </summary>
    public class IpCountriesElement : FlowElementBase<IIpCountriesData, IElementPropertyMetaData>
    {
        private readonly List<string> _allCountryCodes;
        private readonly ILogger<IpCountriesElement> _logger;

        /// <inheritdoc/>
        public override string ElementDataKey => "ipcountries";

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
        /// <param name="allCountryCodes">
        /// The complete list of country codes, already sorted alphabetically.
        /// </param>
        /// <param name="elementDataFactory">
        /// Factory method to create <see cref="IIpCountriesData"/> instances.
        /// </param>
        internal IpCountriesElement(
            ILogger<IpCountriesElement> logger,
            List<string> allCountryCodes,
            Func<IPipeline,
                FlowElementBase<IIpCountriesData, IElementPropertyMetaData>,
                IIpCountriesData> elementDataFactory)
            : base(logger, elementDataFactory)
        {
            _logger = logger;
            _allCountryCodes = allCountryCodes;
        }

        /// <inheritdoc/>
        protected override void ProcessInternal(IFlowData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (_allCountryCodes == null || _allCountryCodes.Count == 0)
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

            // Build the flat lists
            var geoAll = BuildAllList(ipData, "CountryCodesGeographical");
            var popAll = BuildAllList(ipData, "CountryCodesPopulation");

            // Write to this element's own data object
            var elementData = (IpCountriesData)data.GetOrAdd(
                ElementDataKeyTyped,
                CreateElementData);
            elementData.SetCountryCodesGeographicalAll(geoAll);
            elementData.SetCountryCodesPopulationAll(popAll);
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
