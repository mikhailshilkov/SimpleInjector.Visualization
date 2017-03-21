namespace MikhailShilkov.SimpleInjector.Visualisation.Tests
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using NUnit.Framework;
    using global::SimpleInjector;
    using global::SimpleInjector.Internals;
    using MikhailShilkov.SimpleInjector.Visualization;

    [TestFixture]
    public class VisualizerTests
    {
        [Test]
        public void BuildGraph_NotRegisteredType_ReturnsEmptyList()
        {
            var container = new Container();
            var actual = Visualizer.BuildGraph(container, typeof(IsolatedType));
            actual.Should().BeEmpty();
        }

        [Test]
        public void BuildGraph_IsolatedType_ReturnsEmptyList()
        {
            var actual = BuildGraph(typeof(IsolatedType));
            actual.Should().BeEmpty();
        }

        [Test]
        public void BuildGraph_SimpleReference_ReturnsSingleItem()
        {
            var actual = BuildGraph(typeof(SimpleReference));
            actual.Should().BeEquivalentTo(
                new DependencyInfo(typeof(SimpleReference), typeof(IsolatedType), typeof(IIsolated)));
        }

        [Test]
        public void BuildGraph_TransitiveReference_ReturnsChainOfItems()
        {
            var actual = BuildGraph(typeof(Level3));
            actual.Should().BeEquivalentTo(
                new DependencyInfo(typeof(SimpleReference), typeof(IsolatedType), typeof(IIsolated)),
                new DependencyInfo(typeof(Level2), typeof(SimpleReference), typeof(ISimple)),
                new DependencyInfo(typeof(Level3), typeof(Level2), typeof(ILevel2)));
        }

        [TestCase(typeof(ReferenceToMultiple), typeof(IMultiple))]
        [TestCase(typeof(ReferenceToMultiple2), typeof(IMultiple))]
        [TestCase(typeof(ReferenceToMultiple3), typeof(IMultipleGeneric<int>))]
        public void BuildGraph_Collection_ReturnsAllItemsOfCollection(Type type, Type interfaceType)
        {
            var actual = BuildGraph(type);
            actual.Should().BeEquivalentTo(
                new DependencyInfo(type, typeof(Multiple1), interfaceType),
                new DependencyInfo(type, typeof(Multiple2), interfaceType),
                new DependencyInfo(type, typeof(Multiple3), interfaceType));
        }

        [Test]
        public void BuildGraph_MultiplePathsToSameDependency_NoDuplicateEntries()
        {
            var actual = BuildGraph(typeof(Level4));
            actual.Should().BeEquivalentTo(
                new DependencyInfo(typeof(Level4), typeof(Level3), typeof(ILevel3)),
                new DependencyInfo(typeof(Level4), typeof(AnotherLevel3), typeof(IAnotherLevel3)),
                new DependencyInfo(typeof(Level3), typeof(Level2), typeof(ILevel2)),
                new DependencyInfo(typeof(AnotherLevel3), typeof(Level2), typeof(ILevel2)),
                new DependencyInfo(typeof(Level2), typeof(SimpleReference), typeof(ISimple)),
                new DependencyInfo(typeof(SimpleReference), typeof(IsolatedType), typeof(IIsolated)));
        }

        [Test]
        public void BuildGraph_Decorator_ProperReferenceToAndFromDecorator()
        {
            var actual = BuildGraph(typeof(DecoratableReference));
            actual.Should().BeEquivalentTo(
                new DependencyInfo(typeof(DecoratableReference), typeof(Decorator), typeof(IDecoratable)),
                new DependencyInfo(typeof(Decorator), typeof(Decorated), typeof(IDecoratable)));
        }

        [Test]
        public void ToGraphViz_SampleGraph_AsExpected()
        {
            var expected = string.Join(
                Environment.NewLine, 
                "digraph g {",
                "Root -> Level4 [ label = \"ILevel4\" ];",
                "Level4 -> Level3 [ label = \"ILevel3\" ];",
                "Level3 -> Level2 [ label = \"ILevel2\" ];",
                "Level2 -> SimpleReference [ label = \"ISimple\" ];",
                "SimpleReference -> IsolatedType [ label = \"IIsolated\" ];",
                "Level4 -> AnotherLevel3 [ label = \"IAnotherLevel3\" ];",
                "AnotherLevel3 -> Level2 [ label = \"ILevel2\" ];",
                "Root -> Multiple1 [ label = \"IMultipleGeneric<Int32>\" ];",
                "Root -> Multiple2 [ label = \"IMultipleGeneric<Int32>\" ];",
                "Root -> Multiple3 [ label = \"IMultipleGeneric<Int32>\" ];",
                "Root -> Decorator [ label = \"IDecoratable\" ];",
                "Decorator -> Decorated [ label = \"IDecoratable\" ];",
                "}");
            var actual = BuildGraph(typeof(Root)).ToGraphViz();
            actual.Should().Be(expected);
        }

        private static IEnumerable<DependencyInfo> BuildGraph(Type type)
        {
            var container = new Container();
            container.Register<IIsolated, IsolatedType>();
            container.Register<ISimple, SimpleReference>();
            container.Register<ILevel2, Level2>();
            container.Register<ILevel3, Level3>();
            container.RegisterCollection(typeof(IMultiple), new[] { typeof(Multiple1), typeof(Multiple2), typeof(Multiple3) });
            container.RegisterCollection(typeof(IMultipleGeneric<int>), new[] { typeof(Multiple1), typeof(Multiple2), typeof(Multiple3) });
            container.Register<ReferenceToMultiple>();
            container.Register<ReferenceToMultiple2>();
            container.Register<ReferenceToMultiple3>();
            container.Register<IAnotherLevel3, AnotherLevel3>();
            container.Register<ILevel4, Level4>();
            container.Register<IDecoratable, Decorated>();
            container.RegisterDecorator<IDecoratable, Decorator>();
            container.Register<DecoratableReference>();
            container.Register<Root>();
            container.Verify();

            return Visualizer.BuildGraph(container, type);
        }

        private interface IIsolated { }
        private class IsolatedType : IIsolated
        {            
        }

        private interface ISimple { }
        private class SimpleReference : ISimple
        {
            public SimpleReference(IIsolated isolated)
            {                
            }
        }

        private interface ILevel2 { }
        private class Level2 : ILevel2
        {
            public Level2(ISimple simple)
            {
            }
        }

        private interface ILevel3 { }
        private class Level3 : ILevel3
        {
            public Level3(ILevel2 level2)
            {
            }
        }

        private interface IMultiple { }
        private interface IMultipleGeneric<T> { }
        private class Multiple1 : IMultiple, IMultipleGeneric<int> { }
        private class Multiple2 : IMultiple, IMultipleGeneric<int> { }
        private class Multiple3 : IMultiple, IMultipleGeneric<int> { }


        private class ReferenceToMultiple
        {
            public ReferenceToMultiple(IMultiple[] multiples)
            {                
            }
        }

        private class ReferenceToMultiple2
        {
            public ReferenceToMultiple2(IEnumerable<IMultiple> multiples)
            {
            }
        }

        private class ReferenceToMultiple3
        {
            public ReferenceToMultiple3(IMultipleGeneric<int>[] multiples)
            {
            }
        }

        private interface IAnotherLevel3 { }
        private class AnotherLevel3 : IAnotherLevel3
        {
            public AnotherLevel3(ILevel2 level2)
            {
            }
        }

        private interface ILevel4 { }
        private class Level4 : ILevel4
        {
            public Level4(ILevel3 level3, IAnotherLevel3 anotherLevel3)
            {
            }
        }

        private interface IDecoratable { }
        private class Decorated : IDecoratable { }

        private class Decorator : IDecoratable
        {
            public Decorator(IDecoratable decorated)
            {                
            }
        }
        private class DecoratableReference
        {
            public DecoratableReference(IDecoratable decoratable)
            {                
            }
        }

        private class Root
        {
            public Root(ILevel4 level4, IMultipleGeneric<int>[] multiples, IDecoratable decoratable)
            {                
            }
        }
    }
}