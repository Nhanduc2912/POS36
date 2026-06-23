<template>
  <div class="container-fluid px-4 py-4 bg-light min-vh-100">
    <h5 class="fw-bold text-danger mb-4"><i class="bi bi-credit-card-2-front me-2"></i>GÓI DỊCH VỤ CỦA BẠN</h5>

    <!-- Current Plan -->
    <div class="card border-0 shadow-sm mb-4">
      <div class="card-body p-4">
        <div class="row align-items-center">
          <div class="col-md-8">
            <div class="d-flex align-items-center gap-3 mb-3">
              <span class="badge bg-warning text-dark fs-6 px-3 py-2">{{ myPlan.tenGoi }}</span>
              <span class="badge" :class="statusClass">{{ statusText }}</span>
            </div>
            <div class="row g-3">
              <div class="col-auto"><small class="text-muted">Hết hạn</small><div class="fw-bold">{{ formatDate(myPlan.ngayHetHan) }}</div></div>
              <div class="col-auto"><small class="text-muted">Còn lại</small><div class="fw-bold" :class="myPlan.soNgayConLai <= 7 ? 'text-danger' : 'text-success'">{{ myPlan.soNgayConLai }} ngày</div></div>
              <div class="col-auto" v-if="myPlan.gioiHanHoaDon"><small class="text-muted">Hóa đơn tháng này</small><div class="fw-bold">{{ myPlan.hoaDonThangNay }} / {{ myPlan.gioiHanHoaDon }}</div></div>
            </div>
          </div>
          <div class="col-md-4 text-end">
            <div class="display-4 fw-black text-warning">{{ myPlan.soNgayConLai }}</div>
            <div class="text-muted">ngày còn lại</div>
          </div>
        </div>
      </div>
    </div>

    <!-- Available Plans -->
    <h6 class="fw-bold text-dark mb-3"><i class="bi bi-box-seam me-2"></i>Nâng cấp / Gia hạn</h6>
    <div class="row g-3 mb-4">
      <div class="col-md-4" v-for="p in plans" :key="p.id">
        <div class="card border-0 shadow-sm h-100 plan-card" :class="{ 'border-warning border-2': p.maGoi === 'PRO' }">
          <div class="card-body p-4 text-center">
            <span class="badge bg-warning text-dark mb-2">{{ p.maGoi }}</span>
            <h5 class="fw-bold">{{ p.tenGoi }}</h5>
            <div class="display-6 fw-black text-danger my-3">{{ formatVND(p.tongGia) }}</div>
            <div class="text-muted small mb-3">{{ p.soThang }} tháng ({{ formatVND(p.giaThang) }}/tháng)</div>
            <ul class="list-unstyled text-start small mb-4">
              <li class="mb-2"><i class="bi bi-check-circle-fill text-success me-2"></i>{{ p.gioiHanHoaDon ? p.gioiHanHoaDon + ' hóa đơn/tháng' : 'Không giới hạn hóa đơn' }}</li>
              <li class="mb-2"><i class="bi bi-check-circle-fill text-success me-2"></i>{{ p.gioiHanNhanVien ? p.gioiHanNhanVien + ' nhân viên' : 'Không giới hạn nhân viên' }}</li>
              <li><i class="bi bi-check-circle-fill text-success me-2"></i>Đầy đủ tính năng</li>
            </ul>
            <button class="btn btn-danger w-100 fw-bold py-2" :disabled="purchasing" @click="purchase(p)">
              <span v-if="purchasing" class="spinner-border spinner-border-sm me-1"></span>
              <i v-else class="bi bi-cart-check me-1"></i>
              {{ purchasing ? 'Đang xử lý...' : 'Mua gói này' }}
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Payment History -->
    <h6 class="fw-bold text-dark mb-3"><i class="bi bi-clock-history me-2"></i>Lịch sử thanh toán</h6>
    <div class="card border-0 shadow-sm">
      <div class="card-body p-0">
        <table class="table table-hover mb-0">
          <thead class="table-light">
            <tr>
              <th>Gói</th><th>Số tiền</th><th>Mã GD</th><th>Trạng thái</th><th>Ngày tạo</th><th>Hành động</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="h in history" :key="h.id">
              <td class="fw-bold">{{ h.tenGoi }}</td>
              <td class="text-danger fw-bold">{{ formatVND(h.soTienThanhToan) }}</td>
              <td><code>{{ h.maGiaoDich }}</code></td>
              <td>
                <span class="badge" :class="h.trangThai==='DaThanhToan'?'bg-success':h.trangThai==='DaHuy'?'bg-secondary':'bg-warning text-dark'">
                  {{ h.trangThai==='DaThanhToan'?'Đã thanh toán':h.trangThai==='DaHuy'?'Đã hủy':'Chờ thanh toán' }}
                </span>
              </td>
              <td class="small text-muted">{{ formatDate(h.ngayTao) }}</td>
              <td>
                <button
                  v-if="h.trangThai === 'ChoThanhToan'"
                  class="btn btn-sm btn-outline-primary me-1"
                  @click="openCheckout(h.id)">
                  <i class="bi bi-qr-code-scan me-1"></i>Tiếp tục thanh toán
                </button>
              </td>
            </tr>
            <tr v-if="!history.length">
              <td colspan="6" class="text-center text-muted py-4">Chưa có lịch sử thanh toán nào.</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, inject } from "vue";
import axios from "axios";
const swal = inject("$swal");

const myPlan  = ref({ tenGoi:"Loading...", soNgayConLai:0, trangThai:"", ngayHetHan:"", gioiHanHoaDon:0, hoaDonThangNay:0 });
const plans   = ref([]);
const history = ref([]);
const purchasing = ref(false);

const statusClass = computed(() => ({
  DungThu:"bg-info",HoatDong:"bg-success",ChiDoc:"bg-warning text-dark",BiKhoa:"bg-danger"
}[myPlan.value.trangThai]||"bg-secondary"));
const statusText = computed(() => ({
  DungThu:"Dùng thử",HoatDong:"Hoạt động",ChiDoc:"Chỉ đọc",BiKhoa:"Bị khóa"
}[myPlan.value.trangThai]||myPlan.value.trangThai));

const formatDate = (d) => d ? new Date(d).toLocaleDateString("vi-VN") : "—";
const formatVND  = (n) => n ? Number(n).toLocaleString("vi-VN") + "đ" : "0đ";

// Mở tab checkout
const openCheckout = (donDangKyId) => {
  window.open(`/payment/checkout/${donDangKyId}`, '_blank');
};

// Mua gói
const purchase = async (plan) => {
  // Xác nhận trước khi tạo đơn
  const confirm = await swal.fire({
    title: `Mua gói ${plan.tenGoi}?`,
    html: `Số tiền: <strong class="text-danger">${formatVND(plan.tongGia)}</strong><br>Thời hạn: <strong>${plan.soThang} tháng</strong><br><br><small class="text-muted">Sau khi xác nhận, một tab thanh toán mới sẽ được mở. Mã QR có hiệu lực trong <strong>10 phút</strong>.</small>`,
    icon: 'question',
    showCancelButton: true,
    confirmButtonText: 'Xác nhận mua',
    cancelButtonText: 'Hủy',
    confirmButtonColor: '#dc3545',
  });
  if (!confirm.isConfirmed) return;

  purchasing.value = true;
  try {
    const res = await axios.post("/api/Subscription/purchase", { goiDichVuId: plan.id });
    const data = res.data;
    // Mở tab checkout mới
    openCheckout(data.donDangKyId);
    // Reload history sau 1 giây
    setTimeout(loadHistory, 1200);
    swal.fire({
      toast: true, position: 'top-end', icon: 'success',
      title: 'Đã tạo đơn! Tab thanh toán đã mở.',
      timer: 4000, showConfirmButton: false
    });
  } catch (e) {
    const errData = e.response?.data;
    if (errData?.errorCode === 'PENDING_ORDER') {
      // Có đơn đang chờ
      const choice = await swal.fire({
        title: 'Bạn đang có đơn chưa thanh toán!',
        html: `Gói: <strong>${errData.tenGoi}</strong><br>Mã: <code>${errData.maGiaoDich}</code><br>Còn hạn: <strong>${Math.ceil(errData.giayConLai / 60)} phút</strong>`,
        icon: 'warning',
        showCancelButton: true,
        showDenyButton: true,
        confirmButtonText: 'Tiếp tục thanh toán',
        denyButtonText: 'Hủy đơn cũ',
        cancelButtonText: 'Đóng',
        confirmButtonColor: '#f97316',
        denyButtonColor: '#6b7280',
      });
      if (choice.isConfirmed) {
        openCheckout(errData.donDangKyId);
      } else if (choice.isDenied) {
        await cancelPendingOrder(errData.donDangKyId);
      }
    } else if (errData?.errorCode === 'DAILY_LIMIT') {
      swal.fire('Đã đạt giới hạn ngày', errData.message, 'warning');
    } else {
      swal.fire('Lỗi', errData?.message || e.response?.data || 'Không thể tạo đơn', 'error');
    }
  } finally {
    purchasing.value = false;
  }
};

const cancelPendingOrder = async (id) => {
  try {
    await axios.post(`/api/Subscription/cancel/${id}`);
    swal.fire({ toast: true, position: 'top-end', icon: 'success', title: 'Đã hủy đơn cũ!', timer: 2000, showConfirmButton: false });
    await loadHistory();
  } catch {
    swal.fire('Lỗi', 'Không thể hủy đơn. Thử lại sau.', 'error');
  }
};

const loadHistory = async () => {
  try {
    const res = await axios.get("/api/Subscription/history");
    history.value = res.data;
  } catch {}
};

onMounted(async () => {
  try {
    const [p1, p2, p3] = await Promise.all([
      axios.get("/api/Subscription/my-plan"),
      axios.get("/api/Subscription/plans"),
      axios.get("/api/Subscription/history"),
    ]);
    myPlan.value  = p1.data;
    plans.value   = p2.data;
    history.value = p3.data;
  } catch(e) { console.error(e); }
});
</script>

<style scoped>
.fw-black { font-weight: 800; }
.plan-card { transition: transform .2s, box-shadow .2s; }
.plan-card:hover { transform: translateY(-4px); box-shadow: 0 12px 30px rgba(0,0,0,.1)!important; }
</style>
