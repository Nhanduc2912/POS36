<script setup>
import { ref, onMounted, computed, inject, onUnmounted, watch } from "vue";
import axios from "axios";
import { useRouter } from "vue-router";
import { globalState } from "../store";
import { printReceipt } from "../utils/printer";
import { useSignalR } from "../composables/useSignalR";

// Danh sách các bàn đang treo chờ khách quét QR
const swal = inject("$swal");
const router = useRouter();
const { connection, connectionStatus, backendUrl, startConnection, stopConnection } = useSignalR();
import AiCopilot from "../components/AiCopilot.vue";
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

// --- TÌM KIẾM & LỌC THỰC ĐƠN ---
const menuSearchText = ref("");
const categories = ref([]);
const activeCategoryId = ref(null); // null = Tất cả

const filteredProducts = computed(() => {
  let list = products.value;
  if (activeCategoryId.value) {
    list = list.filter((p) => p.danhMucId === activeCategoryId.value);
  }
  const keyword = menuSearchText.value.trim().toLowerCase();
  if (keyword) {
    list = list.filter((p) => p.tenSanPham.toLowerCase().includes(keyword));
  }
  return list;
});

const fetchCategories = async () => {
  try {
    const res = await axios.get("/api/DanhMuc");
    categories.value = res.data;
  } catch (e) {
    console.error("Lỗi tải danh mục", e);
  }
};

// --- KHÁCH HÀNG TÍCH ĐIỂM ---
const customerSearchText = ref("");
const customerResults = ref([]);
const selectedCustomer = ref(null);
let customerSearchTimeout = null;

const searchCustomer = () => {
  clearTimeout(customerSearchTimeout);
  const keyword = customerSearchText.value.trim();
  if (!keyword) {
    customerResults.value = [];
    return;
  }
  customerSearchTimeout = setTimeout(async () => {
    try {
      const res = await axios.get(`/api/KhachHang/tim-kiem?keyword=${encodeURIComponent(keyword)}`);
      customerResults.value = res.data;
    } catch (e) {
      customerResults.value = [];
    }
  }, 400);
};

const selectCustomer = (cust) => {
  selectedCustomer.value = cust;
  customerSearchText.value = "";
  customerResults.value = [];
};

const clearCustomer = () => {
  selectedCustomer.value = null;
  customerSearchText.value = "";
  customerResults.value = [];
};

const handleQuickAddCustomer = async () => {
  const { value: formValues } = await swal.fire({
    title: "Tạo Khách Hàng Mới",
    html: `
      <div class="text-start mb-2 fw-bold text-primary">Thông tin bắt buộc</div>
      <input id="swal-kh-ten" class="form-control mb-2" placeholder="Tên khách hàng *">
      <input id="swal-kh-sdt" class="form-control mb-3" value="${customerSearchText.value}" placeholder="Số điện thoại *">
      <div class="text-start mb-2 fw-bold text-secondary">Thông tin bổ sung</div>
      <input id="swal-kh-email" class="form-control mb-2" type="email" placeholder="Email (Không bắt buộc)">
      <input id="swal-kh-diachi" class="form-control mb-2" placeholder="Địa chỉ (Không bắt buộc)">
      <input id="swal-kh-ngaysinh" class="form-control mb-2" type="date" title="Ngày sinh">
      <textarea id="swal-kh-ghichu" class="form-control" rows="2" placeholder="Ghi chú (Không bắt buộc)"></textarea>
    `,
    showCancelButton: true,
    cancelButtonText: "Hủy",
    confirmButtonText: "Lưu &amp; Ghim",
    preConfirm: () => {
      const ten = document.getElementById("swal-kh-ten").value;
      const sdt = document.getElementById("swal-kh-sdt").value;
      if (!ten || !sdt) {
        swal.showValidationMessage("Nhập Tên và SĐT!");
        return false;
      }
      // Kiểm tra ô Tên không phải số điện thoại
      if (/^[0-9]+$/.test(ten.trim())) {
        swal.showValidationMessage("Ô Tên không được nhập số điện thoại! Hãy nhập họ tên khách hàng.");
        return false;
      }
      // Kiểm tra SĐT hợp lệ (8-11 chữ số)
      if (!/^[0-9]{8,11}$/.test(sdt.trim())) {
        swal.showValidationMessage("Số điện thoại chỉ gồm chữ số, độ dài 8–11 ký tự!");
        return false;
      }
      const email = document.getElementById("swal-kh-email").value;
      const diachi = document.getElementById("swal-kh-diachi").value;
      const ngaysinh = document.getElementById("swal-kh-ngaysinh").value;
      const ghichu = document.getElementById("swal-kh-ghichu").value;
      return {
        tenKhachHang: ten,
        soDienThoai: sdt,
        email: email || null,
        diaChi: diachi || null,
        ngaySinh: ngaysinh || null,
        ghiChu: ghichu || null,
      };
    },
  });
  if (formValues) {
    try {
      const res = await axios.post("/api/KhachHang", formValues);
      selectCustomer(res.data);
      swal.fire({ toast: true, position: "top-end", icon: "success", title: "Đã tạo & ghim khách hàng!", timer: 1500, showConfirmButton: false });
    } catch (e) {
      swal.fire("Lỗi", e.response?.data || "Không thể tạo", "error");
    }
  }
};

// MẢNG LƯU DANH SÁCH THÔNG BÁO
const notificationList = ref([]);
// pendingPayments for QR tracking
const pendingPayments = ref([]);

let refreshTimer = null;

onMounted(async () => {
  try {
    await startConnection();
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

  // Đơn hàng mới (gọi món mới) → refresh danh sách bàn
  connection.on("CoDonHangMoi", (data) => {
    fetchTables(globalState.value.activeBranchId);
  });

  // Cập nhật bàn (chuyển bàn, tách bàn, ghép bàn, thanh toán, hủy món)
  connection.on("CapNhatBan", (data) => {
    fetchTables(globalState.value.activeBranchId);
    // Nếu đang mở bàn nào thì refresh lại order của bàn đó
    if (activeTable.value && activeTable.value.trangThai !== "Trống") {
      openTable(activeTable.value);
    }
  });

  // Lắng nghe Nhân viên báo Hủy QR
  connection.on("NhanHuyMoQR", (banId, lyDo) => {
    pendingPayments.value = pendingPayments.value.filter(
      (p) => p.banId !== banId,
    );
    if (activeTable.value && activeTable.value.id === banId) {
      swal.fire("Đã hủy QR", `Lý do: ${lyDo}`, "warning");
    }
  });

  // Lắng nghe Webhook báo Tiền về thành công
  connection.on("ThanhToanQRThanhCong", (banId) => {
    swal.close();
    // Lấy lại số điểm đã lưu khi bắt đầu chờ QR
    const pending = pendingPayments.value.find((p) => p.banId === banId);
    const diem = pending?.diemSuDung || 0;
    setTimeout(() => {
      thucHienThanhToanChinhThuc(banId, "Chuyển khoản", diem);
    }, 300);
  });

  // Lắng nghe yêu cầu in hóa đơn từ xa từ điện thoại nhân viên
  connection.on("CoYeuCauInBillTuXa", async (chiNhanhId, banId, loaiIn, tongTien) => {
    if (chiNhanhId === globalState.value.activeBranchId && isThuNgan) {
      try {
        const res = await axios.get(`/api/HoaDon/ban/${banId}`);
        if (res.data) {
          const billData = res.data;
          const billItems = billData.danhSachMon.map((mon) => ({
            name: mon.tenSanPham,
            price: mon.donGia,
            qty: mon.soLuong,
          }));
          
          const orderToPrint = {
            tenBan: billData.tenBan,
            tongTien: billData.tongTien,
            items: billItems,
            banId: banId,
            chiNhanhId: chiNhanhId,
            loaiIn: `${loaiIn} (In hộ từ Order)`,
          };
          
          printReceipt(orderToPrint, {
            name: storeInfo.value?.tenCuaHang || "POS36",
            address: storeInfo.value?.diaChi || "Đà Nẵng",
            phone: storeInfo.value?.soDienThoai || "0905",
          });

          swal.fire({
            toast: true,
            position: "top-end",
            icon: "info",
            title: `🔔 Đã in hộ ${loaiIn} cho Bàn ${billData.tenBan}`,
            showConfirmButton: false,
            timer: 3000,
          });
        }
      } catch (e) {
        console.error("Lỗi in hộ từ xa:", e);
      }
    }
  });

  await getBranchIdAndFetch();

  refreshTimer = setInterval(() => {
    tables.value = [...tables.value];
  }, 60000);
});

onUnmounted(() => {
  stopConnection();
  if (refreshTimer) clearInterval(refreshTimer);
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

  await loadSettings();
  await fetchStoreInfo();

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
    await fetchCategories();
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
      `/api/SanPham/danh-sach?chiNhanhId=${branchId}`,
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

  const result = await swal.fire({
    title: `🔔 Báo bếp chế biến`,
    html: `<p class="mb-1">Xác nhận gửi <strong>${unsentCount} món mới</strong> xuống bếp?</p><p class="text-muted small">Bàn: <strong>${activeTable.value.tenBan}</strong></p>`,
    icon: "question",
    showCancelButton: true,
    confirmButtonText: "Gửi bếp ngay",
    cancelButtonText: "Hủy",
    confirmButtonColor: "#f37021",
  });

  if (!result.isConfirmed) return;

  const success = await saveNewOrdersToDatabase();

  if (success) {
    if (activeTable.value.trangThai === "Trống") {
      activeTable.value.trangThai = "Đang phục vụ";
      activeTable.value.timeOpen = new Date().toISOString();
    }
    activeTable.value.tamTinh = totalAmount.value;
    tables.value = [...tables.value];

    swal.fire({
      toast: true,
      position: "top-end",
      icon: "success",
      title: `✅ Đã báo bếp ${unsentCount} món từ ${activeTable.value.tenBan}`,
      timer: 2000,
      showConfirmButton: false,
    });
  }
};

// --- CHUYỂN BÀN ---
const handleChuyenBan = async () => {
  if (!activeTable.value || activeTable.value.trangThai !== "Đang phục vụ") {
    return swal.fire("Chú ý", "Bàn hiện tại chưa có khách để chuyển!", "warning");
  }

  // Lấy các bàn trống còn lại
  const banTrong = tables.value.filter(
    (b) => b.trangThai === "Trống" && b.id !== activeTable.value.id
  );

  if (banTrong.length === 0) {
    return swal.fire("Hết bàn", "Không còn bàn trống nào để chuyển!", "warning");
  }

  const options = banTrong.map((b) => `<option value="${b.id}">${b.tenBan}</option>`).join("");

  const { value: denBanId, isConfirmed } = await swal.fire({
    title: `🔀 Chuyển bàn`,
    html: `
      <p class="mb-2">Từ: <strong>${activeTable.value.tenBan}</strong></p>
      <label class="form-label fw-bold">Chuyển sang bàn:</label>
      <select id="swal-den-ban" class="form-select">${options}</select>
    `,
    icon: "question",
    showCancelButton: true,
    confirmButtonText: "Chuyển bàn",
    cancelButtonText: "Hủy",
    confirmButtonColor: "#3f51b5",
    preConfirm: () => {
      const val = document.getElementById("swal-den-ban").value;
      if (!val) { swal.showValidationMessage("Vui lòng chọn bàn!"); return false; }
      return parseInt(val);
    },
  });

  if (!isConfirmed) return;

  try {
    const res = await axios.post("/api/HoaDon/chuyenban", {
      tuBanId: activeTable.value.id,
      denBanId: denBanId,
    });

    const tenBanMoi = banTrong.find((b) => b.id === denBanId)?.tenBan || "";

    // Cập nhật state local
    const donHienTai = ordersByTable.value[activeTable.value.id] || [];
    ordersByTable.value[denBanId] = donHienTai;
    ordersByTable.value[activeTable.value.id] = [];

    // Refresh danh sách bàn từ server
    await fetchTables(globalState.value.activeBranchId);

    // Chuyển sang bàn mới
    const banMoi = tables.value.find((b) => b.id === denBanId);
    if (banMoi) await openTable(banMoi);

    swal.fire({
      toast: true, position: "top-end", icon: "success",
      title: `✅ Đã chuyển sang ${tenBanMoi}`,
      timer: 2000, showConfirmButton: false,
    });
  } catch (e) {
    swal.fire("Lỗi", e.response?.data || "Không thể chuyển bàn!", "error");
  }
};

// --- TÁCH BÀN ---
const handleTachBan = async () => {
  if (!activeTable.value || activeTable.value.trangThai !== "Đang phục vụ") {
    return swal.fire("Chú ý", "Bàn hiện tại chưa có khách để tách!", "warning");
  }

  const sentItems = currentOrder.value.filter((i) => i.isSent && i.chiTietId);
  if (sentItems.length === 0) {
    return swal.fire("Chú ý", "Chưa có món nào đã được gửi bếp để tách bàn!", "warning");
  }

  const banTrong = tables.value.filter(
    (b) => b.trangThai === "Trống" && b.id !== activeTable.value.id
  );
  if (banTrong.length === 0) {
    return swal.fire("Hết bàn", "Không còn bàn trống nào để tách sang!", "warning");
  }

  const monCheckboxes = sentItems
    .map((i) => `
      <div class="form-check text-start border-bottom py-1">
        <input class="form-check-input tach-check" type="checkbox" value="${i.chiTietId}" id="tach-${i.chiTietId}">
        <label class="form-check-label" for="tach-${i.chiTietId}">
          <strong>${i.name}</strong> <span class="text-muted">x${i.qty}</span>
          <span class="float-end text-danger">${(i.price * i.qty).toLocaleString("vi-VN")}đ</span>
        </label>
      </div>`)
    .join("");

  const optionsBan = banTrong.map((b) => `<option value="${b.id}">${b.tenBan}</option>`).join("");

  const { value: formVal, isConfirmed } = await swal.fire({
    title: `⚡ Tách bàn từ ${activeTable.value.tenBan}`,
    width: 500,
    html: `
      <label class="form-label fw-bold mb-1">Tách sang bàn trống:</label>
      <select id="swal-tach-den-ban" class="form-select mb-3">${optionsBan}</select>
      <label class="form-label fw-bold mb-1">Chọn món cần tách:</label>
      <div style="max-height:220px;overflow-y:auto;border:1px solid #dee2e6;border-radius:6px;padding:4px 8px">
        ${monCheckboxes}
      </div>
    `,
    showCancelButton: true,
    confirmButtonText: "Tách bàn",
    cancelButtonText: "Hủy",
    confirmButtonColor: "#dc3545",
    preConfirm: () => {
      const denBanId = parseInt(document.getElementById("swal-tach-den-ban").value);
      const checked = [...document.querySelectorAll(".tach-check:checked")].map((el) => parseInt(el.value));
      if (!checked.length) { swal.showValidationMessage("Vui lòng chọn ít nhất 1 món!"); return false; }
      return { denBanId, danhSachChiTietId: checked };
    },
  });

  if (!isConfirmed || !formVal) return;

  try {
    await axios.post("/api/HoaDon/tachban", {
      tuBanId: activeTable.value.id,
      denBanId: formVal.denBanId,
      danhSachChiTietId: formVal.danhSachChiTietId,
    });

    const tenBanMoi = banTrong.find((b) => b.id === formVal.denBanId)?.tenBan || "";

    // Refresh lại bàn và đơn hiện tại
    await fetchTables(globalState.value.activeBranchId);
    await openTable(activeTable.value);

    swal.fire({
      toast: true, position: "top-end", icon: "success",
      title: `✅ Đã tách ${formVal.danhSachChiTietId.length} món sang ${tenBanMoi}`,
      timer: 2000, showConfirmButton: false,
    });
  } catch (e) {
    swal.fire("Lỗi", e.response?.data || "Không thể tách bàn!", "error");
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

// --- HÀM THỰC HIỆN THANH TOÁN CHÍNH THỨC ---
const thucHienThanhToanChinhThuc = async (banId, phuongThuc, diemSuDung = 0) => {
  try {
    let url = `/api/HoaDon/thanhtoan/${banId}?phuongThuc=${phuongThuc}&diemSuDung=${diemSuDung}`;
    if (selectedCustomer.value) {
      url += `&khachHangId=${selectedCustomer.value.id}`;
    }
    const res = await axios.post(url);

    pendingPayments.value = pendingPayments.value.filter(
      (p) => p.banId !== banId,
    );

    // Thông báo điểm tích lũy được sau thanh toán
    if (selectedCustomer.value && res.data?.diemCong > 0) {
      setTimeout(() => {
        swal.fire({
          toast: true,
          position: "top-end",
          icon: "success",
          title: `⭐ +${res.data.diemCong} điểm tích lũy cho ${selectedCustomer.value.tenKhachHang}!`,
          timer: 3500,
          showConfirmButton: false,
        });
      }, 500);
    }

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
      activeTable.value = null;
      activeRightTab.value = "tables";
      clearCustomer();
    }
  } catch (e) {
    console.error(e);
    swal.fire("Lỗi", e.response?.data?.message || "Thanh toán trên hệ thống thất bại", "error");
  }
};

// --- XỬ LÝ THANH TOÁN ---
const handleThanhToan = async () => {
  if (!activeTable.value || activeTable.value.trangThai === "Trống") return;

  const soTien = activeTable.value.tamTinh;
  const banId = activeTable.value.id;
  const khachHang = selectedCustomer.value;

  // Biến tạm theo dõi số điểm nhập trong dialog
  let _diemTemp = 0;

  // Xây dựng HTML phần khách hàng & điểm (nếu có ghim khách)
  const diemHtml = khachHang
    ? khachHang.diemHienTai > 0
      ? `<div class="border rounded p-2 mb-3 text-start" style="background:#f0fff4">
          <div class="d-flex justify-content-between align-items-center mb-2">
            <span class="fw-bold small"><i class="bi bi-person-check-fill text-success me-1"></i>${khachHang.tenKhachHang}</span>
            <span class="badge bg-success rounded-pill"><i class="bi bi-star-fill me-1"></i>${khachHang.diemHienTai} điểm</span>
          </div>
          <div class="d-flex align-items-center gap-2">
            <label class="text-muted small text-nowrap mb-0">Dùng điểm (1đ=1.000₫):</label>
            <input id="swal-diem-sd" type="number" class="form-control form-control-sm text-center"
                   value="0" min="0" max="${khachHang.diemHienTai}" style="width:85px">
            <span class="text-danger fw-bold small" id="swal-tien-giam"></span>
          </div>
        </div>`
      : `<div class="border rounded p-2 mb-3 text-start" style="background:#f0fff4">
          <span class="small"><i class="bi bi-person-check-fill text-success me-1"></i>
          <strong>${khachHang.tenKhachHang}</strong> — Chưa có điểm tích lũy</span>
        </div>`
    : "";

  const result = await swal.fire({
    title: `Thanh toán Bàn ${activeTable.value.tenBan}`,
    html: `
      ${diemHtml}
      <h3 class="text-danger fw-bold mb-1" id="swal-so-tien">${formatPrice(soTien)}</h3>
      <p class="text-muted small mb-0" id="swal-so-tien-goc"></p>
    `,
    showDenyButton: true,
    showCancelButton: true,
    confirmButtonText: '<i class="bi bi-cash"></i> Tiền mặt',
    denyButtonText: '<i class="bi bi-qr-code-scan"></i> Chuyển khoản QR',
    cancelButtonText: "Đóng",
    confirmButtonColor: "#28a745",
    denyButtonColor: "#f37021",
    didOpen: () => {
      const input = document.getElementById("swal-diem-sd");
      if (!input) return;
      input.addEventListener("input", () => {
        const diem = Math.max(
          0,
          Math.min(parseInt(input.value) || 0, khachHang?.diemHienTai || 0),
        );
        input.value = diem;
        _diemTemp = diem;
        const tienGiam = Math.min(diem * 1000, soTien);
        const conLai = soTien - tienGiam;
        const elTotal = document.getElementById("swal-so-tien");
        const elGoc = document.getElementById("swal-so-tien-goc");
        const elGiam = document.getElementById("swal-tien-giam");
        if (tienGiam > 0) {
          elTotal.textContent = formatPrice(conLai);
          elGoc.innerHTML = `<s class="text-muted">${formatPrice(soTien)}</s> — Giảm ${formatPrice(tienGiam)}`;
          elGiam.textContent = `−${formatPrice(tienGiam)}`;
        } else {
          elTotal.textContent = formatPrice(soTien);
          elGoc.textContent = "";
          elGiam.textContent = "";
        }
      });
    },
  });

  const diemSuDung = _diemTemp;
  const soTienThucTe = soTien - Math.min(diemSuDung * 1000, soTien);

  // 1. NẾU TRẢ TIỀN MẶT
  if (result.isConfirmed) {
    const orderToPrint = {
      tenBan: activeTable.value.tenBan,
      tongTien: soTienThucTe,
      items: currentOrder.value,
      banId: banId,
      chiNhanhId: globalState.value.activeBranchId || 0,
      loaiIn: "Thanh toán (Tiền mặt)"
    };
    printReceipt(orderToPrint, {
      name: storeInfo.value?.tenCuaHang || "POS36",
      address: storeInfo.value?.diaChi || "Đà Nẵng",
      phone: storeInfo.value?.soDienThoai || "0905",
    });
    thucHienThanhToanChinhThuc(banId, "Tiền mặt", diemSuDung);
  }

  // 2. NẾU CHỌN QUÉT QR (CHUYỂN KHOẢN)
  else if (result.isDenied) {
    if (connection.state === "Connected") {
      await connection.invoke("YeuCauMoQR", banId, soTienThucTe, "");

      pendingPayments.value.push({
        banId: banId,
        tenBan: activeTable.value.tenBan,
        soTien: soTienThucTe,
        diemSuDung: diemSuDung, // Lưu để dùng khi QR thanh toán thành công
      });

      const orderToPrint = {
        tenBan: activeTable.value.tenBan,
        tongTien: soTienThucTe,
        items: currentOrder.value,
        banId: banId,
        chiNhanhId: globalState.value.activeBranchId || 0,
        loaiIn: "Thanh toán (Quét QR)"
      };
      printReceipt(orderToPrint, {
        name: storeInfo.value?.tenCuaHang || "POS36",
        address: storeInfo.value?.diaChi || "Đà Nẵng",
        phone: storeInfo.value?.soDienThoai || "0905",
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
  );
  if (activeTable.value && activeTable.value.id === banId) {
    swal.fire("Đã hủy QR", `Lý do: ${lyDo}`, "warning");
  }
});

// --- BỔ SUNG ĐOẠN CẤU HÌNH VẬN HÀNH CHO THU NGÂN ---

const settings = ref({
  POS_ThuNganInNhieuBill: false,
  POS_ThuNganXemLichSu: true,
});

const storeInfo = ref({ tenCuaHang: "POS36", diaChi: "Đà Nẵng", soDienThoai: "0905" });
const invoiceHistory = ref([]);
const loadingHistory = ref(false);

const loadSettings = async () => {
  try {
    const keys = "POS_ThuNganInNhieuBill,POS_ThuNganXemLichSu";
    const res = await axios.get("/api/ThietLap/batch", { params: { keys } });
    if (res.data) {
      settings.value.POS_ThuNganInNhieuBill = res.data.POS_ThuNganInNhieuBill === "true";
      settings.value.POS_ThuNganXemLichSu = res.data.POS_ThuNganXemLichSu === "true";
    }
  } catch (e) {
    console.error("Lỗi load settings trong PosView", e);
  }
};

const fetchStoreInfo = async () => {
  try {
    const res = await axios.get("/api/ThietLap/store-info");
    if (res.data) {
      storeInfo.value = res.data;
    }
  } catch (e) {
    console.warn("Lỗi lấy thông tin cửa hàng", e);
  }
};

const fetchInvoiceHistory = async () => {
  loadingHistory.value = true;
  try {
    const branchId = globalState.value.activeBranchId;
    const res = await axios.get(`/api/HoaDon/danh-sach-admin?chiNhanhId=${branchId}&status=Đã thanh toán`);
    if (res.data) {
      invoiceHistory.value = res.data.slice(0, 15);
    }
  } catch (e) {
    console.error("Lỗi lấy lịch sử hóa đơn", e);
  } finally {
    loadingHistory.value = false;
  }
};

const reprintInvoice = (invoice) => {
  const billItems = (invoice.chiTiets || []).map((ct) => ({
    name: ct.tenSanPham,
    price: ct.donGia,
    qty: ct.soLuong,
  }));

  const orderToPrint = {
    tenBan: invoice.tenBan,
    tongTien: invoice.tongThanhToan,
    maChungTu: invoice.maChungTu,
    items: billItems,
    chiNhanhId: globalState.value.activeBranchId || 0,
    loaiIn: "In lại hóa đơn (Đã thanh toán)"
  };

  const storeName = storeInfo.value?.tenCuaHang || "POS36";
  const storeAddr = storeInfo.value?.diaChi || "Đà Nẵng";
  const storePhone = storeInfo.value?.soDienThoai || "0905";

  try {
    printReceipt(orderToPrint, {
      name: storeName,
      address: storeAddr,
      phone: storePhone,
    });

    swal.fire({
      toast: true,
      position: "top-end",
      icon: "success",
      title: "Đã gửi lệnh in lại hóa đơn!",
      timer: 1500,
      showConfirmButton: false,
    });
  } catch (err) {
    swal.fire("Lỗi", "Không thể in lại hóa đơn", "error");
  }
};

const handleManualPrint = () => {
  if (!activeTable.value || currentOrder.value.length === 0) return;

  const billItems = currentOrder.value.map((item) => ({
    name: item.name,
    price: item.price,
    qty: item.qty,
  }));

  const orderToPrint = {
    tenBan: activeTable.value.tenBan,
    tongTien: totalAmount.value,
    items: billItems,
    banId: activeTable.value.id,
    chiNhanhId: globalState.value.activeBranchId || 0,
    loaiIn: "Tạm tính (Thu ngân)"
  };

  const storeName = storeInfo.value?.tenCuaHang || "POS36";
  const storeAddr = storeInfo.value?.diaChi || "Đà Nẵng";
  const storePhone = storeInfo.value?.soDienThoai || "0905";

  try {
    printReceipt(orderToPrint, {
      name: storeName,
      address: storeAddr,
      phone: storePhone,
    });

    swal.fire({
      toast: true,
      position: "top-end",
      icon: "success",
      title: "Đã gửi lệnh in hóa đơn!",
      timer: 1500,
      showConfirmButton: false,
    });
  } catch (err) {
    swal.fire("Lỗi", "Không thể in hóa đơn", "error");
  }
};

// Watch activeRightTab
watch(activeRightTab, (newTab) => {
  if (newTab === "history") {
    fetchInvoiceHistory();
  }
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
            v-model="menuSearchText"
            @focus="activeRightTab = 'menu'"
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

        <!-- THANH TÌM KIẾM KHÁCH HÀNG TÍCH ĐIỂM -->
        <div class="border-bottom px-2 py-1" style="background: #fffbea">
          <div v-if="!selectedCustomer" class="position-relative">
            <div class="input-group input-group-sm">
              <span class="input-group-text bg-white border-end-0">
                <i class="bi bi-person-hearts text-warning"></i>
              </span>
              <input
                v-model="customerSearchText"
                @input="searchCustomer"
                type="text"
                class="form-control border-start-0"
                placeholder="Tìm khách theo tên hoặc số điện thoại..."
              />
            </div>
            <!-- Dropdown kết quả -->
            <div
              v-if="customerResults.length > 0"
              class="position-absolute w-100 bg-white border shadow-lg rounded-bottom"
              style="z-index: 999; top: 100%"
            >
              <div
                v-for="cust in customerResults"
                :key="cust.id"
                @click="selectCustomer(cust)"
                class="d-flex justify-content-between align-items-center px-3 py-2 border-bottom cursor-pointer hover-bg"
              >
                <div>
                  <span class="fw-bold">{{ cust.tenKhachHang }}</span>
                  <span class="text-muted ms-2 small">{{ cust.soDienThoai }}</span>
                </div>
                <span class="badge bg-success rounded-pill">
                  <i class="bi bi-star-fill me-1"></i>{{ cust.diemHienTai }} điểm
                </span>
              </div>
            </div>
            <!-- Nút tạo nhanh khi không tìm thấy -->
            <div
              v-if="customerSearchText.length >= 3 && customerResults.length === 0"
              class="position-absolute w-100 bg-white border shadow-lg rounded-bottom px-3 py-2 text-center"
              style="z-index: 999; top: 100%"
            >
              <span class="text-muted small">Không tìm thấy — </span>
              <a href="#" @click.prevent="handleQuickAddCustomer" class="fw-bold text-primary">
                <i class="bi bi-plus-circle me-1"></i>Tạo khách hàng mới
              </a>
            </div>
          </div>
          <!-- Đã ghim khách hàng -->
          <div v-else class="d-flex justify-content-between align-items-center">
            <div>
              <i class="bi bi-person-check-fill text-success me-1"></i>
              <span class="fw-bold">{{ selectedCustomer.tenKhachHang }}</span>
              <span class="text-muted small ms-1">{{ selectedCustomer.soDienThoai }}</span>
              <span class="badge bg-success ms-2 rounded-pill">
                <i class="bi bi-star-fill me-1"></i>{{ selectedCustomer.diemHienTai }} điểm
              </span>
            </div>
            <button @click="clearCustomer" class="btn btn-sm btn-outline-danger rounded-pill px-2 py-0">
              <i class="bi bi-x-lg"></i> Bỏ ghim
            </button>
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
            <div v-if="selectedCustomer" class="small">
              <i class="bi bi-person-check-fill text-success me-1"></i>
              <span class="fw-bold">{{ selectedCustomer.tenKhachHang }}</span>
            </div>
            <div v-else class="text-muted small">
              <i class="bi bi-person"></i> Khách vãng lai
            </div>
            <div class="text-center">
              <div class="small text-muted">Tổng cộng</div>
              <h5 class="fw-bold text-danger mb-0">
                {{ totalAmount.toLocaleString("vi-VN") }}
              </h5>
            </div>
          </div>

          <div class="row g-1 p-1">
            <!-- Dòng 1: Báo chế biến (Quan trọng nhất) -->
            <div class="col-12">
              <button
                @click="handleBaoCheBien"
                :disabled="!activeTable || currentOrder.length === 0"
                class="btn w-100 rounded-0 py-2 fw-bold"
                :class="currentOrder.filter(i => !i.isSent).length > 0 ? 'btn-warning text-dark' : 'btn-dark'"
              >
                <i class="bi bi-bell-fill me-1"></i> Báo chế biến
                <span v-if="currentOrder.filter(i => !i.isSent).length > 0" class="badge bg-danger ms-1">
                  {{ currentOrder.filter(i => !i.isSent).length }}
                </span>
              </button>
            </div>
            <!-- Dòng 2: Chuyển bàn & Tách bàn -->
            <div class="col-6">
              <button
                @click="handleChuyenBan"
                :disabled="!activeTable || activeTable.trangThai !== 'Đang phục vụ'"
                class="btn btn-dark w-100 rounded-0 py-2 small"
              >
                <i class="bi bi-arrow-left-right me-1"></i> Chuyển bàn
              </button>
            </div>
            <div class="col-6">
              <button
                @click="handleTachBan"
                :disabled="!activeTable || activeTable.trangThai !== 'Đang phục vụ'"
                class="btn btn-dark w-100 rounded-0 py-2 small"
              >
                <i class="bi bi-subtract me-1"></i> Tách bàn
              </button>
            </div>
            <div class="col-12 mt-1 d-flex gap-1">
              <button
                v-if="settings.POS_ThuNganInNhieuBill"
                @click="handleManualPrint"
                :disabled="!activeTable || currentOrder.length === 0"
                class="btn btn-outline-primary fw-bold"
                style="width: 25%;"
                title="In hóa đơn tạm tính"
              >
                <i class="bi bi-printer-fill fs-5"></i>
              </button>
              <button
                @click="handleThanhToan"
                class="btn btn-success fw-bold fs-5 text-uppercase flex-grow-1"
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
            class="btn fw-bold rounded-0 py-2 text-white flex-grow-1"
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
            class="btn fw-bold rounded-0 py-2 text-white flex-grow-1"
            :class="
              activeRightTab === 'menu'
                ? 'bg-primary border'
                : 'bg-transparent opacity-75'
            "
          >
            <i class="bi bi-cup-hot"></i> THỰC ĐƠN
          </button>
          <button
            v-if="settings.POS_ThuNganXemLichSu"
            @click="activeRightTab = 'history'"
            class="btn fw-bold rounded-0 py-2 text-white flex-grow-1"
            :class="
              activeRightTab === 'history'
                ? 'bg-primary border'
                : 'bg-transparent opacity-75'
            "
          >
            <i class="bi bi-clock-history"></i> LỊCH SỬ
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
                class="card border-0 text-white cursor-pointer shadow-sm pos-table-card"
                :class="
                  ban.trangThai === 'Trống' ? 'bg-secondary' : 'bg-primary'
                "
              >
                <div
                  class="card-body p-2 d-flex flex-column justify-content-between align-items-center h-100 w-100"
                >
                  <div class="d-flex align-items-center justify-content-center flex-grow-1">
                    <h6 class="fw-bold mb-0 text-center">{{ ban.tenBan }}</h6>
                  </div>
                  <div class="w-100 text-center" style="min-height: 38px;">
                    <div
                      v-if="ban.trangThai !== 'Trống'"
                      class="border-top pt-1 border-light border-opacity-25"
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
        </div>

        <div
          v-if="activeRightTab === 'history'"
          class="flex-grow-1 p-3 overflow-auto bg-white"
        >
          <div v-if="loadingHistory" class="text-center py-5">
            <div class="spinner-border text-primary" role="status"></div>
            <div class="mt-2 text-muted small">Đang tải lịch sử giao dịch...</div>
          </div>
          <div v-else>
            <div class="d-flex justify-content-between align-items-center mb-3 pb-2 border-bottom">
              <h6 class="fw-bold text-dark mb-0 d-flex align-items-center gap-2">
                <i class="bi bi-clock-history text-primary fs-5"></i> HÓA ĐƠN ĐÃ THANH TOÁN (GẦN NHẤT)
              </h6>
              <button @click="fetchInvoiceHistory" class="btn btn-sm btn-outline-secondary rounded-pill">
                <i class="bi bi-arrow-clockwise"></i> Tải lại
              </button>
            </div>
            
            <div v-if="invoiceHistory.length === 0" class="text-center text-muted py-5">
              <i class="bi bi-cash-stack display-3 opacity-25"></i>
              <p class="mt-2 small">Chưa có giao dịch thanh toán nào trong ca làm việc.</p>
            </div>

            <div v-else class="list-group gap-2 p-1">
              <div
                v-for="invoice in invoiceHistory"
                :key="invoice.id"
                class="card border-0 shadow-sm rounded-4 overflow-hidden invoice-history-card position-relative mb-2"
                style="background: #fafafa; border-left: 5px solid #28a745 !important;"
              >
                <!-- Card Header -->
                <div class="card-header bg-white border-0 pt-3 pb-2 px-3 d-flex justify-content-between align-items-center">
                  <div>
                    <span class="badge bg-secondary bg-opacity-10 text-secondary font-monospace fw-bold mb-1" style="font-size:0.75rem">
                      <i class="bi bi-receipt me-1"></i>{{ invoice.maChungTu }}
                    </span>
                    <h5 class="fw-bold text-dark mb-0 mt-1 d-flex align-items-center gap-2">
                      <i class="bi bi-circle-fill text-success" style="font-size: 0.55rem"></i>
                      {{ invoice.tenBan }}
                    </h5>
                  </div>
                  <div class="text-end">
                    <span class="text-muted small d-block" style="font-size:0.75rem">
                      <i class="bi bi-clock-history me-1"></i> {{ new Date(invoice.ngayBan).toLocaleTimeString("vi-VN", { hour: '2-digit', minute: '2-digit' }) }}
                    </span>
                    <span class="fw-bold text-success fs-5 d-block mt-1">
                      {{ formatPrice(invoice.tongThanhToan) }}
                    </span>
                  </div>
                </div>

                <!-- Items Breakdown -->
                <div class="card-body pt-1 pb-3 px-3">
                  <div class="p-2 rounded-3 mb-3 bg-white" style="border: 1px dashed #e2e8f0;">
                    <div v-for="(ct, idx) in invoice.chiTiets" :key="idx" class="d-flex justify-content-between align-items-center py-1 border-bottom border-light" style="font-size: 0.85rem">
                      <span class="text-secondary fw-semibold">{{ ct.tenSanPham }}</span>
                      <div class="d-flex align-items-center gap-4">
                        <span class="text-muted font-monospace">x{{ ct.soLuong }}</span>
                        <span class="fw-bold text-dark font-monospace" style="width: 80px; text-align: right;">{{ formatPrice(ct.ThanhTien) }}</span>
                      </div>
                    </div>
                  </div>
                  
                  <div class="d-flex justify-content-between align-items-center">
                    <span class="small text-muted"><i class="bi bi-person-fill text-muted me-1"></i>{{ invoice.KhachHang || 'Khách lẻ' }}</span>
                    <button
                      @click="reprintInvoice(invoice)"
                      class="btn btn-sm btn-warning text-dark fw-bold rounded-pill px-3 shadow-sm d-flex align-items-center gap-1 hover-reprint-btn"
                    >
                      <i class="bi bi-printer-fill fs-7"></i> IN LẠI HÓA ĐƠN
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div
          v-if="activeRightTab === 'menu'"
          class="flex-grow-1 d-flex flex-column bg-white h-100 overflow-hidden"
        >
          <!-- THANH TÌM KIẾM + LỌC NHÓM -->
          <div class="border-bottom px-2 pt-2 pb-1 bg-light">
            <div class="input-group input-group-sm mb-2">
              <span class="input-group-text bg-white"><i class="bi bi-search text-muted"></i></span>
              <input
                v-model="menuSearchText"
                type="text"
                class="form-control"
                placeholder="Tìm món ăn, thức uống..."
              />
              <button v-if="menuSearchText" @click="menuSearchText = ''" class="btn btn-outline-secondary" type="button">
                <i class="bi bi-x-lg"></i>
              </button>
            </div>
            <div class="d-flex gap-1 overflow-auto pb-1 flex-nowrap category-pills">
              <button
                @click="activeCategoryId = null"
                class="btn btn-sm rounded-pill px-3 flex-shrink-0"
                :class="activeCategoryId === null ? 'btn-primary' : 'btn-outline-secondary'"
              >
                Tất cả <span class="badge bg-white text-dark ms-1">{{ products.length }}</span>
              </button>
              <button
                v-for="cat in categories"
                :key="cat.id"
                @click="activeCategoryId = cat.id"
                class="btn btn-sm rounded-pill px-3 flex-shrink-0"
                :class="activeCategoryId === cat.id ? 'btn-primary' : 'btn-outline-secondary'"
              >
                {{ cat.tenDanhMuc }}
                <span class="badge ms-1" :class="activeCategoryId === cat.id ? 'bg-white text-primary' : 'bg-secondary text-white'">
                  {{ products.filter(p => p.danhMucId === cat.id).length }}
                </span>
              </button>
            </div>
          </div>

          <!-- KẾT QUẢ LỌC -->
          <div class="flex-grow-1 p-2 overflow-auto">
            <div v-if="filteredProducts.length === 0" class="text-center text-muted py-5">
              <i class="bi bi-emoji-frown fs-1 d-block mb-2 opacity-50"></i>
              <p class="small">Không tìm thấy sản phẩm nào{{ menuSearchText ? ' cho "' + menuSearchText + '"' : '' }}</p>
            </div>
            <div class="row g-2">
              <div
                v-for="prod in filteredProducts"
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
                    <span class="text-muted" style="font-size:0.65rem">{{ prod.tenDanhMuc }}</span>
                  </div>
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

  <AiCopilot />
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
.product-card:hover {
  box-shadow: 0 4px 12px rgba(0,0,0,0.15) !important;
  transform: translateY(-1px);
  transition: 0.15s ease;
}
.hover-bg:hover {
  background-color: #f0f7ff;
}
.category-pills::-webkit-scrollbar {
  height: 4px;
}
.category-pills::-webkit-scrollbar-thumb {
  background: #ccc;
  border-radius: 4px;
}
.category-pills::-webkit-scrollbar-track {
  background: transparent;
}
.pos-table-card {
  height: 95px !important;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  transition: transform 0.15s ease, box-shadow 0.15s ease;
  overflow: hidden;
}
.pos-table-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 8px rgba(0,0,0,0.15) !important;
}
.pos-table-card:active {
  transform: scale(0.96);
}
</style>
