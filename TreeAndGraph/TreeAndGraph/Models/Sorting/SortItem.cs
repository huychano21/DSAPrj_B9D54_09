namespace TreeAndGraph.Models.Sorting
{
    /// <summary>
    /// Đại diện cho một phần tử trong mảng cần sắp xếp.
    /// Bao gồm giá trị và trạng thái hiển thị.
    /// </summary>
    public class SortItem : ICloneable // Triển khai ICloneable để tạo bản sao dễ dàng
    {
        /// <summary>
        /// Giá trị của phần tử.
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Trạng thái hiện tại của phần tử (dùng cho việc tô màu, làm nổi bật).
        /// </summary>
        public SortState State { get; set; } = SortState.Normal;

        /// <summary>
        /// Mã định danh duy nhất cho phần tử (hữu ích cho animation JS).
        /// </summary>
        public string Id { get; } = Guid.NewGuid().ToString("N"); // Tạo ID duy nhất

        /// <summary>
        /// Tạo một bản sao sâu của đối tượng SortItem.
        /// </summary>
        public object Clone()
        {
            return new SortItem
            {
                Value = this.Value,
                State = this.State
                // Id không cần clone vì nó là định danh cố định
            };
        }

        // Hàm tạo để dễ dàng khởi tạo
        public SortItem(int value)
        {
            Value = value;
        }
        // Hàm tạo mặc định cần thiết cho một số thao tác
        public SortItem() { }
    }
}
