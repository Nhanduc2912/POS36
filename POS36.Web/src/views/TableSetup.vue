<script setup>
import { ref, inject, watch, onMounted, computed } from "vue";
import axios from "axios";
import { globalState } from "../store";

const swal = inject("$swal");

const areas = ref([]);
const tables = ref([]);
const selectedAreaId = ref(null);

// Lấy tên chi nhánh đang chọn để hiển thị
const currentBranchName = computed(() => {
  const branch = globalState.value.branches.find(
    (b) => b.id === globalState.value.activeBranchId,
  );
  return branch ? branch.tenChiNhanh : "Chưa có";
});

// HÀM TẢI DỮ LIỆU TỪ API
const fetchAreas = async () => {
  if (!globalState.value.activeBranchId) {
    areas.value = [];
    tables.value = [];
    return;
  }
  try {
    const res = await axios.get(
      `/api/KhuVuc/chi-nhanh/${globalState.value.activeBranchId}`,
    );
    areas.value = res.data;
    if (areas.value.length > 0) {
      selectedAreaId.value = areas.value[0].id;
      fetchTables(areas.value[0].id);
    } else {
      tables.value = [];
    }
  } catch (error) {
    console.error("Lỗi tải khu vực", error);
  }
};

const fetchTables = async (areaId) => {
  try {
    const res = await axios.get(`/api/Ban/khu-vuc/${areaId}`);
    tables.value = res.data;
  } catch (error) {
    console.error("Lỗi tải bàn", error);
  }
};

// THEO DÕI SỰ THAY ĐỔI CHI NHÁNH ĐỂ LOAD LẠI DATA
watch(
  () => globalState.value.activeBranchId,
  () => fetchAreas(),
);
watch(selectedAreaId, (newId) => {
  if (newId) fetchTables(newId);
});
onMounted(() => fetchAreas());

// --- CÁC HÀM THÊM MỚI (GỌI API THẬT) ---

// 1. Thêm Chi Nhánh Mới
const handleAddBranch = async () => {
  const { value: name } = await swal.fire({
    title: "Thêm Chi Nhánh Mới",
    input: "text",
    inputPlaceholder: "VD: Cơ sở 1...",
    showCancelButton: true,
  });
  if (name) {
    try {
      await axios.post("/api/ChiNhanh", { tenChiNhanh: name });
      swal.fire("Thành công", "Đã thêm chi nhánh", "success");
      // Tải lại trang để Navbar cập nhật lại Dropdown
      window.location.reload();
    } catch (e) {
      swal.fire("Lỗi", "Không thể lưu!", "error");
    }
  }
};

// 2. Thêm Khu Vực
const handleAddArea = async () => {
  if (!globalState.value.activeBranchId)
    return swal.fire("Lỗi", "Vui lòng thêm Chi nhánh trước!", "warning");

  const { value: name } = await swal.fire({
    title: "Thêm Khu Vực",
    input: "text",
    inputPlaceholder: "VD: Tầng 1...",
    showCancelButton: true,
  });
  if (name) {
    try {
      await axios.post("/api/KhuVuc", {
        chiNhanhId: globalState.value.activeBranchId,
        tenKhuVuc: name,
      });
      swal.fire("Thành công", "Đã thêm khu vực", "success");
      fetchAreas();
    } catch (e) {
      swal.fire("Lỗi", "Không thể lưu!", "error");
    }
  }
};

// 3. Thêm Bàn
const handleAddTable = async () => {
  if (!selectedAreaId.value)
    return swal.fire("Lỗi", "Vui lòng thêm Khu vực trước!", "warning");

  const { value: name } = await swal.fire({
    title: "Thêm Bàn Mới",
    input: "text",
    inputPlaceholder: "VD: Bàn 01...",
    showCancelButton: true,
  });
  if (name) {
    try {
      await axios.post("/api/Ban", {
        khuVucId: selectedAreaId.value,
        tenBan: name,
        trangThai: "Trống",
      });
      fetchTables(selectedAreaId.value);
    } catch (e) {
      swal.fire("Lỗi", "Không thể lưu bàn!", "error");
    }
  }
};
</script>

<template>
  <div class="container-fluid px-4 py-4">
    <div class="card border-0 shadow-sm mb-4">
      <div class="card-body d-flex justify-content-between align-items-center">
        <div>
          <span class="text-muted me-2">Đang thiết lập cho:</span>
          <span class="badge bg-danger fs-6 px-3 py-2 rounded-pill"
            ><i class="bi bi-geo-alt-fill"></i> {{ currentBranchName }}</span
          >
        </div>
        <button
          @click="handleAddBranch"
          class="btn btn-outline-danger fw-bold rounded-pill px-4"
        >
          <i class="bi bi-plus-lg"></i> Thêm Chi Nhánh Mới
        </button>
      </div>
    </div>

    <div class="row g-4">
      <div class="col-lg-3">
        <div class="card border-0 shadow-sm h-100">
          <div
            class="card-header bg-white border-0 pt-4 pb-2 d-flex justify-content-between align-items-center"
          >
            <h6 class="fw-bold text-secondary mb-0">KHU VỰC</h6>
            <button
              @click="handleAddArea"
              class="btn btn-sm btn-outline-success"
            >
              <i class="bi bi-plus"></i> Thêm
            </button>
          </div>
          <div class="card-body p-2">
            <div class="list-group list-group-flush">
              <button
                v-for="area in areas"
                :key="area.id"
                @click="selectedAreaId = area.id"
                class="list-group-item list-group-item-action border-0 rounded-3 mb-1"
                :class="{
                  'active bg-primary text-white': selectedAreaId === area.id,
                }"
              >
                <i class="bi bi-geo-alt-fill me-2"></i> {{ area.tenKhuVuc }}
              </button>
              <div
                v-if="areas.length === 0"
                class="text-center text-muted mt-3 small"
              >
                Chưa có khu vực nào
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="col-lg-9">
        <div class="card border-0 shadow-sm h-100">
          <div
            class="card-header bg-white border-0 pt-4 pb-2 d-flex justify-content-between align-items-center border-bottom"
          >
            <h6 class="fw-bold text-secondary mb-0">DANH SÁCH BÀN</h6>
            <button @click="handleAddTable" class="btn btn-sm btn-primary">
              <i class="bi bi-plus"></i> Thêm Bàn mới
            </button>
          </div>
          <div class="card-body bg-light">
            <div class="row g-3">
              <div
                v-for="table in tables"
                :key="table.id"
                class="col-md-3 col-sm-4"
              >
                <div
                  class="card border-0 shadow-sm table-card text-center py-4 rounded-4"
                >
                  <i class="bi bi-square fs-1 text-primary mb-2"></i>
                  <h5 class="fw-bold text-dark mb-0">{{ table.tenBan }}</h5>
                </div>
              </div>
              <div
                v-if="tables.length === 0"
                class="col-12 text-center text-muted py-5"
              >
                <i class="bi bi-inbox fs-1 d-block mb-3"></i> Khu vực này trống.
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.table-card {
  transition: all 0.2s ease;
  cursor: pointer;
}
.table-card:hover {
  transform: translateY(-3px);
  background-color: #e6f7ff !important;
  border: 1px solid #1890ff !important;
}
</style>
