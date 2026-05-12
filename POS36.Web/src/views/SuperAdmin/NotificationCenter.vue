<template>
  <div>
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h6 class="text-muted fw-bold mb-0"><i class="bi bi-bell me-2"></i>TRUNG TÂM THÔNG BÁO</h6>
    </div>
    <div class="row g-3">
      <div class="col-lg-5">
        <div class="dash-card">
          <h6 class="dash-title">Gửi thông báo mới</h6>
          <div class="mb-3">
            <label class="form-label small fw-bold text-muted">Gửi đến</label>
            <select v-model="form.cuaHangId" class="sa-input w-100">
              <option :value="null">Tất cả cửa hàng</option>
              <option v-for="s in storeList" :key="s.id" :value="s.id">{{ s.tenCuaHang }}</option>
            </select>
          </div>
          <div class="mb-3">
            <label class="form-label small fw-bold text-muted">Loại</label>
            <select v-model="form.loaiThongBao" class="sa-input w-100">
              <option value="ThongTin">Thông tin</option>
              <option value="CanhBao">Cảnh báo</option>
              <option value="KhanCap">Khẩn cấp</option>
            </select>
          </div>
          <div class="mb-3"><label class="form-label small fw-bold text-muted">Tiêu đề</label><input v-model="form.tieuDe" class="sa-input w-100" /></div>
          <div class="mb-3"><label class="form-label small fw-bold text-muted">Nội dung</label><textarea v-model="form.noiDung" class="sa-input w-100" rows="4"></textarea></div>
          <button class="btn btn-warning w-100 fw-bold" @click="send"><i class="bi bi-send me-1"></i>Gửi thông báo</button>
        </div>
      </div>
      <div class="col-lg-7">
        <div class="dash-card h-100">
          <h6 class="dash-title">Lịch sử thông báo</h6>
          <div v-for="n in notifications" :key="n.id" class="notif-item" :class="'notif-' + n.loaiThongBao">
            <div class="notif-header">
              <span class="notif-type-badge">{{ n.loaiThongBao }}</span>
              <span class="notif-date">{{ formatDate(n.ngayTao) }}</span>
            </div>
            <div class="notif-title">{{ n.tieuDe }}</div>
            <div class="notif-body">{{ n.noiDung }}</div>
            <div class="notif-target text-muted small">{{ n.cuaHangId ? `Cửa hàng #${n.cuaHangId}` : 'Tất cả' }}</div>
          </div>
          <div v-if="!notifications.length" class="text-muted text-center py-4">Chưa có thông báo</div>
        </div>
      </div>
    </div>
  </div>
</template>
<script setup>
import { ref, onMounted, inject } from "vue";
import axios from "axios";
const swal = inject("$swal");
const storeList = ref([]);
const notifications = ref([]);
const form = ref({ cuaHangId: null, tieuDe: "", noiDung: "", loaiThongBao: "ThongTin" });
const formatDate = (d) => d ? new Date(d).toLocaleDateString("vi-VN",{hour:"2-digit",minute:"2-digit"}) : "";
const load = async () => {
  try { const [s, n] = await Promise.all([axios.get("/api/SuperAdmin/stores"), axios.get("/api/SuperAdmin/notifications")]); storeList.value = s.data; notifications.value = n.data; } catch(e){}
};
const send = async () => {
  if(!form.value.tieuDe||!form.value.noiDung) return swal.fire("Lỗi","Vui lòng nhập đầy đủ","warning");
  try { await axios.post("/api/SuperAdmin/notifications", form.value); swal.fire({toast:true,position:"top-end",icon:"success",title:"Đã gửi!",timer:1500,showConfirmButton:false}); form.value={cuaHangId:null,tieuDe:"",noiDung:"",loaiThongBao:"ThongTin"}; load(); } catch(e){ swal.fire("Lỗi","Gửi thất bại","error"); }
};
onMounted(load);
</script>
<style scoped>
.dash-card{background:#1a1c23;border:1px solid rgba(255,255,255,.06);border-radius:14px;padding:22px}
.dash-title{font-weight:700;font-size:.82rem;color:#9ca3af;margin-bottom:16px;text-transform:uppercase;letter-spacing:.5px}
.sa-input{background:#16181d;color:#e4e4e7;border:1px solid rgba(255,255,255,.1);border-radius:8px;padding:8px 14px;font-size:.85rem}
.sa-input:focus{outline:none;border-color:#f59e0b}
.notif-item{padding:14px;border:1px solid rgba(255,255,255,.04);border-radius:10px;margin-bottom:8px;border-left:3px solid #6b7280}
.notif-ThongTin{border-left-color:#3b82f6}.notif-CanhBao{border-left-color:#f59e0b}.notif-KhanCap{border-left-color:#ef4444}
.notif-header{display:flex;justify-content:space-between;align-items:center;margin-bottom:6px}
.notif-type-badge{font-size:.7rem;font-weight:700;text-transform:uppercase;letter-spacing:.5px;color:#9ca3af}
.notif-date{font-size:.72rem;color:#6b7280}
.notif-title{font-weight:700;font-size:.9rem;color:#f4f4f5;margin-bottom:4px}
.notif-body{font-size:.82rem;color:#9ca3af;line-height:1.5}
</style>
