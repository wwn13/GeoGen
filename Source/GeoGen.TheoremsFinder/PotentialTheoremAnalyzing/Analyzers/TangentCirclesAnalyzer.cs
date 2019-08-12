﻿using GeoGen.AnalyticGeometry;
using GeoGen.Constructor;
using GeoGen.Core;
using GeoGen.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace GeoGen.TheoremsFinder
{
    /// <summary>
    /// An <see cref="IPotentialTheoremsAnalyzer"/> for <see cref="TheoremType.TangentCircles"/>.
    /// </summary>
    public class TangentCirclesAnalyzer : PotentialTheoremsAnalyzerBase
    {
        /// <summary>
        /// Finds all potential (unverified) theorems in a given contextual picture.
        /// </summary>
        /// <param name="contextualPicture">The picture from which we get the actual geometric objects.</param>
        /// <returns>An enumerable of found potential theorems.</returns>
        public override IEnumerable<PotentialTheorem> FindPotentialTheorems(ContextualPicture contextualPicture)
        {
            // Find new circles. At least one of them must be included in every new theorem
            var newCircles = contextualPicture.GetGeometricObjects<CircleObject>(new ContextualPictureQuery
            {
                Type = ContextualPictureQuery.ObjectsType.New,
                IncludeCirces = true
            }).ToList();

            // Find old circles.
            var oldCircles = contextualPicture.GetGeometricObjects<CircleObject>(new ContextualPictureQuery
            {
                Type = ContextualPictureQuery.ObjectsType.Old,
                IncludeCirces = true
            }).ToList();

            // A local helper function for getting all the pairs of 
            // circles where at least of them is new
            IEnumerable<(CircleObject, CircleObject)> NewPairOfCircles()
            {
                // First combine the new circles with themselves
                foreach (var pairOfCircles in newCircles.UnorderedPairs())
                    yield return pairOfCircles;

                // Now combine the new circles with just the old ones
                foreach (var newCircle in newCircles)
                    foreach (var oldCircle in oldCircles)
                        yield return (newCircle, oldCircle);
            }

            // Go through all the possible combinations
            foreach (var (circle1, circle2) in NewPairOfCircles())
            {
                // Construct the verifier function
                bool Verify(Picture picture)
                {
                    // Cast the circles to their analytic versions
                    var analyticCircle1 = contextualPicture.GetAnalyticObject<Circle>(circle1, picture);
                    var analyticCircle2 = contextualPicture.GetAnalyticObject<Circle>(circle2, picture);

                    // Return if there are tangent to each other
                    return analyticCircle1.IsTangentTo(analyticCircle2);
                }

                // Lazily return the output
                yield return new PotentialTheorem
                {
                    // Set the type using the base property
                    TheoremType = Type,

                    // Set the function
                    VerificationFunction = Verify,

                    // Set the involved objects to our two circles
                    InvolvedObjects = new[] { circle1, circle2 }
                };
            }
        }
    }
}