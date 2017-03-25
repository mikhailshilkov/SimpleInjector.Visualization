# SimpleInjector.Visualization

Visualize dependency tree configured in SimpleInjector DI container.

### Usage example

``` cs
// 1. Initialize the DI container and do the registrations
Container container = ...; 

// 2. Pick the root type
Type rootType = typeof(MyRootClass);

// 3. Build the collection of relations
var dependencies = Visualizer.BuildGraph(container, rootType);

// 4. Convert to Graphviz
var graphviz = dependencies.ToGraphViz();

// Copy-paste to a visualization tool (e.g. 
[webgraphviz.com](http://webgraphviz.com))
```

### Result example

![Dependency Graph](http://mikhail.io/2017/03/visualizing-dependency-tree-from-di-container/class-dependency-graph.png)

### Read more

[Visualizing Dependency Tree from DI Container](http://mikhail.io/2017/03/visualizing-dependency-tree-from-di-container/).