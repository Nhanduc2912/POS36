<template>
  <div>
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h6 class="fw-bold mb-0" style="color:var(--sa-text-faint);font-size:.75rem;text-transform:uppercase;letter-spacing:.5px">
        <i class="bi bi-box-seam me-2"></i>QUẢN LÝ GÓI DỊCH VỤ
      </h6>
      <button class="sa-action-btn" @click="openAdd">
        <i class="bi bi-plus-lg me-1"></i>Thêm gói
      </button>
    </div>

    <div class="row g-3">
      <div class="col-md-4" v-for="p in plans" :key="p.id">
        <div class="plan-card" :class="{ 'plan-inactive': !p.isActive }">
          <div class="plan-header">
            <span class="plan-badge">{{ p.maGoi }}</span>
            <span class="plan-status" v-if="!p.isActive"><i class="bi bi-eye-slash me-1"></i>Ẩn</span>
          </div>
          <h5 class="fw-bold mt-2 mb-1" style="color:var(--sa-text)">{{ p.tenGoi }}</h5>
          <div class="plan-price">{{ formatVND(p.tongGia) }}
            <small style="color:var(--sa-text-muted);font-size:.8rem;font-weight:500"> / {{ p.soThang }} tháng</small>
          </div>
          <div class="plan-info">
            <span><i class="bi bi-receipt me-1"></i>HĐ: {{ p.gioiHanHoaDon || '∞' }}/tháng</span>
            <span><i class="bi bi-people me-1"></i>NV: {{ p.gioiHanNhanVien || '∞' }}</span>
          </div>
          <p class="plan-desc">{{ p.moTa || 'Không có mô tả' }}</p>
          <div class="d-flex gap-2 mt-3">
            <button class="sa-btn-sm sa-btn-info flex-fill" style="width:auto;height:auto;padding:6px 14px;border-radius:8px;font-size:.82rem;font-weight:600;gap:4px;display:flex;align-items:center;justify-content:center" @click="openEdit(p)">
              <i class="bi bi-pencil me-1"></i>Sửa
            </button>
            <button class="sa-btn-sm" :class="p.isActive ? 'sa-btn-warning' : 'sa-btn-success'"
              style="width:auto;height:auto;padding:6px 12px;border-radius:8px;font-size:.82rem;font-weight:600"
              @click="toggleActive(p)" :title="p.isActive ? 'Ẩn gói' : 'Hiện gói'">
              <i :class="p.isActive ? 'bi bi-eye-slash' : 'bi bi-eye'"></i>
            </button>
          </div>
        </div>
      </div>
      <div class="col-12 text-center py-5" v-if="!plans.length" style="color:var(--sa-text-faint)">
        <i class="bi bi-box-seam display-4 opacity-25 d-block mb-2"></i>Chưa có gói dịch vụ nào
      </div>
    </div>

    <!-- Modal Add/Edit -->
    <div class="sa-modal-overlay" v-if="showForm" @click.self="showForm = false">
      <div class="sa-modal" style="max-width:520px">
        <div class="sa-modal-header">
          <h5>{{ editPlan ? '✏️ Sửa gói' : '➕ Thêm gói mới' }}</h5>
          <button class="sa-modal-close" @click="showForm = false">&times;</button>
        </div>
        <div class="sa-modal-body">
          <div class="mb-3">
            <label>Tên gói</label>
            <input v-model="form.tenGoi" class="sa-input w-100" placeholder="VD: Gói Pro" />
          </div>
          <div class="mb-3">
            <label>Mã gói</label>
            <input v-model="form.maGoi" class="sa-input w-100" placeholder="VD: PRO" />
          </div>
          <div class="row g-2 mb-3">
            <div class="col-4">
              <label>Số tháng</label>
              <input v-model.number="form.soThang" type="number" class="sa-input w-100" min="1" />
            </div>
            <div class="col-4">
              <label>Giá/tháng (đ)</label>
              <input v-model.number="form.giaThang" type="number" class="sa-input w-100" min="0" />
            </div>
            <div class="col-4">
              <label>Tổng giá (đ)</label>
              <input v-model.number="form.tongGia" type="number" class="sa-input w-100" min="0" />
            </div>
          </div>
          <div class="row g-2 mb-3">
            <div class="col-6">
              <label>Giới hạn HĐ/tháng (0=∞)</label>
              <input v-model.number="form.gioiHanHoaDon" type="number" class="sa-input w-100" min="0" />
            </div>
            <div class="col-6">
              <label>Giới hạn NV (0=∞)</label>
              <input v-model.number="form.gioiHanNhanVien" type="number" class="sa-input w-100" min="0" />
            </div>
          </div>
          <div class="mb-3">
            <label>Mô tả</label>
            <textarea v-model="form.moTa" class="sa-input w-100" rows="3" placeholder="Mô tả ngắn về gói..."></textarea>
          </div>
          <div class="mb-4 d-flex align-items-center gap-2">
            <input type="checkbox" v-model="form.isActive" id="isActive" style="width:16px;height:16px;accent-color:var(--sa-accent)">
            <label for="isActive" style="font-size:.85rem;color:var(--sa-text-muted);margin:0">Hiển thị gói (IsActive)</label>
          </div>
          <button class="sa-save-btn w-100" @click="save">
            <i class="bi bi-check-lg me-1"></i>{{ editPlan ? 'Cập nhật gói' : 'Tạo gói mới' }}
          </button>
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

const emptyForm = () => ({
  tenGoi: "", maGoi: "", soThang: 6,
  giaThang: 0, tongGia: 0,
  gioiHanHoaDon: 0, gioiHanNhanVien: 0,
  moTa: "", isActive: true, thuTuHienThi: 0,
});
const form = ref(emptyForm());
const formatVND = (n) => n ? Number(n).toLocaleString("vi-VN") + "đ" : "0đ";

const load = async () => {
  try { const r = await axios.get("/api/SuperAdmin/plans"); plans.value = r.data; }
  catch (e) { console.error(e); }
};

const openAdd = () => {
  editPlan.value = null;
  form.value = { ...emptyForm(), thuTuHienThi: plans.value.length };
  showForm.value = true;
};

const openEdit = (p) => {
  editPlan.value = p;
  form.value = { ...p };
  showForm.value = true;
};

const toggleActive = async (p) => {
  try {
    await axios.put(`/api/SuperAdmin/plans/${p.id}`, { ...p, isActive: !p.isActive });
    swal.fire({ toast: true, position: "top-end", icon: "success", title: p.isActive ? "Đã ẩn gói!" : "Đã hiện gói!", timer: 1500, showConfirmButton: false });
    load();
  } catch (e) { swal.fire("Lỗi", "Cập nhật thất bại", "error"); }
};

const save = async () => {
  if (!form.value.tenGoi || !form.value.maGoi) return swal.fire("Thiếu thông tin", "Vui lòng nhập tên và mã gói!", "warning");
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
@import "./sa-shared.css";

.sa-action-btn {
  display: flex; align-items: center; gap: 6px;
  background: rgba(245,158,11,0.15);
  border: 1px solid rgba(245,158,11,0.3);
  color: var(--sa-accent);
  padding: 8px 16px; border-radius: 8px;
  font-size: .82rem; font-weight: 700; cursor: pointer;
  transition: .2s;
}
.sa-action-btn:hover { background: rgba(245,158,11,.25); }

/* Plan Cards */
.plan-card {
  background: var(--sa-surface);
  border: 1px solid var(--sa-border);
  border-radius: 14px; padding: 22px;
  transition: transform .2s, box-shadow .2s;
  height: 100%;
}
.plan-card:hover { transform: translateY(-3px); box-shadow: 0 12px 30px var(--sa-shadow); }
.plan-inactive { opacity: .55; }
.plan-header { display: flex; justify-content: space-between; align-items: center; }
.plan-badge {
  display: inline-block;
  background: rgba(245,158,11,.15); color: var(--sa-accent);
  padding: 3px 12px; border-radius: 6px;
  font-size: .7rem; font-weight: 800; letter-spacing: 1px;
}
.plan-status { font-size: .7rem; color: var(--sa-text-faint); font-weight: 600; }
.plan-price { font-size: 1.5rem; font-weight: 800; color: var(--sa-accent); margin: 8px 0; }
.plan-info {
  display: flex; gap: 16px;
  font-size: .8rem; color: var(--sa-text-muted);
  margin-bottom: 8px;
}
.plan-desc { font-size: .82rem; color: var(--sa-text-faint); line-height: 1.5; }

/* Save Button */
.sa-save-btn {
  background: var(--sa-accent); border: none;
  color: #fff; padding: 10px; border-radius: 8px;
  font-weight: 700; font-size: .9rem; cursor: pointer;
  transition: opacity .2s;
}
.sa-save-btn:hover { opacity: .85; }
</style>
