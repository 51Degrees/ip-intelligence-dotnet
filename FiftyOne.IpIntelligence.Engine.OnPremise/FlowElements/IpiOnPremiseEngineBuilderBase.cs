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
using FiftyOne.IpIntelligence.Shared.FlowElements;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.Exceptions;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines;
using FiftyOne.Pipeline.Engines.FiftyOne.Data;
using FiftyOne.Pipeline.Engines.FlowElements;
using FiftyOne.Pipeline.Engines.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using FiftyOne.IpIntelligence.Engine.OnPremise.Wrappers;

namespace FiftyOne.IpIntelligence.Engine.OnPremise.FlowElements
{
    /// <summary>
    /// Builder for the <see cref="IpiOnPremiseEngine"/>. All options
    /// for the engine should be set here.
    /// </summary>
    public abstract class IpiOnPremiseEngineBuilderBase<TEngine>
       : OnPremiseIpiEngineBuilderBase<IpiOnPremiseEngineBuilderBase<TEngine>, TEngine>
        where TEngine : IpiOnPremiseEngine
    {
        #region Private Properties

        private readonly ILoggerFactory _loggerFactory;

        private IConfigSwigWrapper _config = null;

        #endregion

        internal ISwigFactory SwigFactory { get; set; } = new SwigFactory();

        private IConfigSwigWrapper SwigConfig
        {
            get
            {
                if (_config == null)
                {
                    _config = SwigFactory.CreateConfig();
                }
                return _config;
            }
        }

        #region Constructor

        /// <summary>
        /// Construct a new instance of the builder.
        /// </summary>
        /// <param name="loggerFactory">
        /// Factory used to create loggers for the engine
        /// </param>
        public IpiOnPremiseEngineBuilderBase(
            ILoggerFactory loggerFactory)
            : this(loggerFactory, null)
        {
        }

        /// <summary>
        /// Construct a new instance of the builder.
        /// </summary>
        /// <param name="loggerFactory">
        /// Factory used to create loggers for the engine
        /// </param>
        /// <param name="dataUpdateService">
        /// Data update service used to keep the engine's data up to date.
        /// </param>
        public IpiOnPremiseEngineBuilderBase(
            ILoggerFactory loggerFactory,
            IDataUpdateService dataUpdateService)
            : base(dataUpdateService)
        {
            _loggerFactory = loggerFactory;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Set whether or not an existing temp file should be used if one is
        /// found in the temp directory.
        /// </summary>
        /// <param name="reuse">True if an existing file should be used</param>
        /// <returns>This builder</returns>
        public IpiOnPremiseEngineBuilderBase<TEngine> SetReuseTempFile(bool reuse)
        {
            SwigConfig.setReuseTempFile(reuse);
            return this;
        }

        /// <summary>
        /// Set the performance profile to use when constructing the data set.
        /// </summary>
        /// <param name="profileName">Name of the profile to use</param>
        /// <returns>This builder</returns>
        public IpiOnPremiseEngineBuilderBase<TEngine> SetPerformanceProfile(
            string profileName)
        {
            PerformanceProfiles profile;
            if (Enum.TryParse<PerformanceProfiles>(
                profileName,
                out profile))
            {
                return SetPerformanceProfile(profile);
            }
            else
            {
                var available = Enum.GetNames(typeof(PerformanceProfiles))
                    .Select(i => "'" + i + "'");
                throw new ArgumentException(
                    $"'{profileName}' is not a valid performance profile. " +
                    $"Available profiles are {string.Join(", ", available)}.");
            }
        }

        /// <summary>
        /// Set the performance profile to use when constructing the data set.
        /// </summary>
        /// <param name="profile">Profile to use</param>
        /// <returns>This builder</returns>
        public override IpiOnPremiseEngineBuilderBase<TEngine> SetPerformanceProfile(
            PerformanceProfiles profile)
        {
            switch (profile)
            {
                case PerformanceProfiles.LowMemory:
                    SwigConfig.setLowMemory();
                    break;
                case PerformanceProfiles.Balanced:
                    SwigConfig.setBalanced();
                    break;
                case PerformanceProfiles.BalancedTemp:
                    SwigConfig.setBalancedTemp();
                    break;
                case PerformanceProfiles.HighPerformance:
                    SwigConfig.setHighPerformance();
                    break;
                case PerformanceProfiles.MaxPerformance:
                    SwigConfig.setMaxPerformance();
                    break;
                default:
                    throw new ArgumentException(
                        $"The performance profile '{profile}' is not valid " +
                        $"for a IpiOnPremiseEngine.",
                        nameof(profile));
            }
            return this;
        }

        /// <summary>
        /// Set the expected number of concurrent operations using the engine.
        /// This sets the concurrency of the internal caches to avoid excessive
        /// locking.
        /// </summary>
        /// <param name="concurrency">Expected concurrent accesses</param>
        /// <returns>This builder</returns>
        public override IpiOnPremiseEngineBuilderBase<TEngine> SetConcurrency(
            ushort concurrency)
        {
            SwigConfig.setConcurrency(concurrency);
            return this;
        }

        #endregion

        #region Protected Overrides
        /// <summary>
        /// Called by the 'BuildEngine' method to handle
        /// creation of the engine instance.
        /// </summary>
        /// <param name="properties"></param>
        /// <returns>
        /// An <see cref="IAspectEngine"/>.
        /// </returns>
        protected override TEngine NewEngine(
            List<string> properties)
        {
            if (DataFiles.Count != 1)
            {
                throw new PipelineConfigurationException(
                    "This builder requires one and only one configured file " +
                    $"but it has {DataFileConfigs.Count}");
            }
            var dataFile = DataFiles.First();
            // We remove the data file configuration from the list.
            // This is because the on-premise engine builder base class 
            // adds all the data file configs after engine creation.  
            // However, the IP intelligence data files are supplied 
            // directly to the constructor.
            // Consequently, we remove it here to stop it from being added 
            // again by the base class.
            DataFiles.Remove(dataFile);

            // Update the swig configuration object.
            if (dataFile.Configuration.CreateTempCopy && String.IsNullOrEmpty(TempDir) == false)
            {
                using (var tempDirs = new VectorStringSwig())
                {
                    tempDirs.Add(TempDir);
                    SwigConfig.setTempDirectories(tempDirs);
                }
                SwigConfig.setUseTempFile(true);
            }

            // Create swig property configuration object.
            IRequiredPropertiesConfigSwigWrapper propertyConfig = null;
            using (var vProperties = new VectorStringSwig(properties))
            {
                propertyConfig = SwigFactory.CreateRequiredProperties(vProperties);
            }

            // Create a new instance of the required engine type.
            var engine = CreateEngine(
                _loggerFactory,
                CreateAspectData,
                TempDir);

            // Then set the instances of properties that remain internal to
            // the package.
            engine.SwigFactory = SwigFactory;
            engine.PropertiesConfigSwig = propertyConfig;
            engine.Config = SwigConfig;

            // This must be last as the call results in RefreshData being
            // triggered which requires the prior three properties to be set.
            engine.AddDataFile(dataFile);

            return engine;
        }

        /// <summary>
        /// Get the default value for the 'Type' parameter that is passed
        /// to the 51Degrees Distributor service when checking for updated
        /// data files.
        /// </summary>
        protected override string DefaultDataDownloadType => "IPIV41";
        #endregion

        #region Abstract Methods
        /// <summary>
        /// Overridden in the implementing class to create a new instance of
        /// TEngine with the constructor parameters provided.
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="deviceDataFactory"></param>
        /// <param name="tempDataFilePath"></param>
        /// <returns></returns>
        protected abstract TEngine CreateEngine(
            ILoggerFactory loggerFactory,
            Func<IPipeline, FlowElementBase<
                IIpDataOnPremise,
                IFiftyOneAspectPropertyMetaData>,
                IIpDataOnPremise> deviceDataFactory,
            string tempDataFilePath);
        #endregion


        private IIpDataOnPremise CreateAspectData(IPipeline pipeline,
            FlowElementBase<IIpDataOnPremise, IFiftyOneAspectPropertyMetaData> engine)
        {
            return new IpDataOnPremise(
                _loggerFactory.CreateLogger<IpDataOnPremise>(),
                pipeline,
                engine as IpiOnPremiseEngine,
                MissingPropertyService.Instance);
        }
    }
}
