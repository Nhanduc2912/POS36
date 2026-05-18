<template>
  <div>
    <!-- Tabs -->
    <div class="tabs-bar mb-4">
      <button class="tab-btn" :class="{ active: activeTab === 'subscriptions' }" @click="activeTab = 'subscriptions'">
        <i class="bi bi-credit-card-2-front me-1"></i>Đơn đăng ký gói
        <span class="tab-badge" v-if="pendingCount">{{ pendingCount }}</span>
      </button>
      <button class="tab-btn" :class="{ active: activeTab === 'trials' }" @click="activeTab = 'trials'">
        <i class="bi bi-hourglass-split me-1"></i>Tài khoản dùng thử
        <span class="tab-badge trial-badge">{{ trials.length }}</span>
      </button>
    </div>

    <!-- TAB 1: Đơn đăng ký -->
    <div v-show="activeTab === 'subscriptions'">
      <div class="d-flex gap-2 mb-3 flex-wrap">
        <select v-model="filterStatus" class="sa-select" @change="loadSubs">
          <option value="">Tất cả trạng thái</option>
          <option value="ChoThanhToan">Chờ thanh toán</option>
          <option value="DaThanhToan">Đã thanh toán</option>
          <option value="DaHuy">Đã hủy</option>
        </select>
      </div>
      <div class="sa-table-wrap">
        <table class="sa-table">
          <thead>
            <tr>
              <th>ID</th><th>Cửa hàng</th><th>Gói</th><th>Số tiền</th>
              <th>Mã GD</th><th>Trạng thái</th><th>Ngày tạo</th>
              <th>Ngày TT</th><th>Người duyệt</th><th>Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="s in subs" :key="s.id">
              <td style="color:var(--sa-text-faint)">#{{ s.id }}</td>
              <td class="fw-bold" style="color:var(--sa-text)">{{ s.tenCuaHang }}</td>
              <td><span class="sa-goi-badge">{{ s.tenGoi }}</span></td>
              <td style="color:var(--sa-accent);font-weight:700">{{ formatVND(s.soTienThanhToan) }}</td>
              <td><code style="color:#3b82f6;font-size:.78rem">{{ s.maGiaoDich || '—' }}</code></td>
              <td><span class="sa-status-badge" :class="'status-' + s.trangThai">{{ statusText(s.trangThai) }}</span></td>
              <td style="font-size:.8rem;color:var(--sa-text-faint)">{{ formatDate(s.ngayTao) }}</td>
              <td style="font-size:.8rem;color:var(--sa-text-faint)">{{ s.ngayThanhToan ? formatDate(s.ngayThanhToan) : '—' }}</td>
              <td style="font-size:.8rem;color:var(--sa-text-faint)">{{ s.nguoiDuyet || '—' }}</td>
              <td>
                <div class="d-flex gap-1" v-if="s.trangThai === 'ChoThanhToan'">
                  <button class="sa-btn-sm sa-btn-success" @click="approve(s.id)" title="Duyệt"><i class="bi bi-check-lg"></i></button>
                  <button class="sa-btn-sm sa-btn-danger" @click="reject(s.id)" title="Từ chối"><i class="bi bi-x-lg"></i></button>
                </div>
                <span v-else style="color:var(--sa-text-faint);font-size:.78rem">—</span>
              </td>
            </tr>
            <tr v-if="!subs.length">
              <td colspan="10" class="text-center py-5" style="color:var(--sa-text-faint)">
                <i class="bi bi-inbox display-4 opacity-25 d-block mb-2"></i>Không có đơn nào
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- TAB 2: Tài khoản dùng thử -->
    <div v-show="activeTab === 'trials'">
      <div class="trial-info-bar mb-3">
        <i class="bi bi-info-circle text-info me-2"></i>
        Danh sách cửa hàng đang trong giai đoạn <strong>dùng thử 7 ngày</strong>. Có thể nâng cấp lên gói trả phí hoặc gia hạn thủ công.
      </div>
      <div class="sa-table-wrap">
        <table class="sa-table">
          <thead>
            <tr>
              <th>ID</th><th>Cửa hàng</th><th>SĐT</th><th>Ngày đăng ký</th>
              <th>Hết hạn</th><th>Còn lại</th><th>Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="t in trials" :key="t.id">
              <td style="color:var(--sa-text-faint)">#{{ t.id }}</td>
              <td class="fw-bold" style="color:var(--sa-text)">{{ t.tenCuaHang }}</td>
              <td style="color:var(--sa-text-muted)">{{ t.soDienThoai }}</td>
              <td style="font-size:.8rem;color:var(--sa-text-faint)">{{ formatDate(t.ngayDangKy) }}</td>
              <td style="font-size:.8rem;color:var(--sa-text-faint)">{{ formatDate(t.ngayHetHan) }}</td>
              <td>
                <span :class="t.soNgayConLai <= 2 ? 'text-danger fw-bold' : t.soNgayConLai <= 5 ? 'text-warning fw-bold' : 'text-success'"
                  style="font-size:.85rem">
                  {{ t.soNgayConLai }} ngày
                </span>
              </td>
              <td>
                <button class="sa-btn-sm sa-btn-warning" @click="openUpgrade(t)" title="Nâng cấp gói">
                  <i class="bi bi-arrow-up-circle"></i>
                </button>
              </td>
            </tr>
            <tr v-if="!trials.length">
              <td colspan="7" class="text-center py-5" style="color:var(--sa-text-faint)">
                <i class="bi bi-hourglass display-4 opacity-25 d-block mb-2"></i>Không có tài khoản dùng thử nào
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Modal: Nâng cấp trial -->
    <div class="sa-modal-overlay" v-if="showUpgrade" @click.self="showUpgrade = false">
      <div class="sa-modal" style="max-width:420px">
        <div class="sa-modal-header">
          <h5><i class="bi bi-arrow-up-circle me-2"></i>Nâng cấp: {{ upgradeStore?.tenCuaHang }}</h5>
          <button class="sa-modal-close" @click="showUpgrade = false">&times;</button>
        </div>
        <div class="sa-modal-body">
          <div class="mb-3">
            <label>Chọn gói</label>
            <select v-model="upgradeGoiDichVu" class="sa-select w-100">
              <option value="">-- Chọn gói --</option>
              <option value="starter">Starter (900k/6 tháng)</option>
              <option value="pro">Pro (1.5tr/năm)</option>
              <option value="ultimate">Ultimate (2.8tr/2 năm)</option>
            </select>
          </div>
          <div class="mb-4">
            <label>Số tháng gia hạn</label>
            <select v-model.number="upgradeMonths" class="sa-select w-100">
              <option :value="6">6 tháng</option>
              <option :value="12">12 tháng</option>
              <option :value="24">24 tháng</option>
            </select>
          </div>
          <button style="background:var(--sa-accent);border:none;color:#fff;padding:12px;border-radius:8px;font-weight:700;width:100%;cursor:pointer"
            :disabled="!upgradeGoiDichVu" @click="doUpgrade">
            <i class="bi bi-check-lg me-1"></i>Xác nhận nâng cấp
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, inject } from "vue";
import axios from "axios";
const swal = inject("$swal");

const activeTab = ref("subscriptions");
const subs = ref([]);
const trials = ref([]);
const filterStatus = ref("");
const showUpgrade = ref(false);
const upgradeStore = ref(null);
const upgradeGoiDichVu = ref("");
const upgradeMonths = ref(12);

const pendingCount = computed(() => subs.value.filter(s => s.trangThai === "ChoThanhToan").length);

const formatDate = (d) => d ? new Date(d).toLocaleDateString("vi-VN") : "—";
const formatVND = (n) => n ? Number(n).toLocaleString("vi-VN") + "đ" : "0đ";
const statusText = (s) => ({ ChoThanhToan: "Chờ TT", DaThanhToan: "Đã TT", DaHuy: "Đã hủy" }[s] || s);

const loadSubs = async () => {
  try {
    const params = {};
    if (filterStatus.value) params.trangThai = filterStatus.value;
    const res = await axios.get("/api/SuperAdmin/subscriptions", { params });
    subs.value = res.data;
  } catch (e) { console.error(e); }
};

const loadTrials = async () => {
  try {
    const res = await axios.get("/api/SuperAdmin/stores", { params: { trangThai: "DungThu" } });
    trials.value = res.data;
  } catch (e) { console.error(e); }
};

const approve = async (id) => {
  const r = await swal.fire({ title: "Duyệt đơn này?", icon: "question", showCancelButton: true, confirmButtonText: "Duyệt", cancelButtonText: "Hủy" });
  if (!r.isConfirmed) return;
  try {
    await axios.put(`/api/SuperAdmin/subscriptions/${id}/approve`);
    swal.fire({ toast: true, position: "top-end", icon: "success", title: "Đã duyệt!", timer: 1500, showConfirmButton: false });
    loadSubs();
  } catch (e) { swal.fire("Lỗi", e.response?.data || "Thất bại", "error"); }
};

const reject = async (id) => {
  const r = await swal.fire({ title: "Từ chối đơn này?", input: "text", inputPlaceholder: "Lý do...", icon: "warning", showCancelButton: true, confirmButtonText: "Từ chối" });
  if (!r.isConfirmed) return;
  try {
    await axios.put(`/api/SuperAdmin/subscriptions/${id}/reject`, { lyDo: r.value });
    swal.fire({ toast: true, position: "top-end", icon: "info", title: "Đã từ chối", timer: 1500, showConfirmButton: false });
    loadSubs();
  } catch (e) { swal.fire("Lỗi", "Thất bại", "error"); }
};

const openUpgrade = (store) => {
  upgradeStore.value = store;
  upgradeGoiDichVu.value = "";
  upgradeMonths.value = 12;
  showUpgrade.value = true;
};

const doUpgrade = async () => {
  try {
    await axios.post(`/api/SuperAdmin/stores/${upgradeStore.value.id}/extend`, {
      soThang: upgradeMonths.value,
      goiDichVu: upgradeGoiDichVu.value,
    });
    swal.fire({ toast: true, position: "top-end", icon: "success", title: "Đã nâng cấp thành công!", timer: 2000, showConfirmButton: false });
    showUpgrade.value = false;
    loadTrials();
  } catch (e) { swal.fire("Lỗi", "Nâng cấp thất bại", "error"); }
};

onMounted(() => { loadSubs(); loadTrials(); });
</script>

<style scoped>
@import "./sa-shared.css";

.tabs-bar {
  display: flex; gap: 4px;
  border-bottom: 1px solid var(--sa-border);
  padding-bottom: 0;
}
.tab-btn {
  display: flex; align-items: center; gap: 6px;
  background: none; border: none;
  color: var(--sa-text-muted);
  padding: 10px 18px; border-radius: 8px 8px 0 0;
  font-size: .85rem; font-weight: 600; cursor: pointer;
  border-bottom: 2px solid transparent;
  transition: .2s;
}
.tab-btn:hover { color: var(--sa-text); background: var(--sa-nav-hover-bg); }
.tab-btn.active { color: var(--sa-accent); border-bottom-color: var(--sa-accent); background: var(--sa-nav-active-bg); }
.tab-badge {
  background: #ef4444; color: #fff;
  font-size: .65rem; font-weight: 800;
  padding: 2px 7px; border-radius: 99px; line-height: 1.4;
}
.trial-badge { background: #3b82f6; }

.trial-info-bar {
  background: rgba(59,130,246,.08);
  border: 1px solid rgba(59,130,246,.2);
  border-radius: 8px;
  padding: 10px 16px;
  font-size: .83rem;
  color: var(--sa-text-muted);
}
</style>
