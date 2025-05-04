using TreeAndGraph.Models;
using System.Collections.Generic;
using TreeAndGraph.Models.TreeAndBST;

namespace TreeAndGraph.Services.TreeAndBSTService
{
    public interface ITreeService
    {
        // --- General Tree Methods ---
        TreeNode? ParseParentChildList(string input, bool isBinary, out List<string> errors);
        TreeNode? ParseNestedString(string input, bool isBinary, out List<string> errors);
        TreeNode GenerateRandomTree(int minNodes, int maxNodes, int maxDepth, bool isBinary);
        TreeLayoutResult<TreeNode> CalculateTreeLayout(TreeNode root, bool isBinary);
        List<string> TraverseTree(TreeNode root, string traversalType, bool isBinary);

        // --- BST Methods ---
        BstNode? CreateBstFromNumbers(string inputNumbers, out List<string> errors);
        BstNode? InsertBstNode(BstNode? root, int value);
        BstNode? DeleteBstNode(BstNode? root, int value);
        TreeLayoutResult<BstNode> CalculateBstLayout(BstNode? root);
        List<string> TraverseBst(BstNode? root, string traversalType); // BST traversals are specific
    }
}
