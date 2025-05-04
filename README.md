# Cấu trúc dữ liệu và giải thuật 

![Login](https://imgur.com/3CvBaDe.jpg)
- Date : 04/05/2025 <br/>
Trần Quang Huy (B9D54)

Xem code nguồn tại: https://github.com/huychano21/DSAPrj_B9D54_09
<br/><br/>

Project của cấu trúc dữ liệu và giải thuật.


**Khái quát về chương trình mô tả hoạt động của các *cấu trúc dữ liệu* và *giải thuật***
- Mô tả: Trong quá trình học môn Cấu trúc dữ liệu và giải thuật, các cấu trúc dữ liệu cơ bản được sử dụng là stack, queue và list, sử dụng thêm cây, đồ thị. 
Giải thuật cơ bản là các giải thuật tìm kiếm (Bubblesort, Merge Sort,...., tìm kiếm trên cây nhị phân..)


## 1. Cài đặt
 - Để chạy chương trình,  cần cài đặt sẵn Visual Studio
 - Cài đặt .Net8.0 và tối thiểu Radzen Blazor để chạy chương trình.
 - Để chạy file đã được build (đóng gói), truy cập vào solution, sau đó ấn F5 để chạy chương trình. 


## 2. Các thư mục và tệp tin của dự án
 - README.md: tệp chứa thông tin giới thiệu, cách khởi chạy và hướng dẫn sử dụng chương trình.
 - TreeAndGraph.sln file solution để khởi chạy dự án
- File TreeAndGraph: chứa các thư mục cần thiết để chạy project, các component, thư viện đồ họa...

## 3. Hướng dẫn sử dụng và miêu tả cụ thể chức năng của code
### *3.1 Chạy chương trình*

1. Chạy chương trình bằng cách mở file TreeAndGraph.sln trong thư mục TreeAndGraph, click chuột  và chọn Run Project (F5) để khởi chạy chương trình. Giao diện chính sẽ được hiển thị.

![Stack,queue,list](https://imgur.com/oK10GZX.jpg)

2. Giao diện gồm 2 phần chính: Panel bên trái để chọn chức năng cũng như phần bên phải hiển thị nội dung. Người dùng có thể tương tác trực tiếp với 2 phần này. 



### *3.2, Stack, Queue, List*
![Stack,queue,list](https://imgur.com/V2CdkII.jpg)

- Phần header: Hiển thị nội dung, thông tin và nguyên lý hoạt động của stack, queue, list

 - Phần 1. Stack
    + Người dùng nhập số (chỉ thao tác nhập số) và ấn Enter để thêm giá trị mới vào stack
    + Để xóa, vui lòng ấn pop, đỉnh stack sẽ được đẩy ra.
    + Muốn khôi phục trạng thái stack rỗng, ấn reset
 - Tương tự với Queue, list


- Chú ý:
    + Phần nhập vào chỉ khả dụng đối với số, không khả dụng đối với các phần tử ký tự và ký tự đặc biệt
    + Để hiển thị các phần, cần click chuột vào phần đó, không thì các giao diện sẽ ẩn đi  

### *3.3, **Sắp xếp**.*
![Sort](https://imgur.com/Wsks13F.jpg)
 1.  Bước 1: Người dùng có thể lựa chọn sinh ngẫu nhiên hoặc nhập các phần tử vào, sau đó xác nhận và hiển thị
  
 2. Bước 2: Nhập Thuật toán muốn mô phỏng (Có 6 thuật toán)
   
 3. Kết quả mô phỏng sẽ được hiển thị trong bảng danh sách. Nếu giá trị  không hợp lệ  hộp thoại thông báo "Không có kết quả phù hợp". Nếu phù hợp, người dùng có thể chọn mô phỏng bằng bar hoặc mảng.

- Chú ý:
    + Đối với Merge Sort chỉ có thể mô tả bằng mảng.
    + HeapSort biểu diễn bằng cây tìm kiếm nhị phân nên sẽ có một file riêng.

4. Heapsort. 
 ![HeapSort](https://imgur.com/VN0v1aD.jpg)
Heapsort hoạt động trên cây tìm kiếm nhị phân, do đó, người dùng được yêu cầu nhập thủ công các số (Chưa trang bị nhập tự động) và ấn start để bắt đầu.

### *3.4 **Đồ thị**.* 

 - Giao diện đồ thị gồm 2 phần: Phần điều khiển nằm bên trái và phần tương tác nằm bên phải.

![Graph](https://imgur.com/gS8Bcn0.jpg)

Các phần điều khiển đồ thị: 
- Control: Kiểm soát đồ thị có hướng/ Vô hướng, có trọng số/không có trọng số. Đồng thời, có các chức năng kéo thả, chỉnh sửa

- Create: Tạo đồ thị bằng danh sách cạnh, danh sách kề, ma trận kề.

- Algorithms: Các thuật toán của đồ thị: Duyệt DFS, BFS, Dijktra,....
- Config (Đang mở rộng): Chỉnh được độ rộng của node

### *3.5 Cây (Tree) và cây nhị phân tìm kiếm (Binary Search Tree).*
   ![Tree](https://imgur.com/LodyIXA.jpg)
   
- Giao diện Gồm 3 thành phần chính
   + Lý thuyết: Cung cấp thông tin cơ bản về cây cũng như cây nhị phân tìm kiếm

   + Cây tổng quát: chức năng xây dựng, tạo cây ngẫu nhiên hoặc tự động. Phần canvas có thể tương tác trực tiếp
    + Tương tự đối với cây nhị phân tìm kiếm

- Giao diện cây nhị phân tìm kiếm có thêm một trang riêng, xây dựng sẵn thuật toán tìm kiếm, chèn và xóa giá trị. Đồng thời, khi người dùng nhập vào có thể tự tạo cây nhị phân tìm kiếm và loại bỏ các phần tử trùng nhau

![BST](https://imgur.com/gFPnsKx.jpg)

### *3.6 Bảng băm (Hash Table)*

![HashTable](https://imgur.com/SnJcxh3.jpg)

- Giao diện Gồm 3 thành phần chính
   + Phần khởi tạo: Lựa chọn kích thước bảng băm (hoặc reset về 7)

   + Phần thao tác: Người dùng nhập khóa, và giá trị (Khóa là duy nhất), thông qua hàm băm để xác định vị trí trong bảng băm. Người dùng có thể thêm/ Cập nhật nếu khóa đã tồn tại, tìm kiếm theo khóa hoặc xóa.

    + Phần hiển thị: Hiển thị cặp key-value trong bảng băm. Đồng thời, người dùng có thể chọn trực tiếp các phần tử để cập nhật/Xóa.

    ## 4. Phần kết
- Mặc dù chương trình đã hoạt động khá ổn định và thể hiện được thế mạnh trong thực hiện chức năng mô phỏng hoạt động của cấu trúc dữ liệu và giải thuật. Tuy nhiên, trong quá trình chạy không thể tránh gặp phải những sai sót. Mọi ý kiến đóng góp để hoàn thiện chương trình xin gửi về địa chỉ mail: tranhuyak31cvals@gmail.com. 

- Vui lòng không sử dụng vào mục đích thương mại hoặc các mục đích khác khi chưa có sự đồng ý của tác giả

**Xin chân thành cảm ơn!**
 
