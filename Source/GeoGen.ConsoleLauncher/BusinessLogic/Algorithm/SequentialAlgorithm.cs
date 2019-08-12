﻿using GeoGen.Constructor;
using GeoGen.Core;
using GeoGen.Generator;
using GeoGen.TheoremsAnalyzer;
using GeoGen.TheoremsFinder;
using GeoGen.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeoGen.ConsoleLauncher
{
    /// <summary>
    /// Represents a simple version of the algorithm where each configuration is tested 
    /// for theorems immediately after it's generated.
    /// </summary>
    public class SequentialAlgorithm : IAlgorithm
    {
        #region Dependencies

        /// <summary>
        /// The generator of configurations.
        /// </summary>
        private readonly IGenerator _generator;

        /// <summary>
        /// The finder of theorems in generated configurations.
        /// </summary>
        private readonly IRelevantTheoremsAnalyzer _finder;

        /// <summary>
        /// The factory for creating contextual pictures.
        /// </summary>
        private readonly IContextualPictureFactory _pictureFactory;

        /// <summary>
        /// The analyzer of theorem providing feedback whether they are olympiad or not.
        /// </summary>
        private readonly ITheoremsAnalyzer _analyzer;

        /// <summary>
        /// The factory for creating objects containers.
        /// </summary>
        private readonly IConfigurationObjectsContainerFactory _containerFactory;

        private readonly IPicturesFactory _factory;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequentialAlgorithm"/> class.
        /// </summary>
        /// <param name="generator">The generator of configurations.</param>
        /// <param name="finder">The finder of theorems in generated configurations.</param>
        /// <param name="pictureFactory">The factory for creating contextual pictures.</param>
        /// <param name="analyzer">The analyzer of theorem providing feedback whether they are olympiad or not.</param>
        /// <param name="containerFactory">The factory for creating objects containers.</param>
        public SequentialAlgorithm(IGenerator generator, IRelevantTheoremsAnalyzer finder, IContextualPictureFactory pictureFactory, ITheoremsAnalyzer analyzer, IConfigurationObjectsContainerFactory containerFactory)
        {
            _generator = generator ?? throw new ArgumentNullException(nameof(generator));
            _finder = finder ?? throw new ArgumentNullException(nameof(finder));
            _pictureFactory = pictureFactory ?? throw new ArgumentNullException(nameof(pictureFactory));
            _analyzer = analyzer ?? throw new ArgumentNullException(nameof(analyzer));
            _containerFactory = containerFactory ?? throw new ArgumentNullException(nameof(containerFactory));
        }

        #endregion

        #region IAlgorithm implementation

        /// <summary>
        /// Executes the algorithm for a given generator input.
        /// </summary>
        /// <param name="input">The input for the algorithm.</param>
        /// <returns>A lazy enumerable of all the generated output.</returns>
        public IEnumerable<AlgorithmOutput> GenerateOutputs(GeneratorInput input)
        {
            // Prepare the map for pictures
            var picturesMap = new Dictionary<Configuration, ContextualPicture>();

            var first = false;

            // Perform the generation
            return _generator.Generate(input).Select(output =>
            {
                // Get the configuration for comfort
                var configuration = output.Configuration;

                // Prepare a picture
                var picture = default(ContextualPicture);

                // If this is the initial configuration
                if (configuration.PreviousConfiguration == null)
                //if(true)
                {
                  // Safely execute
                  picture = GeneralUtilities.TryExecute(
                       // Creating of the picture
                       () => _pictureFactory.Create(output.Manager),
                       // While ignoring potential issues (such a configuration will be discarded anyway)
                       (InconstructibleContextualPicture _) => { });
                }
                // If this is not an initial one
                else
                {
                    // Get the cached picture from the map
                    picture = picturesMap[configuration.PreviousConfiguration].ConstructByCloning(output.Manager);
                }
                
                // Add it to the map
                picturesMap.Add(configuration, picture);

                // Return the output together with the constructed picture
                return (output, picture);
            })
            // Take only such pairs where the picture was successfully created
            .Where(pair => pair.picture != null)
            // Skip the initial one
            .Where(pair =>
            {
                if (!first)
                {
                    first = true;
                    return false;
                }

                return true;
            })
            // For each such pair perform the theorem analysis
            .Select(pair =>
            {
                // Deconstruct
                var (output, picture) = pair;

                // Find theorems
                var theorems = _finder.Analyze(output.Configuration, output.Manager, picture);

                // Create a container holding the objects of the configuration
                var container = _containerFactory.CreateContainer(output.Configuration);

                // Analyze them
                var analysisResult = _analyzer.Analyze(new TheoremAnalyzerInput
                {
                    Configuration = output.Configuration,
                    ContextualPicture = picture,
                    Manager = output.Manager,
                    Theorems = theorems,
                    ConfigurationObjectsContainer = container
                });

                // Return the final output
                return new AlgorithmOutput
                {
                    GeneratorOutput = output,
                    Theorems = theorems,
                    AnalyzerOutput = analysisResult
                };
            });
        }

        #endregion
    }
}
