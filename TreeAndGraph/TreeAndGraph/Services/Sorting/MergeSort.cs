using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TreeAndGraph.Models.Sorting;

namespace TreeAndGraph.Services.Sorting
{
    /// <summary>
    /// Triển khai thuật toán Merge Sort.
    /// </summary>
    public class MergeSort : ISortAlgorithm
    {
        private List<SortStep> _steps = new List<SortStep>();
        private List<SortItem> _currentItemsState = new List<SortItem>();

        /// <summary>
        /// Thực hiện Merge Sort và tạo danh sách các bước trực quan hóa.
        /// </summary>
        /// <param name="initialItems">Danh sách các phần tử ban đầu.</param>
        /// <returns>Danh sách các bước (SortStep).</returns>
        public List<SortStep> Sort(List<SortItem> initialItems)
        {
            _steps.Clear();
            _currentItemsState = initialItems.Select(item => (SortItem)item.Clone()).ToList();

            // Bước khởi tạo
            _steps.Add(new SortStep
            {
                Description = "Bắt đầu Merge Sort.",
                StateSnapshot = SortStep.CreateSnapshot(_currentItemsState)
            });

            // Gọi hàm MergeSort đệ quy
            MergeSortRecursive(0, _currentItemsState.Count - 1);

            // Bước cuối cùng: Đánh dấu toàn bộ mảng là đã sắp xếp
            var finalState = SortStep.CreateSnapshot(_currentItemsState);
            finalState.ForEach(item => item.State = SortState.Sorted);
            _steps.Add(new SortStep
            {
                Description = "Hoàn tất Merge Sort. Mảng đã được sắp xếp.",
                StateSnapshot = finalState
            });

            // Cập nhật trạng thái màu sắc cho từng bước
            _steps.ForEach(step => step.UpdateStates());

            return _steps;
        }

        /// <summary>
        /// Hàm đệ quy của Merge Sort: Chia mảng thành các mảng con.
        /// </summary>
        /// <param name="left">Chỉ số bắt đầu của đoạn.</param>
        /// <param name="right">Chỉ số kết thúc của đoạn.</param>
        private void MergeSortRecursive(int left, int right)
        {
            if (left < right)
            {
                int middle = left + (right - left) / 2; // Tránh tràn số với mảng lớn

                // Bước: Chia đoạn
                _steps.Add(new SortStep
                {
                    Description = $"Chia đoạn [{left}...{right}] thành hai nửa: [{left}...{middle}] và [{middle + 1}...{right}].",
                    StateSnapshot = SortStep.CreateSnapshot(_currentItemsState)
                    // Có thể highlight các đoạn này bằng SpecialIndex
                });


                // Đệ quy sắp xếp hai nửa
                MergeSortRecursive(left, middle);
                MergeSortRecursive(middle + 1, right);

                // Trộn hai nửa đã sắp xếp
                Merge(left, middle, right);
            }
            else if (left == right) // Đoạn chỉ có 1 phần tử
            {
                // Bước: Đoạn 1 phần tử coi như đã sort (trong phạm vi của nó)
                _steps.Add(new SortStep
                {
                    Description = $"Đoạn [{left}] chỉ có 1 phần tử, đã được sắp xếp.",
                    StateSnapshot = SortStep.CreateSnapshot(_currentItemsState),
                    Index1 = left // Highlight phần tử này
                });
            }
        }

        /// <summary>
        /// Trộn hai mảng con đã sắp xếp: arr[left...middle] và arr[middle+1...right].
        /// </summary>
        /// <param name="left">Chỉ số bắt đầu của mảng con thứ nhất.</param>
        /// <param name="middle">Chỉ số kết thúc của mảng con thứ nhất.</param>
        /// <param name="right">Chỉ số kết thúc của mảng con thứ hai.</param>
        private void Merge(int left, int middle, int right)
        {
            // Kích thước của hai mảng con cần trộn
            int n1 = middle - left + 1;
            int n2 = right - middle;

            // Tạo các mảng tạm để chứa dữ liệu của hai mảng con
            List<SortItem> L = new List<SortItem>(n1);
            List<SortItem> R = new List<SortItem>(n2);

            // Sao chép dữ liệu vào mảng tạm L và R
            for (int i = 0; i < n1; ++i)
                L.Add((SortItem)_currentItemsState[left + i].Clone()); // Clone để không ảnh hưởng trạng thái gốc
            for (int j = 0; j < n2; ++j)
                R.Add((SortItem)_currentItemsState[middle + 1 + j].Clone());

            // Bước: Bắt đầu trộn
            _steps.Add(new SortStep
            {
                Description = $"Trộn hai đoạn đã sắp xếp: [{left}...{middle}] và [{middle + 1}...{right}].",
                StateSnapshot = SortStep.CreateSnapshot(_currentItemsState),
                // Highlight 2 đoạn bằng Special Indices
                SpecialIndex1 = left,  // Bắt đầu đoạn 1
                SpecialIndex2 = middle + 1 // Bắt đầu đoạn 2
                                           // Index1 = middle, Index2 = right // Kết thúc 2 đoạn (tùy chọn)
            });


            // Chỉ số ban đầu của hai mảng con và mảng gốc sau khi trộn
            int i_L = 0, j_R = 0;
            int k = left; // Chỉ số bắt đầu ghi vào mảng gốc _currentItemsState

            // Trộn các mảng tạm trở lại vào mảng gốc _currentItemsState[left...right]
            while (i_L < n1 && j_R < n2)
            {
                // Bước: So sánh phần tử từ L và R
                _steps.Add(new SortStep
                {
                    Description = $"So sánh phần tử từ đoạn trái ({L[i_L].Value} tại index gốc {left + i_L}) và đoạn phải ({R[j_R].Value} tại index gốc {middle + 1 + j_R}).",
                    StateSnapshot = SortStep.CreateSnapshot(_currentItemsState),
                    Index1 = left + i_L,     // Phần tử đang xét ở đoạn trái (cam)
                    Index2 = middle + 1 + j_R, // Phần tử đang xét ở đoạn phải (cam)
                    SpecialIndex1 = k        // Vị trí sẽ ghi kết quả vào mảng gốc (vàng?)
                });

                if (L[i_L].Value <= R[j_R].Value)
                {
                    // Bước: Chọn phần tử từ L
                    _steps.Add(new SortStep
                    {
                        Description = $"Chọn phần tử nhỏ hơn từ đoạn trái: {L[i_L].Value}. Ghi vào vị trí {k}.",
                        StateSnapshot = SortStep.CreateSnapshot(_currentItemsState), // Trạng thái trước khi ghi
                        Index1 = left + i_L,     // Phần tử được chọn (đỏ?)
                        Index2 = middle + 1 + j_R, // Phần tử còn lại (cam)
                        SpecialIndex1 = k        // Vị trí ghi (vàng?)
                    });

                    _currentItemsState[k] = L[i_L];
                    i_L++;
                }
                else
                {
                    // Bước: Chọn phần tử từ R
                    _steps.Add(new SortStep
                    {
                        Description = $"Chọn phần tử nhỏ hơn từ đoạn phải: {R[j_R].Value}. Ghi vào vị trí {k}.",
                        StateSnapshot = SortStep.CreateSnapshot(_currentItemsState), // Trạng thái trước khi ghi
                        Index1 = left + i_L,     // Phần tử còn lại (cam)
                        Index2 = middle + 1 + j_R, // Phần tử được chọn (đỏ?)
                        SpecialIndex1 = k        // Vị trí ghi (vàng?)
                    });

                    _currentItemsState[k] = R[j_R];
                    j_R++;
                }

                // Bước: Sau khi ghi phần tử vào mảng gốc
                var stateAfterWrite = SortStep.CreateSnapshot(_currentItemsState);
                // Có thể highlight phần tử vừa ghi tại index k
                if (k >= 0 && k < stateAfterWrite.Count)
                {
                    stateAfterWrite[k].State = SortState.Swapping; // Dùng tạm màu Swapping để thấy nó vừa được ghi
                }
                _steps.Add(new SortStep
                {
                    Description = $"Đã ghi phần tử vào vị trí {k}.",
                    StateSnapshot = stateAfterWrite, // Trạng thái sau khi ghi
                    SpecialIndex1 = k // Highlight vị trí vừa ghi
                });


                k++; // Tăng chỉ số ghi vào mảng gốc
            }

            // Sao chép các phần tử còn lại của L (nếu có)
            while (i_L < n1)
            {
                // Bước: Sao chép phần tử còn lại từ L
                _steps.Add(new SortStep
                {
                    Description = $"Sao chép phần tử còn lại từ đoạn trái: {L[i_L].Value} vào vị trí {k}.",
                    StateSnapshot = SortStep.CreateSnapshot(_currentItemsState), // Trạng thái trước ghi
                    Index1 = left + i_L, // Phần tử được chọn (đỏ?)
                    SpecialIndex1 = k   // Vị trí ghi (vàng?)
                });

                _currentItemsState[k] = L[i_L];

                // Bước: Sau khi ghi
                var stateAfterCopyL = SortStep.CreateSnapshot(_currentItemsState);
                if (k >= 0 && k < stateAfterCopyL.Count) stateAfterCopyL[k].State = SortState.Swapping;
                _steps.Add(new SortStep { Description = $"Đã ghi phần tử vào vị trí {k}.", StateSnapshot = stateAfterCopyL, SpecialIndex1 = k });


                i_L++;
                k++;
            }

            // Sao chép các phần tử còn lại của R (nếu có)
            while (j_R < n2)
            {
                // Bước: Sao chép phần tử còn lại từ R
                _steps.Add(new SortStep
                {
                    Description = $"Sao chép phần tử còn lại từ đoạn phải: {R[j_R].Value} vào vị trí {k}.",
                    StateSnapshot = SortStep.CreateSnapshot(_currentItemsState), // Trạng thái trước ghi
                    Index2 = middle + 1 + j_R, // Phần tử được chọn (đỏ?)
                    SpecialIndex1 = k   // Vị trí ghi (vàng?)
                });

                _currentItemsState[k] = R[j_R];

                // Bước: Sau khi ghi
                var stateAfterCopyR = SortStep.CreateSnapshot(_currentItemsState);
                if (k >= 0 && k < stateAfterCopyR.Count) stateAfterCopyR[k].State = SortState.Swapping;
                _steps.Add(new SortStep { Description = $"Đã ghi phần tử vào vị trí {k}.", StateSnapshot = stateAfterCopyR, SpecialIndex1 = k });

                j_R++;
                k++;
            }

            // Bước: Kết thúc trộn đoạn [left...right]
            var stateAfterMerge = SortStep.CreateSnapshot(_currentItemsState);
            // Đánh dấu đoạn vừa merge xong là sorted (tạm thời) để dễ nhìn
            for (int m = left; m <= right; m++)
            {
                if (m < stateAfterMerge.Count) stateAfterMerge[m].State = SortState.Sorted;
            }
            _steps.Add(new SortStep
            {
                Description = $"Đã trộn xong đoạn [{left}...{right}].",
                StateSnapshot = stateAfterMerge
                // Có thể highlight cả đoạn vừa trộn
            });
        }
    }
}
