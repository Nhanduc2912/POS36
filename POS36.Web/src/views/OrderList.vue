<template>
  <div class="order-history h-100 d-flex flex-column bg-light">
    <div
      class="bg-white p-3 border-bottom d-flex align-items-center justify-content-between shadow-sm z-1"
    >
      <h5 class="mb-0 text-danger fw-bold text-uppercase">
        <i class="bi bi-funnel-fill me-2"></i> DANH SÁCH ĐƠN HÀNG
      </h5>
    </div>

    <div class="d-flex flex-grow-1 overflow-hidden">
      <div
        class="sidebar-filter border-end bg-white d-flex flex-column"
        style="width: 280px; overflow-y: auto"
      >
        <div class="p-3 border-bottom">
          <div class="text-danger fw-bold small mb-2">
            <i class="bi bi-search me-1"></i> TÌM KIẾM
          </div>
          <input
            type="text"
            class="form-control form-control-sm shadow-none"
            placeholder="Mã chứng từ..."
            v-model="filter.search"
            @keyup.enter="fetchOrders"
          />
        </div>

        <div class="p-3 border-bottom">
          <div class="text-danger fw-bold small mb-2">
            <i class="bi bi-filter-circle me-1"></i> LỌC THEO TRẠNG THÁI
          </div>
          <div class="form-check mb-2">
            <input
              class="form-check-input shadow-none cursor-pointer"
              type="radio"
              v-model="filter.status"
              value=""
              id="s0"
            />
            <label class="form-check-label small cursor-pointer" for="s0"
              >Tất cả</label
            >
          </div>
          <div class="form-check mb-2">
            <input
              class="form-check-input shadow-none cursor-pointer"
              type="radio"
              v-model="filter.status"
              value="Đang phục vụ"
              id="s1"
            />
            <label class="form-check-label small cursor-pointer" for="s1"
              >Đang phục vụ / Phiếu tạm</label
            >
          </div>
          <div class="form-check mb-2">
            <input
              class="form-check-input shadow-none cursor-pointer"
              type="radio"
              v-model="filter.status"
              value="Đã thanh toán"
              id="s2"
            />
            <label class="form-check-label small cursor-pointer" for="s2"
              >Hoàn thành (Đã thanh toán)</label
            >
          </div>
          <div class="form-check">
            <input
              class="form-check-input shadow-none cursor-pointer"
              type="radio"
              v-model="filter.status"
              value="Đã hủy"
              id="s3"
            />
            <label class="form-check-label small cursor-pointer" for="s3"
              >Hủy</label
            >
          </div>
        </div>

        <div class="p-3">
          <div class="text-danger fw-bold small mb-2">
            <i class="bi bi-calendar-event me-1"></i> LỌC THEO NGÀY
          </div>

          <div class="d-flex flex-column gap-2 mb-3">
            <div class="form-check">
              <input
                class="form-check-input shadow-none cursor-pointer"
                type="radio"
                v-model="datePreset"
                value="all"
                id="d0"
                @change="applyDatePreset"
              />
              <label class="form-check-label small cursor-pointer" for="d0"
                >Toàn thời gian</label
              >
            </div>
            <div class="form-check">
              <input
                class="form-check-input shadow-none cursor-pointer"
                type="radio"
                v-model="datePreset"
                value="today"
                id="d1"
                @change="applyDatePreset"
              />
              <label class="form-check-label small cursor-pointer" for="d1"
                >Hôm nay</label
              >
            </div>
            <div class="form-check">
              <input
                class="form-check-input shadow-none cursor-pointer"
                type="radio"
                v-model="datePreset"
                value="yesterday"
                id="d2"
                @change="applyDatePreset"
              />
              <label class="form-check-label small cursor-pointer" for="d2"
                >Hôm qua</label
              >
            </div>
            <div class="form-check">
              <input
                class="form-check-input shadow-none cursor-pointer"
                type="radio"
                v-model="datePreset"
                value="3days"
                id="d3"
                @change="applyDatePreset"
              />
              <label class="form-check-label small cursor-pointer" for="d3"
                >3 ngày trước</label
              >
            </div>
            <div class="form-check">
              <input
                class="form-check-input shadow-none cursor-pointer"
                type="radio"
                v-model="datePreset"
                value="7days"
                id="d4"
                @change="applyDatePreset"
              />
              <label class="form-check-label small cursor-pointer" for="d4"
                >7 ngày trước</label
              >
            </div>
            <div class="form-check">
              <input
                class="form-check-input shadow-none cursor-pointer"
                type="radio"
                v-model="datePreset"
                value="thisMonth"
                id="d5"
                @change="applyDatePreset"
              />
              <label class="form-check-label small cursor-pointer" for="d5"
                >Tháng này</label
              >
            </div>
            <div class="form-check">
              <input
                class="form-check-input shadow-none cursor-pointer"
                type="radio"
                v-model="datePreset"
                value="lastMonth"
                id="d6"
                @change="applyDatePreset"
              />
              <label class="form-check-label small cursor-pointer" for="d6"
                >Tháng trước</label
              >
            </div>
            <div class="form-check">
              <input
                class="form-check-input shadow-none cursor-pointer"
                type="radio"
                v-model="datePreset"
                value="custom"
                id="d7"
              />
              <label class="form-check-label small cursor-pointer" for="d7"
                >Lựa chọn khác</label
              >
            </div>
          </div>

          <div v-if="datePreset === 'custom'" class="border-top pt-2 mb-3">
            <input
              type="date"
              class="form-control form-control-sm mb-2 shadow-none"
              v-model="filter.startDate"
            />
            <input
              type="date"
              class="form-control form-control-sm shadow-none"
              v-model="filter.endDate"
            />
          </div>

          <button
            class="btn btn-primary w-100 fw-bold shadow-sm"
            @click="fetchOrders"
          >
            <i class="bi bi-search"></i> Lọc dữ liệu
          </button>
        </div>
      </div>

      <div class="flex-grow-1 bg-white d-flex flex-column overflow-hidden">
        <div class="flex-grow-1 overflow-auto p-3">
          <table
            class="table table-hover table-bordered align-middle mb-0"
            style="font-size: 0.85rem"
          >
            <thead class="table-light text-muted align-middle">
              <tr>
                <th class="text-center" style="width: 40px"></th>
                <th>Mã chứng từ</th>
                <th>Khách hàng</th>
                <th class="text-center">Ngày bán</th>
                <th class="text-end">Tổng cộng</th>
                <th class="text-end">Tổng thanh toán</th>
                <th class="text-center" style="width: 150px">Trạng thái</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="loading" class="text-center">
                <td colspan="7" class="py-4">
                  <div
                    class="spinner-border text-primary spinner-border-sm"
                  ></div>
                </td>
              </tr>
              <tr v-else-if="paginatedOrders.length === 0" class="text-center">
                <td colspan="7" class="py-5 text-muted">
                  Không tìm thấy đơn hàng nào.
                </td>
              </tr>

              <template v-else v-for="o in paginatedOrders" :key="o.id">
                <tr
                  class="cursor-pointer"
                  @click="toggleDetail(o.id)"
                  :class="{ 'bg-light': expandedRowId === o.id }"
                >
                  <td class="text-center text-muted">
                    <i
                      class="bi"
                      :class="
                        expandedRowId === o.id
                          ? 'bi-caret-down-fill text-dark'
                          : 'bi-caret-right-fill'
                      "
                    ></i>
                  </td>
                  <td>
                    <div class="fw-bold text-dark">{{ o.maChungTu }}</div>
                    <div class="small text-muted">{{ o.tenBan }}</div>
                  </td>
                  <td>{{ o.khachHang }}</td>
                  <td class="text-center text-muted">
                    <div>{{ formatDateOnly(o.ngayBan) }}</div>
                    <div class="small">{{ formatTimeOnly(o.ngayBan) }}</div>
                  </td>
                  <td class="text-end fw-bold text-dark">
                    {{ formatPrice(o.tongCong) }}
                  </td>
                  <td class="text-end fw-bold text-dark">
                    {{ formatPrice(o.tongThanhToan) }}
                  </td>
                  <td class="text-center">
                    <div
                      class="badge w-100 py-2 rounded-1 fw-bold"
                      :class="
                        o.trangThai === 'Đã thanh toán'
                          ? 'bg-primary'
                          : 'bg-secondary'
                      "
                    >
                      {{
                        o.trangThai === "Đã thanh toán"
                          ? "Hoàn thành"
                          : o.trangThai
                      }}
                    </div>
                    <div
                      class="small text-danger fw-bold mt-1"
                      v-if="o.trangThai === 'Đã thanh toán'"
                      style="font-size: 0.7rem"
                    >
                      TIỀN MẶT
                    </div>
                  </td>
                </tr>

                <tr v-if="expandedRowId === o.id" class="bg-light detail-row">
                  <td
                    colspan="7"
                    class="p-0 border-start border-danger border-4"
                  >
                    <div class="p-3">
                      <ul
                        class="nav nav-tabs mb-3"
                        style="border-bottom: 2px solid #dc3545"
                      >
                        <li class="nav-item">
                          <a
                            class="nav-link active fw-bold text-danger border-danger border-bottom-0 py-1"
                            >Chi tiết</a
                          >
                        </li>
                      </ul>

                      <div
                        class="row g-3 bg-white p-3 border mb-3 mx-0 shadow-sm rounded-2"
                      >
                        <div class="col-md-6">
                          <div class="d-flex mb-2">
                            <div class="fw-bold text-muted w-25">
                              Mã chứng từ
                            </div>
                            <div class="fw-bold">{{ o.maChungTu }}</div>
                          </div>
                          <div class="d-flex mb-2">
                            <div class="fw-bold text-muted w-25">Ngày tạo</div>
                            <div>{{ formatDate(o.ngayBan) }}</div>
                          </div>
                          <div class="d-flex mb-2">
                            <div class="fw-bold text-muted w-25">
                              Trạng thái
                            </div>
                            <div>
                              <span class="badge bg-primary">{{
                                o.trangThai === "Đã thanh toán"
                                  ? "Hoàn thành"
                                  : o.trangThai
                              }}</span>
                            </div>
                          </div>
                        </div>
                        <div class="col-md-6">
                          <div class="d-flex mb-2">
                            <div class="fw-bold text-muted w-25">Người bán</div>
                            <div>Thu ngân</div>
                          </div>
                          <div class="d-flex mb-2">
                            <div class="fw-bold text-muted w-25">
                              Khách hàng
                            </div>
                            <div class="text-danger fw-bold">
                              <i class="bi bi-exclamation-triangle-fill"></i>
                              Chưa có thông tin
                            </div>
                          </div>
                        </div>
                      </div>

                      <div class="mb-2">
                        <button
                          @click="exportToExcel(o)"
                          class="btn btn-outline-primary btn-sm fw-bold bg-white"
                        >
                          <i class="bi bi-file-earmark-excel-fill"></i> EXPORT
                          TO EXCEL
                        </button>
                      </div>

                      <table
                        class="table table-sm table-bordered bg-white mb-3 shadow-sm"
                        style="font-size: 0.85rem"
                      >
                        <thead class="table-light text-muted text-center">
                          <tr>
                            <th class="text-start">Tên hàng hóa</th>
                            <th style="width: 100px">Số lượng</th>
                            <th class="text-end" style="width: 150px">
                              Đơn giá
                            </th>
                            <th class="text-end" style="width: 150px">
                              Thành tiền
                            </th>
                          </tr>
                        </thead>
                        <tbody>
                          <tr v-for="(mon, idx) in o.chiTiets" :key="idx">
                            <td class="fw-bold">{{ mon.tenSanPham }}</td>
                            <td class="text-center text-danger fw-bold">
                              {{ mon.soLuong }}
                            </td>
                            <td class="text-end text-muted">
                              {{ formatPrice(mon.donGia) }}
                            </td>
                            <td class="text-end fw-bold">
                              {{ formatPrice(mon.thanhTien) }}
                            </td>
                          </tr>
                        </tbody>
                      </table>

                      <div
                        class="d-flex justify-content-end bg-white p-2 border rounded-2"
                      >
                        <div style="width: 300px">
                          <div class="d-flex justify-content-between mb-2">
                            <span class="fw-bold text-muted">Tổng cộng</span
                            ><span class="fw-bold">{{
                              formatPrice(o.tongCong)
                            }}</span>
                          </div>
                          <div class="d-flex justify-content-between">
                            <span class="fw-bold text-muted"
                              >Tổng thanh toán</span
                            ><span class="fw-bold">{{
                              formatPrice(o.tongThanhToan)
                            }}</span>
                          </div>
                        </div>
                      </div>
                    </div>
                  </td>
                </tr>
              </template>
            </tbody>
          </table>
        </div>

        <div
          class="p-3 border-top bg-light d-flex justify-content-between align-items-center"
        >
          <span class="small text-muted fw-bold"
            >Hiển thị
            {{ orders.length > 0 ? (currentPage - 1) * pageSize + 1 : 0 }} -
            {{ Math.min(currentPage * pageSize, orders.length) }} /
            {{ orders.length }} đơn hàng</span
          >
          <div class="btn-group shadow-sm">
            <button
              class="btn btn-sm btn-white border bg-white"
              :disabled="currentPage === 1"
              @click="currentPage--"
            >
              <i class="bi bi-chevron-left"></i>
            </button>
            <button
              class="btn btn-sm btn-white border bg-white fw-bold text-primary px-3"
            >
              Trang {{ currentPage }} / {{ totalPages > 0 ? totalPages : 1 }}
            </button>
            <button
              class="btn btn-sm btn-white border bg-white"
              :disabled="currentPage === totalPages || totalPages === 0"
              @click="currentPage++"
            >
              <i class="bi bi-chevron-right"></i>
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from "vue";
import axios from "axios";
import { globalState } from "../store";

const orders = ref([]);
const loading = ref(false);
const expandedRowId = ref(null);

const currentPage = ref(1);
const pageSize = ref(10);

// Biến lưu preset lọc ngày
const datePreset = ref("today");

const filter = ref({
  search: "",
  startDate: "",
  endDate: "",
  status: "",
});

// --- FORMAT FORMAT FORMAT ---
const formatPrice = (price) =>
  new Intl.NumberFormat("vi-VN").format(price || 0);
const formatDate = (dateString) => {
  const d = new Date(dateString);
  return `${String(d.getDate()).padStart(2, "0")}/${String(d.getMonth() + 1).padStart(2, "0")}/${d.getFullYear()} ${String(d.getHours()).padStart(2, "0")}:${String(d.getMinutes()).padStart(2, "0")}`;
};
const formatDateOnly = (dateString) => {
  const d = new Date(dateString);
  return `${String(d.getDate()).padStart(2, "0")}/${String(d.getMonth() + 1).padStart(2, "0")}/${d.getFullYear()}`;
};
const formatTimeOnly = (dateString) => {
  const d = new Date(dateString);
  return `${String(d.getHours()).padStart(2, "0")}:${String(d.getMinutes()).padStart(2, "0")}`;
};

// --- LOGIC LỌC NGÀY TỰ ĐỘNG ---
const applyDatePreset = () => {
  const today = new Date();
  // Hàm lấy chuỗi YYYY-MM-DD theo múi giờ Local
  const formatYMD = (date) => {
    const d = new Date(date);
    d.setMinutes(d.getMinutes() - d.getTimezoneOffset());
    return d.toISOString().split("T")[0];
  };

  if (datePreset.value === "all") {
    filter.value.startDate = "";
    filter.value.endDate = "";
  } else if (datePreset.value === "today") {
    filter.value.startDate = formatYMD(today);
    filter.value.endDate = formatYMD(today);
  } else if (datePreset.value === "yesterday") {
    const yesterday = new Date(today);
    yesterday.setDate(yesterday.getDate() - 1);
    filter.value.startDate = formatYMD(yesterday);
    filter.value.endDate = formatYMD(yesterday);
  } else if (datePreset.value === "3days") {
    const past = new Date(today);
    past.setDate(past.getDate() - 3);
    filter.value.startDate = formatYMD(past);
    filter.value.endDate = formatYMD(today);
  } else if (datePreset.value === "7days") {
    const past = new Date(today);
    past.setDate(past.getDate() - 7);
    filter.value.startDate = formatYMD(past);
    filter.value.endDate = formatYMD(today);
  } else if (datePreset.value === "thisMonth") {
    const firstDay = new Date(today.getFullYear(), today.getMonth(), 1);
    filter.value.startDate = formatYMD(firstDay);
    filter.value.endDate = formatYMD(today);
  } else if (datePreset.value === "lastMonth") {
    const firstDay = new Date(today.getFullYear(), today.getMonth() - 1, 1);
    const lastDay = new Date(today.getFullYear(), today.getMonth(), 0);
    filter.value.startDate = formatYMD(firstDay);
    filter.value.endDate = formatYMD(lastDay);
  }
};

const totalPages = computed(() =>
  Math.ceil(orders.value.length / pageSize.value),
);
const paginatedOrders = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value;
  const end = start + pageSize.value;
  return orders.value.slice(start, end);
});

// Gọi API
const fetchOrders = async () => {
  if (!globalState.value.activeBranchId) return;
  loading.value = true;
  expandedRowId.value = null;
  currentPage.value = 1; // Reset về trang 1
  try {
    const res = await axios.get(
      `/api/HoaDon/danh-sach-admin?chiNhanhId=${globalState.value.activeBranchId}&search=${filter.value.search}&status=${filter.value.status}&startDate=${filter.value.startDate}&endDate=${filter.value.endDate}`,
    );
    orders.value = res.data;
  } catch (e) {
    console.error("Lỗi tải đơn hàng", e);
  } finally {
    loading.value = false;
  }
};

const toggleDetail = (id) => {
  expandedRowId.value = expandedRowId.value === id ? null : id;
};

const exportToExcel = (order) => {
  let csvContent = "data:text/csv;charset=utf-8,\uFEFF";
  csvContent +=
    "Ma Chung Tu,Ngay Ban,Ten Hang Hoa,So Luong,Don Gia,Thanh Tien\r\n";
  order.chiTiets.forEach((mon) => {
    let row = `${order.maChungTu},${formatDate(order.ngayBan)},"${mon.tenSanPham}",${mon.soLuong},${mon.donGia},${mon.thanhTien}`;
    csvContent += row + "\r\n";
  });

  const encodedUri = encodeURI(csvContent);
  const link = document.createElement("a");
  link.setAttribute("href", encodedUri);
  link.setAttribute("download", `${order.maChungTu}_export.csv`);
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
};

watch(() => globalState.value.activeBranchId, fetchOrders);

onMounted(() => {
  // Mặc định load "Toàn thời gian" hoặc "Hôm nay" tùy em, anh để all
  applyDatePreset();
  fetchOrders();
});
</script>

<style scoped>
.sidebar-filter {
  border-right: 1px solid #dee2e6;
}
.cursor-pointer {
  cursor: pointer;
}
.detail-row td {
  box-shadow: inset 0 3px 6px -3px rgba(0, 0, 0, 0.15);
}
</style>
