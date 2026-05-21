<template>
  <div class="cfg-root">
    <!-- Header -->
    <div class="cfg-header">
      <div>
        <h6 class="cfg-page-title">
          <i class="bi bi-gear-wide-connected me-2"></i>CẤU HÌNH HỆ THỐNG
        </h6>
        <p class="cfg-page-sub">Quản lý toàn bộ cấu hình và nhật ký hoạt động của POS36</p>
      </div>
      <button class="sa-action-btn" @click="saveAll" :disabled="saving">
        <span v-if="saving" class="spinner-border spinner-border-sm me-1"></span>
        <i v-else class="bi bi-floppy me-1"></i>{{ saving ? 'Đang lưu...' : 'Lưu tất cả' }}
      </button>
    </div>

    <!-- Layout: Sidebar + Content -->
    <div class="cfg-layout">
      <!-- Sidebar Navigation -->
      <aside class="cfg-sidebar">
        <!-- Single "CẤU HÌNH" category header -->
        <div class="cfg-sidebar-cat">
          <i class="bi bi-gear-wide-connected me-2"></i>CẤU HÌNH
        </div>

        <!-- 4 direct menu items -->
        <button
          v-for="item in navItems"
          :key="item.key"
          class="cfg-nav-btn"
          :class="{ active: activeTab === item.key }"
          @click="activeTab = item.key"
        >
          <i :class="'bi bi-' + item.icon"></i>
          <span>{{ item.label }}</span>
          <i v-if="activeTab === item.key" class="bi bi-chevron-right cfg-nav-arrow"></i>
        </button>
      </aside>

      <!-- Main Content -->
      <main class="cfg-content">
        <!-- ====== Thông tin hệ thống ====== -->
        <div v-show="activeTab === 'general'">
          <div class="cfg-content-title">
            <i class="bi bi-sliders me-2"></i>Cấu hình chung
          </div>
          <div class="dash-card">
            <div class="row g-3">
              <div class="col-lg-6">
                <div class="mb-3"><label>Tên hệ thống</label><input v-model="cfg.General.SiteName" class="sa-input w-100" placeholder="POS36" /></div>
                <div class="mb-3"><label>Slogan</label><input v-model="cfg.General.Slogan" class="sa-input w-100" placeholder="Giải pháp quản lý nhà hàng..." /></div>
                <div class="mb-3"><label>Email hỗ trợ</label><input v-model="cfg.General.SupportEmail" class="sa-input w-100" placeholder="support@pos36.vn" /></div>
                <div class="mb-3"><label>SĐT hỗ trợ</label><input v-model="cfg.General.SupportPhone" class="sa-input w-100" placeholder="0901234567" /></div>
                <div class="mb-3"><label>Số ngày dùng thử</label><input v-model.number="cfg.General.TrialDays" type="number" class="sa-input w-100" min="1" max="30" /></div>
              </div>
              <div class="col-lg-6">
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

        <!-- ====== Nhật ký hệ thống ====== -->
        <div v-show="activeTab === 'logs'">
          <div class="cfg-content-title">
            <i class="bi bi-journal-text me-2"></i>Nhật ký hệ thống
          </div>
          <!-- Filter bar -->
          <div class="d-flex gap-2 mb-3 flex-wrap align-items-center">
            <select v-model="logFilter" class="sa-select" @change="resetAndLoadLogs">
              <option value="">Tất cả hành động</option>
              <option v-for="a in logActions" :key="a" :value="a">{{ a }}</option>
            </select>
            <input v-model="logDateFilter" type="date" class="sa-input" @change="resetAndLoadLogs"
              style="color:var(--sa-text);background:var(--sa-surface)" />
            <button class="sa-btn-sm sa-btn-info" @click="resetAndLoadLogs" title="Làm mới">
              <i class="bi bi-arrow-clockwise"></i>
            </button>
            <span style="color:var(--sa-text-faint);font-size:.82rem;margin-left:auto">
              Hiển thị: <strong style="color:var(--sa-accent)">{{ logs.length }}</strong>
              / Tổng: <strong style="color:var(--sa-accent)">{{ logsTotal }}</strong> bản ghi
            </span>
          </div>
          <!-- Table -->
          <div class="sa-table-wrap">
            <table class="sa-table">
              <thead>
                <tr><th>Hành động</th><th>Mô tả</th><th>Người thực hiện</th><th>IP</th><th>Thời gian</th><th></th></tr>
              </thead>
              <tbody>
                <tr v-if="logsLoading && !logs.length">
                  <td colspan="6" class="text-center py-5">
                    <span class="spinner-border spinner-border-sm me-2" style="color:var(--sa-accent)"></span>
                    <span style="color:var(--sa-text-faint)">Đang tải...</span>
                  </td>
                </tr>
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
                <tr v-if="!logs.length && !logsLoading">
                  <td colspan="6" class="text-center py-5" style="color:var(--sa-text-faint)">
                    <i class="bi bi-journal-x display-4 opacity-25 d-block mb-2"></i>Chưa có nhật ký nào
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- Pagination - ALWAYS VISIBLE when data exists -->
          <div class="logs-pagination" v-if="logsTotal > 0">
            <div class="logs-page-info">
              Trang <strong>{{ logPage }}</strong> / <strong>{{ totalPages }}</strong>
              &nbsp;•&nbsp; {{ logsTotal }} bản ghi
            </div>
            <div class="logs-page-btns">
              <button class="pg-btn" :disabled="logPage <= 1" @click="goToPage(1)" title="Trang đầu">
                <i class="bi bi-chevron-double-left"></i>
              </button>
              <button class="pg-btn" :disabled="logPage <= 1" @click="goToPage(logPage - 1)">
                <i class="bi bi-chevron-left"></i>
              </button>
              <!-- Page number buttons -->
              <button
                v-for="p in pageNumbers"
                :key="p"
                class="pg-btn"
                :class="{ 'pg-btn-active': p === logPage, 'pg-dots': p === '...' }"
                :disabled="p === '...'"
                @click="p !== '...' && goToPage(p)"
              >{{ p }}</button>
              <button class="pg-btn" :disabled="logPage >= totalPages" @click="goToPage(logPage + 1)">
                <i class="bi bi-chevron-right"></i>
              </button>
              <button class="pg-btn" :disabled="logPage >= totalPages" @click="goToPage(totalPages)" title="Trang cuối">
                <i class="bi bi-chevron-double-right"></i>
              </button>
            </div>
            <div class="logs-page-size">
              <select v-model.number="logPageSize" class="sa-select" @change="resetAndLoadLogs" style="font-size:.78rem">
                <option :value="20">20/trang</option>
                <option :value="50">50/trang</option>
                <option :value="100">100/trang</option>
              </select>
            </div>
          </div>
        </div>

        <!-- ====== Thanh toán & Webhook ====== -->
        <div v-show="activeTab === 'payment'">
          <div class="cfg-content-title"><i class="bi bi-credit-card me-2"></i>Thanh toán & Webhook</div>
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
                    <button class="sa-btn-sm sa-btn-info" @click="showSecret = !showSecret"><i :class="'bi bi-' + (showSecret ? 'eye-slash' : 'eye')"></i></button>
                  </div>
                </div>
                <div class="webhook-info">
                  <i class="bi bi-info-circle me-2 text-info"></i>
                  <span style="font-size:.8rem;color:var(--sa-text-muted)">URL webhook: <code style="color:var(--sa-accent)">{{ backendUrl }}/api/Payment/webhook</code></span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- ====== Email & SMTP ====== -->
        <div v-show="activeTab === 'email'">
          <div class="cfg-content-title"><i class="bi bi-envelope me-2"></i>Email & SMTP</div>
          <div class="dash-card">
            <div class="mb-3"><label>Service ID</label><input v-model="cfg.Email.EmailJsServiceId" class="sa-input w-100" placeholder="service_xxx" /></div>
            <div class="mb-3"><label>Template ID</label><input v-model="cfg.Email.EmailJsTemplateId" class="sa-input w-100" placeholder="template_xxx" /></div>
            <div class="mb-3">
              <label>Public Key</label>
              <div class="d-flex gap-2">
                <input :type="showEmailKey ? 'text' : 'password'" v-model="cfg.Email.EmailJsPublicKey" class="sa-input flex-fill" placeholder="••••••••" />
                <button class="sa-btn-sm sa-btn-info" @click="showEmailKey = !showEmailKey"><i :class="'bi bi-eye' + (showEmailKey ? '-slash' : '')"></i></button>
              </div>
            </div>
            <button class="sa-action-btn" @click="testEmail"><i class="bi bi-send me-1"></i>Test gửi email</button>
          </div>
        </div>

        <!-- ====== SMTP (inside email tab) ====== -->
        <div v-show="activeTab === 'email'">
          <div class="cfg-content-title"><i class="bi bi-server me-2"></i>SMTP (Email hệ thống)</div>
          <div class="dash-card">
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
      </main>
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
import { ref, reactive, onMounted, inject, computed, watch } from "vue";
import axios from "axios";
const swal = inject("$swal");

// ===== STATE =====
const saving = ref(false);
const showSecret = ref(false);
const showEmailKey = ref(false);
const showSmtpPw = ref(false);
const backendUrl = "http://localhost:5098";

// ===== SIMPLE NAV ITEMS =====
const activeTab = ref("general");

const navItems = [
  { key: "general",  label: "Cấu hình chung",       icon: "sliders" },
  { key: "logs",     label: "Nhật ký hệ thống",    icon: "journal-text" },
  { key: "payment",  label: "Thanh toán & Webhook", icon: "credit-card" },
  { key: "email",    label: "Email & SMTP",         icon: "envelope" },
];

// ===== CONFIG STATE =====
const cfg = reactive({
  General: { SiteName: "", Slogan: "", SupportEmail: "", SupportPhone: "", SiteLogo: "", PrimaryColor: "#f59e0b", TrialDays: 7 },
  Payment: { BankCode: "", BankAccountNo: "", BankAccountName: "", SePayWebhookSecret: "" },
  Email: { EmailJsServiceId: "", EmailJsTemplateId: "", EmailJsPublicKey: "", SmtpHost: "smtp.gmail.com", SmtpPort: "587", SmtpUser: "", SmtpPassword: "", SmtpFromName: "POS36 System" },
});

// ===== LOGS STATE =====
const logs = ref([]);
const logsTotal = ref(0);
const logPage = ref(1);
const logPageSize = ref(50);
const logFilter = ref("");
const logDateFilter = ref("");
const logDetail = ref(null);
const logsLoading = ref(false);
const logActions = ["Tao", "Sua", "Xoa", "CauHinh", "KhoaCuaHang", "TestEmail", "DangNhap"];

// Computed: total pages
const totalPages = computed(() => Math.max(1, Math.ceil(logsTotal.value / logPageSize.value)));

// Smart page numbers with ellipsis
const pageNumbers = computed(() => {
  const total = totalPages.value;
  const cur = logPage.value;
  if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1);
  const pages = [1];
  if (cur > 3) pages.push('...');
  for (let i = Math.max(2, cur - 1); i <= Math.min(total - 1, cur + 1); i++) pages.push(i);
  if (cur < total - 2) pages.push('...');
  pages.push(total);
  return pages;
});

// ===== UTILS =====
const formatDate = (d) => d ? new Date(d).toLocaleString("vi-VN") : "—";
const formatJson = (j) => { try { return JSON.stringify(JSON.parse(j), null, 2); } catch { return j; } };

// ===== LOAD FUNCTIONS =====
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
  logsLoading.value = true;
  try {
    const params = { page: logPage.value, pageSize: logPageSize.value };
    if (logFilter.value) params.hanhDong = logFilter.value;
    if (logDateFilter.value) params.tuNgay = logDateFilter.value;
    const r = await axios.get("/api/CauHinh/nhat-ky", { params });
    logs.value = r.data.data;
    logsTotal.value = r.data.total;
  } catch (e) { console.error(e); } finally { logsLoading.value = false; }
};

const resetAndLoadLogs = () => {
  logPage.value = 1;
  loadLogs();
};

const goToPage = (p) => {
  if (p < 1 || p > totalPages.value) return;
  logPage.value = p;
  loadLogs();
  // Scroll to top of logs table
  document.querySelector('.sa-table-wrap')?.scrollIntoView({ behavior: 'smooth', block: 'start' });
};

const viewLogDetail = async (id) => {
  try {
    const r = await axios.get(`/api/CauHinh/nhat-ky/${id}`);
    logDetail.value = r.data;
  } catch (e) { console.error(e); }
};

// Auto-load logs when switching to logs tab
watch(activeTab, (tab) => {
  if (tab === 'logs' && !logs.value.length) loadLogs();
});

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

onMounted(() => { loadConfig(); });
</script>

<style scoped>
@import "./sa-shared.css";

/* ===== ROOT ===== */
.cfg-root { display: flex; flex-direction: column; height: 100%; gap: 0; }

/* ===== HEADER ===== */
.cfg-header {
  display: flex; justify-content: space-between; align-items: flex-start;
  margin-bottom: 20px; flex-wrap: wrap; gap: 12px;
}
.cfg-page-title {
  color: var(--sa-text-faint); font-size: .75rem;
  text-transform: uppercase; letter-spacing: .5px; margin: 0 0 4px;
}
.cfg-page-sub { color: var(--sa-text-muted); font-size: .8rem; margin: 0; }

.sa-action-btn {
  display: flex; align-items: center; gap: 6px;
  background: rgba(245,158,11,.15); border: 1px solid rgba(245,158,11,.3);
  color: var(--sa-accent); padding: 8px 16px; border-radius: 8px;
  font-size: .82rem; font-weight: 700; cursor: pointer; transition: .2s;
}
.sa-action-btn:hover { background: rgba(245,158,11,.25); }
.sa-action-btn:disabled { opacity: .6; cursor: not-allowed; }

/* ===== LAYOUT ===== */
.cfg-layout {
  display: flex; gap: 20px;
  flex: 1; min-height: 0;
}

/* ===== SIDEBAR ===== */
.cfg-sidebar {
  width: 200px; flex-shrink: 0;
  background: var(--sa-surface);
  border: 1px solid var(--sa-border);
  border-radius: 14px;
  padding: 8px;
  align-self: flex-start;
  position: sticky; top: 0;
}

/* Category header */
.cfg-sidebar-cat {
  padding: 10px 14px 8px;
  font-size: .7rem; font-weight: 800;
  color: var(--sa-text-faint);
  text-transform: uppercase; letter-spacing: 1px;
  border-bottom: 1px solid var(--sa-border);
  margin-bottom: 6px;
  display: flex; align-items: center;
}

/* Nav buttons */
.cfg-nav-btn {
  width: 100%;
  display: flex; align-items: center; gap: 10px;
  background: none; border: none;
  color: var(--sa-text-muted);
  padding: 10px 14px; border-radius: 9px;
  font-size: .85rem; font-weight: 500;
  cursor: pointer; transition: .15s; text-align: left;
  position: relative;
}
.cfg-nav-btn:hover { background: var(--sa-nav-hover-bg); color: var(--sa-text); }
.cfg-nav-btn.active {
  background: var(--sa-nav-active-bg);
  color: var(--sa-accent); font-weight: 700;
  border-left: 3px solid var(--sa-accent);
  padding-left: 11px;
}
.cfg-nav-btn i { font-size: .95rem; width: 18px; text-align: center; flex-shrink: 0; }
.cfg-nav-arrow { margin-left: auto; font-size: .7rem; }

/* ===== CONTENT ===== */
.cfg-content {
  flex: 1; min-width: 0;
  overflow-y: auto;
}
.cfg-content-title {
  font-size: .75rem; font-weight: 700;
  color: var(--sa-text-faint);
  text-transform: uppercase; letter-spacing: .5px;
  margin-bottom: 16px;
  display: flex; align-items: center;
}

/* ===== LOGS PAGINATION ===== */
.logs-pagination {
  display: flex; align-items: center; justify-content: space-between;
  margin-top: 16px; flex-wrap: wrap; gap: 12px;
  padding: 12px 16px;
  background: var(--sa-surface);
  border: 1px solid var(--sa-border);
  border-radius: 10px;
}
.logs-page-info { font-size: .82rem; color: var(--sa-text-muted); }
.logs-page-info strong { color: var(--sa-accent); }
.logs-page-btns { display: flex; gap: 4px; align-items: center; }
.logs-page-size { display: flex; align-items: center; }

.pg-btn {
  min-width: 34px; height: 34px;
  background: var(--sa-surface-2); border: 1px solid var(--sa-border);
  color: var(--sa-text-muted);
  border-radius: 8px; cursor: pointer;
  display: flex; align-items: center; justify-content: center;
  font-size: .82rem; font-weight: 600;
  transition: .15s; padding: 0 8px;
}
.pg-btn:hover:not(:disabled) { background: var(--sa-nav-hover-bg); color: var(--sa-accent); border-color: rgba(245,158,11,.3); }
.pg-btn-active {
  background: var(--sa-nav-active-bg) !important;
  color: var(--sa-accent) !important;
  border-color: rgba(245,158,11,.4) !important;
  font-weight: 800 !important;
}
.pg-btn:disabled { opacity: .35; cursor: not-allowed; }
.pg-dots { cursor: default !important; background: none !important; border-color: transparent !important; }

/* ===== MISC ===== */
.preview-box {
  border: 2px solid; border-radius: 10px;
  padding: 16px; margin-top: 8px;
  background: var(--sa-nav-hover-bg); text-align: center;
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
  background: rgba(59,130,246,.08); border: 1px solid rgba(59,130,246,.2);
  border-radius: 8px; padding: 10px 14px; margin-top: 8px;
  display: flex; align-items: center;
}
</style>
