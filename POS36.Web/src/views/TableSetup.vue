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
                class="list-group-item list-group-item-action border-0 rounded-3 mb-1 fw-bold"
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
            class="card-header bg-white border-bottom py-3 d-flex justify-content-between align-items-center"
          >
            <h6 class="fw-bold text-secondary mb-0 text-uppercase">
              <i class="bi bi-table me-2"></i> Danh sách bàn
            </h6>
            <button
              @click="handleAddTable"
              class="btn btn-primary btn-sm fw-bold px-3"
            >
              <i class="bi bi-plus-lg"></i> Thêm Bàn mới
            </button>
          </div>
          <div class="card-body p-0">
            <table class="table table-hover align-middle mb-0">
              <thead class="table-light text-muted">
                <tr>
                  <th class="text-center ps-3" style="width: 5%">#</th>
                  <th style="width: 30%">Tên bàn</th>
                  <th class="text-center" style="width: 25%">Trạng thái</th>
                  <th class="text-center" style="width: 40%">Thao tác</th>
                </tr>
              </thead>
              <tbody>
                <tr v-if="tables.length === 0">
                  <td colspan="4" class="text-center text-muted py-5">
                    <i class="bi bi-inbox fs-1 d-block mb-3 opacity-50"></i> Khu
                    vực này chưa có bàn nào.
                  </td>
                </tr>
                <tr v-for="(table, index) in tables" :key="table.id">
                  <td class="text-center text-muted ps-3">{{ index + 1 }}</td>
                  <td class="fw-bold text-dark">{{ table.tenBan }}</td>
                  <td class="text-center">
                    <span
                      class="badge py-2 px-3"
                      :class="
                        table.trangThai === 'Trống'
                          ? 'bg-secondary'
                          : 'bg-warning text-dark'
                      "
                    >
                      {{ table.trangThai }}
                    </span>
                  </td>
                  <td class="text-center">
                    <button
                      @click="handleEditTable(table)"
                      class="btn btn-sm btn-outline-primary fw-bold px-3 me-2"
                    >
                      <i class="bi bi-pencil-square"></i> Sửa
                    </button>
                    <button
                      @click="handleDeleteTable(table)"
                      class="btn btn-sm btn-outline-danger fw-bold px-3"
                    >
                      <i class="bi bi-trash"></i> Xóa
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, inject, watch, onMounted, computed } from "vue";
import axios from "axios";
import { globalState } from "../store";

const swal = inject("$swal");

const areas = ref([]);
const tables = ref([]);
const selectedAreaId = ref(null);

const currentBranchName = computed(() => {
  const branch = globalState.value.branches.find(
    (b) => b.id === globalState.value.activeBranchId,
  );
  return branch ? branch.tenChiNhanh : "Chưa có";
});

// --- LOAD DATA ---
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

watch(
  () => globalState.value.activeBranchId,
  () => fetchAreas(),
);
watch(selectedAreaId, (newId) => {
  if (newId) fetchTables(newId);
});
onMounted(() => fetchAreas());

// --- THÊM CHI NHÁNH & KHU VỰC ---
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
      window.location.reload();
    } catch (e) {
      swal.fire("Lỗi", "Không thể lưu!", "error");
    }
  }
};

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
      swal.fire({
        toast: true,
        position: "top-end",
        icon: "success",
        title: "Đã thêm khu vực",
        timer: 1500,
        showConfirmButton: false,
      });
      fetchAreas();
    } catch (e) {
      swal.fire("Lỗi", "Không thể lưu!", "error");
    }
  }
};

// --- QUẢN LÝ BÀN (THÊM, SỬA, XÓA) ---
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
      swal.fire({
        toast: true,
        position: "top-end",
        icon: "success",
        title: "Đã thêm bàn mới",
        timer: 1500,
        showConfirmButton: false,
      });
      fetchTables(selectedAreaId.value);
    } catch (e) {
      swal.fire("Lỗi", "Không thể lưu bàn!", "error");
    }
  }
};

const handleEditTable = async (table) => {
  const { value: newName } = await swal.fire({
    title: "Cập nhật tên bàn",
    input: "text",
    inputValue: table.tenBan,
    showCancelButton: true,
    confirmButtonText: "Lưu thay đổi",
    inputValidator: (value) => {
      if (!value) return "Tên bàn không được để trống!";
    },
  });

  if (newName && newName !== table.tenBan) {
    try {
      await axios.put(`/api/Ban/${table.id}`, { ...table, tenBan: newName });
      swal.fire({
        toast: true,
        position: "top-end",
        icon: "success",
        title: "Đã cập nhật",
        timer: 1500,
        showConfirmButton: false,
      });
      fetchTables(selectedAreaId.value);
    } catch (e) {
      swal.fire("Lỗi", "Không thể cập nhật bàn", "error");
    }
  }
};

const handleDeleteTable = async (table) => {
  if (table.trangThai !== "Trống") {
    return swal.fire(
      "Cảnh báo",
      "Không thể xóa bàn đang có khách ngồi!",
      "warning",
    );
  }

  const { isConfirmed } = await swal.fire({
    title: "Xóa bàn này?",
    text: `Bạn có chắc chắn muốn xóa ${table.tenBan} không?`,
    icon: "warning",
    showCancelButton: true,
    confirmButtonColor: "#d33",
    confirmButtonText: "Xóa ngay",
  });

  if (isConfirmed) {
    try {
      await axios.delete(`/api/Ban/${table.id}`);
      swal.fire({
        toast: true,
        position: "top-end",
        icon: "success",
        title: "Đã xóa bàn",
        timer: 1500,
        showConfirmButton: false,
      });
      fetchTables(selectedAreaId.value);
    } catch (e) {
      swal.fire(
        "Lỗi",
        "Không thể xóa bàn. Có thể bàn đang chứa dữ liệu lịch sử.",
        "error",
      );
    }
  }
};
</script>

<style scoped>
.table-hover tbody tr:hover {
  background-color: #f8f9fa;
}
</style>
