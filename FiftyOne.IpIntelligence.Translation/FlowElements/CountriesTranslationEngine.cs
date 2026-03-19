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

namespace FiftyOne.IpIntelligence.Translation.FlowElements
{
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
        private readonly IReadOnlyList<KeyValuePair<string, string>>
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
            IReadOnlyList<KeyValuePair<string, string>> allCountries,
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

            var elementData = data.GetOrAdd(
                ElementDataKeyTyped,
                CreateElementData);

            // Resolve the translator and locale for the "All" lists.
            var (translator, locale) = ResolveTranslator(data);
            var comparer = CreateComparer(locale);

            // Get weighted codes from IP engine.
            IElementData ipData = null;
            try
            {
                ipData = data.Get("ip");
            }
            catch (KeyNotFoundException) { }

            var geoCodesWeighted = GetPropertyValue
                <IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>(
                    ipData, "CountryCodesGeographical");
            var popCodesWeighted = GetPropertyValue
                <IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>(
                    ipData, "CountryCodesPopulation");

            // Get the weighted names (translated or pass-through English).
            var geoTranslated = GetPropertyValue
                <IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>(
                    elementData,
                    nameof(ICountriesTranslationData
                        .CountryNamesGeographicalTranslated));
            var popTranslated = GetPropertyValue
                <IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>>(
                    elementData,
                    nameof(ICountriesTranslationData
                        .CountryNamesPopulationTranslated));

            BuildAndStoreAllLists(
                elementData, geoTranslated, geoCodesWeighted,
                translator, comparer,
                nameof(ICountriesTranslationData
                    .CountryNamesGeographicalAllTranslated),
                nameof(ICountriesTranslationData
                    .CountryCodesGeographicalAll));

            BuildAndStoreAllLists(
                elementData, popTranslated, popCodesWeighted,
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
            IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>
                translatedWeightedNames,
            IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>
                weightedCodes,
            Translator translator,
            StringComparer comparer,
            string namesPropertyName,
            string codesPropertyName)
        {
            var weightedTuples = BuildWeightedTuples(
                translatedWeightedNames, weightedCodes);

            var errors = new List<Exception>();
            var allTuples = _allCountries
                .Select(kv => (
                    Name: translator != null
                        ? (string)translator.Translate(kv.Value, errors)
                        : kv.Value,
                    Code: kv.Key))
                .ToList();

            var weightedCodeSet = new HashSet<string>(
                weightedTuples.Select(t => t.Code));
            var remainingTuples = allTuples
                .Where(t => !weightedCodeSet.Contains(t.Code))
                .ToList();

            remainingTuples.Sort((a, b) => comparer.Compare(a.Name, b.Name));

            var combined = weightedTuples.Concat(remainingTuples).ToList();

            elementData[namesPropertyName] =
                new AspectPropertyValue<IReadOnlyList<string>>(
                    combined.Select(t => t.Name).ToList());
            elementData[codesPropertyName] =
                new AspectPropertyValue<IReadOnlyList<string>>(
                    combined.Select(t => t.Code).ToList());
        }

        private static List<(string Name, string Code)> BuildWeightedTuples(
            IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>
                translatedNames,
            IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>
                codes)
        {
            if (translatedNames?.HasValue != true ||
                codes?.HasValue != true)
            {
                return new List<(string, string)>();
            }

            var orderedNames = translatedNames.Value
                .OrderByDescending(w => w.RawWeighting).ToList();
            var orderedCodes = codes.Value
                .OrderByDescending(w => w.RawWeighting).ToList();

            var count = Math.Min(orderedNames.Count, orderedCodes.Count);
            var result = new List<(string Name, string Code)>(count);
            for (int i = 0; i < count; i++)
            {
                result.Add((orderedNames[i].Value, orderedCodes[i].Value));
            }
            return result;
        }

        private static T GetPropertyValue<T>(
            IElementData elementData, string propertyName)
        {
            if (elementData == null) return default;
            try
            {
                if (elementData[propertyName] is T typed) return typed;
            }
            catch (KeyNotFoundException) { }
            return default;
        }

        /// <summary>
        /// Resolves the target language from evidence using the base class's
        /// <see cref="TranslationEngineBase{T}.Languages"/> collection.
        /// English is treated as the base language (no translation needed).
        /// </summary>
        private (Translator Translator, string Locale) ResolveTranslator(
            IFlowData data)
        {
            var evidence = data.GetEvidence().AsDictionary();

            foreach (var key in EvidenceKeys)
            {
                if (evidence.TryGetValue(key, out var value) &&
                    value is string headerValue &&
                    !string.IsNullOrWhiteSpace(headerValue))
                {
                    if (Languages.TryGetTranslator(
                        headerValue,
                        out var translator,
                        out var locale))
                    {
                        return (translator, locale);
                    }

                    // If TryGetTranslator returned false, it means
                    // either English was preferred (base language) or
                    // nothing matched. Either way, no translation.
                    return (null, "en_GB");
                }
            }

            return (null, "en_GB");
        }

        private static StringComparer CreateComparer(string locale)
        {
            if (locale == null) return StringComparer.CurrentCulture;
            try
            {
                var culture = CultureInfo.GetCultureInfo(
                    locale.Replace('_', '-'));
                return StringComparer.Create(culture, ignoreCase: false);
            }
            catch (CultureNotFoundException)
            {
                return StringComparer.CurrentCulture;
            }
        }
    }
}
