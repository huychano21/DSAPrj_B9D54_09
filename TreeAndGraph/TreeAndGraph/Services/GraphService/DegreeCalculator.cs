using System;
using System.Collections.Generic;
using System.Linq;
using TreeAndGraph.Models.GraphModel;

namespace TreeAndGraph.Services.GraphService
{
    /// <summary>
    /// Provides methods to calculate degrees of nodes in a graph.
    /// Note: This functionality is often directly available within the Graph class itself.
    /// This class acts primarily as a wrapper if separation is strictly required.
    /// </summary>
    /// <typeparam name="TNodeData">The type of data stored in graph nodes.</typeparam>
    /// <typeparam name="TEdgeData">The type of data stored in graph edges.</typeparam>
    public class DegreeCalculator<TNodeData, TEdgeData>
    {
        private readonly Graph<TNodeData, TEdgeData> _graph;

        public DegreeCalculator(Graph<TNodeData, TEdgeData> graph)
        {
            _graph = graph;
        }

        /// <summary>
        /// Gets the total degree of a specified node.
        /// Delegates to the Graph object's GetDegree method.
        /// </summary>
        /// <param name="nodeId">The ID of the node.</param>
        /// <returns>The total degree of the node.</returns>
        public int GetTotalDegree(int nodeId)
        {
            return _graph.GetDegree(nodeId);
        }

        /// <summary>
        /// Gets the in-degree of a specified node (for directed graphs).
        /// Delegates to the Graph object's GetInDegree method.
        /// </summary>
        /// <param name="nodeId">The ID of the node.</param>
        /// <returns>The in-degree of the node.</returns>
        public int GetInDegree(int nodeId)
        {
            // In-degree is typically only meaningful for directed graphs.
            // The Graph class method already handles this.
            return _graph.GetInDegree(nodeId);
        }

        /// <summary>
        /// Gets the out-degree of a specified node.
        /// Delegates to the Graph object's GetOutDegree method.
        /// </summary>
        /// <param name="nodeId">The ID of the node.</param>
        /// <returns>The out-degree of the node.</returns>
        public int GetOutDegree(int nodeId)
        {
            return _graph.GetOutDegree(nodeId);
        }

        /// <summary>
        /// Gets a dictionary containing the total degree for each node in the graph.
        /// </summary>
        /// <returns>A dictionary where the key is the node ID and the value is its total degree.</returns>
        public Dictionary<int, int> GetAllNodeDegrees()
        {
            var degrees = new Dictionary<int, int>();
            foreach (int nodeId in _graph.Nodes.Keys)
            {
                degrees[nodeId] = _graph.GetDegree(nodeId);
            }
            return degrees;
        }

        /// <summary>
        /// Gets a dictionary containing the in-degree for each node (relevant for directed graphs).
        /// </summary>
        /// <returns>A dictionary where the key is the node ID and the value is its in-degree.</returns>
        public Dictionary<int, int> GetAllNodeInDegrees()
        {
            var degrees = new Dictionary<int, int>();
            if (!_graph.IsDirected) return degrees; // Return empty if undirected

            foreach (int nodeId in _graph.Nodes.Keys)
            {
                degrees[nodeId] = _graph.GetInDegree(nodeId);
            }
            return degrees;
        }

        /// <summary>
        /// Gets a dictionary containing the out-degree for each node.
        /// </summary>
        /// <returns>A dictionary where the key is the node ID and the value is its out-degree.</returns>
        public Dictionary<int, int> GetAllNodeOutDegrees()
        {
            var degrees = new Dictionary<int, int>();
            foreach (int nodeId in _graph.Nodes.Keys)
            {
                degrees[nodeId] = _graph.GetOutDegree(nodeId);
            }
            return degrees;
        }
    }
}
