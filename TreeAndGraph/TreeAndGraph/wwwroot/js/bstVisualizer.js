// Đảm bảo thư viện vis-network đã được tải trước file này

window.drawBST = (visData, message) => {
    const container = document.getElementById("bst-container");
    const messageContainer = document.getElementById("bst-message");

    if (!container) {
        console.error("BST container element not found!");
        return;
    }
    if (messageContainer) {
        messageContainer.innerText = message || ''; // Hiển thị thông báo
        messageContainer.style.display = message ? 'block' : 'none'; // Ẩn/hiện container thông báo
    }


    if (!visData || !visData.nodes || visData.nodes.length === 0) {
        container.innerHTML = ''; // Xóa cây nếu không có node
        if (messageContainer) messageContainer.innerText = message || 'Cây rỗng.'; // Cập nhật thông báo
        return;
    }

    const nodes = new vis.DataSet(visData.nodes);
    const edges = new vis.DataSet(visData.edges);

    const data = { nodes: nodes, edges: edges };

    const options = {
        layout: {
            hierarchical: {
                enabled: true,
                direction: "UD", // Top-Down
                sortMethod: "directed", // Giữ cấu trúc cây
                levelSeparation: 80,
                nodeSpacing: 120,
                parentCentralization: true,
                shakeTowards: 'roots' // Giúp layout ổn định hơn
            }
        },
        nodes: {
            shape: 'ellipse', // Hình dạng node
            size: 25,       // Kích thước node
            font: {
                size: 14,     // Kích thước chữ
                color: '#ffffff' // Màu chữ mặc định (trên nền màu)
            },
            borderWidth: 1,
            color: {
                border: '#2B7CE9', // Màu viền mặc định
                background: '#97C2FC', // Màu nền mặc định
                highlight: { // Khi hover hoặc select
                    border: '#2B7CE9',
                    background: '#D2E5FF'
                }
            }
        },
        edges: {
            smooth: { // Làm mịn đường nối
                enabled: true,
                type: "cubicBezier",
                forceDirection: "vertical",
                roundness: 0.4
            },
            arrows: { to: { enabled: false } }, // Không hiển thị mũi tên
            color: {
                color: '#848484', // Màu đường nối
                highlight: '#848484'
            }
        },
        physics: {
            enabled: false // Tắt hiệu ứng vật lý
        },
        interaction: {
            dragNodes: false, // Không cho kéo node
            dragView: true,   // Cho phép kéo cả khung nhìn
            zoomView: true    // Cho phép zoom
        }
    };

    // Tạo và vẽ network
    new vis.Network(container, data, options);
};

// Hàm xóa cây (để gọi từ Reset)
window.clearBST = () => {
    const container = document.getElementById("bst-container");
    const messageContainer = document.getElementById("bst-message");
    if (container) {
        container.innerHTML = '';
    }
    if (messageContainer) {
        messageContainer.innerText = '';
        messageContainer.style.display = 'none';
    }
};