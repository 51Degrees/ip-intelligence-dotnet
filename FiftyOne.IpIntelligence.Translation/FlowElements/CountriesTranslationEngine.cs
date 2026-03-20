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

using FiftyOne.IpIntelligence.Translation.Data;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines.Data;
using FiftyOne.Pipeline.Translation.Data;
using FiftyOne.Pipeline.Translation.FlowElements;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FiftyOne.Pipeline.Core.Exceptions;

namespace FiftyOne.IpIntelligence.Translation.FlowElements
{
    using CountryCodeNamePair = KeyValuePair<string, string>;
    
    /// <summary>
    /// Engine which takes country name properties from
    /// <see cref="CountryCodeTranslationEngine"/> and country code properties
    /// from the IP Intelligence engine, translates country names to the
    /// browser language, and produces complete ordered lists of all countries
    /// combining the weighted results with all known countries.
    ///
    /// The base class handles the weighted translations
    /// (CountryNamesGeographical/Population to their Translated variants).
    /// This override adds the "All" list logic on top.
    ///
    /// Results are stored using the
    /// <see cref="Constants.CountryNamesTranslatedKey"/> key.
    /// </summary>
    public class CountriesTranslationEngine
        : TranslationEngineBase<ICountriesTranslationData>
    {
        private static readonly string[] EvidenceKeys = new[]
        {
            "query.translation",
            "query.accept-language",
            "header.accept-language"
        };

        /// <summary>
        /// Static translations passed to the base constructor for the
        /// weighted properties.
        /// </summary>
        private static readonly IReadOnlyCollection<TranslationProperty>
            _translations = new List<TranslationProperty>
            {
                new TranslationProperty(
                    nameof(ICountryCodeTranslationData.CountryNamesGeographical),
                    nameof(ICountriesTranslationData.CountryNamesGeographicalTranslated)),
                new TranslationProperty(
                    nameof(ICountryCodeTranslationData.CountryNamesPopulation),
                    nameof(ICountriesTranslationData.CountryNamesPopulationTranslated))
            };

        /// <summary>
        /// All known country codes and their English names, ordered as they
        /// appear in countrycodes.en_GB.yml.
        /// </summary>
        private readonly IReadOnlyList<CountryCodeNamePair>
            _allCountries;

        /// <inheritdoc/>
        public override string ElementDataKey =>
            Constants.CountryNamesTranslatedKey;

        /// <summary>
        /// Constructor which is only accessible by the builder.
        /// </summary>
        internal CountriesTranslationEngine(
            ILogger<FlowElementBase<
                ICountriesTranslationData,
                IElementPropertyMetaData>> logger,
            IReadOnlyList<CountryCodeNamePair> allCountries,
            Func<
                IPipeline,
                FlowElementBase<
                    ICountriesTranslationData,
                    IElementPropertyMetaData>,
                ICountriesTranslationData> elementDataFactory)
            : base(
                  Constants.CountryNamesKey,
                  _translations,
                  Resources.Resources.GetCountryResources(),
                  null,
                  MissingTranslationBehavior.Original,
                  logger,
                  elementDataFactory)
        {
            _allCountries = allCountries
                ?? throw new ArgumentNullException(nameof(allCountries));
        }

        /// <inheritdoc/>
        protected override void ProcessInternal(IFlowData data)
        {
            // Let the base class handle the weighted translations.
            // The base class uses Languages.TryResolveLocale which
            // recognises English as the base language and uses the
            // empty translator (pass-through) when appropriate.
            base.ProcessInternal(data);

            // Data is already created by base class.
            var elementData = data.Get(ElementDataKeyTyped);

            string cultureUsed = string.Empty;
            // Resolve the translator and locale for the "All" lists.
            var comparer = TryResolveTranslator(data, out var translator, out var locale)
                ? CreateComparer(locale, out cultureUsed)
                : StringComparer.InvariantCultureIgnoreCase;

            elementData[nameof(CountriesTranslationData.SortingCultureUsed)] = cultureUsed;

            // Get weighted codes from IP engine.
            IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> geoCodesWeighted = null;
            IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> popCodesWeighted = null;
            IIpIntelligenceData ipData = null;
            try
            {
                ipData = data.Get<IIpIntelligenceData>();
                geoCodesWeighted = ipData?.CountryCodesGeographical;
                popCodesWeighted = ipData?.CountryCodesPopulation;
            }
            catch (KeyNotFoundException) { }
            catch (PipelineException) { }

            // Get the weighted names (translated or pass-through English).
            var geoTranslated = elementData.CountryNamesGeographicalTranslated;
            var popTranslated = elementData.CountryNamesPopulationTranslated;

            BuildAndStoreAllLists(
                elementData,
                geoTranslated.HasValue 
                    ? geoTranslated.Value
                    : Array.Empty<IWeightedValue<string>>(),
                geoCodesWeighted.HasValue 
                    ? geoCodesWeighted.Value
                    : Array.Empty<IWeightedValue<string>>(),
                translator, comparer,
                nameof(ICountriesTranslationData
                    .CountryNamesGeographicalAllTranslated),
                nameof(ICountriesTranslationData
                    .CountryCodesGeographicalAll));

            BuildAndStoreAllLists(
                elementData,
                popTranslated.HasValue 
                    ? popTranslated.Value
                    : Array.Empty<IWeightedValue<string>>(),
                popCodesWeighted.HasValue 
                    ? popCodesWeighted.Value
                    : Array.Empty<IWeightedValue<string>>(),
                translator, comparer,
                nameof(ICountriesTranslationData
                    .CountryNamesPopulationAllTranslated),
                nameof(ICountriesTranslationData
                    .CountryCodesPopulationAll));
        }

        /// <summary>
        /// Builds the complete "All" lists for one dimension (geographical
        /// or population) and stores them in the element data.
        /// </summary>
        private void BuildAndStoreAllLists(
            ICountriesTranslationData elementData,
            IReadOnlyList<IWeightedValue<string>> translatedWeightedNames,
            IReadOnlyList<IWeightedValue<string>> weightedCodes,
            Translator translator,
            StringComparer comparer,
            string namesPropertyName,
            string codesPropertyName)
        {
            var weightedTuples = BuildWeightedTuples(
                translatedWeightedNames,
                weightedCodes,
                comparer).ToList();

            var errors = new List<Exception>();
            var remainingPairs = _allCountries
                .Where(nextPair =>
                    weightedTuples.Any(weightedPair => 
                        weightedPair.Key == nextPair.Key)
                    == false);
            if (translator is null == false)
            {
                remainingPairs = remainingPairs
                    .Select(nextPair => new CountryCodeNamePair(
                        nextPair.Key,
                        translator.Translate(nextPair.Value, errors) as string
                        ?? nextPair.Value));
            }
            var remainingPairsSorted = remainingPairs.OrderBy(
                x => x.Value,
                comparer);

            var allTuples = weightedTuples.Concat(remainingPairsSorted).ToList();

            elementData[codesPropertyName] =
                new AspectPropertyValue<IReadOnlyList<string>>(
                    allTuples.Select(t => t.Key).ToList());
            elementData[namesPropertyName] =
                new AspectPropertyValue<IReadOnlyList<string>>(
                    allTuples.Select(t => t.Value).ToList());
        }

        private static IEnumerable<CountryCodeNamePair> BuildWeightedTuples(
            IReadOnlyList<IWeightedValue<string>> translatedNames,
            IReadOnlyList<IWeightedValue<string>> codes,
            StringComparer comparer)
        {
            if (translatedNames is null || codes is null)
            {
                return Enumerable.Empty<CountryCodeNamePair>();
            }
            var itemsToTake = Math.Min(translatedNames.Count, codes.Count);
            return translatedNames
                .Take(itemsToTake)
                .Select((weightedName, i) => (weightedName, weightedCode: codes[i]))
                .OrderByDescending(x => x.weightedName.RawWeighting)
                .ThenBy(p => p.weightedName.Value, comparer)
                .Select(p => new CountryCodeNamePair(
                    p.weightedCode.Value,
                    p.weightedName.Value));
        }

        /// <summary>
        /// Resolves the target language from evidence using the base class's
        /// <see cref="TranslationEngineBase{T}.Languages"/> collection.
        /// English is treated as the base language (no translation needed).
        /// </summary>
        private bool TryResolveTranslator(
            IFlowData data,
            out Translator translator,
            out string locale)
        {
            var evidence = data.GetEvidence().AsDictionary();
            translator = null;
            locale = "en_GB";

            foreach (var key in EvidenceKeys)
            {
                if (evidence.TryGetValue(key, out var value) &&
                    value is string headerValue &&
                    !string.IsNullOrWhiteSpace(headerValue))
                {
                    if (Languages.TryGetTranslator(
                        headerValue,
                        out translator,
                        out locale))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static StringComparer CreateComparer(string locale, out string cultureUsed)
        {
            cultureUsed = string.Empty;
            if (locale == null) return StringComparer.CurrentCulture;
            try
            {
                cultureUsed = locale.Replace('_', '-');
                var culture = CultureInfo.GetCultureInfo(cultureUsed);
                return StringComparer.Create(culture, ignoreCase: false);
            }
            catch (CultureNotFoundException)
            {
                return StringComparer.CurrentCulture;
            }
        }
    }
}
