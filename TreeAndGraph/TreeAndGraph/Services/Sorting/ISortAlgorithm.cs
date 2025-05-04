using System.Collections.Generic;
using System.Threading.Tasks;
using TreeAndGraph.Models.Sorting;

namespace TreeAndGraph.Services.Sorting
{
    /// <summary>
    /// Interface chung cho tất cả các thuật toán sắp xếp.
    /// </summary>
    public interface ISortAlgorithm
    {
        /// <summary>
        /// Thực hiện thuật toán sắp xếp và trả về danh sách các bước.
        /// </summary>
        /// <param name="items">Danh sách các phần tử cần sắp xếp.</param>
        /// <returns>Danh sách các bước (SortStep) mô tả quá trình sắp xếp.</returns>
        List<SortStep> Sort(List<SortItem> items);
    }
}
