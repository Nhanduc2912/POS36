<template>
  <div class="inventory-check-container d-flex bg-white h-100">
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
              value="Đang xử lý"
              v-model="filters.trangThai"
            /><label class="form-check-label small" for="tt1">Đang xử lý</label>
          </div>
          <div class="form-check mb-1">
            <input
              class="form-check-input"
              type="checkbox"
              id="tt2"
              value="Hoàn thành"
              v-model="filters.trangThai"
            /><label class="form-check-label small" for="tt2">Hoàn thành</label>
          </div>
        </div>
      </div>
    </div>

    <div class="main-content flex-grow-1 p-3 bg-white">
      <div
        class="d-flex justify-content-between align-items-center mb-3 pb-2 border-bottom"
      >
        <h5 class="fw-bold text-danger mb-0 text-uppercase">
          <i class="bi bi-funnel-fill"></i> Kiểm kê
        </h5>
        <router-link
          to="/admin/inventory-create"
          class="btn btn-warning text-white fw-bold btn-sm px-3"
        >
          <i class="bi bi-plus-circle me-1"></i> THÊM MỚI PHIẾU KIỂM KÊ
        </router-link>
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
              <th class="text-start">Ghi chú</th>
              <th>Ngày tạo</th>
              <th>Ngày cân bằng kho</th>
              <th>Trạng thái</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading" class="text-center">
              <td colspan="6" class="py-4">
                <div class="spinner-border text-danger spinner-border-sm"></div>
              </td>
            </tr>
            <tr v-else-if="filteredVouchers.length === 0" class="text-center">
              <td colspan="6" class="py-4 text-muted">Không có phiếu nào.</td>
            </tr>

            <template v-else v-for="v in filteredVouchers" :key="v.id">
              <tr class="cursor-pointer" @click="toggleDetail(v.id)">
                <td class="text-center text-muted">
                  <i
                    class="bi"
                    :class="
                      expandedVoucherId === v.id
                        ? 'bi-caret-down-fill text-danger'
                        : 'bi-caret-right-fill'
                    "
                  ></i>
                </td>
                <td class="fw-bold text-dark text-start">{{ v.maChungTu }}</td>
                <td class="text-start">{{ v.ghiChu || "---" }}</td>
                <td class="text-center">{{ formatDate(v.ngayTao) }}</td>
                <td class="text-center">
                  {{
                    v.trangThai === "Hoàn thành" ? formatDate(v.ngayTao) : "---"
                  }}
                </td>
                <td class="text-center">
                  <span
                    :class="
                      v.trangThai === 'Hoàn thành'
                        ? 'badge bg-primary'
                        : 'badge bg-warning text-dark'
                    "
                    >{{ v.trangThai }}</span
                  >
                </td>
              </tr>

              <tr v-if="expandedVoucherId === v.id" class="detail-row bg-light">
                <td colspan="6" class="p-0">
                  <div class="p-3 border-start border-danger border-4">
                    <ul
                      class="nav nav-tabs mb-3"
                      style="border-bottom: 2px solid #dc3545"
                    >
                      <li class="nav-item">
                        <a
                          class="nav-link active fw-bold text-danger border-danger border-bottom-0"
                          >Chi tiết</a
                        >
                      </li>
                    </ul>

                    <div v-if="detailLoading" class="text-center py-3">
                      <div
                        class="spinner-border text-danger spinner-border-sm"
                      ></div>
                      Đang tải...
                    </div>

                    <div v-else-if="voucherDetails">
                      <div class="row mb-3 gx-5">
                        <div class="col-md-4">
                          <p class="mb-1">
                            <span class="fw-bold me-2">Mã chứng từ:</span>
                            {{ voucherDetails.maChungTu }}
                          </p>
                          <p class="mb-1">
                            <span class="fw-bold me-2">Ngày tạo:</span>
                            {{ formatDate(voucherDetails.ngayTao) }}
                          </p>
                          <p class="mb-1">
                            <span class="fw-bold me-2">Trạng thái:</span>
                            <span class="badge bg-primary">{{
                              voucherDetails.trangThai
                            }}</span>
                          </p>
                        </div>
                        <div class="col-md-4">
                          <p class="mb-1">
                            <span class="fw-bold me-2">Người tạo:</span> Admin
                          </p>
                          <p class="mb-1">
                            <span class="fw-bold me-2">Ghi chú:</span>
                            {{ voucherDetails.ghiChu || "---" }}
                          </p>
                        </div>
                      </div>

                      <button
                        @click="exportExcel(voucherDetails)"
                        class="btn btn-primary btn-sm mb-2 fw-bold rounded-0"
                      >
                        <i class="bi bi-file-earmark-excel"></i> EXPORT TO EXCEL
                      </button>

                      <table
                        class="table table-sm table-bordered bg-white mb-0"
                      >
                        <thead class="table-light text-muted">
                          <tr>
                            <th>Mã hàng hóa</th>
                            <th>Tên hàng hóa</th>
                            <th class="text-end">Tồn kho</th>
                            <th class="text-end">SL Kiểm kê</th>
                            <th class="text-end">Độ Lệch</th>
                          </tr>
                        </thead>
                        <tbody>
                          <tr class="fw-bold bg-light">
                            <td>Σ</td>
                            <td></td>
                            <td class="text-end">{{ totalDetailTonKho }}</td>
                            <td class="text-end">{{ totalDetailKiemKe }}</td>
                            <td class="text-end text-danger">
                              {{ totalDetailKiemKe - totalDetailTonKho }}
                            </td>
                          </tr>
                          <tr
                            v-for="item in voucherDetails.chiTiets"
                            :key="item.sanPhamId"
                          >
                            <td class="fw-bold text-danger">
                              {{ item.maSanPham }}
                            </td>
                            <td>{{ item.tenSanPham }}</td>
                            <td class="text-end">{{ item.tonKhoHienTai }}</td>
                            <td class="text-end fw-bold text-primary">
                              {{ item.soLuongKiemKe }}
                            </td>
                            <td class="text-end">
                              <span
                                v-if="
                                  item.soLuongKiemKe - item.tonKhoHienTai !== 0
                                "
                                class="badge bg-warning text-dark"
                              >
                                {{
                                  item.soLuongKiemKe - item.tonKhoHienTai > 0
                                    ? "+"
                                    : ""
                                }}{{ item.soLuongKiemKe - item.tonKhoHienTai }}
                              </span>
                              <span v-else>-</span>
                            </td>
                          </tr>
                        </tbody>
                      </table>
                    </div>
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
import { ref, onMounted, computed, watch } from "vue";
import axios from "axios";
import { globalState } from "../store";
import * as XLSX from "xlsx"; // THÊM DÒNG IMPORT NÀY
const vouchers = ref([]);
const loading = ref(false);
const filters = ref({ maChungTu: "", trangThai: [], thoiGian: "all" });

// State cho chức năng Dropdown Detail
const expandedVoucherId = ref(null);
const voucherDetails = ref(null);
const detailLoading = ref(false);

const formatDate = (dateString) => {
  const d = new Date(dateString);
  return `${String(d.getDate()).padStart(2, "0")}/${String(d.getMonth() + 1).padStart(2, "0")}/${d.getFullYear()} ${String(d.getHours()).padStart(2, "0")}:${String(d.getMinutes()).padStart(2, "0")}`;
};

const fetchVouchers = async () => {
  loading.value = true;
  try {
    const res = await axios.get(
      `/api/KiemKe/danh-sach?chiNhanhId=${globalState.value.activeBranchId || 0}`,
    );
    vouchers.value = res.data;
  } catch (e) {
    console.error(e);
  } finally {
    loading.value = false;
  }
};

const filteredVouchers = computed(() => {
  let result = vouchers.value;
  if (filters.value.maChungTu)
    result = result.filter((v) =>
      v.maChungTu.toLowerCase().includes(filters.value.maChungTu.toLowerCase()),
    );
  if (filters.value.trangThai.length > 0)
    result = result.filter((v) =>
      filters.value.trangThai.includes(v.trangThai),
    );
  return result;
});

// Logic đóng mở chi tiết
const toggleDetail = async (id) => {
  if (expandedVoucherId.value === id) {
    expandedVoucherId.value = null; // Bấm lại thì thu gọn
    return;
  }
  expandedVoucherId.value = id;
  detailLoading.value = true;
  try {
    const res = await axios.get(`/api/KiemKe/${id}`);
    voucherDetails.value = res.data;
  } catch (e) {
    console.error(e);
  } finally {
    detailLoading.value = false;
  }
};

// Tính tổng chi tiết
const totalDetailTonKho = computed(
  () =>
    voucherDetails.value?.chiTiets.reduce(
      (sum, item) => sum + item.tonKhoHienTai,
      0,
    ) || 0,
);
const totalDetailKiemKe = computed(
  () =>
    voucherDetails.value?.chiTiets.reduce(
      (sum, item) => sum + item.soLuongKiemKe,
      0,
    ) || 0,
);

watch(() => globalState.value.activeBranchId, fetchVouchers);
onMounted(fetchVouchers);

const exportExcel = (voucher) => {
  if (!voucher || !voucher.chiTiets) {
    return;
  }

  // 1. Chuẩn bị dữ liệu: Biến mảng JSON thành định dạng cột của Excel
  const excelData = voucher.chiTiets.map((item, index) => ({
    STT: index + 1,
    "Mã hàng hóa": item.maSanPham,
    "Tên hàng hóa": item.tenSanPham,
    "Tồn kho hệ thống": item.tonKhoHienTai,
    "SL Kiểm kê thực tế": item.soLuongKiemKe,
    "Độ lệch": item.soLuongKiemKe - item.tonKhoHienTai,
  }));

  // 2. Thêm dòng Tổng cộng vào cuối mảng dữ liệu
  excelData.push({
    STT: "TỔNG",
    "Mã hàng hóa": "",
    "Tên hàng hóa": "",
    "Tồn kho hệ thống": totalDetailTonKho.value,
    "SL Kiểm kê thực tế": totalDetailKiemKe.value,
    "Độ lệch": totalDetailKiemKe.value - totalDetailTonKho.value,
  });

  // 3. Khởi tạo Worksheet và Workbook
  const worksheet = XLSX.utils.json_to_sheet(excelData);
  const workbook = XLSX.utils.book_new();
  XLSX.utils.book_append_sheet(workbook, worksheet, "ChiTietKiemKe");

  // 4. Định dạng độ rộng các cột cho đẹp (Tùy chọn)
  worksheet["!cols"] = [
    { wch: 5 }, // STT
    { wch: 15 }, // Mã HH
    { wch: 30 }, // Tên HH
    { wch: 20 }, // Tồn kho
    { wch: 20 }, // SL Kiểm kê
    { wch: 15 }, // Độ lệch
  ];

  // 5. Tạo tên file chứa Mã chứng từ và tải xuống
  const fileName = `KiemKe_${voucher.maChungTu}.xlsx`;
  XLSX.writeFile(workbook, fileName);
};
</script>

<style scoped>
.filter-header {
  border-left: 3px solid #f37021;
}
.form-check-input:checked {
  background-color: #f37021;
  border-color: #f37021;
}
.cursor-pointer {
  cursor: pointer;
}
.detail-row td {
  box-shadow: inset 0 3px 6px -3px rgba(0, 0, 0, 0.2);
}
</style>
