
// Function to get bounding client rectangle for an element
function getBoundingClientRect(element) {
    if (element && typeof element.getBoundingClientRect === 'function') {
        // console.log('JS: getBoundingClientRect called for', element);
        return element.getBoundingClientRect();
    }
    // console.log('JS: getBoundingClientRect failed for', element);
    return null;
}

// --- Optional: Add more JS functions if needed for complex interactions ---
// Example: Maybe functions to handle SVG pan/zoom if you add that later.
// function initializeSvgPanZoom(svgElementId) { ... }

console.log("tree-interop.js loaded"); // For debugging