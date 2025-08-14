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

using FiftyOne.Common.TestHelpers;
using FiftyOne.IpIntelligence.Cloud.Data;
using FiftyOne.IpIntelligence.Cloud.FlowElements;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines.Data;
using FiftyOne.Pipeline.Engines.FlowElements;
using FiftyOne.Pipeline.Engines.Services;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Engines.TestHelpers;
using FiftyOne.Pipeline.CloudRequestEngine.Data;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace FiftyOne.IpIntelligence.Cloud.Tests
{
    [TestClass]
    public class APVValueConversionTests
    {
        [TestMethod]
        public void TestToValueForAPV_String()
        {
            var rawValue = "someValue-137";
            var apvValue = IpiCloudEngine.ToValueForAPV(rawValue, typeof(string));
            Assert.AreEqual(typeof(string), apvValue.GetType());
            Assert.AreEqual(rawValue, apvValue);
        }

        [TestMethod]
        public void TestToValueForAPV_Int()
        {
            var rawValue = 129;
            var apvValue = IpiCloudEngine.ToValueForAPV(rawValue, typeof(int));
            Assert.AreEqual(typeof(int), apvValue.GetType());
            Assert.AreEqual(rawValue, apvValue);
        }

        [TestMethod]
        public void TestToValueForAPV_DoubleToFloat()
        {
            var rawValue = 7.125;
            var apvValue = IpiCloudEngine.ToValueForAPV(rawValue, typeof(float));
            Assert.AreEqual(typeof(float), apvValue.GetType());
            Assert.AreEqual(rawValue, (float)apvValue);
        }

        [TestMethod]
        public void TestToValueForAPV_IPAddress()
        {
            var rawValue = "8.49.13.251";
            var apvValue = IpiCloudEngine.ToValueForAPV(rawValue, typeof(IPAddress));
            Assert.AreEqual(typeof(IPAddress), apvValue.GetType());
            Assert.AreEqual(rawValue, apvValue.ToString());
        }

        [TestMethod]
        public void TestToValueForAPV_StringList()
        {
            var rawValue = new List<string>
            {
                "alpha",
                "gamma",
                "omega",
            };
            var apvValue = IpiCloudEngine.ToValueForAPV(rawValue, typeof(IReadOnlyList<string>));
            var apvList = (IReadOnlyList<string>) apvValue;
            Assert.IsNotNull(apvList);
            Assert.AreEqual(rawValue.Count, apvList.Count);
            for (int i = 0; i < rawValue.Count; i++)
            {
                Assert.AreEqual(rawValue[i], apvList[i]);
            }
        }

        [TestMethod]
        public void TestToValueForAPV_FloatList()
        {
            var rawValue = new List<float>
            {
                1.0f,
                -0.875f,
                128758.5075f,
            };
            var apvValue = IpiCloudEngine.ToValueForAPV(rawValue, typeof(IReadOnlyList<float>));
            var apvList = (IReadOnlyList<float>) apvValue;
            Assert.IsNotNull(apvList);
            Assert.AreEqual(rawValue.Count, apvList.Count);
            for(int i = 0; i < rawValue.Count; i++)
            {
                Assert.AreEqual(rawValue[i], apvList[i]);
            }
        }

        [TestMethod]
        public void TestToValueForAPV_IPList()
        {
            var rawValue = new List<string>
            {
                "8.65.149.1",
                "3.2.96.7",
                "187.214.38.111",
            };
            var apvValue = IpiCloudEngine.ToValueForAPV(rawValue, typeof(IReadOnlyList<IPAddress>));
            var apvList = (IReadOnlyList<IPAddress>)apvValue;
            Assert.IsNotNull(apvList);
            Assert.AreEqual(rawValue.Count, apvList.Count);
            for (int i = 0; i < rawValue.Count; i++)
            {
                Assert.AreEqual(rawValue[i], apvList[i].ToString());
            }
        }

        [TestMethod]
        public void TestToValueForAPV_WeightedString()
        {
            var rawValue = new Dictionary<string, object>
            {
                { "rawweighting", 2645 },
                { "value", "someValue-137" },
            };
            var apvValue = IpiCloudEngine.ToValueForAPV(rawValue, typeof(IWeightedValue<string>));
            var typedApvValue = apvValue as IWeightedValue<string>;
            Assert.IsNotNull(typedApvValue);
            Assert.AreEqual((ushort)(int)rawValue["rawweighting"], typedApvValue.RawWeighting);
            Assert.AreEqual(rawValue["value"], typedApvValue.Value);
        }

        [TestMethod]
        public void TestToValueForAPV_WeightedInt()
        {
            var rawValue = new Dictionary<string, object>
            {
                { "rawweighting", 1234 },
                { "value", 129 },
            };
            var apvValue = IpiCloudEngine.ToValueForAPV(rawValue, typeof(IWeightedValue<int>));
            var typedApvValue = apvValue as IWeightedValue<int>;
            Assert.IsNotNull(typedApvValue);
            Assert.AreEqual((ushort)(int)rawValue["rawweighting"], typedApvValue.RawWeighting);
            Assert.AreEqual(rawValue["value"], typedApvValue.Value);
        }

        [TestMethod]
        public void TestToValueForAPV_WeightedDoubleToFloat()
        {
            var rawValue = new Dictionary<string, object>
            {
                { "rawweighting", 5678 },
                { "value", 7.125 },
            };
            var apvValue = IpiCloudEngine.ToValueForAPV(rawValue, typeof(IWeightedValue<float>));
            var typedApvValue = apvValue as IWeightedValue<float>;
            Assert.IsNotNull(typedApvValue);
            Assert.AreEqual((ushort)(int)rawValue["rawweighting"], typedApvValue.RawWeighting);
            Assert.AreEqual((float)(double)rawValue["value"], typedApvValue.Value);
        }

        [TestMethod]
        public void TestToValueForAPV_WeightedIPAddress()
        {
            var rawValue = new Dictionary<string, object>
            {
                { "rawweighting", 9012 },
                { "value", "8.49.13.251" },
            };
            var apvValue = IpiCloudEngine.ToValueForAPV(rawValue, typeof(IWeightedValue<IPAddress>));
            var typedApvValue = apvValue as IWeightedValue<IPAddress>;
            Assert.IsNotNull(typedApvValue);
            Assert.AreEqual((ushort)(int)rawValue["rawweighting"], typedApvValue.RawWeighting);
            Assert.AreEqual(rawValue["value"].ToString(), typedApvValue.Value.ToString());
        }

        [TestMethod]
        public void TestToValueForAPV_WeightedStringList()
        {
            var stringList = new List<string>
            {
                "alpha",
                "gamma",
                "omega",
            };
            var rawValue = new Dictionary<string, object>
            {
                { "rawweighting", 3456 },
                { "value", stringList },
            };
            var apvValue = IpiCloudEngine.ToValueForAPV(rawValue, typeof(IWeightedValue<IReadOnlyList<string>>));
            var typedApvValue = apvValue as IWeightedValue<IReadOnlyList<string>>;
            Assert.IsNotNull(typedApvValue);
            Assert.AreEqual((ushort)(int)rawValue["rawweighting"], typedApvValue.RawWeighting);
            
            var valueList = typedApvValue.Value;
            Assert.IsNotNull(valueList);
            Assert.AreEqual(stringList.Count, valueList.Count);
            for (int i = 0; i < stringList.Count; i++)
            {
                Assert.AreEqual(stringList[i], valueList[i]);
            }
        }

        [TestMethod]
        public void TestToValueForAPV_WeightedFloatList()
        {
            var floatList = new List<float>
            {
                1.0f,
                -0.875f,
                128758.5075f,
            };
            var rawValue = new Dictionary<string, object>
            {
                { "rawweighting", 7890 },
                { "value", floatList },
            };
            var apvValue = IpiCloudEngine.ToValueForAPV(rawValue, typeof(IWeightedValue<IReadOnlyList<float>>));
            var typedApvValue = apvValue as IWeightedValue<IReadOnlyList<float>>;
            Assert.IsNotNull(typedApvValue);
            Assert.AreEqual((ushort)(int)rawValue["rawweighting"], typedApvValue.RawWeighting);
            
            var valueList = typedApvValue.Value;
            Assert.IsNotNull(valueList);
            Assert.AreEqual(floatList.Count, valueList.Count);
            for (int i = 0; i < floatList.Count; i++)
            {
                Assert.AreEqual(floatList[i], valueList[i]);
            }
        }

        [TestMethod]
        public void TestToValueForAPV_WeightedIPList()
        {
            var ipStringList = new List<string>
            {
                "8.65.149.1",
                "3.2.96.7",
                "187.214.38.111",
            };
            var rawValue = new Dictionary<string, object>
            {
                { "rawweighting", 4321 },
                { "value", ipStringList },
            };
            var apvValue = IpiCloudEngine.ToValueForAPV(rawValue, typeof(IWeightedValue<IReadOnlyList<IPAddress>>));
            var typedApvValue = apvValue as IWeightedValue<IReadOnlyList<IPAddress>>;
            Assert.IsNotNull(typedApvValue);
            Assert.AreEqual((ushort)(int)rawValue["rawweighting"], typedApvValue.RawWeighting);
            
            var valueList = typedApvValue.Value;
            Assert.IsNotNull(valueList);
            Assert.AreEqual(ipStringList.Count, valueList.Count);
            for (int i = 0; i < ipStringList.Count; i++)
            {
                Assert.AreEqual(ipStringList[i], valueList[i].ToString());
            }
        }

        [TestMethod]
        public void TestToValueForAPV_ListOfWeightedStrings()
        {
            var weightedStringsList = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "rawweighting", 1111 },
                    { "value", "alpha" },
                },
                new Dictionary<string, object>
                {
                    { "rawweighting", 2222 },
                    { "value", "gamma" },
                },
                new Dictionary<string, object>
                {
                    { "rawweighting", 3333 },
                    { "value", "omega" },
                },
            };
            
            var apvValue = IpiCloudEngine.ToValueForAPV(weightedStringsList, typeof(IReadOnlyList<IWeightedValue<string>>));
            var apvList = (IReadOnlyList<IWeightedValue<string>>)apvValue;
            
            Assert.IsNotNull(apvList);
            Assert.AreEqual(weightedStringsList.Count, apvList.Count);
            
            for (int i = 0; i < weightedStringsList.Count; i++)
            {
                var expectedWeighting = (ushort)(int)weightedStringsList[i]["rawweighting"];
                var expectedValue = weightedStringsList[i]["value"].ToString();
                
                Assert.AreEqual(expectedWeighting, apvList[i].RawWeighting);
                Assert.AreEqual(expectedValue, apvList[i].Value);
            }
        }

        [TestMethod]
        public void TestToValueForAPV_ListOfWeightedFloats()
        {
            var weightedFloatsList = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "rawweighting", 4444 },
                    { "value", 1.0 },
                },
                new Dictionary<string, object>
                {
                    { "rawweighting", 5555 },
                    { "value", -0.875 },
                },
                new Dictionary<string, object>
                {
                    { "rawweighting", 6666 },
                    { "value", 128758.5075 },
                },
            };
            
            var apvValue = IpiCloudEngine.ToValueForAPV(weightedFloatsList, typeof(IReadOnlyList<IWeightedValue<float>>));
            var apvList = (IReadOnlyList<IWeightedValue<float>>)apvValue;
            
            Assert.IsNotNull(apvList);
            Assert.AreEqual(weightedFloatsList.Count, apvList.Count);
            
            for (int i = 0; i < weightedFloatsList.Count; i++)
            {
                var expectedWeighting = (ushort)(int)weightedFloatsList[i]["rawweighting"];
                var expectedValue = (float)(double)weightedFloatsList[i]["value"];
                
                Assert.AreEqual(expectedWeighting, apvList[i].RawWeighting);
                Assert.AreEqual(expectedValue, apvList[i].Value);
            }
        }

        [TestMethod]
        public void TestToValueForAPV_ListOfWeightedIPAddresses()
        {
            var weightedIPList = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "rawweighting", 7777 },
                    { "value", "8.65.149.1" },
                },
                new Dictionary<string, object>
                {
                    { "rawweighting", 8888 },
                    { "value", "3.2.96.7" },
                },
                new Dictionary<string, object>
                {
                    { "rawweighting", 9999 },
                    { "value", "187.214.38.111" },
                },
            };
            
            var apvValue = IpiCloudEngine.ToValueForAPV(weightedIPList, typeof(IReadOnlyList<IWeightedValue<IPAddress>>));
            var apvList = (IReadOnlyList<IWeightedValue<IPAddress>>)apvValue;
            
            Assert.IsNotNull(apvList);
            Assert.AreEqual(weightedIPList.Count, apvList.Count);
            
            for (int i = 0; i < weightedIPList.Count; i++)
            {
                var expectedWeighting = (ushort)(int)weightedIPList[i]["rawweighting"];
                var expectedValue = weightedIPList[i]["value"].ToString();
                
                Assert.AreEqual(expectedWeighting, apvList[i].RawWeighting);
                Assert.AreEqual(expectedValue, apvList[i].Value.ToString());
            }
        }
    }
}
