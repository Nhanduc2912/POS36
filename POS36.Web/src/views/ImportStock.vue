<template>
  <div class="import-stock h-100 d-flex flex-column bg-light">
    <div
      class="bg-white p-3 border-bottom d-flex align-items-center justify-content-between shadow-sm z-1"
    >
      <h5 class="mb-0 text-danger fw-bold text-uppercase">NHẬP HÀNG</h5>
      <router-link
        to="/admin/import-create"
        class="btn btn-warning text-white fw-bold px-3"
      >
        <i class="bi bi-plus-lg"></i> Thêm mới
      </router-link>
    </div>

    <div class="d-flex flex-grow-1 overflow-hidden">
      <div
        class="sidebar-filter border-end bg-white d-flex flex-column"
        style="width: 360px"
      >
        <div class="p-3 border-bottom bg-light text-muted fw-bold small">
          <i class="bi bi-funnel"></i> BỘ LỌC
        </div>

        <div class="p-3 flex-grow-1 overflow-auto">
          <div class="mb-3">
            <input
              type="text"
              class="form-control"
              placeholder="Mã phiếu, NCC..."
              v-model="filter.search"
            />
          </div>

          <div class="mb-3">
            <label class="form-label small fw-bold text-secondary"
              >Thời gian</label
            >
            <select class="form-select form-select-sm mb-2" v-model="datePreset">
              <option value="today">Hôm nay</option>
              <option value="1day">1 ngày trước (Hôm qua)</option>
              <option value="3days">3 ngày trước</option>
              <option value="1month">1 tháng trước</option>
              <option value="custom">Tùy chỉnh</option>
            </select>
            <div v-if="datePreset === 'custom'" class="d-flex gap-2 align-items-center">
              <input
                type="date"
                class="form-control form-control-sm"
                v-model="filter.startDate"
              />
              <span class="small text-muted">đến</span>
              <input
                type="date"
                class="form-control form-control-sm"
                v-model="filter.endDate"
              />
            </div>
            <div v-else class="text-muted small ps-1" style="font-size: 0.75rem;">
              Khoảng lọc: {{ formatDate(filter.startDate) }} - {{ formatDate(filter.endDate) }}
            </div>
          </div>

          <div class="mb-3">
            <label class="form-label small fw-bold text-secondary"
              >Trạng thái</label
            >
            <select class="form-select form-select-sm" v-model="filter.status">
              <option value="">Tất cả</option>
              <option value="Đang xử lý">Đang xử lý (Nháp)</option>
              <option value="Hoàn thành">Hoàn thành</option>
            </select>
          </div>
        </div>

        <div class="p-3 border-top bg-light">
          <button
            class="btn btn-primary btn-sm w-100 fw-bold"
            @click="fetchImports"
          >
            <i class="bi bi-search"></i> Tìm kiếm
          </button>
        </div>
      </div>

      <div class="flex-grow-1 bg-white overflow-auto p-3">
        <table
          class="table table-hover table-bordered align-middle"
          style="font-size: 0.9rem"
        >
          <thead class="table-light text-muted">
            <tr>
              <th class="text-center" style="width: 50px">#</th>
              <th>Mã phiếu</th>
              <th>Ngày nhập</th>
              <th>Nhà cung cấp</th>
              <th class="text-end">Tổng tiền</th>
              <th class="text-center">Trạng thái</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading" class="text-center">
              <td colspan="6" class="py-4">
                <div class="spinner-border text-danger spinner-border-sm"></div>
              </td>
            </tr>
            <tr v-else-if="imports.length === 0" class="text-center">
              <td colspan="6" class="py-4 text-muted">
                Không tìm thấy phiếu nhập nào.
              </td>
            </tr>
            <template v-else v-for="(p, index) in imports" :key="p.id">
              <tr class="cursor-pointer" @click="toggleDetail(p.id)">
                <td class="text-center text-muted">{{ index + 1 }}</td>
                <td class="fw-bold text-primary">{{ p.maChungTu }}</td>
                <td>{{ formatDate(p.ngayNhap) }}</td>
                <td>{{ parseNhaCungCap(p.ghiChu) }}</td>
                <td class="text-end fw-bold text-danger">
                  {{ formatPrice(p.tongTien) }}
                </td>
                <td class="text-center">
                  <span
                    class="badge px-3 py-1"
                    :class="
                      p.trangThai === 'Hoàn thành'
                        ? 'bg-success'
                        : 'bg-warning text-dark'
                    "
                  >
                    {{ p.trangThai }}
                  </span>
                </td>
              </tr>
              <tr v-if="expandedRowId === p.id" class="bg-light detail-row">
                <td colspan="6" class="p-0 border-start border-4 border-danger">
                  <div class="p-3">
                    <div class="mb-3 d-flex flex-wrap gap-4 bg-white p-2.5 rounded border">
                      <div>
                        <span class="text-secondary fw-bold small">Nhà cung cấp:</span>
                        <span class="ms-2 fw-bold text-dark">{{ parseNhaCungCap(p.ghiChu) }}</span>
                      </div>
                      <div>
                        <span class="text-secondary fw-bold small">Ghi chú:</span>
                        <span class="ms-2 text-dark">{{ parseGhiChuOnly(p.ghiChu) || '---' }}</span>
                      </div>
                    </div>
                    <h6 class="fw-bold mb-3">CHI TIẾT MẶT HÀNG</h6>
                    <table
                      class="table table-sm table-white table-bordered bg-white mb-0"
                      style="font-size: 0.85rem"
                    >
                      <thead class="text-muted">
                        <tr>
                          <th>Tên hàng hóa</th>
                          <th class="text-center">SL</th>
                          <th class="text-end">Giá nhập</th>
                          <th class="text-end">Thành tiền</th>
                        </tr>
                      </thead>
                      <tbody>
                        <tr v-for="mon in p.chiTiets" :key="mon.sanPhamId">
                          <td class="fw-bold">{{ mon.tenSanPham }}</td>
                          <td class="text-center">{{ mon.soLuong }}</td>
                          <td class="text-end text-primary">
                            {{ formatPrice(mon.donGiaNhap) }}
                          </td>
                          <td class="text-end fw-bold text-danger">
                            {{ formatPrice(mon.soLuong * mon.donGiaNhap) }}
                          </td>
                        </tr>
                      </tbody>
                    </table>
                  </div>
                </td>
              </tr>
            </template>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from "vue";
import axios from "axios";
import { globalState } from "../store";

const imports = ref([]);
const loading = ref(false);
const expandedRowId = ref(null);
const datePreset = ref("1month");

// Bộ lọc
const filter = ref({
  search: "",
  startDate: new Date(new Date().setMonth(new Date().getMonth() - 1)).toISOString().split("T")[0], // Mặc định 1 tháng trước
  endDate: new Date().toISOString().split("T")[0], // Hôm nay
  status: "",
});

const formatPrice = (price) =>
  new Intl.NumberFormat("vi-VN").format(price || 0);
const formatDate = (dateString) =>
  new Date(dateString).toLocaleDateString("vi-VN");

// Backend lưu NCC vào ghi chú, ta tách ra để hiển thị
const parseNhaCungCap = (ghiChu) => {
  if (!ghiChu) return "Nhà cung cấp lẻ";
  if (ghiChu.startsWith("Nhà CC:") && ghiChu.includes(" | Ghi chú:")) {
    return ghiChu.split(" | Ghi chú:")[0].replace("Nhà CC: ", "") || "Nhà cung cấp lẻ";
  }
  return "Nhà cung cấp lẻ";
};

const parseGhiChuOnly = (ghiChu) => {
  if (!ghiChu) return "";
  if (ghiChu.startsWith("Nhà CC:") && ghiChu.includes(" | Ghi chú:")) {
    return ghiChu.split(" | Ghi chú:")[1] || "";
  }
  return ghiChu;
};

// GỌI API LẤY DANH SÁCH (Ráp vào API thật của em)
const fetchImports = async () => {
  if (!globalState.value.activeBranchId) return;
  loading.value = true;
  expandedRowId.value = null; // Đóng hết chi tiết cũ
  try {
    // Gắn query filter vào URL
    const res = await axios.get(
      `/api/NhapHang/danh-sach?chiNhanhId=${globalState.value.activeBranchId}&search=${filter.value.search}&startDate=${filter.value.startDate}&endDate=${filter.value.endDate}&status=${filter.value.status}`,
    );
    imports.value = res.data;
  } catch (e) {
    console.error("Lỗi tải phiếu nhập", e);
  } finally {
    loading.value = false;
  }
};

const toggleDetail = (id) => {
  expandedRowId.value = expandedRowId.value === id ? null : id;
};

// Watch preset thay đổi
watch(datePreset, (newVal) => {
  if (newVal === "custom") return;
  
  const today = new Date();
  const endDateStr = today.toISOString().split("T")[0];
  let startDateStr = endDateStr;
  
  if (newVal === "today") {
    startDateStr = endDateStr;
  } else if (newVal === "1day") {
    const yesterday = new Date();
    yesterday.setDate(today.getDate() - 1);
    startDateStr = yesterday.toISOString().split("T")[0];
  } else if (newVal === "3days") {
    const threeDaysAgo = new Date();
    threeDaysAgo.setDate(today.getDate() - 3);
    startDateStr = threeDaysAgo.toISOString().split("T")[0];
  } else if (newVal === "1month") {
    const oneMonthAgo = new Date();
    oneMonthAgo.setMonth(today.getMonth() - 1);
    startDateStr = oneMonthAgo.toISOString().split("T")[0];
  }
  
  filter.value.startDate = startDateStr;
  filter.value.endDate = endDateStr;
  
  fetchImports();
});

// Tự động load lại khi đổi chi nhánh hoặc đổi status trên bộ lọc
watch(() => globalState.value.activeBranchId, fetchImports);
watch(() => filter.value.status, fetchImports);

onMounted(fetchImports);
</script>

<style scoped>
.sidebar-filter {
  border-right: 1px solid #dee2e6;
}
.cursor-pointer {
  cursor: pointer;
}
.detail-row td {
  box-shadow: inset 0 3px 6px -3px rgba(0, 0, 0, 0.1);
}
</style>
