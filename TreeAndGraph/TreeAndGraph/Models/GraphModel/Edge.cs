using System;

namespace TreeAndGraph.Models.GraphModel
{
    /// <summary>
    /// Represents an edge (or link) connecting two nodes in a graph.
    /// Generic type TEdgeData allows storing custom data associated with the edge.
    /// </summary>
    /// <typeparam name="TEdgeData">The type of data stored in the edge.</typeparam>
    public class Edge<TEdgeData>
    {
        /// <summary>
        /// The identifier of the node where the edge originates.
        /// </summary>
        public int From { get; set; }

        /// <summary>
        /// The identifier of the node where the edge terminates.
        /// </summary>
        public int To { get; set; }

        /// <summary>
        /// Custom data associated with the edge.
        /// </summary>
        public TEdgeData? Data { get; set; }

        /// <summary>
        /// The weight or cost associated with traversing the edge. Defaults to 1.0.
        /// Used in weighted graphs and algorithms like Dijkstra, Kruskal, Prim.
        /// </summary>
        public double Weight { get; set; } = 1.0;

        /// <summary>
        /// Constructor for an Edge.
        /// </summary>
        /// <param name="from">The ID of the starting node.</param>
        /// <param name="to">The ID of the ending node.</param>
        /// <param name="weight">The weight of the edge (optional, defaults to 1.0).</param>
        public Edge(int from, int to, double weight = 1.0)
        {
            From = from;
            To = to;
            Weight = weight;
        }

        // Optional: Override ToString for easier debugging
        public override string ToString()
        {
            return $"Edge({From} -> {To}, Weight={Weight:F1}, Data={Data})";
        }
    }
}
