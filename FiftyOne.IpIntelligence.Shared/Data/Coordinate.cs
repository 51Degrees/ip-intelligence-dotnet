using System;
using System.Collections.Generic;
using System.Text;

namespace FiftyOne.IpIntelligence.Shared.Data
{
    /// <summary>
    /// Stub for compilation
    /// </summary>
    public class Coordinate
    {
        /// <summary>
        /// latitude
        /// </summary>
        public float Latitude => _lat;

        /// <summary>
        /// longitude
        /// </summary>
        public float Longitude => _lon;

        private readonly float _lat;
        private readonly float _lon;

        /// <summary>
        /// Designated constructor
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        public Coordinate(float lat, float lon)
        {
            _lat = lat;
            _lon = lon;
        }
    }
}
