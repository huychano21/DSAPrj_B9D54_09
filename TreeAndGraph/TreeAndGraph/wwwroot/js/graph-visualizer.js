// Biến toàn cục để lưu trữ tham chiếu đến đối tượng helper của Blazor
let dotNetHelper = null;

// Biến cho Canvas và Context
let canvas = null;
let ctx = null;

// Biến lưu trữ dữ liệu đồ thị và cấu hình
let nodes = [];
let edges = [];
let isDirected = false;
let isWeighted = false;
let nodeRadius = 15; // Bán kính nút mặc định
// Các màu mặc định (có thể được ghi đè bởi config từ Blazor nếu bạn thêm logic đó)
let nodeDefaultColor = '#64b5f6'; // Xanh dương nhạt
let nodeFixedColor = '#cccccc';   // Xám nhạt
let nodeHighlightFill = 'yellow';
let nodeHighlightStroke = 'orange';
let nodeStrokeColor = '#222222'; // Viền đen
let edgeDefaultColor = '#555555'; // Xám đậm
let edgeHighlightColor = 'orange';
let nodeLabelColor = '#111111'; // Đen
let edgeWeightColor = 'blue';
let edgeWeightHighlightColor = 'orange';
let tempEdgeColor = 'red';

// Biến trạng thái tương tác
let currentMode = 'force'; // Chế độ hiện tại: 'force', 'draw', 'edit', 'delete'
let draggingNode = null;   // Nút đang được kéo thả
let drawingEdgeFrom = null; // Nút bắt đầu khi vẽ cạnh mới
let offsetX = 0;           // Độ lệch X khi kéo thả
let offsetY = 0;           // Độ lệch Y khi kéo thả
let lastMouseX = 0;        // Vị trí X cuối cùng của chuột
let lastMouseY = 0;        // Vị trí Y cuối cùng của chuột
let isMouseDown = false;   // Cờ báo chuột đang được nhấn giữ

// Biến trạng thái cho trực quan hóa thuật toán
let highlightedNodes = new Set(); // Tập hợp ID các nút được highlight
let highlightedEdges = new Set(); // Tập hợp các cạnh được highlight (lưu dưới dạng chuỗi "from-to")
let nodeLabels = {};              // Nhãn đặc biệt cho nút từ thuật toán { nodeId: "label" }

// --- Khởi tạo ---
export function initGraphCanvas(dotnetHelperRef, initialGraphData) {
    dotNetHelper = dotnetHelperRef;
    canvas = document.getElementById('graphCanvas');
    if (!canvas) {
        console.error("Lỗi: Không tìm thấy phần tử canvas 'graphCanvas'!");
        return;
    }
    ctx = canvas.getContext('2d');
    console.log("JS: initGraphCanvas - Dữ liệu ban đầu nhận được:", JSON.stringify(initialGraphData));
    updateGraphData(initialGraphData || { nodes: [], edges: [], isDirected: false, isWeighted: false });
    canvas.addEventListener('mousedown', handleMouseDown);
    canvas.addEventListener('mousemove', handleMouseMove);
    canvas.addEventListener('mouseup', handleMouseUp);
    canvas.addEventListener('dblclick', handleDoubleClick);
    canvas.addEventListener('mouseleave', handleMouseLeave);
    window.addEventListener('resize', redrawCanvas);
    console.log("Khởi tạo Graph Canvas thành công.");
}

// --- Cập nhật dữ liệu và cấu hình từ Blazor ---
export function updateGraphData(graphData) {
    console.log("JS: updateGraphData - Dữ liệu nhận được:", JSON.stringify(graphData));
    if (!graphData || !Array.isArray(graphData.nodes) || !Array.isArray(graphData.edges)) {
        console.error("Dữ liệu đồ thị không hợp lệ nhận từ Blazor:", graphData);
        nodes = []; edges = []; isDirected = false; isWeighted = false;
    } else {
        nodes = graphData.nodes; // Expects camelCase: id, x, y, label, isFixed
        edges = graphData.edges; // Expects camelCase: from, to, weight
        isDirected = graphData.isDirected || false;
        isWeighted = graphData.isWeighted || false;
    }
    draggingNode = null;
    isMouseDown = false;
    redrawCanvas();
}

export function updateConfig(config) {
    console.log("JS: updateConfig:", config);
    nodeRadius = config.nodeRadius ?? nodeRadius;
    redrawCanvas();
}

export function setMode(mode) {
    console.log("JS: setMode:", mode);
    currentMode = mode.toLowerCase();
    draggingNode = null;
    drawingEdgeFrom = null;
    isMouseDown = false;
    if (canvas) { canvas.style.cursor = getCursorForMode(currentMode); }
    redrawCanvas();
}

// --- Các hàm vẽ (Phiên bản gốc đã sửa lỗi case sensitivity) ---
function redrawCanvas() {
    if (!ctx || !canvas) { console.error("JS: redrawCanvas - Canvas hoặc Context không hợp lệ."); return; }
    const currentWidth = canvas.offsetWidth;
    const currentHeight = 600;
    if (canvas.width !== currentWidth || canvas.height !== currentHeight) {
        canvas.width = currentWidth; canvas.height = currentHeight;
    }
    if (canvas.width <= 0 || canvas.height <= 0) { console.warn("JS: redrawCanvas - Kích thước Canvas không hợp lệ (<= 0)."); return; }
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    try {
        edges.forEach(drawEdge);
        nodes.forEach(drawNode);
        if (currentMode === 'draw' && drawingEdgeFrom) {
            drawTemporaryEdge(drawingEdgeFrom, { x: lastMouseX, y: lastMouseY });
        }
    } catch (error) {
        console.error("Lỗi trong quá trình vẽ canvas:", error, error.stack);
        ctx.fillStyle = 'red'; ctx.font = '16px sans-serif'; ctx.textAlign = 'center';
        ctx.fillText("Lỗi hiển thị đồ thị! Kiểm tra Console.", canvas.width / 2, canvas.height / 2);
    }
}

function drawNode(node) {
    if (!node || node.id === undefined || node.id === null || node.x === undefined || node.y === undefined) {
        // console.warn("JS: drawNode - Bỏ qua nút không hợp lệ:", JSON.stringify(node));
        return;
    }
    // if (typeof node.id !== 'number' && typeof node.id !== 'string') { console.warn(`JS: drawNode - Node id không phải là số hoặc chuỗi: id=${node.id}, Type=${typeof node.id}`, node); }

    ctx.beginPath();
    ctx.arc(node.x, node.y, nodeRadius, 0, Math.PI * 2);

    if (highlightedNodes.has(node.id)) {
        ctx.fillStyle = nodeHighlightFill; ctx.lineWidth = 2; ctx.strokeStyle = nodeHighlightStroke;
    } else {
        ctx.fillStyle = node.isFixed ? nodeFixedColor : nodeDefaultColor;
        ctx.lineWidth = 1; ctx.strokeStyle = nodeStrokeColor;
    }
    ctx.fill(); ctx.stroke();

    let label = 'ERR';
    const nodeSpecificLabel = nodeLabels[node.id];
    const nodeDataLabel = node.label;
    let nodeIdStr = `(ID?)`;
    try { nodeIdStr = node.id.toString(); }
    catch (e) { console.error(`JS: drawNode - Lỗi khi gọi toString() cho node.id: ${node.id}`, "Đối tượng nút:", JSON.stringify(node), "Lỗi:", e); }
    label = nodeSpecificLabel || nodeDataLabel || nodeIdStr;

    ctx.fillStyle = nodeLabelColor;
    ctx.textAlign = 'center'; ctx.textBaseline = 'middle';
    ctx.font = `${Math.max(8, nodeRadius * 0.8)}px sans-serif`;
    ctx.fillText(label, node.x, node.y);
}

function drawEdge(edge) {
    if (!edge || edge.from === undefined || edge.from === null || edge.to === undefined || edge.to === null) { return; }
    const fromNode = nodes.find(n => n && n.id === edge.from); const toNode = nodes.find(n => n && n.id === edge.to);
    if (!fromNode || !toNode || fromNode.x === undefined || fromNode.y === undefined || toNode.x === undefined || toNode.y === undefined) { return; }
    const edgeKey = `${edge.from}-${edge.to}`; const reverseEdgeKey = `${edge.to}-${edge.from}`;
    ctx.beginPath(); ctx.moveTo(fromNode.x, fromNode.y); ctx.lineTo(toNode.x, toNode.y);
    const isHighlighted = highlightedEdges.has(edgeKey) || (!isDirected && highlightedEdges.has(reverseEdgeKey));
    ctx.strokeStyle = isHighlighted ? edgeHighlightColor : edgeDefaultColor; ctx.lineWidth = isHighlighted ? 3 : 1.5; ctx.stroke();
    if (isDirected) { drawArrowhead(ctx, fromNode, toNode, ctx.strokeStyle, ctx.lineWidth); }
    if (isWeighted && edge.weight !== undefined && edge.weight !== null) { drawEdgeWeight(ctx, fromNode, toNode, edge.weight, isHighlighted); }
    ctx.lineWidth = 1;
}

function drawTemporaryEdge(fromNode, toPos) {
    if (!fromNode || fromNode.x === undefined || fromNode.y === undefined) return;
    ctx.beginPath(); ctx.moveTo(fromNode.x, fromNode.y); ctx.lineTo(toPos.x, toPos.y);
    ctx.strokeStyle = tempEdgeColor; ctx.lineWidth = 1; ctx.setLineDash([4, 4]); ctx.stroke(); ctx.setLineDash([]);
}

function drawArrowhead(ctx, fromNode, toNode, color, lineWidth) {
    if (!fromNode || !toNode || fromNode.x === undefined || fromNode.y === undefined || toNode.x === undefined || toNode.y === undefined) return;
    const headlen = Math.max(8, nodeRadius * 0.6); const dx = toNode.x - fromNode.x; const dy = toNode.y - fromNode.y;
    if (dx === 0 && dy === 0) return; const angle = Math.atan2(dy, dx);
    const edgeEndX = toNode.x - nodeRadius * Math.cos(angle); const edgeEndY = toNode.y - nodeRadius * Math.sin(angle);
    ctx.save(); ctx.strokeStyle = color; ctx.fillStyle = color; ctx.lineWidth = lineWidth;
    ctx.beginPath(); ctx.moveTo(edgeEndX, edgeEndY);
    ctx.lineTo(edgeEndX - headlen * Math.cos(angle - Math.PI / 6), edgeEndY - headlen * Math.sin(angle - Math.PI / 6));
    ctx.lineTo(edgeEndX - headlen * Math.cos(angle + Math.PI / 6), edgeEndY - headlen * Math.sin(angle + Math.PI / 6));
    ctx.closePath(); ctx.stroke(); ctx.fill(); ctx.restore();
}

function drawEdgeWeight(ctx, fromNode, toNode, weight, isHighlighted) {
    if (!fromNode || !toNode || fromNode.x === undefined || fromNode.y === undefined || toNode.x === undefined || toNode.y === undefined) return;
    const midX = (fromNode.x + toNode.x) / 2; const midY = (fromNode.y + toNode.y) / 2;
    const text = typeof weight === 'number' ? weight.toFixed(1) : '?';
    ctx.font = `${Math.max(8, nodeRadius * 0.65)}px sans-serif`;
    const textWidth = ctx.measureText(text).width; const textHeight = nodeRadius * 0.7;
    ctx.fillStyle = 'rgba(248, 249, 250, 0.8)'; // Background color for readability
    ctx.fillRect(midX - textWidth / 2 - 2, midY - textHeight / 2 - 2, textWidth + 4, textHeight + 4);
    ctx.fillStyle = isHighlighted ? edgeWeightHighlightColor : edgeWeightColor;
    ctx.textAlign = 'center'; ctx.textBaseline = 'middle'; ctx.fillText(text, midX, midY);
}

// --- Xử lý tương tác ---
function getMousePos(canvasEl, evt) {
    if (!canvasEl) return { x: 0, y: 0 }; const rect = canvasEl.getBoundingClientRect();
    const scaleX = canvasEl.width / rect.width; const scaleY = canvasEl.height / rect.height;
    return { x: (evt.clientX - rect.left) * scaleX, y: (evt.clientY - rect.top) * scaleY };
}
function getNodeAtPos(x, y) {
    for (let i = nodes.length - 1; i >= 0; i--) {
        const node = nodes[i];
        if (!node || node.x === undefined || node.y === undefined || node.id === undefined) continue;
        const dx = x - node.x; const dy = y - node.y;
        if (dx * dx + dy * dy < nodeRadius * nodeRadius) { return node; }
    } return null;
}
function isPointOnLine(p, a, b, threshold = 5) {
    if (!p || !a || !b || a.x === undefined || a.y === undefined || b.x === undefined || b.y === undefined) return false;
    const dxL = b.x - a.x; const dyL = b.y - a.y; const lenSq = dxL * dxL + dyL * dyL;
    if (lenSq === 0) { const dxP = p.x - a.x; const dyP = p.y - a.y; return dxP * dxP + dyP * dyP < threshold * threshold; }
    const t = ((p.x - a.x) * dxL + (p.y - a.y) * dyL) / lenSq; let projX, projY;
    if (t < 0) { projX = a.x; projY = a.y; } else if (t > 1) { projX = b.x; projY = b.y; } else { projX = a.x + t * dxL; projY = a.y + t * dyL; }
    const dx = p.x - projX; const dy = p.y - projY; const distSq = dx * dx + dy * dy; return distSq < threshold * threshold;
}
function getEdgeAtPos(x, y) {
    for (const edge of edges) {
        if (!edge || edge.from === undefined || edge.to === undefined) continue;
        const fromNode = nodes.find(n => n && n.id === edge.from); const toNode = nodes.find(n => n && n.id === edge.to);
        if (fromNode && toNode) { if (isPointOnLine({ x, y }, fromNode, toNode, nodeRadius * 0.5)) { return edge; } }
    } return null;
}
function handleMouseDown(e) {
    if (!dotNetHelper || !canvas) return; isMouseDown = true; const pos = getMousePos(canvas, e);
    const clickedNode = getNodeAtPos(pos.x, pos.y); const clickedEdge = !clickedNode ? getEdgeAtPos(pos.x, pos.y) : null;
    switch (currentMode) {
        case 'force':
            if (clickedNode && !clickedNode.isFixed) { draggingNode = clickedNode; offsetX = pos.x - draggingNode.x; offsetY = pos.y - draggingNode.y; canvas.style.cursor = 'grabbing'; }
            else if (clickedNode && clickedNode.isFixed) { canvas.style.cursor = 'not-allowed'; }
            else if (!clickedNode && !clickedEdge) { dotNetHelper.invokeMethodAsync('HandleNodeClick', -1); } break;
        case 'draw':
            if (clickedNode) { drawingEdgeFrom = clickedNode; canvas.style.cursor = 'crosshair'; redrawCanvas(); }
            else { if (drawingEdgeFrom) { drawingEdgeFrom = null; redrawCanvas(); } else { dotNetHelper.invokeMethodAsync('HandleCanvasClick', pos.x, pos.y); } } break;
        case 'edit':
            if (clickedNode) { dotNetHelper.invokeMethodAsync('HandleNodeClick', clickedNode.id); }
            else if (clickedEdge && isWeighted) { dotNetHelper.invokeMethodAsync('HandleEdgeClick', clickedEdge.from, clickedEdge.to); } break;
        case 'delete':
            if (clickedNode) { dotNetHelper.invokeMethodAsync('HandleNodeClick', clickedNode.id); }
            else if (clickedEdge) { dotNetHelper.invokeMethodAsync('HandleEdgeClick', clickedEdge.from, clickedEdge.to); } break;
    }
}
function handleMouseMove(e) {
    if (!canvas) return; const pos = getMousePos(canvas, e); lastMouseX = pos.x; lastMouseY = pos.y;
    if (currentMode === 'force' && draggingNode) { draggingNode.x = pos.x - offsetX; draggingNode.y = pos.y - offsetY; redrawCanvas(); }
    else if (currentMode === 'draw' && drawingEdgeFrom) { redrawCanvas(); }
    else if (isMouseDown && currentMode === 'force') { /* Panning? */ }
    else { const hoveredNode = getNodeAtPos(pos.x, pos.y); const hoveredEdge = !hoveredNode ? getEdgeAtPos(pos.x, pos.y) : null; canvas.style.cursor = getCursorForMode(currentMode, hoveredNode, hoveredEdge); }
}
function handleMouseUp(e) {
    if (!dotNetHelper || !canvas) return; isMouseDown = false; const pos = getMousePos(canvas, e);
    if (currentMode === 'force' && draggingNode) { dotNetHelper.invokeMethodAsync('UpdateNodePosition', draggingNode.id, draggingNode.x, draggingNode.y); draggingNode = null; const hoveredNode = getNodeAtPos(pos.x, pos.y); canvas.style.cursor = getCursorForMode(currentMode, hoveredNode); }
    else if (currentMode === 'draw' && drawingEdgeFrom) { const targetNode = getNodeAtPos(pos.x, pos.y); if (targetNode && targetNode.id !== drawingEdgeFrom.id) { dotNetHelper.invokeMethodAsync('HandleEdgeDrawn', drawingEdgeFrom.id, targetNode.id); } drawingEdgeFrom = null; canvas.style.cursor = getCursorForMode(currentMode); redrawCanvas(); }
}
function handleDoubleClick(e) {
    if (!dotNetHelper || !canvas) return; const pos = getMousePos(canvas, e); const clickedNode = getNodeAtPos(pos.x, pos.y);
    if (currentMode === 'force' && clickedNode) { clickedNode.isFixed = !clickedNode.isFixed; console.log(`Node ${clickedNode.id} isFixed set to ${clickedNode.isFixed}`); redrawCanvas(); }
}
function handleMouseLeave(e) {
    if (!canvas) return; if (draggingNode) { draggingNode = null; }
    if (drawingEdgeFrom) { drawingEdgeFrom = null; redrawCanvas(); }
    canvas.style.cursor = 'default'; isMouseDown = false;
}
function getCursorForMode(mode, hoveredNode = null, hoveredEdge = null) {
    switch (mode) {
        case 'force': if (draggingNode) return 'grabbing'; if (hoveredNode) return hoveredNode.isFixed ? 'not-allowed' : 'grab'; return 'default';
        case 'draw': if (drawingEdgeFrom) return 'crosshair'; return hoveredNode ? 'pointer' : 'crosshair';
        case 'delete': return (hoveredNode || hoveredEdge) ? 'pointer' : 'default';
        case 'edit': const canEditEdge = hoveredEdge && isWeighted; return (hoveredNode || canEditEdge) ? 'pointer' : 'default';
        default: return 'default';
    }
}

// --- Trực quan hóa Thuật toán ---
export function highlightElements(highlightData) {
    console.log("JS: Highlighting elements:", JSON.stringify(highlightData));
    highlightedNodes.clear(); highlightedEdges.clear(); nodeLabels = {};
    if (highlightData) {
        if (Array.isArray(highlightData.nodes)) { highlightData.nodes.forEach(id => { if (typeof id === 'number' || typeof id === 'string') { highlightedNodes.add(id); } else { console.warn("JS: highlightElements - Bỏ qua ID nút không hợp lệ:", id); } }); }
        if (Array.isArray(highlightData.edges)) { highlightData.edges.forEach(edge => { if (edge && (typeof edge.from === 'number' || typeof edge.from === 'string') && (typeof edge.to === 'number' || typeof edge.to === 'string')) { highlightedEdges.add(`${edge.from}-${edge.to}`); } else { console.warn("JS: highlightElements - Bỏ qua cạnh không hợp lệ:", edge); } }); }
        if (highlightData.nodeLabels && typeof highlightData.nodeLabels === 'object') { for (const nodeId in highlightData.nodeLabels) { if (highlightData.nodeLabels.hasOwnProperty(nodeId)) { nodeLabels[nodeId] = highlightData.nodeLabels[nodeId]; } } }
    }
    console.log("JS: Highlight state updated:", { nodes: Array.from(highlightedNodes), edges: Array.from(highlightedEdges), labels: nodeLabels });
    redrawCanvas();
}
export function resetHighlights() {
    console.log("JS: Resetting highlights");
    highlightedNodes.clear(); highlightedEdges.clear(); nodeLabels = {};
    redrawCanvas();
}

// --- ADDED: Helper functions for prompt ---
/**
 * Hiển thị hộp thoại prompt để lấy nhãn nút mới.
 * @param {string} promptMessage - Thông điệp hiển thị cho người dùng.
 * @param {string} defaultValue - Giá trị mặc định trong ô nhập liệu.
 * @returns {string | null} Giá trị người dùng nhập hoặc null nếu hủy.
 */
export function getNodeLabelInput(promptMessage, defaultValue) {
    try {
        return prompt(promptMessage, defaultValue);
    } catch (e) {
        console.error("Lỗi khi gọi prompt() cho nhãn nút:", e);
        return null; // Trả về null nếu có lỗi
    }
}

