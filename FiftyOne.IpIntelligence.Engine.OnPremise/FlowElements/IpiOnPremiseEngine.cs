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
using FiftyOne.IpIntelligence.Engine.OnPremise.Interop;
using FiftyOne.IpIntelligence.Engine.OnPremise.Wrappers;
using FiftyOne.IpIntelligence.Shared.Data;
using FiftyOne.IpIntelligence.Shared.FlowElements;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.Exceptions;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines.Data;
using FiftyOne.Pipeline.Engines.FiftyOne.Data;
using FiftyOne.Pipeline.Engines.FiftyOne.FlowElements;
using FiftyOne.Pipeline.Engines.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FiftyOne.IpIntelligence.Engine.OnPremise.FlowElements
{
    /// <summary>
    /// IP intelligence engine. This engine takes IP addresses and
    /// other relevant HTTP headers and returns properties about the IP
    /// range that the IP address fall within
    /// </summary>
    public class IpiOnPremiseEngine : OnPremiseIpiEngineBase<IIpDataOnPremise>
    {
        /// <summary>
        /// Factory used to create a new <see cref="IEngineSwigWrapper"/> when
        /// <see cref="RefreshData(string)"/> or 
        /// <see cref="RefreshData(string, Stream)"/> is called.
        /// </summary>
        /// <remarks>
        /// Must be set after construction and before usage.
        /// </remarks>
        internal ISwigFactory SwigFactory { get; set; }

        private IEngineSwigWrapper _engine;

        private IEvidenceKeyFilter _evidenceKeyFilter;

        private IList<IFiftyOneAspectPropertyMetaData> _properties;
        private IList<IComponentMetaData> _components;

        /// <summary>
        /// Wrapper to pass general configuration from managed code to unmanaged 
        /// code.
        /// </summary>
        /// <remarks>
        /// Must be set after construction and before usage.
        /// </remarks>
        internal IConfigSwigWrapper Config { get; set; }

        /// <summary>
        /// Wrapper to pass property configuration from managed code to 
        /// unmanaged code.
        /// </summary>
        internal IRequiredPropertiesConfigSwigWrapper PropertiesConfigSwig { get; set; }

        private static Random _rng = new Random();

        // The component used for metric properties.
        private ComponentMetaDataDefault _ipMetricsComponent = new ComponentMetaDataIpi("Metrics");

        /// <summary>
        /// This event is fired whenever the data that this engine makes use
        /// of has been updated.
        /// </summary>
        public override event EventHandler<EventArgs> RefreshCompleted;

        /// <summary>
        /// Construct a new instance of the IP intelligence engine.
        /// </summary>
        /// <param name="loggerFactory">Logger to use</param>
        /// <param name="ipDataFactory">
        /// Method used to get an aspect data instance
        /// </param>
        /// <param name="dataFile">Meta data related to the data file</param>
        /// <param name="config">Configuration instance</param>
        /// <param name="properties">Properties to be initialised</param>
        /// <param name="tempDataFilePath">
        /// The directory to use when storing temporary copies of the 
        /// data file(s) used by this engine.
        /// </param>
        /// <param name="swigFactory">
        /// The factory object to use when creating swig wrapper instances.
        /// Usually a <see cref="Wrappers.SwigFactory"/> instance.
        /// Unit tests can override this to mock behaviour as needed.
        /// </param>
        internal protected IpiOnPremiseEngine(
            ILoggerFactory loggerFactory,
            Func<IPipeline, FlowElementBase<IIpDataOnPremise, IFiftyOneAspectPropertyMetaData>, IIpDataOnPremise> ipDataFactory,
            string tempDataFilePath)
            : base(
                  loggerFactory.CreateLogger<IpiOnPremiseEngine>(),
                  ipDataFactory,
                  tempDataFilePath)
        {
        }

        /// <summary>
        /// The key to use for this element's data in a 
        /// <see cref="IFlowData"/> instance.
        /// </summary>
        public override string ElementDataKey => "ip";

        internal IMetaDataSwigWrapper MetaData => _engine.getMetaData();

        /// <summary>
        /// Get the meta-data for properties populated by this engine.
        /// </summary>
        public override IList<IFiftyOneAspectPropertyMetaData> Properties
        {
            get
            {
                return _properties;
            }
        }

        /// <summary>
        /// Get the meta-data for profiles that may be returned by this
        /// engine.
        /// </summary>
        public override IEnumerable<IProfileMetaData> Profiles
        {
            get
            {
                using (var profiles = _engine.getMetaData().getProfiles(this))
                {
                    foreach (var profile in profiles)
                    {
                        yield return profile;
                    }
                }
            }
        }

        /// <summary>
        /// Get the meta-data for components populated by this engine.
        /// </summary>
        public override IEnumerable<IComponentMetaData> Components
        {
            get
            {
                return _components;
            }
        }

        /// <summary>
        /// Get the meta-data for values that can be returned by this engine.
        /// </summary>
        public override IEnumerable<IValueMetaData> Values
        {
            get
            {
                using (var values = _engine.getMetaData().getValues(this))
                {
                    foreach (var value in values)
                    {
                        yield return value;
                    }
                }
            }
        }

        /// <summary>
        /// The tier of the data that is currently being used by this engine.
        /// For example, 'Lite' or 'Enterprise'
        /// </summary>
        public override string DataSourceTier => _engine.getType();

        /// <summary>
        /// True if the data used by this engine will automatically be
        /// updated when a new file is available.
        /// False if the data will only be updated manually.
        /// </summary>
        public bool AutomaticUpdatesEnabled => _engine.getAutomaticUpdatesEnabled();

        /// <summary>
        /// A filter that defines the evidence that this engine can 
        /// make use of.
        /// </summary>
        public override IEvidenceKeyFilter EvidenceKeyFilter => _evidenceKeyFilter;

        /// <summary>
        /// Called when update data is available in order to get the 
        /// engine to refresh it's internal data structures.
        /// This overload is used if the data is a physical file on disk.
        /// </summary>
        /// <param name="dataFileIdentifier">
        /// The identifier of the data file to update.
        /// This engine only uses one data file so this parameter is ignored.
        /// </param>
        public override void RefreshData(string dataFileIdentifier)
        {
            var dataFile = DataFiles.Single();
            if (_engine == null)
            {
                _engine = SwigFactory.CreateEngine(dataFile.DataFilePath, Config, PropertiesConfigSwig);
            }
            else
            {
                _engine.refreshData();
            }
            InitEngineMetaData();
            RefreshCompleted?.Invoke(this, null);
        }

        /// <summary>
        /// Called when update data is available in order to get the 
        /// engine to refresh it's internal data structures.
        /// This overload is used when the data is presented as a 
        /// <see cref="Stream"/>, usually a <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="dataFileIdentifier">
        /// The identifier of the data file to update.
        /// This engine only uses one data file so this parameter is ignored.
        /// </param>
        /// <param name="stream">
        /// The <see cref="Stream"/> containing the data to refresh the
        /// engine with.
        /// </param>
        public override void RefreshData(string dataFileIdentifier, Stream stream)
        {
            var data = ReadBytesFromStream(stream);

            if (_engine == null)
            {
                _engine = SwigFactory.CreateEngine(data, data.Length, Config, PropertiesConfigSwig);
            }
            else
            {
                _engine.refreshData(data, data.Length);
            }
            InitEngineMetaData();
            RefreshCompleted?.Invoke(this, null);
        }

        /// <summary>
        /// Perform processing for this engine
        /// </summary>
        /// <param name="data">
        /// The <see cref="IFlowData"/> instance containing data for the 
        /// current request.
        /// </param>
        /// <param name="ipData">
        /// The <see cref="IIpDataOnPremise"/> instance to populate with
        /// property values
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if a required parameter is null
        /// </exception>
        protected override void ProcessEngine(IFlowData data, IIpDataOnPremise ipData)
        {
            if (data == null) { throw new ArgumentNullException(nameof(data)); }
            if (ipData == null) { throw new ArgumentNullException(nameof(ipData)); }

            using (var relevantEvidence = new EvidenceIpiSwig())
            {
                foreach (var evidenceItem in data.GetEvidence().AsDictionary())
                {
                    if (EvidenceKeyFilter.Include(evidenceItem.Key))
                    {
                        relevantEvidence.Add(new KeyValuePair<string, string>(
                            evidenceItem.Key,
                            evidenceItem.Value.ToString()));
                    }
                }
                (ipData as IpDataOnPremise).SetResults(_engine.process(relevantEvidence));
            }
        }

        /// <summary>
        /// Dispose of any unmanaged resources.
        /// </summary>
        protected override void UnmanagedResourcesCleanup()
        {
            if (_engine != null)
            {
                _engine.Dispose();
            }
        }

        private IList<IComponentMetaData> ConstructComponents()
        {
            var result = new List<IComponentMetaData>();
            using (var components = _engine.getMetaData().getComponents(this))
            {
                foreach (var component in components)
                {
                    result.Add(component);
                }
            }
            result.Add(_ipMetricsComponent);
            return result;
        }

        private IList<IFiftyOneAspectPropertyMetaData> ConstructProperties()
        {
            var result = new List<IFiftyOneAspectPropertyMetaData>();
            using (var properties = _engine.getMetaData().getProperties(this))
            {
                foreach (var property in properties)
                {
                    result.Add(property);
                }
            }
            return result;
        }

        private void InitEngineMetaData()
        {
            _evidenceKeyFilter = new EvidenceKeyFilterWhitelist(
                new List<string>(_engine.getKeys()),
                // new List<string>(_engine.getKeys()),
                StringComparer.InvariantCultureIgnoreCase);

            _properties = ConstructProperties();
            _components = ConstructComponents();

            // Populate these data file properties from the native engine.
            var dataFileMetaData = GetDataFileMetaData() as IFiftyOneDataFile;
            if (dataFileMetaData != null)
            {
                dataFileMetaData.DataPublishedDateTime = GetDataFilePublishedDate();
                dataFileMetaData.UpdateAvailableTime = GetDataFileUpdateAvailableTime();
                dataFileMetaData.TempDataFilePath = GetDataFileTempPath();
            }
        }

        private DateTime GetDataFilePublishedDate()
        {
            if (_engine != null)
            {
                var value = _engine.getPublishedTime();
                return new DateTime(
                    value.getYear(),
                    value.getMonth(),
                    value.getDay(),
                    0,
                    0,
                    0,
                    DateTimeKind.Utc);
            }
            return new DateTime();
        }
        private DateTime GetDataFileUpdateAvailableTime()
        {
            if (_engine != null)
            {
                var value = _engine.getUpdateAvailableTime();
                return new DateTime(
                    value.getYear(),
                    value.getMonth(),
                    value.getDay(),
                    12,
                    _rng.Next(0, 60),
                    0,
                    DateTimeKind.Utc);
            }
            return new DateTime();
        }
        private string GetDataFileTempPath()
        {
            return _engine?.getDataFileTempPath();
        }

        /// <summary>
        /// Get the value to use for the 'Type' parameter when calling
        /// the 51Degrees Distributor service to check for a newer 
        /// data file.
        /// </summary>
        /// <param name="identifier">
        /// The identifier of the data file to get the type for.
        /// This engine only uses one file so this parameter is ignored.
        /// </param>
        /// <returns>
        /// A string
        /// </returns>
        public override string GetDataDownloadType(string identifier)
        {
            return _engine.getType();
        }
    }
}
