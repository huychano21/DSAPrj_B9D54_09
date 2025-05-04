using Microsoft.AspNetCore.Mvc;

namespace TreeAndGraph.Components.Pages
{
    public class BSTNode : Controller
    {
        public int Value { get; set; }
        public BSTNode? Left { get; set; }
        public BSTNode? Right { get; set; }

        public BSTNode(int value)
        {
            Value = value;
            Left = null;
            Right = null;
        }
    }
}
