﻿using System;
using System.Collections.Generic;
using System.Linq;
using GeoGen.AnalyticalGeometry;
using GeoGen.Core;
using GeoGen.Utilities;
using Moq;
using NUnit.Framework;
using static GeoGen.Analyzer.Test.TestHelpers.Utilities;

namespace GeoGen.Analyzer.Test.Theorems.TheoremVerifiers
{
    [TestFixture]
    public class ConcurrencyVerifierTest
    {
        private static IObjectsContainersManager _containers;

        private static ConcurrencyVerifier Verifier() => new ConcurrencyVerifier(new AnalyticalHelper(), new SubsetsProvider());

        private static ContextualContainer Container(Configuration configuration, params List<AnalyticalObject>[] objects)
        {
            var helper = new AnalyticalHelper();

            var containers = objects.Select(analyticalObjects =>
            {
                var result = new ObjectsContainer();

                var allObjects = configuration.ObjectsMap.AllObjects;

                for (var i = 0; i < allObjects.Count; i++)
                {
                    var iCopy = i;
                    result.Add(new[] {allObjects[i]}, c => new List<AnalyticalObject> {analyticalObjects[iCopy]});
                }

                return result;
            }).ToList();

            var manager = new Mock<IObjectsContainersManager>();

            manager.Setup(s => s.GetEnumerator()).Returns(() => containers.GetEnumerator());

            _containers = manager.Object;

            return new ContextualContainer(configuration, _containers, helper);
        }

        [Test]
        public void Test_Medians_Are_Concurrent()
        {
            var looseObjects = new List<LooseConfigurationObject>
            {
                new LooseConfigurationObject(ConfigurationObjectType.Point) {Id = 1},
                new LooseConfigurationObject(ConfigurationObjectType.Point) {Id = 2},
                new LooseConfigurationObject(ConfigurationObjectType.Point) {Id = 3},
                new LooseConfigurationObject(ConfigurationObjectType.Point) {Id = 4},
                new LooseConfigurationObject(ConfigurationObjectType.Point) {Id = 5},
                new LooseConfigurationObject(ConfigurationObjectType.Point) {Id = 6}
            };

            var configuration = new Configuration(looseObjects, new List<ConstructedConfigurationObject>());

            var analyticalObjects = new[]
            {
                new List<AnalyticalObject>
                {
                    new Point(1, 4),
                    new Point(-1, -1),
                    new Point(7, -1),
                    new Point(3, -1),
                    new Point(4, 1.5),
                    new Point(0, 1.5)
                },
                new List<AnalyticalObject>
                {
                    new Point(0, 5),
                    new Point(-2, -1),
                    new Point(7, -1),
                    new Point(2.5, -1),
                    new Point(3.5, 2),
                    new Point(-1, 2)
                }
            };

            var correctOutputs = Verifier().GetOutput(Container(configuration, analyticalObjects))
                    .Where(output => _containers.All(c => output.VerifierFunction(c)))
                    .ToList();

            Assert.AreEqual(8, correctOutputs.Count);
        }

        [Test]
        public void Test_With_Orthocenter()
        {
            var looseObjects = new List<LooseConfigurationObject>
            {
                new LooseConfigurationObject(ConfigurationObjectType.Point) {Id = 1},
                new LooseConfigurationObject(ConfigurationObjectType.Point) {Id = 2},
                new LooseConfigurationObject(ConfigurationObjectType.Point) {Id = 3},
                new LooseConfigurationObject(ConfigurationObjectType.Line) {Id = 4},
                new LooseConfigurationObject(ConfigurationObjectType.Line) {Id = 5},
                new LooseConfigurationObject(ConfigurationObjectType.Circle) {Id = 6}
            };

            var configuration = new Configuration(looseObjects, new List<ConstructedConfigurationObject>());

            var analyticalObjects = new[]
            {
                new List<AnalyticalObject>
                {
                    new Point(5, 3),
                    new Point(3, -3),
                    new Point(8, -3),
                    new Line(new Point(7, -1), new Point(3, -3)),
                    new Line(new Point(3.5, -1.5), new Point(8, -3)),
                    new Circle(new Point(7, -1), new Point(3.5, -1.5), new Point(5, 3))
                },
                new List<AnalyticalObject>
                {
                    new Point(7, 5),
                    new Point(3, -3),
                    new Point(11, -3),
                    new Line(new Point(9.4, 0.2), new Point(3, -3)),
                    new Line(new Point(4.6, 0.2), new Point(11, -3)),
                    new Circle(new Point(7, 5), new Point(9.4, 0.2), new Point(4.6, 0.2))
                }
            };

            var correctOutputs = Verifier().GetOutput(Container(configuration, analyticalObjects))
                    .Where(output => _containers.All(c => output.VerifierFunction(c)))
                    .ToList();

            Assert.AreEqual(3, correctOutputs.Count);
        }
    }
}