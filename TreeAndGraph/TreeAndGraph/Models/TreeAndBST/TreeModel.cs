using System.Collections.Generic;
using System.Diagnostics; // For DebuggerDisplay


namespace TreeAndGraph.Models.TreeAndBST
{

    [DebuggerDisplay("Data = {Data}")] // Giúp debug dễ hơn
    public class TreeNode
    {
        public string Id { get; } // Unique ID for mapping positions, etc.
        public string Data { get; set; }
        public List<TreeNode> Children { get; set; } = new(); // For General Tree
        public TreeNode? Left { get; set; } // For Binary Tree representation within TreeNode
        public TreeNode? Right { get; set; } // For Binary Tree representation within TreeNode

        // Calculated Position (set by TreeService layout)
        public double X { get; set; }
        public double Y { get; set; }

        // Constructor initializes with data and a unique ID
        public TreeNode(string data, string? id = null)
        {
            Data = data;
            Id = id ?? Guid.NewGuid().ToString("N"); // Ensure a unique ID
        }

        // Helper to get children regardless of tree type assumption (based on Left/Right vs Children list)
        public IEnumerable<TreeNode> GetChildren()
        {
            // Ưu tiên cấu trúc nhị phân nếu có
            if (Left != null || Right != null)
            {
                if (Left != null) yield return Left;
                if (Right != null) yield return Right;
            }
            else // Ngược lại, dùng danh sách Children
            {
                foreach (var child in Children)
                {
                    yield return child;
                }
            }
        }
    }

    [DebuggerDisplay("Value = {Value}")]
    public class BstNode
    {
        public string Id { get; } // Unique ID based on value for BST
        public int Value { get; set; } // BST uses comparable values
        public BstNode? Left { get; set; }
        public BstNode? Right { get; set; }

        // Calculated Position (set by TreeService layout)
        public double X { get; set; }
        public double Y { get; set; }
        public int Depth { get; set; } // Needed for some layout algorithms

        public BstNode(int value)
        {
            Value = value;
            Id = Guid.NewGuid().ToString("N"); // Use value as string ID for BST visualization
        }
    }

    // Represents an edge for visualization purposes
    public class TreeEdge
    {
        public string FromId { get; }
        public string ToId { get; }

        public TreeEdge(string fromId, string toId)
        {
            FromId = fromId;
            ToId = toId;
        }

        // Override Equals and GetHashCode if storing Edges in HashSet/Dictionary
        public override bool Equals(object? obj) => obj is TreeEdge edge && FromId == edge.FromId && ToId == edge.ToId;
        public override int GetHashCode() => HashCode.Combine(FromId, ToId);
    }

    // Result structure for layout calculation
    public class TreeLayoutResult<TNode> where TNode : class
    {
        public Dictionary<string, (double X, double Y)> NodePositions { get; set; } = new();
        public List<TreeEdge> Edges { get; set; } = new();
        public double MinX { get; set; } = 0;
        public double MaxX { get; set; } = 0;
        public double MinY { get; set; } = 0;
        public double MaxY { get; set; } = 0;
        public double Width { get; set; } = 600; // Kích thước SVG mặc định/tối thiểu
        public double Height { get; set; } = 400; // Kích thước SVG mặc định/tối thiểu
    }
}
