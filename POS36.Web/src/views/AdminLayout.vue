<script setup>
import { onMounted, onUnmounted, ref, computed } from "vue";
import { useRouter } from "vue-router";
import axios from "axios";
import { globalState } from "../store";
import AiCopilot from "../components/AiCopilot.vue";

const storeTrangThai = ref(localStorage.getItem("pos36_storeTrangThai") || "HoatDong");
const soNgayConLai = ref(999);

const router = useRouter();

// ==========================================
// PHÂN QUYỀN THU NGÂN
// ==========================================
const userRole = localStorage.getItem("pos36_role") || "";
const tenNhanVien = localStorage.getItem("tenNhanVien") || "Nhân viên";
const isThuNgan = userRole === "ThuNgan";
const quyenRaw = localStorage.getItem("pos36_quyen_thungan") || "";
const danhSachQuyen = quyenRaw ? quyenRaw.split(",").map(q => q.trim()) : [];

// Helper kiểm tra quyền (Thu ngân)
const hasQuyen = (ma) => !isThuNgan || danhSachQuyen.includes(ma);

// Nút quay lại màn hình Thu ngân
const goBackToPOS = () => router.push("/pos");

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

const handleLogout = async () => {
  try {
    await axios.post("/api/Auth/logout");
  } catch (e) {
    console.error("Lỗi đăng xuất", e);
  }
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

  // SaaS: Load trạng thái gói dịch vụ
  loadSubscriptionStatus();

  // FIX-SEC-5: Load cài đặt Auto-Logout và khởi động bộ đếm nếu được bật
  loadAutoLogoutSettings();
});

onUnmounted(() => {
  // Dọn dẹp timer khi thoát khỏi trang
  stopInactivityWatcher();
});

const loadSubscriptionStatus = async () => {
  try {
    const res = await axios.get("/api/Subscription/my-plan");
    storeTrangThai.value = res.data.trangThai;
    soNgayConLai.value = res.data.soNgayConLai;
    localStorage.setItem("pos36_storeTrangThai", res.data.trangThai);
  } catch (e) {
    // Ignore
  }
};

// ==========================================
// FIX-SEC-5: INACTIVITY TIMER AUTO-LOGOUT
// ==========================================
let inactivityTimer = null;
const autoLogoutEnabled = ref(false);
const autoLogoutMinutes = ref(30);

const resetTimer = () => {
  if (!autoLogoutEnabled.value) return;
  clearTimeout(inactivityTimer);
  inactivityTimer = setTimeout(async () => {
    // Tự đăng xuất khi quá thời gian không tương tác
    try { await axios.post("/api/Auth/logout"); } catch (e) { /* ignore */ }
    localStorage.clear();
    window.location.href = "/login";
  }, autoLogoutMinutes.value * 60 * 1000);
};

const startInactivityWatcher = () => {
  const events = ["mousemove", "keydown", "mousedown", "touchstart", "scroll", "click"];
  events.forEach(evt => window.addEventListener(evt, resetTimer, { passive: true }));
  resetTimer(); // Khởi động lần đầu
};

const stopInactivityWatcher = () => {
  clearTimeout(inactivityTimer);
  const events = ["mousemove", "keydown", "mousedown", "touchstart", "scroll", "click"];
  events.forEach(evt => window.removeEventListener(evt, resetTimer));
};

const loadAutoLogoutSettings = async () => {
  try {
    const res = await axios.get("/api/ThietLap/batch", {
      params: { keys: "Security_AutoLogout,Security_TimeoutPhut" }
    });
    if (res.data) {
      autoLogoutEnabled.value = res.data.Security_AutoLogout === "true";
      const phut = parseInt(res.data.Security_TimeoutPhut);
      if (!isNaN(phut) && phut > 0) autoLogoutMinutes.value = phut;

      if (autoLogoutEnabled.value) {
        startInactivityWatcher();
      }
    }
  } catch (e) {
    console.error("Lỗi load cấu hình Auto-Logout", e);
  }
};

const showBanner = computed(() => {
  return storeTrangThai.value === "DungThu" ||
         storeTrangThai.value === "ChiDoc" ||
         (storeTrangThai.value === "HoatDong" && soNgayConLai.value <= 7);
});

const bannerInfo = computed(() => {
  if (storeTrangThai.value === "ChiDoc") {
    return { text: "⚠️ Gói dịch vụ đã hết hạn! Hệ thống đang ở chế độ CHỈ ĐỌC. Vui lòng gia hạn để tiếp tục sử dụng.", cls: "banner-danger" };
  }
  if (storeTrangThai.value === "DungThu") {
    return { text: `🎉 Bạn đang dùng thử miễn phí. Còn ${soNgayConLai.value} ngày. Mua gói để sử dụng không giới hạn!`, cls: "banner-info" };
  }
  if (soNgayConLai.value <= 7) {
    return { text: `⏰ Gói dịch vụ sắp hết hạn! Còn ${soNgayConLai.value} ngày. Gia hạn ngay để tránh gián đoạn.`, cls: "banner-warning" };
  }
  return { text: "", cls: "" };
});

// Tên hiển thị cho Thu ngân
const userDisplayLabel = computed(() => {
  if (isThuNgan) return `${tenNhanVien} (Thu ngân)`;
  return "Admin";
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
          <!-- Banner chế độ hạn chế khi là Thu ngân -->
          <div v-if="isThuNgan" class="d-flex align-items-center me-3">
            <span class="badge rounded-pill px-3 py-2" style="background: rgba(255,193,7,0.2); border: 1px solid rgba(255,193,7,0.5); color: #ffc107; font-size: 0.75rem;">
              <i class="bi bi-shield-lock-fill me-1"></i> Chế độ hạn chế
            </span>
            <button @click="goBackToPOS" class="btn btn-sm ms-2 fw-bold" style="background: rgba(255,255,255,0.15); color: white; border: 1px solid rgba(255,255,255,0.3); border-radius: 8px;">
              <i class="bi bi-arrow-left me-1"></i> Về Thu ngân
            </button>
          </div>

          <ul class="navbar-nav me-auto mb-2 mb-lg-0 fw-medium">
            <!-- Hàng hóa: Ẩn toàn bộ với Thu ngân trừ khi có quyền xem thực đơn -->
            <li v-if="!isThuNgan || hasQuyen('view_products') || hasQuyen('view_inventory')" class="nav-item dropdown me-2">
              <a
                class="nav-link dropdown-toggle"
                href="#"
                data-bs-toggle="dropdown"
              >
                <i class="bi bi-box-seam me-1"></i> Hàng hóa
              </a>
              <ul class="dropdown-menu shadow-sm border-0">
                <li v-if="hasQuyen('view_products')">
                  <router-link
                    class="dropdown-item fw-bold"
                    to="/admin/products"
                    ><i class="bi bi-card-list"></i> Thực đơn</router-link
                  >
                </li>
                <!-- Thiết lập giá: Ẩn với Thu ngân -->
                <li v-if="!isThuNgan">
                  <router-link class="dropdown-item fw-bold" to="/admin/prices"
                    ><i class="bi bi-currency-dollar"></i> Thiết lập
                    giá</router-link
                  >
                </li>
                <li v-if="hasQuyen('view_inventory')">
                  <router-link
                    class="dropdown-item fw-bold"
                    to="/admin/inventory"
                    ><i class="bi bi-clipboard-check"></i> Kiểm kê</router-link
                  >
                </li>
              </ul>
            </li>

            <!-- Nhà hàng: Ẩn tòan bộ với Thu ngân -->
            <li v-if="!isThuNgan" class="nav-item dropdown me-2">
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

            <!-- Giao dịch -->
            <li v-if="!isThuNgan || hasQuyen('view_orders') || hasQuyen('view_import_stock')" class="nav-item dropdown me-2">
              <a
                class="nav-link dropdown-toggle"
                href="#"
                data-bs-toggle="dropdown"
              >
                <i class="bi bi-arrow-left-right me-1"></i> Giao dịch
              </a>
              <ul class="dropdown-menu shadow-sm border-0">
                <li v-if="hasQuyen('view_orders')">
                  <router-link class="dropdown-item" to="/admin/orders"
                    ><i class="bi bi-receipt me-2 text-muted"></i> Danh sách đơn hàng</router-link
                  >
                </li>
                <li v-if="hasQuyen('view_import_stock')">
                  <router-link
                    class="dropdown-item fw-bold"
                    to="/admin/import-stock"
                    ><i class="bi bi-box-arrow-in-down"></i> Nhập
                    hàng</router-link
                  >
                </li>
              </ul>
            </li>

            <!-- Nội bộ: Ẩn toàn bộ với Thu ngân (trừ khi có quyền xem khách hàng) -->
            <li v-if="!isThuNgan || hasQuyen('view_customers')" class="nav-item dropdown me-2">
              <a
                class="nav-link dropdown-toggle"
                href="#"
                data-bs-toggle="dropdown"
              >
                <i class="bi bi-people me-1"></i> Nội bộ
              </a>
              <ul class="dropdown-menu shadow-sm border-0">
                <!-- Nhân viên: Ẩn với Thu ngân -->
                <li v-if="!isThuNgan">
                  <router-link
                    class="dropdown-item fw-bold"
                    to="/admin/employees"
                    ><i class="bi bi-person-badge me-1"></i> Nhân viên</router-link
                  >
                </li>
                <li v-if="hasQuyen('view_customers')">
                  <router-link
                    class="dropdown-item fw-bold"
                    to="/admin/customers"
                    ><i class="bi bi-person-hearts me-1"></i> Khách hàng</router-link
                  >
                </li>
              </ul>
            </li>

            <li v-if="hasQuyen('view_cashbook')" class="nav-item me-2">
              <router-link class="nav-link" to="/admin/cashbook">
                <i class="bi bi-wallet2 me-1"></i> Thu &amp; Chi
              </router-link>
            </li>

            <!-- Báo cáo -->
            <li v-if="!isThuNgan || hasQuyen('view_daily_summary') || hasQuyen('view_sales_report') || hasQuyen('view_lai_gop') || hasQuyen('view_ai_report')" class="nav-item dropdown me-2">
              <a
                class="nav-link dropdown-toggle"
                href="#"
                data-bs-toggle="dropdown"
              >
                <i class="bi bi-bar-chart-line me-1"></i> Báo cáo
              </a>
              <ul class="dropdown-menu shadow border-0 mt-2">
                <li v-if="hasQuyen('view_daily_summary')">
                  <router-link
                    to="/admin/daily-summary"
                    class="dropdown-item py-2"
                    ><i class="bi bi-calendar-check me-2 text-muted"></i> Tổng kết cuối ngày</router-link
                  >
                </li>
                <li v-if="hasQuyen('view_sales_report')">
                  <router-link
                    to="/admin/sales-report"
                    class="dropdown-item py-2"
                    ><i class="bi bi-cash-stack me-2 text-muted"></i> Báo cáo bán hàng</router-link
                  >
                </li>
                <li v-if="hasQuyen('view_lai_gop')">
                  <router-link
                    to="/admin/lai-gop"
                    class="dropdown-item py-2"
                  ><i class="bi bi-graph-up-arrow text-success me-1"></i> Báo cáo Lãi gộp</router-link>
                </li>
                <li><hr class="dropdown-divider" /></li>
                <li v-if="hasQuyen('view_ai_report')">
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
                v-if="globalState.branches.length > 1"
                class="form-select form-select-sm border-0 shadow-none fw-bold text-dark p-0"
                v-model="globalState.activeBranchId"
              >
                <option
                  v-for="b in globalState.branches"
                  :key="b.id"
                  :value="b.id"
                >
                  {{ b.tenChiNhanh }}
                </option>
              </select>
              <span v-else class="fw-bold text-dark small">
                {{ globalState.branches[0]?.tenChiNhanh || 'Chưa có chi nhánh' }}
              </span>
            </div>

            <span class="text-white me-2 fw-medium">
              <i class="bi bi-person-circle fs-5 align-middle me-1"></i>
              {{ userDisplayLabel }}
              <span v-if="isThuNgan" class="badge ms-1" style="background: rgba(255,193,7,0.3); color: #ffc107; font-size: 0.65rem;">Hạn chế</span>
            </span>

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
                  <!-- Chỉ hiển thị các mục nhạy cảm với Admin thực sự, ẩn với Thu ngân -->
                  <li v-if="!isThuNgan">
                    <router-link class="dropdown-item" to="/admin/store-info"
                      ><i class="bi bi-shop-window me-2 text-muted"></i> Thông
                      tin cửa hàng</router-link
                    >
                  </li>
                  <li>
                    <router-link class="dropdown-item" to="/admin/profile"
                      ><i class="bi bi-person-vcard me-2 text-muted"></i> Thông
                      tin cá nhân</router-link
                    >
                  </li>

                  <li v-if="!isThuNgan">
                    <router-link class="dropdown-item" to="/admin/print-setup"
                      ><i class="bi bi-printer me-2 text-muted"></i> Thiết lập
                      mẫu in</router-link
                    >
                  </li>
                  <li v-if="!isThuNgan">
                    <router-link class="dropdown-item" to="/admin/bank-setup"
                      ><i class="bi bi-qr-code-scan me-2 text-muted"></i> Thiết
                      lập chuyển khoản</router-link
                    >
                  </li>
                  <li v-if="!isThuNgan">
                    <router-link class="dropdown-item fw-bold text-warning" to="/admin/subscription"
                      ><i class="bi bi-credit-card-2-front me-2"></i> Gói dịch vụ</router-link
                    >
                  </li>
                  <li v-if="!isThuNgan">
                    <router-link class="dropdown-item fw-bold" to="/admin/thiet-lap"
                      ><i class="bi bi-gear me-2 text-muted"></i> Thiết lập hệ thống</router-link
                    >
                  </li>
                  <li v-if="!isThuNgan">
                    <router-link class="dropdown-item fw-bold text-info" to="/admin/audit-log"
                      ><i class="bi bi-journal-text me-2"></i> Nhật ký hoạt động</router-link
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

    <!-- SaaS: Banner cảnh báo hết hạn / dùng thử -->
    <div v-if="showBanner" class="subscription-banner" :class="bannerInfo.cls">
      <span>{{ bannerInfo.text }}</span>
      <router-link to="/admin/subscription" class="banner-btn">Xem gói dịch vụ →</router-link>
    </div>

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
  animation: spin 3s linear infinite;
  pointer-events: none;
}

@keyframes spin {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
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

/* 8. BANNER CẢNH BÁO GÓI DỊCH VỤ SaaS */
.subscription-banner {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 16px;
  padding: 10px 20px;
  font-size: 0.85rem;
  font-weight: 600;
  text-align: center;
  animation: slideDown 0.4s ease;
}
@keyframes slideDown {
  from { transform: translateY(-100%); opacity: 0; }
  to { transform: translateY(0); opacity: 1; }
}
.banner-info {
  background: linear-gradient(90deg, #3b82f6, #2563eb);
  color: white;
}
.banner-warning {
  background: linear-gradient(90deg, #f59e0b, #d97706);
  color: white;
}
.banner-danger {
  background: linear-gradient(90deg, #ef4444, #dc2626);
  color: white;
}
.banner-btn {
  background: rgba(255,255,255,0.2);
  color: white;
  padding: 4px 14px;
  border-radius: 20px;
  text-decoration: none;
  font-weight: 700;
  font-size: 0.8rem;
  transition: 0.2s;
  white-space: nowrap;
}
.banner-btn:hover {
  background: rgba(255,255,255,0.35);
  color: white;
}
</style>
