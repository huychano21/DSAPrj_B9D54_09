// TreeAndGraph/Models/HeapNode.cs
// **** LƯU Ý: Model này cần phát triển thêm đáng kể để vẽ cây SVG ****
// **** Mã dưới đây chỉ là cấu trúc cơ bản ****

namespace TreeAndGraph.Models.Sorting
{
    /// <summary>
    /// Đại diện cho một nút trong cây nhị phân (sử dụng cho Heap Sort).
    /// Cần thêm thuộc tính vị trí (X, Y) và các phương thức để vẽ SVG.
    /// </summary>
    public class HeapNode
    {
        public int Value { get; set; }
        public HeapNode? Left { get; set; }
        public HeapNode? Right { get; set; }
        public SortState State { get; set; } = SortState.Normal;

        // Thuộc tính cần thêm cho việc vẽ SVG
        public double X { get; set; }
        public double Y { get; set; }
        public int Level { get; set; } // Cấp độ của nút trong cây

        public HeapNode(int value)
        {
            Value = value;
        }
    }
}
