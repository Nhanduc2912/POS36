<template>
  <div
    class="order-screen container-fluid p-0 d-flex flex-column bg-light mobile-first-layout"
  >
    <nav
      class="navbar navbar-dark bg-info text-white shadow-sm px-3 sticky-top"
    >
      <div class="d-flex align-items-center gap-2">
        <i class="bi bi-tablet-landscape fs-4"></i>
        <h6 class="mb-0 fw-bold text-truncate">Order - Bàn</h6>
      </div>
      <div class="d-flex align-items-center gap-3">
        <span class="fw-bold small"
          ><i class="bi bi-person-circle"></i> {{ tenNhanVien }}</span
        >

        <div
          class="position-relative cursor-pointer"
          @click="toggleNotifications"
        >
          <i class="bi bi-bell fs-5"></i>
          <span
            v-if="notifications.length > 0"
            class="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger"
            style="font-size: 0.6rem"
          >
            {{ notifications.length }}
          </span>
        </div>
        <button
          class="btn btn-sm btn-light text-danger fw-bold rounded-circle px-2"
          @click="logout"
          title="Đăng xuất"
        >
          <i class="bi bi-power"></i>
        </button>
      </div>
    </nav>

    <div
      class="flex-grow-1 overflow-auto p-2"
      style="padding-bottom: 70px !important"
    >
      <div v-show="activeTab === 'map'" class="fade-in">
        <div v-if="loadingTables" class="text-center py-5">
          <div class="spinner-border text-info" role="status"></div>
        </div>

        <div v-else>
          <div class="table-grid p-2">
            <div
              v-for="ban in tables"
              :key="ban.id"
              class="table-card position-relative shadow-sm rounded-4 text-center d-flex flex-column justify-content-center p-2 mb-2"
              :class="[
                ban.trangThai === 'Trống'
                  ? 'border border-light bg-white text-dark'
                  : 'bg-warning text-dark border-0 shadow',
                { 'pulse-animation': hasPendingItems(ban.id) },
              ]"
              @click="onTableClick(ban)"
            >
              <i
                class="bi"
                :class="
                  ban.trangThai === 'Trống'
                    ? 'bi-circle text-muted fs-4 mb-1'
                    : 'bi-person-fill text-dark fs-4 mb-1'
                "
              ></i>
              <span class="d-block fw-bold fs-6">{{ ban.tenBan }}</span>

              <div v-if="ban.trangThai !== 'Trống'" class="mt-1">
                <span class="d-block text-danger small fw-bold">{{
                  formatPrice(ban.tamTinh)
                }}</span>
                <span
                  class="d-block text-dark mt-1 fw-bold"
                  style="font-size: 0.75rem"
                >
                  <i class="bi bi-clock"></i>
                  {{ calculateTimeElapsed(ban.timeOpen) }}
                </span>
              </div>

              <span
                v-if="hasPendingItems(ban.id)"
                class="position-absolute top-0 start-100 translate-middle p-2 bg-danger border border-light rounded-circle"
              ></span>
            </div>
          </div>
          <div v-if="tables.length === 0" class="text-center text-muted py-5">
            Chưa có bàn nào
          </div>
        </div>
      </div>

      <div v-show="activeTab === 'notifications'" class="fade-in p-2">
        <h5 class="fw-bold text-secondary mb-3 mt-2 px-2">
          <i class="bi bi-bell-fill text-warning me-2"></i>Thông báo từ Bếp
        </h5>
        <div class="list-group shadow-sm rounded-4">
          <div
            v-for="notif in notificationList"
            :key="notif.id"
            class="list-group-item list-group-item-action p-3 border-light"
          >
            <div
              class="d-flex w-100 justify-content-between align-items-center mb-1"
            >
              <h6 class="mb-0 fw-bold text-info">
                <i class="bi bi-check-circle-fill me-1"></i> Bàn
                {{ notif.tenBan }}
              </h6>
              <small class="text-muted">{{ notif.time }}</small>
            </div>
            <p class="mb-0 text-dark">{{ notif.tenMon }}</p>
          </div>
          <div
            v-if="notificationList.length === 0"
            class="text-center text-muted py-5"
          >
            <i class="bi bi-inbox display-1 text-light"></i>
            <p class="mt-3">Chưa có thông báo nào</p>
          </div>
        </div>
      </div>
    </div>

    <div
      class="bottom-nav bg-white border-top shadow-lg d-flex justify-content-around py-2 fixed-bottom"
    >
      <div
        class="nav-item text-center flex-fill"
        :class="{
          'text-info fw-bold': activeTab === 'map',
          'text-muted': activeTab !== 'map',
        }"
        @click="activeTab = 'map'"
      >
        <i class="bi bi-grid-3x3-gap fs-4 d-block mb-1"></i>
        <span style="font-size: 0.75rem">Sơ Đồ Bàn</span>
      </div>
      <div
        class="nav-item text-center flex-fill position-relative"
        :class="{
          'text-info fw-bold': activeTab === 'notifications',
          'text-muted': activeTab !== 'notifications',
        }"
        @click="toggleNotifications"
      >
        <i class="bi bi-bell fs-4 d-block mb-1"></i>
        <span style="font-size: 0.75rem">Thông báo</span>
      </div>
    </div>

    <div
      class="modal fade"
      id="tableActionModal"
      tabindex="-1"
      ref="tableModalRef"
    >
      <div class="modal-dialog modal-dialog-centered modal-fullscreen-sm-down">
        <div class="modal-content border-0 rounded-top-4">
          <div class="modal-header bg-info bg-opacity-10 border-0 px-4 py-3">
            <h5 class="modal-title fw-bold text-info">
              {{ selectedTable?.tenBan }}
              <span
                class="badge ms-2 fs-7"
                :class="
                  selectedTable?.trangThai === 'Trống'
                    ? 'bg-secondary'
                    : 'bg-warning text-dark'
                "
                >{{ selectedTable?.trangThai }}</span
              >
            </h5>
            <button
              type="button"
              class="btn-close"
              data-bs-dismiss="modal"
            ></button>
          </div>

          <div class="modal-body p-0 bg-light">
            <div
              v-if="selectedTable?.trangThai === 'Trống'"
              class="p-4 text-center"
            >
              <i class="bi bi-door-open display-4 text-muted mb-3"></i>
              <h6 class="text-secondary mb-4">
                Bàn đang trống, khách mới vào?
              </h6>
              <button
                class="btn btn-info text-white w-100 rounded-pill fw-bold p-3 shadow-sm mb-2"
                @click="openMenu"
              >
                <i class="bi bi-cart-plus me-2"></i> MỞ BÀN GỌI MÓN
              </button>
            </div>

            <div v-else class="p-0">
              <div v-if="loadingBill" class="text-center py-5">
                <div
                  class="spinner-border text-warning spinner-border-sm"
                ></div>
              </div>
              <div v-else class="bg-white">
                <div
                  class="p-3 border-bottom bg-warning bg-opacity-10 d-flex justify-content-between align-items-center"
                >
                  <div>
                    <small class="text-muted d-block">Tạm tính</small>
                    <span class="fw-bold text-danger fs-5">{{
                      formatPrice(currentBill?.tongTien || 0)
                    }}</span>
                  </div>
                </div>

                <div
                  class="list-group list-group-flush max-vh-50 overflow-auto"
                >
                  <div
                    v-for="item in currentBill?.danhSachMon"
                    :key="item.chiTietId"
                    class="list-group-item p-3 border-light"
                  >
                    <div
                      class="d-flex justify-content-between align-items-start"
                    >
                      <div>
                        <div class="fw-bold fs-6 text-dark">
                          {{ item.tenSanPham }}
                        </div>
                        <div class="text-muted small">
                          {{ formatPrice(item.donGia) }} x {{ item.soLuong }}
                        </div>
                      </div>
                      <div class="text-end d-flex align-items-center gap-2">
                        <div>
                          <span class="fw-bold text-secondary">{{
                            formatPrice(item.thanhTien)
                          }}</span
                          ><br />
                          <span
                            class="badge"
                            :class="{
                              'bg-secondary':
                                item.trangThaiMon === 'Chờ chế biến',
                              'bg-success': item.trangThaiMon === 'Đã Xong',
                            }"
                            >{{ item.trangThaiMon }}</span
                          >
                        </div>
                        <button
                          class="btn btn-sm btn-outline-danger rounded-circle px-2"
                          @click="handleOrderCancelItem(item)"
                          title="Hủy món"
                        >
                          <i class="bi bi-trash"></i>
                        </button>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div
            class="modal-footer border-0 bg-white d-block"
            v-if="selectedTable?.trangThai !== 'Trống'"
          >
            <div class="row w-100 g-2 mx-0">
              <div class="col-6">
                <button
                  class="btn btn-outline-info w-100 fw-bold rounded-3"
                  @click="openMenu"
                >
                  <i class="bi bi-cart-plus me-1"></i> GỌI THÊM
                </button>
              </div>
              <div class="col-6">
                <button
                  class="btn btn-danger w-100 fw-bold rounded-3"
                  @click="requestPayment"
                >
                  <i class="bi bi-megaphone me-1"></i> BÁO THU NGÂN
                </button>
              </div>
              <div class="col-4">
                <button
                  class="btn btn-outline-dark w-100 rounded-3 small"
                  @click="handleOrderChuyenBan"
                >
                  <i class="bi bi-arrow-left-right"></i> Chuyển bàn
                </button>
              </div>
              <div class="col-4">
                <button
                  class="btn btn-outline-dark w-100 rounded-3 small"
                  @click="handleOrderTachBan"
                >
                  <i class="bi bi-subtract"></i> Tách bàn
                </button>
              </div>
              <div class="col-4">
                <button
                  class="btn btn-outline-warning w-100 rounded-3 small fw-bold"
                  @click="handleOrderBaoCheBien"
                >
                  <i class="bi bi-bell-fill"></i> Báo bếp
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div
      class="offcanvas custom-offcanvas"
      :class="isMobile ? 'offcanvas-bottom' : 'offcanvas-end'"
      tabindex="-1"
      id="menuOffcanvas"
      ref="menuOffcanvasRef"
    >
      <div class="offcanvas-header bg-white border-bottom shadow-sm z-1">
        <h5 class="offcanvas-title fw-bold">
          Chi Tiết Order - {{ selectedTable?.tenBan }}
        </h5>
        <button
          type="button"
          class="btn-close"
          data-bs-dismiss="offcanvas"
        ></button>
      </div>
      <div
        class="offcanvas-body p-0 d-flex flex-column bg-light h-100 overflow-hidden"
      >
        <div class="flex-grow-1 overflow-auto bg-light p-2 position-relative">
          <div
            class="position-sticky top-0 bg-light p-2 z-2 shadow-sm rounded-bottom-4 mb-2"
          >
            <input
              type="text"
              class="form-control rounded-pill text-center border-0 shadow-sm py-2"
              placeholder="🔍 Tìm món ăn..."
              v-model="searchQuery"
            />
          </div>

          <div class="menu-grid">
            <div
              v-for="item in filteredProducts"
              :key="item.id"
              class="card border-0 shadow-sm rounded-4 menu-item-card overflow-hidden"
              @click="addToCart(item)"
            >
              <div class="bg-light" style="height: 80px; width: 100%">
                <img
                  v-if="item.hinhAnh"
                  :src="backendUrl + item.hinhAnh"
                  class="w-100 h-100"
                  style="object-fit: cover"
                />
                <div
                  v-else
                  class="w-100 h-100 d-flex align-items-center justify-content-center"
                >
                  <i class="bi bi-cup-hot text-info fs-3"></i>
                </div>
              </div>
              <div
                class="p-2 text-center d-flex flex-column justify-content-between"
              >
                <h6 class="fw-bold mb-1 lh-sm text-truncate" style="font-size: 0.8rem">
                  {{ item.tenSanPham }}
                </h6>
                <span class="text-success fw-bold fs-7">{{
                  formatPrice(item.giaBan)
                }}</span>
              </div>
            </div>
            <div
              v-if="filteredProducts.length === 0"
              class="text-center w-100 text-muted mt-3"
            >
              Không tìm thấy thực đơn.
            </div>
          </div>
        </div>

        <div
          class="bg-white border-top shadow-lg z-3"
          style="max-height: 40vh; display: flex; flex-direction: column"
        >
          <div
            class="p-2 border-bottom fw-bold d-flex justify-content-between text-secondary bg-light"
            @click="showCartDetail = !showCartDetail"
            style="cursor: pointer"
          >
            <span
              >Giỏ hàng ({{ cart.length }} món)
              <i
                class="bi"
                :class="showCartDetail ? 'bi-chevron-down' : 'bi-chevron-up'"
              ></i
            ></span>
            <span class="text-danger">{{ formatPrice(cartTotal) }}</span>
          </div>

          <div v-show="showCartDetail" class="overflow-auto" style="flex: 1">
            <ul class="list-group list-group-flush">
              <li
                v-for="(cartItem, index) in cart"
                :key="index"
                class="list-group-item p-3 border-light"
              >
                <div
                  class="d-flex justify-content-between align-items-center mb-2"
                >
                  <span class="fw-bold">{{ cartItem.tenSanPham }}</span>
                  <span class="fw-bold text-success">{{
                    formatPrice(cartItem.giaBan * cartItem.soLuong)
                  }}</span>
                </div>
                <div
                  class="d-flex justify-content-between align-items-center gap-2"
                >
                  <div class="input-group input-group-sm w-auto">
                    <button
                      class="btn btn-outline-secondary px-3"
                      @click="decrementQuantity(index)"
                    >
                      -
                    </button>
                    <span class="input-group-text bg-white px-3 fw-bold">{{
                      cartItem.soLuong
                    }}</span>
                    <button
                      class="btn btn-outline-info px-3"
                      @click="incrementQuantity(index)"
                    >
                      +
                    </button>
                  </div>
                </div>
              </li>
            </ul>
          </div>

          <div class="p-3 bg-white border-top">
            <button
              class="btn btn-info text-white w-100 py-3 fw-bold rounded-pill shadow-sm fs-6"
              :disabled="cart.length === 0 || ordering"
              @click="submitOrder"
            >
              <span
                v-if="ordering"
                class="spinner-border spinner-border-sm me-2"
              ></span>
              GỬI BẾP - {{ formatPrice(cartTotal) }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>

  <div
    v-if="showQRModal"
    class="position-fixed top-0 start-0 w-100 h-100 d-flex align-items-center justify-content-center"
    style="background-color: rgba(0, 0, 0, 0.7); z-index: 9999"
  >
    <div
      class="bg-white p-4 rounded-4 shadow-lg text-center mx-3 fade-in"
      style="width: 100%; max-width: 360px"
    >
      <h5 class="fw-bold text-danger mb-2">QUÉT MÃ THANH TOÁN</h5>
      <div class="badge bg-secondary mb-3 fs-6 px-3 py-2 shadow-sm">
        {{ qrData.tenBan }}
      </div>

      <div class="bg-white p-2 rounded-3 border mb-3 shadow-sm">
        <img :src="qrData.url" class="img-fluid w-100" alt="QR Code" />
      </div>

      <h3 class="fw-bold text-primary mb-1">
        {{ formatPrice(qrData.soTien) }}
      </h3>
      <p class="text-muted font-monospace mb-4">
        Nội dung: <span class="fw-bold text-dark">{{ qrData.maChungTu }}</span>
      </p>

      <div class="d-flex gap-2">
        <button
          @click="huyHienThiQR"
          class="btn btn-outline-secondary w-100 fw-bold py-2 rounded-3"
        >
          Hủy bỏ
        </button>
      </div>
    </div>
  </div>

  <AiCopilot />
</template>

<script setup>
import { ref, computed, onMounted, inject, onUnmounted } from "vue";
import { useRouter } from "vue-router";
import axios from "axios";
import * as signalR from "@microsoft/signalr";
import { Modal, Offcanvas } from "bootstrap";
import { globalState } from "../store";
import AiCopilot from "../components/AiCopilot.vue";
const router = useRouter();
const swal = inject("$swal");
const backendUrl = "http://localhost:5098";

const isMobile = ref(window.innerWidth < 768);
const tenNhanVien = ref(
  localStorage.getItem("tenNhanVien") || "Nhân viên Order",
);
const showQRModal = ref(false);
const qrData = ref({ url: "", soTien: 0, maChungTu: "", banId: null });
const tables = ref([]);
const notificationList = ref([]);
const products = ref([]);
const selectedTable = ref(null);
const cart = ref([]);
const searchQuery = ref("");
const loadingTables = ref(true);
const ordering = ref(false);
const notifications = ref([]);
const activeTab = ref("map");

const tableModalRef = ref(null);
const menuOffcanvasRef = ref(null);
let tableModal = null;
let menuOffcanvas = null;

const currentBill = ref(null);
const loadingBill = ref(false);
const showCartDetail = ref(true);

const connection = new signalR.HubConnectionBuilder()
  .withUrl(`${backendUrl}/kitchenHub`)
  .withAutomaticReconnect()
  .build();

onMounted(async () => {
  window.addEventListener("resize", () => {
    isMobile.value = window.innerWidth < 768;
  });
  tableModal = new Modal(tableModalRef.value);
  menuOffcanvas = new Offcanvas(menuOffcanvasRef.value);

  try {
    await connection.start();
    connection.on("CoDonHangMoi", () =>
      fetchStructure(globalState.value.activeBranchId),
    );

    connection.on("MonAnDaXong", (data) => {
      if (!notifications.value.includes(data.banId))
        notifications.value.push(data.banId);

      notificationList.value.unshift({
        id: Date.now(),
        tenBan: data.tenBan,
        tenMon: data.tenMon,
        time: new Date().toLocaleTimeString("vi-VN"),
      });

      swal.fire({
        toast: true,
        position: "top-end",
        icon: "info",
        title: `Bếp báo: Bàn ${data.tenBan} ${data.tenMon}`,
        showConfirmButton: false,
        timer: 4000,
      });
      fetchStructure(globalState.value.activeBranchId); // SỬA Ở ĐÂY NỮA
    });
  } catch (err) {
    console.error("SignalR Lỗi: ", err);
  }

  await getBranchIdAndFetch();

  setInterval(() => {
    tables.value = [...tables.value];
  }, 60000);
});

onUnmounted(() => {
  connection.stop();
});

const calculateTimeElapsed = (timeOpenString) => {
  if (!timeOpenString) return "00:00";
  const openTime = new Date(timeOpenString);
  const now = new Date();
  const diffMins = Math.floor((now - openTime) / 60000);
  return `${Math.floor(diffMins / 60)
    .toString()
    .padStart(2, "0")}:${(diffMins % 60).toString().padStart(2, "0")}`;
};

const hasPendingItems = (tableId) => notifications.value.includes(tableId);
const formatPrice = (price) =>
  new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND" }).format(
    price || 0,
  );
const cartTotal = computed(() =>
  cart.value.reduce((t, i) => t + i.giaBan * i.soLuong, 0),
);

const filteredProducts = computed(() => {
  if (!searchQuery.value) return products.value;
  return products.value.filter((p) =>
    p.tenSanPham.toLowerCase().includes(searchQuery.value.toLowerCase()),
  );
});

const getBranchId = () => globalState.value.activeBranchId || 1;

const fetchStructure = async (branchId) => {
  loadingTables.value = true;
  try {
    // Không dùng getBranchId() nữa mà dùng branchId truyền vào
    const response = await axios.get(
      `/api/Ban/danh-sach-pos?chiNhanhId=${branchId}`,
    );
    tables.value = response.data;
  } catch (err) {
    console.error(err);
  } finally {
    loadingTables.value = false;
  }
};

// 3. Sửa lại hàm fetchProducts nhận ID động
const fetchProducts = async (branchId) => {
  try {
    const response = await axios.get(
      `/api/SanPham/danh-sach?chiNhanhId=${branchId}`,
    );
    products.value = response.data.filter((p) => p.trangThai === true);
  } catch (err) {
    console.error(err);
  }
};

const onTableClick = async (table) => {
  selectedTable.value = table;
  cart.value = [];
  searchQuery.value = "";

  if (table.trangThai === "Đang phục vụ") {
    // Tắt chấm đỏ trên bàn nếu đã click vào xem
    notifications.value = notifications.value.filter((id) => id !== table.id);
    await fetchBillDetails(table.id);
  }
  tableModal.show();
};

const fetchBillDetails = async (tableId) => {
  loadingBill.value = true;
  try {
    const res = await axios.get(`/api/HoaDon/ban/${tableId}`);
    currentBill.value = res.data;
  } catch (e) {
    currentBill.value = null;
  } finally {
    loadingBill.value = false;
  }
};

const openMenu = () => {
  tableModal.hide();
  menuOffcanvas.show();
};

const addToCart = (product) => {
  const existing = cart.value.find((c) => c.sanPhamId === product.id);
  if (existing) {
    existing.soLuong++;
  } else {
    cart.value.push({
      sanPhamId: product.id,
      tenSanPham: product.tenSanPham,
      giaBan: product.giaBan,
      soLuong: 1,
    });
  }
};

const incrementQuantity = (index) => cart.value[index].soLuong++;
const decrementQuantity = (index) => {
  if (cart.value[index].soLuong > 1) cart.value[index].soLuong--;
  else cart.value.splice(index, 1);
};

const submitOrder = async () => {
  ordering.value = true;
  try {
    const payload = {
      banId: selectedTable.value.id,
      danhSachMon: cart.value.map((item) => ({
        sanPhamId: item.sanPhamId,
        soLuong: item.soLuong,
        ghiChu: "",
      })),
    };
    await axios.post("/api/HoaDon/goimon", payload);

    swal.fire({
      toast: true,
      position: "top-end",
      icon: "success",
      title: "Đã gửi bếp!",
      timer: 1500,
      showConfirmButton: false,
    });
    cart.value = [];
    menuOffcanvas.hide();
    fetchStructure(globalState.value.activeBranchId);
  } catch (err) {
    swal.fire("Lỗi", err.response?.data || "Có lỗi xảy ra", "error");
  } finally {
    ordering.value = false;
  }
};

const requestPayment = async () => {
  swal
    .fire({
      title: "Báo thanh toán?",
      text: `Yêu cầu thu ngân thanh toán bàn ${selectedTable.value.tenBan}?`,
      icon: "question",
      showCancelButton: true,
      confirmButtonText: "Đồng ý",
    })
    .then(async (result) => {
      if (result.isConfirmed) {
        try {
          await connection.invoke(
            "YeuCauThanhToan",
            getBranchId(),
            selectedTable.value.tenBan,
          );
          tableModal.hide();
          swal.fire({
            toast: true,
            position: "top-end",
            icon: "success",
            title: "Đã báo Thu ngân!",
            timer: 1500,
            showConfirmButton: false,
          });
        } catch (e) {
          console.error("Lỗi", e);
        }
      }
    });
};

// --- HỦY MÓN (NV ORDER) ---
const handleOrderCancelItem = async (item) => {
  const { value: formValues } = await swal.fire({
    title: "Hủy / Trả đồ",
    html: `
      <div class="d-flex justify-content-center align-items-center mb-3">
         <span class="fs-5 fw-bold text-danger me-2">Số lượng hủy:</span>
         <input id="cancel-qty" type="number" class="form-control w-25 text-center fw-bold" value="1" min="1" max="${item.soLuong}">
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
      const reason = document.querySelector('input[name="reason"]:checked').value;
      if (qty > item.soLuong) {
        swal.showValidationMessage("Vượt quá SL thực tế!");
        return false;
      }
      return { qty, reason };
    },
  });

  if (formValues) {
    try {
      await axios.post("/api/HoaDon/huymon", {
        chiTietId: item.chiTietId,
        soLuongHuy: formValues.qty,
        lyDo: formValues.reason,
      });
      swal.fire({
        toast: true, position: "top-end", icon: "success",
        title: `Đã hủy ${formValues.qty} ${item.tenSanPham}`,
        timer: 2000, showConfirmButton: false,
      });
      await fetchBillDetails(selectedTable.value.id);
      await fetchStructure(globalState.value.activeBranchId);
    } catch (e) {
      swal.fire("Lỗi", "Không thể hủy món!", "error");
    }
  }
};

// --- CHUYỂN BÀN (NV ORDER) ---
const handleOrderChuyenBan = async () => {
  if (!selectedTable.value) return;
  const banTrong = tables.value.filter(
    (b) => b.trangThai === "Trống" && b.id !== selectedTable.value.id
  );
  if (banTrong.length === 0) {
    return swal.fire("Hết bàn", "Không còn bàn trống nào!", "warning");
  }
  const options = banTrong.map((b) => `<option value="${b.id}">${b.tenBan}</option>`).join("");
  const { value: denBanId, isConfirmed } = await swal.fire({
    title: `🔀 Chuyển bàn`,
    html: `<p class="mb-2">Từ: <strong>${selectedTable.value.tenBan}</strong></p>
      <label class="form-label fw-bold">Chuyển sang:</label>
      <select id="swal-den-ban" class="form-select">${options}</select>`,
    showCancelButton: true,
    confirmButtonText: "Chuyển bàn",
    confirmButtonColor: "#3f51b5",
    preConfirm: () => {
      const val = document.getElementById("swal-den-ban").value;
      if (!val) { swal.showValidationMessage("Chọn bàn!"); return false; }
      return parseInt(val);
    },
  });
  if (!isConfirmed) return;
  try {
    await axios.post("/api/HoaDon/chuyenban", {
      tuBanId: selectedTable.value.id,
      denBanId: denBanId,
    });
    tableModal.hide();
    await fetchStructure(globalState.value.activeBranchId);
    swal.fire({ toast: true, position: "top-end", icon: "success", title: "Đã chuyển bàn!", timer: 2000, showConfirmButton: false });
  } catch (e) {
    swal.fire("Lỗi", e.response?.data || "Không thể chuyển bàn!", "error");
  }
};

// --- TÁCH BÀN (NV ORDER) ---
const handleOrderTachBan = async () => {
  if (!selectedTable.value || !currentBill.value) return;
  const sentItems = currentBill.value.danhSachMon || [];
  if (sentItems.length === 0) {
    return swal.fire("Chú ý", "Chưa có món nào để tách!", "warning");
  }
  const banTrong = tables.value.filter(
    (b) => b.trangThai === "Trống" && b.id !== selectedTable.value.id
  );
  if (banTrong.length === 0) {
    return swal.fire("Hết bàn", "Không còn bàn trống nào!", "warning");
  }
  const monCheckboxes = sentItems.map((i) => `
    <div class="form-check text-start border-bottom py-1">
      <input class="form-check-input tach-check" type="checkbox" value="${i.chiTietId}" id="tach-${i.chiTietId}">
      <label class="form-check-label" for="tach-${i.chiTietId}">
        <strong>${i.tenSanPham}</strong> <span class="text-muted">x${i.soLuong}</span>
        <span class="float-end text-danger">${formatPrice(i.thanhTien)}</span>
      </label>
    </div>`).join("");
  const optionsBan = banTrong.map((b) => `<option value="${b.id}">${b.tenBan}</option>`).join("");

  const { value: formVal, isConfirmed } = await swal.fire({
    title: `⚡ Tách bàn từ ${selectedTable.value.tenBan}`,
    width: 500,
    html: `
      <label class="form-label fw-bold mb-1">Tách sang bàn trống:</label>
      <select id="swal-tach-den-ban" class="form-select mb-3">${optionsBan}</select>
      <label class="form-label fw-bold mb-1">Chọn món cần tách:</label>
      <div style="max-height:220px;overflow-y:auto;border:1px solid #dee2e6;border-radius:6px;padding:4px 8px">
        ${monCheckboxes}
      </div>`,
    showCancelButton: true,
    confirmButtonText: "Tách bàn",
    confirmButtonColor: "#dc3545",
    preConfirm: () => {
      const denBanId = parseInt(document.getElementById("swal-tach-den-ban").value);
      const checked = [...document.querySelectorAll(".tach-check:checked")].map((el) => parseInt(el.value));
      if (!checked.length) { swal.showValidationMessage("Chọn ít nhất 1 món!"); return false; }
      return { denBanId, danhSachChiTietId: checked };
    },
  });
  if (!isConfirmed || !formVal) return;
  try {
    await axios.post("/api/HoaDon/tachban", {
      tuBanId: selectedTable.value.id,
      denBanId: formVal.denBanId,
      danhSachChiTietId: formVal.danhSachChiTietId,
    });
    tableModal.hide();
    await fetchStructure(globalState.value.activeBranchId);
    swal.fire({ toast: true, position: "top-end", icon: "success", title: `Đã tách ${formVal.danhSachChiTietId.length} món!`, timer: 2000, showConfirmButton: false });
  } catch (e) {
    swal.fire("Lỗi", e.response?.data || "Không thể tách bàn!", "error");
  }
};

// --- BÁO CHẾ BIẾN (NV ORDER) ---
const handleOrderBaoCheBien = () => {
  // NV Order thì mở thực đơn để gọi món rồi gửi bếp
  openMenu();
};

const toggleNotifications = () => {
  activeTab.value =
    activeTab.value === "notifications" ? "map" : "notifications";
};

const logout = () => {
  // 1. Quét sạch toàn bộ Token và ID Chi nhánh cũ lưu trong máy
  localStorage.clear();

  // 2. DÙNG window.location.href THAY VÌ router.push('/login')
  // Lý do: Việc tải lại trang web sẽ quét sạch mọi dữ liệu cũ còn kẹt trong RAM (globalState) của Vue
  window.location.href = "/login";
};

// --- BẮT ĐẦU ĐOẠN CODE MỚI ---

// 1. Hàm tự động tìm đúng Chi nhánh đang làm việc
const getBranchIdAndFetch = async () => {
  // --- THÊM ĐOẠN NÀY ĐỂ ĐỒNG BỘ CẤU HÌNH NGÂN HÀNG TỪ CSDL VỀ MÁY ---
  try {
    const resConfig = await axios.get("/api/ThietLap/BankConfig");
    if (resConfig.data && resConfig.data.duLieu) {
      localStorage.setItem("pos36_bank_config", resConfig.data.duLieu);
    }
  } catch (e) {
    console.log("Chưa có cấu hình NH");
  }
  // -----------------------------------------------------------------

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

  // Sau khi chắc chắn có branchId đúng của tài khoản này rồi mới gọi API lấy bàn và món
  if (branchId) {
    // Đảm bảo RAM Vue được đồng bộ
    globalState.value.activeBranchId = branchId;
    await fetchStructure(branchId);
    await fetchProducts(branchId);
  }
};

// Lắng nghe lệnh yêu cầu mở QR từ Thu Ngân (SỬA LỖI KHÔNG HIỆN)
// Lắng nghe lệnh yêu cầu mở QR
connection.on("NhanYeuCauMoQR", (banId, soTien) => {
  const table = tables.value.find((t) => t.id === banId);
  const tenBanHienTai = table ? table.tenBan : `Bàn số ${banId}`;
  const maChungTu = `POS36B${banId}`; // <--- MÃ MỚI SIÊU CHUẨN

  const bankConfig = JSON.parse(
    localStorage.getItem("pos36_bank_config") || "{}",
  );
  if (!bankConfig.bankId) return swal.fire("Lỗi", "Chưa thiết lập NH", "error");

  const accountName = encodeURIComponent(bankConfig.accountName);
  const url = `https://img.vietqr.io/image/${bankConfig.bankId}-${bankConfig.accountNo}-${bankConfig.template}.png?amount=${soTien}&addInfo=${maChungTu}&accountName=${accountName}`;

  qrData.value = { url, soTien, maChungTu, banId, tenBan: tenBanHienTai };
  showQRModal.value = true;
});
// Nhân viên chủ động bấm Hủy (Do khách muốn trả tiền mặt hoặc sai bill)
const huyHienThiQR = () => {
  showQRModal.value = false;
  connection.invoke(
    "HuyMoQR",
    qrData.value.banId,
    "Nhân viên/Khách hàng báo hủy",
  );
};

// Lắng nghe tiền về thật (để tự đóng modal)
connection.on("ThanhToanQRThanhCong", (banId) => {
  if (qrData.value.banId === banId) {
    showQRModal.value = false;
    swal.fire(
      "Thành công!",
      "Hệ thống đã nhận được tiền chuyển khoản.",
      "success",
    );
  }
});
</script>

<style scoped>
.mobile-first-layout {
  height: 100vh;
  touch-action: pan-y;
}
.fs-7 {
  font-size: 0.8rem;
}
.cursor-pointer {
  cursor: pointer;
}
.table-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(100px, 1fr));
  gap: 15px;
}
.table-card {
  aspect-ratio: 1/1;
  cursor: pointer;
  transition: transform 0.2s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}
.table-card:active {
  transform: scale(0.95);
}
.menu-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(110px, 1fr));
  gap: 10px;
  padding-bottom: 20px;
}
.menu-item-card {
  aspect-ratio: 4/5;
  cursor: pointer;
}
.menu-item-card:active {
  background-color: #f8f9fa !important;
}
.fade-in {
  animation: fadeIn 0.3s ease-in-out;
}
@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
.pulse-animation {
  animation: pulse 2s infinite;
}
@keyframes pulse {
  0% {
    box-shadow: 0 0 0 0 rgba(220, 53, 69, 0.7);
  }
  70% {
    box-shadow: 0 0 0 10px rgba(220, 53, 69, 0);
  }
  100% {
    box-shadow: 0 0 0 0 rgba(220, 53, 69, 0);
  }
}
.custom-offcanvas.offcanvas-bottom {
  height: 90vh !important;
  border-top-left-radius: 20px;
  border-top-right-radius: 20px;
}
.custom-offcanvas.offcanvas-end {
  width: 450px !important;
}
.bottom-nav {
  z-index: 1030;
  padding-bottom: env(safe-area-inset-bottom);
}
</style>
