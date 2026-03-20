<script setup>
import { ref, onMounted, computed, inject, watch } from "vue";
import axios from "axios";
import { globalState } from "../store";

const swal = inject("$swal");
const products = ref([]);
const searchQuery = ref("");
const cart = ref([]); // Giỏ chứa các món chuẩn bị nhập kho
const ghiChu = ref("");

// Lấy danh sách hàng hóa (Để chọn nhập)
const fetchProducts = async () => {
  if (!globalState.value.activeBranchId) return;
  try {
    const res = await axios.get(
      `/api/SanPham/danh-sach?chiNhanhId=${globalState.value.activeBranchId}`,
    );
    products.value = res.data;
  } catch (error) {
    console.error("Lỗi", error);
  }
};

onMounted(() => fetchProducts());
watch(
  () => globalState.value.activeBranchId,
  () => {
    fetchProducts();
    cart.value = []; // Đổi chi nhánh thì làm trống giỏ nhập hàng
  },
);

const filteredProducts = computed(() => {
  if (!searchQuery.value) return products.value;
  return products.value.filter((p) =>
    p.tenSanPham.toLowerCase().includes(searchQuery.value.toLowerCase()),
  );
});

// TÍNH TỔNG TIỀN PHIẾU NHẬP
const totalAmount = computed(() => {
  return cart.value.reduce(
    (total, item) => total + item.soLuong * item.donGiaNhap,
    0,
  );
});

// THÊM VÀO GIỎ NHẬP HÀNG
const addToCart = (prod) => {
  const existingItem = cart.value.find((item) => item.sanPhamId === prod.id);
  if (existingItem) {
    existingItem.soLuong += 1;
  } else {
    cart.value.push({
      sanPhamId: prod.id,
      tenSanPham: prod.tenSanPham,
      soLuong: 1,
      donGiaNhap: 0, // Yêu cầu người dùng tự gõ giá nhập
    });
  }
};

const removeFromCart = (index) => {
  cart.value.splice(index, 1);
};

// GỬI PHIẾU NHẬP LÊN BACKEND
const submitImport = async () => {
  if (cart.value.length === 0)
    return swal.fire("Cảnh báo", "Chưa chọn mặt hàng nào để nhập!", "warning");

  // Kiểm tra xem có dòng nào chưa nhập giá gốc hay không
  const invalidItem = cart.value.find(
    (item) => item.donGiaNhap <= 0 || item.soLuong <= 0,
  );
  if (invalidItem)
    return swal.fire(
      "Lỗi",
      "Số lượng và Đơn giá nhập phải lớn hơn 0!",
      "error",
    );

  try {
    const payload = {
      chiNhanhId: globalState.value.activeBranchId,
      ghiChu: ghiChu.value,
      tongTien: totalAmount.value,
      chiTiets: cart.value.map((item) => ({
        sanPhamId: item.sanPhamId,
        soLuong: item.soLuong,
        donGiaNhap: item.donGiaNhap,
      })),
    };

    await axios.post("/api/Kho/nhap-hang", payload);

    swal.fire({
      icon: "success",
      title: "Nhập kho thành công!",
      text: "Số lượng đã được cộng vào chi nhánh hiện tại.",
      timer: 2000,
    });
    cart.value = [];
    ghiChu.value = "";
    fetchProducts(); // Load lại tồn kho bên trái cho nó nảy số mới
  } catch (error) {
    swal.fire("Lỗi", "Có lỗi xảy ra khi nhập hàng", "error");
  }
};
</script>

<template>
  <div class="container-fluid px-4 py-4">
    <div class="row g-4">
      <div class="col-lg-5">
        <div class="card border-0 shadow-sm h-100 rounded-3">
          <div class="card-header bg-white border-bottom py-3">
            <input
              v-model="searchQuery"
              type="text"
              class="form-control form-control-lg bg-light"
              placeholder="Tìm tên hàng hóa để nhập..."
            />
          </div>
          <div
            class="card-body p-0"
            style="max-height: 600px; overflow-y: auto"
          >
            <div class="list-group list-group-flush">
              <button
                v-for="prod in filteredProducts"
                :key="prod.id"
                @click="addToCart(prod)"
                class="list-group-item list-group-item-action p-3 border-bottom d-flex justify-content-between align-items-center"
              >
                <div>
                  <h6 class="mb-1 fw-bold text-dark">{{ prod.tenSanPham }}</h6>
                  <small class="text-muted"
                    >Tồn hiện tại:
                    <span class="fw-bold text-primary">{{
                      prod.tonKho
                    }}</span></small
                  >
                </div>
                <i class="bi bi-plus-circle-fill fs-4 text-success"></i>
              </button>
            </div>
          </div>
        </div>
      </div>

      <div class="col-lg-7">
        <div class="card border-0 shadow-sm h-100 rounded-3">
          <div
            class="card-header bg-primary text-white border-bottom py-3 d-flex justify-content-between align-items-center"
          >
            <h5 class="fw-bold mb-0">
              <i class="bi bi-card-checklist me-2"></i> PHIẾU NHẬP HÀNG
            </h5>
            <span class="badge bg-light text-primary fs-6"
              >{{ cart.length }} món</span
            >
          </div>

          <div class="card-body p-0 table-responsive" style="min-height: 400px">
            <table class="table table-hover align-middle mb-0">
              <thead class="table-light text-muted small">
                <tr>
                  <th class="ps-3">Tên hàng</th>
                  <th style="width: 120px">Số lượng</th>
                  <th style="width: 150px">Đơn giá nhập</th>
                  <th class="text-end">Thành tiền</th>
                  <th class="text-center" style="width: 50px"></th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="(item, index) in cart" :key="index">
                  <td class="ps-3 fw-bold">{{ item.tenSanPham }}</td>
                  <td>
                    <input
                      v-model="item.soLuong"
                      type="number"
                      class="form-control form-control-sm text-center"
                      min="1"
                    />
                  </td>
                  <td>
                    <input
                      v-model="item.donGiaNhap"
                      type="number"
                      class="form-control form-control-sm text-end"
                      placeholder="Giá nhập..."
                    />
                  </td>
                  <td class="text-end fw-bold text-danger">
                    {{
                      (item.soLuong * item.donGiaNhap).toLocaleString("vi-VN")
                    }}
                  </td>
                  <td class="text-center">
                    <button
                      @click="removeFromCart(index)"
                      class="btn btn-sm btn-outline-danger border-0"
                    >
                      <i class="bi bi-trash"></i>
                    </button>
                  </td>
                </tr>
                <tr v-if="cart.length === 0">
                  <td colspan="5" class="text-center py-5 text-muted">
                    <i class="bi bi-cart-x fs-1 d-block mb-2"></i> Chưa có hàng
                    hóa nào được chọn
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <div class="card-footer bg-white border-top p-4">
            <div class="row align-items-end">
              <div class="col-md-7">
                <label class="form-label text-muted small fw-bold"
                  >Ghi chú phiếu nhập</label
                >
                <input
                  v-model="ghiChu"
                  type="text"
                  class="form-control"
                  placeholder="Ví dụ: Nhập hàng đợt 1 từ nhà cung cấp A..."
                />
              </div>
              <div class="col-md-5 text-end">
                <p class="mb-1 text-muted">Tổng cộng</p>
                <h3 class="fw-bold text-danger mb-3">
                  {{ totalAmount.toLocaleString("vi-VN") }} đ
                </h3>
                <button
                  @click="submitImport"
                  class="btn btn-primary w-100 fw-bold py-2"
                >
                  <i class="bi bi-check-circle me-2"></i> HOÀN THÀNH NHẬP KHO
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
