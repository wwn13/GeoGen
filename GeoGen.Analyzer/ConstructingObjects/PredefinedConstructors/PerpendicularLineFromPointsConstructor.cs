﻿using System;
using System.Collections.Generic;
using System.Linq;
using GeoGen.AnalyticalGeometry;
using GeoGen.Core;

namespace GeoGen.Analyzer
{
    /// <summary>
    /// An <see cref="IObjectsConstructor"/> for <see cref="PredefinedConstructionType.PerpendicularLineFromPoints"/>>.
    /// </summary>
    internal class PerpendicularLineFromPointsConstructor : PredefinedConstructorBase
    {
        /// <summary>
        /// Constructs a list of analytical objects from a given list of 
        /// flattened objects from the arguments and a container that is used to 
        /// obtain the actual analytical versions of these objects.
        /// </summary>
        /// <param name="flattenedObjects">The flattened argument objects.</param>
        /// <param name="container">The objects container.</param>
        /// <returns>The list of constructed analytical objects.</returns>
        protected override List<AnalyticalObject> Construct(List<ConfigurationObject> flattenedObjects, IObjectsContainer container)
        {
            // Pull line points
            var linePoint1 = container.Get<Point>(flattenedObjects[1]);
            var linePoint2 = container.Get<Point>(flattenedObjects[2]);

            // Pull the point from which we erect the perpendicular like
            var pointFrom = container.Get<Point>(flattenedObjects[0]);

            try
            {
                // Construct the line (this might throw an AnalyticalException)
                var line = new Line(linePoint1, linePoint2);

                // Construct the result
                return new List<AnalyticalObject> {line.PerpendicularLine(pointFrom)};
            }
            catch (AnalyticalException)
            {
                // Return null in case the construction has failed
                return null;
            }
        }

        /// <summary>
        /// Constructs a list of default theorems using a newly constructed objects and
        /// flattened objects from the passed arguments.
        /// </summary>
        /// <param name="input">The constructed objects.</param>
        /// <param name="flattenedObjects">The flattened argument objects.</param>
        /// <returns>The list of default theorems.</returns>
        protected override List<Theorem> FindDefaultTheorms(List<ConstructedConfigurationObject> input, List<ConfigurationObject> flattenedObjects)
        {
            return new List<Theorem>();
        }
    }
}