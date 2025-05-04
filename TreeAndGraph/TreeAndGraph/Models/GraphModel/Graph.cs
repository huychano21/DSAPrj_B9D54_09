using System;
using System.Collections.Generic;
using System.Linq;
using TreeAndGraph.Models.GraphModel;
using TreeAndGraph.Services.GraphService;

namespace TreeAndGraph.Models.GraphModel
{
    /// <summary>
    /// Represents a graph structure consisting of nodes and edges.
    /// Can be configured as directed/undirected and weighted/unweighted.
    /// Generic types allow custom data for nodes and edges.
    /// </summary>
    /// <typeparam name="TNodeData">The type of data stored in nodes.</typeparam>
    /// <typeparam name="TEdgeData">The type of data stored in edges.</typeparam>
    public class Graph<TNodeData, TEdgeData>
    {
        /// <summary>
        /// Dictionary storing all nodes in the graph, keyed by their unique ID.
        /// Provides fast lookup of nodes by ID.
        /// </summary>
        public Dictionary<int, Node<TNodeData>> Nodes { get; private set; } = new Dictionary<int, Node<TNodeData>>();

        /// <summary>
        /// List storing all edges in the graph.
        /// Using a List allows for parallel edges if needed (controlled by AddEdge logic).
        /// </summary>
        public List<Edge<TEdgeData>> Edges { get; private set; } = new List<Edge<TEdgeData>>();

        /// <summary>
        /// Specifies whether the graph is directed (true) or undirected (false).
        /// Affects how edges are interpreted and how neighbors/degrees are calculated.
        /// </summary>
        public bool IsDirected { get; set; } = false;

        /// <summary>
        /// Specifies whether the graph is weighted (true) or unweighted (false).
        /// Affects whether edge weights are considered in algorithms and visualization.
        /// </summary>
        public bool IsWeighted { get; set; } = false;

        // Counter to generate unique IDs for new nodes.
        private int _nextNodeId = 0;

        /// <summary>
        /// Adds a new node to the graph at the specified coordinates.
        /// </summary>
        /// <param name="x">The initial X-coordinate for visualization.</param>
        /// <param name="y">The initial Y-coordinate for visualization.</param>
        /// <param name="data">Optional custom data for the node.</param>
        /// <returns>The newly created Node object.</returns>
        public Node<TNodeData> AddNode(double x, double y, TNodeData? data = default)
        {
            int id = _nextNodeId++;
            // Use the correct namespace for Node
            var node = new Node<TNodeData>(id, x, y) { Data = data };
            Nodes.Add(id, node);
            return node;
        }

        /// <summary>
        /// Removes a node and all incident edges from the graph.
        /// </summary>
        /// <param name="nodeId">The ID of the node to remove.</param>
        /// <returns>True if the node was found and removed, false otherwise.</returns>
        public bool RemoveNode(int nodeId)
        {
            if (Nodes.Remove(nodeId))
            {
                // Remove all edges connected to this node.
                // Iterate backwards when removing from a list to avoid index issues.
                for (int i = Edges.Count - 1; i >= 0; i--)
                {
                    if (Edges[i].From == nodeId || Edges[i].To == nodeId)
                    {
                        Edges.RemoveAt(i);
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds a new edge connecting two existing nodes.
        /// Handles checks for node existence and prevents duplicate edges
        /// in undirected graphs unless parallel edges are explicitly allowed.
        /// </summary>
        /// <param name="fromId">The ID of the starting node.</param>
        /// <param name="toId">The ID of the ending node.</param>
        /// <param name="weight">The weight of the edge (used if IsWeighted is true).</param>
        /// <param name="data">Optional custom data for the edge.</param>
        /// <returns>The newly created Edge object, or null if the edge couldn't be added (e.g., nodes don't exist, duplicate edge).</returns>
        public Edge<TEdgeData>? AddEdge(int fromId, int toId, double weight = 1.0, TEdgeData? data = default)
        {
            // Ensure both nodes exist
            if (!Nodes.ContainsKey(fromId) || !Nodes.ContainsKey(toId))
            {
                Console.WriteLine($"Warning: Cannot add edge ({fromId}-{toId}). Node(s) not found.");
                return null;
            }

            // Prevent self-loops if desired (uncomment to enable)
            // if (fromId == toId) {
            //     Console.WriteLine($"Warning: Cannot add self-loop edge ({fromId}-{toId}).");
            //     return null;
            // }

            // Check for duplicate edges, considering graph directionality
            bool exists = Edges.Any(e =>
                (e.From == fromId && e.To == toId) || // Direct match
                (!IsDirected && e.From == toId && e.To == fromId) // Reverse match for undirected graphs
            );

            // If duplicates are not allowed and one exists, return null
            if (exists && !AllowParallelEdges())
            {
                Console.WriteLine($"Warning: Cannot add duplicate edge ({fromId}-{toId}).");
                return null;
            }

            // Create and add the new edge (Use correct namespace for Edge)
            var edge = new Edge<TEdgeData>(fromId, toId, weight) { Data = data };
            Edges.Add(edge);
            return edge;
        }

        /// <summary>
        /// Removes the first occurrence of an edge between two specified nodes.
        /// Considers graph directionality.
        /// </summary>
        /// <param name="fromId">The ID of the starting node.</param>
        /// <param name="toId">The ID of the ending node.</param>
        /// <returns>True if an edge was found and removed, false otherwise.</returns>
        public bool RemoveEdge(int fromId, int toId)
        {
            Edge<TEdgeData>? edgeToRemove = null;

            // Find the edge based on directionality
            if (IsDirected)
            {
                // Find the specific directed edge
                edgeToRemove = Edges.FirstOrDefault(e => e.From == fromId && e.To == toId);
            }
            else
            {
                // Find the edge regardless of direction for undirected graphs
                edgeToRemove = Edges.FirstOrDefault(e =>
                    (e.From == fromId && e.To == toId) ||
                    (e.From == toId && e.To == fromId));
            }

            // If an edge was found, remove it from the list
            if (edgeToRemove != null)
            {
                return Edges.Remove(edgeToRemove);
            }
            return false; // Edge not found
        }

        /// <summary>
        /// Gets the neighboring nodes of a given node.
        /// Considers graph directionality (only outgoing neighbors for directed graphs).
        /// </summary>
        /// <param name="nodeId">The ID of the node.</param>
        /// <returns>An enumerable collection of neighbor nodes.</returns>
        public IEnumerable<Node<TNodeData>> GetNeighbors(int nodeId)
        {
            if (!Nodes.ContainsKey(nodeId)) yield break; // Node doesn't exist

            foreach (var edge in Edges)
            {
                if (edge.From == nodeId) // Edge starts at the node
                {
                    if (Nodes.TryGetValue(edge.To, out var neighbor))
                        yield return neighbor;
                }
                else if (!IsDirected && edge.To == nodeId) // For undirected, also consider edges ending at the node
                {
                    if (Nodes.TryGetValue(edge.From, out var neighbor))
                        yield return neighbor;
                }
            }
        }

        /// <summary>
        /// Gets the edges connected to a given node.
        /// Considers graph directionality (only outgoing edges for directed graphs by default,
        /// but includes incoming for undirected).
        /// </summary>
        /// <param name="nodeId">The ID of the node.</param>
        /// <returns>An enumerable collection of adjacent edges.</returns>
        public IEnumerable<Edge<TEdgeData>> GetAdjacentEdges(int nodeId)
        {
            if (!Nodes.ContainsKey(nodeId)) yield break; // Node doesn't exist

            foreach (var edge in Edges)
            {
                if (edge.From == nodeId) // Edge starts at the node
                {
                    yield return edge;
                }
                else if (!IsDirected && edge.To == nodeId) // For undirected, also consider edges ending at the node
                {
                    yield return edge;
                }
            }
        }

        // --- Degree Calculation Methods ---

        /// <summary>
        /// Calculates the degree of a node (total number of connected edges).
        /// For directed graphs, this is typically InDegree + OutDegree.
        /// For undirected graphs, it's the count of incident edges.
        /// </summary>
        /// <param name="nodeId">The ID of the node.</param>
        /// <returns>The degree of the node, or 0 if the node doesn't exist.</returns>
        public int GetDegree(int nodeId)
        {
            if (!Nodes.ContainsKey(nodeId)) return 0;

            if (IsDirected)
            {
                // For directed graphs, degree is often defined as in-degree + out-degree
                return GetInDegree(nodeId) + GetOutDegree(nodeId);
            }
            else
            {
                // For undirected graphs, count all edges connected to the node.
                // Be careful with self-loops: they usually count as 2 towards the degree.
                int degree = 0;
                foreach (var edge in Edges)
                {
                    if (edge.From == nodeId)
                    {
                        degree++;
                    }
                    // Check 'else if' to avoid double-counting non-loop edges.
                    // If it's a self-loop (From == To == nodeId), the first condition already counted it.
                    else if (edge.To == nodeId && edge.From != nodeId) // Ensure not double counting self-loop
                    {
                        degree++;
                    }
                }
                return degree;
                // Simpler Linq version for undirected (counts self-loops once):
                // return Edges.Count(e => e.From == nodeId || e.To == nodeId);
            }
        }

        /// <summary>
        /// Calculates the in-degree of a node (number of edges pointing towards it).
        /// Only meaningful for directed graphs.
        /// </summary>
        /// <param name="nodeId">The ID of the node.</param>
        /// <returns>The in-degree, or 0 if the node doesn't exist or the graph is undirected.</returns>
        public int GetInDegree(int nodeId)
        {
            // In-degree is only relevant for directed graphs
            if (!Nodes.ContainsKey(nodeId) || !IsDirected) return 0;
            // Count edges where the 'To' node matches the given nodeId
            return Edges.Count(e => e.To == nodeId);
        }

        /// <summary>
        /// Calculates the out-degree of a node (number of edges originating from it).
        /// For undirected graphs, this is the same as the total degree.
        /// </summary>
        /// <param name="nodeId">The ID of the node.</param>
        /// <returns>The out-degree, or 0 if the node doesn't exist.</returns>
        public int GetOutDegree(int nodeId)
        {
            if (!Nodes.ContainsKey(nodeId)) return 0;

            if (!IsDirected)
            {
                // For undirected graphs, out-degree is the same as the total degree
                // Re-calculate here to be explicit, although GetDegree() could be called.
                int degree = 0;
                foreach (var edge in Edges)
                {
                    if (edge.From == nodeId) degree++;
                    else if (edge.To == nodeId && edge.From != nodeId) degree++; // Avoid double count self-loop
                }
                return degree;
            }
            else
            {
                // For directed graphs, count edges where the 'From' node matches
                return Edges.Count(e => e.From == nodeId);
            }
        }

        /// <summary>
        /// Resets the algorithm-specific state (Visited, Predecessor, Distance) for all nodes.
        /// Should be called before running a new algorithm execution.
        /// </summary>
        public void ResetAlgorithmState()
        {
            foreach (var node in Nodes.Values)
            {
                node.Visited = false;
                node.Predecessor = null;
                node.Distance = double.PositiveInfinity;
            }
        }

        // --- Static Factory and Helper Methods ---

        /// <summary>
        /// Creates a default sample graph for initial display or testing.
        /// </summary>
        /// <returns>A sample Graph object.</returns>
        public static Graph<string, object> CreateDefaultGraph()
        {
            // Creates a simple undirected, unweighted graph with 4 nodes (A, B, C, D) and some edges.
            var graph = new Graph<string, object> { IsDirected = false, IsWeighted = false };
            var n1 = graph.AddNode(100, 100, "A");
            var n2 = graph.AddNode(250, 100, "B");
            var n3 = graph.AddNode(100, 250, "C");
            var n4 = graph.AddNode(250, 250, "D");
            graph.AddEdge(n1.Id, n2.Id);
            graph.AddEdge(n1.Id, n3.Id);
            graph.AddEdge(n2.Id, n4.Id);
            graph.AddEdge(n3.Id, n4.Id);
            return graph;
        }

        /// <summary>
        /// Creates a graph instance from a string representation of an edge list.
        /// Format per line: "NodeFrom NodeTo [Weight]" (Weight optional, space/tab/comma delimited).
        /// </summary>
        /// <param name="edgeListInput">The string containing the edge list data.</param>
        /// <param name="isDirected">Whether the created graph should be directed.</param>
        /// <param name="isWeighted">Whether the created graph should be weighted (expects weights in input).</param>
        /// <returns>A new Graph instance populated from the input.</returns>
        public static Graph<string, object> CreateFromEdgeList(string edgeListInput, bool isDirected, bool isWeighted)
        {
            var graph = new Graph<string, object> { IsDirected = isDirected, IsWeighted = isWeighted };
            var nodeMap = new Dictionary<string, int>(); // Map node label (string) to internal ID (int)
            int nextNodeIdCounter = 0; // Use a separate counter for node IDs

            var lines = edgeListInput.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var parts = line.Trim().Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) continue; // Need at least two node labels per line

                string fromLabel = parts[0];
                string toLabel = parts[1];
                double weight = 1.0; // Default weight

                // Parse weight if the graph is weighted and weight is provided
                if (isWeighted && parts.Length >= 3)
                {
                    if (!double.TryParse(parts[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out weight))
                    {
                        Console.WriteLine($"Warning: Could not parse weight '{parts[2]}' on line: {line}. Using default weight 1.0.");
                        weight = 1.0;
                    }
                }

                // Add nodes to the graph if they don't exist yet, mapping labels to IDs
                if (!nodeMap.ContainsKey(fromLabel))
                {
                    int newId = nextNodeIdCounter++;
                    nodeMap[fromLabel] = newId;
                    // Add node with random initial position and the label as data
                    graph.AddNode(Random.Shared.Next(50, 450), Random.Shared.Next(50, 350), fromLabel);
                }
                if (!nodeMap.ContainsKey(toLabel))
                {
                    int newId = nextNodeIdCounter++;
                    nodeMap[toLabel] = newId;
                    graph.AddNode(Random.Shared.Next(50, 450), Random.Shared.Next(50, 350), toLabel);
                }

                // Get the internal IDs for the nodes
                int fromId = nodeMap[fromLabel];
                int toId = nodeMap[toLabel];

                // Add the edge using the internal IDs
                graph.AddEdge(fromId, toId, weight);
            }

            // Note: The node's Data property is already set to the label during AddNode.
            // No need for the extra loop at the end from the previous version.

            return graph;
        }

        // Placeholder for adding methods to create graph from Adjacency List or Matrix
        // public static Graph<string, object> CreateFromAdjacencyList(string input, ...) { ... }
        // public static Graph<string, object> CreateFromAdjacencyMatrix(string input, ...) { ... }


        // --- Algorithm Execution Methods ---
        // These methods now delegate to classes within the TreeAndGraph.Algorithms namespace.

        /// <summary>
        /// Runs the Depth First Search algorithm starting from a given node.
        /// </summary>
        /// <param name="startNodeId">The ID of the starting node.</param>
        /// <returns>A list of steps representing the algorithm's execution for visualization.</returns>
        /// <exception cref="ArgumentException">Thrown if the start node ID does not exist.</exception>
        public List<AlgorithmStep<TNodeData, TEdgeData>> RunDFS(int startNodeId)
        {
            if (!Nodes.ContainsKey(startNodeId))
                throw new ArgumentException($"Start node with ID {startNodeId} not found in the graph.", nameof(startNodeId));

            // Assumes Traversal class is in TreeAndGraph.Algorithms
            var dfsRunner = new Traversal<TNodeData, TEdgeData>(this);
            return dfsRunner.DFS(startNodeId);
        }

        /// <summary>
        /// Runs the Breadth First Search algorithm starting from a given node.
        /// </summary>
        /// <param name="startNodeId">The ID of the starting node.</param>
        /// <returns>A list of steps representing the algorithm's execution for visualization.</returns>
        /// <exception cref="ArgumentException">Thrown if the start node ID does not exist.</exception>
        public List<AlgorithmStep<TNodeData, TEdgeData>> RunBFS(int startNodeId)
        {
            if (!Nodes.ContainsKey(startNodeId))
                throw new ArgumentException($"Start node with ID {startNodeId} not found in the graph.", nameof(startNodeId));

            // Assumes Traversal class is in TreeAndGraph.Algorithms
            var bfsRunner = new Traversal<TNodeData, TEdgeData>(this);
            return bfsRunner.BFS(startNodeId);
        }

        /// <summary>
        /// Runs Dijkstra's algorithm to find the shortest path between two nodes in a weighted graph.
        /// </summary>
        /// <param name="startNodeId">The ID of the starting node.</param>
        /// <param name="endNodeId">The ID of the destination node.</param>
        /// <returns>A list of steps representing the algorithm's execution for visualization.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the graph is not weighted.</exception>
        /// <exception cref="ArgumentException">Thrown if start or end node IDs do not exist.</exception>
        public List<AlgorithmStep<TNodeData, TEdgeData>> RunDijkstra(int startNodeId, int endNodeId)
        {
            if (!IsWeighted)
                throw new InvalidOperationException("Dijkstra's algorithm requires a weighted graph.");
            if (!Nodes.ContainsKey(startNodeId))
                throw new ArgumentException($"Start node with ID {startNodeId} not found.", nameof(startNodeId));
            if (!Nodes.ContainsKey(endNodeId))
                throw new ArgumentException($"End node with ID {endNodeId} not found.", nameof(endNodeId));

            // Assumes Pathfinding class is in TreeAndGraph.Algorithms
            var pathfinder = new Pathfinding<TNodeData, TEdgeData>(this);
            return pathfinder.Dijkstra(startNodeId, endNodeId);
        }

        // Add similar methods for Kruskal, Prim, Euler, Hamilton check etc.
        // Example:
        // public List<AlgorithmStep<TNodeData, TEdgeData>> RunKruskal()
        // {
        //     if (!IsWeighted) throw new InvalidOperationException("Kruskal's algorithm requires a weighted graph.");
        //     var mstBuilder = new SpanningTree<TNodeData, TEdgeData>(this);
        //     return mstBuilder.Kruskal();
        // }


        // --- Helper Methods ---

        /// <summary>
        /// Determines whether parallel edges (multiple edges between the same two nodes
        /// in the same direction) are allowed. Currently returns false.
        /// </summary>
        /// <returns>False.</returns>
        private bool AllowParallelEdges() => false; // Change to true if needed

    }

}
