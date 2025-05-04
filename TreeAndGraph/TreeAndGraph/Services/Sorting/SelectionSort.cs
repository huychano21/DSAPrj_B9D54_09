using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TreeAndGraph.Models.Sorting;
namespace TreeAndGraph.Services.Sorting
{
    /// <summary>
    /// Triển khai thuật toán Selection Sort.
    /// </summary>
    public class SelectionSort : ISortAlgorithm
    {
        /// <summary>
        /// Thực hiện Selection Sort và tạo danh sách các bước trực quan hóa.
        /// </summary>
        /// <param name="initialItems">Danh sách các phần tử ban đầu.</param>
        /// <returns>Danh sách các bước (SortStep).</returns>
        public List<SortStep> Sort(List<SortItem> initialItems)
        {
            // Tạo bản sao để không thay đổi list gốc truyền vào
            var items = initialItems.Select(item => (SortItem)item.Clone()).ToList();
            var steps = new List<SortStep>();
            int n = items.Count;

            // Bước khởi tạo
            steps.Add(new SortStep
            {
                Description = "Bắt đầu Selection Sort.",
                StateSnapshot = SortStep.CreateSnapshot(items)
            });

            for (int i = 0; i < n - 1; i++)
            {
                // Giả sử phần tử nhỏ nhất là phần tử đầu tiên của đoạn chưa sắp xếp
                int minIdx = i;

                // Bước: Bắt đầu lượt tìm kiếm min
                steps.Add(new SortStep
                {
                    Description = $"Lượt {i + 1}: Tìm phần tử nhỏ nhất trong đoạn [{i}...{n - 1}]. Giả sử min là {items[minIdx].Value} tại chỉ số {minIdx}.",
                    StateSnapshot = SortStep.CreateSnapshot(items),
                    SpecialIndex1 = minIdx, // Đánh dấu min hiện tại (màu vàng)
                    SortedIndex = i // Chỉ số bắt đầu của vùng chưa sắp xếp (đánh dấu vùng đã sort trước đó màu xanh lá)
                });


                // Duyệt qua các phần tử còn lại để tìm min thực sự
                for (int j = i + 1; j < n; j++)
                {
                    // Bước: So sánh phần tử hiện tại với min đang giữ
                    steps.Add(new SortStep
                    {
                        Description = $"So sánh min hiện tại ({items[minIdx].Value} tại {minIdx}) với phần tử tại {j} ({items[j].Value}).",
                        StateSnapshot = SortStep.CreateSnapshot(items),
                        SpecialIndex1 = minIdx, // min index (vàng)
                        Index2 = j, // current index being compared (cam)
                        SortedIndex = i // Vùng đã sorted (xanh lá)
                    });

                    if (items[j].Value < items[minIdx].Value)
                    {
                        int oldMinIdx = minIdx;
                        minIdx = j;
                        // Bước: Tìm thấy min mới
                        steps.Add(new SortStep
                        {
                            Description = $"Tìm thấy phần tử nhỏ hơn mới: {items[minIdx].Value} tại chỉ số {minIdx}.",
                            StateSnapshot = SortStep.CreateSnapshot(items),
                            SpecialIndex1 = minIdx, // new min index (vàng)
                            Index2 = oldMinIdx, // Highlight old min briefly? (cam) - Optional, current just highlights new min and keeps track of sorted area
                            SortedIndex = i
                        });
                    }
                    else
                    {
                        // Bước: Không cập nhật min
                        steps.Add(new SortStep
                        {
                            Description = $"{items[j].Value} >= {items[minIdx].Value}. Giữ nguyên min là {items[minIdx].Value} tại {minIdx}.",
                            StateSnapshot = SortStep.CreateSnapshot(items),
                            SpecialIndex1 = minIdx, // min index (vàng)
                            Index2 = j, // current index being compared (cam)
                            SortedIndex = i
                        });
                    }
                }

                // Sau vòng lặp for j, minIdx là chỉ số của phần tử nhỏ nhất trong đoạn [i...n-1]

                // Nếu phần tử nhỏ nhất không phải là phần tử đầu đoạn (items[i]) thì hoán đổi
                if (minIdx != i)
                {
                    // Bước: Chuẩn bị hoán đổi
                    steps.Add(new SortStep
                    {
                        Description = $"Hoán đổi phần tử nhỏ nhất tìm được ({items[minIdx].Value} tại {minIdx}) với phần tử đầu đoạn ({items[i].Value} tại {i}).",
                        StateSnapshot = SortStep.CreateSnapshot(items), // Trạng thái trước swap
                        Index1 = i,        // Phần tử đầu đoạn (đỏ)
                        Index2 = minIdx,   // Phần tử min tìm được (đỏ)
                        SortedIndex = i    // Vùng đã sorted (xanh lá)
                    });

                    // Thực hiện hoán đổi
                    SortItem temp = items[minIdx];
                    items[minIdx] = items[i];
                    items[i] = temp;

                    // Bước: Sau khi hoán đổi
                    steps.Add(new SortStep
                    {
                        Description = $"Đã hoán đổi. Phần tử {items[i].Value} đã về đúng vị trí {i}.",
                        StateSnapshot = SortStep.CreateSnapshot(items), // Trạng thái sau swap
                        Index1 = i,       // Vị trí mới của min (đỏ)
                        Index2 = minIdx,  // Vị trí mới của phần tử cũ ở đầu đoạn (đỏ)
                        SortedIndex = i + 1 // Vùng đã sorted mở rộng thêm 1 (xanh lá)
                    });
                }
                else
                {
                    // Bước: Không cần hoán đổi, phần tử đầu đoạn đã là nhỏ nhất
                    steps.Add(new SortStep
                    {
                        Description = $"Phần tử {items[i].Value} tại {i} đã là nhỏ nhất trong đoạn. Không cần hoán đổi.",
                        StateSnapshot = SortStep.CreateSnapshot(items),
                        Index1 = i, // Chỉ highlight phần tử đã đúng vị trí (màu gì? có thể là sorted luôn)
                        SortedIndex = i + 1 // Vùng đã sorted mở rộng thêm 1 (xanh lá)
                    });
                }
                // Cập nhật trạng thái sorted cho phần tử items[i] sau mỗi lượt
                // Điều này được xử lý bởi SortedIndex trong các step trên và UpdateStates()
            }

            // Bước cuối cùng: Đánh dấu toàn bộ mảng là đã sắp xếp
            var finalState = SortStep.CreateSnapshot(items);
            finalState.ForEach(item => item.State = SortState.Sorted);
            steps.Add(new SortStep
            {
                Description = "Hoàn tất Selection Sort. Mảng đã được sắp xếp.",
                StateSnapshot = finalState
            });

            // Cập nhật trạng thái màu sắc cho từng bước dựa trên các chỉ số
            steps.ForEach(step => step.UpdateStates());

            return steps;
        }
    }
}
