<template>
  <div>
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h6 class="text-muted fw-bold mb-0"><i class="bi bi-box-seam me-2"></i>QUẢN LÝ GÓI DỊCH VỤ</h6>
      <button class="btn btn-warning btn-sm fw-bold" @click="openAdd"><i class="bi bi-plus-lg me-1"></i>Thêm gói</button>
    </div>
    <div class="row g-3">
      <div class="col-md-4" v-for="p in plans" :key="p.id">
        <div class="plan-card">
          <div class="plan-badge">{{ p.maGoi }}</div>
          <h5 class="fw-bold mt-2">{{ p.tenGoi }}</h5>
          <div class="plan-price">{{ formatVND(p.tongGia) }} <small class="text-muted">/ {{ p.soThang }} tháng</small></div>
          <div class="plan-info">
            <div><i class="bi bi-receipt me-1"></i>HĐ: {{ p.gioiHanHoaDon || '∞' }}/tháng</div>
            <div><i class="bi bi-people me-1"></i>NV: {{ p.gioiHanNhanVien || '∞' }}</div>
          </div>
          <div class="plan-desc text-muted small">{{ p.moTa || "Không có mô tả" }}</div>
          <div class="d-flex gap-2 mt-3">
            <button class="btn btn-sm btn-outline-warning flex-fill fw-bold" @click="openEdit(p)"><i class="bi bi-pencil me-1"></i>Sửa</button>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal -->
    <div class="sa-modal-overlay" v-if="showForm" @click.self="showForm = false">
      <div class="sa-modal" style="max-width: 500px">
        <div class="sa-modal-header">
          <h5>{{ editPlan ? 'Sửa gói' : 'Thêm gói mới' }}</h5>
          <button class="sa-modal-close" @click="showForm = false">&times;</button>
        </div>
        <div class="sa-modal-body">
          <div class="mb-3"><label class="form-label small fw-bold text-muted">Tên gói</label><input v-model="form.tenGoi" class="sa-input w-100" /></div>
          <div class="mb-3"><label class="form-label small fw-bold text-muted">Mã gói</label><input v-model="form.maGoi" class="sa-input w-100" /></div>
          <div class="row g-2 mb-3">
            <div class="col-4"><label class="form-label small fw-bold text-muted">Số tháng</label><input v-model.number="form.soThang" type="number" class="sa-input w-100" /></div>
            <div class="col-4"><label class="form-label small fw-bold text-muted">Giá/tháng</label><input v-model.number="form.giaThang" type="number" class="sa-input w-100" /></div>
            <div class="col-4"><label class="form-label small fw-bold text-muted">Tổng giá</label><input v-model.number="form.tongGia" type="number" class="sa-input w-100" /></div>
          </div>
          <div class="row g-2 mb-3">
            <div class="col-6"><label class="form-label small fw-bold text-muted">Giới hạn HĐ/tháng (0=∞)</label><input v-model.number="form.gioiHanHoaDon" type="number" class="sa-input w-100" /></div>
            <div class="col-6"><label class="form-label small fw-bold text-muted">Giới hạn NV (0=∞)</label><input v-model.number="form.gioiHanNhanVien" type="number" class="sa-input w-100" /></div>
          </div>
          <div class="mb-3"><label class="form-label small fw-bold text-muted">Mô tả</label><textarea v-model="form.moTa" class="sa-input w-100" rows="2"></textarea></div>
          <button class="btn btn-warning w-100 fw-bold" @click="save"><i class="bi bi-check-lg me-1"></i>{{ editPlan ? 'Cập nhật' : 'Tạo gói' }}</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, inject } from "vue";
import axios from "axios";
const swal = inject("$swal");
const plans = ref([]);
const showForm = ref(false);
const editPlan = ref(null);
const form = ref({ tenGoi: "", maGoi: "", soThang: 6, giaThang: 0, tongGia: 0, gioiHanHoaDon: 0, gioiHanNhanVien: 0, moTa: "", isActive: true, thuTuHienThi: 0 });

const formatVND = (n) => n ? Number(n).toLocaleString("vi-VN") + "đ" : "0đ";

const load = async () => {
  try { const res = await axios.get("/api/SuperAdmin/plans"); plans.value = res.data; } catch (e) { console.error(e); }
};

const openAdd = () => {
  editPlan.value = null;
  form.value = { tenGoi: "", maGoi: "", soThang: 6, giaThang: 0, tongGia: 0, gioiHanHoaDon: 0, gioiHanNhanVien: 0, moTa: "", isActive: true, thuTuHienThi: plans.value.length };
  showForm.value = true;
};

const openEdit = (p) => {
  editPlan.value = p;
  form.value = { ...p };
  showForm.value = true;
};

const save = async () => {
  try {
    if (editPlan.value) {
      await axios.put(`/api/SuperAdmin/plans/${editPlan.value.id}`, form.value);
    } else {
      await axios.post("/api/SuperAdmin/plans", form.value);
    }
    swal.fire({ toast: true, position: "top-end", icon: "success", title: "Đã lưu!", timer: 1500, showConfirmButton: false });
    showForm.value = false;
    load();
  } catch (e) { swal.fire("Lỗi", "Lưu thất bại", "error"); }
};

onMounted(load);
</script>

<style scoped>
.plan-card { background: #1a1c23; border: 1px solid rgba(255,255,255,0.06); border-radius: 14px; padding: 22px; transition: transform 0.2s; }
.plan-card:hover { transform: translateY(-3px); box-shadow: 0 12px 30px rgba(0,0,0,0.3); }
.plan-badge { display: inline-block; background: rgba(245,158,11,0.15); color: #f59e0b; padding: 3px 12px; border-radius: 6px; font-size: 0.7rem; font-weight: 800; letter-spacing: 1px; }
.plan-price { font-size: 1.5rem; font-weight: 800; color: #f59e0b; margin: 8px 0; }
.plan-info { display: flex; gap: 16px; font-size: 0.82rem; color: #9ca3af; margin-bottom: 8px; }
.plan-desc { line-height: 1.5; }
.sa-input { background: #16181d; color: #e4e4e7; border: 1px solid rgba(255,255,255,0.1); border-radius: 8px; padding: 8px 14px; font-size: 0.85rem; }
.sa-input:focus { outline: none; border-color: #f59e0b; }
.sa-modal-overlay { position: fixed; top: 0; left: 0; right: 0; bottom: 0; background: rgba(0,0,0,0.6); backdrop-filter: blur(4px); z-index: 1000; display: flex; align-items: center; justify-content: center; }
.sa-modal { background: #1a1c23; border: 1px solid rgba(255,255,255,0.08); border-radius: 16px; width: 90%; max-height: 80vh; overflow-y: auto; }
.sa-modal-header { display: flex; justify-content: space-between; align-items: center; padding: 18px 24px; border-bottom: 1px solid rgba(255,255,255,0.06); }
.sa-modal-header h5 { margin: 0; font-weight: 700; color: #f4f4f5; font-size: 1rem; }
.sa-modal-close { background: none; border: none; color: #6b7280; font-size: 1.5rem; cursor: pointer; }
.sa-modal-body { padding: 20px 24px; }
</style>
