﻿using GeoGen.Core;
using GeoGen.Utilities;
using System.Linq;

namespace GeoGen.Generator
{
    /// <summary>
    /// Represents a <see cref="Configuration"/> that was generated by the <see cref="Generator"/>.
    /// </summary>
    public class GeneratedConfiguration : Configuration
    {
        #region Public properties

        /// <summary>
        /// Gets or sets the previous configuration that was extended to obtain
        /// this one. This value will be null for the initial configuration.
        /// </summary>
        public GeneratedConfiguration PreviousConfiguration { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratedConfiguration"/> class 
        /// using a configuration that was extended and the object that should be added to it.
        /// </summary>
        /// <param name="currentConfiguration">The configuration that was extended.</param>
        /// <param name="newObject">The new object with which this configuration was extended.</param>
        public GeneratedConfiguration(GeneratedConfiguration currentConfiguration, ConstructedConfigurationObject newObject)
            : base(currentConfiguration.LooseObjectsHolder, currentConfiguration.ConstructedObjects.Concat(newObject.AsEnumerable()).ToList())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratedConfiguration"/> class 
        /// representing a configuration with no previous configuration.
        /// </summary>
        /// <param name="configuration">The configuration to be wrapped by this object.</param>
        public GeneratedConfiguration(Configuration configuration)
            : base(configuration.LooseObjectsHolder, configuration.ConstructedObjects)
        {
        }

        #endregion
    }
}