<script setup>
import { ref, watch, onMounted, inject } from "vue";
import axios from "axios";
import { globalState } from "../store";

const swal = inject("$swal");

// CHÚ Ý: Đảm bảo port 5198 đúng với port C# đang chạy của em
const backendUrl = "http://localhost:5198";
const getImageUrl = (path) => (path ? backendUrl + path : null);

const categories = ref([]);
const products = ref([]);
const selectedCategoryId = ref(0);

const fetchCategories = async () => {
  try {
    const res = await axios.get("/api/DanhMuc");
    categories.value = res.data;
  } catch (error) {
    console.error("Lỗi tải danh mục", error);
  }
};

const fetchProducts = async () => {
  if (!globalState.value.activeBranchId) {
    products.value = [];
    return;
  }
  try {
    let url = `/api/SanPham/danh-sach?chiNhanhId=${globalState.value.activeBranchId}`;
    if (selectedCategoryId.value > 0)
      url += `&danhMucId=${selectedCategoryId.value}`;
    const res = await axios.get(url);
    products.value = res.data;
  } catch (error) {
    console.error("Lỗi tải sản phẩm", error);
  }
};

watch(
  () => globalState.value.activeBranchId,
  () => fetchProducts(),
);
watch(selectedCategoryId, () => fetchProducts());
onMounted(() => {
  fetchCategories();
  fetchProducts();
});

// --- HÀM THÊM NHÓM (CÓ ẢNH) ---
const handleAddCategory = async () => {
  swal
    .fire({
      title: "Thêm Nhóm Hàng",
      html: `
      <input id="swal-cat-name" class="form-control mb-3" placeholder="Tên nhóm (VD: Lẩu, Nước)">
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

          await axios.post("/api/DanhMuc", formData, {
            headers: { "Content-Type": "multipart/form-data" },
          });
          fetchCategories();
        } catch (e) {
          swal.fire("Lỗi", "Không thể lưu nhóm!", "error");
        }
      }
    });
};

// --- HÀM TẮT/BẬT TRẠNG THÁI ---
const handleToggleStatus = async (prod) => {
  try {
    await axios.put(`/api/SanPham/toggle-status/${prod.id}`);
    prod.trangThai = !prod.trangThai;
  } catch (error) {
    swal.fire("Lỗi", "Không cập nhật được", "error");
  }
};

// --- HÀM THÊM SẢN PHẨM ---
const handleAddProduct = async () => {
  let categoryOptions = '<option value="">-- Chọn nhóm hàng --</option>';
  categories.value.forEach((cat) => {
    let isSelected = cat.id === selectedCategoryId.value ? "selected" : "";
    categoryOptions += `<option value="${cat.id}" ${isSelected}>${cat.tenDanhMuc}</option>`;
  });

  swal
    .fire({
      title: "Thêm Hàng Hóa Mới",
      html: `
      <select id="swal-category" class="form-select mb-3">${categoryOptions}</select>
      <input id="swal-name" class="form-control mb-3" placeholder="Tên hàng hóa (VD: Lẩu Thái)">
      <input id="swal-price" class="form-control mb-3" type="number" placeholder="Giá bán (VNĐ)">
      <label class="d-block text-start mb-1 small text-muted">Hình ảnh (Tùy chọn):</label>
      <input id="swal-image" class="form-control" type="file" accept="image/*">
    `,
      showCancelButton: true,
      confirmButtonText: "Lưu Món",
      preConfirm: () => {
        const catId = document.getElementById("swal-category").value;
        const name = document.getElementById("swal-name").value;
        const price = document.getElementById("swal-price").value;
        const imageFile = document.getElementById("swal-image").files[0];

        if (!catId || !name || !price) {
          swal.showValidationMessage("Vui lòng nhập đủ thông tin cơ bản!");
          return false;
        }
        return {
          danhMucId: catId,
          tenSanPham: name,
          giaBan: price,
          imageFile: imageFile,
        };
      },
    })
    .then(async (result) => {
      if (result.isConfirmed) {
        try {
          const formData = new FormData();
          formData.append("DanhMucId", result.value.danhMucId);
          formData.append("TenSanPham", result.value.tenSanPham);
          formData.append("GiaBan", result.value.giaBan);
          if (result.value.imageFile)
            formData.append("HinhAnhFile", result.value.imageFile);

          await axios.post("/api/SanPham", formData, {
            headers: { "Content-Type": "multipart/form-data" },
          });
          fetchProducts();
        } catch (error) {
          swal.fire("Lỗi", "Không thể thêm sản phẩm", "error");
        }
      }
    });
};

// --- HÀM SỬA SẢN PHẨM (ĐÃ ĐƯỢC PHỤC HỒI) ---
const handleEditProduct = async (prod) => {
  let categoryOptions = "";
  categories.value.forEach((cat) => {
    let isSelected = cat.id === prod.danhMucId ? "selected" : "";
    categoryOptions += `<option value="${cat.id}" ${isSelected}>${cat.tenDanhMuc}</option>`;
  });

  swal
    .fire({
      title: "Chỉnh Sửa Hàng Hóa",
      html: `
      <select id="swal-category-edit" class="form-select mb-3">${categoryOptions}</select>
      <input id="swal-name-edit" class="form-control mb-3" value="${prod.tenSanPham}">
      <input id="swal-price-edit" class="form-control mb-3" type="number" value="${prod.giaBan}">
      <label class="d-block text-start mb-1 small text-muted">Ảnh mới (Bỏ trống nếu giữ ảnh cũ):</label>
      <input id="swal-image-edit" class="form-control" type="file" accept="image/*">
    `,
      showCancelButton: true,
      confirmButtonText: "Cập nhật",
      preConfirm: () => {
        const catId = document.getElementById("swal-category-edit").value;
        const name = document.getElementById("swal-name-edit").value;
        const price = document.getElementById("swal-price-edit").value;
        const imageFile = document.getElementById("swal-image-edit").files[0];
        if (!catId || !name || !price) {
          swal.showValidationMessage("Vui lòng nhập đủ thông tin!");
          return false;
        }
        return {
          danhMucId: catId,
          tenSanPham: name,
          giaBan: price,
          imageFile: imageFile,
        };
      },
    })
    .then(async (result) => {
      if (result.isConfirmed) {
        try {
          const formData = new FormData();
          formData.append("DanhMucId", result.value.danhMucId);
          formData.append("TenSanPham", result.value.tenSanPham);
          formData.append("GiaBan", result.value.giaBan);
          if (result.value.imageFile)
            formData.append("HinhAnhFile", result.value.imageFile);

          await axios.put(`/api/SanPham/${prod.id}`, formData, {
            headers: { "Content-Type": "multipart/form-data" },
          });
          swal.fire({
            icon: "success",
            title: "Cập nhật thành công",
            timer: 1000,
            showConfirmButton: false,
          });
          fetchProducts();
        } catch (e) {
          swal.fire("Lỗi", "Không thể sửa sản phẩm", "error");
        }
      }
    });
};

// --- HÀM XÓA SẢN PHẨM ---
const handleDeleteProduct = (id) => {
  swal
    .fire({
      title: "Bạn chắc chắn muốn xóa?",
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#d33",
      confirmButtonText: "Đồng ý xóa",
    })
    .then(async (result) => {
      if (result.isConfirmed) {
        try {
          await axios.delete(`/api/SanPham/${id}`);
          fetchProducts();
        } catch (e) {
          swal.fire("Lỗi", "Không thể xóa", "error");
        }
      }
    });
};
</script>

<template>
  <div class="container-fluid px-4 py-4">
    <div class="row g-3">
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

      <div class="col-lg-10">
        <div class="card border-0 shadow-sm rounded-3">
          <div
            class="card-header bg-white border-bottom py-3 d-flex justify-content-between align-items-center"
          >
            <h5 class="fw-bold text-dark mb-0">
              <i class="bi bi-box-seam text-warning me-2"></i> DANH SÁCH HÀNG
              HÓA
            </h5>
            <button
              @click="handleAddProduct"
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
                  <th>Mã / Tên hàng hóa</th>
                  <th>Nhóm</th>
                  <th class="text-end">Giá bán</th>
                  <th class="text-end">Tồn kho</th>
                  <th class="text-center">Đang bán</th>
                  <th class="text-center" style="width: 100px">Thao tác</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="(prod, index) in products" :key="prod.id">
                  <td class="ps-4 fw-bold text-muted">{{ index + 1 }}</td>
                  <td>
                    <div class="d-flex align-items-center">
                      <div
                        class="me-3 border"
                        style="
                          width: 45px;
                          height: 45px;
                          overflow: hidden;
                          border-radius: 8px;
                        "
                      >
                        <img
                          v-if="prod.hinhAnh"
                          :src="getImageUrl(prod.hinhAnh)"
                          class="w-100 h-100"
                          style="object-fit: cover"
                          alt="sp"
                        />
                        <div
                          v-else
                          class="bg-light w-100 h-100 d-flex align-items-center justify-content-center"
                        >
                          <i class="bi bi-camera text-secondary fs-5"></i>
                        </div>
                      </div>
                      <span
                        class="fw-bold"
                        :class="
                          prod.trangThai
                            ? 'text-dark'
                            : 'text-muted text-decoration-line-through'
                        "
                        >{{ prod.tenSanPham }}</span
                      >
                    </div>
                  </td>
                  <td>
                    <span class="badge bg-secondary rounded-pill fw-normal">{{
                      prod.tenDanhMuc || "Chưa phân nhóm"
                    }}</span>
                  </td>
                  <td class="text-end fw-bold">
                    {{ prod.giaBan.toLocaleString("vi-VN") }}
                  </td>
                  <td class="text-end">
                    <span
                      class="fw-bold"
                      :class="prod.tonKho > 0 ? 'text-success' : 'text-danger'"
                      >{{ prod.tonKho }}</span
                    >
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
                        :checked="prod.trangThai"
                        @change="handleToggleStatus(prod)"
                      />
                    </div>
                  </td>
                  <td class="text-center">
                    <button
                      @click="handleEditProduct(prod)"
                      class="btn btn-sm btn-light text-primary me-1"
                    >
                      <i class="bi bi-pencil-square"></i>
                    </button>
                    <button
                      @click="handleDeleteProduct(prod.id)"
                      class="btn btn-sm btn-light text-danger"
                    >
                      <i class="bi bi-trash"></i>
                    </button>
                  </td>
                </tr>
                <tr v-if="products.length === 0">
                  <td colspan="7" class="text-center py-5 text-muted">
                    <i class="bi bi-inbox fs-1 d-block mb-2"></i> Không có dữ
                    liệu hàng hóa.
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
