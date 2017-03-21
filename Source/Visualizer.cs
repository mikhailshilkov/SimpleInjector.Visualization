namespace MikhailShilkov.SimpleInjector.Visualization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using global::SimpleInjector;
    using global::SimpleInjector.Advanced;

    public static class Visualizer
    {
        public static IEnumerable<DependencyInfo> BuildGraph(Container container, Type rootType)
        {
            var result = new List<DependencyInfo>();
            BuildDependencies(container, rootType, result, new HashSet<InstanceProducer>());
            return result;
        }

        public static string ToGraphViz(this IEnumerable<DependencyInfo> dependencies)
        {
            var ds = dependencies
                .Select(d => $"{PrettyName(d.Parent)} -> {PrettyName(d.Child)} [ label = \"{PrettyName(d.Via)}\" ];")
                .ToArray();
            return "digraph g {" + Environment.NewLine + string.Join(Environment.NewLine, ds) + Environment.NewLine + "}";
        }

        private static void BuildDependencies(Container container, Type type, List<DependencyInfo> acc, HashSet<InstanceProducer> visited)
        {
            var registration = container.GetRegistration(type);
            if (registration != null)
            {
                BuildDependencies(container, registration, acc, visited);
            }
        }

        private static void BuildDependencies(Container container, InstanceProducer producer, List<DependencyInfo> acc, HashSet<InstanceProducer> visited)
        {
            if (visited.Contains(producer))
            {
                return;
            }

            visited.Add(producer);
            foreach (var rel in producer.GetRelationships())
            {
                var child = rel.Dependency.Registration.ImplementationType;
                if (child.IsArray)
                {
                    var interfaceType = child.GetElementType();
                    foreach (var dep in container.GetAllInstances(interfaceType))
                    {
                        acc.Add(new DependencyInfo(producer.ServiceType, dep.GetType(), interfaceType));
                        BuildDependencies(container, dep.GetType(), acc, visited);
                    }
                }
                else if (child.IsGenericType && child.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    var interfaceType = child.GetGenericArguments()[0];
                    foreach (var dep in container.GetAllInstances(interfaceType))
                    {
                        acc.Add(new DependencyInfo(producer.ServiceType, dep.GetType(), interfaceType));
                        BuildDependencies(container, dep.GetType(), acc, visited);
                    }
                }
                else
                {
                    acc.Add(new DependencyInfo(producer.Registration.ImplementationType, rel.Dependency.Registration.ImplementationType, rel.Dependency.ServiceType));
                    BuildDependencies(container, rel.Dependency, acc, visited);
                }
            }
        }

        private static string PrettyName(Type type)
        {
            if (type.GetGenericArguments().Length == 0)
            {
                return type.Name;
            }
            var genericArguments = type.GetGenericArguments();
            var typeDefeninition = type.Name;
            var unmangledName = typeDefeninition.Substring(0, typeDefeninition.IndexOf("`", StringComparison.Ordinal));
            return unmangledName + "<" + String.Join(",", genericArguments.Select(PrettyName)) + ">";
        }
    }
}
