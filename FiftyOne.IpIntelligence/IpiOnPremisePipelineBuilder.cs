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
using FiftyOne.IpIntelligence.Shared.FlowElements;
using FiftyOne.Pipeline.Core.Exceptions;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines;
using FiftyOne.Pipeline.Engines.Configuration;
using FiftyOne.Pipeline.Engines.Data;
using FiftyOne.Pipeline.Engines.FiftyOne.FlowElements;
using FiftyOne.Pipeline.Engines.FlowElements;
using FiftyOne.Pipeline.Engines.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;

namespace FiftyOne.IpIntelligence
{
    /// <summary>
    /// Builder used to create pipelines with an on-premise 
    /// IP intelligence engine.
    /// </summary>
    public class IpiOnPremisePipelineBuilder :
        PrePackagedPipelineBuilderBase<IpiOnPremisePipelineBuilder>
    {
        private string _filename;
        private bool _createTempDataCopy;

        private bool? _autoUpdateEnabled = null;
        private bool? _dataUpdateOnStartUpEnabled = null;
        private bool? _dataFileSystemWatcherEnabled = null;
        private int? _updatePollingInterval = null;
        private int? _updateRandomisationMax = null;
        private string _dataUpdateLicenseKey = null;
        private ushort? _concurrency = null;
        private PerformanceProfiles _performanceProfile =
            PerformanceProfiles.Balanced;
        private bool _shareUsageEnabled = true;
        private List<string> _requestedProperties = new List<string>();

        private string _dataUpdateUrlString = null;
        private Uri _dataUpdateUrlUri = null;

        /// <summary>
        /// A Nullable box for a single nullable formatter reference.
        /// </summary>
        private IList<IDataUpdateUrlFormatter> _dataUpdateUrlFormatter = null;

        private bool? _dataUpdateVerifyMd5 = null;

        private IDataUpdateService _dataUpdateService;
        private HttpClient _httpClient;

        /// <summary>
        /// Internal constructor
        /// This builder should only be created through the 
        /// <see cref="IpiPipelineBuilder"/> 
        /// </summary>
        /// <param name="loggerFactory">
        /// The <see cref="ILoggerFactory"/> to use when creating loggers.
        /// </param>
        /// <param name="dataUpdateService">
        /// The <see cref="IDataUpdateService"/> to use when registering 
        /// data files for automatic updates.
        /// </param>
        /// <param name="httpClient">
        /// The <see cref="HttpClient"/> to use for any web requests.
        /// </param>
        internal IpiOnPremisePipelineBuilder(
            ILoggerFactory loggerFactory,
            IDataUpdateService dataUpdateService,
            HttpClient httpClient) : base(loggerFactory)
        {
            _dataUpdateService = dataUpdateService;
            _httpClient = httpClient;
        }

        /// <summary>
        /// Set the filename of the IP intelligence data file that the
        /// engine should use.
        /// </summary>
        /// <param name="filename">
        /// The data file
        /// </param>
        /// <param name="createTempDataCopy">
        /// </param>
        /// <returns>
        /// This builder instance.
        /// </returns>
        /// <exception cref="PipelineConfigurationException">
        /// Thrown if the filename has an unknown extension.
        /// </exception>
        [Obsolete("Call the overload that takes a license key instead. " +
            "This method will be removed in a future version")]
        internal IpiOnPremisePipelineBuilder SetFilename(string filename, bool createTempDataCopy)
        {
            return SetFilename(filename, _dataUpdateLicenseKey, createTempDataCopy);
        }

        /// <summary>
        /// Set the filename of the IP intelligence data file that the
        /// engine should use.
        /// </summary>
        /// <param name="filename">
        /// The data file
        /// </param>
        /// <param name="key">
        /// The license key to use when checking for updates to the
        /// data file.
        /// This parameter can be set to null, but doing so will disable 
        /// automatic updates. 
        /// </param>
        /// <param name="createTempDataCopy">
        /// True to create a temporary copy of the data file when 
        /// the engine is built.
        /// This is required in order for automatic updates
        /// to work correctly.
        /// </param>
        /// <returns>
        /// This builder instance.
        /// </returns>
        /// <exception cref="PipelineConfigurationException">
        /// Thrown if the filename has an unknown extension.
        /// </exception>
        internal IpiOnPremisePipelineBuilder SetFilename(
            string filename,
            string key,
            bool createTempDataCopy = true)
        {
            _filename = filename;
            _createTempDataCopy = createTempDataCopy;
            _dataUpdateLicenseKey = key;
            return this;
        }

        /// <summary>
        /// Set share usage enabled/disabled.
        /// Defaults to enabled.
        /// </summary>
        /// <param name="enabled">
        /// true to enable usage sharing. False to disable.
        /// </param>
        /// <returns>
        /// This builder instance.
        /// </returns>
        public IpiOnPremisePipelineBuilder SetShareUsage(bool enabled)
        {
            _shareUsageEnabled = enabled;
            return this;
        }

        /// <summary>
        /// Add the specified property as one to be returned.
        /// By default, all properties will be returned.
        /// Reducing the properties that are returned can yield a performance improvement in some 
        /// scenarios.
        /// </summary>
        /// <param name="property">
        /// The property to be populated by.
        /// </param>
        /// <returns>
        /// This builder instance.
        /// </returns>
        public IpiOnPremisePipelineBuilder SetProperty(string property)
        {
            _requestedProperties.Add(property);
            return this;
        }

        /// <summary>
        /// Enable/Disable auto update.
        /// Defaults to enabled.
        /// If enabled, the auto update system will automatically download
        /// and apply new data files for IP Intelligence.
        /// </summary>
        /// <param name="enabled">
        /// true to enable auto update. False to disable.
        /// </param>
        /// <returns>
        /// This builder instance.
        /// </returns>
        public IpiOnPremisePipelineBuilder SetAutoUpdate(bool enabled)
        {
            _autoUpdateEnabled = enabled;
            return this;
        }

        /// <summary>
        /// Enable/Disable update on startup.
        /// Defaults to enabled.
        /// If enabled, the auto update system will be used to check for
        /// an update before the IP intelligence engine is created.
        /// If an update is available, it will be downloaded and applied
        /// before the pipeline is built and returned for use so this may 
        /// take some time.
        /// </summary>
        /// <param name="enabled">
        /// True to enable update on startup. False to disable.
        /// </param>
        /// <returns>
        /// This builder.
        /// </returns>
        public IpiOnPremisePipelineBuilder SetDataUpdateOnStartUp(bool enabled)
        {
            _dataUpdateOnStartUpEnabled = enabled;
            return this;
        }

        /// <summary>
        /// Set the license key used when checking for new 
        /// IP intelligence data files.
        /// Defaults to null.
        /// </summary>
        /// <param name="key">
        /// The license key
        /// </param>
        /// <returns>
        /// This builder instance.
        /// </returns>
        public IpiOnPremisePipelineBuilder SetDataUpdateLicenseKey(string key)
        {
            _dataUpdateLicenseKey = key;
            return this;
        }

        /// <summary>
        /// Set the time between checks for a new data file made by the FiftyOne.Pipeline.Engines.Services.DataUpdateService.
        /// Default = 30 minutes.
        /// </summary>
        /// <param name="pollingIntervalSeconds">
        /// The number of seconds between checks.
        /// </param>
        /// <returns>
        /// This builder instance.
        /// </returns>
        public IpiOnPremisePipelineBuilder SetUpdatePollingInterval(int pollingIntervalSeconds)
        {
            _updatePollingInterval = pollingIntervalSeconds;
            return this;
        }

        /// <summary>
        /// Set the time between checks for a new data file made by the FiftyOne.Pipeline.Engines.Services.DataUpdateService.
        /// Default = 30 minutes.
        /// </summary>
        /// <param name="pollingInterval">
        /// The time between checks.
        /// </param>
        /// <returns>
        /// This builder instance.
        /// </returns>
        public IpiOnPremisePipelineBuilder SetUpdatePollingInterval(TimeSpan pollingInterval)
        {
            _updatePollingInterval = (int)pollingInterval.TotalSeconds;
            return this;
        }

        /// <summary>
        /// A random element can be added to the FiftyOne.Pipeline.Engines.Services.DataUpdateService
        /// polling interval. This option sets the maximum length of this random addition.
        /// Default = 10 minutes.
        /// </summary>
        /// <param name="maximumDeviationSeconds">
        /// The maximum time added to the data update polling interval.
        /// </param>
        /// <returns>
        /// This builder instance.
        /// </returns>
        public IpiOnPremisePipelineBuilder SetUpdateRandomisationMax(int maximumDeviationSeconds)
        {
            _updateRandomisationMax = maximumDeviationSeconds;
            return this;
        }

        /// <summary>
        /// A random element can be added to the FiftyOne.Pipeline.Engines.Services.DataUpdateService
        /// polling interval. This option sets the maximum length of this random addition.
        /// Default = 10 minutes.
        /// </summary>
        /// <param name="maximumDeviation">
        /// The maximum time added to the data update polling interval.
        /// </param>
        /// <returns>
        /// This builder instance.
        /// </returns>
        public IpiOnPremisePipelineBuilder SetUpdateRandomisationMax(TimeSpan maximumDeviation)
        {
            _updateRandomisationMax = (int)maximumDeviation.TotalSeconds;
            return this;
        }

        /// <summary>
        /// Set the performance profile for the IP intelligence engine.
        /// Defaults to balanced.
        /// </summary>
        /// <param name="profile">
        /// The performance profile to use.
        /// </param>
        /// <returns>
        /// This builder instance.
        /// </returns>
        public IpiOnPremisePipelineBuilder SetPerformanceProfile(PerformanceProfiles profile)
        {
            _performanceProfile = profile;
            return this;
        }

        /// <summary>
        /// Set the expected number of concurrent operations using the engine.
        /// This sets the concurrency of the internal caches to avoid excessive
        /// locking.
        /// </summary>
        /// <param name="concurrency">Expected concurrent accesses</param>
        /// <returns>This builder</returns>
        public IpiOnPremisePipelineBuilder SetConcurrency(
            ushort concurrency)
        {
            _concurrency = concurrency;
            return this;
        }

        /// <summary>
        /// Configure the engine to use the specified URL when looking for
        /// an updated data file.
        /// </summary>
        /// <param name="url">
        /// The URL to check for a new data file.
        /// </param>
        /// <returns>
        /// This builder instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if url parameter is null or and empty string
        /// </exception>
        public IpiOnPremisePipelineBuilder SetDataUpdateUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
            _dataUpdateUrlString = url;
            _dataUpdateUrlUri = null;
            return this;
        }

        /// <summary>
        /// Configure the engine to use the specified URL when looking for
        /// an updated data file.
        /// </summary>
        /// <param name="url">
        /// The URL to check for a new data file.
        /// </param>
        /// <returns>
        /// This builder instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if url parameter is null
        /// </exception>
        public IpiOnPremisePipelineBuilder SetDataUpdateUrl(Uri url)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));
            _dataUpdateUrlString = null;
            _dataUpdateUrlUri = url;
            return this;
        }

        /// <summary>
        /// Specify a <see cref="IDataUpdateUrlFormatter"/> to be 
        /// used by the <see cref="DataUpdateService"/> when building the 
        /// complete URL to query for updated data.
        /// </summary>
        /// <param name="formatter">
        /// The formatter to use.
        /// </param>
        /// <returns>
        /// This builder instance.
        /// </returns>
        public IpiOnPremisePipelineBuilder SetDataUpdateUrlFormatter(
            IDataUpdateUrlFormatter formatter)
        {
            _dataUpdateUrlFormatter = new IDataUpdateUrlFormatter[] { formatter };
            return this;
        }

        /// <summary>
        /// Set a value indicating if the <see cref="DataUpdateService"/>
        /// should expect the response from the data update URL to contain a
        /// 'content-md5' HTTP header that can be used to verify the integrity
        /// of the content.
        /// </summary>
        /// <param name="verify">
        /// True if the content should be verified with the Md5 hash.
        /// False otherwise.
        /// </param>
        /// <returns>
        /// This builder instance.
        /// </returns>
        public IpiOnPremisePipelineBuilder SetDataUpdateVerifyMd5(bool verify)
        {
            _dataUpdateVerifyMd5 = verify;
            return this;
        }

        /// <summary>
        /// The <see cref="DataUpdateService"/> has the ability to watch a 
        /// file on disk and refresh the engine as soon as that file is 
        /// updated.
        /// This setting enables/disables that feature.
        /// </summary>
        /// <param name="enabled">
        /// Pass true to enable the feature.
        /// </param>
        /// <returns></returns>
        public IpiOnPremisePipelineBuilder SetDataFileSystemWatcher(bool enabled)
        {
            _dataFileSystemWatcherEnabled = enabled;
            return this;
        }

        /// <summary>
        /// Build and return a pipeline that can perform IP intelligence.
        /// </summary>
        /// <returns></returns>
        public override IPipeline Build()
        {
            IAspectEngine ipiEngine = null;

            // Create the IP intelligence engine based on the configuration.
            var ipiBuilder = new IpiOnPremiseEngineBuilder(LoggerFactory, _dataUpdateService);
            ipiEngine = ConfigureAndBuild(ipiBuilder);

            if (ipiEngine != null)
            {
                // Add the share usage element to the list if enabled
                if (_shareUsageEnabled)
                {
                    FlowElements.Add(new ShareUsageBuilder(LoggerFactory, _httpClient).Build());
                }
                // Add the IP intelligence engine to the list
                FlowElements.Add(ipiEngine);
            }
            else
            {
                throw new PipelineException(Messages.ExceptionErrorOnStartup);
            }

            // Create and return the pipeline
            return base.Build();
        }

        /// <summary>
        /// Private method used to set configuration options
        /// </summary>
        /// <typeparam name="TBuilder">
        /// The type of the builder. Can be inferred from the builder parameter.
        /// </typeparam>
        /// <typeparam name="TEngine">
        /// The type of the engine. Can be inferred from the builder parameter.
        /// </typeparam>
        /// <param name="builder">
        /// The builder to configure.
        /// </param>
        /// <returns>
        /// A new IP intelligence engine instance.
        /// </returns>
        private TEngine ConfigureAndBuild<TBuilder, TEngine>(OnPremiseIpiEngineBuilderBase<TBuilder, TEngine> builder)
            where TBuilder : OnPremiseIpiEngineBuilderBase<TBuilder, TEngine>
            where TEngine : IFiftyOneAspectEngine
        {
            // Configure caching
            if (ResultsCache)
            {
                builder.SetCache(new CacheConfiguration() { Size = ResultsCacheSize });
            }
            // Configure lazy loading
            if (LazyLoading)
            {
                builder.SetLazyLoading(new LazyLoadingConfiguration(
                    (int)LazyLoadingTimeout.TotalMilliseconds,
                    LazyLoadingCancellationToken));
            }

            // Configure auto update.
            if (_autoUpdateEnabled.HasValue)
            {
                builder.SetAutoUpdate(_autoUpdateEnabled.Value);
            }
            // Configure data update on startup.
            if (_dataUpdateOnStartUpEnabled.HasValue)
            {
                builder.SetDataUpdateOnStartup(_dataUpdateOnStartUpEnabled.Value);
            }
            // Configure file system watcher.
            if (_dataFileSystemWatcherEnabled.HasValue)
            {
                builder.SetDataFileSystemWatcher(_dataFileSystemWatcherEnabled.Value);
            }
            // Configure update poilling interval.
            if (_updatePollingInterval.HasValue)
            {
                builder.SetUpdatePollingInterval(_updatePollingInterval.Value);
            }
            // Configure update polling interval randomisation.
            if (_updateRandomisationMax.HasValue)
            {
                builder.SetUpdateRandomisationMax(_updateRandomisationMax.Value);
            }
            // Configure requested properties
            foreach (var property in _requestedProperties)
            {
                builder.SetProperty(property);
            }

            if (_dataUpdateUrlString != null)
            {
#               pragma warning disable CA2234 // Pass system uri objects instead of strings
                builder.SetDataUpdateUrl(_dataUpdateUrlString);
#               pragma warning restore CA2234 // Pass system uri objects instead of strings
            }
            else if (_dataUpdateUrlUri != null)
            {
                builder.SetDataUpdateUrl(_dataUpdateUrlUri);
            }
            if (_dataUpdateUrlFormatter != null && _dataUpdateUrlFormatter.Count > 0)
            {
                builder.SetDataUpdateUrlFormatter(_dataUpdateUrlFormatter[0]);
            }
            if (_dataUpdateVerifyMd5.HasValue)
            {
                builder.SetDataUpdateVerifyMd5(_dataUpdateVerifyMd5.Value);
            }

            builder.SetDataUpdateLicenseKey(_dataUpdateLicenseKey);

            // Configure performance profile
            builder.SetPerformanceProfile(_performanceProfile);
            // Configure concurrency
            if (_concurrency.HasValue)
            {
                builder.SetConcurrency(_concurrency.Value);
            }
            // Build the engine
            TEngine engine = default(TEngine);

            if (string.IsNullOrEmpty(_filename) == false)
            {
                engine = builder.Build(_filename, _createTempDataCopy);
            }
            else
            {
                throw new PipelineConfigurationException(
                    Messages.ExceptionNoEngineData);
            }

            return engine;
        }

    }
}
