﻿using FluentAssertions;
using GeoGen.Constructor;
using GeoGen.Core;
using GeoGen.DependenciesResolver;
using GeoGen.Utilities;
using Ninject;
using NUnit.Framework;
using System.Linq;
using static GeoGen.Core.ComposedConstructions;
using static GeoGen.Core.ConfigurationObjectType;
using static GeoGen.Core.LooseObjectsLayout;
using static GeoGen.Core.PredefinedConstructions;
using static GeoGen.Core.TheoremType;

namespace GeoGen.TheoremFinder.Tests
{
    /// <summary>
    /// The test class for <see cref="TheoremFinder"/>.
    /// </summary>
    [TestFixture]
    public class TheoremFinderTest
    {
        #region Running algorithm

        /// <summary>
        /// Runs the algorithm on the configuration to find new and all theorems. 
        /// </summary>
        /// <param name="configuration">The configuration where we're looking for theorems.</param>
        /// <returns>The old, new and all theorems.</returns>
        private static (TheoremMap oldTheorems, TheoremMap newTheorems, TheoremMap allTheorems) FindTheorems(Configuration configuration)
        {
            // Prepare the kernel
            var kernel = IoC.CreateKernel().AddConstructor(new PicturesSettings
            {
                NumberOfPictures = 5,
                MaximalAttemptsToReconstructAllPictures = 0,
                MaximalAttemptsToReconstructOnePicture = 0
            })
            // Look for only some types
            .AddTheoremFinder(new TangentCirclesTheoremFinderSettings
            {
                // Don't exclude tangencies
                ExcludeTangencyInsidePicture = false
            },
            // We don't want line and circle tangencies
            lineTangentToCirclesFinderSettings: null,
            // The wanted types:
            types: new[]
            {
                ParallelLines,
                PerpendicularLines,
                EqualLineSegments,
                TangentCircles,
                Incidence
            }
            // As a set
            .ToReadOnlyHashSet());

            // Create the finder
            var finder = kernel.Get<ITheoremFinder>();

            // Create the old configuration, so we can find its theorems
            var oldConfiguration = new Configuration(configuration.LooseObjectsHolder,
                // We just don't want to include the last object
                configuration.ConstructedObjects.Except(new[] { configuration.LastConstructedObject }).ToList());

            // Construct the old configuration
            var oldPictures = kernel.Get<IGeometryConstructor>().Construct(oldConfiguration).pictures;

            // Construct the old contextual picture
            var oldContextualPicture = kernel.Get<IContextualPictureFactory>().CreateContextualPicture(oldPictures);

            // Finally get the old theorems
            var oldTheorems = finder.FindAllTheorems(oldContextualPicture);

            // Create the pictures for the current configuration
            var pictures = kernel.Get<IGeometryConstructor>().Construct(configuration).pictures;

            // Create the contextual picture for the current configuration
            var contextualPicture = kernel.Get<IContextualPictureFactory>().CreateContextualPicture(pictures);

            // Run both algorithms
            var newTheorems = finder.FindNewTheorems(contextualPicture, oldTheorems);
            var allTheorems = finder.FindAllTheorems(contextualPicture);

            // Return them for further assertions
            return (oldTheorems, newTheorems, allTheorems);
        }

        #endregion

        [Test]
        public void Test_Orthocenter_And_Intersection()
        {
            // Create the objects
            var A = new LooseConfigurationObject(Point);
            var B = new LooseConfigurationObject(Point);
            var C = new LooseConfigurationObject(Point);
            var H = new ConstructedConfigurationObject(Orthocenter, A, B, C);
            var D = new ConstructedConfigurationObject(IntersectionOfLinesFromPoints, B, C, A, H);

            // Create the configuration
            var configuration = Configuration.DeriveFromObjects(ThreePoints, H, D);

            // Run
            var (oldTheorems, newTheorems, allTheorems) = FindTheorems(configuration);

            // Assert that new + old = all
            newTheorems.AllObjects.Concat(oldTheorems.AllObjects).OrderlessEquals(allTheorems.AllObjects).Should().BeTrue();

            // Assert new theorems
            newTheorems.AllObjects.OrderlessEquals(new[]
            {
                new Theorem(configuration, PerpendicularLines, new[]
                {
                    new LineTheoremObject(D, H),
                    new LineTheoremObject(D, B)
                }),
                new Theorem(configuration, PerpendicularLines, new[]
                {
                    new LineTheoremObject(D, H),
                    new LineTheoremObject(D, C)
                }),
                new Theorem(configuration, PerpendicularLines, new[]
                {
                    new LineTheoremObject(D, H),
                    new LineTheoremObject(B, C)
                }),
                new Theorem(configuration, PerpendicularLines, new[]
                {
                    new LineTheoremObject(D, A),
                    new LineTheoremObject(D, B)
                }),
                new Theorem(configuration, PerpendicularLines, new[]
                {
                    new LineTheoremObject(D, A),
                    new LineTheoremObject(D, C)
                }),
                new Theorem(configuration, PerpendicularLines, new[]
                {
                    new LineTheoremObject(D, A),
                    new LineTheoremObject(B, C)
                }),
                new Theorem(configuration, PerpendicularLines, new[]
                {
                    new LineTheoremObject(A, H),
                    new LineTheoremObject(D, B)
                }),
                new Theorem(configuration, PerpendicularLines, new[]
                {
                    new LineTheoremObject(A, H),
                    new LineTheoremObject(D, C)
                })
            })
            .Should().BeTrue();

            // Assert all theorems
            allTheorems.AllObjects.OrderlessEquals(new[]
            {
                new Theorem(configuration, PerpendicularLines, new[]
                {
                    new LineTheoremObject(D, H),
                    new LineTheoremObject(D, B)
                }),
                new Theorem(configuration, PerpendicularLines, new[]
                {
                    new LineTheoremObject(D, H),
                    new LineTheoremObject(D, C)
                }),
                new Theorem(configuration, PerpendicularLines, new[]
                {
                    new LineTheoremObject(D, H),
                    new LineTheoremObject(B, C)
                }),
                new Theorem(configuration, PerpendicularLines, new[]
                {
                    new LineTheoremObject(D, A),
                    new LineTheoremObject(D, B)
                }),
                new Theorem(configuration, PerpendicularLines, new[]
                {
                    new LineTheoremObject(D, A),
                    new LineTheoremObject(D, C)
                }),
                new Theorem(configuration, PerpendicularLines, new[]
                {
                    new LineTheoremObject(D, A),
                    new LineTheoremObject(B, C)
                }),
                new Theorem(configuration, PerpendicularLines, new[]
                {
                    new LineTheoremObject(A, H),
                    new LineTheoremObject(D, B)
                }),
                new Theorem(configuration, PerpendicularLines, new[]
                {
                    new LineTheoremObject(A, H),
                    new LineTheoremObject(D, C)
                }),
                new Theorem(configuration, PerpendicularLines, new[]
                {
                    new LineTheoremObject(H, A),
                    new LineTheoremObject(B, C)
                }),
                new Theorem(configuration, PerpendicularLines, new[]
                {
                    new LineTheoremObject(H, B),
                    new LineTheoremObject(A, C)
                }),
                new Theorem(configuration, PerpendicularLines, new[]
                {
                    new LineTheoremObject(H, C),
                    new LineTheoremObject(B, A)
                })
            })
            .Should().BeTrue();
        }

        [Test]
        public void Test_Triangle_With_Midpoints_And_Explicit_Line_At_The_End()
        {
            // Create the objects
            var A = new LooseConfigurationObject(Point);
            var B = new LooseConfigurationObject(Point);
            var C = new LooseConfigurationObject(Point);
            var D = new ConstructedConfigurationObject(Midpoint, A, B);
            var E = new ConstructedConfigurationObject(Midpoint, A, C);
            var F = new ConstructedConfigurationObject(Midpoint, D, E);
            var l = new ConstructedConfigurationObject(LineFromPoints, B, C);

            // Create the configuration
            var configuration = Configuration.DeriveFromObjects(ThreePoints, D, E, F, l);

            // Run
            var (oldTheorems, newTheorems, allTheorems) = FindTheorems(configuration);

            // Assert that new + old = all
            newTheorems.AllObjects.Concat(oldTheorems.AllObjects).OrderlessEquals(allTheorems.AllObjects).Should().BeTrue();

            // Assert new theorems
            newTheorems.AllObjects.OrderlessEquals(new[]
            {
                new Theorem(configuration, ParallelLines, new[]
                {
                    new LineTheoremObject(l),
                    new LineTheoremObject(D, E)
                }),
                new Theorem(configuration, ParallelLines, new[]
                {
                    new LineTheoremObject(l),
                    new LineTheoremObject(D, F)
                }),
                new Theorem(configuration, ParallelLines, new[]
                {
                    new LineTheoremObject(l),
                    new LineTheoremObject(E, F)
                }),
                new Theorem(configuration, Incidence, B, l),
                new Theorem(configuration, Incidence, C, l)
            })
            .Should().BeTrue();

            // Assert all theorems
            allTheorems.AllObjects.OrderlessEquals(new[]
            {
                new Theorem(configuration, ParallelLines, new[]
                {
                    new LineTheoremObject(l),
                    new LineTheoremObject(D, E)
                }),
                new Theorem(configuration, ParallelLines, new[]
                {
                    new LineTheoremObject(l),
                    new LineTheoremObject(D, F)
                }),
                new Theorem(configuration, ParallelLines, new[]
                {
                    new LineTheoremObject(l),
                    new LineTheoremObject(E, F)
                }),
                new Theorem(configuration, ParallelLines, new[]
                {
                    new LineTheoremObject(B, C),
                    new LineTheoremObject(D, E)
                }),
                new Theorem(configuration, ParallelLines, new[]
                {
                    new LineTheoremObject(B, C),
                    new LineTheoremObject(D, F)
                }),
                new Theorem(configuration, ParallelLines, new[]
                {
                    new LineTheoremObject(B, C),
                    new LineTheoremObject(E, F)
                }),
                new Theorem(configuration, TangentCircles, new[]
                {
                    new CircleTheoremObject(A, D, E),
                    new CircleTheoremObject(A, B, C)
                }),
                new Theorem(configuration, EqualLineSegments, new[]
                {
                    new LineSegmentTheoremObject(A, D),
                    new LineSegmentTheoremObject(B, D)
                }),
                new Theorem(configuration, EqualLineSegments, new[]
                {
                    new LineSegmentTheoremObject(A, E),
                    new LineSegmentTheoremObject(C, E)
                }),
                new Theorem(configuration, EqualLineSegments, new[]
                {
                    new LineSegmentTheoremObject(D, F),
                    new LineSegmentTheoremObject(E, F)
                }),
                new Theorem(configuration, Incidence, B, l),
                new Theorem(configuration, Incidence, C, l)
            })
            .Should().BeTrue();
        }

        [Test]
        public void Test_Triangle_With_Midpoints_And_Point_At_The_End()
        {
            // Create the objects
            var A = new LooseConfigurationObject(Point);
            var B = new LooseConfigurationObject(Point);
            var C = new LooseConfigurationObject(Point);
            var D = new ConstructedConfigurationObject(Midpoint, A, B);
            var E = new ConstructedConfigurationObject(Midpoint, A, C);
            var b = new ConstructedConfigurationObject(InternalAngleBisector, A, B, C);
            var P = new ConstructedConfigurationObject(IntersectionOfLineAndLineFromPoints, b, B, C);
            var l = new ConstructedConfigurationObject(LineFromPoints, B, C);
            var F = new ConstructedConfigurationObject(Midpoint, D, E);

            // Create the configuration
            var configuration = Configuration.DeriveFromObjects(ThreePoints, D, E, P, l, F);

            // Run
            var (oldTheorems, newTheorems, allTheorems) = FindTheorems(configuration);

            // Assert that new + old = all
            newTheorems.AllObjects.Concat(oldTheorems.AllObjects).OrderlessEquals(allTheorems.AllObjects).Should().BeTrue();

            // Assert new theorems
            newTheorems.AllObjects.OrderlessEquals(new[]
            {
                new Theorem(configuration, ParallelLines, new[]
                {
                    new LineTheoremObject(F, D),
                    new LineTheoremObject(B, C)
                }),
                new Theorem(configuration, ParallelLines, new[]
                {
                    new LineTheoremObject(F, E),
                    new LineTheoremObject(B, C)
                }),
                new Theorem(configuration, ParallelLines, new[]
                {
                    new LineTheoremObject(F, D),
                    new LineTheoremObject(B, P)
                }),
                new Theorem(configuration, ParallelLines, new[]
                {
                    new LineTheoremObject(F, E),
                    new LineTheoremObject(B, P)
                }),
                new Theorem(configuration, ParallelLines, new[]
                {
                    new LineTheoremObject(F, D),
                    new LineTheoremObject(C, P)
                }),
                new Theorem(configuration, ParallelLines, new[]
                {
                    new LineTheoremObject(F, E),
                    new LineTheoremObject(C, P)
                }),
                new Theorem(configuration, ParallelLines, new[]
                {
                    new LineTheoremObject(F, D),
                    new LineTheoremObject(l)
                }),
                new Theorem(configuration, ParallelLines, new[]
                {
                    new LineTheoremObject(F, E),
                    new LineTheoremObject(l)
                }),
                new Theorem(configuration, EqualLineSegments, new[]
                {
                    new LineSegmentTheoremObject(D, F),
                    new LineSegmentTheoremObject(E, F)
                })
            })
            .Should().BeTrue();

            // Assert all theorems
            allTheorems.AllObjects.OrderlessEquals(new[]
            {
                new Theorem(configuration, ParallelLines, new[]
                {
                    new LineTheoremObject(F, D),
                    new LineTheoremObject(B, C)
                }),
                new Theorem(configuration, ParallelLines, new[]
                {
                    new LineTheoremObject(F, E),
                    new LineTheoremObject(B, C)
                }),
                new Theorem(configuration, ParallelLines, new[]
                {
                    new LineTheoremObject(D, E),
                    new LineTheoremObject(B, C)
                }),
                new Theorem(configuration, ParallelLines, new[]
                {
                    new LineTheoremObject(F, D),
                    new LineTheoremObject(B, P)
                }),
                new Theorem(configuration, ParallelLines, new[]
                {
                    new LineTheoremObject(F, E),
                    new LineTheoremObject(B, P)
                }),
                new Theorem(configuration, ParallelLines, new[]
                {
                    new LineTheoremObject(D, E),
                    new LineTheoremObject(B, P)
                }),
                new Theorem(configuration, ParallelLines, new[]
                {
                    new LineTheoremObject(F, D),
                    new LineTheoremObject(C, P)
                }),
                new Theorem(configuration, ParallelLines, new[]
                {
                    new LineTheoremObject(F, E),
                    new LineTheoremObject(C, P)
                }),
                new Theorem(configuration, ParallelLines, new[]
                {
                    new LineTheoremObject(D, E),
                    new LineTheoremObject(C, P)
                }),
                new Theorem(configuration, ParallelLines, new[]
                {
                    new LineTheoremObject(F, D),
                    new LineTheoremObject(l)
                }),
                new Theorem(configuration, ParallelLines, new[]
                {
                    new LineTheoremObject(F, E),
                    new LineTheoremObject(l)
                }),
                new Theorem(configuration, ParallelLines, new[]
                {
                    new LineTheoremObject(D, E),
                    new LineTheoremObject(l)
                }),
                new Theorem(configuration, TangentCircles, new[]
                {
                    new CircleTheoremObject(A, D, E),
                    new CircleTheoremObject(A, B, C)
                }),
                new Theorem(configuration, EqualLineSegments, new[]
                {
                    new LineSegmentTheoremObject(A, D),
                    new LineSegmentTheoremObject(B, D)
                }),
                new Theorem(configuration, EqualLineSegments, new[]
                {
                    new LineSegmentTheoremObject(A, E),
                    new LineSegmentTheoremObject(C, E)
                }),
                new Theorem(configuration, EqualLineSegments, new[]
                {
                    new LineSegmentTheoremObject(D, F),
                    new LineSegmentTheoremObject(E, F)
                }),
                new Theorem(configuration, Incidence, B, l),
                new Theorem(configuration, Incidence, C, l),
                new Theorem(configuration, Incidence, P, l),
                new Theorem(configuration, Incidence, A, b),
                new Theorem(configuration, Incidence, P, b)
            })
            .Should().BeTrue();
        }

        [Test]
        public void Test_Triangle_With_Tangent_Circles_And_Explicit_One_At_The_End()
        {
            // Create the objects
            var A = new LooseConfigurationObject(Point);
            var B = new LooseConfigurationObject(Point);
            var C = new LooseConfigurationObject(Point);
            var D = new ConstructedConfigurationObject(Midpoint, A, B);
            var O = new ConstructedConfigurationObject(Circumcenter, A, B, C);
            var P = new ConstructedConfigurationObject(PointReflection, A, O);
            var c = new ConstructedConfigurationObject(Circumcircle, P, B, C);

            // Create the configuration
            var configuration = Configuration.DeriveFromObjects(ThreePoints, D, O, P, c);

            // Run
            var (oldTheorems, newTheorems, allTheorems) = FindTheorems(configuration);

            // Assert that new + old = all
            newTheorems.AllObjects.Concat(oldTheorems.AllObjects).OrderlessEquals(allTheorems.AllObjects).Should().BeTrue();

            // Assert new theorems
            newTheorems.AllObjects.OrderlessEquals(new[]
            {
                new Theorem(configuration, TangentCircles, new[]
                {
                    new CircleTheoremObject(A, D, O),
                    new CircleTheoremObject(c)
                }),
                new Theorem(configuration, TangentCircles, new[]
                {
                    new CircleTheoremObject(B, D, O),
                    new CircleTheoremObject(c)
                }),
                new Theorem(configuration, Incidence, A, c),
                new Theorem(configuration, Incidence, B, c),
                new Theorem(configuration, Incidence, C, c),
                new Theorem(configuration, Incidence, P, c)
            })
            .Should().BeTrue();

            // Assert all theorems
            allTheorems.AllObjects.OrderlessEquals(new[]
            {
                new Theorem(configuration, ParallelLines, new[]
                {
                    new LineTheoremObject(B, P),
                    new LineTheoremObject(O, D)
                }),
                new Theorem(configuration, EqualLineSegments, new[]
                {
                    new LineSegmentTheoremObject(A, D),
                    new LineSegmentTheoremObject(D, B)
                }),
                new Theorem(configuration, EqualLineSegments, new[]
                {
                    new LineSegmentTheoremObject(O, A),
                    new LineSegmentTheoremObject(O, B)
                }),
                new Theorem(configuration, EqualLineSegments, new[]
                {
                    new LineSegmentTheoremObject(O, A),
                    new LineSegmentTheoremObject(O, C)
                }),
                new Theorem(configuration, EqualLineSegments, new[]
                {
                    new LineSegmentTheoremObject(O, A),
                    new LineSegmentTheoremObject(O, P)
                }),
                new Theorem(configuration, EqualLineSegments, new[]
                {
                    new LineSegmentTheoremObject(O, B),
                    new LineSegmentTheoremObject(O, C)
                }),
                new Theorem(configuration, EqualLineSegments, new[]
                {
                    new LineSegmentTheoremObject(O, B),
                    new LineSegmentTheoremObject(O, P)
                }),
                new Theorem(configuration, EqualLineSegments, new[]
                {
                    new LineSegmentTheoremObject(O, C),
                    new LineSegmentTheoremObject(O, P)
                }),
                new Theorem(configuration, TangentCircles, new[]
                {
                    new CircleTheoremObject(A, D, O),
                    new CircleTheoremObject(A, B, C)
                }),
                new Theorem(configuration, TangentCircles, new[]
                {
                    new CircleTheoremObject(A, D, O),
                    new CircleTheoremObject(A, B, P)
                }),
                new Theorem(configuration, TangentCircles, new[]
                {
                    new CircleTheoremObject(A, D, O),
                    new CircleTheoremObject(A, C, P)
                }),
                new Theorem(configuration, TangentCircles, new[]
                {
                    new CircleTheoremObject(A, D, O),
                    new CircleTheoremObject(B, C, P)
                }),
                new Theorem(configuration, TangentCircles, new[]
                {
                    new CircleTheoremObject(A, D, O),
                    new CircleTheoremObject(c)
                }),
                new Theorem(configuration, TangentCircles, new[]
                {
                    new CircleTheoremObject(B, D, O),
                    new CircleTheoremObject(A, B, C)
                }),
                new Theorem(configuration, TangentCircles, new[]
                {
                    new CircleTheoremObject(B, D, O),
                    new CircleTheoremObject(A, B, P)
                }),
                new Theorem(configuration, TangentCircles, new[]
                {
                    new CircleTheoremObject(B, D, O),
                    new CircleTheoremObject(A, C, P)
                }),
                new Theorem(configuration, TangentCircles, new[]
                {
                    new CircleTheoremObject(B, D, O),
                    new CircleTheoremObject(B, C, P)
                }),
                new Theorem(configuration, TangentCircles, new[]
                {
                    new CircleTheoremObject(B, D, O),
                    new CircleTheoremObject(c)
                }),
                new Theorem(configuration, Incidence, A, c),
                new Theorem(configuration, Incidence, B, c),
                new Theorem(configuration, Incidence, C, c),
                new Theorem(configuration, Incidence, P, c),
                new Theorem(configuration, PerpendicularLines, new[]
                {
                    new LineTheoremObject(O, D),
                    new LineTheoremObject(B, A)
                }),
                new Theorem(configuration, PerpendicularLines, new[]
                {
                    new LineTheoremObject(O, D),
                    new LineTheoremObject(B, D)
                }),
                new Theorem(configuration, PerpendicularLines, new[]
                {
                    new LineTheoremObject(O, D),
                    new LineTheoremObject(A, D)
                }),
                new Theorem(configuration, PerpendicularLines, new[]
                {
                    new LineTheoremObject(P, B),
                    new LineTheoremObject(B, A)
                }),
                new Theorem(configuration, PerpendicularLines, new[]
                {
                    new LineTheoremObject(P, B),
                    new LineTheoremObject(B, D)
                }),
                new Theorem(configuration, PerpendicularLines, new[]
                {
                    new LineTheoremObject(P, B),
                    new LineTheoremObject(A, D)
                }),
                new Theorem(configuration, PerpendicularLines, new[]
                {
                    new LineTheoremObject(P, C),
                    new LineTheoremObject(A, C)
                })
            })
            .Should().BeTrue();
        }

        [Test]
        public void Test_Triangle_With_Tangent_Circles_And_Point_At_The_End()
        {
            // Create the objects
            var A = new LooseConfigurationObject(Point);
            var B = new LooseConfigurationObject(Point);
            var C = new LooseConfigurationObject(Point);
            var D = new ConstructedConfigurationObject(Midpoint, A, B);
            var O = new ConstructedConfigurationObject(Circumcenter, A, B, C);
            var c = new ConstructedConfigurationObject(Circumcircle, A, B, C);
            var P = new ConstructedConfigurationObject(PointReflection, A, O);

            // Create the configuration
            var configuration = Configuration.DeriveFromObjects(ThreePoints, D, O, c, P);

            // Run
            var (oldTheorems, newTheorems, allTheorems) = FindTheorems(configuration);

            // Assert that new + old = all
            newTheorems.AllObjects.Concat(oldTheorems.AllObjects).OrderlessEquals(allTheorems.AllObjects).Should().BeTrue();

            // Assert new theorems
            newTheorems.AllObjects.OrderlessEquals(new[]
            {
                new Theorem(configuration, ParallelLines, new[]
                {
                    new LineTheoremObject(B, P),
                    new LineTheoremObject(O, D)
                }),
                new Theorem(configuration, EqualLineSegments, new[]
                {
                    new LineSegmentTheoremObject(O, A),
                    new LineSegmentTheoremObject(O, P)
                }),
                new Theorem(configuration, EqualLineSegments, new[]
                {
                    new LineSegmentTheoremObject(O, B),
                    new LineSegmentTheoremObject(O, P)
                }),
                new Theorem(configuration, EqualLineSegments, new[]
                {
                    new LineSegmentTheoremObject(O, C),
                    new LineSegmentTheoremObject(O, P)
                }),
                new Theorem(configuration, TangentCircles, new[]
                {
                    new CircleTheoremObject(A, D, O),
                    new CircleTheoremObject(A, B, P)
                }),
                new Theorem(configuration, TangentCircles, new[]
                {
                    new CircleTheoremObject(A, D, O),
                    new CircleTheoremObject(A, C, P)
                }),
                new Theorem(configuration, TangentCircles, new[]
                {
                    new CircleTheoremObject(A, D, O),
                    new CircleTheoremObject(B, C, P)
                }),
                new Theorem(configuration, TangentCircles, new[]
                {
                    new CircleTheoremObject(B, D, O),
                    new CircleTheoremObject(A, B, P)
                }),
                new Theorem(configuration, TangentCircles, new[]
                {
                    new CircleTheoremObject(B, D, O),
                    new CircleTheoremObject(A, C, P)
                }),
                new Theorem(configuration, TangentCircles, new[]
                {
                    new CircleTheoremObject(B, D, O),
                    new CircleTheoremObject(B, C, P)
                }),
                new Theorem(configuration, Incidence, P, c),
                new Theorem(configuration, PerpendicularLines, new[]
                {
                    new LineTheoremObject(P, B),
                    new LineTheoremObject(B, A)
                }),
                new Theorem(configuration, PerpendicularLines, new[]
                {
                    new LineTheoremObject(P, B),
                    new LineTheoremObject(B, D)
                }),
                new Theorem(configuration, PerpendicularLines, new[]
                {
                    new LineTheoremObject(P, B),
                    new LineTheoremObject(A, D)
                }),
                new Theorem(configuration, PerpendicularLines, new[]
                {
                    new LineTheoremObject(P, C),
                    new LineTheoremObject(A, C)
                })
            })
            .Should().BeTrue();

            // Assert all theorems
            allTheorems.AllObjects.OrderlessEquals(new[]
            {
                new Theorem(configuration, ParallelLines, new[]
                {
                    new LineTheoremObject(B, P),
                    new LineTheoremObject(O, D)
                }),
                new Theorem(configuration, EqualLineSegments, new[]
                {
                    new LineSegmentTheoremObject(A, D),
                    new LineSegmentTheoremObject(D, B)
                }),
                new Theorem(configuration, EqualLineSegments, new[]
                {
                    new LineSegmentTheoremObject(O, A),
                    new LineSegmentTheoremObject(O, B)
                }),
                new Theorem(configuration, EqualLineSegments, new[]
                {
                    new LineSegmentTheoremObject(O, A),
                    new LineSegmentTheoremObject(O, C)
                }),
                new Theorem(configuration, EqualLineSegments, new[]
                {
                    new LineSegmentTheoremObject(O, A),
                    new LineSegmentTheoremObject(O, P)
                }),
                new Theorem(configuration, EqualLineSegments, new[]
                {
                    new LineSegmentTheoremObject(O, B),
                    new LineSegmentTheoremObject(O, C)
                }),
                new Theorem(configuration, EqualLineSegments, new[]
                {
                    new LineSegmentTheoremObject(O, B),
                    new LineSegmentTheoremObject(O, P)
                }),
                new Theorem(configuration, EqualLineSegments, new[]
                {
                    new LineSegmentTheoremObject(O, C),
                    new LineSegmentTheoremObject(O, P)
                }),
                new Theorem(configuration, TangentCircles, new[]
                {
                    new CircleTheoremObject(A, D, O),
                    new CircleTheoremObject(A, B, C)
                }),
                new Theorem(configuration, TangentCircles, new[]
                {
                    new CircleTheoremObject(A, D, O),
                    new CircleTheoremObject(A, B, P)
                }),
                new Theorem(configuration, TangentCircles, new[]
                {
                    new CircleTheoremObject(A, D, O),
                    new CircleTheoremObject(A, C, P)
                }),
                new Theorem(configuration, TangentCircles, new[]
                {
                    new CircleTheoremObject(A, D, O),
                    new CircleTheoremObject(B, C, P)
                }),
                new Theorem(configuration, TangentCircles, new[]
                {
                    new CircleTheoremObject(A, D, O),
                    new CircleTheoremObject(c)
                }),
                new Theorem(configuration, TangentCircles, new[]
                {
                    new CircleTheoremObject(B, D, O),
                    new CircleTheoremObject(A, B, C)
                }),
                new Theorem(configuration, TangentCircles, new[]
                {
                    new CircleTheoremObject(B, D, O),
                    new CircleTheoremObject(A, B, P)
                }),
                new Theorem(configuration, TangentCircles, new[]
                {
                    new CircleTheoremObject(B, D, O),
                    new CircleTheoremObject(A, C, P)
                }),
                new Theorem(configuration, TangentCircles, new[]
                {
                    new CircleTheoremObject(B, D, O),
                    new CircleTheoremObject(B, C, P)
                }),
                new Theorem(configuration, TangentCircles, new[]
                {
                    new CircleTheoremObject(B, D, O),
                    new CircleTheoremObject(c)
                }),
                new Theorem(configuration, Incidence, A, c),
                new Theorem(configuration, Incidence, B, c),
                new Theorem(configuration, Incidence, C, c),
                new Theorem(configuration, Incidence, P, c),
                new Theorem(configuration, PerpendicularLines, new[]
                {
                    new LineTheoremObject(O, D),
                    new LineTheoremObject(B, A)
                }),
                new Theorem(configuration, PerpendicularLines, new[]
                {
                    new LineTheoremObject(O, D),
                    new LineTheoremObject(B, D)
                }),
                new Theorem(configuration, PerpendicularLines, new[]
                {
                    new LineTheoremObject(O, D),
                    new LineTheoremObject(A, D)
                }),
                new Theorem(configuration, PerpendicularLines, new[]
                {
                    new LineTheoremObject(P, B),
                    new LineTheoremObject(B, A)
                }),
                new Theorem(configuration, PerpendicularLines, new[]
                {
                    new LineTheoremObject(P, B),
                    new LineTheoremObject(B, D)
                }),
                new Theorem(configuration, PerpendicularLines, new[]
                {
                    new LineTheoremObject(P, B),
                    new LineTheoremObject(A, D)
                }),
                new Theorem(configuration, PerpendicularLines, new[]
                {
                    new LineTheoremObject(P, C),
                    new LineTheoremObject(A, C)
                })
            })
            .Should().BeTrue();
        }
    }
}
