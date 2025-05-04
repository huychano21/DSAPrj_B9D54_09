using System;
using System.Collections.Generic;
using System.Linq;
using TreeAndGraph.Models.GraphModel;

namespace TreeAndGraph.Services.GraphService
{
    /// <summary>
    /// Provides implementations for pathfinding algorithms like Dijkstra's.
    /// </summary>
    /// <typeparam name="TNodeData">The type of data stored in graph nodes.</typeparam>
    /// <typeparam name="TEdgeData">The type of data stored in graph edges.</typeparam>
    public class Pathfinding<TNodeData, TEdgeData>
    {
        private readonly Graph<TNodeData, TEdgeData> _graph;

        /// <summary>
        /// Initializes a new instance of the Pathfinding class.
        /// </summary>
        /// <param name="graph">The graph instance to perform pathfinding on.</param>
        public Pathfinding(Graph<TNodeData, TEdgeData> graph)
        {
            _graph = graph;
        }

        /// <summary>
        /// Implements Dijkstra's algorithm to find the shortest path from a start node to an end node.
        /// Assumes non-negative edge weights.
        /// </summary>
        /// <param name="startNodeId">The ID of the starting node.</param>
        /// <param name="endNodeId">The ID of the destination node.</param>
        /// <returns>A list of AlgorithmStep objects detailing the Dijkstra process for visualization.</returns>
        public List<AlgorithmStep<TNodeData, TEdgeData>> Dijkstra(int startNodeId, int endNodeId)
        {
            var steps = new List<AlgorithmStep<TNodeData, TEdgeData>>();
            // Priority queue stores nodes to visit, ordered by distance. Key: Node ID, Value: Distance
            var priorityQueue = new PriorityQueue<int, double>();
            // Keep track of nodes for which the shortest path is finalized
            var finalizedNodes = new HashSet<int>();
            // Store the edges forming the shortest path tree discovered so far
            var shortestPathTreeEdges = new HashSet<(int from, int to)>();

            _graph.ResetAlgorithmState();

            if (!_graph.Nodes.ContainsKey(startNodeId) || !_graph.Nodes.ContainsKey(endNodeId))
            {
                steps.Add(new AlgorithmStep<TNodeData, TEdgeData>("Lỗi: Nút bắt đầu hoặc kết thúc không tồn tại."));
                return steps;
            }

            // Initialize start node
            _graph.Nodes[startNodeId].Distance = 0;
            priorityQueue.Enqueue(startNodeId, 0);

            steps.Add(new AlgorithmStep<TNodeData, TEdgeData>($"Bắt đầu Dijkstra từ {GetNodeLabel(startNodeId)} đến {GetNodeLabel(endNodeId)}")
            {
                HighlightedNodes = { startNodeId },
                NodeLabels = { { startNodeId, "0" } } // Show initial distance
            });

            while (priorityQueue.Count > 0)
            {
                // Get the node with the smallest distance from the queue
                int currentNodeId = priorityQueue.Dequeue();

                // If already finalized, skip (can happen with some PQ implementations or graph structures)
                if (finalizedNodes.Contains(currentNodeId))
                {
                    continue;
                }

                // Finalize the node - its shortest path is now known
                finalizedNodes.Add(currentNodeId);
                var currentNode = _graph.Nodes[currentNodeId];

                // --- Visualization Step: Finalizing Node ---
                var finalizeStep = new AlgorithmStep<TNodeData, TEdgeData>($"Chọn nút {GetNodeLabel(currentNodeId)} (Khoảng cách: {currentNode.Distance:F1})")
                {
                    HighlightedNodes = new HashSet<int>(finalizedNodes), // Highlight all finalized nodes
                    HighlightedEdges = new HashSet<(int, int)>(shortestPathTreeEdges), // Highlight the current shortest path tree
                    NodeLabels = GetCurrentDistanceLabels() // Show current known distances
                };
                // Highlight the edge that led to this node being finalized
                if (currentNode.Predecessor != null)
                {
                    shortestPathTreeEdges.Add((currentNode.Predecessor.Id, currentNodeId));
                    finalizeStep.HighlightedEdges.Add((currentNode.Predecessor.Id, currentNodeId));
                }
                steps.Add(finalizeStep);
                // --- End Visualization Step ---


                // If we reached the destination, we can optionally stop early
                if (currentNodeId == endNodeId)
                {
                    steps.Add(new AlgorithmStep<TNodeData, TEdgeData>($"Đã tìm thấy nút đích {GetNodeLabel(endNodeId)}!"));
                    break; // Found the shortest path to the destination
                }

                // Relax edges going out from the current node
                foreach (var edge in _graph.GetAdjacentEdges(currentNodeId))
                {
                    // In Dijkstra, we only care about outgoing edges or all edges if undirected
                    // The GetAdjacentEdges handles directionality based on _graph.IsDirected
                    int neighborId = (edge.From == currentNodeId) ? edge.To : edge.From; // Get the neighbor ID

                    // Skip if the neighbor is already finalized
                    if (finalizedNodes.Contains(neighborId)) continue;

                    if (_graph.Nodes.TryGetValue(neighborId, out var neighborNode))
                    {
                        double weight = _graph.IsWeighted ? edge.Weight : 1.0; // Use 1.0 for unweighted edges if needed
                        double newDist = currentNode.Distance + weight;

                        // --- Visualization Step: Considering Edge ---
                        var considerEdgeStep = new AlgorithmStep<TNodeData, TEdgeData>($"Xem xét cạnh ({GetNodeLabel(currentNodeId)} -> {GetNodeLabel(neighborId)}), trọng số {weight:F1}")
                        {
                            HighlightedNodes = new HashSet<int>(finalizedNodes) { neighborId }, // Highlight finalized + neighbor
                            HighlightedEdges = new HashSet<(int, int)>(shortestPathTreeEdges) { (currentNodeId, neighborId) }, // Highlight tree + current edge
                            NodeLabels = GetCurrentDistanceLabels()
                        };
                        steps.Add(considerEdgeStep);
                        // --- End Visualization Step ---


                        // If a shorter path to the neighbor is found
                        if (newDist < neighborNode.Distance)
                        {
                            neighborNode.Distance = newDist;
                            neighborNode.Predecessor = currentNode;
                            // Add or update the neighbor in the priority queue
                            priorityQueue.Enqueue(neighborId, newDist); // PQ handles updates or adds duplicates (take lowest)

                            // --- Visualization Step: Relaxing Edge ---
                            steps.Add(new AlgorithmStep<TNodeData, TEdgeData>($"Cập nhật khoảng cách {GetNodeLabel(neighborId)} thành {newDist:F1} thông qua {GetNodeLabel(currentNodeId)}")
                            {
                                HighlightedNodes = new HashSet<int>(finalizedNodes) { neighborId },
                                HighlightedEdges = new HashSet<(int, int)>(shortestPathTreeEdges) { (currentNodeId, neighborId) }, // Keep edge highlighted
                                NodeLabels = GetCurrentDistanceLabels(neighborId, newDist) // Update label for the relaxed node
                            });
                            // --- End Visualization Step ---
                        }
                        else
                        {
                            // Optional: Step indicating no update was needed
                            // steps.Add(new AlgorithmStep<TNodeData, TEdgeData>($"Khoảng cách đến {GetNodeLabel(neighborId)} ({neighborNode.Distance:F1}) không được cải thiện."));
                        }
                    }
                }
            }

            // After loop, check if destination was reached and reconstruct path for final highlight
            if (_graph.Nodes[endNodeId].Distance == double.PositiveInfinity)
            {
                steps.Add(new AlgorithmStep<TNodeData, TEdgeData>($"Không tìm thấy đường đi từ {GetNodeLabel(startNodeId)} đến {GetNodeLabel(endNodeId)}.")
                {
                    HighlightedNodes = new HashSet<int>(finalizedNodes),
                    HighlightedEdges = new HashSet<(int, int)>(shortestPathTreeEdges),
                    NodeLabels = GetCurrentDistanceLabels()
                });
            }
            else
            {
                // Reconstruct the shortest path for final visualization
                var finalPathNodes = new HashSet<int>();
                var finalPathEdges = new HashSet<(int from, int to)>();
                int? pathNodeId = endNodeId;
                while (pathNodeId.HasValue)
                {
                    finalPathNodes.Add(pathNodeId.Value);
                    var node = _graph.Nodes[pathNodeId.Value];
                    if (node.Predecessor != null)
                    {
                        finalPathEdges.Add((node.Predecessor.Id, pathNodeId.Value));
                        pathNodeId = node.Predecessor.Id;
                    }
                    else
                    {
                        pathNodeId = null; // Reached start node
                    }
                }
                steps.Add(new AlgorithmStep<TNodeData, TEdgeData>($"Dijkstra hoàn thành. Đường đi ngắn nhất từ {GetNodeLabel(startNodeId)} đến {GetNodeLabel(endNodeId)} có khoảng cách {_graph.Nodes[endNodeId].Distance:F1}.")
                {
                    HighlightedNodes = finalPathNodes, // Highlight only nodes in the final path
                    HighlightedEdges = finalPathEdges, // Highlight only edges in the final path
                    NodeLabels = GetCurrentDistanceLabels() // Show final distances
                });
            }

            return steps;
        }

        // Helper to get node labels with current distances for visualization
        private Dictionary<int, string> GetCurrentDistanceLabels(int? updatedNodeId = null, double? updatedDistance = null)
        {
            var labels = new Dictionary<int, string>();
            foreach (var node in _graph.Nodes.Values)
            {
                string label = GetNodeLabel(node.Id);
                string distStr = "";
                if (node.Id == updatedNodeId && updatedDistance.HasValue)
                {
                    distStr = updatedDistance.Value == double.PositiveInfinity ? "\u221E" : $"{updatedDistance.Value:F1}"; // Infinity symbol or formatted distance
                }
                else if (node.Distance != double.PositiveInfinity)
                {
                    distStr = $"{node.Distance:F1}";
                }

                if (!string.IsNullOrEmpty(distStr))
                {
                    labels.Add(node.Id, $"{label} ({distStr})");
                }
                // else keep default label if distance is infinity and not just updated
            }
            return labels;
        }


        // Helper to get a display label for a node
        private string GetNodeLabel(int nodeId)
        {
            if (_graph.Nodes.TryGetValue(nodeId, out var node) && node.Data != null)
            {
                return node.Data.ToString() ?? nodeId.ToString();
            }
            return nodeId.ToString();
        }
    }
}
