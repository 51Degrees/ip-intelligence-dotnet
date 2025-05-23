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

using FiftyOne.IpIntelligence.TestHelpers;
using FiftyOne.IpIntelligence.TestHelpers.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FiftyOne.IpIntelligence.OnPremise.Tests.Data
{
    public class MetaDataHasher : IMetaDataHasher
    {
        public int HashProperties(int hash, IWrapper wrapper)
        {
            foreach (var property in wrapper.Properties
                    .Where((p, i) => { return i % 10 == 0; }))
            {
                hash ^= property.GetHashCode();

                foreach (var value in property.GetValues()
                    .Where((v, i) => { return i % 10 == 0; }))
                {
                    hash ^= value.GetHashCode();
                }
                hash ^= property.Component.GetHashCode();
                if (property.DefaultValue != null)
                {
                    hash ^= property.DefaultValue.GetHashCode();
                }
            }
            return hash;
        }

        public int HashValues(int hash, IWrapper wrapper)
        {
            // For a big data file, this is a very time consuming task
            // as it requires a linear search through the values list
            // Therefore skip, then take a good amount of data and perform
            // hash against them. This should ensure that the integrity
            // of the data file is reasonably verified.
            // TODO: To move the iteration logic to the C layer
            foreach (var value in wrapper.Values.Skip(100000).Take(100000))
            {
                hash ^= value.GetHashCode();
                hash ^= value.GetProperty() == null ? 0 : value.GetProperty().GetHashCode();
                // Dispose value
                value.Dispose();
            }
            return hash;
        }

        public int HashComponents(int hash, IWrapper wrapper)
        {
            foreach (var component in wrapper.Components)
            {
                hash ^= component.GetHashCode();
                foreach (var property in component.Properties.
                    Where((p, i) => { return i % 10 == 0; }))
                {
                    hash ^= property.GetHashCode();
                }
                hash ^= component.DefaultProfile == null ? 0 : component.DefaultProfile.GetHashCode();
            }
            return hash;
        }

        public int HashProfiles(int hash, IWrapper wrapper)
        {
            // For a big data file, this is a very time consuming task
            // as it requires a linear search through the profiles list
            // Therefore skip, then take a good amount of data and perform
            // hash against them. This should ensure that the integrity
            // of the data file is reasonably verified.
            // TODO: To move the iteration logic to the C layer
            foreach (var profile in wrapper.Profiles.Skip(100000).Take(50000))
            {
                hash ^= profile.GetHashCode();
                hash ^= profile.Component.GetHashCode();
                foreach (var value in profile.GetValues()
                    .Where((v, i) => { return i % 10 == 0; }))
                {
                    hash ^= value.GetHashCode();
                    // Dispose value
                    value.Dispose();
                }
                // Dispose profile
                profile.Dispose();
            }
            return hash;
        }
    }
}
