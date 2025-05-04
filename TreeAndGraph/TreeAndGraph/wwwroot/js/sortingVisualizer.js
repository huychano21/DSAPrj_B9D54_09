// wwwroot/js/sortingVisualizer.js

// --- Configuration ---
const transitionDuration = 300; // ms, should match --transition-duration in CSS if possible
const blinkDuration = 400; // ms, should match blink animation duration * repeats

// --- Helper Functions ---
function getElementById(id) {
    const element = document.getElementById(id);
    // if (!element) console.error(`Element with ID "${id}" not found.`);
    return element;
}

// Function to create a single bar element
function createBarElement(item, maxVal, containerHeight) {
    const bar = document.createElement('div');
    bar.classList.add('bar-item');
    bar.style.height = `${Math.max(5, (item.value / maxVal) * (containerHeight - 20))}px`; // Min height 5px, leave space at top
    bar.dataset.id = item.id; // Store C# Guid ID
    bar.dataset.value = item.value; // Store value if needed
    // bar.innerText = item.value; // Show value on bar if needed and space permits
    updateElementState(bar, item.state); // Apply initial state class
    return bar;
}

// Function to create a single array box element
function createArrayElement(item) {
    const box = document.createElement('div');
    box.classList.add('array-item');
    box.dataset.id = item.id; // Store C# Guid ID
    box.dataset.value = item.value;
    box.innerText = item.value;
    updateElementState(box, item.state); // Apply initial state class
    return box;
}

// Function to apply state classes
function updateElementState(element, stateName) {
    // Remove all known state classes first
    const states = ["normal", "comparing", "swapping", "pivot", "sorted", "minselection", "lefthalf", "righthalf", "merging", "heapcomparing", "heapswapping"];
    element.classList.remove(...states);

    // Add the new state class (convert enum string name to lowercase)
    if (stateName && stateName.toLowerCase() !== 'normal') {
        element.classList.add(stateName.toLowerCase());
    }
}

// Function to add temporary blinking effect
function blinkElements(elements) {
    elements.forEach(el => {
        if (el) {
            el.classList.add('blinking');
            setTimeout(() => {
                el.classList.remove('blinking');
            }, blinkDuration); // Remove after animation duration
        }
    });
}

// --- Global Draw Functions ---

window.sortingVisualizer = {
    // --- Bar Chart ---
    drawBars: (containerId, items) => {
        const container = getElementById(containerId);
        if (!container || !items) return;
        container.innerHTML = ''; // Clear previous bars

        const maxValue = items.length > 0 ? Math.max(...items.map(i => i.value)) : 100;
        const containerHeight = container.clientHeight || 330; // Use container height or default

        items.forEach(item => {
            const barElement = createBarElement(item, maxValue, containerHeight);
            container.appendChild(barElement);
        });
    },

    updateBars: (containerId, step) => {
        const container = getElementById(containerId);
        if (!container || !step) return;

        const findBar = (index) => container.children[index] || null; // Assumes order matches C# index
        // More robust: Find by ID if indices might not match DOM order after swaps
        // const findBarById = (id) => container.querySelector(`[data-id="${id}"]`);

        switch (step.type) {
            case "compare": {
                const bar1 = findBar(step.index1);
                const bar2 = findBar(step.index2);
                if (bar1) updateElementState(bar1, "comparing");
                if (bar2) updateElementState(bar2, "comparing");
                blinkElements([bar1, bar2]);
                break;
            }
            case "swap": {
                const bar1 = findBar(step.index1);
                const bar2 = findBar(step.index2);
                if (bar1 && bar2) {
                    updateElementState(bar1, "swapping");
                    updateElementState(bar2, "swapping");
                    blinkElements([bar1, bar2]);

                    // --- Simple Swap Animation (Height/Value) ---
                    // For simplicity, we swap heights and data attributes directly.
                    // True positional swap requires transform/absolute positioning.
                    const h1 = bar1.style.height;
                    const v1 = bar1.dataset.value;
                    const id1 = bar1.dataset.id; // Keep ID with visual position for now

                    bar1.style.height = bar2.style.height;
                    bar1.dataset.value = bar2.dataset.value;
                    // bar1.innerText = bar2.dataset.value; // Update text if shown

                    bar2.style.height = h1;
                    bar2.dataset.value = v1;
                    // bar2.innerText = v1;

                    // Update IDs if necessary to match logical swap (more complex)
                    // bar1.dataset.id = bar2.dataset.id;
                    // bar2.dataset.id = id1;

                }
                break;
            }
            case "set": // Not typically used for bars, but could adjust height
                const barSet = findBar(step.index);
                if (barSet) {
                    const maxValue = Math.max(step.value, ...Array.from(container.children).map(b => parseInt(b.dataset.value || "0")));
                    const containerHeight = container.clientHeight || 330;
                    barSet.style.height = `${Math.max(5, (step.value / maxValue) * (containerHeight - 20))}px`;
                    barSet.dataset.value = step.value;
                    // barSet.innerText = step.value;
                    updateElementState(barSet, step.state || "merging"); // Apply state if provided
                }
                break;
            case "highlight": {
                step.indices.forEach(index => {
                    const barH = findBar(index);
                    if (barH) updateElementState(barH, step.state);
                });
                // Optional blink on highlight
                // blinkElements(step.indices.map(findBar));
                break;
            }
            case "partition": {
                const pivotBar = findBar(step.pivotIndex);
                if (pivotBar) updateElementState(pivotBar, "pivot");
                // Optionally highlight low/high boundary bars
                break;
            }
            // Add cases for merge_subarray if specific bar highlighting needed
        }
    },

    // --- Array View ---
    drawArray: (containerId, items) => {
        const container = getElementById(containerId);
        if (!container || !items) return;
        container.innerHTML = ''; // Clear previous items

        items.forEach(item => {
            const arrayElement = createArrayElement(item);
            container.appendChild(arrayElement);
        });
    },

    updateArray: (containerId, step) => {
        const container = getElementById(containerId);
        if (!container || !step) return;

        const findBox = (index) => container.children[index] || null; // Assumes order matches

        switch (step.type) {
            case "compare": {
                const box1 = findBox(step.index1);
                const box2 = findBox(step.index2);
                if (box1) updateElementState(box1, "comparing");
                if (box2) updateElementState(box2, "comparing");
                blinkElements([box1, box2]);
                break;
            }
            case "swap": {
                const box1 = findBox(step.index1);
                const box2 = findBox(step.index2);
                if (box1 && box2) {
                    updateElementState(box1, "swapping");
                    updateElementState(box2, "swapping");
                    blinkElements([box1, box2]);

                    // --- Simple Swap Animation (Text Content) ---
                    // More complex animation could involve transform: translateX
                    const val1 = box1.innerText;
                    const dsVal1 = box1.dataset.value;
                    const id1 = box1.dataset.id;

                    box1.innerText = box2.innerText;
                    box1.dataset.value = box2.dataset.value;
                    // box1.dataset.id = box2.dataset.id; // Update ID if necessary

                    box2.innerText = val1;
                    box2.dataset.value = dsVal1;
                    // box2.dataset.id = id1;
                }
                break;
            }
            case "set": {
                const boxSet = findBox(step.index);
                if (boxSet) {
                    boxSet.innerText = step.value;
                    boxSet.dataset.value = step.value;
                    updateElementState(boxSet, step.state || "merging");
                    blinkElements([boxSet]);
                }
                break;
            }
            case "highlight": {
                step.indices.forEach(index => {
                    const boxH = findBox(index);
                    if (boxH) updateElementState(boxH, step.state);
                });
                // Optional blink on highlight
                // blinkElements(step.indices.map(findBox));
                break;
            }
            case "partition": {
                const pivotBox = findBox(step.pivotIndex);
                if (pivotBox) updateElementState(pivotBox, "pivot");
                // Optionally highlight low/high boundaries
                break;
            }
            case "merge_subarray": {
                // Highlight subarrays being merged
                const leftIndices = Array.from({ length: step.mid - step.start + 1 }, (_, i) => step.start + i);
                const rightIndices = Array.from({ length: step.end - step.mid }, (_, i) => step.mid + 1 + i);
                leftIndices.forEach(index => { const box = findBox(index); if (box) updateElementState(box, "lefthalf"); });
                rightIndices.forEach(index => { const box = findBox(index); if (box) updateElementState(box, "righthalf"); });
                break;
            }
        }
    },

    // --- Generic Clear ---
    clearVisualizer: (containerId) => {
        const container = getElementById(containerId);
        if (container) container.innerHTML = '';
    }
};

// --- HeapSort Visualizer Integration ---
// Assume heapsortVisualizer.js exists and defines window.heapsortVisualizer object
// We need to adapt it or add functions here that understand the new steps

window.heapsortVisualizer = window.heapsortVisualizer || {}; // Ensure object exists

// Adapt/create drawing function for heap sort (needs array + tree)
window.heapsortVisualizer.drawHeapTree = (containerId, initialArray, highlight1, highlight2, description) => {
    // TODO: Implement or adapt drawing logic for heap tree + array view
    // This likely reuses much of the original heapsort drawTree function
    // but might need adjustments for the container and description display.
    // It should draw the initial state based on initialArray.
    console.warn("drawHeapTree function not fully implemented/adapted yet.");
    const container = getElementById(containerId);
    if (container) container.innerHTML = `<p>Heap visualization placeholder for array: [${initialArray.join(', ')}]</p><div id="heap-tree-vis-${containerId}" style="height: 300px; border: 1px dashed grey;"></div><div id="heap-array-vis-${containerId}"></div>`;
    // Call original vis.js logic here targeting the inner div id?

    // Also need to draw the array representation part:
    // sortingVisualizer.drawArray(`heap-array-vis-${containerId}`, initialArray.map(v => ({Value: v, State: ElementState.Normal, Id: Guid.NewGuid() }))); // Need a way to create ArrayItem-like objects

};

// Adapt/create update function for heap sort steps
window.heapsortVisualizer.updateHeapTree = (containerId, step) => {
    // TODO: Implement logic to update tree/array based on HeapStep or SwapStep
    console.log("Updating Heap Tree:", containerId, step);
    const descElement = document.getElementById("step-description"); // Assuming common description area
    if (descElement && step.description) descElement.innerText = step.description;

    // Find elements in tree/array to highlight/swap based on step.index1, step.index2, step.isSwap
    // This requires integrating with the vis.js update mechanisms or direct DOM manipulation
    // if not using vis.js for the new heap view.
};

// Adapt/create clear function
window.heapsortVisualizer.clearHeapTree = (containerId) => {
    const container = getElementById(containerId);
    if (container) container.innerHTML = '';
};

// TreeAndGraph/wwwroot/js/interop.js

// Hàm này có thể dùng để thực hiện animation phức tạp hơn nếu cần,
// ví dụ: di chuyển mượt mà các thanh bar khi hoán đổi.
// Hiện tại, Blazor và CSS transition đã xử lý việc thay đổi màu sắc/chiều cao.
// Việc di chuyển vị trí thực sự (swap) bằng JS sẽ phức tạp hơn nhiều
// vì cần đồng bộ với trạng thái Blazor.

window.blazorInterop = {
    // Ví dụ hàm JS có thể được gọi từ C#
    highlightElement: function (elementId, cssClass) {
        const element = document.getElementById(elementId);
        if (element) {
            // Xóa các lớp trạng thái cũ trước khi thêm lớp mới
            element.classList.remove('state-comparing', 'state-swapping', 'state-sorted', 'state-pivot', 'state-special1', 'state-special2');
            if (cssClass && cssClass !== 'state-normal') { // Chỉ thêm nếu class khác normal
                element.classList.add(cssClass);
            }
            // console.log(`Highlighting ${elementId} with ${cssClass}`);
        } else {
            // console.warn(`Element with ID ${elementId} not found for highlighting.`);
        }
    },

    // Hàm để cuộn khu vực mô tả xuống dưới cùng khi có bước mới
    scrollToBottom: function (elementId) {
        const element = document.getElementById(elementId);
        if (element) {
            element.scrollTop = element.scrollHeight;
        }
    }

    // Có thể thêm các hàm animation khác ở đây, ví dụ:
    // animateSwap: function(elementId1, elementId2, duration) { ... }
    // animateTreeHighlight: function(nodeId) { ... }
};

// Lưu ý: Việc gọi trực tiếp các hàm JS để thay đổi class như highlightElement
// có thể xung đột với cách Blazor render lại DOM dựa trên trạng thái C#.
// Cách tiếp cận an toàn hơn là cập nhật trạng thái trong C# (SortItem.State),
// để Blazor render lại với class CSS tương ứng, và dùng CSS transition/animation.
// JS Interop nên dùng cho những thứ Blazor/CSS không làm được hoặc làm không tốt (animation phức tạp).