﻿using System.Collections.Generic;

namespace GeoGen.Core
{
    /// <summary>
    /// Represents an <see cref="IEqualityComparer{T}"/> of configuration objects
    /// that compares them according to their ids.
    /// </summary>
    public class ConfigurationObjectsEqualityComparer : IEqualityComparer<ConfigurationObject>
    {
        #region Singleton instance

        /// <summary>
        /// The singleton instance of this comparer.
        /// </summary>
        public static readonly ConfigurationObjectsEqualityComparer Instance = new ConfigurationObjectsEqualityComparer();

        #endregion

        #region IEqualityComparer implementation

        /// <summary>
        /// Finds out if two given configuration objects are equal.
        /// </summary>
        /// <param name="x">The first configuration object.</param>
        /// <param name="y">The second configuration object.</param>
        /// <returns>true, if they are equal; false otherwise.</returns>
        public bool Equals(ConfigurationObject x, ConfigurationObject y)
        {
            return x.Id == y.Id;
        }

        /// <summary>
        /// Gets the hash code of a given configuration object.
        /// </summary>
        /// <param name="obj">The theorem object.</param>
        /// <returns>The hash code.</returns>
        public int GetHashCode(ConfigurationObject obj)
        {
            return obj.Id.GetHashCode();
        } 

        #endregion
    }
}
