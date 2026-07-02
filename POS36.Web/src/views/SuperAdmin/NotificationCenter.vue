<template>
  <div>
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h6 class="fw-bold mb-0" style="color:var(--sa-text-faint);font-size:.75rem;text-transform:uppercase;letter-spacing:.5px">
        <i class="bi bi-bell me-2"></i>TRUNG TÂM THÔNG BÁO
      </h6>
    </div>
    <div class="row g-3">
      <!-- Form gửi -->
      <div class="col-lg-5">
        <div class="dash-card">
          <h6 class="dash-card-title"><i class="bi bi-send me-2"></i>Gửi thông báo mới</h6>
          <div class="mb-3">
            <label>Gửi đến</label>
            <div class="position-relative">
              <div 
                class="sa-select w-100 d-flex justify-content-between align-items-center cursor-pointer" 
                @click="showStoreDropdown = !showStoreDropdown"
              >
                <span>{{ getSelectedStoreName() }}</span>
                <i class="bi bi-chevron-down"></i>
              </div>
              
              <div v-if="showStoreDropdown" class="store-dropdown shadow-sm border rounded bg-white position-absolute w-100 z-3 mt-1">
                <div class="p-2 border-bottom">
                  <input v-model="searchQuery" class="form-control form-control-sm" placeholder="Tìm tên / SĐT cửa hàng..." autofocus>
                </div>
                <div class="dropdown-list" style="max-height: 200px; overflow-y: auto;">
                  <div class="dropdown-item p-2 cursor-pointer" @click="selectStore(null)">
                    📢 Tất cả cửa hàng
                  </div>
                  <div 
                    v-for="s in filteredStores" :key="s.id" 
                    class="dropdown-item p-2 cursor-pointer" 
                    @click="selectStore(s.id)"
                  >
                    {{ s.tenCuaHang }} - {{ s.soDienThoai }}
                  </div>
                  <div v-if="filteredStores.length === 0" class="p-2 text-muted text-center small">
                    Không tìm thấy cửa hàng
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div class="mb-3">
            <label>Loại thông báo</label>
            <div class="d-flex gap-2">
              <button v-for="t in notifTypes" :key="t.val"
                class="type-btn flex-fill"
                :class="{ active: form.loaiThongBao === t.val }"
                :style="form.loaiThongBao === t.val ? `background:${t.color}1a;border-color:${t.color};color:${t.color}` : ''"
                @click="form.loaiThongBao = t.val">
                <i :class="'bi bi-' + t.icon + ' me-1'"></i>{{ t.label }}
              </button>
            </div>
          </div>
          <div class="mb-3">
            <label>Tiêu đề</label>
            <input v-model="form.tieuDe" class="sa-input w-100" placeholder="Tiêu đề thông báo..." />
          </div>
          <div class="mb-4">
            <label>Nội dung</label>
            <textarea v-model="form.noiDung" class="sa-input w-100" rows="4" placeholder="Nội dung chi tiết..."></textarea>
          </div>
          <button class="send-btn w-100" @click="send" :disabled="sending">
            <span v-if="sending" class="spinner-border spinner-border-sm me-2"></span>
            <i v-else class="bi bi-send-fill me-2"></i>
            {{ sending ? 'Đang gửi...' : 'Gửi thông báo' }}
          </button>
        </div>
      </div>

      <!-- Lịch sử -->
      <div class="col-lg-7">
        <div class="dash-card h-100">
          <h6 class="dash-card-title"><i class="bi bi-clock-history me-2"></i>Lịch sử thông báo</h6>
          <div class="notif-list">
            <div v-for="n in notifications" :key="n.id" class="notif-item" :class="'notif-' + n.loaiThongBao">
              <div class="notif-header">
                <span class="notif-badge" :class="'badge-' + n.loaiThongBao">
                  <i :class="'bi bi-' + typeIcon(n.loaiThongBao) + ' me-1'"></i>{{ n.loaiThongBao }}
                </span>
                <span style="font-size:.72rem;color:var(--sa-text-faint)">{{ formatDate(n.ngayTao) }}</span>
              </div>
              <div class="notif-title">{{ n.tieuDe }}</div>
              <div class="notif-body">{{ n.noiDung }}</div>
              <div class="notif-target">
                <i class="bi bi-shop me-1"></i>
                {{ n.cuaHangId ? `Cửa hàng #${n.cuaHangId}` : 'Tất cả cửa hàng' }}
              </div>
            </div>
            <div v-if="!notifications.length" class="text-center py-5" style="color:var(--sa-text-faint)">
              <i class="bi bi-bell-slash display-4 opacity-25 d-block mb-2"></i>Chưa có thông báo nào
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, inject, computed } from "vue";
import axios from "axios";
const swal = inject("$swal");

const storeList = ref([]);
const notifications = ref([]);
const sending = ref(false);
const form = ref({ cuaHangId: null, tieuDe: "", noiDung: "", loaiThongBao: "ThongTin" });

const searchQuery = ref("");
const showStoreDropdown = ref(false);

const filteredStores = computed(() => {
  if (!searchQuery.value) return storeList.value;
  const q = searchQuery.value.toLowerCase();
  return storeList.value.filter(s => 
    (s.tenCuaHang && s.tenCuaHang.toLowerCase().includes(q)) || 
    (s.soDienThoai && s.soDienThoai.includes(q))
  );
});

const getSelectedStoreName = () => {
  if (form.value.cuaHangId === null) return "📢 Tất cả cửa hàng";
  const store = storeList.value.find(s => s.id === form.value.cuaHangId);
  return store ? `${store.tenCuaHang} - ${store.soDienThoai}` : "📢 Tất cả cửa hàng";
};

const selectStore = (id) => {
  form.value.cuaHangId = id;
  showStoreDropdown.value = false;
  searchQuery.value = "";
};

const notifTypes = [
  { val: "ThongTin", label: "Thông tin", icon: "info-circle", color: "#3b82f6" },
  { val: "CanhBao", label: "Cảnh báo", icon: "exclamation-triangle", color: "#f59e0b" },
  { val: "KhanCap", label: "Khẩn cấp", icon: "exclamation-octagon", color: "#ef4444" },
];

const typeIcon = (t) => ({ ThongTin: "info-circle", CanhBao: "exclamation-triangle", KhanCap: "exclamation-octagon" }[t] || "bell");

const formatDate = (d) => d ? new Date(d).toLocaleString("vi-VN", { hour: "2-digit", minute: "2-digit", day: "2-digit", month: "2-digit" }) : "";

const load = async () => {
  try {
    const [s, n] = await Promise.all([
      axios.get("/api/SuperAdmin/stores"),
      axios.get("/api/SuperAdmin/notifications"),
    ]);
    storeList.value = s.data;
    notifications.value = n.data;
  } catch (e) { console.error(e); }
};

const send = async () => {
  if (!form.value.tieuDe || !form.value.noiDung)
    return swal.fire("Thiếu thông tin", "Vui lòng nhập tiêu đề và nội dung", "warning");
  sending.value = true;
  try {
    await axios.post("/api/SuperAdmin/notifications", form.value);
    swal.fire({ toast: true, position: "top-end", icon: "success", title: "Đã gửi thông báo!", timer: 1800, showConfirmButton: false });
    form.value = { cuaHangId: null, tieuDe: "", noiDung: "", loaiThongBao: "ThongTin" };
    load();
  } catch (e) {
    swal.fire("Lỗi", "Gửi thất bại", "error");
  } finally { sending.value = false; }
};

onMounted(load);
</script>

<style scoped>
@import "./sa-shared.css";

/* Type selector */
.type-btn {
  background: var(--sa-nav-hover-bg);
  border: 1px solid var(--sa-border);
  color: var(--sa-text-muted);
  padding: 7px 10px; border-radius: 8px;
  font-size: .75rem; font-weight: 600; cursor: pointer;
  transition: .2s; text-align: center;
}
.type-btn:hover { background: var(--sa-nav-active-bg); }

/* Send button */
.send-btn {
  background: var(--sa-accent); border: none;
  color: #fff; padding: 12px; border-radius: 8px;
  font-weight: 700; font-size: .9rem; cursor: pointer;
  transition: opacity .2s; display: flex; align-items: center; justify-content: center;
}
.send-btn:hover { opacity: .85; }
.send-btn:disabled { opacity: .6; cursor: not-allowed; }

/* Notification list */
.notif-list { display: flex; flex-direction: column; gap: 10px; max-height: 520px; overflow-y: auto; }
.notif-item {
  padding: 14px 16px;
  border: 1px solid var(--sa-border);
  border-left: 3px solid var(--sa-text-faint);
  border-radius: 10px;
  background: var(--sa-surface-2);
  transition: .2s;
}
.notif-item:hover { border-left-width: 4px; }
.notif-ThongTin { border-left-color: #3b82f6; }
.notif-CanhBao  { border-left-color: #f59e0b; }
.notif-KhanCap  { border-left-color: #ef4444; }

.notif-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 6px; }
.notif-badge {
  font-size: .68rem; font-weight: 700;
  text-transform: uppercase; letter-spacing: .5px;
  padding: 2px 8px; border-radius: 6px;
}
.badge-ThongTin { background: rgba(59,130,246,.15); color: #3b82f6; }
.badge-CanhBao  { background: rgba(245,158,11,.15); color: #f59e0b; }
.badge-KhanCap  { background: rgba(239,68,68,.15);  color: #ef4444; }
.notif-title { font-weight: 700; font-size: .9rem; color: var(--sa-text); margin-bottom: 4px; }
.notif-body  { font-size: .82rem; color: var(--sa-text-muted); line-height: 1.5; margin-bottom: 6px; }
.notif-target { font-size: .72rem; color: var(--sa-text-faint); }

/* Dropdown */
.store-dropdown {
  background: white;
  border-color: var(--sa-border);
}
.dropdown-item:hover {
  background-color: var(--sa-nav-hover-bg);
}
.cursor-pointer {
  cursor: pointer;
}
</style>
