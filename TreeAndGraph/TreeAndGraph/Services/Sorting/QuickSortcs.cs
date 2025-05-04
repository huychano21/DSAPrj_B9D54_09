using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TreeAndGraph.Models;
using TreeAndGraph.Models.Sorting;

namespace TreeAndGraph.Services.Sorting
{
    /// <summary>
    /// Triển khai thuật toán Quick Sort (sử dụng Lomuto partition scheme).
    /// </summary>
    public class QuickSort : ISortAlgorithm
    {
        private List<SortStep> _steps = new List<SortStep>(); // Danh sách các bước dùng chung cho các lần gọi đệ quy
        private List<SortItem> _currentItemsState = new List<SortItem>(); // Trạng thái hiện tại của mảng

        /// <summary>
        /// Thực hiện Quick Sort và tạo danh sách các bước trực quan hóa.
        /// </summary>
        /// <param name="initialItems">Danh sách các phần tử ban đầu.</param>
        /// <returns>Danh sách các bước (SortStep).</returns>
        public List<SortStep> Sort(List<SortItem> initialItems)
        {
            _steps.Clear(); // Xóa các bước cũ nếu có
            _currentItemsState = initialItems.Select(item => (SortItem)item.Clone()).ToList(); // Tạo bản sao ban đầu

            // Bước khởi tạo
            _steps.Add(new SortStep
            {
                Description = "Bắt đầu Quick Sort.",
                StateSnapshot = SortStep.CreateSnapshot(_currentItemsState)
            });

            // Gọi hàm QuickSort đệ quy
            QuickSortRecursive(0, _currentItemsState.Count - 1);

            // Bước cuối cùng: Đánh dấu toàn bộ mảng là đã sắp xếp
            var finalState = SortStep.CreateSnapshot(_currentItemsState);
            finalState.ForEach(item => item.State = SortState.Sorted);
            _steps.Add(new SortStep
            {
                Description = "Hoàn tất Quick Sort. Mảng đã được sắp xếp.",
                StateSnapshot = finalState
            });

            // Cập nhật trạng thái màu sắc cho từng bước
            // Lưu ý: Trạng thái Sorted được cập nhật cả trong Partition và ở cuối cùng
            _steps.ForEach(step => step.UpdateStates());


            return _steps;
        }

        /// <summary>
        /// Hàm đệ quy của Quick Sort.
        /// </summary>
        /// <param name="low">Chỉ số bắt đầu của đoạn cần sắp xếp.</param>
        /// <param name="high">Chỉ số kết thúc của đoạn cần sắp xếp.</param>
        private void QuickSortRecursive(int low, int high)
        {
            if (low < high)
            {
                // Bước: Gọi đệ quy cho đoạn [low...high]
                _steps.Add(new SortStep
                {
                    Description = $"Gọi QuickSort cho đoạn [{low}...{high}].",
                    StateSnapshot = SortStep.CreateSnapshot(_currentItemsState)
                    // Có thể thêm Index1=low, Index2=high để highlight đoạn đang xử lý
                });

                // Phân hoạch mảng và lấy chỉ số của pivot sau khi phân hoạch
                int pivotIndex = Partition(low, high);

                // Bước: Đánh dấu pivot đã về đúng vị trí (Sorted)
                var stateAfterPartition = SortStep.CreateSnapshot(_currentItemsState);
                if (pivotIndex >= 0 && pivotIndex < stateAfterPartition.Count)
                {
                    stateAfterPartition[pivotIndex].State = SortState.Sorted; // Đánh dấu pivot là sorted
                }
                _steps.Add(new SortStep
                {
                    Description = $"Kết thúc phân hoạch đoạn [{low}...{high}]. Pivot {stateAfterPartition[pivotIndex].Value} tại index {pivotIndex} đã ở đúng vị trí.",
                    StateSnapshot = stateAfterPartition,
                    PivotIndex = pivotIndex, // Highlight pivot lần cuối là Sorted
                                             // Giữ lại các trạng thái sorted khác nếu có
                                             // Cần đảm bảo UpdateStates không ghi đè trạng thái Sorted này
                });


                // Đệ quy sắp xếp hai mảng con trước và sau pivot
                QuickSortRecursive(low, pivotIndex - 1);
                QuickSortRecursive(pivotIndex + 1, high);
            }
            else if (low == high) // Trường hợp đoạn chỉ có 1 phần tử
            {
                // Bước: Đoạn có 1 phần tử, coi như đã sắp xếp
                var singleElementState = SortStep.CreateSnapshot(_currentItemsState);
                if (low >= 0 && low < singleElementState.Count)
                {
                    singleElementState[low].State = SortState.Sorted; // Đánh dấu là sorted
                }
                _steps.Add(new SortStep
                {
                    Description = $"Đoạn [{low}] chỉ có 1 phần tử, đã sắp xếp.",
                    StateSnapshot = singleElementState,
                    SortedIndex = low // Chỉ số cuối cùng của vùng sort là low
                });
            }
        }

        /// <summary>
        /// Thực hiện phân hoạch Lomuto. Chọn phần tử cuối cùng làm pivot.
        /// Đặt pivot vào đúng vị trí, các phần tử nhỏ hơn ở bên trái, lớn hơn ở bên phải.
        /// </summary>
        /// <param name="low">Chỉ số bắt đầu.</param>
        /// <param name="high">Chỉ số kết thúc (vị trí pivot).</param>
        /// <returns>Chỉ số cuối cùng của pivot sau khi phân hoạch.</returns>
        private int Partition(int low, int high)
        {
            SortItem pivot = _currentItemsState[high]; // Chọn phần tử cuối làm pivot
            int i = low - 1; // Chỉ số của phần tử nhỏ hơn cuối cùng được tìm thấy

            // Bước: Chọn pivot
            _steps.Add(new SortStep
            {
                Description = $"Phân hoạch đoạn [{low}...{high}]. Chọn pivot là phần tử cuối: {pivot.Value} tại index {high}.",
                StateSnapshot = SortStep.CreateSnapshot(_currentItemsState),
                PivotIndex = high // Đánh dấu pivot (màu tím)
            });

            for (int j = low; j < high; j++)
            {
                // Bước: So sánh phần tử j với pivot
                _steps.Add(new SortStep
                {
                    Description = $"So sánh phần tử tại index {j} ({_currentItemsState[j].Value}) với pivot ({pivot.Value}).",
                    StateSnapshot = SortStep.CreateSnapshot(_currentItemsState),
                    Index1 = j, // Phần tử đang xét (cam)
                    PivotIndex = high, // Pivot (tím)
                    SpecialIndex1 = i // Vị trí tiềm năng để swap (vàng?) - nếu cần
                });

                // Nếu phần tử hiện tại nhỏ hơn hoặc bằng pivot
                if (_currentItemsState[j].Value <= pivot.Value)
                {
                    i++; // Tăng chỉ số của vùng chứa các phần tử nhỏ hơn pivot

                    // Bước: Chuẩn bị hoán đổi items[i] và items[j]
                    _steps.Add(new SortStep
                    {
                        Description = $"{_currentItemsState[j].Value} <= {pivot.Value}. Chuẩn bị hoán đổi phần tử tại index {i} ({_currentItemsState[i].Value}) và index {j} ({_currentItemsState[j].Value}).",
                        StateSnapshot = SortStep.CreateSnapshot(_currentItemsState),
                        Index1 = i, // Vị trí sẽ swap (đỏ)
                        Index2 = j, // Vị trí sẽ swap (đỏ)
                        PivotIndex = high
                    });

                    // Hoán đổi items[i] và items[j]
                    SortItem temp = _currentItemsState[i];
                    _currentItemsState[i] = _currentItemsState[j];
                    _currentItemsState[j] = temp;

                    // Bước: Sau khi hoán đổi
                    _steps.Add(new SortStep
                    {
                        Description = $"Đã hoán đổi phần tử tại index {i} và {j}.",
                        StateSnapshot = SortStep.CreateSnapshot(_currentItemsState), // Trạng thái sau swap
                        Index1 = i, // Vị trí sau swap (đỏ)
                        Index2 = j, // Vị trí sau swap (đỏ)
                        PivotIndex = high
                    });
                }
                else
                {
                    // Bước: Không cần hoán đổi
                    _steps.Add(new SortStep
                    {
                        Description = $"{_currentItemsState[j].Value} > {pivot.Value}. Không hoán đổi.",
                        StateSnapshot = SortStep.CreateSnapshot(_currentItemsState),
                        Index1 = j,       // Phần tử đang xét (cam)
                        PivotIndex = high, // Pivot (tím)
                        SpecialIndex1 = i // Vị trí cuối của vùng nhỏ hơn (vàng?)
                    });
                }
            }

            // Sau vòng lặp, i+1 là vị trí đúng của pivot
            // Hoán đổi pivot (items[high]) với phần tử tại i+1
            int finalPivotIndex = i + 1;

            // Bước: Chuẩn bị hoán đổi pivot vào đúng vị trí
            _steps.Add(new SortStep
            {
                Description = $"Kết thúc duyệt. Chuẩn bị hoán đổi pivot ({pivot.Value} tại {high}) vào đúng vị trí tại index {finalPivotIndex} ({_currentItemsState[finalPivotIndex].Value}).",
                StateSnapshot = SortStep.CreateSnapshot(_currentItemsState),
                Index1 = finalPivotIndex, // Vị trí đúng của pivot (đỏ)
                Index2 = high,            // Vị trí hiện tại của pivot (đỏ)
                PivotIndex = high         // Đánh dấu pivot đang chuẩn bị di chuyển
            });

            SortItem tempPivot = _currentItemsState[finalPivotIndex];
            _currentItemsState[finalPivotIndex] = _currentItemsState[high]; // Đưa pivot vào vị trí finalPivotIndex
            _currentItemsState[high] = tempPivot; // Đưa phần tử tại finalPivotIndex ra vị trí high cũ

            // Bước: Sau khi hoán đổi pivot (pivot đã về đúng vị trí)
            // Trạng thái sorted của pivot sẽ được đánh dấu ở bước tiếp theo trong hàm QuickSortRecursive


            return finalPivotIndex; // Trả về chỉ số của pivot sau khi phân hoạch
        }
    }
}
