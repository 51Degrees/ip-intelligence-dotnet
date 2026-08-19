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

using FiftyOne.IpIntelligence.Engine.OnPremise.Data;
using FiftyOne.IpIntelligence.Engine.OnPremise.Interop;
using FiftyOne.IpIntelligence.Engine.OnPremise.Wrappers;
using FiftyOne.IpIntelligence.Shared.Data;
using FiftyOne.IpIntelligence.Shared.FlowElements;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines.FiftyOne.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        /// <summary>
        /// The evidence keys the engine accepts, in the order the native
        /// lookup consults them. ProcessEngine walks this list both to
        /// select the evidence handed to the native engine and to pick the
        /// echoed client IP - changing it changes what reaches the lookup,
        /// not just the echo. See
        /// <see cref="OrderEvidenceKeysAsNativeEngine"/>.
        /// </summary>
        private string[] _orderedEvidenceKeys;

        /// <summary>
        /// The evidence prefixes the native lookup reads, in the order it
        /// reads them. See <see cref="OrderEvidenceKeysAsNativeEngine"/>.
        /// </summary>
        private static readonly string[] _nativeEvidencePrefixPriority =
        {
            Pipeline.Core.Constants.EVIDENCE_QUERY_PREFIX +
                Pipeline.Core.Constants.EVIDENCE_SEPERATOR,
            Pipeline.Core.Constants.EVIDENCE_SERVER_PREFIX +
                Pipeline.Core.Constants.EVIDENCE_SEPERATOR,
        };

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

        private static readonly Random _rng = new Random();

        // The component used for metric properties.
        private readonly ComponentMetaDataDefault _ipMetricsComponent =
            new ComponentMetaDataIpi("Metrics");

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
        /// <param name="tempDataFilePath">
        /// The directory to use when storing temporary copies of the 
        /// data file(s) used by this engine.
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

            // Walk the client IP evidence keys in the order the native
            // lookup consults them - see OrderEvidenceKeysAsNativeEngine
            // for how that order is derived - doing two things in the one
            // pass:
            //
            // 1. Hand every readable value to the native engine. It raises
            //    INCORRECT_IP_ADDRESS_FORMAT on a value it cannot parse,
            //    which ends its processing before lower-priority evidence
            //    is tried, so parse here instead: a value the tolerant
            //    parser can read is passed on in canonical bare-address
            //    form, and a value it cannot read is left out so the native
            //    fall-through still happens. This is deliberately stricter
            //    than the native parser, which stops at a '/' or ' ' and so
            //    resolved "1.2.3.4/24" and "1.2.3.4 x" as 1.2.3.4; those
            //    now yield no result. Requiring a whole, valid address is
            //    what lets a malformed high-priority value fall through to
            //    a valid lower-priority one (issue #319). The value goes
            //    under the engine's own spelling of the key: the filter
            //    admits any casing, but the native prefix match is
            //    case-sensitive and would silently drop "Query.client-ip".
            //
            // 2. Capture the client IP to echo back as the synthetic Ip /
            //    IpV6 properties. The first readable value in this order is
            //    the one the native lookup resolves, so the echo names the
            //    address the location properties beside it describe
            //    (issue #333). Selection is by validity, not presence: an
            //    unreadable value lets the search continue.
            System.Net.IPAddress chosenAddress = null;

            // Whether the request offered a client IP at all, so that
            // "nothing resolved" can be told apart from "nothing was
            // offered". Every key walked here is a client IP header, so a
            // non-blank value none of them could parse means the IP
            // supplied was invalid, which is reported differently. See
            // SetEchoIp.
            var clientIpSupplied = false;

            using (var relevantEvidence = new EvidenceIpiSwig())
            {
                foreach (var evidenceKey in _orderedEvidenceKeys)
                {
                    if (data.TryGetEvidence(evidenceKey, out object rawValue) == false)
                    {
                        continue;
                    }
                    var rawText = rawValue?.ToString();
                    // A blank value is nothing offered rather than
                    // something unreadable, so it must not be reported as
                    // invalid.
                    if (string.IsNullOrWhiteSpace(rawText) == false)
                    {
                        clientIpSupplied = true;
                    }
                    if (ClientIpParser.TryParse(
                        rawText,
                        out var address,
                        out var addressText))
                    {
                        relevantEvidence.Add(new KeyValuePair<string, string>(
                            evidenceKey,
                            addressText));
                        if (chosenAddress == null)
                        {
                            chosenAddress = address;
                        }
                    }
                }
                (ipData as IpDataOnPremise).SetResults(_engine.process(relevantEvidence));
            }

            (ipData as IpDataOnPremise).SetEchoIp(
                chosenAddress?.AddressFamily ==
                    System.Net.Sockets.AddressFamily.InterNetwork
                    ? chosenAddress : null,
                chosenAddress?.AddressFamily ==
                    System.Net.Sockets.AddressFamily.InterNetworkV6
                    ? chosenAddress : null,
                clientIpSupplied);
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

            // Synthetic echo properties — populated from request evidence
            // during ProcessEngine. Component must be "Network" so they
            // align with common-metadata's Network component (VendorIds:
            // ["ip"]) and resolve against the CloudV5* product entries.
            var networkComponent = new Data.ComponentMetaDataIpi("Network");
            var noDataTiers = new System.Collections.Generic.List<string>();
            var noDefault = new ValueMetaDataDefault("N/A");

            result.Add(new Data.FiftyOneAspectPropertyMetaDataIpi(
                this,
                "Ip",
                typeof(System.Net.IPAddress),
                "Network",
                noDataTiers,
                true,
                networkComponent,
                noDefault,
                "The IPv4 address of the request as a string."));
            result.Add(new Data.FiftyOneAspectPropertyMetaDataIpi(
                this,
                "IpV6",
                typeof(System.Net.IPAddress),
                "Network",
                noDataTiers,
                true,
                networkComponent,
                noDefault,
                "The IPv6 address of the request as a string."));

            return result;
        }

        /// <summary>
        /// Order the engine's evidence keys the way the native lookup
        /// consults them, so that a walk over the result meets the keys in
        /// the order fiftyoneDegreesResultsIpiFromEvidence does.
        /// </summary>
        /// <remarks>
        /// The native engine lists its keys header by header, in the
        /// priority order the data file defines, giving each header under
        /// every prefix in turn: query.h0, server.h0, query.h1, server.h1,
        /// ... (EngineIpi::initHttpHeaderKeys). It does not search evidence
        /// in that interleaved order: fiftyoneDegreesResultsIpiFromEvidence
        /// runs a full pass over the 'query.' prefix, walking the headers
        /// in priority order, and only falls back to a 'server.' pass when
        /// that produced nothing. This regroups the keys by prefix, keeping
        /// the header order within each group.
        /// </remarks>
        /// <param name="engineKeys">
        /// The keys as reported by the native engine.
        /// </param>
        /// <returns>
        /// The keys grouped by <see cref="_nativeEvidencePrefixPriority"/>,
        /// in their original relative order within each group. A key under
        /// no known prefix is left out: the native lookup never reads such
        /// evidence, so it must neither reach the lookup nor drive the echo.
        /// </returns>
        internal static string[] OrderEvidenceKeysAsNativeEngine(
            IReadOnlyCollection<string> engineKeys)
        {
            return _nativeEvidencePrefixPriority
                .SelectMany(prefix => engineKeys.Where(evidenceKey =>
                    evidenceKey.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
                .ToArray();
        }

        private void InitEngineMetaData()
        {
            var engineKeys = new List<string>(_engine.getKeys());
            // OrdinalIgnoreCase to match the comparer of the evidence
            // dictionary the ProcessEngine walk reads through
            // (FlowData.TryGetEvidence), so a key the filter admits is
            // always a key the walk can find.
            _evidenceKeyFilter = new EvidenceKeyFilterWhitelist(
                engineKeys,
                StringComparer.OrdinalIgnoreCase);
            _orderedEvidenceKeys = OrderEvidenceKeysAsNativeEngine(engineKeys);

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
