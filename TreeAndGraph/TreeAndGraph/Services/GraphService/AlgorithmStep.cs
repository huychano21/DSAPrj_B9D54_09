using System.Collections.Generic;

namespace TreeAndGraph.Services.GraphService
{
    // <summary>
    /// Represents a single step in the execution of a graph algorithm,
    /// containing information necessary for visualization.
    /// Generic types correspond to the data types used in the Graph's Nodes and Edges.
    /// </summary>
    /// <typeparam name="TNodeData">The type of data stored in graph nodes.</typeparam>
    /// <typeparam name="TEdgeData">The type of data stored in graph edges.</typeparam>
    public class AlgorithmStep<TNodeData, TEdgeData>
    {
        /// <summary>
        /// A textual description of the current step (e.g., "Visiting node X", "Considering edge Y").
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// A set of Node IDs that should be highlighted in this step.
        /// </summary>
        public HashSet<int> HighlightedNodes { get; set; } = new HashSet<int>();

        /// <summary>
        /// A set of Edges (represented by a tuple of from/to Node IDs) that should be highlighted.
        /// Note: For undirected graphs, the visualization logic might need to check both (from, to) and (to, from).
        /// </summary>
        public HashSet<(int from, int to)> HighlightedEdges { get; set; } = new HashSet<(int, int)>();

        /// <summary>
        /// Optional dictionary to provide specific labels for nodes during this step
        /// (e.g., displaying distances in Dijkstra's algorithm). Key is Node ID, Value is the label string.
        /// </summary>
        public Dictionary<int, string> NodeLabels { get; set; } = new Dictionary<int, string>();

        /// <summary>
        /// Optional: Add properties for specific colors or styles if needed for more complex visualizations.
        /// public Dictionary<int, string> NodeColors { get; set; } = new Dictionary<int, string>();
        /// public Dictionary<(int, int), string> EdgeColors { get; set; } = new Dictionary<(int, int), string>();
        /// </summary>

        // Constructor (optional, can use object initializer)
        public AlgorithmStep(string description = "")
        {
            Description = description;
        }
    }
}
