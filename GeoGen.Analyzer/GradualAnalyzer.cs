﻿using System;
using System.Collections.Generic;
using System.Linq;
using GeoGen.Analyzer.Objects;
using GeoGen.Analyzer.Theorems;
using GeoGen.Core.Configurations;
using GeoGen.Core.Theorems;
using GeoGen.Core.Utilities;

namespace GeoGen.Analyzer
{
    /// <summary>
    /// A default implementation of <see cref="IGradualAnalyzer"/>.
    /// This class is thread-safe.
    /// </summary>
    internal sealed class GradualAnalyzer : IGradualAnalyzer
    {
        #region Private fields

        private readonly IGeometryRegistrar _registrar;

        private readonly ITheoremsContainer _container;

        private readonly ITheoremsVerifier _verifier;

        public GradualAnalyzer(IGeometryRegistrar registrar, ITheoremsContainer container, ITheoremsVerifier verifier)
        {
            _registrar = registrar;
            _container = container;
            _verifier = verifier;
        }

        #endregion

        #region IGradualAnalyzer methods

        /// <summary>
        /// Analyses a given configuration provided as two lists, or old and new configuration
        /// objects.
        /// </summary>
        /// <param name="oldObjects">The old objects.</param>
        /// <param name="newObjects">The new objects.</param>
        /// <returns>The analyzer output.</returns>
        public GradualAnalyzerOutput Analyze(List<ConfigurationObject> oldObjects, List<ConstructedConfigurationObject> newObjects)
        {
            if (oldObjects == null)
                throw new ArgumentNullException(nameof(oldObjects));

            if (newObjects == null)
                throw new ArgumentNullException(nameof(newObjects));

            var result = _registrar.Register(newObjects);

            var duplicateObjects = result.GeometricalDuplicates;
            var canBeConstructed = result.CanBeConstructed;
            var theorems = new List<Theorem>();

            var unambiguouslyConstructible = duplicateObjects.Empty() && canBeConstructed;

            if (unambiguouslyConstructible)
            {
                var oldObjectsMap = new ConfigurationObjectsMap(oldObjects);
                var newObjectsMap = new ConfigurationObjectsMap(newObjects);

                var newTheorems = _verifier.FindTheorems(oldObjectsMap, newObjectsMap)
                        .Where(theorem => !_container.Contains(theorem));

                theorems.AddRange(newTheorems);
            }

            foreach (var pair in duplicateObjects)
            {
                var newObject = pair.Key;
                var duplicate = pair.Value;

                var involvedObjects = new List<TheoremObject>
                {
                    new TheoremObject(newObject),
                    new TheoremObject(duplicate)
                };

                var theorem = new Theorem(TheoremType.SameObjects, involvedObjects);

                theorems.Add(theorem);
            }

            return new GradualAnalyzerOutput
            {
                Theorems = theorems,
                UnambiguouslyConstructible = unambiguouslyConstructible
            };
        }

        #endregion
    }
}