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
                      <div class="text-end">
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
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div
            class="modal-footer border-0 bg-white"
            v-if="selectedTable?.trangThai !== 'Trống'"
          >
            <div class="row w-100 g-2">
              <div class="col-6">
                <button
                  class="btn btn-outline-info w-100 fw-bold rounded-3"
                  @click="openMenu"
                >
                  GỌI THÊM
                </button>
              </div>
              <div class="col-6">
                <button
                  class="btn btn-danger w-100 fw-bold rounded-3"
                  @click="requestPayment"
                >
                  BÁO THU NGÂN
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
              class="card border-0 shadow-sm rounded-4 menu-item-card"
              @click="addToCart(item)"
            >
              <div
                class="card-body p-2 text-center d-flex flex-column justify-content-between h-100"
              >
                <i class="bi bi-cup-hot text-info fs-3 my-1"></i>
                <h6 class="fw-bold mb-1 lh-sm" style="font-size: 0.8rem">
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
</template>

<script setup>
import { ref, computed, onMounted, inject, onUnmounted } from "vue";
import { useRouter } from "vue-router";
import axios from "axios";
import * as signalR from "@microsoft/signalr";
import { Modal, Offcanvas } from "bootstrap";
import { globalState } from "../store";

const router = useRouter();
const swal = inject("$swal");
const backendUrl = "http://localhost:5198";

const isMobile = ref(window.innerWidth < 768);
const tenNhanVien = ref(
  localStorage.getItem("tenNhanVien") || "Nhân viên Order",
);

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
    connection.on("CoDonHangMoi", () => fetchStructure());

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
      fetchStructure();
    });
  } catch (err) {
    console.error("SignalR Lỗi: ", err);
  }

  await fetchStructure();
  await fetchProducts();

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

const fetchStructure = async () => {
  loadingTables.value = true;
  try {
    const response = await axios.get(
      `/api/Ban/danh-sach-pos?chiNhanhId=${getBranchId()}`,
    );
    tables.value = response.data;
  } catch (err) {
    console.error(err);
  } finally {
    loadingTables.value = false;
  }
};

const fetchProducts = async () => {
  try {
    const response = await axios.get(
      `/api/SanPham/danh-sach?chiNhanhId=${getBranchId()}`,
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
    fetchStructure();
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

const toggleNotifications = () => {
  activeTab.value =
    activeTab.value === "notifications" ? "map" : "notifications";
};

const logout = () => {
  localStorage.clear();
  router.push("/login");
};
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
