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
            <button class="btn btn-danger w-100 fw-bold py-2" @click="purchase(p)">
              <i class="bi bi-cart-check me-1"></i> Mua gói này
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Payment QR Modal -->
    <div v-if="showQR" class="modal d-block" style="background:rgba(0,0,0,0.5)">
      <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content border-0 shadow-lg">
          <div class="modal-header bg-danger text-white"><h5 class="modal-title fw-bold">Thanh toán qua chuyển khoản</h5><button class="btn-close btn-close-white" @click="showQR=false"></button></div>
          <div class="modal-body text-center p-4">
            <!-- Khi SuperAdmin chưa cấu hình ngân hàng -->
            <div v-if="!systemBank.configured" class="alert alert-danger fw-bold">
              <i class="bi bi-exclamation-triangle me-1"></i>
              Hệ thống chưa cấu hình tài khoản ngân hàng nhận thanh toán. Vui lòng liên hệ quản trị viên.
            </div>
            <template v-else>
              <div class="alert alert-warning small fw-bold"><i class="bi bi-info-circle me-1"></i>Vui lòng chuyển khoản với nội dung chính xác bên dưới. Hệ thống sẽ tự động kích hoạt gói sau khi nhận được thanh toán.</div>
              <img :src="qrUrl" class="img-fluid rounded border p-2 mb-3" style="max-height:280px" />
              <div class="bg-light p-3 rounded-3 text-start">
                <div class="d-flex justify-content-between mb-2"><span class="text-muted">Ngân hàng:</span><span class="fw-bold">{{ systemBank.bankCode }}</span></div>
                <div class="d-flex justify-content-between mb-2"><span class="text-muted">Số tài khoản:</span><span class="fw-bold font-monospace">{{ systemBank.bankAccountNo }}</span></div>
                <div class="d-flex justify-content-between mb-2"><span class="text-muted">Chủ tài khoản:</span><span class="fw-bold text-uppercase">{{ systemBank.bankAccountName }}</span></div>
                <hr class="my-2" />
                <div class="d-flex justify-content-between mb-2"><span class="text-muted">Số tiền:</span><span class="fw-bold text-danger fs-5">{{ formatVND(purchaseData.tongTien) }}</span></div>
                <div class="d-flex justify-content-between mb-2"><span class="text-muted">Nội dung CK:</span><span class="fw-bold font-monospace text-primary">{{ purchaseData.maGiaoDich }}</span></div>
                <div class="d-flex justify-content-between"><span class="text-muted">Gói:</span><span class="fw-bold">{{ purchaseData.tenGoi }}</span></div>
              </div>
            </template>
          </div>

        </div>
      </div>
    </div>

    <!-- Payment History -->
    <h6 class="fw-bold text-dark mb-3"><i class="bi bi-clock-history me-2"></i>Lịch sử thanh toán</h6>
    <div class="card border-0 shadow-sm">
      <div class="card-body p-0">
        <table class="table table-hover mb-0">
          <thead class="table-light"><tr><th>Gói</th><th>Số tiền</th><th>Mã GD</th><th>Trạng thái</th><th>Ngày</th></tr></thead>
          <tbody>
            <tr v-for="h in history" :key="h.id">
              <td class="fw-bold">{{ h.tenGoi }}</td>
              <td class="text-danger fw-bold">{{ formatVND(h.soTienThanhToan) }}</td>
              <td><code>{{ h.maGiaoDich }}</code></td>
              <td><span class="badge" :class="h.trangThai==='DaThanhToan'?'bg-success':'bg-warning text-dark'">{{ h.trangThai==='DaThanhToan'?'Đã TT':'Chờ TT' }}</span></td>
              <td class="small text-muted">{{ formatDate(h.ngayTao) }}</td>
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
const myPlan = ref({ tenGoi:"Loading...", soNgayConLai:0, trangThai:"", ngayHetHan:"", gioiHanHoaDon:0, hoaDonThangNay:0 });
const plans = ref([]);
const history = ref([]);
const showQR = ref(false);
const purchaseData = ref({});

// Cấu hình ngân hàng HỆ THỐNG của SuperAdmin (chủ cửa hàng chuyển tiền mua gói VÀO đây)
const systemBank = ref({ bankCode:"", bankAccountNo:"", bankAccountName:"", configured: false });

const statusClass = computed(() => ({
  DungThu:"bg-info",HoatDong:"bg-success",ChiDoc:"bg-warning text-dark",BiKhoa:"bg-danger"
}[myPlan.value.trangThai]||"bg-secondary"));
const statusText = computed(() => ({
  DungThu:"Dùng thử",HoatDong:"Hoạt động",ChiDoc:"Chỉ đọc",BiKhoa:"Bị khóa"
}[myPlan.value.trangThai]||myPlan.value.trangThai));

const formatDate = (d) => d ? new Date(d).toLocaleDateString("vi-VN") : "—";
const formatVND = (n) => n ? Number(n).toLocaleString("vi-VN") + "đ" : "0đ";

const qrUrl = computed(() => {
  if(!purchaseData.value.maGiaoDich) return "";
  if(!systemBank.value.configured) return "";
  const b = systemBank.value;
  return `https://img.vietqr.io/image/${b.bankCode}-${b.bankAccountNo}-compact2.png?amount=${purchaseData.value.tongTien}&addInfo=${purchaseData.value.maGiaoDich}&accountName=${encodeURIComponent(b.bankAccountName)}`;
});

const purchase = async (plan) => {
  try {
    const res = await axios.post("/api/Subscription/purchase", { goiDichVuId: plan.id });
    purchaseData.value = res.data;
    showQR.value = true;
  } catch(e) { swal.fire("Lỗi", e.response?.data || "Không thể tạo đơn", "error"); }
};

onMounted(async () => {
  try {
    // Load cấu hình ngân hàng của HỆ THỐNG (SuperAdmin) — chủ cửa hàng chuyển tiền mua gói vào đây
    try { const bc = await axios.get("/api/Subscription/payment-config"); if(bc.data) systemBank.value = bc.data; } catch(e){}
    const [p1, p2, p3] = await Promise.all([
      axios.get("/api/Subscription/my-plan"),
      axios.get("/api/Subscription/plans"),
      axios.get("/api/Subscription/history"),
    ]);
    myPlan.value = p1.data;
    plans.value = p2.data;
    history.value = p3.data;
  } catch(e) { console.error(e); }
});
</script>

<style scoped>
.fw-black{font-weight:800}
.plan-card{transition:transform .2s,box-shadow .2s}
.plan-card:hover{transform:translateY(-4px);box-shadow:0 12px 30px rgba(0,0,0,.1)!important}
</style>
