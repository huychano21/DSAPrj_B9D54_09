using System;
using System.Collections.Generic;
using System.Linq;
using TreeAndGraph.Models.GraphModel;

namespace TreeAndGraph.Services.GraphService
{
    public class SpanningTree<TNodeData, TEdgeData>
    {
        private readonly Graph<TNodeData, TEdgeData> _graph;

        /// <summary>
        /// Initializes a new instance of the SpanningTree class.
        /// </summary>
        /// <param name="graph">The graph instance to find the MST for. Should be weighted and ideally undirected.</param>
        public SpanningTree(Graph<TNodeData, TEdgeData> graph)
        {
            // MST algorithms typically work on connected, undirected, weighted graphs.
            // Add checks or warnings if needed.
            if (!graph.IsWeighted)
                Console.WriteLine("Warning: Running MST algorithm on an unweighted graph. Edge weights default to 1.0.");
            if (graph.IsDirected)
                Console.WriteLine("Warning: Running MST algorithm on a directed graph. Treating edges as undirected.");

            _graph = graph;
        }

        /// <summary>
        /// Implements Kruskal's algorithm to find a Minimum Spanning Tree (MST).
        /// Uses a Disjoint Set Union (DSU) data structure.
        /// </summary>
        /// <returns>A list of AlgorithmStep objects detailing the Kruskal process for visualization.</returns>
        public List<AlgorithmStep<TNodeData, TEdgeData>> Kruskal()
        {
            var steps = new List<AlgorithmStep<TNodeData, TEdgeData>>();
            var mstEdges = new List<Edge<TEdgeData>>();
            double totalWeight = 0;

            // 1. Sort all edges by weight in ascending order.
            var sortedEdges = _graph.Edges.OrderBy(e => e.Weight).ToList();

            steps.Add(new AlgorithmStep<TNodeData, TEdgeData>("Bắt đầu Kruskal. Sắp xếp các cạnh theo trọng số."));

            // 2. Initialize Disjoint Set Union (DSU) structure for all nodes.
            var dsu = new DisjointSetUnion(_graph.Nodes.Keys);

            // 3. Iterate through sorted edges.
            foreach (var edge in sortedEdges)
            {
                // --- Visualization Step: Considering Edge ---
                steps.Add(new AlgorithmStep<TNodeData, TEdgeData>($"Xem xét cạnh ({GetNodeLabel(edge.From)} - {GetNodeLabel(edge.To)}), trọng số {edge.Weight:F1}")
                {
                    HighlightedEdges = { (edge.From, edge.To) }, // Highlight the edge being considered
                                                                 // Optionally highlight current MST edges
                                                                 // HighlightedEdges = new HashSet<(int, int)>(mstEdges.Select(e => (e.From, e.To))) { (edge.From, edge.To) }
                });
                // --- End Visualization Step ---

                // 4. Check if adding the edge creates a cycle using DSU.
                if (dsu.Find(edge.From) != dsu.Find(edge.To))
                {
                    // 5. If no cycle, add the edge to the MST and union the sets.
                    mstEdges.Add(edge);
                    totalWeight += edge.Weight;
                    dsu.Union(edge.From, edge.To);

                    // --- Visualization Step: Adding Edge to MST ---
                    steps.Add(new AlgorithmStep<TNodeData, TEdgeData>($"Thêm cạnh ({GetNodeLabel(edge.From)} - {GetNodeLabel(edge.To)}) vào MST. Không tạo chu trình.")
                    {
                        HighlightedEdges = new HashSet<(int, int)>(mstEdges.Select(e => (e.From, e.To))), // Highlight all MST edges
                        HighlightedNodes = GetNodesFromEdges(mstEdges) // Highlight nodes connected by MST
                    });
                    // --- End Visualization Step ---

                    // Optimization: Stop if we have N-1 edges (where N is the number of nodes)
                    if (mstEdges.Count == _graph.Nodes.Count - 1)
                    {
                        break;
                    }
                }
                else
                {
                    // --- Visualization Step: Skipping Edge ---
                    steps.Add(new AlgorithmStep<TNodeData, TEdgeData>($"Bỏ qua cạnh ({GetNodeLabel(edge.From)} - {GetNodeLabel(edge.To)}). Sẽ tạo chu trình.")
                    {
                        HighlightedEdges = new HashSet<(int, int)>(mstEdges.Select(e => (e.From, e.To))), // Keep MST highlighted
                        HighlightedNodes = GetNodesFromEdges(mstEdges)
                        // Optionally dim or cross out the skipped edge: needs JS support
                    });
                    // --- End Visualization Step ---
                }
            }

            // Final step
            if (mstEdges.Count < _graph.Nodes.Count - 1 && _graph.Nodes.Count > 1)
            {
                steps.Add(new AlgorithmStep<TNodeData, TEdgeData>($"Kruskal hoàn thành. Đồ thị có thể không liên thông. MST tìm được có trọng số {totalWeight:F1}.")
                {
                    HighlightedEdges = new HashSet<(int, int)>(mstEdges.Select(e => (e.From, e.To))),
                    HighlightedNodes = GetNodesFromEdges(mstEdges)
                });
            }
            else if (_graph.Nodes.Count <= 1)
            {
                steps.Add(new AlgorithmStep<TNodeData, TEdgeData>($"Kruskal hoàn thành. Đồ thị có {_graph.Nodes.Count} nút.")
                {
                    HighlightedNodes = new HashSet<int>(_graph.Nodes.Keys)
                });
            }
            else
            {
                steps.Add(new AlgorithmStep<TNodeData, TEdgeData>($"Kruskal hoàn thành. MST có trọng số {totalWeight:F1}.")
                {
                    HighlightedEdges = new HashSet<(int, int)>(mstEdges.Select(e => (e.From, e.To))),
                    HighlightedNodes = GetNodesFromEdges(mstEdges)
                });
            }

            return steps;
        }


        /// <summary>
        /// Implements Prim's algorithm to find a Minimum Spanning Tree (MST).
        /// Starts from an arbitrary node and grows the tree. Uses a Priority Queue.
        /// </summary>
        /// <param name="startNodeId">Optional starting node ID. If null, picks the first node.</param>
        /// <returns>A list of AlgorithmStep objects detailing the Prim process for visualization.</returns>
        public List<AlgorithmStep<TNodeData, TEdgeData>> Prim(int? startNodeId = null)
        {
            var steps = new List<AlgorithmStep<TNodeData, TEdgeData>>();
            if (_graph.Nodes.Count == 0)
            {
                steps.Add(new AlgorithmStep<TNodeData, TEdgeData>("Prim: Đồ thị trống."));
                return steps;
            }

            // Choose a starting node if not provided
            int actualStartNodeId = startNodeId ?? _graph.Nodes.Keys.First();

            if (!_graph.Nodes.ContainsKey(actualStartNodeId))
            {
                steps.Add(new AlgorithmStep<TNodeData, TEdgeData>($"Prim: Nút bắt đầu {actualStartNodeId} không tồn tại."));
                return steps;
            }

            var mstNodes = new HashSet<int>(); // Nodes included in the MST so far
            var mstEdges = new HashSet<(int from, int to)>(); // Edges included in the MST
            double totalWeight = 0;
            // Priority Queue stores potential edges to add. Key: Edge (or target node), Value: Edge Weight.
            // We store Tuple<(int from, int to, double weight)> for easier access. PQ orders by weight.
            var priorityQueue = new PriorityQueue<(int from, int to, double weight), double>();
            // Keep track of the edge that connects a node to the MST with the minimum weight found so far
            var minEdgeToNode = new Dictionary<int, (int from, int to, double weight)?>();


            _graph.ResetAlgorithmState(); // Use Distance/Predecessor or manage state here

            steps.Add(new AlgorithmStep<TNodeData, TEdgeData>($"Bắt đầu Prim từ nút {GetNodeLabel(actualStartNodeId)}."));

            // Initialize: Add the start node
            mstNodes.Add(actualStartNodeId);
            AddEdgesToQueue(actualStartNodeId, priorityQueue, mstNodes, steps);


            while (priorityQueue.Count > 0 && mstNodes.Count < _graph.Nodes.Count)
            {
                // Get the minimum weight edge connecting a node in MST to a node outside MST
                (int from, int to, double weight) minEdge;
                do
                {
                    if (priorityQueue.Count == 0) goto FinishPrim; // No more edges to consider
                    minEdge = priorityQueue.Dequeue();
                } while (mstNodes.Contains(minEdge.to)); // Ensure the target node isn't already in MST

                // Add the new node and edge to the MST
                int newNodeId = minEdge.to;
                mstNodes.Add(newNodeId);
                mstEdges.Add((minEdge.from, minEdge.to));
                totalWeight += minEdge.weight;

                // --- Visualization Step: Adding Node and Edge ---
                steps.Add(new AlgorithmStep<TNodeData, TEdgeData>($"Thêm nút {GetNodeLabel(newNodeId)} và cạnh ({GetNodeLabel(minEdge.from)} - {GetNodeLabel(newNodeId)}) vào MST, trọng số {minEdge.weight:F1}")
                {
                    HighlightedNodes = new HashSet<int>(mstNodes),
                    HighlightedEdges = new HashSet<(int, int)>(mstEdges)
                });
                // --- End Visualization Step ---

                // Add/update edges from the newly added node to the priority queue
                AddEdgesToQueue(newNodeId, priorityQueue, mstNodes, steps);

            }

        FinishPrim:
            // Final step message
            if (mstNodes.Count < _graph.Nodes.Count && _graph.Nodes.Count > 1)
            {
                steps.Add(new AlgorithmStep<TNodeData, TEdgeData>($"Prim hoàn thành. Đồ thị có thể không liên thông. MST tìm được có trọng số {totalWeight:F1}.")
                {
                    HighlightedEdges = new HashSet<(int, int)>(mstEdges),
                    HighlightedNodes = new HashSet<int>(mstNodes)
                });
            }
            else
            {
                steps.Add(new AlgorithmStep<TNodeData, TEdgeData>($"Prim hoàn thành. MST có trọng số {totalWeight:F1}.")
                {
                    HighlightedEdges = new HashSet<(int, int)>(mstEdges),
                    HighlightedNodes = new HashSet<int>(mstNodes)
                });
            }

            return steps;
        }

        // Helper for Prim's: Add edges from a newly added node to the PQ
        private void AddEdgesToQueue(int nodeId, PriorityQueue<(int from, int to, double weight), double> pq, HashSet<int> mstNodes, List<AlgorithmStep<TNodeData, TEdgeData>> steps)
        {
            // --- Visualization Step: Exploring Edges from New Node ---
            var exploreStep = new AlgorithmStep<TNodeData, TEdgeData>($"Xem xét các cạnh từ nút mới {GetNodeLabel(nodeId)}")
            {
                HighlightedNodes = new HashSet<int>(mstNodes),
                HighlightedEdges = new HashSet<(int, int)>(mstNodes.Count > 1 ? steps.Last().HighlightedEdges : new HashSet<(int, int)>()) // Keep MST highlighted
            };
            bool addedExploreStep = false;
            // --- End Visualization Step ---

            foreach (var edge in _graph.GetAdjacentEdges(nodeId))
            {
                // Determine the neighbor node ID (the one that is NOT nodeId)
                int neighborId = (edge.From == nodeId) ? edge.To : edge.From;

                // Only consider edges leading to nodes not yet in the MST
                if (!mstNodes.Contains(neighborId))
                {
                    double weight = _graph.IsWeighted ? edge.Weight : 1.0;
                    // Add edge to PQ. The PQ will handle finding the minimum.
                    pq.Enqueue((nodeId, neighborId, weight), weight);

                    // Add highlight for considered edge in the visualization step
                    exploreStep.HighlightedEdges.Add((nodeId, neighborId));
                    addedExploreStep = true;
                }
            }
            // Add the exploration step only if new edges were actually considered
            if (addedExploreStep)
            {
                steps.Add(exploreStep);
            }
        }


        // Helper to get all unique node IDs involved in a list of edges
        private HashSet<int> GetNodesFromEdges(IEnumerable<Edge<TEdgeData>> edges)
        {
            var nodeIds = new HashSet<int>();
            foreach (var edge in edges)
            {
                nodeIds.Add(edge.From);
                nodeIds.Add(edge.To);
            }
            return nodeIds;
        }
        private HashSet<int> GetNodesFromEdges(IEnumerable<(int from, int to)> edges)
        {
            var nodeIds = new HashSet<int>();
            foreach (var edge in edges)
            {
                nodeIds.Add(edge.from);
                nodeIds.Add(edge.to);
            }
            return nodeIds;
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


        // --- Disjoint Set Union (DSU) Helper Class for Kruskal's ---
        private class DisjointSetUnion
        {
            private readonly Dictionary<int, int> parent;
            private readonly Dictionary<int, int> rank; // For optimization (union by rank)

            public DisjointSetUnion(IEnumerable<int> elements)
            {
                parent = new Dictionary<int, int>();
                rank = new Dictionary<int, int>();
                foreach (var element in elements)
                {
                    parent[element] = element; // Each element is its own parent initially
                    rank[element] = 0;         // Initial rank is 0
                }
            }

            // Find the representative (root) of the set containing element 'i' with path compression
            public int Find(int i)
            {
                if (parent[i] == i)
                    return i;
                // Path compression: Make the parent of i the root directly
                parent[i] = Find(parent[i]);
                return parent[i];
            }

            // Union the sets containing elements 'x' and 'y' using union by rank
            public void Union(int x, int y)
            {
                int rootX = Find(x);
                int rootY = Find(y);

                if (rootX != rootY) // Only union if they are in different sets
                {
                    // Union by rank: Attach the shorter tree to the taller tree
                    if (rank[rootX] < rank[rootY])
                    {
                        parent[rootX] = rootY;
                    }
                    else if (rank[rootX] > rank[rootY])
                    {
                        parent[rootY] = rootX;
                    }
                    else
                    {
                        // Ranks are equal, arbitrarily make one root the parent and increment its rank
                        parent[rootY] = rootX;
                        rank[rootX]++;
                    }
                }
            }
        }
    }
}
