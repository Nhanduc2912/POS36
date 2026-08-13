<script setup>
import { ref, watch, onMounted, inject } from "vue";
import axios from "axios";
import { globalState } from "../store";

const swal = inject("$swal");

// CHÚ Ý: Đảm bảo port 5098 đúng với port C# đang chạy của em
const backendUrl = window.location.hostname === "localhost" || window.location.hostname === "127.0.0.1"
  ? "http://localhost:5098"
  : `http://${window.location.hostname}:5098`;
const getImageUrl = (path) => {
  if (!path) return null;
  if (path.startsWith("http://") || path.startsWith("https://") || path.startsWith("data:")) {
    return path;
  }
  return backendUrl + path;
};

const categories = ref([]);
const ingredients = ref([]);
const expiredWarnings = ref([]);
const selectedCategoryId = ref(0);

const fetchCategories = async () => {
  try {
    const res = await axios.get("/api/DanhMucNguyenVatLieu");
    categories.value = res.data;
  } catch (error) {
    console.error("Lỗi tải danh mục", error);
  }
};

const fetchIngredients = async () => {
  try {
    let url = `/api/NguyenVatLieu?chiNhanhId=${globalState.value.activeBranchId || 0}`;
    if (selectedCategoryId.value > 0)
      url += `&danhMucId=${selectedCategoryId.value}`;
    const res = await axios.get(url);
    ingredients.value = res.data;
  } catch (error) {
    console.error("Lỗi tải nguyên vật liệu", error);
  }
};

const fetchExpiredWarnings = async () => {
  if (!globalState.value.activeBranchId) return;
  try {
    const res = await axios.get(`/api/NguyenVatLieu/canhbao-hethan?chiNhanhId=${globalState.value.activeBranchId}`);
    expiredWarnings.value = res.data;
  } catch (error) {
    console.error("Lỗi tải cảnh báo", error);
  }
};

watch(selectedCategoryId, () => fetchIngredients());
watch(() => globalState.value.activeBranchId, () => fetchExpiredWarnings());

onMounted(() => {
  fetchCategories();
  fetchIngredients();
  fetchExpiredWarnings();
});

// --- HÀM THÊM NHÓM ---
const handleAddCategory = async () => {
  swal
    .fire({
      title: "Thêm Nhóm NVL",
      html: `
      <input id="swal-cat-name" class="form-control mb-3" placeholder="Tên nhóm (VD: Hàng tươi sống)">
      <label class="d-block text-start mb-1 small text-muted">Ảnh minh họa (Tùy chọn):</label>
      <input id="swal-cat-image" class="form-control" type="file" accept="image/*">
    `,
      showCancelButton: true,
      confirmButtonText: "Lưu",
      preConfirm: () => {
        const name = document.getElementById("swal-cat-name").value;
        const file = document.getElementById("swal-cat-image").files[0];
        if (!name) {
          swal.showValidationMessage("Vui lòng nhập tên nhóm!");
          return false;
        }
        return { tenDanhMuc: name, imageFile: file };
      },
    })
    .then(async (result) => {
      if (result.isConfirmed) {
        try {
          const formData = new FormData();
          formData.append("TenDanhMuc", result.value.tenDanhMuc);
          if (result.value.imageFile)
            formData.append("HinhAnhFile", result.value.imageFile);

          await axios.post("/api/DanhMucNguyenVatLieu", formData, {
            headers: { "Content-Type": "multipart/form-data" },
          });
          fetchCategories();
        } catch (e) {
          swal.fire("Lỗi", "Không thể lưu nhóm!", "error");
        }
      }
    });
};

// --- HÀM THÊM NGUYÊN VẬT LIỆU ---
const handleAddIngredient = async () => {
  let categoryOptions = '<option value="">-- Chọn nhóm hàng --</option>';
  categories.value.forEach((cat) => {
    let isSelected = cat.id === selectedCategoryId.value ? "selected" : "";
    categoryOptions += `<option value="${cat.id}" ${isSelected}>${cat.tenDanhMuc}</option>`;
  });

  swal
    .fire({
      title: "Thêm Nguyên Vật Liệu",
      html: `
      <select id="swal-category" class="form-select mb-3">${categoryOptions}</select>
      <input id="swal-name" class="form-control mb-3" placeholder="Tên NVL (VD: Trân châu)">
      <input id="swal-dvt" class="form-control mb-3" placeholder="Đơn vị tính (VD: kg, gram, lít)">
      <div class="row">
        <div class="col-6 mb-3">
          <label class="d-block text-start mb-1 small fw-semibold">Ngưỡng cảnh báo tồn:</label>
          <input id="swal-nguong" class="form-control" type="number" value="1" min="0" step="0.1">
        </div>
        <div class="col-6 mb-3">
          <label class="d-block text-start mb-1 small fw-semibold">Báo hết hạn trước (ngày):</label>
          <input id="swal-songay" class="form-control" type="number" value="7" min="0">
        </div>
      </div>
    `,
      showCancelButton: true,
      confirmButtonText: "Lưu NVL",
      preConfirm: () => {
        const catId = document.getElementById("swal-category").value;
        const name = document.getElementById("swal-name").value;
        const dvt = document.getElementById("swal-dvt").value;
        const nguong = parseFloat(document.getElementById("swal-nguong").value);
        const songay = parseInt(document.getElementById("swal-songay").value);

        if (!name || !dvt) {
          swal.showValidationMessage("Vui lòng nhập tên và đơn vị tính!");
          return false;
        }
        return {
          DanhMucNguyenVatLieuId: catId ? parseInt(catId) : null,
          TenNguyenVatLieu: name,
          DonViTinh: dvt,
          NguongCanhBao: nguong,
          SoNgayCanhBaoHetHan: songay,
          TrangThai: true
        };
      },
    })
    .then(async (result) => {
      if (result.isConfirmed) {
        try {
          await axios.post("/api/NguyenVatLieu", result.value);
          fetchIngredients();
        } catch (error) {
          swal.fire("Lỗi", "Không thể thêm nguyên vật liệu", "error");
        }
      }
    });
};

// --- HÀM SỬA NVL ---
const handleEditIngredient = async (item) => {
  let categoryOptions = '<option value="">-- Chọn nhóm hàng --</option>';
  categories.value.forEach((cat) => {
    let isSelected = cat.id === item.danhMucNguyenVatLieuId ? "selected" : "";
    categoryOptions += `<option value="${cat.id}" ${isSelected}>${cat.tenDanhMuc}</option>`;
  });

  swal
    .fire({
      title: "Chỉnh Sửa NVL",
      html: `
      <select id="swal-category-edit" class="form-select mb-3">${categoryOptions}</select>
      <input id="swal-name-edit" class="form-control mb-3" value="${item.tenNguyenVatLieu}">
      <input id="swal-dvt-edit" class="form-control mb-3" value="${item.donViTinh}">
      <div class="row">
        <div class="col-6 mb-3">
          <label class="d-block text-start mb-1 small fw-semibold">Ngưỡng tồn:</label>
          <input id="swal-nguong-edit" class="form-control" type="number" value="${item.nguongCanhBao}" min="0" step="0.1">
        </div>
        <div class="col-6 mb-3">
          <label class="d-block text-start mb-1 small fw-semibold">Báo HSD (ngày):</label>
          <input id="swal-songay-edit" class="form-control" type="number" value="${item.soNgayCanhBaoHetHan}" min="0">
        </div>
      </div>
    `,
      showCancelButton: true,
      confirmButtonText: "Cập nhật",
      preConfirm: () => {
        const catId = document.getElementById("swal-category-edit").value;
        const name = document.getElementById("swal-name-edit").value;
        const dvt = document.getElementById("swal-dvt-edit").value;
        const nguong = parseFloat(document.getElementById("swal-nguong-edit").value);
        const songay = parseInt(document.getElementById("swal-songay-edit").value);

        if (!name || !dvt) {
          swal.showValidationMessage("Vui lòng nhập tên và DVT!");
          return false;
        }
        return {
          Id: item.id,
          DanhMucNguyenVatLieuId: catId ? parseInt(catId) : null,
          TenNguyenVatLieu: name,
          DonViTinh: dvt,
          NguongCanhBao: nguong,
          SoNgayCanhBaoHetHan: songay,
          TrangThai: item.trangThai
        };
      },
    })
    .then(async (result) => {
      if (result.isConfirmed) {
        try {
          await axios.put(`/api/NguyenVatLieu/${item.id}`, result.value);
          swal.fire({
            icon: "success",
            title: "Cập nhật thành công",
            timer: 1000,
            showConfirmButton: false,
          });
          fetchIngredients();
        } catch (e) {
          swal.fire("Lỗi", "Không thể sửa nguyên vật liệu", "error");
        }
      }
    });
};

const handleToggleStatus = async (item) => {
  try {
    await axios.put(`/api/NguyenVatLieu/${item.id}`, {
      Id: item.id,
      DanhMucNguyenVatLieuId: item.danhMucNguyenVatLieuId,
      TenNguyenVatLieu: item.tenNguyenVatLieu,
      DonViTinh: item.donViTinh,
      NguongCanhBao: item.nguongCanhBao,
      SoNgayCanhBaoHetHan: item.soNgayCanhBaoHetHan,
      TrangThai: !item.trangThai
    });
    item.trangThai = !item.trangThai;
  } catch (error) {
    swal.fire("Lỗi", "Không cập nhật được", "error");
  }
};

const handleDeleteIngredient = (id) => {
  swal
    .fire({
      title: "Bạn chắc chắn muốn xóa?",
      text: "Xóa nguyên vật liệu có thể ảnh hưởng đến các công thức định lượng đã cấu hình.",
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#d33",
      confirmButtonText: "Đồng ý xóa",
    })
    .then(async (result) => {
      if (result.isConfirmed) {
        try {
          await axios.delete(`/api/NguyenVatLieu/${id}`);
          fetchIngredients();
        } catch (e) {
          swal.fire("Lỗi", "Không thể xóa", "error");
        }
      }
    });
};
</script>

<template>
  <div class="container-fluid px-4 py-4">
    <!-- KHU VỰC CẢNH BÁO HẾT HẠN (SẼ HIỂN THỊ NẾU CÓ) -->
    <div v-if="expiredWarnings.length > 0" class="alert alert-danger shadow-sm mb-4">
      <h6 class="fw-bold mb-2"><i class="bi bi-exclamation-triangle-fill"></i> LÔ NGUYÊN VẬT LIỆU SẮP HẾT HẠN HOẶC ĐÃ HẾT HẠN</h6>
      <ul class="mb-0" style="font-size: 0.9rem;">
        <li v-for="(w, idx) in expiredWarnings" :key="idx" class="mb-1">
          <span class="fw-bold">{{ w.tenNguyenVatLieu }}</span> 
          (Lô: <strong>{{ new Date(w.ngayHetHan).toLocaleDateString('vi-VN') }}</strong>) 
          - Tồn: <strong>{{ w.soLuongTon }} {{ w.donViTinh }}</strong> 
          - <span v-if="w.soNgayConLai < 0" class="text-danger fw-bold">Đã hết hạn {{ Math.abs(w.soNgayConLai) }} ngày</span>
            <span v-else class="text-warning fw-bold text-dark">Còn lại {{ w.soNgayConLai }} ngày</span>
        </li>
      </ul>
    </div>

    <div class="row g-3">
      <!-- SIDEBAR DANH MỤC -->
      <div class="col-lg-2">
        <div class="card border-0 shadow-sm h-100 rounded-3">
          <div
            class="card-header bg-white border-bottom pt-3 pb-2 d-flex justify-content-between align-items-center"
          >
            <span class="fw-bold text-dark fs-6"
              ><i class="bi bi-funnel"></i> NHÓM HÀNG</span
            >
            <button
              @click="handleAddCategory"
              class="btn btn-sm btn-outline-warning rounded-circle p-1 lh-1"
            >
              <i class="bi bi-plus"></i>
            </button>
          </div>
          <div class="card-body p-0">
            <div class="list-group list-group-flush bg-transparent">
              <button
                @click="selectedCategoryId = 0"
                class="list-group-item list-group-item-action border-0 py-2 fw-medium category-item"
                :class="{ 'active-cat': selectedCategoryId === 0 }"
              >
                <i class="bi bi-grid-fill me-2 text-muted"></i> Tất cả
              </button>

              <button
                v-for="cat in categories"
                :key="cat.id"
                @click="selectedCategoryId = cat.id"
                class="list-group-item list-group-item-action border-0 py-2 fw-medium text-uppercase category-item d-flex align-items-center"
                :class="{ 'active-cat': selectedCategoryId === cat.id }"
              >
                <div
                  class="me-2 overflow-hidden rounded-circle bg-light border d-flex align-items-center justify-content-center"
                  style="width: 28px; height: 28px; min-width: 28px"
                >
                  <img
                    v-if="cat.hinhAnh"
                    :src="getImageUrl(cat.hinhAnh)"
                    class="w-100 h-100"
                    style="object-fit: cover"
                  />
                  <i v-else class="bi bi-tag-fill text-secondary small"></i>
                </div>
                {{ cat.tenDanhMuc }}
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- MAIN CONTENT -->
      <div class="col-lg-10">
        <div class="card border-0 shadow-sm rounded-3">
          <div
            class="card-header bg-white border-bottom py-3 d-flex justify-content-between align-items-center"
          >
            <h5 class="fw-bold text-dark mb-0">
              <i class="bi bi-egg-fried text-warning me-2"></i> DANH MỤC NGUYÊN VẬT LIỆU
            </h5>
            <button
              @click="handleAddIngredient"
              class="btn btn-success btn-sm fw-bold px-4 rounded-pill shadow-sm"
            >
              <i class="bi bi-plus-circle me-1"></i> THÊM MỚI
            </button>
          </div>

          <div class="card-body p-0 table-responsive">
            <table class="table table-hover align-middle mb-0">
              <thead class="table-light text-muted small">
                <tr>
                  <th class="ps-4" style="width: 50px">#</th>
                  <th>Mã / Tên Nguyên Vật Liệu</th>
                  <th>Đơn vị tính</th>
                  <th class="text-end">Tồn kho</th>
                  <th class="text-end">Ngưỡng tồn</th>
                  <th class="text-end">Cảnh báo HSD (ngày)</th>
                  <th class="text-center">Kích hoạt</th>
                  <th class="text-center" style="width: 100px">Thao tác</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="(item, index) in ingredients" :key="item.id">
                  <td class="ps-4 fw-bold text-muted">{{ index + 1 }}</td>
                  <td>
                    <span
                      class="fw-bold"
                      :class="
                        item.trangThai
                          ? 'text-dark'
                          : 'text-muted text-decoration-line-through'
                      "
                      >{{ item.tenNguyenVatLieu }}</span
                    >
                  </td>
                  <td>
                    <span class="badge bg-secondary rounded-pill fw-normal">{{ item.donViTinh }}</span>
                  </td>
                  <td class="text-end fw-bold text-primary">
                    {{ item.tonKho || 0 }}
                  </td>
                  <td class="text-end fw-bold">
                    {{ item.nguongCanhBao }}
                  </td>
                  <td class="text-end">
                    {{ item.soNgayCanhBaoHetHan }}
                  </td>
                  <td class="text-center">
                    <div
                      class="form-check form-switch d-flex justify-content-center m-0"
                    >
                      <input
                        class="form-check-input"
                        type="checkbox"
                        role="switch"
                        style="cursor: pointer; width: 40px; height: 20px"
                        :checked="item.trangThai"
                        @change="handleToggleStatus(item)"
                      />
                    </div>
                  </td>
                  <td class="text-center">
                    <button
                      @click="handleEditIngredient(item)"
                      class="btn btn-sm btn-light text-primary me-1"
                    >
                      <i class="bi bi-pencil-square"></i>
                    </button>
                    <button
                      @click="handleDeleteIngredient(item.id)"
                      class="btn btn-sm btn-light text-danger"
                    >
                      <i class="bi bi-trash"></i>
                    </button>
                  </td>
                </tr>
                <tr v-if="ingredients.length === 0">
                  <td colspan="8" class="text-center py-5 text-muted">
                    <i class="bi bi-inbox fs-1 d-block mb-2"></i> Không có dữ
                    liệu nguyên vật liệu.
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

<style scoped>
.category-item {
  font-size: 13px;
  color: #495057;
  transition: all 0.2s;
  padding-left: 20px;
}
.category-item:hover {
  background-color: #f8f9fa;
  color: #f37021;
}
.active-cat {
  color: #f37021 !important;
  font-weight: bold !important;
  background-color: #fff3ed !important;
  border-left: 4px solid #f37021 !important;
}
.table th {
  font-weight: 600;
  text-transform: uppercase;
  font-size: 12px;
}
.table td {
  font-size: 14px;
}
.form-check-input:checked {
  background-color: #198754;
  border-color: #198754;
}
</style>
