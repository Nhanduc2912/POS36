<template>
  <div class="thiet-lap-wrap">
    <div class="tl-header">
      <h5><i class="bi bi-gear me-2"></i>Thiết lập cửa hàng</h5>
      <button class="tl-save-btn" @click="saveAll" :disabled="saving">
        <span v-if="saving" class="spinner-border spinner-border-sm me-1"></span>
        <i v-else class="bi bi-floppy me-1"></i>{{ saving ? 'Đang lưu...' : 'Lưu tất cả' }}
      </button>
    </div>

    <!-- Tabs -->
    <div class="tl-tabs">
      <button v-for="t in tabs" :key="t.key" class="tl-tab" :class="{active: activeTab===t.key}" @click="activeTab=t.key">
        <i :class="'bi bi-'+t.icon+' me-1'"></i>{{ t.label }}
      </button>
    </div>

    <!-- TAB 1: Thông tin cửa hàng -->
    <div v-show="activeTab==='info'" class="tl-body">
      <div class="tl-card">
        <h6 class="tl-card-title"><i class="bi bi-shop me-2"></i>Thông tin cơ bản</h6>
        <div class="row g-3">
          <div class="col-md-6">
            <label>Tên cửa hàng</label>
            <input v-model="info.tenCuaHang" class="tl-input w-100" placeholder="Quán Ăn ABC" />
          </div>
          <div class="col-md-6">
            <label>Số điện thoại</label>
            <input v-model="info.soDienThoai" class="tl-input w-100" placeholder="0901234567" disabled />
            <small class="tl-hint">Liên hệ Super Admin để đổi SĐT</small>
          </div>
          <div class="col-md-6">
            <label>Email</label>
            <input v-model="info.email" class="tl-input w-100" placeholder="quan@gmail.com" />
          </div>
          <div class="col-md-6">
            <label>Địa chỉ</label>
            <input v-model="info.diaChi" class="tl-input w-100" placeholder="123 Nguyễn Văn A, Q1..." />
          </div>
          <div class="col-12">
            <label>URL Logo</label>
            <input v-model="info.logoUrl" class="tl-input w-100" placeholder="https://..." />
            <div class="mt-2" v-if="info.logoUrl">
              <img :src="info.logoUrl" style="max-height:60px;border-radius:8px;border:1px solid var(--border-color)" />
            </div>
          </div>
        </div>
      </div>
      <div class="tl-card mt-3">
        <h6 class="tl-card-title"><i class="bi bi-receipt me-2"></i>Mẫu in hóa đơn</h6>
        <div class="mb-3">
          <label>Tiêu đề hóa đơn</label>
          <input v-model="cfg.InHoaDon_TieuDe" class="tl-input w-100" placeholder="HÓA ĐƠN THANH TOÁN" />
        </div>
        <div class="mb-3">
          <label>Ghi chú cuối hóa đơn</label>
          <textarea v-model="cfg.InHoaDon_GhiChu" class="tl-input w-100" rows="2" placeholder="Cảm ơn quý khách!"></textarea>
        </div>
        <div class="mb-3">
          <label>Prefix mã hóa đơn</label>
          <input v-model="cfg.InHoaDon_Prefix" class="tl-input w-100" placeholder="HD" maxlength="5" />
        </div>
      </div>
    </div>

    <!-- TAB 2: Tùy chọn POS -->
    <div v-show="activeTab==='pos'" class="tl-body">
      <div class="tl-card">
        <h6 class="tl-card-title"><i class="bi bi-cash-register me-2"></i>Tùy chọn thanh toán & vận hành</h6>
        <div class="tl-toggle-group">
          <div class="tl-toggle-item">
            <div>
              <div class="tl-toggle-label">Cho phép giảm giá thủ công</div>
              <div class="tl-toggle-desc">Thu ngân có thể nhập % hoặc số tiền giảm trực tiếp</div>
            </div>
            <label class="tl-switch">
              <input type="checkbox" v-model="cfgBool.POS_ChophepGiamGia" />
              <span class="tl-slider"></span>
            </label>
          </div>
          <div class="tl-toggle-item">
            <div>
              <div class="tl-toggle-label">Tự động in hóa đơn sau thanh toán</div>
              <div class="tl-toggle-desc">In ngay khi xác nhận thanh toán thành công</div>
            </div>
            <label class="tl-switch">
              <input type="checkbox" v-model="cfgBool.POS_TuDongIn" />
              <span class="tl-slider"></span>
            </label>
          </div>
          <div class="tl-toggle-item">
            <div>
              <div class="tl-toggle-label">Yêu cầu xác nhận trước khi gửi bếp</div>
              <div class="tl-toggle-desc">Hiện popup xác nhận trước khi order sang màn hình bếp</div>
            </div>
            <label class="tl-switch">
              <input type="checkbox" v-model="cfgBool.POS_XacNhanGuiBep" />
              <span class="tl-slider"></span>
            </label>
          </div>
          <div class="tl-toggle-item">
            <div>
              <div class="tl-toggle-label">Hiển thị QR thanh toán tự động</div>
              <div class="tl-toggle-desc">Tự hiện VietQR khi bắt đầu thanh toán</div>
            </div>
            <label class="tl-switch">
              <input type="checkbox" v-model="cfgBool.POS_HienQR" />
              <span class="tl-slider"></span>
            </label>
          </div>
        </div>
        <div class="row g-3 mt-2">
          <div class="col-md-6">
            <label>Giảm giá tối đa (%)</label>
            <input type="number" v-model.number="cfg.POS_GiamGiaMax" class="tl-input w-100" min="0" max="100" />
          </div>
          <div class="col-md-6">
            <label>Cảnh báo kho khi còn (đơn vị)</label>
            <input type="number" v-model.number="cfg.POS_CanhBaoKho" class="tl-input w-100" min="0" />
          </div>
        </div>
      </div>
    </div>

    <!-- TAB 3: Tích điểm -->
    <div v-show="activeTab==='loyalty'" class="tl-body">
      <div class="tl-card">
        <h6 class="tl-card-title"><i class="bi bi-star me-2"></i>Chương trình tích điểm</h6>
        <div class="tl-toggle-item mb-4">
          <div>
            <div class="tl-toggle-label">Bật tích điểm khách hàng</div>
            <div class="tl-toggle-desc">Khách hàng được tích điểm mỗi khi thanh toán</div>
          </div>
          <label class="tl-switch">
            <input type="checkbox" v-model="cfgBool.Loyalty_BatTat" />
            <span class="tl-slider"></span>
          </label>
        </div>
        <div class="row g-3" :class="{'opacity-50': !cfgBool.Loyalty_BatTat}">
          <div class="col-md-6">
            <label>Tỉ lệ tích điểm (VNĐ → điểm)</label>
            <div class="d-flex align-items-center gap-2">
              <input type="number" v-model.number="cfg.Loyalty_TiLeKiem" class="tl-input flex-fill" min="1000" step="1000" />
              <span style="color:var(--text-muted);white-space:nowrap;font-size:.82rem">đ = 1 điểm</span>
            </div>
          </div>
          <div class="col-md-6">
            <label>Tỉ lệ quy đổi (điểm → VNĐ)</label>
            <div class="d-flex align-items-center gap-2">
              <input type="number" v-model.number="cfg.Loyalty_TiLeDoiDiem" class="tl-input flex-fill" min="1" />
              <span style="color:var(--text-muted);white-space:nowrap;font-size:.82rem">điểm = 1.000đ</span>
            </div>
          </div>
          <div class="col-12">
            <label style="font-size:.78rem;font-weight:700;color:var(--text-muted)">NGƯỠNG HẠNG THÀNH VIÊN (điểm tích lũy)</label>
            <div class="row g-2 mt-1">
              <div class="col-md-4">
                <div class="rank-card rank-dong">
                  <i class="bi bi-award me-2"></i>Đồng
                  <input type="number" v-model.number="cfg.Loyalty_NguongDong" class="tl-input w-100 mt-2" min="0" placeholder="0" />
                </div>
              </div>
              <div class="col-md-4">
                <div class="rank-card rank-bac">
                  <i class="bi bi-award-fill me-2"></i>Bạc
                  <input type="number" v-model.number="cfg.Loyalty_NguongBac" class="tl-input w-100 mt-2" min="0" placeholder="500" />
                </div>
              </div>
              <div class="col-md-4">
                <div class="rank-card rank-vang">
                  <i class="bi bi-trophy-fill me-2"></i>Vàng
                  <input type="number" v-model.number="cfg.Loyalty_NguongVang" class="tl-input w-100 mt-2" min="0" placeholder="2000" />
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- TAB 4: Thanh toán -->
    <div v-show="activeTab==='payment'" class="tl-body">
      <div class="tl-card">
        <h6 class="tl-card-title"><i class="bi bi-qr-code me-2"></i>VietQR / Chuyển khoản</h6>
        <div class="row g-3">
          <div class="col-md-4">
            <label>Mã ngân hàng</label>
            <select v-model="cfg.Bank_Code" class="tl-select w-100">
              <option value="">-- Chọn ngân hàng --</option>
              <option v-for="b in banks" :key="b.code" :value="b.code">{{ b.name }}</option>
            </select>
          </div>
          <div class="col-md-4">
            <label>Số tài khoản</label>
            <input v-model="cfg.Bank_AccountNo" class="tl-input w-100" placeholder="0123456789" />
          </div>
          <div class="col-md-4">
            <label>Tên chủ tài khoản</label>
            <input v-model="cfg.Bank_AccountName" class="tl-input w-100" placeholder="NGUYEN VAN A" />
          </div>
          <div class="col-12" v-if="cfg.Bank_Code && cfg.Bank_AccountNo">
            <label>Preview QR</label>
            <div class="qr-preview">
              <img :src="`https://img.vietqr.io/image/${cfg.Bank_Code}-${cfg.Bank_AccountNo}-compact2.png?accountName=${encodeURIComponent(cfg.Bank_AccountName||'')}`"
                alt="VietQR" style="max-height:180px;border-radius:8px" />
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- TAB 5: Bảo mật & Nhân viên -->
    <div v-show="activeTab==='security'" class="tl-body">
      <div class="tl-card">
        <h6 class="tl-card-title"><i class="bi bi-shield-lock me-2"></i>Bảo mật vận hành</h6>
        <div class="tl-toggle-group">
          <div class="tl-toggle-item">
            <div>
              <div class="tl-toggle-label">Yêu cầu PIN nhân viên</div>
              <div class="tl-toggle-desc">Nhân viên cần xác nhận PIN 4 số khi thực hiện thao tác quan trọng</div>
            </div>
            <label class="tl-switch">
              <input type="checkbox" v-model="cfgBool.Security_YeuCauPIN" />
              <span class="tl-slider"></span>
            </label>
          </div>
          <div class="tl-toggle-item">
            <div>
              <div class="tl-toggle-label">Tự động đăng xuất khi không hoạt động</div>
              <div class="tl-toggle-desc">Bảo vệ màn hình POS khi bỏ trống</div>
            </div>
            <label class="tl-switch">
              <input type="checkbox" v-model="cfgBool.Security_AutoLogout" />
              <span class="tl-slider"></span>
            </label>
          </div>
        </div>
        <div class="row g-3 mt-2">
          <div class="col-md-6" v-if="cfgBool.Security_AutoLogout">
            <label>Thời gian tự đăng xuất (phút)</label>
            <input type="number" v-model.number="cfg.Security_TimeoutPhut" class="tl-input w-100" min="5" max="120" />
          </div>
        </div>
      </div>
      <div class="tl-card mt-3">
        <h6 class="tl-card-title"><i class="bi bi-info-circle me-2"></i>Gói dịch vụ hiện tại</h6>
        <div class="plan-info-box">
          <div class="plan-name">{{ info.goiDichVu ? info.goiDichVu.toUpperCase() : 'DÙNG THỬ' }}</div>
          <div class="plan-expire">Hết hạn: <strong>{{ formatDate(info.ngayHetHan) }}</strong></div>
          <router-link to="/admin/subscription" class="plan-upgrade-btn">
            <i class="bi bi-arrow-up-circle me-1"></i>Nâng cấp gói
          </router-link>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, inject } from 'vue';
import axios from 'axios';
const swal = inject('$swal');

const activeTab = ref('info');
const saving = ref(false);

const tabs = [
  { key: 'info', label: 'Thông tin CH', icon: 'shop' },
  { key: 'pos', label: 'Tùy chọn POS', icon: 'cash-register' },
  { key: 'loyalty', label: 'Tích điểm', icon: 'star' },
  { key: 'payment', label: 'Thanh toán', icon: 'qr-code' },
  { key: 'security', label: 'Bảo mật', icon: 'shield-lock' },
];

const banks = [
  { code: 'MBBank', name: 'MB Bank' }, { code: 'VCB', name: 'Vietcombank' },
  { code: 'TCB', name: 'Techcombank' }, { code: 'VPB', name: 'VPBank' },
  { code: 'ACB', name: 'ACB' }, { code: 'BIDV', name: 'BIDV' },
  { code: 'VTB', name: 'VietinBank' }, { code: 'TPB', name: 'TPBank' },
  { code: 'SHB', name: 'SHB' }, { code: 'MSB', name: 'MSB' },
];

const info = reactive({ tenCuaHang:'', soDienThoai:'', email:'', diaChi:'', logoUrl:'', trangThai:'', ngayHetHan:null, goiDichVu:'' });

const cfg = reactive({
  InHoaDon_TieuDe:'HÓA ĐƠN THANH TOÁN', InHoaDon_GhiChu:'Cảm ơn quý khách!', InHoaDon_Prefix:'HD',
  POS_GiamGiaMax:'20', POS_CanhBaoKho:'5',
  Loyalty_TiLeKiem:'10000', Loyalty_TiLeDoiDiem:'100',
  Loyalty_NguongDong:'0', Loyalty_NguongBac:'500', Loyalty_NguongVang:'2000',
  Bank_Code:'', Bank_AccountNo:'', Bank_AccountName:'',
  Security_TimeoutPhut:'30',
});

const cfgBool = reactive({
  POS_ChophepGiamGia:true, POS_TuDongIn:false, POS_XacNhanGuiBep:false, POS_HienQR:true,
  Loyalty_BatTat:false, Security_YeuCauPIN:false, Security_AutoLogout:true,
});

const boolKeys = Object.keys(cfgBool);
const allKeys = [...Object.keys(cfg), ...boolKeys].join(',');

const formatDate = (d) => d ? new Date(d).toLocaleDateString('vi-VN') : '—';

const load = async () => {
  try {
    const [si, tl] = await Promise.all([
      axios.get('/api/ThietLap/store-info'),
      axios.get('/api/ThietLap/batch', { params: { keys: allKeys } }),
    ]);
    Object.assign(info, si.data);
    const data = tl.data;
    for (const k of Object.keys(cfg)) if (data[k] !== undefined && data[k] !== '') cfg[k] = data[k];
    for (const k of boolKeys) if (data[k] !== undefined) cfgBool[k] = data[k] === 'true';
  } catch (e) { console.error(e); }
};

const saveAll = async () => {
  saving.value = true;
  try {
    const batch = { ...cfg };
    for (const k of boolKeys) batch[k] = String(cfgBool[k]);

    await Promise.all([
      axios.put('/api/ThietLap/store-info', { tenCuaHang: info.tenCuaHang, email: info.email, diaChi: info.diaChi, logoUrl: info.logoUrl }),
      axios.post('/api/ThietLap/batch', batch),
    ]);
    swal.fire({ toast:true, position:'top-end', icon:'success', title:'Đã lưu thiết lập!', timer:2000, showConfirmButton:false });
  } catch (e) {
    swal.fire('Lỗi', 'Lưu thất bại!', 'error');
  } finally { saving.value = false; }
};

onMounted(load);
</script>

<style scoped>
.thiet-lap-wrap { max-width: 960px; }
.tl-header { display:flex; justify-content:space-between; align-items:center; margin-bottom:20px; }
.tl-header h5 { margin:0; font-weight:700; font-size:1rem; color:var(--text-main,#1e293b); }
.tl-save-btn {
  display:flex; align-items:center; gap:6px;
  background:#f59e0b; border:none; color:#fff;
  padding:9px 20px; border-radius:8px; font-weight:700; font-size:.85rem; cursor:pointer;
  transition:.2s;
}
.tl-save-btn:hover { background:#d97706; }
.tl-save-btn:disabled { opacity:.6; cursor:not-allowed; }

/* Tabs */
.tl-tabs { display:flex; gap:4px; border-bottom:2px solid #e2e8f0; margin-bottom:20px; flex-wrap:wrap; }
.tl-tab {
  background:none; border:none; padding:9px 16px;
  border-bottom:3px solid transparent; margin-bottom:-2px;
  font-size:.83rem; font-weight:600; cursor:pointer;
  color:#64748b; border-radius:6px 6px 0 0; transition:.2s;
}
.tl-tab:hover { background:#f1f5f9; color:#334155; }
.tl-tab.active { color:#f59e0b; border-bottom-color:#f59e0b; background:#fffbeb; }

/* Card */
.tl-card {
  background:#fff; border:1px solid #e2e8f0;
  border-radius:12px; padding:20px;
  box-shadow:0 1px 3px rgba(0,0,0,.06);
}
.tl-card-title { font-size:.8rem; font-weight:700; color:#64748b; text-transform:uppercase; letter-spacing:.5px; margin-bottom:16px; }

/* Input */
.tl-input {
  background:#f8fafc; border:1px solid #e2e8f0;
  border-radius:8px; padding:8px 12px; font-size:.85rem;
  color:#1e293b; transition:.2s; outline:none;
}
.tl-input:focus { border-color:#f59e0b; background:#fff; }
.tl-input:disabled { opacity:.6; cursor:not-allowed; }
.tl-select {
  background:#f8fafc; border:1px solid #e2e8f0;
  border-radius:8px; padding:8px 12px; font-size:.85rem; color:#1e293b; outline:none;
}
.tl-hint { font-size:.72rem; color:#94a3b8; }

/* Toggle */
.tl-toggle-group { display:flex; flex-direction:column; gap:0; }
.tl-toggle-item {
  display:flex; justify-content:space-between; align-items:center;
  padding:14px 0; border-bottom:1px solid #f1f5f9;
}
.tl-toggle-item:last-child { border-bottom:none; }
.tl-toggle-label { font-weight:600; font-size:.88rem; color:#1e293b; }
.tl-toggle-desc { font-size:.75rem; color:#94a3b8; margin-top:2px; }

/* Switch */
.tl-switch { position:relative; width:44px; height:24px; flex-shrink:0; }
.tl-switch input { opacity:0; width:0; height:0; }
.tl-slider {
  position:absolute; top:0; left:0; right:0; bottom:0;
  background:#cbd5e1; border-radius:24px; cursor:pointer; transition:.3s;
}
.tl-slider:before {
  content:''; position:absolute; width:18px; height:18px;
  left:3px; bottom:3px; background:#fff; border-radius:50%; transition:.3s;
}
.tl-switch input:checked + .tl-slider { background:#f59e0b; }
.tl-switch input:checked + .tl-slider:before { transform:translateX(20px); }

/* Rank cards */
.rank-card { border-radius:10px; padding:14px; text-align:center; font-weight:700; font-size:.82rem; }
.rank-dong { background:#fef3c7; color:#92400e; border:1px solid #fde68a; }
.rank-bac  { background:#f1f5f9; color:#475569; border:1px solid #cbd5e1; }
.rank-vang { background:#fffbeb; color:#b45309; border:1px solid #fcd34d; }

/* QR */
.qr-preview { background:#f8fafc; border:1px solid #e2e8f0; border-radius:10px; padding:16px; text-align:center; }

/* Plan info */
.plan-info-box { background:#fffbeb; border:1px solid #fde68a; border-radius:10px; padding:16px; display:flex; align-items:center; gap:16px; flex-wrap:wrap; }
.plan-name { font-weight:800; font-size:1.1rem; color:#92400e; }
.plan-expire { font-size:.85rem; color:#78350f; flex:1; }
.plan-upgrade-btn {
  background:#f59e0b; color:#fff; border:none; padding:8px 16px;
  border-radius:8px; font-weight:700; font-size:.82rem; text-decoration:none;
  display:inline-flex; align-items:center;
}
</style>
