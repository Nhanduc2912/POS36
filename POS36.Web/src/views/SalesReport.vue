<template>
  <div class="container-fluid p-4 bg-light min-vh-100">
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h3 class="fw-bold text-dark">
        <i class="bi bi-graph-up-arrow me-2 text-success"></i>Báo Cáo Hàng Hóa
      </h3>
    </div>

    <div class="card border-0 shadow-sm rounded-4 mb-4">
      <div class="card-body d-flex gap-3 align-items-end">
        <div>
          <label class="form-label small fw-bold text-muted">Từ ngày</label>
          <input type="date" class="form-control" v-model="tuNgay" />
        </div>
        <div>
          <label class="form-label small fw-bold text-muted">Đến ngày</label>
          <input type="date" class="form-control" v-model="denNgay" />
        </div>
        <button
          class="btn btn-primary fw-bold"
          @click="fetchData"
          :disabled="isLoading"
        >
          Lọc Dữ Liệu
        </button>
      </div>
    </div>

    <div class="card border-0 shadow-sm rounded-4">
      <div class="card-header bg-white border-bottom py-3">
        <h6 class="fw-bold mb-0">Xếp Hạng Mặt Hàng Bán Chạy (Thực Tế)</h6>
      </div>
      <div class="card-body p-0 table-responsive">
        <div v-if="isLoading" class="text-center py-5">
          <span class="spinner-border text-primary"></span>
        </div>
        <table v-else class="table table-hover align-middle mb-0">
          <thead class="table-light text-muted small">
            <tr>
              <th class="ps-4">TOP</th>
              <th>TÊN HÀNG HÓA</th>
              <th class="text-center">SỐ LƯỢNG BÁN</th>
              <th class="text-end pe-4">DOANH THU MANG LẠI</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="danhSachMon.length === 0">
              <td colspan="4" class="text-center py-4 text-muted fst-italic">
                Không có dữ liệu trong khoảng thời gian này.
              </td>
            </tr>
            <tr v-for="(mon, index) in danhSachMon" :key="index">
              <td class="ps-4 fw-bold">
                <span
                  :class="
                    index === 0
                      ? 'text-warning fs-5'
                      : index === 1
                        ? 'text-secondary fs-5'
                        : 'text-muted'
                  "
                  >#{{ index + 1 }}</span
                >
              </td>
              <td class="fw-bold text-dark">{{ mon.tenMon }}</td>
              <td class="text-center">
                <span class="badge bg-light text-dark border px-3 py-2">{{
                  mon.soLuong
                }}</span>
              </td>
              <td class="text-end pe-4 fw-bold text-success">
                {{ formatPrice(mon.doanhThu) }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import axios from "axios";

const tuNgay = ref(new Date(new Date().setDate(1)).toISOString().split("T")[0]);
const denNgay = ref(new Date().toISOString().split("T")[0]);
const danhSachMon = ref([]);
const isLoading = ref(false);

const formatPrice = (price) =>
  new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND" }).format(
    price || 0,
  );

const fetchData = async () => {
  isLoading.value = true;
  try {
    const token =
      localStorage.getItem("token") || localStorage.getItem("pos36_token");
    const res = await axios.get(
      `/api/Report/sales?fromDate=${tuNgay.value}&toDate=${denNgay.value}`,
      {
        headers: { Authorization: `Bearer ${token}` },
      },
    );
    danhSachMon.value = res.data;
  } catch (error) {
    console.error("Lỗi lấy báo cáo:", error);
  } finally {
    isLoading.value = false;
  }
};

onMounted(() => fetchData());
</script>
