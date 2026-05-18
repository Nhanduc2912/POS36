<template>
  <div>
    <!-- Header -->
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h6 class="fw-bold mb-0" style="color:var(--sa-text-faint);font-size:.75rem;text-transform:uppercase;letter-spacing:.5px">
        <i class="bi bi-gear me-2"></i>CẤU HÌNH HỆ THỐNG
      </h6>
      <button class="sa-action-btn" @click="saveAll" :disabled="saving">
        <span v-if="saving" class="spinner-border spinner-border-sm me-1"></span>
        <i v-else class="bi bi-floppy me-1"></i>{{ saving ? 'Đang lưu...' : 'Lưu tất cả' }}
      </button>
    </div>

    <!-- Tabs -->
    <div class="tabs-bar mb-4">
      <button class="tab-btn" v-for="t in tabs" :key="t.key"
        :class="{ active: activeTab === t.key }" @click="activeTab = t.key">
        <i :class="'bi bi-' + t.icon + ' me-1'"></i>{{ t.label }}
      </button>
    </div>

    <!-- Tab: Cấu hình chung -->
    <div v-show="activeTab === 'general'">
      <div class="row g-3">
        <div class="col-lg-6">
          <div class="dash-card">
            <h6 class="dash-card-title"><i class="bi bi-globe me-2"></i>Thông tin hệ thống</h6>
            <div class="mb-3">
              <label>Tên hệ thống</label>
              <input v-model="cfg.General.SiteName" class="sa-input w-100" placeholder="POS36" />
            </div>
            <div class="mb-3">
              <label>Slogan</label>
              <input v-model="cfg.General.Slogan" class="sa-input w-100" placeholder="Giải pháp quản lý nhà hàng..." />
            </div>
            <div class="mb-3">
              <label>Email hỗ trợ</label>
              <input v-model="cfg.General.SupportEmail" class="sa-input w-100" placeholder="support@pos36.vn" />
            </div>
            <div class="mb-3">
              <label>SĐT hỗ trợ</label>
              <input v-model="cfg.General.SupportPhone" class="sa-input w-100" placeholder="0901234567" />
            </div>
            <div class="mb-3">
              <label>Số ngày dùng thử</label>
              <input v-model.number="cfg.General.TrialDays" type="number" class="sa-input w-100" min="1" max="30" />
            </div>
          </div>
        </div>
        <div class="col-lg-6">
          <div class="dash-card mb-3">
            <h6 class="dash-card-title"><i class="bi bi-palette me-2"></i>Giao diện & Logo</h6>
            <div class="mb-3">
              <label>URL Logo (để trống dùng chữ)</label>
              <input v-model="cfg.General.SiteLogo" class="sa-input w-100" placeholder="https://..." />
            </div>
            <div class="mb-3">
              <label>Màu chủ đề</label>
              <div class="d-flex gap-2 align-items-center">
                <input type="color" v-model="cfg.General.PrimaryColor"
                  style="width:44px;height:38px;border-radius:8px;border:1px solid var(--sa-border);background:var(--sa-surface);cursor:pointer;padding:2px" />
                <input v-model="cfg.General.PrimaryColor" class="sa-input flex-fill" placeholder="#f59e0b" />
              </div>
            </div>
            <!-- Preview -->
            <div class="preview-box" :style="{ borderColor: cfg.General.PrimaryColor }">
              <div v-if="cfg.General.SiteLogo" class="mb-2">
                <img :src="cfg.General.SiteLogo" alt="Logo" style="max-height:50px;max-width:100%;border-radius:6px" />
              </div>
              <div class="fw-bold" :style="{ color: cfg.General.PrimaryColor }">{{ cfg.General.SiteName || 'POS36' }}</div>
              <div style="font-size:.78rem;color:var(--sa-text-faint)">{{ cfg.General.Slogan }}</div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Tab: Nhật ký hệ thống -->
    <div v-show="activeTab === 'logs'">
      <div class="d-flex gap-2 mb-3 flex-wrap align-items-center">
        <select v-model="logFilter" class="sa-select" @change="loadLogs">
          <option value="">Tất cả hành động</option>
          <option v-for="a in logActions" :key="a" :value="a">{{ a }}</option>
        </select>
        <input v-model="logDateFilter" type="date" class="sa-input" @change="loadLogs"
          style="color:var(--sa-text);background:var(--sa-surface)" />
        <span style="color:var(--sa-text-faint);font-size:.82rem;margin-left:auto">
          Tổng: <strong style="color:var(--sa-accent)">{{ logsTotal }}</strong> bản ghi
        </span>
      </div>
      <div class="sa-table-wrap">
        <table class="sa-table">
          <thead>
            <tr><th>Hành động</th><th>Mô tả</th><th>Người thực hiện</th><th>IP</th><th>Thời gian</th><th></th></tr>
          </thead>
          <tbody>
            <tr v-for="log in logs" :key="log.id" @click="viewLogDetail(log.id)" style="cursor:pointer">
              <td><span class="log-badge" :class="'log-' + log.hanhDong">{{ log.hanhDong }}</span></td>
              <td style="color:var(--sa-text);max-width:300px">{{ log.moTa }}</td>
              <td style="color:var(--sa-text-muted);font-size:.82rem">{{ log.nguoiThucHien || '—' }}</td>
              <td style="color:var(--sa-text-faint);font-size:.78rem"><code>{{ log.ipAddress || '—' }}</code></td>
              <td style="color:var(--sa-text-faint);font-size:.78rem">{{ formatDate(log.thoiGian) }}</td>
              <td>
                <a v-if="log.urlLienQuan" :href="log.urlLienQuan" class="sa-btn-sm sa-btn-info"
                  style="display:inline-flex;align-items:center;justify-content:center" title="Đến trang liên quan">
                  <i class="bi bi-arrow-right"></i>
                </a>
              </td>
            </tr>
            <tr v-if="!logs.length">
              <td colspan="6" class="text-center py-5" style="color:var(--sa-text-faint)">
                <i class="bi bi-journal-x display-4 opacity-25 d-block mb-2"></i>Chưa có nhật ký nào
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <!-- Pagination -->
      <div class="d-flex justify-content-center gap-2 mt-3" v-if="logsTotal > logPageSize">
        <button class="sa-btn-sm sa-btn-info" :disabled="logPage === 1" @click="logPage--; loadLogs()">
          <i class="bi bi-chevron-left"></i>
        </button>
        <span style="color:var(--sa-text-muted);font-size:.85rem;line-height:32px">{{ logPage }} / {{ Math.ceil(logsTotal/logPageSize) }}</span>
        <button class="sa-btn-sm sa-btn-info" :disabled="logPage >= Math.ceil(logsTotal/logPageSize)" @click="logPage++; loadLogs()">
          <i class="bi bi-chevron-right"></i>
        </button>
      </div>
    </div>

    <!-- Tab: Thanh toán & Webhook -->
    <div v-show="activeTab === 'payment'">
      <div class="row g-3">
        <div class="col-lg-6">
          <div class="dash-card">
            <h6 class="dash-card-title"><i class="bi bi-bank me-2"></i>Thông tin ngân hàng (VietQR)</h6>
            <div class="mb-3"><label>Mã ngân hàng</label><input v-model="cfg.Payment.BankCode" class="sa-input w-100" placeholder="MBBank" /></div>
            <div class="mb-3"><label>Số tài khoản</label><input v-model="cfg.Payment.BankAccountNo" class="sa-input w-100" placeholder="0123456789" /></div>
            <div class="mb-3"><label>Tên chủ tài khoản</label><input v-model="cfg.Payment.BankAccountName" class="sa-input w-100" placeholder="NGUYEN VAN A" /></div>
          </div>
        </div>
        <div class="col-lg-6">
          <div class="dash-card">
            <h6 class="dash-card-title"><i class="bi bi-webhook me-2"></i>SePay Webhook</h6>
            <div class="mb-3">
              <label>Webhook Secret Key</label>
              <div class="d-flex gap-2">
                <input :type="showSecret ? 'text' : 'password'" v-model="cfg.Payment.SePayWebhookSecret" class="sa-input flex-fill" placeholder="••••••••" />
                <button class="sa-btn-sm sa-btn-info" @click="showSecret = !showSecret">
                  <i :class="'bi bi-' + (showSecret ? 'eye-slash' : 'eye')"></i>
                </button>
              </div>
            </div>
            <div class="webhook-info">
              <i class="bi bi-info-circle me-2 text-info"></i>
              <span style="font-size:.8rem;color:var(--sa-text-muted)">
                URL webhook: <code style="color:var(--sa-accent)">{{ backendUrl }}/api/Payment/webhook</code>
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Tab: Email -->
    <div v-show="activeTab === 'email'">
      <div class="row g-3">
        <div class="col-lg-6">
          <div class="dash-card">
            <h6 class="dash-card-title"><i class="bi bi-envelope me-2"></i>EmailJS (Form liên hệ / Landing page)</h6>
            <div class="mb-3"><label>Service ID</label><input v-model="cfg.Email.EmailJsServiceId" class="sa-input w-100" placeholder="service_xxx" /></div>
            <div class="mb-3"><label>Template ID</label><input v-model="cfg.Email.EmailJsTemplateId" class="sa-input w-100" placeholder="template_xxx" /></div>
            <div class="mb-3">
              <label>Public Key</label>
              <div class="d-flex gap-2">
                <input :type="showEmailKey ? 'text' : 'password'" v-model="cfg.Email.EmailJsPublicKey" class="sa-input flex-fill" placeholder="••••••••" />
                <button class="sa-btn-sm sa-btn-info" @click="showEmailKey = !showEmailKey"><i :class="'bi bi-eye' + (showEmailKey ? '-slash' : '')"></i></button>
              </div>
            </div>
            <button class="sa-action-btn w-100 justify-content-center" @click="testEmail">
              <i class="bi bi-send me-1"></i>Test gửi email
            </button>
          </div>
        </div>
        <div class="col-lg-6">
          <div class="dash-card">
            <h6 class="dash-card-title"><i class="bi bi-server me-2"></i>SMTP (Email hệ thống)</h6>
            <div class="row g-2 mb-3">
              <div class="col-8"><label>SMTP Host</label><input v-model="cfg.Email.SmtpHost" class="sa-input w-100" placeholder="smtp.gmail.com" /></div>
              <div class="col-4"><label>Port</label><input v-model="cfg.Email.SmtpPort" class="sa-input w-100" placeholder="587" /></div>
            </div>
            <div class="mb-3"><label>Username</label><input v-model="cfg.Email.SmtpUser" class="sa-input w-100" placeholder="yourmail@gmail.com" /></div>
            <div class="mb-3">
              <label>Password (App Password)</label>
              <div class="d-flex gap-2">
                <input :type="showSmtpPw ? 'text' : 'password'" v-model="cfg.Email.SmtpPassword" class="sa-input flex-fill" placeholder="••••••••" />
                <button class="sa-btn-sm sa-btn-info" @click="showSmtpPw = !showSmtpPw"><i :class="'bi bi-eye' + (showSmtpPw ? '-slash' : '')"></i></button>
              </div>
            </div>
            <div class="mb-3"><label>Tên hiển thị khi gửi</label><input v-model="cfg.Email.SmtpFromName" class="sa-input w-100" placeholder="POS36 System" /></div>
          </div>
        </div>
      </div>
    </div>

    <!-- Log Detail Modal -->
    <div class="sa-modal-overlay" v-if="logDetail" @click.self="logDetail = null">
      <div class="sa-modal" style="max-width:600px">
        <div class="sa-modal-header">
          <h5><i class="bi bi-journal-text me-2"></i>Chi tiết nhật ký #{{ logDetail.id }}</h5>
          <button class="sa-modal-close" @click="logDetail = null">&times;</button>
        </div>
        <div class="sa-modal-body">
          <div class="row g-3 mb-3">
            <div class="col-6"><label>Hành động</label><p><span class="log-badge" :class="'log-' + logDetail.hanhDong">{{ logDetail.hanhDong }}</span></p></div>
            <div class="col-6"><label>Người thực hiện</label><p>{{ logDetail.nguoiThucHien || '—' }}</p></div>
            <div class="col-6"><label>Thời gian</label><p>{{ formatDate(logDetail.thoiGian) }}</p></div>
            <div class="col-6"><label>IP Address</label><p><code>{{ logDetail.ipAddress || '—' }}</code></p></div>
            <div class="col-12"><label>Mô tả</label><p>{{ logDetail.moTa }}</p></div>
            <div class="col-12" v-if="logDetail.urlLienQuan">
              <label>Trang liên quan</label>
              <p><a :href="logDetail.urlLienQuan" style="color:var(--sa-accent)">{{ logDetail.urlLienQuan }}</a></p>
            </div>
          </div>
          <div v-if="logDetail.chiTietJson" class="mb-3">
            <label>Chi tiết thay đổi</label>
            <pre class="log-json">{{ formatJson(logDetail.chiTietJson) }}</pre>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, inject } from "vue";
import axios from "axios";
const swal = inject("$swal");

const activeTab = ref("general");
const saving = ref(false);
const showSecret = ref(false);
const showEmailKey = ref(false);
const showSmtpPw = ref(false);
const backendUrl = "http://localhost:5098";

const tabs = [
  { key: "general", label: "Cấu hình chung", icon: "sliders" },
  { key: "logs", label: "Nhật ký hệ thống", icon: "journal-text" },
  { key: "payment", label: "Thanh toán & Webhook", icon: "credit-card" },
  { key: "email", label: "Email & SMTP", icon: "envelope" },
];

// Config state
const cfg = reactive({
  General: { SiteName: "", Slogan: "", SupportEmail: "", SupportPhone: "", SiteLogo: "", PrimaryColor: "#f59e0b", TrialDays: 7 },
  Payment: { BankCode: "", BankAccountNo: "", BankAccountName: "", SePayWebhookSecret: "" },
  Email: { EmailJsServiceId: "", EmailJsTemplateId: "", EmailJsPublicKey: "", SmtpHost: "smtp.gmail.com", SmtpPort: "587", SmtpUser: "", SmtpPassword: "", SmtpFromName: "POS36 System" },
});

// Logs
const logs = ref([]);
const logsTotal = ref(0);
const logPage = ref(1);
const logPageSize = 50;
const logFilter = ref("");
const logDateFilter = ref("");
const logDetail = ref(null);
const logActions = ["Tao", "Sua", "Xoa", "CauHinh", "KhoaCuaHang", "TestEmail", "DangNhap"];

const formatDate = (d) => d ? new Date(d).toLocaleString("vi-VN") : "—";
const formatJson = (j) => { try { return JSON.stringify(JSON.parse(j), null, 2); } catch { return j; } };

const loadConfig = async () => {
  try {
    const r = await axios.get("/api/CauHinh");
    const data = r.data;
    for (const [nhom, items] of Object.entries(data)) {
      if (!cfg[nhom]) continue;
      for (const item of items) {
        if (cfg[nhom][item.maKey] !== undefined) cfg[nhom][item.maKey] = item.giaTri;
      }
    }
  } catch (e) { console.error(e); }
};

const loadLogs = async () => {
  try {
    const params = { page: logPage.value, pageSize: logPageSize };
    if (logFilter.value) params.hanhDong = logFilter.value;
    if (logDateFilter.value) params.tuNgay = logDateFilter.value;
    const r = await axios.get("/api/CauHinh/nhat-ky", { params });
    logs.value = r.data.data;
    logsTotal.value = r.data.total;
  } catch (e) { console.error(e); }
};

const viewLogDetail = async (id) => {
  try {
    const r = await axios.get(`/api/CauHinh/nhat-ky/${id}`);
    logDetail.value = r.data;
  } catch (e) { console.error(e); }
};

const saveAll = async () => {
  saving.value = true;
  try {
    const items = [];
    for (const [nhom, keys] of Object.entries(cfg)) {
      for (const [key, val] of Object.entries(keys)) {
        items.push({ nhomCauHinh: nhom, maKey: key, giaTri: String(val) });
      }
    }
    await axios.put("/api/CauHinh/batch", items);
    swal.fire({ toast: true, position: "top-end", icon: "success", title: "Đã lưu cấu hình!", timer: 2000, showConfirmButton: false });
  } catch (e) {
    swal.fire("Lỗi", "Lưu cấu hình thất bại", "error");
  } finally { saving.value = false; }
};

const testEmail = async () => {
  const r = await swal.fire({ title: "Test gửi email", input: "email", inputPlaceholder: "Nhập email nhận...", showCancelButton: true });
  if (!r.isConfirmed) return;
  try {
    await axios.post("/api/CauHinh/test-email", { to: r.value });
    swal.fire({ toast: true, position: "top-end", icon: "success", title: "Đã ghi nhận! EmailJS sẽ gửi qua frontend.", timer: 2500, showConfirmButton: false });
  } catch (e) { swal.fire("Lỗi", "Thất bại", "error"); }
};

onMounted(() => { loadConfig(); loadLogs(); });
</script>

<style scoped>
@import "./sa-shared.css";

.sa-action-btn {
  display: flex; align-items: center; gap: 6px;
  background: rgba(245,158,11,.15); border: 1px solid rgba(245,158,11,.3);
  color: var(--sa-accent); padding: 8px 16px; border-radius: 8px;
  font-size: .82rem; font-weight: 700; cursor: pointer; transition: .2s;
}
.sa-action-btn:hover { background: rgba(245,158,11,.25); }
.sa-action-btn:disabled { opacity: .6; cursor: not-allowed; }

.tabs-bar { display: flex; gap: 4px; border-bottom: 1px solid var(--sa-border); }
.tab-btn {
  background: none; border: none; color: var(--sa-text-muted);
  padding: 10px 18px; border-radius: 8px 8px 0 0;
  font-size: .85rem; font-weight: 600; cursor: pointer;
  border-bottom: 2px solid transparent; transition: .2s;
}
.tab-btn:hover { color: var(--sa-text); background: var(--sa-nav-hover-bg); }
.tab-btn.active { color: var(--sa-accent); border-bottom-color: var(--sa-accent); background: var(--sa-nav-active-bg); }

.preview-box {
  border: 2px solid; border-radius: 10px;
  padding: 16px; margin-top: 8px;
  background: var(--sa-nav-hover-bg);
  text-align: center;
}

.log-badge {
  padding: 3px 10px; border-radius: 6px;
  font-size: .72rem; font-weight: 700; letter-spacing: .3px;
  background: var(--sa-nav-hover-bg); color: var(--sa-text-muted);
}
.log-Tao { background: rgba(34,197,94,.15); color: #22c55e; }
.log-Sua { background: rgba(59,130,246,.15); color: #3b82f6; }
.log-Xoa { background: rgba(239,68,68,.15); color: #ef4444; }
.log-CauHinh { background: rgba(245,158,11,.15); color: #f59e0b; }
.log-KhoaCuaHang { background: rgba(239,68,68,.15); color: #ef4444; }
.log-DangNhap { background: rgba(139,92,246,.15); color: #8b5cf6; }

.log-json {
  background: var(--sa-surface-2); border: 1px solid var(--sa-border);
  border-radius: 8px; padding: 12px; font-size: .78rem;
  color: var(--sa-text-muted); overflow-x: auto;
  max-height: 200px; overflow-y: auto;
}

.webhook-info {
  background: rgba(59,130,246,.08);
  border: 1px solid rgba(59,130,246,.2);
  border-radius: 8px; padding: 10px 14px;
  margin-top: 8px;
}
</style>
