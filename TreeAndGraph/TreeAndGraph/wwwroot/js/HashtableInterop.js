// Function to add a temporary CSS class for a flash effect
function flashElement(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.classList.add('flash-effect');
        // Remove the class after the animation duration (adjust timing if needed)
        setTimeout(() => {
            element.classList.remove('flash-effect');
        }, 500); // Duration matches CSS animation
    } else {
        console.warn(`Element with ID '${elementId}' not found for flashing.`);
    }
}

// Optional: Function to scroll an element into view
function scrollToElement(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    }
}