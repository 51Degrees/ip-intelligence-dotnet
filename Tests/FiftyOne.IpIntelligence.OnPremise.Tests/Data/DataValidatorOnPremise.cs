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

using FiftyOne.IpIntelligence.Engine.OnPremise.Data;
using FiftyOne.IpIntelligence.Engine.OnPremise.FlowElements;
using FiftyOne.IpIntelligence.TestHelpers.Data;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Engines.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FiftyOne.IpIntelligence.OnPremise.Tests.Data
{
    public class DataValidatorOnPremise : IDataValidator
    {
        private IpiOnPremiseEngine _engine;

        public DataValidatorOnPremise(IpiOnPremiseEngine engine)
        {
            _engine = engine;
        }

        public void ValidateData(IFlowData data, bool validEvidence = true)
        {
            var elementData = data.GetFromElement(_engine);
            var dict = elementData.AsDictionary();

            foreach (var property in _engine.Properties
                .Where(p => p.Available))
            {
                Assert.IsTrue(dict.ContainsKey(property.Name));
                IAspectPropertyValue value = dict[property.Name] as IAspectPropertyValue;
                if (validEvidence)
                {
                    if (!value.HasValue) {
                        Assert.IsTrue(
                            value.NoValueMessage.Contains(
                                "The results contained a null profile"));
                    }
                }
                else
                {
                    if (property.Category.Equals("IP Metrics"))
                    {
                        Assert.IsTrue(value.HasValue);
                    }
                    else
                    {
                        Assert.IsFalse(value.HasValue);
                    }
                }
            }

            // TODO: Ask Ben for validation routines
            //Assert.IsTrue(string.IsNullOrEmpty(elementData.NetworkId.Value) == false);
            //if (validEvidence == false)
            //{
            //    Regex regex = new Regex("^(0:1.0+)(\\|(0:1.0+))*");
            //    Assert.IsTrue(regex.IsMatch(elementData.NetworkId.Value));
            //}
        }
    }
}
