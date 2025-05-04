using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TreeAndGraph.Models.Sorting;

namespace TreeAndGraph.Services.Sorting
{
    /// <summary>
    /// Triển khai thuật toán Heap Sort.
    /// Trực quan hóa trên mảng.
    /// </summary>
    public class HeapSort : ISortAlgorithm
    {
        private List<SortStep> _steps = new List<SortStep>();
        private List<SortItem> _currentItemsState = new List<SortItem>();
        private int _heapSize; // Kích thước heap hiện tại trong quá trình sort

        /// <summary>
        /// Thực hiện Heap Sort và tạo danh sách các bước trực quan hóa.
        /// </summary>
        /// <param name="initialItems">Danh sách các phần tử ban đầu.</param>
        /// <returns>Danh sách các bước (SortStep).</returns>
        public List<SortStep> Sort(List<SortItem> initialItems)
        {
            _steps.Clear();
            _currentItemsState = initialItems.Select(item => (SortItem)item.Clone()).ToList();
            int n = _currentItemsState.Count;
            _heapSize = n; // Ban đầu heap size bằng kích thước mảng

            // Bước khởi tạo
            _steps.Add(new SortStep
            {
                Description = "Bắt đầu Heap Sort.",
                StateSnapshot = SortStep.CreateSnapshot(_currentItemsState)
            });

            // 1. Xây dựng Max Heap từ mảng ban đầu
            _steps.Add(new SortStep
            {
                Description = "Giai đoạn 1: Xây dựng Max Heap.",
                StateSnapshot = SortStep.CreateSnapshot(_currentItemsState)
            });
            // Bắt đầu từ nút không phải lá cuối cùng (n/2 - 1) và vun đống xuống
            for (int i = n / 2 - 1; i >= 0; i--)
            {
                MaxHeapify(i); // Gọi MaxHeapify cho từng nút không phải lá
            }
            _steps.Add(new SortStep
            {
                Description = "Đã xây dựng xong Max Heap.",
                StateSnapshot = SortStep.CreateSnapshot(_currentItemsState)
            });


            // 2. Trích xuất lần lượt các phần tử lớn nhất từ heap
            _steps.Add(new SortStep
            {
                Description = "Giai đoạn 2: Trích xuất phần tử lớn nhất và sắp xếp.",
                StateSnapshot = SortStep.CreateSnapshot(_currentItemsState)
            });
            for (int i = n - 1; i > 0; i--)
            {
                // Bước: Chuẩn bị hoán đổi root (max) với phần tử cuối heap hiện tại
                _steps.Add(new SortStep
                {
                    Description = $"Hoán đổi phần tử lớn nhất (root: {_currentItemsState[0].Value} tại index 0) với phần tử cuối của heap hiện tại ({_currentItemsState[i].Value} tại index {i}).",
                    StateSnapshot = SortStep.CreateSnapshot(_currentItemsState),
                    Index1 = 0, // Root (đỏ)
                    Index2 = i, // Phần tử cuối heap (đỏ)
                    SortedIndex = i + 1 // Vùng đã sorted (bên phải i) - nếu có
                });

                // Hoán đổi root (phần tử lớn nhất) với phần tử cuối cùng của heap hiện tại
                SortItem temp = _currentItemsState[0];
                _currentItemsState[0] = _currentItemsState[i];
                _currentItemsState[i] = temp;

                _heapSize--; // Giảm kích thước heap đi 1 (phần tử cuối đã đúng vị trí)

                // Bước: Sau khi hoán đổi, phần tử cuối đã đúng vị trí (sorted)
                var stateAfterSwap = SortStep.CreateSnapshot(_currentItemsState);
                // Đánh dấu phần tử tại i là sorted
                if (i >= 0 && i < stateAfterSwap.Count) stateAfterSwap[i].State = SortState.Sorted;
                _steps.Add(new SortStep
                {
                    Description = $"Đã hoán đổi. Phần tử {stateAfterSwap[i].Value} tại index {i} đã ở đúng vị trí. Giảm kích thước heap còn {_heapSize}.",
                    StateSnapshot = stateAfterSwap,
                    Index1 = 0, // Root mới (có thể sai vị trí)
                    Index2 = i, // Phần tử vừa sorted (xanh lá)
                    SortedIndex = i // Chỉ số bắt đầu vùng sorted mới
                });


                // Bước: Gọi MaxHeapify cho root mới để đảm bảo tính chất Max Heap
                _steps.Add(new SortStep
                {
                    Description = $"Gọi MaxHeapify cho root mới ({_currentItemsState[0].Value} tại index 0) với heap size = {_heapSize}.",
                    StateSnapshot = SortStep.CreateSnapshot(_currentItemsState),
                    Index1 = 0, // Root cần heapify
                    SortedIndex = i // Vùng đã sorted
                });

                // Vun đống lại cho root mới (index 0) với kích thước heap đã giảm
                MaxHeapify(0);

                // Bước: Sau khi heapify root (có thể không có thay đổi nếu root đã đúng)
                _steps.Add(new SortStep
                {
                    Description = $"Kết thúc MaxHeapify cho root. Heap size = {_heapSize}.",
                    StateSnapshot = SortStep.CreateSnapshot(_currentItemsState),
                    SortedIndex = i // Vùng đã sorted
                });
            }

            // Phần tử cuối cùng (index 0) tự động đúng vị trí
            var finalState = SortStep.CreateSnapshot(_currentItemsState);
            finalState.ForEach(item => item.State = SortState.Sorted);
            _steps.Add(new SortStep
            {
                Description = "Hoàn tất Heap Sort. Mảng đã được sắp xếp.",
                StateSnapshot = finalState
            });

            // Cập nhật trạng thái màu sắc
            _steps.ForEach(step => step.UpdateStates());

            return _steps;
        }

        /// <summary>
        /// Đảm bảo cây con có gốc tại index `i` là một Max Heap.
        /// </summary>
        /// <param name="i">Index của nút gốc cần vun đống.</param>
        private void MaxHeapify(int i)
        {
            int largest = i; // Giả sử nút gốc là lớn nhất
            int left = 2 * i + 1; // Index con trái
            int right = 2 * i + 2; // Index con phải

            // Bước: Bắt đầu MaxHeapify tại node i
            _steps.Add(new SortStep
            {
                Description = $"MaxHeapify tại index {i} ({_currentItemsState[i].Value}).",
                StateSnapshot = SortStep.CreateSnapshot(_currentItemsState),
                Index1 = i, // Node đang xét (vàng?)
                            // Highlight thêm left, right nếu muốn
                Index2 = left < _heapSize ? left : -1,
                SpecialIndex1 = right < _heapSize ? right : -1,
                // Cần truyền cả SortedIndex nếu đang ở giai đoạn 2
                SortedIndex = _currentItemsState.Count - _heapSize
            });


            // So sánh nút gốc với con trái
            if (left < _heapSize) // Kiểm tra con trái có tồn tại trong heap không
            {
                // Bước: So sánh với con trái
                _steps.Add(new SortStep
                {
                    Description = $"So sánh node {i} ({_currentItemsState[i].Value}) với con trái {left} ({_currentItemsState[left].Value}).",
                    StateSnapshot = SortStep.CreateSnapshot(_currentItemsState),
                    Index1 = i,      // Node cha (cam)
                    Index2 = left,  // Con trái (cam)
                    SpecialIndex1 = right < _heapSize ? right : -1, // Con phải (nếu có)
                    SortedIndex = _currentItemsState.Count - _heapSize
                });
                if (_currentItemsState[left].Value > _currentItemsState[largest].Value)
                {
                    largest = left;
                    // Bước: Con trái lớn hơn
                    _steps.Add(new SortStep { Description = $"Con trái lớn hơn. largest = {largest}.", StateSnapshot = SortStep.CreateSnapshot(_currentItemsState), Index1 = largest, SortedIndex = _currentItemsState.Count - _heapSize });
                }
                else
                {
                    // Bước: Node hiện tại lớn hơn/bằng con trái
                    _steps.Add(new SortStep { Description = $"Node {i} lớn hơn hoặc bằng con trái. largest = {largest}.", StateSnapshot = SortStep.CreateSnapshot(_currentItemsState), Index1 = i, Index2 = left, SortedIndex = _currentItemsState.Count - _heapSize });
                }
            }


            // So sánh nút lớn nhất hiện tại (largest) với con phải
            if (right < _heapSize) // Kiểm tra con phải có tồn tại trong heap không
            {
                // Bước: So sánh với con phải
                _steps.Add(new SortStep
                {
                    Description = $"So sánh node lớn nhất hiện tại (largest={largest}, value={_currentItemsState[largest].Value}) với con phải {right} ({_currentItemsState[right].Value}).",
                    StateSnapshot = SortStep.CreateSnapshot(_currentItemsState),
                    Index1 = largest, // Node lớn nhất tạm thời (cam)
                    Index2 = right, // Con phải (cam)
                    SpecialIndex1 = left < _heapSize ? left : -1, // Con trái (nếu có)
                    SortedIndex = _currentItemsState.Count - _heapSize
                });

                if (_currentItemsState[right].Value > _currentItemsState[largest].Value)
                {
                    largest = right;
                    // Bước: Con phải lớn hơn
                    _steps.Add(new SortStep { Description = $"Con phải lớn hơn. largest = {largest}.", StateSnapshot = SortStep.CreateSnapshot(_currentItemsState), Index1 = largest, SortedIndex = _currentItemsState.Count - _heapSize });
                }
                else
                {
                    // Bước: Node largest lớn hơn/bằng con phải
                    _steps.Add(new SortStep { Description = $"Node largest ({largest}) lớn hơn hoặc bằng con phải. largest = {largest}.", StateSnapshot = SortStep.CreateSnapshot(_currentItemsState), Index1 = largest, Index2 = right, SortedIndex = _currentItemsState.Count - _heapSize });
                }
            }


            // Nếu nút lớn nhất không phải là nút gốc ban đầu (i)
            if (largest != i)
            {
                // Bước: Chuẩn bị hoán đổi node i với node largest
                _steps.Add(new SortStep
                {
                    Description = $"Node lớn nhất không phải gốc ({largest} != {i}). Chuẩn bị hoán đổi index {i} ({_currentItemsState[i].Value}) và index {largest} ({_currentItemsState[largest].Value}).",
                    StateSnapshot = SortStep.CreateSnapshot(_currentItemsState),
                    Index1 = i,       // Node gốc (đỏ)
                    Index2 = largest, // Node lớn nhất tìm được (đỏ)
                    SortedIndex = _currentItemsState.Count - _heapSize
                });

                // Hoán đổi
                SortItem swap = _currentItemsState[i];
                _currentItemsState[i] = _currentItemsState[largest];
                _currentItemsState[largest] = swap;

                // Bước: Sau khi hoán đổi
                _steps.Add(new SortStep
                {
                    Description = $"Đã hoán đổi index {i} và {largest}.",
                    StateSnapshot = SortStep.CreateSnapshot(_currentItemsState),
                    Index1 = i,       // Vị trí mới (đỏ)
                    Index2 = largest, // Vị trí mới (đỏ)
                    SortedIndex = _currentItemsState.Count - _heapSize
                });


                // Bước: Gọi đệ quy MaxHeapify cho nút bị ảnh hưởng (nút largest cũ, giờ chứa giá trị nhỏ hơn)
                _steps.Add(new SortStep
                {
                    Description = $"Đệ quy gọi MaxHeapify cho nút bị ảnh hưởng tại index {largest}.",
                    StateSnapshot = SortStep.CreateSnapshot(_currentItemsState),
                    Index1 = largest, // Node cần heapify tiếp (vàng?)
                    SortedIndex = _currentItemsState.Count - _heapSize
                });

                // Đệ quy vun đống cho cây con bị ảnh hưởng
                MaxHeapify(largest);
            }
            else
            {
                // Bước: Node gốc đã là lớn nhất, không cần hoán đổi
                _steps.Add(new SortStep
                {
                    Description = $"Node {i} ({_currentItemsState[i].Value}) đã là lớn nhất so với các con. Kết thúc MaxHeapify cho node này.",
                    StateSnapshot = SortStep.CreateSnapshot(_currentItemsState),
                    Index1 = i, // Node gốc (xanh lá?)
                    SortedIndex = _currentItemsState.Count - _heapSize
                });
            }
        }
    }
}
