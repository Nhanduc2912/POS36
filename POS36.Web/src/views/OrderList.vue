<template>
  <div class="order-list-container d-flex bg-white h-100">
    <div
      class="sidebar-filter border-end bg-light p-0 d-flex flex-column"
      style="width: 260px; min-height: calc(100vh - 56px)"
    >
      <div class="filter-group border-bottom">
        <div class="filter-header bg-light text-danger fw-bold p-2 small">
          <i class="bi bi-list-ul me-1"></i> TÌM KIẾM
        </div>
        <div class="p-2">
          <input
            type="text"
            class="form-control form-control-sm mb-2 shadow-none"
            placeholder="Mã chứng từ"
            v-model="filters.maChungTu"
          />
          <div class="input-group input-group-sm mb-2">
            <input
              type="text"
              class="form-control shadow-none"
              placeholder="Tìm kiếm mặt hàng"
            />
            <button
              class="btn btn-outline-secondary bg-white text-muted border-start-0"
            >
              <i class="bi bi-search"></i>
            </button>
          </div>
          <div class="input-group input-group-sm">
            <input
              type="text"
              class="form-control shadow-none"
              placeholder="Tìm đối tác"
            />
            <button
              class="btn btn-outline-secondary bg-white text-muted border-start-0"
            >
              <i class="bi bi-search"></i>
            </button>
          </div>
        </div>
      </div>

      <div class="filter-group border-bottom">
        <div class="filter-header bg-light text-danger fw-bold p-2 small">
          <i class="bi bi-shop me-1"></i> KÊNH BÁN HÀNG
        </div>
        <div class="p-2">
          <select class="form-select form-select-sm shadow-none">
            <option>Tất cả</option>
          </select>
        </div>
      </div>

      <div class="filter-group border-bottom">
        <div class="filter-header bg-light text-danger fw-bold p-2 small">
          <i class="bi bi-funnel me-1"></i> LỌC THEO TRẠNG THÁI
        </div>
        <div class="p-2">
          <div class="form-check mb-1">
            <input
              class="form-check-input"
              type="checkbox"
              id="tt1"
              value="Đang phục vụ"
              v-model="filters.trangThai"
            />
            <label class="form-check-label small" for="tt1"
              >Đang phục vụ / Phiếu tạm</label
            >
          </div>
          <div class="form-check mb-1">
            <input
              class="form-check-input"
              type="checkbox"
              id="tt2"
              value="Đã thanh toán"
              v-model="filters.trangThai"
            />
            <label class="form-check-label small" for="tt2"
              >Hoàn thành (Đã thanh toán)</label
            >
          </div>
          <div class="form-check">
            <input
              class="form-check-input"
              type="checkbox"
              id="tt3"
              value="Đã Hủy"
              v-model="filters.trangThai"
            />
            <label class="form-check-label small" for="tt3">Hủy</label>
          </div>
        </div>
      </div>

      <div class="filter-group">
        <div class="filter-header bg-light text-danger fw-bold p-2 small">
          <i class="bi bi-calendar3 me-1"></i> LỌC THEO NGÀY
        </div>
        <div class="p-2">
          <select
            class="form-select form-select-sm shadow-none mb-2 text-danger fw-bold"
          >
            <option>Ngày bán</option>
          </select>
          <div class="form-check mb-1">
            <input
              class="form-check-input"
              type="radio"
              name="timeFilter"
              id="tf1"
              value="all"
              v-model="filters.thoiGian"
              checked
            />
            <label class="form-check-label small" for="tf1"
              >Toàn thời gian</label
            >
          </div>
          <div class="form-check mb-1">
            <input
              class="form-check-input"
              type="radio"
              name="timeFilter"
              id="tf2"
              value="today"
              v-model="filters.thoiGian"
            />
            <label class="form-check-label small" for="tf2">Hôm nay</label>
          </div>
          <div class="form-check mb-1">
            <input
              class="form-check-input"
              type="radio"
              name="timeFilter"
              id="tf3"
              value="yesterday"
              v-model="filters.thoiGian"
            />
            <label class="form-check-label small" for="tf3">Hôm qua</label>
          </div>
        </div>
      </div>
    </div>

    <div class="main-content flex-grow-1 p-3 bg-white">
      <div
        class="d-flex justify-content-between align-items-center mb-3 pb-2 border-bottom"
      >
        <h5 class="fw-bold text-danger mb-0 text-uppercase">
          <i class="bi bi-funnel-fill"></i> Danh sách đơn hàng
        </h5>
        <button class="btn btn-success btn-sm fw-bold px-3">
          <i class="bi bi-cart-plus me-1"></i> BÁN HÀNG
        </button>
      </div>

      <div class="table-responsive">
        <table
          class="table table-hover table-bordered align-middle"
          style="font-size: 0.85rem"
        >
          <thead class="table-light text-muted text-center align-middle">
            <tr>
              <th style="width: 40px"></th>
              <th class="text-start">Mã chứng từ</th>
              <th class="text-start">Khách hàng</th>
              <th>Ngày bán</th>
              <th class="text-end">Tổng cộng</th>
              <th class="text-end">Tổng thanh toán</th>
              <th>Trạng thái</th>
            </tr>
          </thead>
          <tbody>
            <tr class="fw-bold bg-light">
              <td class="text-center">Σ</td>
              <td colspan="3"></td>
              <td class="text-end text-dark">
                {{ formatPrice(totalTongCong) }}
              </td>
              <td class="text-end text-dark">
                {{ formatPrice(totalThanhToan) }}
              </td>
              <td></td>
            </tr>

            <tr v-if="loading" class="text-center">
              <td colspan="7" class="py-4">
                <div class="spinner-border text-danger spinner-border-sm"></div>
                Đang tải dữ liệu...
              </td>
            </tr>
            <tr v-else-if="filteredOrders.length === 0" class="text-center">
              <td colspan="7" class="py-4 text-muted">
                Không có dữ liệu đơn hàng nào phù hợp.
              </td>
            </tr>

            <tr
              v-else
              v-for="order in filteredOrders"
              :key="order.id"
              class="cursor-pointer"
            >
              <td class="text-center text-muted">
                <i class="bi bi-caret-right-fill"></i>
              </td>
              <td class="text-start">
                <div class="fw-bold text-dark">{{ order.maChungTu }}</div>
                <div class="small text-muted">{{ order.tenBan }}</div>
              </td>
              <td class="text-start">{{ order.khachHang }}</td>
              <td class="text-center">{{ formatDate(order.ngayBan) }}</td>
              <td class="text-end">{{ formatPrice(order.tongCong) }}</td>
              <td class="text-end">{{ formatPrice(order.tongThanhToan) }}</td>
              <td class="text-center">
                <span
                  v-if="order.trangThai === 'Đã thanh toán'"
                  class="badge bg-primary rounded-0 w-100 mb-1"
                  >Hoàn thành</span
                >
                <span
                  v-else-if="order.trangThai === 'Đang phục vụ'"
                  class="badge bg-warning text-dark rounded-0 w-100 mb-1"
                  >Đang phục vụ</span
                >
                <span v-else class="badge bg-danger rounded-0 w-100 mb-1">{{
                  order.trangThai
                }}</span>
                <div class="text-danger fw-bold" style="font-size: 0.7rem">
                  TIỀN MẶT
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed, watch } from "vue";
import axios from "axios";
import { globalState } from "../store";

const orders = ref([]);
const loading = ref(false);

// Bộ lọc
const filters = ref({
  maChungTu: "",
  trangThai: [], // Mảng chứa các trạng thái được check
  thoiGian: "all",
});

const formatPrice = (price) =>
  new Intl.NumberFormat("vi-VN").format(price || 0);

const formatDate = (dateString) => {
  const d = new Date(dateString);
  const day = String(d.getDate()).padStart(2, "0");
  const month = String(d.getMonth() + 1).padStart(2, "0");
  const year = d.getFullYear();
  const hours = String(d.getHours()).padStart(2, "0");
  const minutes = String(d.getMinutes()).padStart(2, "0");
  return `${day}/${month}/${year}\n${hours}:${minutes}`;
};

// Gọi API
const fetchOrders = async () => {
  loading.value = true;
  try {
    const branchId = globalState.value.activeBranchId || 0;
    const res = await axios.get(
      `/api/HoaDon/danh-sach-admin?chiNhanhId=${branchId}`,
    );
    orders.value = res.data;
  } catch (e) {
    console.error("Lỗi lấy danh sách đơn hàng:", e);
  } finally {
    loading.value = false;
  }
};

// Hàm tính toán dữ liệu đã được lọc (Mã, Trạng thái)
const filteredOrders = computed(() => {
  let result = orders.value;

  // 1. Lọc theo mã chứng từ
  if (filters.value.maChungTu) {
    result = result.filter((o) =>
      o.maChungTu.toLowerCase().includes(filters.value.maChungTu.toLowerCase()),
    );
  }

  // 2. Lọc theo trạng thái (nếu có chọn checkbox)
  if (filters.value.trangThai.length > 0) {
    result = result.filter((o) =>
      filters.value.trangThai.includes(o.trangThai),
    );
  }

  // 3. Lọc theo thời gian (Hôm nay, hôm qua...)
  const now = new Date();
  if (filters.value.thoiGian === "today") {
    result = result.filter(
      (o) => new Date(o.ngayBan).toDateString() === now.toDateString(),
    );
  } else if (filters.value.thoiGian === "yesterday") {
    const yesterday = new Date(now);
    yesterday.setDate(yesterday.getDate() - 1);
    result = result.filter(
      (o) => new Date(o.ngayBan).toDateString() === yesterday.toDateString(),
    );
  }

  return result;
});

// Tính tổng cho dòng Σ
const totalTongCong = computed(() =>
  filteredOrders.value.reduce((sum, item) => sum + item.tongCong, 0),
);
const totalThanhToan = computed(() =>
  filteredOrders.value.reduce((sum, item) => sum + item.tongThanhToan, 0),
);

// Tự động tải lại dữ liệu khi đổi Chi nhánh ở Navbar Admin
watch(
  () => globalState.value.activeBranchId,
  () => {
    fetchOrders();
  },
);

onMounted(() => {
  fetchOrders();
});
</script>

<style scoped>
.order-list-container {
  font-family: Arial, sans-serif;
}
.filter-header {
  border-left: 3px solid #f37021;
}
.table th {
  font-weight: 600;
  border-bottom-width: 1px;
  white-space: nowrap;
}
.table td {
  vertical-align: middle;
  white-space: pre-line; /* Để ngày và giờ rớt dòng chuẩn */
}
.cursor-pointer {
  cursor: pointer;
}
.form-check-input:checked {
  background-color: #f37021;
  border-color: #f37021;
}
/* Tuỳ chỉnh thanh cuộn cho bảng nếu quá dài */
.table-responsive {
  max-height: calc(100vh - 160px);
  overflow-y: auto;
}
</style>
