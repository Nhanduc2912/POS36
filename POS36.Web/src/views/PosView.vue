<script setup>
import { ref, onMounted, computed, inject, onUnmounted } from "vue";
import axios from "axios";
import * as signalR from "@microsoft/signalr";
import { useRouter } from "vue-router";
import { globalState } from "../store";
import { printReceipt } from "../utils/printer";

// Danh sách các bàn đang treo chờ khách quét QR
const pendingPayments = ref([]);
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

  connection.on("CoYeuCauThanhToan", (chiNhanhId, tenBan) => {
    if (chiNhanhId === globalState.value.activeBranchId && isThuNgan) {
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

  fetchTables(globalState.value.activeBranchId);

  connection.on("CoDonHangMoi", (data) => {
    fetchTables(globalState.value.activeBranchId);
  });

  await getBranchIdAndFetch();

  setInterval(() => {
    tables.value = [...tables.value];
  }, 60000);
});

onUnmounted(() => {
  connection.stop();
});

const getBranchIdAndFetch = async () => {
  try {
    // 1. Kéo BankConfig (Đã có)
    const resConfig = await axios.get("/api/ThietLap/BankConfig");
    if (resConfig.data && resConfig.data.duLieu) {
      localStorage.setItem("pos36_bank_config", resConfig.data.duLieu);
    }

    // 2. THÊM ĐOẠN NÀY ĐỂ KÉO MẪU IN MỚI NHẤT VỀ MÁY THU NGÂN
    const resPrint = await axios.get("/api/ThietLap/PrintTemplate");
    if (resPrint.data && resPrint.data.duLieu) {
      localStorage.setItem("pos36_print_template", resPrint.data.duLieu);
    }
  } catch (e) {
    console.log("Lỗi tải cấu hình ngầm");
  }

  let branchId =
    globalState.value.activeBranchId ||
    localStorage.getItem("pos36_active_branch");

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

  if (branchId) {
    await fetchTables(branchId);
    await fetchProducts(branchId);
  }
};

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

const openTable = async (ban) => {
  activeTable.value = ban;
  activeRightTab.value = "menu";

  if (ban.trangThai === "Trống") {
    if (!ordersByTable.value[ban.id]) ordersByTable.value[ban.id] = [];
  } else {
    try {
      const res = await axios.get(`/api/HoaDon/ban/${ban.id}`);
      const data = res.data;

      ordersByTable.value[ban.id] = data.danhSachMon.map((mon) => ({
        id: mon.sanPhamId,
        chiTietId: mon.chiTietId,
        name: mon.tenSanPham,
        price: mon.donGia,
        qty: mon.soLuong,
        isSent: true,
      }));

      activeTable.value.tamTinh = data.tongTien;
    } catch (e) {
      console.error("Lỗi tải hóa đơn cũ:", e);
    }
  }
};

const currentOrder = computed(() =>
  activeTable.value ? ordersByTable.value[activeTable.value.id] || [] : [],
);
const totalAmount = computed(() =>
  currentOrder.value.reduce((sum, item) => sum + item.price * item.qty, 0),
);

const saveNewOrdersToDatabase = async () => {
  if (!activeTable.value || currentOrder.value.length === 0) return false;
  const unsentItems = currentOrder.value.filter((item) => !item.isSent);
  if (unsentItems.length === 0) return true;

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
    unsentItems.forEach((item) => (item.isSent = true));
    return true;
  } catch (e) {
    swal.fire("Lỗi", e.response?.data || "Không thể lưu Order", "error");
    return false;
  }
};

const addItem = async (prod) => {
  if (!activeTable.value)
    return swal.fire("Chú ý", "Vui lòng chọn bàn trước!", "warning");

  let orderList = ordersByTable.value[activeTable.value.id];
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

  if (isThuNgan) {
    const success = await saveNewOrdersToDatabase();
    if (success) {
      if (activeTable.value.trangThai === "Trống") {
        activeTable.value.trangThai = "Đang phục vụ";
        activeTable.value.timeOpen = new Date().toISOString();
      }
      tables.value = [...tables.value];
    }
  }
};

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

const handleCancelItem = async (item, index) => {
  const { value: formValues } = await swal.fire({
    title: "Hủy / Trả đồ",
    html: `
      <div class="d-flex justify-content-center align-items-center mb-3">
         <span class="fs-5 fw-bold text-danger me-2">Số lượng hủy:</span>
         <input id="cancel-qty" type="number" class="form-control w-25 text-center fw-bold" value="1" min="1" max="${item.qty}">
      </div>
      <div class="text-start fw-bold mb-2">Lý do:</div>
      <div class="d-flex gap-2 mb-3">
         <input type="radio" class="btn-check" name="reason" id="r1" value="Khách yêu cầu" checked>
         <label class="btn btn-outline-secondary w-50" for="r1">Khách yêu cầu</label>
         <input type="radio" class="btn-check" name="reason" id="r2" value="Lỗi thao tác">
         <label class="btn btn-outline-secondary w-50" for="r2">Lỗi thao tác</label>
      </div>
    `,
    showCancelButton: true,
    confirmButtonText: "Đồng ý Hủy",
    confirmButtonColor: "#dc3545",
    preConfirm: () => {
      const qty = parseInt(document.getElementById("cancel-qty").value);
      const reason = document.querySelector(
        'input[name="reason"]:checked',
      ).value;
      if (qty > item.qty) {
        swal.showValidationMessage("Vượt quá SL thực tế đang có!");
        return false;
      }
      return { qty, reason };
    },
  });

  if (formValues) {
    try {
      if (item.isSent && item.chiTietId) {
        await axios.post("/api/HoaDon/huymon", {
          chiTietId: item.chiTietId,
          soLuongHuy: formValues.qty,
          lyDo: formValues.reason,
        });
      }

      if (formValues.qty >= item.qty) {
        ordersByTable.value[activeTable.value.id].splice(index, 1);
      } else {
        item.qty -= formValues.qty;
      }

      if (ordersByTable.value[activeTable.value.id].length === 0) {
        activeTable.value.trangThai = "Trống";
        activeTable.value.timeOpen = null;
        activeRightTab.value = "tables";
      }

      activeTable.value.tamTinh = totalAmount.value;
      tables.value = [...tables.value];
      fetchTables(globalState.value.activeBranchId);

      swal.fire({
        toast: true,
        position: "top-end",
        icon: "success",
        title: `Đã hủy ${formValues.qty} ${item.name} (${formValues.reason})`,
        showConfirmButton: false,
        timer: 2000,
      });
    } catch (e) {
      swal.fire("Lỗi", "Không thể hủy món trong CSDL!", "error");
    }
  }
};

const logout = () => {
  localStorage.clear();
  router.push("/login");
};

// --- ĐẢO HÀM NÀY LÊN TRÊN ĐỂ KHÔNG BỊ LỖI IS NOT DEFINED ---
const thucHienThanhToanChinhThuc = async (banId) => {
  try {
    await axios.post(`/api/HoaDon/thanhtoan/${banId}`);

    pendingPayments.value = pendingPayments.value.filter(
      (p) => p.banId !== banId,
    );

    swal.fire({
      toast: true,
      position: "top-end",
      icon: "success",
      title: `Bàn ${banId} thanh toán thành công!`,
      timer: 2000,
      showConfirmButton: false,
    });

    fetchTables(globalState.value.activeBranchId);

    if (activeTable.value && activeTable.value.id === banId) {
      ordersByTable.value[banId] = [];
      activeTable.value.trangThai = "Trống";
      activeTable.value.timeOpen = null;
      activeRightTab.value = "tables";
    }
  } catch (e) {
    console.error(e);
    swal.fire("Lỗi", "Thanh toán trên hệ thống thất bại", "error");
  }
};

// --- XỬ LÝ THANH TOÁN ---
const handleThanhToan = async () => {
  if (!activeTable.value || activeTable.value.trangThai === "Trống") return;

  const soTien = activeTable.value.tamTinh;
  const banId = activeTable.value.id;

  swal
    .fire({
      title: `Thanh toán Bàn ${activeTable.value.tenBan}`,
      html: `<h3 class="text-danger fw-bold">${formatPrice(soTien)}</h3>`,
      showDenyButton: true,
      showCancelButton: true,
      confirmButtonText: '<i class="bi bi-cash"></i> Tiền mặt',
      denyButtonText: '<i class="bi bi-qr-code-scan"></i> Chuyển khoản QR',
      cancelButtonText: "Đóng",
      confirmButtonColor: "#28a745",
      denyButtonColor: "#f37021",
    })
    .then(async (result) => {
      // 1. NẾU TRẢ TIỀN MẶT
      if (result.isConfirmed) {
        // In hóa đơn chính thức
        const orderToPrint = {
          tenBan: activeTable.value.tenBan,
          tongTien: soTien,
          items: currentOrder.value,
        };
        printReceipt(orderToPrint, {
          name: "POS36",
          address: "Đà Nẵng",
          phone: "0905",
        });

        // Chốt đơn CSDL
        thucHienThanhToanChinhThuc(banId);
      }

      // 2. NẾU CHỌN QUÉT QR
      else if (result.isDenied) {
        if (connection.state === "Connected") {
          // Bắn lệnh qua máy nhân viên (Truyền chuỗi rỗng vào tham số thứ 3 để hết lỗi C#)
          await connection.invoke("YeuCauMoQR", banId, soTien, "");

          pendingPayments.value.push({
            banId: banId,
            tenBan: activeTable.value.tenBan,
            soTien: soTien,
          });

          // IN HÓA ĐƠN TẠM TÍNH CHO KHÁCH (Đã mở khóa)
          const orderToPrint = {
            tenBan: activeTable.value.tenBan,
            tongTien: soTien,
            items: currentOrder.value,
          };
          printReceipt(orderToPrint, {
            name: "POS36",
            address: "Đà Nẵng",
            phone: "0905",
          });

          swal.fire({
            toast: true,
            position: "top-end",
            icon: "info",
            title: `Đã in hóa đơn tạm tính & chuyển Bàn ${activeTable.value.tenBan} sang chờ QR`,
            timer: 2000,
            showConfirmButton: false,
          });
          activeRightTab.value = "tables";
        }
      }
    });
};

const huyYeuCauQRGocManHinh = (banId) => {
  pendingPayments.value = pendingPayments.value.filter(
    (p) => p.banId !== banId,
  );
  connection.invoke("HuyMoQR", banId, "Thu ngân đã hủy yêu cầu QR");
};

// Lắng nghe Nhân viên báo Hủy QR
connection.on("NhanHuyMoQR", (banId, lyDo) => {
  pendingPayments.value = pendingPayments.value.filter(
    (p) => p.banId !== banId,
  ); // Gỡ khỏi góc phải nếu nhân viên hủy
  if (activeTable.value && activeTable.value.id === banId) {
    swal.fire("Đã hủy QR", `Lý do: ${lyDo}`, "warning");
  }
});

// Lắng nghe Webhook báo Tiền về thành công
connection.on("ThanhToanQRThanhCong", (banId) => {
  // Lấy lại danh sách món ăn của bàn vừa thanh toán để in hóa đơn chính thức nếu cần
  // (Ở đây vì đã in tạm tính lúc nãy rồi, tùy nghiệp vụ em có muốn in thêm 1 tờ lúc trả xong không thì gọi printReceipt ở đây)
  thucHienThanhToanChinhThuc(banId);
});
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
            <i class="bi bi-cart fs-1 mb-2 opacity-50"></i>
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

  <div class="position-fixed bottom-0 end-0 p-3" style="z-index: 1050">
    <div
      v-for="p in pendingPayments"
      :key="p.banId"
      class="toast show mb-2 shadow-lg border-warning fade-in"
      role="alert"
    >
      <div
        class="toast-header bg-warning bg-opacity-25 text-dark border-bottom-0 pb-1"
      >
        <div
          class="spinner-border spinner-border-sm text-warning me-2"
          role="status"
        ></div>
        <strong class="me-auto">Đang chờ quét QR</strong>
        <button
          type="button"
          class="btn-close"
          @click="huyYeuCauQRGocManHinh(p.banId)"
        ></button>
      </div>
      <div class="toast-body bg-white rounded-bottom px-3 py-2 text-center">
        <h6 class="fw-bold text-dark mb-1">{{ p.tenBan }}</h6>
        <h4 class="fw-bold text-danger mb-0">{{ formatPrice(p.soTien) }}</h4>
        <div class="text-muted font-monospace mt-1" style="font-size: 0.8rem">
          Mã: POS36B{{ p.banId }}
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
