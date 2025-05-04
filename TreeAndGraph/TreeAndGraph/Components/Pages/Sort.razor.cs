using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TreeAndGraph.Services.Sorting;
using TreeAndGraph.Models.Sorting; 
using Radzen;

namespace TreeAndGraph.Components.Pages 
{
    public partial class Sort : ComponentBase, IDisposable
    {
        [Inject] IJSRuntime JSRuntime { get; set; } = default!;
        [Inject] NotificationService NotificationService { get; set; } = default!;


        private int _arraySize = 10;
        private string _inputString = string.Empty;
        private string _inputError = string.Empty;
        private List<SortItem>? _originalItems;
        private List<SortItem>? _items;
        private List<SortItem>? _currentDisplayItems;
        private int _maxValue = 100;

        private List<SortStep> _sortSteps = new List<SortStep>();
        private int _currentStepIndex = -1;

        private string _selectedAlgorithmName = string.Empty;
        private ISortAlgorithm? _selectedAlgorithm;

        private string _visualizationMode = "bar";

        private bool _isSorting = false;
        // private Timer? _autoRunTimer; // XÓA: Không cần thiết nữa
        private int _autoRunDelay = 500;
        private CancellationTokenSource _cts = new CancellationTokenSource();

        private const double VISUALIZATION_CONTAINER_HEIGHT_PX = 350.0;
        // Khoảng đệm dưới đáy khu vực bar 
        private const double BAR_BOTTOM_PADDING_PX = 0.0; // Đặt là 0 nếu muốn chạm đáy
        // Chiều cao tối thiểu của một thanh bar (bằng pixel)
        private const double MIN_BAR_HEIGHT_PX = 20.0; 

        //=========================================
        // Khởi tạo và Nhập liệu
        //=========================================

        private void OnSizeChanged(int newSize)
        {
            _arraySize = newSize;
            if (string.IsNullOrWhiteSpace(_inputString))
            {
                GenerateRandom(); // Sinh ngẫu nhiên nếu input rỗng
            }
            else
            {
                // Nếu có input, chỉ validate lại với size mới
                ValidateInput(_inputString);
            }
            // XÓA: ResetVisualizationState(); // Không reset state ở đây
            StateHasChanged();
        }

        private void ValidateInput(string input)
        {
            _inputError = string.Empty;
            if (string.IsNullOrWhiteSpace(input)) return;

            if (!Regex.IsMatch(input.Trim(), @"^\s*-?\d+(\s*,\s*-?\d+)*\s*$"))
            {
                if (!Regex.IsMatch(input.Trim(), @"^\s*-?\d+\s*$"))
                {
                    _inputError = "Chỉ được nhập số nguyên, cách nhau bởi dấu phẩy.";
                    return;
                }
            }

            var parts = input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length == 0 && Regex.IsMatch(input.Trim(), @"^\s*-?\d+\s*$"))
            {
                parts = new string[] { input.Trim() };
            }

            var numbers = new List<int>();
            foreach (var part in parts)
            {
                var trimmedPart = part.Trim();
                if (string.IsNullOrEmpty(trimmedPart)) continue;
                if (!int.TryParse(trimmedPart, out int number))
                {
                    _inputError = $"'{trimmedPart}' không phải là số nguyên hợp lệ.";
                    return;
                }
                numbers.Add(number);
            }

            if (numbers.Count != _arraySize)
            {
                _inputError = $"Số lượng phần tử ({numbers.Count}) phải bằng kích thước đã chọn ({_arraySize}).";
            }
            // Cập nhật UI nếu lỗi thay đổi (quan trọng khi dùng @bind-Value:event="oninput")
            StateHasChanged();
        }

        private void GenerateRandom()
        {
            if (_arraySize <= 0) return;
            var random = new Random();
            var numbers = Enumerable.Range(0, _arraySize).Select(_ => random.Next(1, 101)).ToList();
            _inputString = string.Join(", ", numbers); // Chỉ cập nhật chuỗi
            _inputError = string.Empty; // Xóa lỗi
            // XÓA: ResetVisualizationState(); // Không reset state ở đây
            ValidateInput(_inputString); // Validate chuỗi mới tạo (để xóa lỗi nếu có)
            StateHasChanged(); // Cập nhật UI (ô input và thông báo lỗi)
        }

        private void InitializeArray()
        {
            ValidateInput(_inputString); // Luôn validate lại
            if (!string.IsNullOrEmpty(_inputError) || string.IsNullOrWhiteSpace(_inputString))
            {
                NotificationService.Notify(NotificationSeverity.Error, "Lỗi dữ liệu", "Vui lòng kiểm tra lại dữ liệu nhập vào.");
                return;
            }

            // Chỉ reset state KHI khởi tạo thành công
            ResetVisualizationState(false); // Gọi reset state ở đây (không reset input)

            try
            {
                var parts = _inputString.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (parts.Length == 0 && Regex.IsMatch(_inputString.Trim(), @"^\s*-?\d+\s*$"))
                {
                    parts = new string[] { _inputString.Trim() };
                }
                var numbers = parts.Select(part => int.Parse(part.Trim())).ToList();

                _originalItems = numbers.Select(n => new SortItem(n)).ToList();
                _items = _originalItems.Select(item => (SortItem)item.Clone()).ToList();
                _maxValue = _items.Any() ? _items.Max(i => Math.Abs(i.Value)) : 1;
                if (_maxValue == 0) _maxValue = 1;

                _currentDisplayItems = _items.Select(item => (SortItem)item.Clone()).ToList();
                // Các reset khác đã được thực hiện trong ResetVisualizationState()

                NotificationService.Notify(NotificationSeverity.Success, "Thành công", "Đã khởi tạo mảng. Hãy chọn thuật toán.");
                StateHasChanged();
            }
            catch (Exception ex)
            {
                _inputError = $"Lỗi khởi tạo mảng: {ex.Message}";
                _items = null;
                _originalItems = null;
                _currentDisplayItems = null;
                NotificationService.Notify(NotificationSeverity.Error, "Lỗi", "Đã xảy ra lỗi khi xử lý dữ liệu.");
                StateHasChanged(); // Cập nhật UI để hiển thị lỗi
            }
        }

        //=========================================
        // Chọn và Chạy Thuật toán
        //=========================================

        private void SelectAlgorithm(string algorithmName)
        {
            if (_originalItems == null || !_originalItems.Any())
            {
                NotificationService.Notify(NotificationSeverity.Warning, "Chưa có dữ liệu", "Vui lòng nhập hoặc sinh dữ liệu trước khi chọn thuật toán.");
                return;
            }
            StopAutomaticRun();

            _selectedAlgorithmName = algorithmName;
            _items = _originalItems.Select(item => (SortItem)item.Clone()).ToList(); // Luôn lấy bản sao mới nhất
            _currentDisplayItems = _items.Select(item => (SortItem)item.Clone()).ToList(); // Hiển thị trạng thái ban đầu

            if (algorithmName == "MergeSort") _visualizationMode = "array";
            else if (algorithmName == "HeapSort") _visualizationMode = "array";
            else
            {
                if (_visualizationMode != "bar" && _visualizationMode != "array") _visualizationMode = "bar";
            }

            _selectedAlgorithm = algorithmName switch
            {
                "BubbleSort" => new BubbleSort(),
                "SelectionSort" => new SelectionSort(),
                "InsertionSort" => new InsertionSort(),
                "QuickSort" => new QuickSort(),
                "MergeSort" => new MergeSort(),
                "HeapSort" => new HeapSort(),
                _ => null
            };

            if (_selectedAlgorithm != null)
            {
                _sortSteps.Clear();
                _currentStepIndex = -1;
                try
                {
                    _sortSteps = _selectedAlgorithm.Sort(_items); // Sinh steps
                    if (_sortSteps.Any())
                    {
                        _currentStepIndex = 0; // Đặt lại bước đầu tiên
                        UpdateVisualization(); // Hiển thị bước đầu
                    }
                    else
                    {
                        _currentStepIndex = -1;
                        UpdateVisualization(); // Hiển thị trạng thái gốc nếu không có step
                    }
                    NotificationService.Notify(NotificationSeverity.Info, "Đã chọn", $"Thuật toán {_selectedAlgorithmName}. Sẵn sàng chạy.", 3000);
                }
                catch (Exception ex)
                {
                    NotificationService.Notify(NotificationSeverity.Error, "Lỗi thuật toán", $"Lỗi: {ex.Message}", 5000);
                    _sortSteps.Clear();
                    _currentStepIndex = -1;
                    _selectedAlgorithmName = "";
                    _selectedAlgorithm = null;
                    UpdateVisualization(); // Hiển thị lại trạng thái gốc
                }
            }
            else
            {
                // Xử lý trường hợp không tìm thấy thuật toán (dù switch nên bao phủ hết)
                _selectedAlgorithmName = string.Empty;
                _sortSteps.Clear();
                _currentStepIndex = -1;
                UpdateVisualization();
                NotificationService.Notify(NotificationSeverity.Error, "Lỗi", "Không tìm thấy instance thuật toán.");
            }
            StateHasChanged();
        }

        private void NextStep()
        {
            if (_sortSteps == null || !_sortSteps.Any() || _currentStepIndex >= _sortSteps.Count - 1) return;
            _currentStepIndex++;
            UpdateVisualization();
            StateHasChanged();
            ScrollDescriptionToEnd();
        }

        private async Task RunAutomatically()
        {
            if (_isSorting || _sortSteps == null || !_sortSteps.Any() || _currentStepIndex >= _sortSteps.Count - 1) return;
            _isSorting = true;
            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            StateHasChanged();

            await Task.Run(async () =>
            {
                while (_currentStepIndex < _sortSteps.Count - 1)
                {
                    if (token.IsCancellationRequested) break;
                    await InvokeAsync(() => { if (!token.IsCancellationRequested) NextStep(); });
                    if (token.IsCancellationRequested) break;
                    try { await Task.Delay(_autoRunDelay, token); }
                    catch (TaskCanceledException) { break; }
                }
                await InvokeAsync(() => {
                    _isSorting = false;
                    if (!token.IsCancellationRequested && _sortSteps.Any() && _currentStepIndex == _sortSteps.Count - 1)
                    {
                        NotificationService.Notify(NotificationSeverity.Success, "Hoàn thành", $"Đã chạy xong thuật toán {_selectedAlgorithmName}.", 3000);
                    }
                    StateHasChanged();
                });
            }, token);
        }

        private void StopAutomaticRun()
        {
            if (_isSorting)
            {
                _cts?.Cancel();
                // Để Task tự cập nhật _isSorting
                StateHasChanged(); // Cập nhật nút
                NotificationService.Notify(NotificationSeverity.Warning, "Đã dừng", "Đã dừng chạy tự động.", 3000);
            }
        }

        // SỬA: Logic ResetVisualization
        private void ResetVisualization()
        {
            StopAutomaticRun();
            if (_originalItems == null || !_originalItems.Any())
            {
                // Nếu chưa có dữ liệu gốc, coi như reset hoàn toàn
                ResetVisualizationState(true);
                NotificationService.Notify(NotificationSeverity.Info, "Đã reset", "Trạng thái đã được reset hoàn toàn.");
                StateHasChanged();
                return;
            }

            // Nếu đã có dữ liệu gốc
            _items = _originalItems.Select(item => (SortItem)item.Clone()).ToList();
            _currentDisplayItems = _items.Select(item => (SortItem)item.Clone()).ToList();
            _maxValue = _items.Any() ? _items.Max(i => Math.Abs(i.Value)) : 1;
            if (_maxValue == 0) _maxValue = 1;

            // Chỉ quay về bước 0 nếu thuật toán đã được chọn và steps đã tồn tại
            if (_selectedAlgorithm != null && _sortSteps.Any())
            {
                _currentStepIndex = 0;
                UpdateVisualization(); // Hiển thị bước đầu tiên
                NotificationService.Notify(NotificationSeverity.Info, "Đã reset", "Trở về bước đầu tiên của thuật toán.", 3000);
            }
            else
            {
                // Nếu chưa chọn thuật toán hoặc chưa có steps, chỉ hiển thị mảng gốc
                _currentStepIndex = -1;
                _currentDisplayItems?.ForEach(i => i.State = SortState.Normal);
                UpdateVisualization(); // Đảm bảo hiển thị đúng trạng thái gốc
                NotificationService.Notify(NotificationSeverity.Info, "Đã reset", "Trở về trạng thái ban đầu của mảng.", 3000);
            }
            StateHasChanged();
        }

        // SỬA: Logic ResetVisualizationState
        private void ResetVisualizationState(bool resetInput = false)
        {
            StopAutomaticRun();
            _sortSteps.Clear();
            _currentStepIndex = -1;
            _selectedAlgorithmName = string.Empty; // Reset cả tên thuật toán
            _selectedAlgorithm = null; // Reset cả thuật toán
            _items = null;
            _currentDisplayItems = null;
            _originalItems = null; // Reset cả gốc khi reset state

            if (resetInput)
            {
                _inputString = string.Empty;
                _inputError = string.Empty;
                _arraySize = 10;
                GenerateRandom(); // Sinh lại dữ liệu mới
            }
            // Không cần gọi StateHasChanged() ở đây nữa
        }

        //=========================================
        // Hiển thị và Cập nhật UI
        //=========================================

        private void UpdateVisualization()
        {
            if (_sortSteps != null && _currentStepIndex >= 0 && _currentStepIndex < _sortSteps.Count)
            {
                var currentStep = _sortSteps[_currentStepIndex];
                _currentDisplayItems = currentStep.StateSnapshot.Select(item => (SortItem)item.Clone()).ToList();
            }
            else if (_items != null) // Hiển thị _items (bản sao mới nhất) nếu không có step hợp lệ
            {
                _currentDisplayItems = _items.Select(item => (SortItem)item.Clone()).ToList();
                _currentDisplayItems.ForEach(i => i.State = SortState.Normal); // Đảm bảo là normal
            }
            else if (_originalItems != null) // Nếu _items cũng null, thử hiển thị gốc
            {
                _currentDisplayItems = _originalItems.Select(item => (SortItem)item.Clone()).ToList();
                _currentDisplayItems.ForEach(i => i.State = SortState.Normal);
            }
            else // Nếu không có gì cả
            {
                _currentDisplayItems = null;
            }
        }

        //private double CalculateBarHeight(int value)
        //{
        //    if (_maxValue <= 0) return 1;
        //    double height = Math.Max(1.0, ((double)Math.Abs(value) / _maxValue) * 100.0);
        //    return Math.Min(100.0, height);
        //}
        

        /// <summary>
        /// Tính chiều cao tuyệt đối của thanh bar (bằng pixel) dựa trên giá trị,
        /// giá trị lớn nhất, chiều cao container và chiều cao tối thiểu.
        /// </summary>
        /// <param name="value">Giá trị của phần tử.</param>
        /// <returns>Chiều cao dạng pixel (double).</returns>
        private double CalculateBarHeightPixels(double value)
        {
            // _maxValue đã được tính bằng Max(Abs(Value)) trong InitializeArray
            if (_maxValue <= 0) return MIN_BAR_HEIGHT_PX; // Tránh chia cho 0, max <= 0

            // Chiều cao khả dụng để vẽ thanh (từ đáy lên đến gần đỉnh container)
            double availableHeight = VISUALIZATION_CONTAINER_HEIGHT_PX - BAR_BOTTOM_PADDING_PX;
            if (availableHeight <= MIN_BAR_HEIGHT_PX) return MIN_BAR_HEIGHT_PX; // Đảm bảo chiều cao khả dụng hợp lệ

            // Tính chiều cao tỷ lệ
            // Nếu _maxValue = 0 (chỉ xảy ra nếu mọi value là 0), đặt chiều cao là tối thiểu
            double calculatedHeight = (_maxValue == 0) ? MIN_BAR_HEIGHT_PX :
                                      ((double)Math.Abs(value) / _maxValue) * availableHeight;

            // Đảm bảo chiều cao tối thiểu nhưng không vượt quá chiều cao khả dụng
            double finalHeight = Math.Max(MIN_BAR_HEIGHT_PX, calculatedHeight);
            finalHeight = Math.Min(finalHeight, availableHeight);

            return finalHeight;
        }


        private string GetItemCssClass(SortItem item)
        {
            string baseClass = _visualizationMode switch
            {
                "bar" => "bar",
                "array" => "array-cell",
                "heap" => "array-cell", // Map heap sang array-cell
                _ => ""
            };
            string stateClass = item.State switch
            {
                SortState.Comparing => "state-comparing",
                SortState.Swapping => "state-swapping",
                SortState.Sorted => "state-sorted",
                SortState.Pivot => "state-pivot",
                SortState.Special1 => "state-special1",
                SortState.Special2 => "state-special2",
                _ => "state-normal"
            };
            return $"{baseClass} {stateClass}";
        }

        private async void ScrollDescriptionToEnd()
        {
            try
            {
                await Task.Delay(50); // Đợi DOM update
                await JSRuntime.InvokeVoidAsync("blazorInterop.scrollToBottom", "descriptionPanel");
            }
            catch (JSDisconnectedException) { /* Bỏ qua */ }
            catch (TaskCanceledException) { /* Bỏ qua nếu task bị hủy */ }
            catch (Exception ex) { Console.WriteLine($"Error scrolling: {ex.Message}"); }
        }

        //=========================================
        // Dọn dẹp
        //=========================================

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            // _autoRunTimer?.Dispose(); // XÓA: Không cần thiết
            GC.SuppressFinalize(this);
        }
    }
}