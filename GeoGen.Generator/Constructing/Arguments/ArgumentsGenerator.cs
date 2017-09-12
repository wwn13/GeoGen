﻿using System;
using System.Collections.Generic;
using System.Linq;
using GeoGen.Core.Configurations;
using GeoGen.Core.Utilities.Combinator;
using GeoGen.Core.Utilities.Variations;
using GeoGen.Generator.ConfigurationHandling.ConfigurationsContainer;
using GeoGen.Generator.Constructing.Arguments.Container;
using GeoGen.Generator.Constructing.Arguments.SignatureMatching;

namespace GeoGen.Generator.Constructing.Arguments
{
    /// <summary>
    /// A default implementation of <see cref="IArgumentsGenerator"/>.
    /// </summary>
    internal class ArgumentsGenerator : IArgumentsGenerator
    {
        #region Private fields

        /// <summary>
        /// The construction signature matcher factory.
        /// </summary>
        private readonly IConstructionSignatureMatcherFactory _constructionSignatureMatcherFactory;

        /// <summary>
        /// The arguments container factory.
        /// </summary>
        private readonly IArgumentsContainerFactory _argumentsContainerFactory;

        /// <summary>
        /// The variations of configuration objects provider.
        /// </summary>
        private readonly IVariationsProvider<ConfigurationObject> _variationsProvider;

        /// <summary>
        /// The combinator of lists of configuration object of distint types.
        /// </summary>
        private readonly ICombinator<ConfigurationObjectType, List<ConfigurationObject>> _combinator;

        #endregion

        #region Constructor

        /// <summary>
        /// Construct a new arguments generator. The generator uses combinator and 
        /// variations provider to create all possible sets of objects to be passed
        /// as arguments. These arguments are created by construction signature matcher factory
        /// and then kept in an arguments container so that they we provide only
        /// distinct set of arguments.
        /// </summary>
        /// <param name="combinator">The combinator.</param>
        /// <param name="constructionSignatureMatcherFactory">The construction signature matcher factory.</param>
        /// <param name="variationsProvider">The variations provider.</param>
        /// <param name="argumentsContainerFactory">The arguments container factory.</param>
        public ArgumentsGenerator(ICombinator<ConfigurationObjectType, List<ConfigurationObject>> combinator,
            IConstructionSignatureMatcherFactory constructionSignatureMatcherFactory,
            IVariationsProvider<ConfigurationObject> variationsProvider,
            IArgumentsContainerFactory argumentsContainerFactory)
        {
            _constructionSignatureMatcherFactory = constructionSignatureMatcherFactory ?? throw new ArgumentNullException(nameof(constructionSignatureMatcherFactory));
            _argumentsContainerFactory = argumentsContainerFactory ?? throw new ArgumentNullException(nameof(argumentsContainerFactory));
            _variationsProvider = variationsProvider ?? throw new ArgumentNullException(nameof(variationsProvider));
            _combinator = combinator ?? throw new ArgumentNullException(nameof(combinator));
        }

        #endregion

        #region IArgumentsGenerator implementation

        /// <summary>
        /// Generates a container of all possible distinct arguments that can be passed to 
        /// a given construction, using object from a given configuration.
        /// </summary>
        /// <param name="configuration">The wrapper cofiguration.</param>
        /// <param name="construction">The wrapped construction.</param>
        /// <returns>The container of resulting arguments.</returns>
        public IArgumentsContainer GenerateArguments(ConfigurationWrapper configuration, ConstructionWrapper construction)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if (construction == null)
                throw new ArgumentNullException(nameof(construction));

            // First we check if we can even perform the construction. Whether there are enough
            // objects to do so. If not, we return an empty enumerable.
            if (!CanWePerformConstruction(configuration, construction))
            {
                return _argumentsContainerFactory.CreateContainer();
            }

            var dictionaryForCombinator = configuration.ObjectTypeToObjects.Where
                    (
                        pair => construction.ObjectTypesToNeededCount.ContainsKey(pair.Key)
                    )
                    .ToDictionary
                    (
                        keyValue => keyValue.Key,
                        keyValue => _variationsProvider
                                .GetVariations(keyValue.Value, construction.ObjectTypesToNeededCount[keyValue.Key])
                                .Select(variation => variation.ToList())
                    );

            var argumentsContainer = _argumentsContainerFactory.CreateContainer();
            var signatureMatcher = _constructionSignatureMatcherFactory.CreateMatcher();

            foreach (var dictonaryForMatcher in _combinator.Combine(dictionaryForCombinator))
            {
                // Initialize the signature matcher with a new object dictionary
                signatureMatcher.Initialize(dictonaryForMatcher);

                // Take the parameters from the construction
                var parameters = construction.Construction.ConstructionParameters;

                // Let the matcher match the parameters to obtain arguments
                var arguments = signatureMatcher.Match(parameters);

                // Add arguments to the container
                argumentsContainer.AddArguments(arguments);
            }

            return argumentsContainer;
        }

        /// <summary>
        /// Returns if we can perform the currently examined construction according to
        /// the given dictonaries.
        /// </summary>
        /// <returns>true, if there is no object type (such as Point) from which we have fewer
        /// objects than we need to construction.</returns>
        private static bool CanWePerformConstruction(ConfigurationWrapper configuration, ConstructionWrapper construction)
        {
            foreach (var pair in construction.ObjectTypesToNeededCount)
            {
                var numberOfElementsInArguments = pair.Value;

                if (!configuration.ObjectTypeToObjects.ContainsKey(pair.Key))
                    return false;

                var realObjects = configuration.ObjectTypeToObjects[pair.Key];
                var numberOfRealObjects = realObjects.Count();

                // if there are more neeed arguments than available objects, 
                // then we can't perform the construction
                if (numberOfElementsInArguments > numberOfRealObjects)
                    return false;
            }

            return true;
        }

        #endregion
    }
}