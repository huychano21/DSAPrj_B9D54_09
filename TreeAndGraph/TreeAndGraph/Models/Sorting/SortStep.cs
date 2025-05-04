namespace TreeAndGraph.Models.Sorting
{
    /// <summary>
    /// Đại diện cho một bước trong quá trình thực thi thuật toán sắp xếp.
    /// </summary>
    public class SortStep
    {
        /// <summary>
        /// Mô tả bằng tiếng Việt về hành động đang diễn ra ở bước này.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Bản ghi (snapshot) trạng thái của mảng sau khi thực hiện hành động của bước này.
        /// Sử dụng List<SortItem> thay vì SortItem[] để linh hoạt hơn.
        /// </summary>
        public List<SortItem> StateSnapshot { get; set; } = new List<SortItem>();

        /// <summary>
        /// Chỉ số của phần tử thứ nhất liên quan đến hành động (ví dụ: so sánh, hoán đổi).
        /// Dùng -1 nếu không áp dụng.
        /// </summary>
        public int Index1 { get; set; } = -1;

        /// <summary>
        /// Chỉ số của phần tử thứ hai liên quan đến hành động.
        /// Dùng -1 nếu không áp dụng.
        /// </summary>
        public int Index2 { get; set; } = -1;

        /// <summary>
        /// Chỉ số của phần tử pivot (nếu có, dùng trong QuickSort).
        /// Dùng -1 nếu không áp dụng.
        /// </summary>
        public int PivotIndex { get; set; } = -1;

        /// <summary>
        /// Chỉ số của phần tử đã được sắp xếp cuối cùng (nếu có).
        /// Dùng -1 nếu không áp dụng.
        /// </summary>
        public int SortedIndex { get; set; } = -1;

        /// <summary>
        /// Chỉ số đặc biệt 1 (ví dụ: min trong Selection Sort).
        /// Dùng -1 nếu không áp dụng.
        /// </summary>
        public int SpecialIndex1 { get; set; } = -1;

        /// <summary>
        /// Chỉ số đặc biệt 2 (ví dụ: phần tử đang xét trong Insertion Sort).
        /// Dùng -1 nếu không áp dụng.
        /// </summary>
        public int SpecialIndex2 { get; set; } = -1;

        /// <summary>
        /// Hàm tạo một bản sao trạng thái hiện tại của mảng.
        /// </summary>
        /// <param name="items">Danh sách SortItem gốc.</param>
        /// <returns>Một danh sách mới chứa bản sao của các SortItem.</returns>
        public static List<SortItem> CreateSnapshot(List<SortItem> items)
        {
            // Sử dụng Clone() đã định nghĩa trong SortItem
            return items.Select(item => (SortItem)item.Clone()).ToList();
        }

        /// <summary>
        /// Cập nhật trạng thái các phần tử dựa trên các chỉ số trong bước này.
        /// </summary>
        public void UpdateStates()
        {
            // Reset trạng thái về Normal trước khi áp dụng trạng thái mới,
            // NHƯNG giữ lại trạng thái Sorted đã có từ các bước trước.
            foreach (var item in StateSnapshot)
            {
                if (item.State != SortState.Sorted)
                {
                    item.State = SortState.Normal;
                }
            }

            // Áp dụng các trạng thái mới, nhưng không ghi đè lên Sorted trừ khi chỉ số đó là SortedIndex
            Action<int, SortState> setStateIfNotSorted = (index, newState) => {
                if (index >= 0 && index < StateSnapshot.Count && StateSnapshot[index].State != SortState.Sorted)
                {
                    StateSnapshot[index].State = newState;
                }
            };

            // Đánh dấu pivot (ưu tiên cao hơn các trạng thái khác, trừ Sorted)
            if (PivotIndex >= 0 && PivotIndex < StateSnapshot.Count && StateSnapshot[PivotIndex].State != SortState.Sorted)
            {
                StateSnapshot[PivotIndex].State = SortState.Pivot;
            }

            // Đánh dấu các trạng thái đặc biệt
            setStateIfNotSorted(SpecialIndex1, SortState.Special1);
            setStateIfNotSorted(SpecialIndex2, SortState.Special2);

            // Đánh dấu Comparing / Swapping
            bool isSwapping = Description.Contains("Hoán đổi") || Description.Contains("swap") || Description.Contains("Ghi vào vị trí"); // Mở rộng check swap
            if (isSwapping)
            {
                setStateIfNotSorted(Index1, SortState.Swapping);
                setStateIfNotSorted(Index2, SortState.Swapping);
            }
            else // Nếu không phải swap thì mới là comparing (giả định)
            {
                setStateIfNotSorted(Index1, SortState.Comparing);
                setStateIfNotSorted(Index2, SortState.Comparing);
            }


            // Đánh dấu các phần tử đã sắp xếp CUỐI CÙNG
            // Logic này áp dụng cho các thuật toán như BubbleSort, SelectionSort, HeapSort
            // nơi vùng sorted tăng dần từ cuối hoặc đầu mảng.
            if (SortedIndex >= -1) // SortedIndex có thể là -1 (chưa có gì sort) hoặc chỉ số bắt đầu/kết thúc vùng sort
            {
                // Ví dụ: HeapSort, SelectionSort - vùng sorted từ SortedIndex đến cuối
                if (Description.Contains("Heap Sort") || Description.Contains("Selection Sort"))
                {
                    for (int i = SortedIndex; i < StateSnapshot.Count; i++)
                    {
                        if (i >= 0) StateSnapshot[i].State = SortState.Sorted;
                    }
                }
                // Ví dụ: InsertionSort - vùng sorted từ đầu đến SortedIndex
                else if (Description.Contains("Insertion Sort"))
                {
                    for (int i = 0; i <= SortedIndex && i < StateSnapshot.Count; i++)
                    {
                        if (i >= 0) StateSnapshot[i].State = SortState.Sorted;
                    }
                }
                // Ví dụ: BubbleSort - vùng sorted từ SortedIndex đến cuối
                else if (Description.Contains("Bubble Sort"))
                {
                    for (int i = SortedIndex; i < StateSnapshot.Count; i++)
                    {
                        if (i >= 0) StateSnapshot[i].State = SortState.Sorted;
                    }
                }
                // QuickSort đánh dấu pivot là Sorted khi nó về đúng vị trí (đã xử lý riêng)
                // MergeSort đánh dấu sorted sau khi merge xong 1 đoạn (có thể làm trong thuật toán hoặc ở đây)

            }

            // QuickSort: đảm bảo pivot được đánh dấu Sorted sau khi phân hoạch xong
            if (Description.Contains("đúng vị trí") && PivotIndex >= 0 && PivotIndex < StateSnapshot.Count)
            {
                StateSnapshot[PivotIndex].State = SortState.Sorted;
            }


        }
    }
}
