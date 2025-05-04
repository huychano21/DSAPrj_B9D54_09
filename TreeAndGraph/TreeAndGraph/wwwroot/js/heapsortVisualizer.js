

window.drawTree = (array, highlight1, highlight2) => {
    const container = document.getElementById("tree-container");
    if (!container) {
        console.error("Tree container not found!");
        return;
    }
    if (!array || array.length === 0) {
        // Clear the container if the array is empty or null
        container.innerHTML = '';
        return;
    }

    const nodes = [];
    const edges = [];

    // Create nodes and edges based *only* on the actual array elements
    for (let i = 0; i < array.length; i++) {
        const value = array[i].toString(); // Get the value for the current node
        const isHighlighted = i === highlight1 || i === highlight2;

        // Add the node
        nodes.push({
            id: i,
            label: value,
            color: isHighlighted ? '#FF6B6B' : '#97C2FC', // Highlight color (e.g., light red), default node color
            font: { size: 18, color: isHighlighted ? '#FFFFFF' : '#000000' }, // White text on highlighted, black otherwise
            borderWidth: isHighlighted ? 2 : 1,
            borderColor: isHighlighted ? '#C00000' : '#2B7CE9' // Darker border for highlighted
        });

        // Calculate children indices
        const leftChildIndex = 2 * i + 1;
        const rightChildIndex = 2 * i + 2;

        // Add edge to left child *only if* it exists within the array bounds
        if (leftChildIndex < array.length) {
            edges.push({ from: i, to: leftChildIndex, arrows: { to: { enabled: false } } }); // Ensure no arrows
        }

        // Add edge to right child *only if* it exists within the array bounds
        if (rightChildIndex < array.length) {
            edges.push({ from: i, to: rightChildIndex, arrows: { to: { enabled: false } } }); // Ensure no arrows
        }
    }

    // Create the data object for vis.js
    const data = {
        nodes: new vis.DataSet(nodes),
        edges: new vis.DataSet(edges)
    };

    // Configure vis.js options
    const options = {
        layout: {
            hierarchical: {
                direction: "UD", // Top-Down layout
                sortMethod: "directed", // Ensures structure is like a tree
                nodeSpacing: 100,      // Horizontal spacing
                levelSeparation: 80,  // Vertical spacing between levels
                parentCentralization: true,
                shakeTowards: 'roots'
            }
        },
        edges: {
            smooth: {
                enabled: true,
                type: "cubicBezier", // Or 'dynamic' if preferred
                forceDirection: "vertical",
                roundness: 0.5
            },
            color: {
                color: '#848484', // Default edge color
                highlight: '#848484' // Keep color same on hover
            }
            // arrows: { to: false } // Global setting (can be overridden per edge) - Redundant if set per edge
        },
        nodes: {
            shape: 'ellipse', // or 'circle'
            size: 25, // Default node size
            font: {
                size: 18 // Default font size if not overridden
            }
        },
        physics: {
            enabled: false // Disable physics for static tree layout
        },
        interaction: {
            dragNodes: false, // Prevent user from dragging nodes
            dragView: true,  // Allow panning the view
            zoomView: true   // Allow zooming
        }
    };

    // Create and render the network graph
    new vis.Network(container, data, options);
};

// Optional: Function to clear the tree container
window.clearTree = () => {
    const container = document.getElementById("tree-container");
    if (container) {
        container.innerHTML = '';
    }
}