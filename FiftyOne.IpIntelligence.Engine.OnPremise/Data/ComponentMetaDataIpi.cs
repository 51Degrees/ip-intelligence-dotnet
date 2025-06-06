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

using FiftyOne.IpIntelligence.Shared.Data;
using FiftyOne.Pipeline.Engines.FiftyOne.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace FiftyOne.IpIntelligence.Engine.OnPremise.Data
{
    /// <summary>
    /// Data class that contains meta-data relating to a specific 
    /// component.
    /// A component is a logical group of properties for example, all
    /// properties relating to the autonomous system number or all properties
    /// relating to the internet service provider.
    /// This variation is used for meta-data that is generated by the
    /// c# code as opposed to the <see cref="ComponentMetaData"/> class
    /// that is used for meta-data generated from the native C/C++ code.
    /// For example, this is used to add placeholder component meta-data 
    /// for the 'metrics' properties because they don't have any associated
    /// meta-data in the data file.
    /// </summary>
    public class ComponentMetaDataIpi : ComponentMetaDataDefault
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">
        /// The name of this component
        /// </param>
        public ComponentMetaDataIpi(string name) : base(name)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">
        /// The name of this component
        /// </param>
        /// <param name="properties">
        /// The meta-data for the properties associated with this
        /// component.
        /// </param>
        public ComponentMetaDataIpi(string name,
            List<IFiftyOneAspectPropertyMetaData> properties) :
            base(name, properties)
        {
        }
    }
}
