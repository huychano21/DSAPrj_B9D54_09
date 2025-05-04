using System;
using System.Collections.Generic;
using System.Linq;
using TreeAndGraph.Models.GraphModel;

namespace TreeAndGraph.Services.GraphService
{
    /// <summary>
    /// Provides implementations for cycle detection algorithms like Euler and Hamilton cycles/paths.
    /// </summary>
    /// <typeparam name="TNodeData">The type of data stored in graph nodes.</typeparam>
    /// <typeparam name="TEdgeData">The type of data stored in graph edges.</typeparam>
    public class CycleDetection<TNodeData, TEdgeData>
    {
        private readonly Graph<TNodeData, TEdgeData> _graph;

        public CycleDetection(Graph<TNodeData, TEdgeData> graph)
        {
            _graph = graph;
        }

        // --- Euler Path/Cycle ---

        /// <summary>
        /// Checks for the existence of an Euler path or cycle in the graph.
        /// An Euler path visits every edge exactly once.
        /// An Euler cycle is an Euler path that starts and ends at the same node.
        /// Assumes the graph is connected (or checks the relevant component).
        /// </summary>
        /// <returns>A list of AlgorithmStep indicating the result and reasoning.</returns>
        public List<AlgorithmStep<TNodeData, TEdgeData>> CheckEulerian()
        {
            var steps = new List<AlgorithmStep<TNodeData, TEdgeData>>();
            if (_graph.Nodes.Count == 0)
            {
                steps.Add(new AlgorithmStep<TNodeData, TEdgeData>("Đồ thị trống, không có đường đi/chu trình Euler."));
                return steps;
            }

            // 1. Check Connectivity (basic check: all nodes with edges must be connected)
            // A more robust check involves running BFS/DFS from a node with degree > 0
            // and ensuring all other nodes with degree > 0 are visited.
            // For simplicity here, we'll focus on the degree condition.
            var nodesWithEdges = _graph.Nodes.Values.Where(n => _graph.GetDegree(n.Id) > 0).Select(n => n.Id).ToHashSet();
            if (nodesWithEdges.Count > 0)
            {
                // Simple connectivity check placeholder - needs actual BFS/DFS
                bool isConnected = CheckConnectivity(nodesWithEdges.First(), nodesWithEdges);
                if (!isConnected)
                {
                    steps.Add(new AlgorithmStep<TNodeData, TEdgeData>("Kiểm tra liên thông: Đồ thị (phần có cạnh) không liên thông. Không thể có đường đi/chu trình Euler."));
                    // Highlight nodes with edges to show the potentially disconnected components
                    steps.Last().HighlightedNodes = nodesWithEdges;
                    return steps;
                }
                else
                {
                    steps.Add(new AlgorithmStep<TNodeData, TEdgeData>("Kiểm tra liên thông: Đồ thị (phần có cạnh) liên thông.") { HighlightedNodes = nodesWithEdges });
                }
            }
            else
            {
                steps.Add(new AlgorithmStep<TNodeData, TEdgeData>("Đồ thị không có cạnh nào."));
                return steps; // Trivial case
            }


            // 2. Count nodes with odd degrees.
            int oddDegreeCount = 0;
            var oddDegreeNodes = new HashSet<int>();
            foreach (var nodeId in nodesWithEdges) // Only check nodes part of the main graph structure
            {
                int degree = _graph.GetDegree(nodeId); // Use appropriate degree (in+out for directed if needed)
                if (degree % 2 != 0)
                {
                    oddDegreeCount++;
                    oddDegreeNodes.Add(nodeId);
                }
            }

            steps.Add(new AlgorithmStep<TNodeData, TEdgeData>($"Kiểm tra bậc của các đỉnh: Tìm thấy {oddDegreeCount} đỉnh có bậc lẻ.")
            {
                HighlightedNodes = oddDegreeNodes // Highlight odd-degree nodes
            });


            // 3. Determine Euler path/cycle existence based on odd degree count.
            if (oddDegreeCount == 0)
            {
                steps.Add(new AlgorithmStep<TNodeData, TEdgeData>("Kết luận: Đồ thị có chu trình Euler (Tất cả các đỉnh đều có bậc chẵn và đồ thị liên thông)."));
            }
            else if (oddDegreeCount == 2)
            {
                steps.Add(new AlgorithmStep<TNodeData, TEdgeData>("Kết luận: Đồ thị có đường đi Euler (Có đúng 2 đỉnh bậc lẻ và đồ thị liên thông).")
                {
                    HighlightedNodes = oddDegreeNodes // Highlight the start/end nodes
                });
            }
            else
            {
                steps.Add(new AlgorithmStep<TNodeData, TEdgeData>($"Kết luận: Đồ thị không có đường đi hay chu trình Euler (Số đỉnh bậc lẻ là {oddDegreeCount}, không phải 0 hoặc 2).")
                {
                    HighlightedNodes = oddDegreeNodes
                });
            }

            // Note: Finding the actual path/cycle requires a different algorithm (e.g., Hierholzer's algorithm).
            // This method only checks for existence.

            return steps;
        }

        // Basic connectivity check using BFS/DFS (Helper for CheckEulerian)
        private bool CheckConnectivity(int startNodeId, HashSet<int> relevantNodes)
        {
            if (relevantNodes.Count <= 1) return true; // Trivial

            var visited = new HashSet<int>();
            var queue = new Queue<int>();

            visited.Add(startNodeId);
            queue.Enqueue(startNodeId);

            while (queue.Count > 0)
            {
                int u = queue.Dequeue();
                foreach (var neighbor in _graph.GetNeighbors(u))
                {
                    // Only consider neighbors that are part of the graph structure we care about
                    if (relevantNodes.Contains(neighbor.Id) && !visited.Contains(neighbor.Id))
                    {
                        visited.Add(neighbor.Id);
                        queue.Enqueue(neighbor.Id);
                    }
                }
            }

            // Check if all relevant nodes were visited
            return visited.Count == relevantNodes.Count;
        }


        // --- Hamilton Path/Cycle ---
        // WARNING: Finding Hamiltonian paths/cycles is NP-complete.
        // This backtracking implementation is only feasible for very small graphs.

        private List<int>? _hamiltonianPath = null;
        private bool _foundHamiltonianPath = false;
        private List<AlgorithmStep<TNodeData, TEdgeData>> _hamiltonianSteps = new();

        /// <summary>
        /// Attempts to find a Hamiltonian path (visits every node exactly once) using backtracking.
        /// </summary>
        /// <returns>A list of AlgorithmStep detailing the search process.</returns>
        public List<AlgorithmStep<TNodeData, TEdgeData>> FindHamiltonianPath()
        {
            _hamiltonianSteps = new List<AlgorithmStep<TNodeData, TEdgeData>>();
            _hamiltonianPath = null;
            _foundHamiltonianPath = false;

            if (_graph.Nodes.Count == 0)
            {
                _hamiltonianSteps.Add(new AlgorithmStep<TNodeData, TEdgeData>("Hamilton: Đồ thị trống."));
                return _hamiltonianSteps;
            }

            _hamiltonianSteps.Add(new AlgorithmStep<TNodeData, TEdgeData>("Bắt đầu tìm đường đi Hamilton (Backtracking)."));

            // Try starting from each node
            foreach (var startNodeId in _graph.Nodes.Keys)
            {
                var path = new List<int> { startNodeId };
                var visited = new HashSet<int> { startNodeId };

                _hamiltonianSteps.Add(new AlgorithmStep<TNodeData, TEdgeData>($"Thử bắt đầu từ nút {GetNodeLabel(startNodeId)}") { HighlightedNodes = { startNodeId } });

                if (HamiltonianBacktrack(startNodeId, path, visited))
                {
                    _foundHamiltonianPath = true;
                    break; // Found one path, stop searching
                }
                else if (!_foundHamiltonianPath) // Add step only if path wasn't found starting here
                {
                    _hamiltonianSteps.Add(new AlgorithmStep<TNodeData, TEdgeData>($"Không tìm thấy đường đi bắt đầu từ {GetNodeLabel(startNodeId)}."));
                }
            }

            if (_foundHamiltonianPath && _hamiltonianPath != null)
            {
                var finalStep = new AlgorithmStep<TNodeData, TEdgeData>($"Tìm thấy đường đi Hamilton: {string.Join(" -> ", _hamiltonianPath.Select(GetNodeLabel))}");
                finalStep.HighlightedNodes = new HashSet<int>(_hamiltonianPath);
                for (int i = 0; i < _hamiltonianPath.Count - 1; i++)
                {
                    finalStep.HighlightedEdges.Add((_hamiltonianPath[i], _hamiltonianPath[i + 1]));
                }
                _hamiltonianSteps.Add(finalStep);
            }
            else
            {
                _hamiltonianSteps.Add(new AlgorithmStep<TNodeData, TEdgeData>("Không tìm thấy đường đi Hamilton nào trong đồ thị."));
            }

            return _hamiltonianSteps;
        }

        // Recursive backtracking function for Hamiltonian path
        private bool HamiltonianBacktrack(int u, List<int> path, HashSet<int> visited)
        {
            // Base case: If all nodes are included in the path
            if (path.Count == _graph.Nodes.Count)
            {
                _hamiltonianPath = new List<int>(path); // Store the found path
                return true; // Path found
            }

            // Try neighbors of the current node 'u'
            foreach (var v_node in _graph.GetNeighbors(u).OrderBy(n => n.Id)) // Consistent order
            {
                int v = v_node.Id;
                if (!visited.Contains(v))
                {
                    // Add neighbor to path and mark as visited
                    path.Add(v);
                    visited.Add(v);

                    // --- Visualization Step: Exploring ---
                    var step = new AlgorithmStep<TNodeData, TEdgeData>($"Thêm {GetNodeLabel(v)} vào đường đi: {string.Join("->", path.Select(GetNodeLabel))}");
                    step.HighlightedNodes = new HashSet<int>(path);
                    for (int i = 0; i < path.Count - 1; i++) { step.HighlightedEdges.Add((path[i], path[i + 1])); }
                    _hamiltonianSteps.Add(step);
                    // --- End Visualization Step ---


                    // Recur for the neighbor
                    if (HamiltonianBacktrack(v, path, visited))
                    {
                        return true; // Path found down this branch
                    }

                    // Backtrack: Remove neighbor from path and visited set
                    // --- Visualization Step: Backtracking ---
                    _hamiltonianSteps.Add(new AlgorithmStep<TNodeData, TEdgeData>($"Backtrack: Loại bỏ {GetNodeLabel(v)} khỏi đường đi.") { HighlightedNodes = new HashSet<int>(path) });
                    // --- End Visualization Step ---
                    path.RemoveAt(path.Count - 1);
                    visited.Remove(v);
                    _hamiltonianSteps.Last().HighlightedNodes.Remove(v); // Update highlight after backtrack


                }
            }

            return false; // No Hamiltonian path found from this state
        }

        // Note: Checking for Hamiltonian Cycle would involve:
        // 1. Finding a Hamiltonian path.
        // 2. Checking if there's an edge between the last node and the first node of the path.


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
