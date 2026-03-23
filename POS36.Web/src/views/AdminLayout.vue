<script setup>
import { onMounted } from "vue";
import { useRouter } from "vue-router";
import axios from "axios";
import { globalState } from "../store";

const router = useRouter();

// Hàm kéo danh sách Chi nhánh từ API gắn vào thanh Navbar
// Hàm kéo danh sách Chi nhánh từ API
const fetchBranches = async () => {
  try {
    const res = await axios.get("/api/ChiNhanh");
    globalState.value.branches = res.data;

    if (res.data.length > 0) {
      // KIỂM TRA CHỐNG LỖI NHỚ NHẦM TÀI KHOẢN CŨ
      const currentId = globalState.value.activeBranchId;
      // Kiểm tra xem currentId có nằm trong danh sách chi nhánh của User này không
      const isValid = res.data.some((b) => b.id === currentId);

      if (!currentId || !isValid) {
        // Nếu không hợp lệ hoặc chưa có, tự động lấy chi nhánh đầu tiên
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
  localStorage.clear(); // CHỐT HẠ: Quét sạch bộ nhớ không chừa một thứ gì
  window.location.href = "/login";
};

onMounted(() => {
  fetchBranches();
});
</script>

<template>
  <div class="admin-wrapper">
    <nav class="navbar navbar-expand-lg navbar-dark pos-navbar shadow-sm">
      <div class="container-fluid px-4">
        <router-link class="navbar-brand fw-bold fs-4 me-4" to="/admin"
          ><i class="bi bi-shop me-2"></i>POS36</router-link
        >

        <div class="collapse navbar-collapse" id="adminMenu">
          <ul class="navbar-nav me-auto mb-2 mb-lg-0 fw-medium">
            <li class="nav-item dropdown me-2">
              <a
                class="nav-link dropdown-toggle"
                href="#"
                data-bs-toggle="dropdown"
                ><i class="bi bi-box-seam me-1"></i> Hàng hóa</a
              >
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
                  >
                    <i class="bi bi-clipboard-check"></i> Kiểm kê
                  </router-link>
                </li>
              </ul>
            </li>
            <li class="nav-item dropdown me-2">
              <a
                class="nav-link dropdown-toggle"
                href="#"
                data-bs-toggle="dropdown"
                ><i class="bi bi-shop-window me-1"></i> Nhà hàng</a
              >
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
                ><i class="bi bi-arrow-left-right me-1"></i> Giao dịch</a
              >
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
                ><i class="bi bi-people me-1"></i> Nội bộ</a
              >
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
                ><i class="bi bi-bar-chart-line me-1"></i> Báo cáo</a
              >
              <ul class="dropdown-menu shadow-sm border-0">
                <li>
                  <a class="dropdown-item" href="#">Tổng kết cuối ngày</a>
                </li>
                <li><a class="dropdown-item" href="#">Báo cáo bán hàng</a></li>
              </ul>
            </li>
          </ul>

          <div class="d-flex align-items-center">
            <div
              class="me-4 d-flex align-items-center bg-white rounded-pill px-2 py-1"
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

            <span class="text-white me-3"
              ><i class="bi bi-person-circle fs-5"></i> Admin</span
            >
            <button
              @click="handleLogout"
              class="btn btn-sm btn-light text-danger fw-bold rounded-pill px-3"
            >
              Thoát
            </button>
          </div>
        </div>
      </div>
    </nav>
    <router-view></router-view>
  </div>
</template>

<style scoped>
.admin-wrapper {
  background-color: #f0f2f5;
  min-height: 100vh;
}
.pos-navbar {
  background-color: #f37021;
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
.dropdown-menu {
  border-radius: 8px;
  margin-top: 8px;
  border: 1px solid #eee;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1) !important;
}
.dropdown-item {
  padding: 10px 20px;
  font-weight: 500;
}
.dropdown-item:hover {
  background-color: #fff3ed;
  color: #f37021;
}
</style>
