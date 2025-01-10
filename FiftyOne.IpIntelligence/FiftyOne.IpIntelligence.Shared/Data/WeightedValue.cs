using System;
using System.Collections.Generic;
using System.Text;

namespace FiftyOne.IpIntelligence.Shared.Data
{
    /// <summary>
    /// Stub for compilation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WeightedValue<T>
    {
        /// <summary>
        /// Value
        /// </summary>
        public T Value => _value;

        /// <summary>
        /// Weight
        /// </summary>
        public float Weight => _weight;

        private readonly T _value;
        private readonly float _weight;

        /// <summary>
        /// Designated constructor
        /// </summary>
        /// <param name="value"></param>
        /// <param name="weight"></param>
        public WeightedValue(T value, float weight)
        {
            _value = value;
            _weight = weight;
        }
    }
}
