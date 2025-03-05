/* *********************************************************************
 * This Original Work is copyright of 51 Degrees Mobile Experts Limited.
 * Copyright 2020 51 Degrees Mobile Experts Limited, 5 Charlotte Close,
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

using FiftyOne.IpIntelligence.Engine.OnPremise.Interop;
using FiftyOne.IpIntelligence.Shared.Data;
using FiftyOne.Pipeline.Core.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FiftyOne.IpIntelligence.Engine.OnPremise.Wrappers
{
    internal class WeightedCoordinateListSwigWrapper : IReadOnlyList<IWeightedValue<Coordinate>>
    {
        private WeightedCoordinateListSwig _object;

        public IWeightedValue<Coordinate> this[int index] => GetWeightedValue(index);

        public int Count => _object.Count;

        public WeightedCoordinateListSwigWrapper(
            WeightedCoordinateListSwig instance)
        {
            _object = instance;
        }

        private IWeightedValue<Coordinate> GetWeightedValue(int index)
        {
            using (var coordinateSwig = this._object[index].getValue())
            {
                var coordinate = new Coordinate(coordinateSwig.lat, coordinateSwig.lon);
                return new WeightedValue<Coordinate>(this._object[index].getRawWeight(), coordinate);
            }
        }

        public IEnumerator<IWeightedValue<Coordinate>> GetEnumerator()
        {
            for (int i = 0; i < _object.Count; i++)
            {
                yield return GetWeightedValue(i);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator(); ;
        }
    }
}
