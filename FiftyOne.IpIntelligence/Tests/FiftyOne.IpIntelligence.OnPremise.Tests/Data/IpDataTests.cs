/* *********************************************************************
 * This Original Work is copyright of 51 Degrees Mobile Experts Limited.
 * Copyright 2019 51 Degrees Mobile Experts Limited, 5 Charlotte Close,
 * Caversham, Reading, Berkshire, United Kingdom RG4 7BY.
 *
 * This Original Work is licensed under the European Union Public Licence (EUPL) 
 * v.1.2 and is subject to its terms as set out below.
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

using FiftyOne.IpIntelligence.Shared.Data;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.Data.Types;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines;
using FiftyOne.Pipeline.Engines.Data;
using FiftyOne.Pipeline.Engines.FlowElements;
using FiftyOne.Pipeline.Engines.Services;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;

namespace FiftyOne.IpIntelligence.Tests.Core.Data
{
    [TestClass]
    public class DeviceDataOnPremiseTests
    {
        private Mock<ILogger<AspectDataBase>> _logger;

        private Mock<IFlowData> _flowData;

        private Mock<IAspectEngine> _engine;

        private Mock<IMissingPropertyService> _missingPropertyService;

        private static string _testPropertyName = "testproperty";

        /// <summary>
        /// Test class to extend the results wrapper and add a single property.
        /// </summary>
        private class TestResults<T> : IpDataBaseOnPremise<IDisposable>
        {
            private object _value;

            internal TestResults(
                ILogger<AspectDataBase> logger,
                IPipeline pipeline,
                IAspectEngine engine,
                IMissingPropertyService missingPropertyService,
                object value)
                : base(logger, pipeline, engine, missingPropertyService)
            {
                _value = value;
                // The ResultManager needs to have something added to it 
                // in order to allow access to the values.
                // For this test, we can just give it a null reference.
                Results.AddResult(null);
            }

            protected override IAspectPropertyValue<IReadOnlyList<WeightedValue<bool>>> 
                GetValuesAsWeightedBoolList(string propertyName)
            {
                if (propertyName == _testPropertyName)
                {
                    return new AspectPropertyValue<IReadOnlyList<WeightedValue<bool>>>(
                        (IReadOnlyList<WeightedValue<bool>>)_value);
                }
                else
                {
                    throw new PropertyMissingException();
                }
            }

            protected override IAspectPropertyValue<IReadOnlyList<WeightedValue<double>>> 
                GetValuesAsWeightedDoubleList(string propertyName)
            {
                if (propertyName == _testPropertyName)
                {
                    return new AspectPropertyValue<IReadOnlyList<WeightedValue<double>>>(
                        (IReadOnlyList<WeightedValue<double>>)_value);
                }
                else
                {
                    throw new PropertyMissingException();
                }
            }

            protected override IAspectPropertyValue<IReadOnlyList<WeightedValue<int>>> 
                GetValuesAsWeightedIntegerList(string propertyName)
            {
                if (propertyName == _testPropertyName)
                {
                    return new AspectPropertyValue<IReadOnlyList<WeightedValue<int>>>(
                        (IReadOnlyList<WeightedValue<int>>)_value);
                }
                else
                {
                    throw new PropertyMissingException();
                }
            }

            protected override IAspectPropertyValue<IReadOnlyList<WeightedValue<string>>> 
                GetValuesAsWeightedStringList(string propertyName)
            {
                if (propertyName == _testPropertyName)
                {
                    return new AspectPropertyValue<IReadOnlyList<WeightedValue<string>>>(
                        (IReadOnlyList<WeightedValue<string>>)_value);
                }
                else
                {
                    throw new PropertyMissingException();
                }
            }

            protected override IAspectPropertyValue<Coordinate>
                GetValueAsCoordinate(string propertyName)
            {
                if (propertyName == _testPropertyName)
                {
                    return new AspectPropertyValue<Coordinate>(
                        (Coordinate)_value);
                }
                else
                {
                    throw new PropertyMissingException();
                }
            }

            protected override IAspectPropertyValue<IPAddress>
                GetValueAsIpAddress(string propertyName)
            {
                if (propertyName == _testPropertyName)
                {
                    return new AspectPropertyValue<IPAddress>(
                        (IPAddress)_value);
                }
                else
                {
                    throw new PropertyMissingException();
                }
            }

            public override IAspectPropertyValue<IReadOnlyList<string>> 
                GetValues(string propertyName)
            {
                if (propertyName == _testPropertyName)
                {
                    return new AspectPropertyValue<IReadOnlyList<string>>(
                        (IReadOnlyList<string>)_value);
                }
                else
                {
                    throw new PropertyMissingException();
                }
            }
            protected override bool PropertyIsAvailable(string propertyName)
            {
                return propertyName == _testPropertyName;
            }
        }

        private void SetupElementProperties(Type type)
        {
            var properties = new Dictionary<string, IElementPropertyMetaData>();
            var property = new ElementPropertyMetaData(
                _engine.Object,
                _testPropertyName,
                type,
                true,
                "category");
            properties.Add(_testPropertyName, property);
            var elementProperties = 
                new Dictionary<string, IReadOnlyDictionary<string, IElementPropertyMetaData>>();
            elementProperties.Add(_engine.Object.ElementDataKey, properties);
            _flowData.SetupGet(f => f.Pipeline.ElementAvailableProperties)
                .Returns(elementProperties);
        }

        /// <summary>
        /// Initialise the test instance.
        /// </summary>
        [TestInitialize]
        public void Init()
        {
            _logger = new Mock<ILogger<AspectDataBase>>();
            _missingPropertyService = new Mock<IMissingPropertyService>();
            _engine = new Mock<IAspectEngine>();
            _engine.SetupGet(e => e.ElementDataKey).Returns("test");
            _flowData = new Mock<IFlowData>();
            var pipeline = new Mock<IPipeline>();
            _flowData.Setup(f => f.Pipeline).Returns(pipeline.Object);
        }

        /// <summary>
        /// Check that a weighted string list is returned from the internal results instance
        /// using the correct get method.
        /// </summary>
        [TestMethod]
        public void GetWeightedStringList()
        {
            SetupElementProperties(typeof(string));
            IReadOnlyList<WeightedValue<string>> expected = new List<WeightedValue<string>>();
            TestResults<IReadOnlyList<WeightedValue<string>>> results =
                new TestResults<IReadOnlyList<WeightedValue<string>>>(
                    _logger.Object,
                    _flowData.Object.Pipeline,
                    _engine.Object,
                    _missingPropertyService.Object,
                    expected);

            var value = results[_testPropertyName];
            Assert.IsTrue(typeof(IAspectPropertyValue).IsAssignableFrom(value.GetType()));
            Assert.AreEqual(expected, ((IAspectPropertyValue)value).Value);
            var dict = results.AsDictionary();
            Assert.IsTrue(dict.ContainsKey(_testPropertyName));
            var dictValue = dict[_testPropertyName];
            Assert.IsTrue(typeof(IAspectPropertyValue).IsAssignableFrom(dictValue.GetType()));
            Assert.AreEqual(expected, ((IAspectPropertyValue)dictValue).Value);
        }

        /// <summary>
        /// Check that a weighted bool list is returned from the internal results instance
        /// using the correct get method.
        /// </summary>
        [TestMethod]
        public void GetWeightedBoolList()
        {
            SetupElementProperties(typeof(bool));
            IReadOnlyList<WeightedValue<bool>> expected = new List<WeightedValue<bool>>();
            TestResults<IReadOnlyList<WeightedValue<bool>>> results =
                new TestResults<IReadOnlyList<WeightedValue<bool>>>(
                    _logger.Object,
                    _flowData.Object.Pipeline,
                    _engine.Object,
                    _missingPropertyService.Object,
                    expected);

            var value = results[_testPropertyName];
            Assert.IsTrue(typeof(IAspectPropertyValue).IsAssignableFrom(value.GetType()));
            Assert.AreEqual(expected, ((IAspectPropertyValue)value).Value);
            var dict = results.AsDictionary();
            Assert.IsTrue(dict.ContainsKey(_testPropertyName));
            var dictValue = dict[_testPropertyName];
            Assert.IsTrue(typeof(IAspectPropertyValue).IsAssignableFrom(dictValue.GetType()));
            Assert.AreEqual(expected, ((IAspectPropertyValue)dictValue).Value);
        }

        /// <summary>
        /// Check that a weighted int list is returned from the internal results instance
        /// using the correct get method.
        /// </summary>
        [TestMethod]
        public void GetWeightedIntList()
        {
            SetupElementProperties(typeof(int));
            IReadOnlyList<WeightedValue<int>> expected = new List<WeightedValue<int>>();
            TestResults<IReadOnlyList<WeightedValue<int>>> results =
                new TestResults<IReadOnlyList<WeightedValue<int>>>(
                    _logger.Object,
                    _flowData.Object.Pipeline,
                    _engine.Object,
                    _missingPropertyService.Object,
                    expected);

            var value = results[_testPropertyName];
            Assert.IsTrue(value is IAspectPropertyValue);
            Assert.IsTrue(typeof(IAspectPropertyValue).IsAssignableFrom(value.GetType()));
            var dict = results.AsDictionary();
            Assert.IsTrue(dict.ContainsKey(_testPropertyName));
            var dictValue = dict[_testPropertyName];
            Assert.IsTrue(typeof(IAspectPropertyValue).IsAssignableFrom(dictValue.GetType()));
            Assert.AreEqual(expected, ((IAspectPropertyValue)dictValue).Value);
        }

        /// <summary>
        /// Check that a weighted double list is returned from the internal results instance
        /// using the correct get method.
        /// </summary>
        [TestMethod]
        public void GetWeightedDoubleList()
        {
            SetupElementProperties(typeof(double));
            IReadOnlyList<WeightedValue<double>> expected = new List<WeightedValue<double>>();
            TestResults<IReadOnlyList<WeightedValue<double>>> results =
                new TestResults<IReadOnlyList<WeightedValue<double>>>(
                    _logger.Object,
                    _flowData.Object.Pipeline,
                    _engine.Object,
                    _missingPropertyService.Object,
                    expected);

            var value = results[_testPropertyName];
            Assert.IsTrue(value is IAspectPropertyValue);
            Assert.IsTrue(typeof(IAspectPropertyValue).IsAssignableFrom(value.GetType()));
            var dict = results.AsDictionary();
            Assert.IsTrue(dict.ContainsKey(_testPropertyName));
            var dictValue = dict[_testPropertyName];
            Assert.IsTrue(typeof(IAspectPropertyValue).IsAssignableFrom(dictValue.GetType()));
            Assert.AreEqual(expected, ((IAspectPropertyValue)dictValue).Value);
        }

        // <summary>
        /// Check that a coordinate is returned from the internal results instance
        /// using the correct get method.
        /// </summary>
        [TestMethod]
        public void GetCoordinate()
        {
            SetupElementProperties(typeof(Coordinate));
            Coordinate expected = new Coordinate(1, 1);
            TestResults<Coordinate> results =
                new TestResults<Coordinate>(
                    _logger.Object,
                    _flowData.Object.Pipeline,
                    _engine.Object,
                    _missingPropertyService.Object,
                    expected);

            var value = results[_testPropertyName];
            Assert.IsTrue(value is IAspectPropertyValue);
            Assert.IsTrue(typeof(IAspectPropertyValue).IsAssignableFrom(value.GetType()));
            var dict = results.AsDictionary();
            Assert.IsTrue(dict.ContainsKey(_testPropertyName));
            var dictValue = dict[_testPropertyName];
            Assert.IsTrue(typeof(IAspectPropertyValue).IsAssignableFrom(dictValue.GetType()));
            Assert.AreEqual(expected, ((IAspectPropertyValue)dictValue).Value);
        }

        // <summary>
        /// Check that an IP address is returned from the internal results instance
        /// using the correct get method.
        /// </summary>
        [TestMethod]
        public void GetIpAddress()
        {
            SetupElementProperties(typeof(IPAddress));
            IPAddress expected = IPAddress.Parse("::1");
            TestResults<IPAddress> results =
                new TestResults<IPAddress>(
                    _logger.Object,
                    _flowData.Object.Pipeline,
                    _engine.Object,
                    _missingPropertyService.Object,
                    expected);

            var value = results[_testPropertyName];
            Assert.IsTrue(value is IAspectPropertyValue);
            Assert.IsTrue(typeof(IAspectPropertyValue).IsAssignableFrom(value.GetType()));
            var dict = results.AsDictionary();
            Assert.IsTrue(dict.ContainsKey(_testPropertyName));
            var dictValue = dict[_testPropertyName];
            Assert.IsTrue(typeof(IAspectPropertyValue).IsAssignableFrom(dictValue.GetType()));
            Assert.AreEqual(expected, ((IAspectPropertyValue)dictValue).Value);
        }
    }
}
