<script setup>
import { ref, onMounted, computed, inject } from "vue";
import axios from "axios";
import { globalState } from "../store";

const swal = inject("$swal");
const products = ref([]);
const searchQuery = ref(""); // Biến lưu trữ từ khóa tìm kiếm

// Biến phục vụ tính năng sửa giá tại chỗ (Inline Edit)
const editingId = ref(null);
const editPriceValue = ref(0);

// LẤY DANH SÁCH SẢN PHẨM TỪ API
const fetchProducts = async () => {
  if (!globalState.value.activeBranchId) return;
  try {
    const res = await axios.get(
      `/api/SanPham/danh-sach?chiNhanhId=${globalState.value.activeBranchId}`,
    );
    products.value = res.data;
  } catch (error) {
    console.error("Lỗi tải dữ liệu", error);
  }
};

onMounted(() => {
  fetchProducts();
});

// TỰ ĐỘNG LỌC SẢN PHẨM THEO Ô TÌM KIẾM
const filteredProducts = computed(() => {
  if (!searchQuery.value) return products.value;
  return products.value.filter((p) =>
    p.tenSanPham.toLowerCase().includes(searchQuery.value.toLowerCase()),
  );
});

// --- CÁC HÀM SỬA GIÁ NHANH ---
const startEditing = (prod) => {
  editingId.value = prod.id;
  editPriceValue.value = prod.giaBan;
};

const cancelEditing = () => {
  editingId.value = null;
};

const savePrice = async (prod) => {
  if (editPriceValue.value < 0) {
    swal.fire("Lỗi", "Giá bán không được nhỏ hơn 0", "error");
    return;
  }

  try {
    await axios.put(`/api/SanPham/update-price/${prod.id}`, {
      giaBan: editPriceValue.value,
    });
    // Cập nhật giá trên giao diện ngay lập tức mà không cần gọi lại API GET
    prod.giaBan = editPriceValue.value;
    editingId.value = null;

    // Bật một thông báo nhỏ gọn ở góc màn hình
    swal.fire({
      toast: true,
      position: "top-end",
      icon: "success",
      title: "Đã lưu giá mới",
      showConfirmButton: false,
      timer: 1500,
    });
  } catch (error) {
    swal.fire("Lỗi", "Không thể lưu giá", "error");
  }
};
</script>

<template>
  <div class="container-fluid px-4 py-4">
    <div class="row g-3">
      <div class="col-lg-2">
        <div class="card border-0 shadow-sm rounded-3 mb-3">
          <div class="card-header bg-white border-bottom pt-3 pb-2">
            <span class="fw-bold text-dark fs-6 text-uppercase"
              ><i class="bi bi-list-task text-warning me-1"></i> Danh sách bảng
              giá</span
            >
          </div>
          <div class="card-body p-0">
            <div class="list-group list-group-flush bg-transparent">
              <button
                class="list-group-item list-group-item-action border-0 py-2 fw-bold active-cat"
              >
                Giá niêm yết
              </button>
            </div>
          </div>
        </div>

        <div class="card border-0 shadow-sm rounded-3">
          <div class="card-header bg-white border-bottom pt-3 pb-2">
            <span class="fw-bold text-dark fs-6 text-uppercase"
              ><i class="bi bi-search text-warning me-1"></i> Tìm kiếm</span
            >
          </div>
          <div class="card-body p-2">
            <input
              v-model="searchQuery"
              type="text"
              class="form-control form-control-sm"
              placeholder="Mã, Tên hàng hóa..."
            />
          </div>
        </div>
      </div>

      <div class="col-lg-10">
        <div class="card border-0 shadow-sm rounded-3">
          <div
            class="card-header bg-white border-bottom py-3 d-flex justify-content-between align-items-center"
          >
            <h5 class="fw-bold text-dark mb-0">
              <i class="bi bi-funnel text-warning me-2"></i> GIÁ NIÊM YẾT
            </h5>
            <button
              class="btn btn-outline-secondary btn-sm fw-bold px-3 rounded-pill"
            >
              <i class="bi bi-file-earmark-arrow-up"></i> XUẤT RA FILE
            </button>
          </div>

          <div class="card-body p-0 table-responsive">
            <table class="table table-hover align-middle mb-0">
              <thead class="table-light text-muted small">
                <tr>
                  <th class="ps-4">Mã hàng hóa</th>
                  <th>Tên hàng hóa</th>
                  <th class="text-end" style="width: 15%">Giá vốn</th>
                  <th class="text-end pe-4" style="width: 25%">Giá bán</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="prod in filteredProducts" :key="prod.id">
                  <td class="ps-4 fw-bold text-muted">HH-00{{ prod.id }}</td>
                  <td class="fw-bold text-dark">{{ prod.tenSanPham }}</td>

                  <td class="text-end text-muted">0</td>

                  <td class="text-end pe-4">
                    <div
                      v-if="editingId === prod.id"
                      class="d-flex justify-content-end align-items-center"
                    >
                      <input
                        v-model="editPriceValue"
                        type="number"
                        class="form-control form-control-sm text-end me-2"
                        style="width: 120px"
                        @keyup.enter="savePrice(prod)"
                        @keyup.esc="cancelEditing"
                        autofocus
                      />
                      <button
                        @click="savePrice(prod)"
                        class="btn btn-sm btn-success p-1 lh-1 me-1"
                      >
                        <i class="bi bi-check"></i>
                      </button>
                      <button
                        @click="cancelEditing"
                        class="btn btn-sm btn-danger p-1 lh-1"
                      >
                        <i class="bi bi-x"></i>
                      </button>
                    </div>

                    <div
                      v-else
                      class="d-flex justify-content-end align-items-center"
                      style="cursor: pointer"
                      @click="startEditing(prod)"
                    >
                      <span class="fw-bold me-2">{{
                        prod.giaBan.toLocaleString("vi-VN")
                      }}</span>
                      <i class="bi bi-pencil-square text-muted small"></i>
                    </div>
                  </td>
                </tr>
                <tr v-if="filteredProducts.length === 0">
                  <td colspan="4" class="text-center py-5 text-muted">
                    Không tìm thấy hàng hóa.
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
.active-cat {
  color: #f37021 !important;
  font-weight: bold !important;
  background-color: #fff3ed !important;
  border-left: 4px solid #f37021 !important;
}
.table th {
  font-weight: 600;
  text-transform: uppercase;
  font-size: 13px;
}
.table td {
  font-size: 14px;
}
/* Tạo hiệu ứng hover nhẹ cho ô giá để user biết có thể bấm vào */
td .bi-pencil-square {
  opacity: 0;
  transition: opacity 0.2s;
}
td:hover .bi-pencil-square {
  opacity: 1;
  color: #f37021 !important;
}
</style>
