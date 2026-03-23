<script setup>
import { ref, onMounted, computed, inject, onUnmounted } from "vue";
import axios from "axios";
import * as signalR from "@microsoft/signalr";
import { useRouter } from "vue-router";
import { globalState } from "../store";

const swal = inject("$swal");
const router = useRouter();
const backendUrl = "http://localhost:5198";

// --- THÔNG TIN NHÂN VIÊN & PHÂN QUYỀN ---
const tenNhanVien = ref(localStorage.getItem("tenNhanVien") || "Nhân viên");
const userRole = localStorage.getItem("pos36_role") || "ThuNgan";
const isThuNgan =
  userRole === "ThuNgan" || userRole === "Admin" || userRole === "QuanLy";

// --- STATE GIAO DIỆN ---
const activeRightTab = ref("tables");
const tables = ref([]);
const products = ref([]);
const activeTable = ref(null);
const ordersByTable = ref({});
const formatPrice = (price) =>
  new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND" }).format(
    price || 0,
  );

// MẢNG LƯU DANH SÁCH THÔNG BÁO
const notificationList = ref([]);

// --- SIGNALR (REAL-TIME) ---
const connection = new signalR.HubConnectionBuilder()
  .withUrl(`${backendUrl}/kitchenHub`)
  .withAutomaticReconnect()
  .build();

onMounted(async () => {
  try {
    await connection.start();
    console.log("Đã kết nối SignalR!");
  } catch (err) {
    console.error("SignalR Lỗi: ", err);
  }
  // THÊM ĐOẠN NÀY ĐỂ BẮT SÓNG ORDER BÁO THANH TOÁN
  // THÊM VÀO TRONG onMounted CỦA PosView.vue
  connection.on("CoYeuCauThanhToan", (chiNhanhId, tenBan) => {
    if (chiNhanhId === globalState.value.activeBranchId && isThuNgan) {
      // Lưu vào danh sách thông báo
      notificationList.value.unshift({
        id: Date.now(),
        title: `Bàn ${tenBan} yêu cầu thanh toán`,
        time: new Date().toLocaleTimeString("vi-VN"),
      });

      swal.fire({
        position: "top-end",
        icon: "warning",
        title: `🔔 BÀN ${tenBan} YÊU CẦU THANH TOÁN!`,
        toast: true,
        showConfirmButton: false,
        timer: 8000,
        background: "#ffc107",
        color: "#000",
      });
    }
  });
  fetchTables();
  // Lắng nghe sự kiện Bếp báo lại hoặc máy khác gọi món để cập nhật màn hình
  connection.on("CoDonHangMoi", (data) => {
    // Có thể thêm logic popup thông báo ở đây nếu cần
    fetchTables(); // Tải lại danh sách bàn để cập nhật trạng thái mới nhất
  });

  await getBranchIdAndFetch();

  // Cập nhật đồng hồ đếm giờ mỗi phút
  setInterval(() => {
    tables.value = [...tables.value];
  }, 60000);
});

onUnmounted(() => {
  connection.stop();
});
// THÊM HÀM NÀY VÀO DƯỚI CÁC KHAI BÁO BIẾN
const getBranchIdAndFetch = async () => {
  let branchId =
    globalState.value.activeBranchId ||
    localStorage.getItem("pos36_active_branch");

  // Nếu chưa có chi nhánh nào trong RAM (Vừa login xong) -> Tự gọi API lấy chi nhánh
  if (!branchId || branchId === "null") {
    try {
      const res = await axios.get("/api/ChiNhanh");
      if (res.data && res.data.length > 0) {
        branchId = res.data[0].id;
        globalState.value.activeBranchId = branchId;
        localStorage.setItem("pos36_active_branch", branchId);
      }
    } catch (e) {
      console.error("Lỗi lấy chi nhánh", e);
    }
  }

  // Sau khi chắc chắn có branchId rồi mới gọi API lấy bàn và món
  if (branchId) {
    await fetchTables(branchId);
    await fetchProducts(branchId);
  }
};
// --- HÀM FETCH DỮ LIỆU ---
// SỬA LẠI HÀM fetchTables và fetchProducts để nhận tham số
const fetchTables = async (branchId) => {
  try {
    const res = await axios.get(
      `/api/Ban/danh-sach-pos?chiNhanhId=${branchId}`,
    );
    tables.value = res.data;
  } catch (e) {}
};

const fetchProducts = async (branchId) => {
  if (!branchId) return;
  try {
    const res = await axios.get(
      `/api/SanPham/danh-sach?chiNhanhId=${globalState.value.activeBranchId}`,
    );
    products.value = res.data.filter((p) => p.trangThai === true);
  } catch (e) {
    console.error(e);
  }
};

const calculateTimeElapsed = (timeOpenString) => {
  if (!timeOpenString) return "00:00";
  const openTime = new Date(timeOpenString);
  const now = new Date();
  const diffMins = Math.floor((now - openTime) / 60000);
  return `${Math.floor(diffMins / 60)
    .toString()
    .padStart(2, "0")}:${(diffMins % 60).toString().padStart(2, "0")}`;
};

// --- 1. MỞ BÀN VÀ TẢI HÓA ĐƠN ---
const openTable = async (ban) => {
  activeTable.value = ban;
  activeRightTab.value = "menu";

  if (ban.trangThai === "Trống") {
    if (!ordersByTable.value[ban.id]) ordersByTable.value[ban.id] = [];
  } else {
    // GỌI API GET CỦA EM ĐỂ LẤY CHI TIẾT HÓA ĐƠN LÊN GIỎ HÀNG
    try {
      const res = await axios.get(`/api/HoaDon/ban/${ban.id}`);
      const data = res.data;

      ordersByTable.value[ban.id] = data.danhSachMon.map((mon) => ({
        id: mon.sanPhamId, // Id gốc của món ăn
        chiTietId: mon.chiTietId, // Id dòng trong DB
        name: mon.tenSanPham,
        price: mon.donGia,
        qty: mon.soLuong,
        isSent: true, // Đánh dấu là đã gửi Bếp/lưu DB rồi
      }));

      activeTable.value.tamTinh = data.tongTien;
      // Lấy mốc thời gian từ DB tính ngược ra (nếu cần)
    } catch (e) {
      console.error("Lỗi tải hóa đơn cũ:", e);
    }
  }
};

// Tính toán Giỏ hàng hiển thị
const currentOrder = computed(() =>
  activeTable.value ? ordersByTable.value[activeTable.value.id] || [] : [],
);
const totalAmount = computed(() =>
  currentOrder.value.reduce((sum, item) => sum + item.price * item.qty, 0),
);

// --- 2. HÀM GỌI API GỌI MÓN (POST) CHỈ GỬI NHỮNG MÓN MỚI ---
const saveNewOrdersToDatabase = async () => {
  if (!activeTable.value || currentOrder.value.length === 0) return false;

  // Chỉ lọc ra những món CHƯA GỬI (isSent: false)
  const unsentItems = currentOrder.value.filter((item) => !item.isSent);
  if (unsentItems.length === 0) return true; // Tránh gọi API dư thừa nếu không có món mới

  // Gói dữ liệu theo chuẩn TaoDonHangDto
  const payload = {
    banId: activeTable.value.id,
    danhSachMon: unsentItems.map((item) => ({
      sanPhamId: item.id,
      soLuong: item.qty,
      ghiChu: "",
    })),
  };

  try {
    await axios.post("/api/HoaDon/goimon", payload);

    // Sau khi gửi thành công, đánh dấu các món đó là đã gửi
    unsentItems.forEach((item) => (item.isSent = true));
    return true;
  } catch (e) {
    swal.fire("Lỗi", e.response?.data || "Không thể lưu Order", "error");
    return false;
  }
};

// --- 3. THÊM MÓN VÀO GIỎ ---
const addItem = async (prod) => {
  if (!activeTable.value)
    return swal.fire("Chú ý", "Vui lòng chọn bàn trước!", "warning");

  let orderList = ordersByTable.value[activeTable.value.id];

  // Tìm xem món này đã có trong giỏ NHƯNG PHẢI LÀ MÓN CHƯA GỬI không
  const existingUnsentItem = orderList.find(
    (i) => i.id === prod.id && !i.isSent,
  );

  if (existingUnsentItem) {
    existingUnsentItem.qty++;
  } else {
    orderList.push({
      id: prod.id,
      name: prod.tenSanPham,
      price: prod.giaBan,
      qty: 1,
      isSent: false,
    });
  }

  activeTable.value.tamTinh = totalAmount.value;

  // THU NGÂN: Click là lưu thẳng luôn
  if (isThuNgan) {
    const success = await saveNewOrdersToDatabase();
    if (success) {
      if (activeTable.value.trangThai === "Trống") {
        activeTable.value.trangThai = "Đang phục vụ";
        activeTable.value.timeOpen = new Date().toISOString();
      }
      tables.value = [...tables.value]; // Ép Vue vẽ lại
    }
  }
};

// --- 4. BÁO CHẾ BIẾN (ORDER HOẶC THU NGÂN XÁC NHẬN LẠI) ---
const handleBaoCheBien = async () => {
  if (!activeTable.value || currentOrder.value.length === 0) {
    return swal.fire("Trống", "Chưa có món nào để gửi Bếp!", "warning");
  }

  const unsentCount = currentOrder.value.filter((i) => !i.isSent).length;
  if (unsentCount === 0)
    return swal.fire({
      toast: true,
      position: "top-end",
      icon: "info",
      title: "Không có món mới để báo bếp",
      showConfirmButton: false,
      timer: 1500,
    });

  const success = await saveNewOrdersToDatabase();

  if (success) {
    if (activeTable.value.trangThai === "Trống") {
      activeTable.value.trangThai = "Đang phục vụ";
      activeTable.value.timeOpen = new Date().toISOString();
    }
    activeTable.value.tamTinh = totalAmount.value;
    tables.value = [...tables.value];

    if (!isThuNgan) {
      swal.fire({
        toast: true,
        position: "top-end",
        icon: "success",
        title: "Đã gửi bếp",
        timer: 1500,
        showConfirmButton: false,
      });
    } else {
      swal.fire({
        toast: true,
        position: "top-end",
        icon: "info",
        title: "Đã cập nhật đơn",
        timer: 1000,
        showConfirmButton: false,
      });
    }
  }
};

// --- 5. HỦY MÓN (XỬ LÝ Ở GIAO DIỆN TRƯỚC, BÀI TỚI NỐI API HỦY SAU) ---
const handleCancelItem = async (item, index) => {
  // Note: Nếu món đã gửi bếp (item.isSent), thực tế phải gọi API xuống DB để trừ.
  // Hiện tại tạm thời trừ trên giao diện.
  const { value: formValues } = await swal.fire({
    title: "Hủy/trả đồ",
    html: `
      <div class="d-flex justify-content-center align-items-center mb-3">
         <span class="fs-5 fw-bold text-danger me-2">Số lượng hủy:</span>
         <input id="cancel-qty" type="number" class="form-control w-25 text-center fw-bold" value="1" min="1" max="${item.qty}">
      </div>
      <div class="text-start fw-bold mb-2">Lý do:</div>
      <div class="d-flex gap-2 mb-3">
         <input type="radio" class="btn-check" name="reason" id="r1" value="Khách yêu cầu" checked>
         <label class="btn btn-outline-secondary w-50" for="r1">Khách yêu cầu</label>
      </div>
    `,
    showCancelButton: true,
    confirmButtonText: "Đồng ý",
    confirmButtonColor: "#f37021",
    preConfirm: () => {
      const qty = parseInt(document.getElementById("cancel-qty").value);
      if (qty > item.qty) {
        swal.showValidationMessage("Vượt quá SL thực tế!");
        return false;
      }
      return { qty };
    },
  });

  if (formValues) {
    if (formValues.qty >= item.qty)
      ordersByTable.value[activeTable.value.id].splice(index, 1);
    else item.qty -= formValues.qty;

    activeTable.value.tamTinh = totalAmount.value;
    tables.value = [...tables.value];
    swal.fire({
      toast: true,
      position: "top-end",
      icon: "success",
      title: "Đã hủy món",
      showConfirmButton: false,
      timer: 1500,
    });
  }
};

const logout = () => {
  localStorage.clear();
  router.push("/login");
};
// --- 6. THANH TOÁN HÓA ĐƠN ---
const handleThanhToan = async () => {
  if (!activeTable.value || activeTable.value.trangThai === "Trống") {
    return swal.fire("Lỗi", "Bàn chưa có hóa đơn để thanh toán!", "warning");
  }

  swal
    .fire({
      title: `Thu tiền Bàn ${activeTable.value.tenBan}`,
      text: `Tổng số tiền: ${formatPrice(activeTable.value.tamTinh)}`,
      icon: "info",
      showCancelButton: true,
      confirmButtonText: "Đã Thu Tiền",
      confirmButtonColor: "#28a745",
    })
    .then(async (result) => {
      if (result.isConfirmed) {
        try {
          // Gọi API Thanh toán
          await axios.post(`/api/HoaDon/thanhtoan/${activeTable.value.id}`);

          swal.fire({
            toast: true,
            position: "top-end",
            icon: "success",
            title: "Thanh toán hoàn tất!",
            timer: 1500,
            showConfirmButton: false,
          });

          // Trả giao diện về trạng thái Trống
          ordersByTable.value[activeTable.value.id] = [];
          activeTable.value = null;
          activeRightTab.value = "tables";
          fetchTables(); // Load lại sơ đồ bàn (Bàn sẽ đổi về màu xám)
        } catch (e) {
          swal.fire("Lỗi", "Thanh toán thất bại", "error");
        }
      }
    });
};
</script>

<template>
  <div
    class="pos-container d-flex flex-column vh-100 bg-light"
    style="font-family: Arial, sans-serif"
  >
    <div
      class="d-flex justify-content-between align-items-center px-3 py-1"
      style="background-color: #f37021; color: white"
    >
      <div class="d-flex align-items-center">
        <div class="input-group input-group-sm me-3" style="width: 250px">
          <span class="input-group-text bg-white border-0"
            ><i class="bi bi-search text-muted"></i
          ></span>
          <input
            type="text"
            class="form-control border-0"
            placeholder="Tìm thực đơn - F1"
          />
        </div>
        <button class="btn btn-sm btn-dark fw-bold me-2">MANG VỀ</button>
      </div>
      <div class="d-flex align-items-center gap-3">
        <span><i class="bi bi-geo-alt-fill"></i> Nhánh chính</span>
        <span class="fw-bold text-uppercase"
          ><i class="bi bi-person-circle"></i> {{ tenNhanVien }} ({{
            userRole
          }})</span
        >

        <div class="dropdown">
          <button
            class="btn btn-sm btn-dark position-relative rounded-circle"
            data-bs-toggle="dropdown"
          >
            <i class="bi bi-bell"></i>
            <span
              v-if="notificationList.length > 0"
              class="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger"
              style="font-size: 0.6rem"
            >
              {{ notificationList.length }}
            </span>
          </button>
          <ul
            class="dropdown-menu dropdown-menu-end shadow-lg border-0 mt-2"
            style="width: 320px; max-height: 400px; overflow-y: auto"
          >
            <li class="dropdown-header fw-bold bg-light">THÔNG BÁO TỪ ORDER</li>
            <li
              v-if="notificationList.length === 0"
              class="dropdown-item text-muted text-center py-3"
            >
              Không có thông báo nào
            </li>
            <li
              v-for="notif in notificationList"
              :key="notif.id"
              class="dropdown-item border-bottom text-wrap py-2"
            >
              <div class="fw-bold text-danger">
                <i class="bi bi-exclamation-circle-fill me-1"></i>
                {{ notif.title }}
              </div>
              <div class="small text-muted text-end mt-1">{{ notif.time }}</div>
            </li>
          </ul>
        </div>

        <button @click="logout" class="btn btn-sm btn-danger rounded-circle">
          <i class="bi bi-power"></i>
        </button>
      </div>
    </div>

    <div class="row g-0 flex-grow-1 overflow-hidden">
      <div
        class="col-lg-5 d-flex flex-column border-end bg-white h-100 position-relative"
      >
        <div
          class="bg-light border-bottom p-2 d-flex justify-content-between align-items-center"
        >
          <div class="d-flex align-items-center gap-2">
            <span class="badge bg-secondary fs-6">{{
              activeTable ? activeTable.tenBan : "Chưa chọn bàn"
            }}</span>
            <span
              v-if="activeTable && activeTable.trangThai === 'Đang phục vụ'"
              class="text-danger small fw-bold"
              ><i class="bi bi-clock"></i>
              {{ calculateTimeElapsed(activeTable.timeOpen) }}</span
            >
          </div>
          <div class="d-flex text-muted small gap-3">
            <span><i class="bi bi-person"></i> 0</span
            ><span><i class="bi bi-journal-text"></i> Ghi chú</span>
          </div>
        </div>

        <div class="d-flex text-muted small fw-bold border-bottom p-2 bg-light">
          <div style="width: 5%">#</div>
          <div style="width: 45%">TÊN HÀNG HÓA</div>
          <div style="width: 15%" class="text-center">SL</div>
          <div style="width: 15%" class="text-end">ĐƠN GIÁ</div>
          <div style="width: 20%" class="text-end">THÀNH TIỀN</div>
        </div>

        <div class="flex-grow-1 overflow-auto p-0">
          <div
            v-if="currentOrder.length === 0"
            class="h-100 d-flex flex-column align-items-center justify-content-center text-muted text-center p-4"
          >
            <i class="bi bi-cart3 fs-1 mb-2 opacity-50"></i>
            <p class="fst-italic small">
              Chưa có mặt hàng nào.<br />Double-click vào bàn và chọn món.
            </p>
          </div>
          <div
            v-else
            v-for="(item, index) in currentOrder"
            :key="index"
            class="d-flex align-items-center p-2 border-bottom"
          >
            <div style="width: 5%" class="text-muted small">
              {{ index + 1 }}
            </div>
            <div style="width: 45%" class="fw-bold text-dark text-truncate">
              {{ item.name }}
            </div>
            <div
              style="width: 15%"
              class="d-flex justify-content-center align-items-center"
            >
              <span class="fw-bold px-2 py-1 bg-light border rounded">{{
                item.qty
              }}</span>
            </div>
            <div style="width: 15%" class="text-end text-muted small">
              {{ item.price.toLocaleString() }}
            </div>
            <div
              style="width: 20%"
              class="text-end d-flex justify-content-end align-items-center gap-2"
            >
              <span class="fw-bold">{{
                (item.price * item.qty).toLocaleString()
              }}</span>
              <i
                @click="handleCancelItem(item, index)"
                class="bi bi-trash text-danger"
                style="cursor: pointer"
              ></i>
            </div>
          </div>
        </div>

        <div class="bg-light border-top">
          <div
            class="p-2 d-flex justify-content-between align-items-center border-bottom"
          >
            <div class="text-muted small">
              <i class="bi bi-search"></i> Tìm khách hàng (F4)
            </div>
            <div class="text-center">
              <div class="small text-muted">Tổng cộng</div>
              <h5 class="fw-bold text-danger mb-0">
                {{ totalAmount.toLocaleString("vi-VN") }}
              </h5>
            </div>
          </div>

          <div class="row g-1 p-1">
            <div class="col-6">
              <button class="btn btn-dark w-100 rounded-0 py-2 small">
                <i class="bi bi-arrow-left-right"></i> Chuyển bàn
              </button>
            </div>
            <div class="col-6">
              <button
                @click="handleBaoCheBien"
                class="btn btn-dark w-100 rounded-0 py-2 small"
              >
                <i class="bi bi-bell"></i> Báo chế biến [F9]
              </button>
            </div>
            <div class="col-6">
              <button class="btn btn-dark w-100 rounded-0 py-2 small">
                <i class="bi bi-subtract"></i> Tách bàn
              </button>
            </div>
            <div class="col-6">
              <button class="btn btn-dark w-100 rounded-0 py-2 small">
                <i class="bi bi-printer"></i> Tạm tính [F3]
              </button>
            </div>
            <div class="col-12 mt-1">
              <button
                @click="handleThanhToan"
                class="btn btn-success w-100 rounded-0 py-2 fw-bold fs-5 text-uppercase"
              >
                <i class="bi bi-cash-coin me-2"></i>Thanh toán [F4]
              </button>
            </div>
          </div>
        </div>
      </div>

      <div class="col-lg-7 d-flex flex-column h-100 bg-light">
        <div class="d-flex p-1 gap-1" style="background-color: #3f51b5">
          <button
            @click="activeRightTab = 'tables'"
            class="btn w-50 fw-bold rounded-0 py-2 text-white"
            :class="
              activeRightTab === 'tables'
                ? 'bg-primary border'
                : 'bg-transparent opacity-75'
            "
          >
            <i class="bi bi-grid-3x3-gap"></i> PHÒNG / BÀN
          </button>
          <button
            @click="activeRightTab = 'menu'"
            class="btn w-50 fw-bold rounded-0 py-2 text-white"
            :class="
              activeRightTab === 'menu'
                ? 'bg-primary border'
                : 'bg-transparent opacity-75'
            "
          >
            <i class="bi bi-cup-hot"></i> THỰC ĐƠN
          </button>
        </div>

        <div
          v-if="activeRightTab === 'tables'"
          class="flex-grow-1 p-3 overflow-auto"
        >
          <div class="row g-2">
            <div
              v-for="ban in tables"
              :key="ban.id"
              class="col-xl-2 col-lg-3 col-md-4 col-4"
            >
              <div
                @dblclick="openTable(ban)"
                class="card border-0 text-white cursor-pointer h-100 shadow-sm"
                :class="
                  ban.trangThai === 'Trống' ? 'bg-secondary' : 'bg-primary'
                "
              >
                <div
                  class="card-body p-2 d-flex flex-column justify-content-center align-items-center"
                >
                  <h6 class="fw-bold mb-1">{{ ban.tenBan }}</h6>
                  <div
                    v-if="ban.trangThai !== 'Trống'"
                    class="text-center w-100 mt-1 border-top pt-1 border-light border-opacity-25"
                  >
                    <small class="d-block fw-bold text-warning"
                      >{{
                        ban.tamTinh ? ban.tamTinh.toLocaleString("vi-VN") : "0"
                      }}đ</small
                    >
                    <small class="d-block" style="font-size: 0.75rem"
                      ><i class="bi bi-clock me-1"></i
                      >{{ calculateTimeElapsed(ban.timeOpen) }}</small
                    >
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div
          v-if="activeRightTab === 'menu'"
          class="flex-grow-1 p-2 overflow-auto bg-white"
        >
          <div class="row g-2">
            <div
              v-for="prod in products"
              :key="prod.id"
              class="col-xl-3 col-lg-4 col-md-4 col-sm-6"
            >
              <div
                @click="addItem(prod)"
                class="card h-100 border-0 cursor-pointer position-relative rounded-2 shadow-sm overflow-hidden product-card"
              >
                <div class="bg-light" style="height: 100px; width: 100%">
                  <img
                    v-if="prod.hinhAnh"
                    :src="backendUrl + prod.hinhAnh"
                    class="w-100 h-100"
                    style="object-fit: cover"
                  />
                  <div
                    v-else
                    class="w-100 h-100 d-flex align-items-center justify-content-center"
                  >
                    <i class="bi bi-image text-muted fs-3"></i>
                  </div>
                </div>
                <span
                  class="position-absolute top-0 end-0 badge bg-danger m-1 shadow-sm"
                  >{{ prod.giaBan.toLocaleString("vi-VN") }}</span
                >
                <div class="card-body p-2 text-center bg-white border-top">
                  <span class="fw-bold text-dark small text-truncate d-block">{{
                    prod.tenSanPham
                  }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.pos-container {
  font-size: 14px;
}
.cursor-pointer {
  cursor: pointer;
}
.product-card:active {
  transform: scale(0.96);
  transition: 0.1s;
}
</style>
