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

using FiftyOne.IpIntelligence.Engine.OnPremise.FlowElements;
using FiftyOne.IpIntelligence.Engine.OnPremise.Interop;
using FiftyOne.IpIntelligence.Engine.OnPremise.Wrappers;
using FiftyOne.IpIntelligence.Shared;
using FiftyOne.IpIntelligence.Shared.Data;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.Data.Types;
using FiftyOne.Pipeline.Core.Exceptions;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines.Data;
using FiftyOne.Pipeline.Engines.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace FiftyOne.IpIntelligence.Engine.OnPremise.Data
{
    /// <summary>
    /// Class used for on-premise results
    /// </summary>
    internal class IpDataOnPremise : IpDataBaseOnPremise<ResultsIpiSwig>, IIpDataOnPremise
    {
        #region Constructor

        /// <summary>
        /// Construct a new instance of the wrapper.
        /// </summary>
        /// <param name="logger">
        /// The logger instance to use.
        /// </param>
        /// <param name="pipeline">
        /// The Pipeline that created this data instance.
        /// </param>
        /// <param name="engine">
        /// The engine that create this data instance.
        /// </param>
        /// <param name="missingPropertyService">
        /// The <see cref="IMissingPropertyService"/> to use if a requested
        /// property does not exist.
        /// </param>
        internal IpDataOnPremise(
            ILogger<AspectDataBase> logger,
            IPipeline pipeline,
            IpiOnPremiseEngine engine,
            IMissingPropertyService missingPropertyService)
            : base(logger, pipeline, engine, missingPropertyService)
        {
        }

        #endregion

        #region Internal Methods
        internal void SetResults(ResultsIpiSwig results)
        {
            Results.AddResult(results);
        }
        #endregion

        #region Private Methods

        private ResultsIpiSwig GetResultsContainingProperty(string propertyName)
        {
            foreach (var results in Results.ResultsList)
            {
                if (results.containsProperty(propertyName))
                {
                    return results;
                }
            }
            return null;
        }

        #endregion

        #region Protected Methods

        protected override bool PropertyIsAvailable(string propertyName)
        {
            return Results.ResultsList
                .Any(r => r.containsProperty(propertyName));
        }

        protected override IAspectPropertyValue<IReadOnlyList<IWeightedValue<bool>>> GetValuesAsWeightedBoolList(string propertyName)
        {
            var result = new AspectPropertyValue<IReadOnlyList<IWeightedValue<bool>>>();
            var results = GetResultsContainingProperty(propertyName);

            if (results != null)
            {
                using (var value = results.getValuesAsWeightedBoolList(propertyName))
                {
                    if (value.hasValue())
                    {
                        result.Value = new WeightedBoolListSwigWrapper(value.getValue());
                    }
                    else
                    {
                        result.NoValueMessage = value.getNoValueMessage();
                    }
                }
            }
            return result;
        }

        protected override IAspectPropertyValue<IReadOnlyList<IWeightedValue<double>>> GetValuesAsWeightedDoubleList(string propertyName)
        {
            var result = new AspectPropertyValue<IReadOnlyList<IWeightedValue<double>>>();
            var results = GetResultsContainingProperty(propertyName);

            if (results != null)
            {
                using (var value = results.getValuesAsWeightedDoubleList(propertyName))
                {
                    if (value.hasValue())
                    {
                        result.Value = new WeightedDoubleListSwigWrapper(value.getValue());
                    }
                    else
                    {
                        result.NoValueMessage = value.getNoValueMessage();
                    }
                }
            }
            return result;
        }

        protected override IAspectPropertyValue<IReadOnlyList<IWeightedValue<float>>> GetValuesAsWeightedFloatList(string propertyName)
        {
            var result = new AspectPropertyValue<IReadOnlyList<IWeightedValue<float>>>();
            var results = GetResultsContainingProperty(propertyName);

            if (results != null)
            {
                using (var value = results.getValuesAsWeightedDoubleList(propertyName))
                {
                    if (value.hasValue())
                    {
                        result.Value = new List<IWeightedValue<float>>(new WeightedDoubleListSwigWrapper(value.getValue())
                            .Select(x => new WeightedValue<float>(x.RawWeighting, (float)x.Value)));
                    }
                    else
                    {
                        result.NoValueMessage = value.getNoValueMessage();
                    }
                }
            }
            return result;
        }

        protected override IAspectPropertyValue<IReadOnlyList<IWeightedValue<int>>> GetValuesAsWeightedIntegerList(string propertyName)
        {
            var result = new AspectPropertyValue<IReadOnlyList<IWeightedValue<int>>>();
            var results = GetResultsContainingProperty(propertyName);

            if (results != null)
            {
                using (var value = results.getValuesAsWeightedIntegerList(propertyName))
                {
                    if (value.hasValue())
                    {
                        result.Value = new WeightedIntListSwigWrapper(value.getValue());
                    }
                    else
                    {
                        result.NoValueMessage = value.getNoValueMessage();
                    }
                }
            }
            return result;
        }

        public override IAspectPropertyValue<IReadOnlyList<string>> GetValues(string propertyName)
        {
            var result = new AspectPropertyValue<IReadOnlyList<string>>();
            var results = GetResultsContainingProperty(propertyName);

            if (results != null)
            {
                using (var value = results.getValues(propertyName))
                {
                    if (value.hasValue())
                    {
                        using (var vector = value.getValue())
                        {
                            result.Value = vector.ToList();
                        }
                    }
                    else
                    {
                        result.NoValueMessage = value.getNoValueMessage();
                    }
                }
            }
            return result;
        }

        protected override IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> GetValuesAsWeightedStringList(string propertyName)
        {
            var result = new AspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>();
            var results = GetResultsContainingProperty(propertyName);

            if (results != null)
            {
                using (var value = results.getValuesAsWeightedUTF8StringList(propertyName))
                {
                    if (value.hasValue())
                    {
                        result.Value = new WeightedUTF8StringListSwigWrapper(value.getValue());
                    }
                    else
                    {
                        result.NoValueMessage = value.getNoValueMessage();
                    }
                }
            }
            return result;
        }

        protected override IAspectPropertyValue<IReadOnlyList<IWeightedValue<IPAddress>>> GetValuesAsWeightedIPList(string propertyName)
        {
            var result = new AspectPropertyValue<IReadOnlyList<IWeightedValue<IPAddress>>>();
            var results = GetResultsContainingProperty(propertyName);

            if (results != null)
            {
                using (var value = results.getValuesAsWeightedStringList(propertyName))
                {
                    if (value.hasValue())
                    {
                        result.Value = new List<IWeightedValue<IPAddress>>(new WeightedStringListSwigWrapper(value.getValue())
                            .Select(x => new WeightedValue<IPAddress>(x.RawWeighting, IPAddress.Parse(x.Value))));
                    }
                    else
                    {
                        result.NoValueMessage = value.getNoValueMessage();
                    }
                }
            }
            return result;
        }

        protected override IAspectPropertyValue<IReadOnlyList<IWeightedValue<string>>> GetValuesAsWeightedWKTStringList(
            string propertyName, byte decimalPlaces)
        {
            var result = new AspectPropertyValue<IReadOnlyList<IWeightedValue<string>>>();
            var results = GetResultsContainingProperty(propertyName);

            if (results != null)
            {
                using (var value = results.getValuesAsWeightedWKTStringList(propertyName, decimalPlaces))
                {
                    if (value.hasValue())
                    {
                        result.Value = new WeightedStringListSwigWrapper(value.getValue());
                    }
                    else
                    {
                        result.NoValueMessage = value.getNoValueMessage();
                    }
                }
            }
            return result;
        }

        #endregion
    }
}
