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

using FiftyOne.IpIntelligence.Shared.FlowElements;
using FiftyOne.Pipeline.Engines.FiftyOne.Data;
using FiftyOne.Pipeline.Engines.FiftyOne.FlowElements;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FiftyOne.IpIntelligence.Shared.Services
{
    /// <summary>
    /// Helper service that removes some of the complexity of dealing with 
    /// meta-data from multiple engines.
    /// Also mitigates some of the performance penalties that come from 
    /// repeatedly querying certain meta-data collections via the SWIG 
    /// wrappers.
    /// </summary>
    public class MetaDataService : IMetaDataService
    {
        private IOnPremiseIpiEngine[] _engines;

        private ConcurrentDictionary<byte, uint?> _componentIdToDefaultProfileId;
        private ConcurrentDictionary<uint, byte?> _profileIdToComponentId;

        private bool _setAllDefaults = false;
        private readonly object _setAllDefaultsLock = new object();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engines">
        /// An array of engines that are involved in populating IP intelligence 
        /// data properties.
        /// For example, this could be one engine that populates autonomous system number
        /// properties and another that populates internet service provider properties.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if a required parameter is null
        /// </exception>
        public MetaDataService(IOnPremiseIpiEngine[] engines)
        {
            if (engines == null) { throw new ArgumentNullException(nameof(engines)); }

            _componentIdToDefaultProfileId = new ConcurrentDictionary<byte, uint?>();
            _profileIdToComponentId = new ConcurrentDictionary<uint, byte?>();
            _engines = engines;
            foreach (var engine in _engines)
            {
                engine.RefreshCompleted += Engine_RefreshCompleted;
            }
        }

        /// <summary>
        /// Get the default profile Id for the specified component
        /// </summary>
        /// <param name="componentId">
        /// The component Id to get the default for
        /// </param>
        /// <returns>
        /// The default profile Id for the specified component
        /// </returns>
        public uint? DefaultProfileIdForComponent(byte componentId)
        {
            return _componentIdToDefaultProfileId.GetOrAdd(componentId, cid =>
            {
                var matches = _engines.SelectMany(e => e.Components)
                    .Where(c => c.ComponentId == cid);
                if (matches.Count() == 1)
                {
                    return matches.Single().DefaultProfile?.ProfileId;
                }
                return null;
            });
        }

        // TODO: This loads all profiles into memory and cause
        // performance overhead. Comment out until a better
        // solution is found.
        //
        // /// <summary>
        // /// Get the component Id for the specified profile
        // /// </summary>
        // /// <param name="profileId">
        // /// The profile Id to get the component Id for
        // /// </param>
        // /// <returns>
        // /// The component Id for the specified profile
        // /// </returns>
        // public byte? ComponentIdForProfile(uint profileId)
        // {
        // 
        //     return _profileIdToComponentId.GetOrAdd(profileId, pid =>
        //     {
        //         IList<IProfileMetaData> matches = new List<IProfileMetaData>();
        //         foreach (var engine in _engines) 
        //         {
        //             foreach (var profile in engine.Profiles)
        //             {
        //                 if (profile.ProfileId == pid) 
        //                 {
        //                     matches.Add(profile);
        //                 }
        //                 else {
        //                     profile.Dispose();
        //                 }
        //             }
        //         }
        //         if (matches.Count > 0)
        //         {
        //             byte compId = matches.Single().Component.ComponentId;
        //             matches.Single().Dispose();
        //             return compId;
        //         }
        //         return null;
        //     });
        // }

        /// <summary>
        /// Get all default profile Ids
        /// </summary>
        /// <returns>
        /// A dictionary with key of component Id and value of profile Id.
        /// </returns>
        public IReadOnlyDictionary<byte, uint?> DefaultProfilesIds()
        {
            if (_setAllDefaults == false)
            {
                lock (_setAllDefaultsLock)
                {
                    if (_setAllDefaults == false)
                    {
                        _setAllDefaults = true;
                        foreach (var cid in _engines
                            .SelectMany(e => e.Components)
                            .Select(c => c.ComponentId))
                        {
                            DefaultProfileIdForComponent(cid);
                        }
                    }
                }
            }
            return _componentIdToDefaultProfileId;
        }


        /// <summary>
        /// Called when the engine is refreshed with new data.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Engine_RefreshCompleted(object sender, EventArgs e)
        {
            ClearData();
        }

        private void ClearData()
        {
            _componentIdToDefaultProfileId.Clear();
            _profileIdToComponentId.Clear();
            _setAllDefaults = false;
        }
    }
}
