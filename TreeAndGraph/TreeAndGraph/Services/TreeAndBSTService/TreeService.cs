// File: Services/TreeService.cs
using TreeAndGraph.Models.TreeAndBST; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization; // Cần cho CultureInfo.InvariantCulture

namespace TreeAndGraph.Services.TreeAndBSTService // Namespace từ code bạn gửi - Đảm bảo đúng với cấu trúc dự án
{
    public class TreeService : ITreeService
    {
        private Random _random = new Random();
        private int _newNodeCounter = 1; // For random node naming

        // --- Helper: Reset state for new tree generation ---
        private void ResetRandomCounter() => _newNodeCounter = 1;
        private string GenerateNewNodeData()
        {
            int num = _newNodeCounter++;
            string name = "";
            while (num > 0) { int r = (num - 1) % 26; name = (char)(65 + r) + name; num = (num - 1) / 26; }
            return string.IsNullOrEmpty(name) ? "A" : name;
        }

        // --- General Tree Parsing ---
        public TreeNode? ParseParentChildList(string input, bool isBinary, out List<string> errors)
        {
            errors = new List<string>();
            var nodeMap = new Dictionary<string, TreeNode>();
            var parentChildPairs = new List<(string Parent, string Child)>();
            var allNodeData = new HashSet<string>();

            var lines = input.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var parts = line.Split("->", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (parts.Length == 2 && !string.IsNullOrWhiteSpace(parts[0]) && !string.IsNullOrWhiteSpace(parts[1]))
                {
                    parentChildPairs.Add((parts[0], parts[1]));
                    allNodeData.Add(parts[0]);
                    allNodeData.Add(parts[1]);
                }
                else if (!string.IsNullOrWhiteSpace(line)) { errors.Add($"Định dạng không hợp lệ, bỏ qua dòng: '{line}'"); }
            }

            if (!parentChildPairs.Any() && allNodeData.Count == 1) { return GetOrCreateNode(nodeMap, allNodeData.First()); }
            if (!parentChildPairs.Any()) { errors.Add("Không tìm thấy cặp cha-con hợp lệ."); return null; }

            var parents = parentChildPairs.Select(p => p.Parent).ToHashSet();
            var children = parentChildPairs.Select(p => p.Child).ToHashSet();
            var roots = parents.Except(children).ToList();

            if (!roots.Any() && parents.Any()) roots.Add(parentChildPairs.First().Parent);
            roots.AddRange(allNodeData.Except(parents).Except(children));
            roots = roots.Distinct().ToList();

            if (roots.Count != 1) { errors.Add($"Lỗi: Tìm thấy {roots.Count} gốc tiềm năng ({string.Join(", ", roots)}). Cần có đúng 1 gốc."); return null; }

            TreeNode rootNode = GetOrCreateNode(nodeMap, roots[0]);
            var addedEdges = new HashSet<(string, string)>();
            var queue = new Queue<TreeNode>();
            queue.Enqueue(rootNode);

            while (queue.Count > 0)
            {
                TreeNode currentParentNode = queue.Dequeue();
                var childrenOfCurrent = parentChildPairs.Where(p => p.Parent == currentParentNode.Data).ToList();
                int addedChildCount = 0;

                foreach (var (_, childData) in childrenOfCurrent)
                {
                    if (addedEdges.Contains((currentParentNode.Data, childData))) continue;
                    TreeNode childNode = GetOrCreateNode(nodeMap, childData);
                    bool added = AddChildNode(currentParentNode, childNode, isBinary, errors);
                    if (added)
                    {
                        addedChildCount++;
                        if (isBinary && addedChildCount > 2) break;
                        addedEdges.Add((currentParentNode.Data, childData));
                        if (!queue.Any(n => n.Id == childNode.Id)) queue.Enqueue(childNode);
                    }
                }
            }
            return rootNode;
        }

        public TreeNode? ParseNestedString(string input, bool isBinary, out List<string> errors)
        {
            errors = new List<string>();
            if (string.IsNullOrWhiteSpace(input)) return null;
            var nodeMap = new Dictionary<string, TreeNode>();
            int index = 0;
            try
            {
                TreeNode? root = ParseNodeRecursive(input, ref index, nodeMap, isBinary, errors);
                if (index < input.Length)
                {
                    while (index < input.Length && char.IsWhiteSpace(input[index])) index++;
                    if (index < input.Length) errors.Add($"Phân tích không hoàn chỉnh, còn dư ký tự tại vị trí {index}: '{input.Substring(index)}'");
                }
                return root;
            }
            catch (FormatException ex) { errors.Add($"Lỗi định dạng chuỗi: {ex.Message}"); return null; }
            catch (Exception ex) { errors.Add($"Lỗi không mong đợi khi phân tích chuỗi: {ex.Message}"); return null; }
        }

        private TreeNode? ParseNodeRecursive(string input, ref int index, Dictionary<string, TreeNode> nodeMap, bool isBinary, List<string> errors)
        {
            while (index < input.Length && char.IsWhiteSpace(input[index])) index++;
            if (index >= input.Length || !char.IsLetterOrDigit(input[index]))
            {
                if (index < input.Length && (input[index] == ')' || input[index] == ',')) return null;
                if (index < input.Length) throw new FormatException($"Ký tự không mong đợi '{input[index]}' tại vị trí {index}.");
                return null;
            }
            int startIndex = index;
            while (index < input.Length && char.IsLetterOrDigit(input[index])) index++;
            string nodeData = input.Substring(startIndex, index - startIndex);
            TreeNode node = GetOrCreateNode(nodeMap, nodeData);
            while (index < input.Length && char.IsWhiteSpace(input[index])) index++;
            if (index < input.Length && input[index] == '(')
            {
                index++; int childCount = 0;
                while (index < input.Length)
                {
                    while (index < input.Length && char.IsWhiteSpace(input[index])) index++;
                    if (index >= input.Length) throw new FormatException($"Thiếu dấu ')' đóng tại vị trí {index}.");
                    if (input[index] == ')') { index++; break; }
                    TreeNode? child = ParseNodeRecursive(input, ref index, nodeMap, isBinary, errors);
                    if (child != null) { bool added = AddChildNode(node, child, isBinary, errors); if (added) childCount++; if (isBinary && childCount > 2) { /* Error added by AddChildNode */ } }
                    while (index < input.Length && char.IsWhiteSpace(input[index])) index++;
                    if (index < input.Length && input[index] == ',') index++;
                    else if (index < input.Length && input[index] != ')') throw new FormatException($"Ký tự không mong đợi '{input[index]}' tại vị trí {index}. Mong đợi ',' hoặc ')'.");
                }
            }
            return node;
        }

        private TreeNode GetOrCreateNode(Dictionary<string, TreeNode> nodeMap, string data)
        {
            if (!nodeMap.TryGetValue(data, out var node)) { node = new TreeNode(data); nodeMap[data] = node; }
            return node;
        }

        private bool AddChildNode(TreeNode parent, TreeNode child, bool isBinary, List<string> errors)
        {
            if (parent == child) { errors.Add($"Lỗi: Không thể thêm nút '{child.Data}' làm con của chính nó."); return false; }
            if (isBinary)
            {
                if (parent.Left == null) { parent.Left = child; return true; }
                if (parent.Right == null) { parent.Right = child; return true; }
                errors.Add($"Cảnh báo: Nút '{parent.Data}' đã có 2 con (nhị phân). Bỏ qua việc thêm '{child.Data}'."); return false;
            }
            else { if (!parent.Children.Any(c => c.Id == child.Id)) { parent.Children.Add(child); return true; } return false; }
        }

        // --- Random Tree Generation ---
        public TreeNode GenerateRandomTree(int minNodes, int maxNodes, int maxDepth, bool isBinary)
        {
            ResetRandomCounter(); var nodeMap = new Dictionary<string, TreeNode>();
            int nodeCount = _random.Next(minNodes, maxNodes + 1); string rootData = GenerateNewNodeData();
            TreeNode root = GetOrCreateNode(nodeMap, rootData); int nodesToCreate = nodeCount - 1;
            GenerateRandomSubtreeRecursive(root, ref nodesToCreate, maxDepth, 1, nodeMap, isBinary);
            return root;
        }

        private void GenerateRandomSubtreeRecursive(TreeNode parent, ref int remainingNodes, int maxDepth, int currentDepth, Dictionary<string, TreeNode> nodeMap, bool isBinary)
        {
            if (remainingNodes <= 0 || currentDepth >= maxDepth) return;
            int childrenToAttempt = isBinary ? 2 : _random.Next(1, 4); double creationProbability = 0.65;
            for (int i = 0; i < childrenToAttempt && remainingNodes > 0; i++)
            {
                if (_random.NextDouble() < creationProbability)
                {
                    string childData = GenerateNewNodeData(); TreeNode childNode = GetOrCreateNode(nodeMap, childData); bool added = false;
                    if (isBinary) { if (i == 0 && parent.Left == null) { parent.Left = childNode; added = true; } else if (i == 1 && parent.Right == null) { parent.Right = childNode; added = true; } }
                    else { parent.Children.Add(childNode); added = true; }
                    if (added) { remainingNodes--; GenerateRandomSubtreeRecursive(childNode, ref remainingNodes, maxDepth, currentDepth + 1, nodeMap, isBinary); }
                }
            }
        }

        // ========================================================================
        // === PHẦN LAYOUT (Đã cập nhật ở bước trước)                         ===
        // ========================================================================

        private const double BaseNodeSize = 40;
        private const double SiblingSeparation = BaseNodeSize * 0.5;
        private const double SubtreeSeparation = BaseNodeSize;
        private const double LevelSeparation = BaseNodeSize * 2.0;

        private class LayoutNodeInfo<TNode>
        {
            public TNode Node { get; }
            public LayoutNodeInfo<TNode>? Parent { get; }
            public List<LayoutNodeInfo<TNode>> Children { get; } = new();
            public int Depth { get; }
            public double X { get; set; }
            public double Y { get; set; }
            public double Modifier { get; set; }
            public double PreliminaryX { get; set; }
            public LayoutNodeInfo<TNode>? LeftSibling => Parent?.Children.ElementAtOrDefault(Parent.Children.IndexOf(this) - 1);
            public LayoutNodeInfo(TNode node, LayoutNodeInfo<TNode>? parent, int depth) { Node = node; Parent = parent; Depth = depth; Y = depth * LevelSeparation; }
        }

        public TreeLayoutResult<TreeNode> CalculateTreeLayout(TreeNode root, bool isBinary)
        {
            var result = new TreeLayoutResult<TreeNode>(); if (root == null) return result;
            var nodeInfoMap = new Dictionary<string, LayoutNodeInfo<TreeNode>>();
            LayoutNodeInfo<TreeNode> rootInfo = BuildLayoutHierarchy(root, null, 0, nodeInfoMap, isBinary);
            FirstWalk(rootInfo, isBinary); SecondWalk(rootInfo, 0, nodeInfoMap);
            CalculateResultBoundsAndNormalize(nodeInfoMap.Values, result);
            return result;
        }

        private LayoutNodeInfo<TreeNode> BuildLayoutHierarchy(TreeNode node, LayoutNodeInfo<TreeNode>? parentInfo, int depth, Dictionary<string, LayoutNodeInfo<TreeNode>> map, bool isBinary)
        {
            var nodeInfo = new LayoutNodeInfo<TreeNode>(node, parentInfo, depth); map[node.Id] = nodeInfo;
            foreach (var child in GetNodeChildren(node, isBinary)) { nodeInfo.Children.Add(BuildLayoutHierarchy(child, nodeInfo, depth + 1, map, isBinary)); }
            return nodeInfo;
        }

        public TreeLayoutResult<BstNode> CalculateBstLayout(BstNode? root)
        {
            var result = new TreeLayoutResult<BstNode>(); if (root == null) return result;
            var nodeInfoMap = new Dictionary<string, LayoutNodeInfo<BstNode>>();
            LayoutNodeInfo<BstNode> rootInfo = BuildBstLayoutHierarchy(root, null, 0, nodeInfoMap);
            FirstWalk(rootInfo, true); SecondWalk(rootInfo, 0, nodeInfoMap);
            CalculateResultBoundsAndNormalize(nodeInfoMap.Values, result);
            return result;
        }

        private LayoutNodeInfo<BstNode> BuildBstLayoutHierarchy(BstNode node, LayoutNodeInfo<BstNode>? parentInfo, int depth, Dictionary<string, LayoutNodeInfo<BstNode>> map)
        {
            var nodeInfo = new LayoutNodeInfo<BstNode>(node, parentInfo, depth);
            map[node.Id] = nodeInfo;
            if (node.Left != null) nodeInfo.Children.Add(BuildBstLayoutHierarchy(node.Left, nodeInfo, depth + 1, map));
            if (node.Right != null) nodeInfo.Children.Add(BuildBstLayoutHierarchy(node.Right, nodeInfo, depth + 1, map));

            // *** SỬA LỖI Ở ĐÂY: Sắp xếp lại danh sách Children để Left luôn đứng trước Right ***
            if (nodeInfo.Children.Count == 2) // Chỉ cần sắp xếp nếu có cả hai con
            {
                nodeInfo.Children.Sort((a, b) => {
                    // Lấy đối tượng BstNode cha (là node hiện tại đang xử lý)
                    var parentBstNode = node;
                    // So sánh tham chiếu object để xác định con trái/phải
                    bool aIsLeft = ReferenceEquals(parentBstNode.Left, a.Node);
                    bool bIsLeft = ReferenceEquals(parentBstNode.Left, b.Node);

                    if (aIsLeft) return -1; // Nếu a là con trái, nó đứng trước
                    if (bIsLeft) return 1;  // Nếu b là con trái, a phải là con phải, a đứng sau

                    return 0; // Trường hợp khác (lỗi logic hoặc chỉ có 1 con đã được thêm)
                });
            }
            // *** KẾT THÚC SỬA LỖI SẮP XẾP ***
            return nodeInfo;
        }


        private void FirstWalk<TNode>(LayoutNodeInfo<TNode> nodeInfo, bool isBinary) where TNode : class
        { /* ... Giữ nguyên như code trước ... */
            if (!nodeInfo.Children.Any()) { if (nodeInfo.LeftSibling != null) { nodeInfo.PreliminaryX = nodeInfo.LeftSibling.PreliminaryX + BaseNodeSize + SiblingSeparation; } else { nodeInfo.PreliminaryX = 0; } return; }
            foreach (var child in nodeInfo.Children) { FirstWalk(child, isBinary); }
            double midpoint = (nodeInfo.Children.First().PreliminaryX + nodeInfo.Children.Last().PreliminaryX) / 2.0;
            if (nodeInfo.LeftSibling != null) { nodeInfo.PreliminaryX = nodeInfo.LeftSibling.PreliminaryX + BaseNodeSize + SiblingSeparation; nodeInfo.Modifier = nodeInfo.PreliminaryX - midpoint; }
            else { nodeInfo.PreliminaryX = midpoint; }
            Apportion(nodeInfo, isBinary);
        }
        private void Apportion<TNode>(LayoutNodeInfo<TNode> nodeInfo, bool isBinary) where TNode : class
        { /* ... Giữ nguyên như code trước ... */
            for (int i = 0; i < nodeInfo.Children.Count - 1; i++) { var leftChild = nodeInfo.Children[i]; var rightChild = nodeInfo.Children[i + 1]; double currentDist = rightChild.PreliminaryX - leftChild.PreliminaryX; double requiredDist = BaseNodeSize + SiblingSeparation; if (currentDist < requiredDist) { double shift = requiredDist - currentDist; for (int j = i + 1; j < nodeInfo.Children.Count; j++) { nodeInfo.Children[j].PreliminaryX += shift; nodeInfo.Children[j].Modifier += shift; } } }
        }
        private void SecondWalk<TNode>(LayoutNodeInfo<TNode> nodeInfo, double modifierSum, Dictionary<string, LayoutNodeInfo<TNode>> map) where TNode : class
        { /* ... Giữ nguyên như code trước ... */
            nodeInfo.X = nodeInfo.PreliminaryX + modifierSum; nodeInfo.Y = nodeInfo.Depth * LevelSeparation;
            foreach (var child in nodeInfo.Children) { SecondWalk(child, modifierSum + nodeInfo.Modifier, map); }
        }
        private void CalculateResultBoundsAndNormalize<TNode>(IEnumerable<LayoutNodeInfo<TNode>> nodeInfos, TreeLayoutResult<TNode> result) where TNode : class
        { /* ... Giữ nguyên như code trước, đã bao gồm tính Width/Height ... */
            if (!nodeInfos.Any()) { result.Width = 600; result.Height = 400; return; }
            double minX = double.MaxValue, maxX = double.MinValue, minY = double.MaxValue, maxY = double.MinValue; double nodeRadius = BaseNodeSize / 2.0;
            foreach (var info in nodeInfos) { minX = Math.Min(minX, info.X); maxX = Math.Max(maxX, info.X); minY = Math.Min(minY, info.Y); maxY = Math.Max(maxY, info.Y); result.NodePositions[GetNodeId(info.Node)] = (info.X, info.Y); }
            minX -= nodeRadius; maxX += nodeRadius; minY -= nodeRadius; maxY += nodeRadius;
            double padding = nodeRadius; minX -= padding; maxX += padding; minY -= padding; maxY += padding;
            double offsetX = -minX; double offsetY = -minY;
            var finalPositions = new Dictionary<string, (double X, double Y)>();
            foreach (var kvp in result.NodePositions) { finalPositions[kvp.Key] = (kvp.Value.X + offsetX, kvp.Value.Y + offsetY); }
            result.NodePositions = finalPositions;
            result.Width = Math.Max(600, maxX - minX); result.Height = Math.Max(400, maxY - minY);
            PopulateEdges(nodeInfos, result);
        }
        private void PopulateEdges<TNode>(IEnumerable<LayoutNodeInfo<TNode>> nodeInfos, TreeLayoutResult<TNode> result) where TNode : class
        { /* ... Giữ nguyên như code trước ... */
            result.Edges.Clear(); var roots = nodeInfos.Where(n => n.Parent == null); var queue = new Queue<LayoutNodeInfo<TNode>>(roots);
            while (queue.Count > 0) { var parentInfo = queue.Dequeue(); string parentId = GetNodeId(parentInfo.Node); foreach (var childInfo in parentInfo.Children) { string childId = GetNodeId(childInfo.Node); result.Edges.Add(new TreeEdge(parentId, childId)); queue.Enqueue(childInfo); } }
        }
        private string GetNodeId<TNode>(TNode node) where TNode : class
        { /* ... Giữ nguyên như code trước ... */
            if (node is TreeNode tn) return tn.Id; if (node is BstNode bn) return bn.Id;
            dynamic dynNode = node; try { return dynNode.Id; } catch { throw new InvalidOperationException($"Unsupported node type '{typeof(TNode).Name}' for layout."); }
        }
        private IEnumerable<TNode> GetNodeChildren<TNode>(TNode node, bool isBinary) where TNode : class
        { /* ... Giữ nguyên như code trước ... */
            if (node is TreeNode tn) { if (isBinary) { if (tn.Left != null) yield return (tn.Left as TNode)!; if (tn.Right != null) yield return (tn.Right as TNode)!; } else { foreach (var child in tn.Children) yield return (child as TNode)!; } }
            else if (node is BstNode bn) { if (bn.Left != null) yield return (bn.Left as TNode)!; if (bn.Right != null) yield return (bn.Right as TNode)!; }
        }

        // ========================================================================
        // === KẾT THÚC PHẦN LAYOUT ===
        // ========================================================================


        // --- General Tree Traversal (Giữ nguyên) ---
        public List<string> TraverseTree(TreeNode root, string traversalType, bool isBinary) { /* ... */ var result = new List<string>(); if (root == null) return result; switch (traversalType) { case "Pre-order": PreOrder(root, result, isBinary); break; case "In-order": if (!isBinary) PreOrder(root, result, isBinary); else InOrder(root, result); break; case "Post-order": PostOrder(root, result, isBinary); break; case "Level-order": LevelOrder(root, result, isBinary); break; } return result; }
        private void PreOrder(TreeNode node, List<string> result, bool isBinary) { result.Add(node.Data); foreach (var child in GetNodeChildren(node, isBinary)) PreOrder(child, result, isBinary); }
        private void InOrder(TreeNode? node, List<string> result) { if (node == null) return; InOrder(node.Left, result); result.Add(node.Data); InOrder(node.Right, result); }
        private void PostOrder(TreeNode node, List<string> result, bool isBinary) { foreach (var child in GetNodeChildren(node, isBinary)) PostOrder(child, result, isBinary); result.Add(node.Data); }
        private void LevelOrder(TreeNode root, List<string> result, bool isBinary) { var queue = new Queue<TreeNode>(); queue.Enqueue(root); while (queue.Count > 0) { var current = queue.Dequeue(); result.Add(current.Data); foreach (var child in GetNodeChildren(current, isBinary)) queue.Enqueue(child); } }

        // --- BST Methods (Giữ nguyên) ---
        public BstNode? CreateBstFromNumbers(string inputNumbers, out List<string> errors) { /* ... */ errors = new List<string>(); BstNode? root = null; var numbers = inputNumbers.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList(); foreach (var numStr in numbers) { if (int.TryParse(numStr, out int value)) { root = InsertBstNode(root, value); } else { errors.Add($"Giá trị không hợp lệ '{numStr}' sẽ bị bỏ qua."); } } if (root == null && numbers.Any()) errors.Add("Không tạo được cây BST từ đầu vào."); return root; }
        public BstNode? InsertBstNode(BstNode? root, int value) { if (root == null) return new BstNode(value); if (value < root.Value) root.Left = InsertBstNode(root.Left, value); else if (value > root.Value) root.Right = InsertBstNode(root.Right, value); return root; }
        public BstNode? DeleteBstNode(BstNode? root, int value) { if (root == null) return null; if (value < root.Value) root.Left = DeleteBstNode(root.Left, value); else if (value > root.Value) root.Right = DeleteBstNode(root.Right, value); else { if (root.Left == null) return root.Right; if (root.Right == null) return root.Left; root.Value = FindMinValueBst(root.Right); root.Right = DeleteBstNode(root.Right, root.Value); } return root; }
        private int FindMinValueBst(BstNode node) { int minv = node.Value; while (node.Left != null) { minv = node.Left.Value; node = node.Left; } return minv; }

        public List<string> TraverseBst(BstNode? root, string traversalType)
        {
            var result = new List<string>(); if (root == null) return result;
            // Tạm thời trả về ID (Guid) - Component sẽ map sang Value khi cần hiển thị động
            switch (traversalType)
            {
                case "Pre-order": BstPreOrderRec(root, result); break;
                case "In-order": BstInOrderRec(root, result); break;
                case "Post-order": BstPostOrderRec(root, result); break;
                case "Level-order": BstLevelOrderRec(root, result); break;
            }
            return result;
        }
        private void BstPreOrderRec(BstNode node, List<string> result) { result.Add(node.Id); if (node.Left != null) BstPreOrderRec(node.Left, result); if (node.Right != null) BstPreOrderRec(node.Right, result); }
        private void BstInOrderRec(BstNode node, List<string> result) { if (node.Left != null) BstInOrderRec(node.Left, result); result.Add(node.Id); if (node.Right != null) BstInOrderRec(node.Right, result); }
        private void BstPostOrderRec(BstNode node, List<string> result) { if (node.Left != null) BstPostOrderRec(node.Left, result); if (node.Right != null) BstPostOrderRec(node.Right, result); result.Add(node.Id); }
        private void BstLevelOrderRec(BstNode root, List<string> result) { var q = new Queue<BstNode>(); q.Enqueue(root); while (q.Count > 0) { var n = q.Dequeue(); result.Add(n.Id); if (n.Left != null) q.Enqueue(n.Left); if (n.Right != null) q.Enqueue(n.Right); } }

    }
} 