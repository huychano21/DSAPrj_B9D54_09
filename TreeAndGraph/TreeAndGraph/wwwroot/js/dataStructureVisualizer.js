// wwwroot/js/dataStructureVisualizer.js

// --- Generic Helper ---
function createVisualItem(value, className) {
    const item = document.createElement('div'); 
    item.classList.add(className); // e.g., 'stack-item', 'queue-item', 'list-item'
    item.innerText = value;
    return item;
}

function addAnimationAndRemove(item, animationClass) {
    item.classList.add(animationClass);
    item.addEventListener('animationend', () => {
        item.remove();
    }, { once: true });
}

function addAnimationAndCleanup(item, animationClass) {
    item.classList.add(animationClass);
    item.addEventListener('animationend', () => {
        item.classList.remove(animationClass);
    }, { once: true });
}

function getContainer(containerId) {
    const container = document.getElementById(containerId);
    if (!container) {
        console.error(`Container with id "${containerId}" not found.`);
        return null;
    }
    return container;
}

// --- Stack Functions ---
window.jsStackPush = (containerId, value) => {
    const container = getContainer(containerId);
    if (!container) return;
    const newItem = createVisualItem(value, 'stack-item');
    container.prepend(newItem); // Visual top due to column-reverse
    addAnimationAndCleanup(newItem, 'stack-item-push');
};

window.jsStackPop = (containerId) => {
    return new Promise((resolve) => {
        const container = getContainer(containerId);
        if (!container) { resolve(); return; }
        const topItem = container.firstElementChild; // Visual top is first in DOM
        if (topItem) {
            addAnimationAndRemove(topItem, 'stack-item-pop');
            // Resolve slightly before animation ends to allow C# state update
            setTimeout(resolve, 350); // Adjust timing based on CSS animation duration
        } else {
            resolve();
        }
    });
};

window.jsStackClear = (containerId) => {
    const container = getContainer(containerId);
    if (container) container.innerHTML = '';
};


// --- Queue Functions ---
window.jsQueueEnqueue = (containerId, value) => {
    const container = getContainer(containerId);
    if (!container) return;
    const newItem = createVisualItem(value, 'queue-item');
    container.appendChild(newItem); // Add to the visual end (right)
    addAnimationAndCleanup(newItem, 'queue-item-enqueue');
};

window.jsQueueDequeue = (containerId) => {
    return new Promise((resolve) => {
        const container = getContainer(containerId);
        if (!container) { resolve(); return; }
        const frontItem = container.firstElementChild; // Visual front is first in DOM
        if (frontItem) {
            addAnimationAndRemove(frontItem, 'queue-item-dequeue');
            setTimeout(resolve, 350);
        } else {
            resolve();
        }
    });
};

window.jsQueueClear = (containerId) => {
    const container = getContainer(containerId);
    if (container) container.innerHTML = '';
};


// --- List Functions (Simplified) ---
window.jsListAdd = (containerId, value) => {
    const container = getContainer(containerId);
    if (!container) return;
    const newItem = createVisualItem(value, 'list-item');
    // Optional: Add index data attribute if needed later
    // newItem.setAttribute('data-index', container.children.length);
    container.appendChild(newItem); // Add to the end
    addAnimationAndCleanup(newItem, 'list-item-add');
};

window.jsListRemove = (containerId, index) => {
    return new Promise((resolve) => {
        const container = getContainer(containerId);
        if (!container) { resolve(); return; }
        if (index >= 0 && index < container.children.length) {
            const itemToRemove = container.children[index];
            addAnimationAndRemove(itemToRemove, 'list-item-remove');
            setTimeout(resolve, 350);
            // Note: This does not visually update indices of subsequent items
        } else {
            console.warn(`jsListRemove: Invalid index ${index}`);
            resolve(); // Resolve even if index invalid
        }
    });
};

window.jsListClear = (containerId) => {
    const container = getContainer(containerId);
    if (container) container.innerHTML = '';
};