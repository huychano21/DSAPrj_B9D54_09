using System;
using System.Collections.Generic;

namespace TreeAndGraph.Models.GraphModel
{
    /// <summary>
    /// Represents a node (or vertex) in a graph.
    /// Generic type TNodeData allows storing custom data associated with the node.
    /// </summary>
    /// <typeparam name="TNodeData">The type of data stored in the node.</typeparam>
    public class Node<TNodeData>
    {
        /// <summary>
        /// Unique identifier for the node.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Custom data associated with the node (e.g., label, properties).
        /// </summary>
        public TNodeData? Data { get; set; }

        /// <summary>
        /// X-coordinate for visualization purposes.
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Y-coordinate for visualization purposes.
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Indicates if the node's position is fixed (used in Force mode).
        /// </summary>
        public bool IsFixed { get; set; } = false;

        // --- Properties used by graph algorithms ---
        // These can be reset by Graph.ResetAlgorithmState()

        /// <summary>
        /// Flag indicating if the node has been visited during a traversal.
        /// </summary>
        public bool Visited { get; set; } = false;

        /// <summary>
        /// Reference to the preceding node in a path found by an algorithm (e.g., BFS, Dijkstra).
        /// </summary>
        public Node<TNodeData>? Predecessor { get; set; } = null;

        /// <summary>
        /// Calculated distance from a source node (used in pathfinding algorithms like Dijkstra).
        /// Initialized to positive infinity.
        /// </summary>
        public double Distance { get; set; } = double.PositiveInfinity;

        /// <summary>
        /// Constructor for a Node.
        /// </summary>
        /// <param name="id">The unique ID for the node.</param>
        /// <param name="x">The initial X-coordinate.</param>
        /// <param name="y">The initial Y-coordinate.</param>
        public Node(int id, double x, double y)
        {
            Id = id;
            X = x;
            Y = y;
        }

        // Optional: Override ToString for easier debugging or display
        public override string ToString()
        {
            return $"Node(Id={Id}, Data={Data}, Pos=({X:F1},{Y:F1}))";
        }
    }
}
