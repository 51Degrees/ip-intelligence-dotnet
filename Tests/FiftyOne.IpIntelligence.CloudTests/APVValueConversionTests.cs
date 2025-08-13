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
    }
}
