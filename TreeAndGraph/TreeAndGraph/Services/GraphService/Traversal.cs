using System.Collections.Generic;
using System.Linq;
using TreeAndGraph.Models.GraphModel;

namespace TreeAndGraph.Services.GraphService
{
    public class Traversal<TNodeData, TEdgeData>
    {
        private readonly Graph<TNodeData, TEdgeData> _graph;

        /// <summary>
        /// Initializes a new instance of the Traversal class.
        /// </summary>
        /// <param name="graph">The graph instance to perform traversal on.</param>
        public Traversal(Graph<TNodeData, TEdgeData> graph)
        {
            _graph = graph;
        }

        /// <summary>
        /// Performs Depth First Search starting from a given node.
        /// </summary>
        /// <param name="startNodeId">The ID of the node to start the search from.</param>
        /// <returns>A list of AlgorithmStep objects detailing the DFS process for visualization.</returns>
        public List<AlgorithmStep<TNodeData, TEdgeData>> DFS(int startNodeId)
        {
            var steps = new List<AlgorithmStep<TNodeData, TEdgeData>>();
            var visited = new HashSet<int>();
            var stack = new Stack<int>(); // Use Stack for DFS

            // Ensure the graph state is clean before starting
            _graph.ResetAlgorithmState();

            if (!_graph.Nodes.ContainsKey(startNodeId))
            {
                steps.Add(new AlgorithmStep<TNodeData, TEdgeData>($"Lỗi: Nút bắt đầu {startNodeId} không tồn tại."));
                return steps;
            }

            stack.Push(startNodeId);

            steps.Add(new AlgorithmStep<TNodeData, TEdgeData>($"Bắt đầu DFS từ nút {GetNodeLabel(startNodeId)}")
            {
                HighlightedNodes = { startNodeId }
            });

            while (stack.Count > 0)
            {
                var currentNodeId = stack.Pop();

                // If already visited (can happen if pushed multiple times before visiting), skip.
                if (visited.Contains(currentNodeId))
                {
                    continue;
                }

                visited.Add(currentNodeId);
                _graph.Nodes[currentNodeId].Visited = true;

                // --- Visualization Step: Visiting Node ---
                var visitStep = new AlgorithmStep<TNodeData, TEdgeData>($"Thăm nút {GetNodeLabel(currentNodeId)}")
                {
                    HighlightedNodes = new HashSet<int>(visited) // Highlight all visited nodes so far
                };
                // Highlight the edge that led to this node (if applicable)
                var predecessorId = _graph.Nodes[currentNodeId].Predecessor?.Id;
                if (predecessorId.HasValue)
                {
                    // Add edge from predecessor to current node
                    visitStep.HighlightedEdges.Add((predecessorId.Value, currentNodeId));
                    // For undirected, visualization might need to handle both directions or Graph.cs ensures edge uniqueness
                }
                // Add previously highlighted edges from the path taken
                steps.LastOrDefault()?.HighlightedEdges.ToList().ForEach(e => visitStep.HighlightedEdges.Add(e));
                steps.Add(visitStep);
                // --- End Visualization Step ---


                // Explore neighbors
                // Process neighbors in a specific order (e.g., reverse to mimic typical recursive DFS stack behavior)
                var neighbors = _graph.GetNeighbors(currentNodeId).OrderBy(n => n.Id).ToList(); // Consistent order
                foreach (var neighbor in neighbors)
                {
                    if (!visited.Contains(neighbor.Id))
                    {
                        // --- Visualization Step: Exploring Edge ---
                        var exploreStep = new AlgorithmStep<TNodeData, TEdgeData>($"Xem xét cạnh ({GetNodeLabel(currentNodeId)} -> {GetNodeLabel(neighbor.Id)})")
                        {
                            HighlightedNodes = new HashSet<int>(visited) { neighbor.Id }, // Highlight visited + the neighbor being considered
                            HighlightedEdges = new HashSet<(int, int)>(visitStep.HighlightedEdges) // Copy edges from previous step
                        };
                        exploreStep.HighlightedEdges.Add((currentNodeId, neighbor.Id)); // Highlight the edge being explored
                        steps.Add(exploreStep);
                        // --- End Visualization Step ---

                        stack.Push(neighbor.Id);
                        _graph.Nodes[neighbor.Id].Predecessor = _graph.Nodes[currentNodeId]; // Keep track of path
                    }
                    else
                    {
                        // Optional: Add a step indicating a visited neighbor was skipped
                        // steps.Add(new AlgorithmStep<TNodeData, TEdgeData>($"Bỏ qua nút đã thăm {GetNodeLabel(neighbor.Id)}"));
                    }
                }
            }

            steps.Add(new AlgorithmStep<TNodeData, TEdgeData>("DFS hoàn thành.") { HighlightedNodes = new HashSet<int>(visited) });
            return steps;
        }

        /// <summary>
        /// Performs Breadth First Search starting from a given node.
        /// </summary>
        /// <param name="startNodeId">The ID of the node to start the search from.</param>
        /// <returns>A list of AlgorithmStep objects detailing the BFS process for visualization.</returns>
        public List<AlgorithmStep<TNodeData, TEdgeData>> BFS(int startNodeId)
        {
            var steps = new List<AlgorithmStep<TNodeData, TEdgeData>>();
            var visited = new HashSet<int>();
            var queue = new Queue<int>(); // Use Queue for BFS
            var currentPathEdges = new HashSet<(int from, int to)>(); // Track edges in the BFS tree

            _graph.ResetAlgorithmState();

            if (!_graph.Nodes.ContainsKey(startNodeId))
            {
                steps.Add(new AlgorithmStep<TNodeData, TEdgeData>($"Lỗi: Nút bắt đầu {startNodeId} không tồn tại."));
                return steps;
            }

            visited.Add(startNodeId);
            _graph.Nodes[startNodeId].Visited = true;
            _graph.Nodes[startNodeId].Distance = 0; // Distance from start
            queue.Enqueue(startNodeId);

            steps.Add(new AlgorithmStep<TNodeData, TEdgeData>($"Bắt đầu BFS từ nút {GetNodeLabel(startNodeId)}")
            {
                HighlightedNodes = { startNodeId }
            });

            while (queue.Count > 0)
            {
                var currentNodeId = queue.Dequeue();

                // --- Visualization Step: Dequeueing/Visiting Node ---
                var visitStep = new AlgorithmStep<TNodeData, TEdgeData>($"Thăm nút {GetNodeLabel(currentNodeId)} (Dequeue)")
                {
                    HighlightedNodes = new HashSet<int>(visited), // Highlight all visited nodes
                    HighlightedEdges = new HashSet<(int, int)>(currentPathEdges) // Highlight edges forming the BFS tree
                };
                steps.Add(visitStep);
                // --- End Visualization Step ---


                // Explore neighbors
                var neighbors = _graph.GetNeighbors(currentNodeId).OrderBy(n => n.Id).ToList(); // Consistent order
                foreach (var neighbor in neighbors)
                {
                    if (!visited.Contains(neighbor.Id))
                    {
                        visited.Add(neighbor.Id);
                        _graph.Nodes[neighbor.Id].Visited = true;
                        _graph.Nodes[neighbor.Id].Predecessor = _graph.Nodes[currentNodeId];
                        _graph.Nodes[neighbor.Id].Distance = _graph.Nodes[currentNodeId].Distance + 1;
                        queue.Enqueue(neighbor.Id);

                        // Add the edge to our path highlight set
                        currentPathEdges.Add((currentNodeId, neighbor.Id));

                        // --- Visualization Step: Enqueueing Neighbor ---
                        steps.Add(new AlgorithmStep<TNodeData, TEdgeData>($"Khám phá cạnh ({GetNodeLabel(currentNodeId)} -> {GetNodeLabel(neighbor.Id)}), thêm {GetNodeLabel(neighbor.Id)} vào hàng đợi")
                        {
                            HighlightedNodes = new HashSet<int>(visited), // Highlight all visited nodes including the new one
                            HighlightedEdges = new HashSet<(int, int)>(currentPathEdges) // Highlight the growing BFS tree
                        });
                        // --- End Visualization Step ---
                    }
                    else
                    {
                        // Optional: Add a step indicating a visited neighbor was skipped during exploration
                        // steps.Add(new AlgorithmStep<TNodeData, TEdgeData>($"Cạnh ({GetNodeLabel(currentNodeId)} -> {GetNodeLabel(neighbor.Id)}): Nút đích đã thăm."));
                    }
                }
            }

            steps.Add(new AlgorithmStep<TNodeData, TEdgeData>("BFS hoàn thành.")
            {
                HighlightedNodes = new HashSet<int>(visited),
                HighlightedEdges = new HashSet<(int, int)>(currentPathEdges)
            });
            return steps;
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
