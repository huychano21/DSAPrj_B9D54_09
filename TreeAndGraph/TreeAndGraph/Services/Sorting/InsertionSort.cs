using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TreeAndGraph.Models.Sorting;

namespace TreeAndGraph.Services.Sorting
{
    /// <summary>
    /// Triển khai thuật toán Insertion Sort.
    /// </summary>
    public class InsertionSort : ISortAlgorithm
    {
        /// <summary>
        /// Thực hiện Insertion Sort và tạo danh sách các bước trực quan hóa.
        /// </summary>
        /// <param name="initialItems">Danh sách các phần tử ban đầu.</param>
        /// <returns>Danh sách các bước (SortStep).</returns>
        public List<SortStep> Sort(List<SortItem> initialItems)
        {
            var items = initialItems.Select(item => (SortItem)item.Clone()).ToList();
            var steps = new List<SortStep>();
            int n = items.Count;

            // Bước khởi tạo
            steps.Add(new SortStep
            {
                Description = "Bắt đầu Insertion Sort. Phần tử đầu tiên được coi là đã sắp xếp.",
                StateSnapshot = SortStep.CreateSnapshot(items)
                // Có thể đánh dấu phần tử đầu tiên là sorted nếu muốn
                // SortedIndex = 1 // Chỉ số BẮT ĐẦU của vùng chưa sắp xếp
            });
            // Đánh dấu phần tử đầu tiên là sorted trong state ban đầu (optional)
            if (n > 0)
            {
                var initialState = SortStep.CreateSnapshot(items);
                initialState[0].State = SortState.Sorted;
                steps.Add(new SortStep
                {
                    Description = "Phần tử đầu tiên (index 0) được coi là vùng đã sắp xếp ban đầu.",
                    StateSnapshot = initialState,
                    SortedIndex = 0 // Chỉ số CUỐI CÙNG của vùng đã sort
                });
            }


            // Bắt đầu từ phần tử thứ hai (index 1)
            for (int i = 1; i < n; i++)
            {
                SortItem key = (SortItem)items[i].Clone(); // Lấy phần tử cần chèn (key) và tạo bản sao
                int j = i - 1;

                // Bước: Chọn key và chuẩn bị dịch chuyển
                steps.Add(new SortStep
                {
                    Description = $"Lượt {i}: Chọn phần tử tại index {i} ({key.Value}) làm key để chèn vào đoạn đã sắp xếp [0...{j}].",
                    StateSnapshot = SortStep.CreateSnapshot(items),
                    SpecialIndex2 = i, // Đánh dấu key (màu xanh dương)
                    SortedIndex = i - 1 // Vùng đã sort là từ 0 đến i-1 (màu xanh lá)
                });

                // Di chuyển các phần tử của đoạn đã sắp xếp [0...i-1] mà lớn hơn key
                // sang phải một vị trí để tạo chỗ trống cho key.
                while (j >= 0 && items[j].Value > key.Value)
                {
                    // Bước: So sánh key với phần tử trong đoạn đã sắp xếp
                    steps.Add(new SortStep
                    {
                        Description = $"So sánh key ({key.Value}) với phần tử tại index {j} ({items[j].Value}). Vì {items[j].Value} > {key.Value}, dịch chuyển {items[j].Value} sang phải.",
                        StateSnapshot = SortStep.CreateSnapshot(items),
                        Index1 = j,       // Phần tử đang so sánh trong vùng sorted (cam)
                        SpecialIndex2 = i, // Key vẫn được giữ (xanh dương) - hoặc vị trí trống nó sẽ vào
                        SortedIndex = i - 1 // Vùng đã sort gốc
                    });

                    // Thực hiện dịch chuyển
                    items[j + 1] = items[j];

                    // Bước: Sau khi dịch chuyển
                    steps.Add(new SortStep
                    {
                        Description = $"Đã dịch chuyển phần tử {items[j + 1].Value} từ index {j} sang {j + 1}.",
                        StateSnapshot = SortStep.CreateSnapshot(items), // Trạng thái sau khi dịch
                        Index1 = j + 1,   // Vị trí mới của phần tử vừa dịch (đỏ?)
                        SpecialIndex2 = i, // Key (xanh dương)
                        SortedIndex = i - 1
                    });

                    j = j - 1; // Xét phần tử tiếp theo bên trái
                }

                // Tại đây, j+1 là vị trí đúng để chèn key vào

                // Bước: Chèn key vào vị trí tìm được
                // Kiểm tra xem có thực sự chèn vào vị trí khác không (nếu j+1 != i)
                if (j + 1 != i)
                {
                    steps.Add(new SortStep
                    {
                        Description = $"Chèn key ({key.Value}) vào vị trí đúng tại index {j + 1}.",
                        StateSnapshot = SortStep.CreateSnapshot(items), // Trạng thái ngay trước khi chèn key
                        Index1 = j + 1,      // Vị trí sẽ chèn key (đỏ?)
                        SpecialIndex2 = i,   // Vị trí gốc của key (xanh dương) - có thể bỏ
                        SortedIndex = i - 1   // Vùng sort cũ
                    });
                }
                else
                {
                    steps.Add(new SortStep
                    {
                        Description = $"Key ({key.Value}) đã ở đúng vị trí tại index {i}. Không cần dịch chuyển hay chèn.",
                        StateSnapshot = SortStep.CreateSnapshot(items),
                        Index1 = i, // Highlight vị trí đúng (xanh lá?)
                        SortedIndex = i // Vùng sort mở rộng
                    });
                }


                // Thực hiện chèn key
                items[j + 1] = key;

                // Bước: Sau khi chèn key, đoạn [0...i] đã được sắp xếp
                var stateAfterInsert = SortStep.CreateSnapshot(items);
                // Đánh dấu vùng [0...i] là sorted cho bước này
                for (int k = 0; k <= i; k++)
                {
                    if (k < stateAfterInsert.Count) stateAfterInsert[k].State = SortState.Sorted;
                }
                steps.Add(new SortStep
                {
                    Description = $"Đã chèn key {key.Value}. Đoạn [0...{i}] đã được sắp xếp.",
                    StateSnapshot = stateAfterInsert,
                    //Index1 = j + 1, // Highlight vị trí key vừa chèn (xanh lá)
                    SortedIndex = i // Chỉ số cuối cùng của vùng đã sort mới
                });
            }

            // Bước cuối cùng: Toàn bộ mảng đã sắp xếp
            var finalState = SortStep.CreateSnapshot(items);
            finalState.ForEach(item => item.State = SortState.Sorted);
            steps.Add(new SortStep
            {
                Description = "Hoàn tất Insertion Sort. Mảng đã được sắp xếp.",
                StateSnapshot = finalState
            });

            // Cập nhật trạng thái màu sắc cho từng bước
            steps.ForEach(step => step.UpdateStates());

            return steps;
        }
    }
}
