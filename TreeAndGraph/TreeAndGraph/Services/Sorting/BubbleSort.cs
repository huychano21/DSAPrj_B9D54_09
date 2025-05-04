using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TreeAndGraph.Models.Sorting;
using System.Linq;

namespace TreeAndGraph.Services.Sorting
{
    /// <summary>
    /// Triển khai thuật toán Bubble Sort.
    /// </summary>
    public class BubbleSort : ISortAlgorithm
    {
        /// <summary>
        /// Thực hiện Bubble Sort và tạo danh sách các bước trực quan hóa.
        /// </summary>
        /// <param name="initialItems">Danh sách các phần tử ban đầu.</param>
        /// <returns>Danh sách các bước (SortStep).</returns>
        public List<SortStep> Sort(List<SortItem> initialItems)
        {
            // Tạo bản sao để không thay đổi list gốc truyền vào
            var items = initialItems.Select(item => (SortItem)item.Clone()).ToList();
            var steps = new List<SortStep>();
            int n = items.Count;
            bool swapped;

            // Thêm bước khởi tạo
            steps.Add(new SortStep
            {
                Description = "Bắt đầu Bubble Sort.",
                StateSnapshot = SortStep.CreateSnapshot(items)
            });

            for (int i = 0; i < n - 1; i++)
            {
                swapped = false;
                steps.Add(new SortStep
                {
                    Description = $"Bắt đầu lượt {i + 1}. Duyệt từ đầu đến phần tử thứ {n - i - 1}.",
                    StateSnapshot = SortStep.CreateSnapshot(items),
                    SortedIndex = n - i // Đánh dấu các phần tử đã đúng vị trí cuối
                });

                for (int j = 0; j < n - i - 1; j++)
                {
                    // Bước: Đánh dấu đang so sánh
                    steps.Add(new SortStep
                    {
                        Description = $"So sánh phần tử tại chỉ số {j} ({items[j].Value}) và {j + 1} ({items[j + 1].Value}).",
                        StateSnapshot = SortStep.CreateSnapshot(items),
                        Index1 = j,
                        Index2 = j + 1,
                        SortedIndex = n - i // Giữ đánh dấu sorted
                    });

                    if (items[j].Value > items[j + 1].Value)
                    {
                        // Bước: Chuẩn bị hoán đổi
                        steps.Add(new SortStep
                        {
                            Description = $"Phần tử {items[j].Value} > {items[j + 1].Value}. Chuẩn bị hoán đổi.",
                            StateSnapshot = SortStep.CreateSnapshot(items),
                            Index1 = j,
                            Index2 = j + 1,
                            SortedIndex = n - i
                        });

                        // Thực hiện hoán đổi
                        SortItem temp = items[j];
                        items[j] = items[j + 1];
                        items[j + 1] = temp;
                        swapped = true;

                        // Bước: Sau khi hoán đổi
                        steps.Add(new SortStep
                        {
                            Description = $"Đã hoán đổi phần tử tại chỉ số {j} và {j + 1}.",
                            StateSnapshot = SortStep.CreateSnapshot(items), // Ghi lại trạng thái sau hoán đổi
                            Index1 = j, // Vẫn highlight để thấy sự thay đổi
                            Index2 = j + 1,
                            SortedIndex = n - i
                        });
                    }
                    else
                    {
                        // Bước: Không cần hoán đổi
                        steps.Add(new SortStep
                        {
                            Description = $"Phần tử {items[j].Value} <= {items[j + 1].Value}. Không hoán đổi.",
                            StateSnapshot = SortStep.CreateSnapshot(items),
                            Index1 = j,
                            Index2 = j + 1,
                            SortedIndex = n - i
                        });
                    }
                }

                // Đánh dấu phần tử cuối cùng của lượt này là đã sắp xếp
                if (n - 1 - i >= 0 && n - 1 - i < items.Count)
                {
                    // Tạo snapshot mới để cập nhật trạng thái sorted
                    var finalPassState = SortStep.CreateSnapshot(items);
                    // Đánh dấu tất cả các phần tử từ vị trí này về cuối là sorted
                    for (int k = n - 1 - i; k < n; k++)
                    {
                        if (k >= 0 && k < finalPassState.Count)
                            finalPassState[k].State = SortState.Sorted;
                    }
                    steps.Add(new SortStep
                    {
                        Description = $"Kết thúc lượt {i + 1}. Phần tử {items[n - 1 - i].Value} đã ở đúng vị trí.",
                        StateSnapshot = finalPassState,
                        SortedIndex = n - 1 - i // Chỉ số đầu tiên của vùng đã sort
                    });
                }


                // Nếu không có hoán đổi nào trong lượt này, mảng đã được sắp xếp
                if (!swapped)
                {
                    steps.Add(new SortStep
                    {
                        Description = "Không có hoán đổi nào trong lượt vừa qua. Mảng đã được sắp xếp.",
                        StateSnapshot = SortStep.CreateSnapshot(items) // Trạng thái cuối cùng
                    });
                    break;
                }
            }

            // Đảm bảo tất cả các phần tử được đánh dấu là Sorted ở bước cuối cùng
            var finalState = SortStep.CreateSnapshot(items);
            finalState.ForEach(item => item.State = SortState.Sorted);
            steps.Add(new SortStep
            {
                Description = "Hoàn tất Bubble Sort.",
                StateSnapshot = finalState
            });


            // Cập nhật trạng thái cho từng bước (quan trọng cho việc highlight)
            steps.ForEach(step => step.UpdateStates());

            return steps;
        }
    }
}
