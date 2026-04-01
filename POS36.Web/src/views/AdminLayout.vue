<script setup>
import { onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import axios from "axios";
import { globalState } from "../store";
import AiCopilot from "../components/AiCopilot.vue";

const router = useRouter();

// ==========================================
// THUẬT TOÁN ĐỔI MÀU GIAO DIỆN (THEME)
// ==========================================
const currentTheme = ref("#f37021"); // Mặc định là màu Cam POS36 cũ của sếp

// 6 mã màu chuẩn xác bốc từ ảnh sếp gửi
const themeColors = [
  { name: "Xanh lá", hex: "#22c55e" },
  { name: "Cam", hex: "#f37021" },
  { name: "Đỏ", hex: "#ef4444" },
  { name: "Xanh dương", hex: "#3b82f6" },
  { name: "Tím", hex: "#a855f7" },
  { name: "Navy", hex: "#334155" },
];

// Hàm chuyển Hex sang RGB để Bootstrap xài được
const hexToRgb = (hex) => {
  let r = parseInt(hex.slice(1, 3), 16);
  let g = parseInt(hex.slice(3, 5), 16);
  let b = parseInt(hex.slice(5, 7), 16);
  return `${r}, ${g}, ${b}`;
};

const changeTheme = (hex) => {
  currentTheme.value = hex;
  localStorage.setItem("pos36_admin_theme", hex);
};

// ==========================================

const fetchBranches = async () => {
  try {
    const res = await axios.get("/api/ChiNhanh");
    globalState.value.branches = res.data;

    if (res.data.length > 0) {
      const currentId = globalState.value.activeBranchId;
      const isValid = res.data.some((b) => b.id === currentId);

      if (!currentId || !isValid) {
        globalState.value.activeBranchId = res.data[0].id;
        localStorage.setItem("pos36_active_branch", res.data[0].id);
      }
    } else {
      globalState.value.activeBranchId = null;
    }
  } catch (error) {
    console.error("Lỗi tải danh sách chi nhánh", error);
  }
};

const handleLogout = () => {
  localStorage.clear();
  window.location.href = "/login";
};

onMounted(() => {
  fetchBranches();

  // Khôi phục màu sếp đã chọn từ lần trước
  const savedTheme = localStorage.getItem("pos36_admin_theme");
  if (savedTheme) {
    currentTheme.value = savedTheme;
  }
});
</script>

<template>
  <div
    class="admin-wrapper"
    :style="{
      '--bs-primary': currentTheme,
      '--bs-primary-rgb': hexToRgb(currentTheme),
      '--bs-body-color':
        '#333' /* 2. ÉP TẤT CẢ BODY TEXT VỀ MÀU XÁM ĐẬM TRUNG TÍNH */,
    }"
  >
    <nav class="navbar navbar-expand-lg navbar-dark pos-navbar shadow-sm">
      <div class="container-fluid px-4">
        <router-link class="navbar-brand fw-bold fs-4 me-4" to="/admin">
          <i class="bi bi-shop me-2"></i>POS36
        </router-link>

        <div class="collapse navbar-collapse" id="adminMenu">
          <ul class="navbar-nav me-auto mb-2 mb-lg-0 fw-medium">
            <li class="nav-item dropdown me-2">
              <a
                class="nav-link dropdown-toggle"
                href="#"
                data-bs-toggle="dropdown"
              >
                <i class="bi bi-box-seam me-1"></i> Hàng hóa
              </a>
              <ul class="dropdown-menu shadow-sm border-0">
                <li>
                  <router-link
                    class="dropdown-item fw-bold"
                    to="/admin/products"
                    ><i class="bi bi-card-list"></i> Thực đơn</router-link
                  >
                </li>
                <li>
                  <router-link class="dropdown-item fw-bold" to="/admin/prices"
                    ><i class="bi bi-currency-dollar"></i> Thiết lập
                    giá</router-link
                  >
                </li>
                <li>
                  <router-link
                    class="dropdown-item fw-bold"
                    to="/admin/inventory"
                    ><i class="bi bi-clipboard-check"></i> Kiểm kê</router-link
                  >
                </li>
              </ul>
            </li>

            <li class="nav-item dropdown me-2">
              <a
                class="nav-link dropdown-toggle"
                href="#"
                data-bs-toggle="dropdown"
              >
                <i class="bi bi-shop-window me-1"></i> Nhà hàng
              </a>
              <ul class="dropdown-menu shadow-sm border-0">
                <li>
                  <router-link
                    class="dropdown-item text-primary fw-bold"
                    to="/pos"
                    target="_blank"
                    >Màn hình Thu ngân</router-link
                  >
                </li>
                <li>
                  <router-link
                    class="dropdown-item text-success fw-bold"
                    to="/order"
                    target="_blank"
                    >Màn hình Nhân viên</router-link
                  >
                </li>
                <li>
                  <router-link
                    class="dropdown-item text-danger fw-bold"
                    to="/kitchen"
                    target="_blank"
                    >Màn hình Bếp</router-link
                  >
                </li>
                <li><hr class="dropdown-divider" /></li>
                <li>
                  <router-link class="dropdown-item fw-bold" to="/admin/tables"
                    ><i class="bi bi-grid-3x3-gap"></i> Danh sách
                    phòng/bàn</router-link
                  >
                </li>
              </ul>
            </li>

            <li class="nav-item dropdown me-2">
              <a
                class="nav-link dropdown-toggle"
                href="#"
                data-bs-toggle="dropdown"
              >
                <i class="bi bi-arrow-left-right me-1"></i> Giao dịch
              </a>
              <ul class="dropdown-menu shadow-sm border-0">
                <li>
                  <router-link class="dropdown-item" to="/admin/orders"
                    >Danh sách đơn hàng</router-link
                  >
                </li>
                <li>
                  <router-link
                    class="dropdown-item fw-bold"
                    to="/admin/import-stock"
                    ><i class="bi bi-box-arrow-in-down"></i> Nhập
                    hàng</router-link
                  >
                </li>
              </ul>
            </li>

            <li class="nav-item dropdown me-2">
              <a
                class="nav-link dropdown-toggle"
                href="#"
                data-bs-toggle="dropdown"
              >
                <i class="bi bi-people me-1"></i> Nội bộ
              </a>
              <ul class="dropdown-menu shadow-sm border-0">
                <li>
                  <router-link
                    class="dropdown-item fw-bold"
                    to="/admin/employees"
                    ><i class="bi bi-person-badge"></i> Nhân viên</router-link
                  >
                </li>
              </ul>
            </li>

            <li class="nav-item me-2">
              <router-link class="nav-link" to="/admin/cashbook">
                <i class="bi bi-wallet2 me-1"></i> Thu & Chi
              </router-link>
            </li>

            <li class="nav-item dropdown me-2">
              <a
                class="nav-link dropdown-toggle"
                href="#"
                data-bs-toggle="dropdown"
              >
                <i class="bi bi-bar-chart-line me-1"></i> Báo cáo
              </a>
              <ul class="dropdown-menu shadow border-0 mt-2">
                <li>
                  <router-link
                    to="/admin/daily-summary"
                    class="dropdown-item py-2"
                    >Tổng kết cuối ngày</router-link
                  >
                </li>
                <li>
                  <router-link
                    to="/admin/sales-report"
                    class="dropdown-item py-2"
                    >Báo cáo bán hàng</router-link
                  >
                </li>
                <li><hr class="dropdown-divider" /></li>
                <li>
                  <router-link
                    to="/admin/ai-report"
                    class="dropdown-item py-2 fw-bold text-primary"
                  >
                    <i class="bi bi-robot text-danger me-2"></i> Trợ lý AI Phân
                    tích
                  </router-link>
                </li>
              </ul>
            </li>
          </ul>

          <div class="d-flex align-items-center">
            <div
              class="me-3 d-flex align-items-center bg-white rounded-pill px-2 py-1"
              style="min-width: 200px"
            >
              <i class="bi bi-geo-alt-fill text-danger me-2 ms-1"></i>
              <select
                class="form-select form-select-sm border-0 shadow-none fw-bold text-dark"
                v-model="globalState.activeBranchId"
              >
                <option
                  v-for="b in globalState.branches"
                  :key="b.id"
                  :value="b.id"
                >
                  {{ b.tenChiNhanh }}
                </option>
                <option v-if="globalState.branches.length === 0" value="null">
                  Chưa có chi nhánh
                </option>
              </select>
            </div>

            <span class="text-white me-2 fw-medium"
              ><i class="bi bi-person-circle fs-5 align-middle me-1"></i>
              Admin</span
            >

            <ul class="navbar-nav">
              <li class="nav-item dropdown ms-1">
                <a
                  class="nav-link text-white hide-caret p-0 ms-2"
                  href="#"
                  role="button"
                  data-bs-toggle="dropdown"
                  data-bs-auto-close="outside"
                  aria-expanded="false"
                  title="Cài đặt hệ thống"
                >
                  <i class="bi bi-gear-fill fs-4 gear-icon"></i>
                </a>

                <ul
                  class="dropdown-menu dropdown-menu-end shadow border-0 mt-3 py-2"
                  style="width: 260px"
                >
                  <li>
                    <h6 class="dropdown-header text-muted fw-bold">HỆ THỐNG</h6>
                  </li>
                  <li>
                    <a class="dropdown-item" href="#"
                      ><i class="bi bi-shop-window me-2 text-muted"></i> Thông
                      tin cửa hàng</a
                    >
                  </li>
                  <li>
                    <a class="dropdown-item" href="#"
                      ><i class="bi bi-person-vcard me-2 text-muted"></i> Thông
                      tin cá nhân</a
                    >
                  </li>
                  <li>
                    <a class="dropdown-item" href="#"
                      ><i class="bi bi-sliders me-2 text-muted"></i> Thiết lập
                      cửa hàng</a
                    >
                  </li>
                  <li>
                    <router-link class="dropdown-item" to="/admin/print-setup"
                      ><i class="bi bi-printer me-2 text-muted"></i> Thiết lập
                      mẫu in</router-link
                    >
                  </li>
                  <li>
                    <router-link class="dropdown-item" to="/admin/bank-setup"
                      ><i class="bi bi-qr-code-scan me-2 text-muted"></i> Thiết
                      lập chuyển khoản</router-link
                    >
                  </li>

                  <li><hr class="dropdown-divider my-2" /></li>
                  <li>
                    <a
                      class="dropdown-item text-danger fw-bold"
                      href="#"
                      @click.prevent="handleLogout"
                    >
                      <i class="bi bi-box-arrow-right me-2"></i> Đăng xuất
                    </a>
                  </li>

                  <li><hr class="dropdown-divider my-2" /></li>
                  <li class="px-3 pb-2 pt-1">
                    <span
                      class="d-block fw-bold text-muted mb-2"
                      style="font-size: 0.75rem"
                      ><i class="bi bi-palette-fill me-1"></i> MÀU SẮC GIAO
                      DIỆN</span
                    >
                    <div class="row g-2">
                      <div
                        v-for="(color, index) in themeColors"
                        :key="index"
                        class="col-4"
                      >
                        <div
                          class="color-box w-100 rounded-1 cursor-pointer shadow-sm position-relative"
                          :style="{ backgroundColor: color.hex }"
                          :class="{
                            'active-color': currentTheme === color.hex,
                          }"
                          @click="changeTheme(color.hex)"
                        >
                          <i
                            v-if="currentTheme === color.hex"
                            class="bi bi-check-lg position-absolute top-50 start-50 translate-middle text-white fw-bold fs-5 shadow-sm"
                          ></i>
                        </div>
                      </div>
                    </div>
                  </li>
                </ul>
              </li>
            </ul>
          </div>
        </div>
      </div>
    </nav>
    <router-view></router-view>
  </div>

  <AiCopilot />
</template>

<style scoped>
.admin-wrapper {
  background-color: #f0f2f5;
  min-height: 100vh;
  transition: all 0.3s ease;
}

/* 3. SỬ DỤNG BIẾN CSS CHO NAVBAR ĐỂ NÓ ĐỔI MÀU THEO */
.pos-navbar {
  background-color: var(--bs-primary);
  transition: background-color 0.3s ease;
}

.pos-navbar .nav-link {
  color: rgba(255, 255, 255, 0.9) !important;
  padding: 10px 15px !important;
}
.pos-navbar .nav-link:hover {
  background-color: rgba(0, 0, 0, 0.1);
  border-radius: 4px;
  color: #fff !important;
}
select:focus {
  outline: none;
  box-shadow: none;
}

/* Dropdown styling */
.dropdown-menu {
  border-radius: 8px;
  border: 1px solid #eee;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.1) !important;
}
.dropdown-item {
  padding: 10px 20px;
  font-weight: 500;
  transition: all 0.2s;
}

/* 4. MÀU HOVER CỦA DROPDOWN CŨNG TỰ ĐỘNG ĐỔI THEO THEME */
.dropdown-item:hover {
  background-color: rgba(var(--bs-primary-rgb), 0.1) !important;
  color: var(--bs-primary) !important;
}
.dropdown-item:hover i {
  color: var(--bs-primary) !important;
}

.hide-caret::after {
  display: none !important;
}

.gear-icon {
  display: inline-block;
  transition: transform 0.4s ease;
  pointer-events: none;
}
.nav-link:hover .gear-icon {
  transform: rotate(90deg);
  color: #fff !important;
}

/* STYLE CHO CÁC Ô CHỌN MÀU */
.color-box {
  height: 40px;
  border: 2px solid transparent;
  transition:
    transform 0.2s ease,
    border-color 0.2s ease;
}
.color-box:hover {
  transform: scale(1.08);
}
.active-color {
  border-color: #000 !important; /* Viền đen báo hiệu đang được chọn */
  transform: scale(1.08);
}
.cursor-pointer {
  cursor: pointer;
}

/* =========================================
   5. BẮT BUỘC BOOTSTRAP TRONG ADMIN ĐỔI MÀU SÂU
   ========================================= */
.admin-wrapper :deep(.bg-primary) {
  background-color: var(--bs-primary) !important;
}
.admin-wrapper :deep(.text-primary) {
  color: var(--bs-primary) !important;
}
.admin-wrapper :deep(.btn-primary) {
  background-color: var(--bs-primary) !important;
  border-color: var(--bs-primary) !important;
}
.admin-wrapper :deep(.btn-outline-primary) {
  color: var(--bs-primary) !important;
  border-color: var(--bs-primary) !important;
}
.admin-wrapper :deep(.btn-outline-primary:hover) {
  background-color: var(--bs-primary) !important;
  color: #fff !important;
}

/* 6. ÉP TIÊU ĐỀ CỘT CỦA BẢNG ĐỔI MÀU (IMAGE_4, IMAGE_5, IMAGE_6) */
.admin-wrapper :deep(table th) {
  color: var(--bs-primary) !important;
  font-weight: 700 !important;
}

/* 7. ÉP CÁC CHỮ CÓ CLASS .text-danger VÀ .text-success GIỮ NGUYÊN MÀU MẶC ĐỊNH
   ĐỂ ĐẢM BẢO TÍNH NGHĨA (VD: MÀU ĐỎ LÀ CẢNH BÁO) */
.admin-wrapper :deep(.text-danger) {
  color: #dc3545 !important;
}
.admin-wrapper :deep(.text-success) {
  color: #198754 !important;
}
</style>
