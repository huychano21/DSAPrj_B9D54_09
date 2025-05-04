namespace TreeAndGraph.Models.Sorting
{
    /// <summary>
    /// Định nghĩa các trạng thái của một phần tử trong quá trình sắp xếp để hiển thị.
    /// </summary>
    public enum SortState
    {
        /// <summary>
        /// Trạng thái bình thường.
        /// </summary>
        Normal,
        /// <summary>
        /// Đang được so sánh.
        /// </summary>
        Comparing,
        /// <summary>
        /// Đang được hoán đổi.
        /// </summary>
        Swapping,
        /// <summary>
        /// Đã được sắp xếp (nằm ở vị trí cuối cùng đúng).
        /// </summary>
        Sorted,
        /// <summary>
        /// Là phần tử chốt (pivot) trong QuickSort.
        /// </summary>
        Pivot,
        /// <summary>
        /// Phần tử đặc biệt 1 (ví dụ: min trong Selection Sort).
        /// </summary>
        Special1,
        /// <summary>
        /// Phần tử đặc biệt 2 (ví dụ: phần tử đang xét trong Insertion Sort).
        /// </summary>
        Special2
    }
}
