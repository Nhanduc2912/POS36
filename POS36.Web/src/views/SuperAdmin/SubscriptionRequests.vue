<template>
  <div>
    <div class="d-flex justify-content-between align-items-center mb-4">
      <select v-model="filterStatus" class="sa-select" @change="load">
        <option value="">Tất cả</option>
        <option value="ChoThanhToan">Chờ thanh toán</option>
        <option value="DaThanhToan">Đã thanh toán</option>
        <option value="DaHuy">Đã hủy</option>
      </select>
    </div>
    <div class="sa-table-wrap">
      <table class="sa-table">
        <thead><tr><th>ID</th><th>Cửa hàng</th><th>Gói</th><th>Số tiền</th><th>Mã GD</th><th>Trạng thái</th><th>Ngày tạo</th><th>Ngày TT</th><th>Người duyệt</th><th>Thao tác</th></tr></thead>
        <tbody>
          <tr v-for="s in subs" :key="s.id">
            <td class="text-muted">#{{ s.id }}</td>
            <td class="fw-bold">{{ s.tenCuaHang }}</td>
            <td><span class="sa-goi-badge">{{ s.tenGoi }}</span></td>
            <td class="text-warning fw-bold">{{ formatVND(s.soTienThanhToan) }}</td>
            <td><code class="text-info">{{ s.maGiaoDich }}</code></td>
            <td><span class="sa-status-badge" :class="'status-' + s.trangThai">{{ statusText(s.trangThai) }}</span></td>
            <td class="small">{{ formatDate(s.ngayTao) }}</td>
            <td class="small">{{ s.ngayThanhToan ? formatDate(s.ngayThanhToan) : "—" }}</td>
            <td class="small text-muted">{{ s.nguoiDuyet || "—" }}</td>
            <td>
              <div class="d-flex gap-1" v-if="s.trangThai === 'ChoThanhToan'">
                <button class="sa-btn-sm sa-btn-success" @click="approve(s.id)" title="Duyệt"><i class="bi bi-check-lg"></i></button>
                <button class="sa-btn-sm sa-btn-danger" @click="reject(s.id)" title="Từ chối"><i class="bi bi-x-lg"></i></button>
              </div>
              <span v-else class="text-muted small">—</span>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, inject } from "vue";
import axios from "axios";
const swal = inject("$swal");
const subs = ref([]);
const filterStatus = ref("");

const formatDate = (d) => d ? new Date(d).toLocaleDateString("vi-VN") : "—";
const formatVND = (n) => n ? Number(n).toLocaleString("vi-VN") + "đ" : "0đ";
const statusText = (s) => ({ ChoThanhToan: "Chờ TT", DaThanhToan: "Đã TT", DaHuy: "Đã hủy" }[s] || s);

const load = async () => {
  try {
    const params = {};
    if (filterStatus.value) params.trangThai = filterStatus.value;
    const res = await axios.get("/api/SuperAdmin/subscriptions", { params });
    subs.value = res.data;
  } catch (e) { console.error(e); }
};

const approve = async (id) => {
  const r = await swal.fire({ title: "Duyệt đơn này?", icon: "question", showCancelButton: true, confirmButtonText: "Duyệt", cancelButtonText: "Hủy" });
  if (!r.isConfirmed) return;
  try {
    await axios.put(`/api/SuperAdmin/subscriptions/${id}/approve`);
    swal.fire({ toast: true, position: "top-end", icon: "success", title: "Đã duyệt!", timer: 1500, showConfirmButton: false });
    load();
  } catch (e) { swal.fire("Lỗi", e.response?.data || "Thất bại", "error"); }
};

const reject = async (id) => {
  const r = await swal.fire({ title: "Từ chối đơn này?", input: "text", inputPlaceholder: "Lý do...", icon: "warning", showCancelButton: true, confirmButtonText: "Từ chối" });
  if (!r.isConfirmed) return;
  try {
    await axios.put(`/api/SuperAdmin/subscriptions/${id}/reject`, { lyDo: r.value });
    swal.fire({ toast: true, position: "top-end", icon: "info", title: "Đã từ chối", timer: 1500, showConfirmButton: false });
    load();
  } catch (e) { swal.fire("Lỗi", "Thất bại", "error"); }
};

onMounted(load);
</script>

<style scoped>
.sa-select { background: #1a1c23; color: #e4e4e7; border: 1px solid rgba(255,255,255,0.1); border-radius: 8px; padding: 8px 14px; font-size: 0.85rem; }
.sa-table-wrap { overflow-x: auto; }
.sa-table { width: 100%; border-collapse: separate; border-spacing: 0; }
.sa-table th { font-size: 0.72rem; text-transform: uppercase; color: #6b7280; padding: 10px 12px; border-bottom: 1px solid rgba(255,255,255,0.06); letter-spacing: 0.5px; font-weight: 600; }
.sa-table td { padding: 12px; border-bottom: 1px solid rgba(255,255,255,0.04); font-size: 0.85rem; vertical-align: middle; }
.sa-table tbody tr:hover { background: rgba(245, 158, 11, 0.04); }
.sa-goi-badge { background: rgba(245,158,11,0.15); color: #f59e0b; padding: 3px 10px; border-radius: 6px; font-size: 0.75rem; font-weight: 700; }
.sa-status-badge { padding: 3px 10px; border-radius: 6px; font-size: 0.75rem; font-weight: 700; }
.status-ChoThanhToan { background: rgba(245,158,11,0.15); color: #f59e0b; }
.status-DaThanhToan { background: rgba(34,197,94,0.15); color: #22c55e; }
.status-DaHuy { background: rgba(239,68,68,0.15); color: #ef4444; }
.sa-btn-sm { width: 32px; height: 32px; border: none; border-radius: 8px; display: flex; align-items: center; justify-content: center; cursor: pointer; transition: 0.2s; font-size: 0.85rem; }
.sa-btn-success { background: rgba(34,197,94,0.15); color: #22c55e; }
.sa-btn-success:hover { background: rgba(34,197,94,0.3); }
.sa-btn-danger { background: rgba(239,68,68,0.15); color: #ef4444; }
.sa-btn-danger:hover { background: rgba(239,68,68,0.3); }
</style>
