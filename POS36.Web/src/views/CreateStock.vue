<template>
  <div class="create-stock h-100 d-flex flex-column bg-light">
    <div
      class="bg-white p-3 border-bottom d-flex align-items-center justify-content-between shadow-sm z-1"
    >
      <h5 class="mb-0 text-danger fw-bold text-uppercase">
        THÊM MỚI PHIẾU NHẬP HÀNG
      </h5>
      <span class="badge bg-warning text-dark fs-6 px-3"
        ><i class="bi bi-geo-alt-fill"></i> Nhánh Trung tâm</span
      >
    </div>

    <div class="d-flex flex-grow-1 overflow-hidden">
      <div class="flex-grow-1 bg-white d-flex flex-column">
        <div class="p-3 bg-light border-bottom d-flex gap-2">
          <div class="input-group w-50 position-relative">
            <span class="input-group-text bg-white border-end-0"
              ><i class="bi bi-search"></i
            ></span>
            <input
              type="text"
              class="form-control border-start-0 shadow-none"
              placeholder="Tìm kiếm mặt hàng..."
              v-model="searchQuery"
              @focus="showDropdown = true"
              @blur="hideDropdownDelay"
            />
            <ul
              v-if="showDropdown && filteredProducts.length > 0"
              class="list-group position-absolute w-100 shadow z-3 product-search-result"
              style="top: 100%"
            >
              <li
                v-for="p in filteredProducts"
                :key="p.id"
                class="list-group-item list-group-item-action cursor-pointer d-flex justify-content-between"
                @click="addToCart(p)"
              >
                <span
                  ><b>{{ p.maSanPham || "SP" }}</b> - {{ p.tenSanPham }}</span
                >
                <span class="text-success">{{ formatPrice(p.giaBan) }}</span>
              </li>
            </ul>
          </div>
          <button class="btn btn-warning text-white fw-bold">
            <i class="bi bi-plus"></i> Hàng mới
          </button>
          <button
            class="btn btn-warning text-white fw-bold"
            @click="addByGroup"
          >
            <i class="bi bi-collection"></i> Thêm theo Nhóm
          </button>
        </div>

        <div class="flex-grow-1 overflow-auto">
          <table
            class="table table-hover table-bordered align-middle mb-0"
            style="font-size: 0.9rem"
          >
            <thead class="table-light text-muted">
              <tr>
                <th class="ps-3" style="width: 15%">Mã hàng</th>
                <th style="width: 35%">Tên hàng hóa</th>
                <th class="text-center" style="width: 15%">SL</th>
                <th class="text-end" style="width: 15%">Giá nhập</th>
                <th class="text-end" style="width: 15%">Thành tiền</th>
                <th class="text-center" style="width: 5%"></th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="importList.length === 0">
                <td colspan="6" class="text-center py-5 text-muted fst-italic">
                  Vui lòng tìm kiếm mặt hàng hoặc thêm theo nhóm để nhập...
                </td>
              </tr>
              <tr v-for="(item, index) in importList" :key="item.id">
                <td class="ps-3 text-muted">{{ item.maSanPham || "SP" }}</td>
                <td class="fw-bold text-dark">{{ item.tenSanPham }}</td>
                <td class="text-center p-1">
                  <input
                    type="number"
                    class="form-control form-control-sm text-center fw-bold"
                    v-model="item.soLuong"
                    min="1"
                  />
                </td>
                <td class="text-end p-1">
                  <input
                    type="number"
                    class="form-control form-control-sm text-end text-primary fw-bold"
                    v-model="item.giaNhap"
                    min="0"
                  />
                </td>
                <td class="text-end fw-bold text-danger">
                  {{ formatPrice(item.soLuong * item.giaNhap) }}
                </td>
                <td
                  class="text-center text-danger cursor-pointer"
                  @click="removeItem(index)"
                >
                  <i class="bi bi-trash fs-5"></i>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <div
        class="bg-light border-start d-flex flex-column"
        style="width: 360px"
      >
        <div class="p-3 bg-white border-bottom fw-bold text-danger text-center">
          <i class="bi bi-info-circle me-1"></i> CHI TIẾT PHIẾU
        </div>

        <div class="p-3 flex-grow-1 overflow-auto form-sm">
          <div class="row mb-3 g-2">
            <div class="col-6">
              <label class="form-label small text-secondary fw-bold"
                >Ngày nhập</label
              >
              <input
                type="text"
                class="form-control form-control-sm text-muted"
                :value="currentDate"
                disabled
              />
            </div>
            <div class="col-6">
              <label class="form-label small text-secondary fw-bold"
                >Trạng thái</label
              >
              <span
                class="form-control form-control-sm bg-warning text-dark text-center fw-bold border-0"
                >Đang xử lý</span
              >
            </div>
          </div>

          <div class="bg-white p-3 rounded-4 border shadow-sm mt-4">
            <div
              class="d-flex justify-content-between mb-2 border-bottom pb-2 align-items-center"
            >
              <span class="fw-bold text-secondary"
                >Tổng số lượng ({{ importList.length }} món)</span
              >
              <span class="badge bg-primary fs-6 px-3">{{ totalQty }}</span>
            </div>
            <div class="d-flex justify-content-between mb-3 align-items-center">
              <span class="fw-bold fs-6 text-dark">Tổng tiền hàng</span>
              <span class="fw-bold fs-5 text-danger">{{
                formatPrice(totalAmount)
              }}</span>
            </div>
            <div class="d-flex justify-content-between align-items-center mb-3">
              <span class="fw-bold text-secondary">Thanh toán cho NCC</span>
              <input
                type="number"
                class="form-control form-control-sm text-end fw-bold text-primary w-50"
                v-model="form.tienThanhToan"
              />
            </div>
            <div class="d-flex justify-content-between border-top pt-2">
              <span class="fw-bold text-secondary">Tài khoản trả</span>
              <span class="text-success fw-bold text-uppercase"
                ><i class="bi bi-cash"></i> Tiền mặt</span
              >
            </div>
          </div>
        </div>

        <div class="p-3 bg-white border-top d-flex gap-2 justify-content-end">
          <button
            class="btn btn-success fw-bold w-50"
            @click="savePhieu('Hoàn thành')"
          >
            <i class="bi bi-check-circle me-1"></i> HOÀN THÀNH
          </button>
          <div class="d-flex flex-column w-50 gap-2">
            <button
              class="btn btn-warning text-white fw-bold btn-sm"
              @click="savePhieu('Đang xử lý')"
            >
              <i class="bi bi-save"></i> LƯU NHÁP
            </button>
            <router-link
              to="/admin/import"
              class="btn btn-secondary fw-bold btn-sm"
              ><i class="bi bi-x-lg"></i> HỦY</router-link
            >
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed, inject } from "vue";
import { useRouter } from "vue-router";
import axios from "axios";
import { globalState } from "../store";

const router = useRouter();
const swal = inject("$swal");

const products = ref([]); // Danh sách SP để tìm kiếm
const searchQuery = ref("");
const showDropdown = ref(false);

const importList = ref([]); // Giỏ hàng nhập
const form = ref({ nhaCungCap: "", tienThanhToan: 0 });

const currentDate = new Date().toLocaleString("vi-VN");
const formatPrice = (price) =>
  new Intl.NumberFormat("vi-VN").format(price || 0);

// Tính toán tổng cộng
const totalQty = computed(() =>
  importList.value.reduce((sum, item) => sum + item.soLuong, 0),
);
const totalAmount = computed(() =>
  importList.value.reduce((sum, item) => sum + item.soLuong * item.giaNhap, 0),
);

// Logic tìm kiếm sản phẩm
const filteredProducts = computed(() => {
  if (!searchQuery.value) return [];
  return products.value.filter((p) =>
    p.tenSanPham.toLowerCase().includes(searchQuery.value.toLowerCase()),
  );
});

// Load danh sách sản phẩm để làm data tìm kiếm
const fetchProducts = async () => {
  try {
    // ĐÃ SỬA LẠI ĐÚNG ĐƯỜNG DẪN API
    const res = await axios.get(
      `/api/KiemKe/san-pham-ton-kho?chiNhanhId=${globalState.value.activeBranchId || 0}`,
    );
    products.value = res.data;
  } catch (e) {
    console.error(e);
  }
};
onMounted(fetchProducts);

// Chọn SP từ dropdown đưa vào bảng
const addToCart = (prod) => {
  const exist = importList.value.find((c) => c.id === prod.id);
  if (exist) {
    exist.soLuong++;
  } else {
    importList.value.unshift({
      id: prod.id,
      maSanPham: prod.maSanPham,
      tenSanPham: prod.tenSanPham,
      tenDanhMuc: prod.tenDanhMuc,
      soLuong: 1,
      giaNhap: prod.giaVon || 0, // Default giá nhập = Giá vốn hiện tại
    });
  }
  searchQuery.value = "";
  showDropdown.value = false;
};

const removeItem = (index) => {
  importList.value.splice(index, 1);
};

// Đóng dropdown tìm kiếm sau 1 chút (để kịp nhận sự kiện click)
const hideDropdownDelay = () => {
  setTimeout(() => {
    showDropdown.value = false;
  }, 200);
};

// ==========================================
// CHỨC NĂNG 1: THÊM THEO NHÓM (SWEETALERT LẬP LẬP)
// ==========================================
const addByGroup = async () => {
  // Lấy danh sách nhóm (danh mục) độc duy
  const groups = [
    ...new Set(products.value.map((p) => p.tenDanhMuc || "Khác")),
  ];

  // Tạo cấu trúc options cho SweetAlert
  const inputOptions = {};
  groups.forEach((g) => {
    inputOptions[g] = g;
  });

  const { value: selectedGroup } = await swal.fire({
    title: "Chọn nhóm hàng muốn thêm",
    input: "select",
    inputOptions: inputOptions,
    inputPlaceholder: "-- Vui lòng chọn --",
    showCancelButton: true,
    confirmButtonText: "Thêm ngay",
    cancelButtonText: "Hủy",
  });

  if (selectedGroup) {
    // Lọc các SP thuộc nhóm này
    const itemsToAdd = products.value.filter(
      (p) => (p.tenDanhMuc || "Khác") === selectedGroup,
    );
    // Đưa hết vào giỏ hàng
    itemsToAdd.forEach((prod) => addToCart(prod));

    swal.fire({
      toast: true,
      position: "top-end",
      icon: "success",
      title: `Đã thêm ${itemsToAdd.length} món từ nhóm ${selectedGroup}`,
      timer: 1500,
      showConfirmButton: false,
    });
  }
};

// ==========================================
// CHỨC NĂNG 2: LƯU PHIẾU (GỌI API TẠO PHIẾU + TĂNG KHO + TẠO PHIẾU CHI)
// ==========================================
const savePhieu = async (trangThai) => {
  if (importList.value.length === 0)
    return swal.fire("Chú ý", "Chưa có mặt hàng nào để nhập kho!", "warning");

  let title = "Lưu nháp?";
  let text = "Hệ thống sẽ lưu phiếu mà chưa tăng tồn kho.";
  let icon = "question";

  if (trangThai === "Hoàn thành") {
    title = "Chốt nhập kho?";
    text =
      "Hệ thống sẽ CỘNG số lượng vào kho và tạo PHIẾU CHI tự động (nếu có thanh toán tiền cho NCC).";
    icon = "warning";
  }

  const { isConfirmed } = await swal.fire({
    title,
    text,
    icon,
    showCancelButton: true,
    confirmButtonText: "Đồng ý",
    confirmButtonColor: "#28a745",
  });

  if (isConfirmed) {
    try {
      // Chuẩn bị payload theo đúng chuẩn API đã thiết kế hôm qua
      const payload = {
        chiNhanhId: globalState.value.activeBranchId || 0,
        nhaCungCap: form.value.nhaCungCap,
        ghiChu: "",
        trangThai: trangThai,
        tienThanhToan: form.value.tienThanhToan,
        chiTiets: importList.value.map((c) => ({
          sanPhamId: c.id,
          soLuong: c.soLuong,
          giaNhap: c.giaNhap,
        })),
      };

      await axios.post("/api/NhapHang", payload);

      swal.fire({
        toast: true,
        position: "top-end",
        icon: "success",
        title: "Lưu phiếu nhập hàng thành công!",
        timer: 2000,
        showConfirmButton: false,
      });
      // Thành công thì quay về trang danh sách
      router.push("/admin/import-stock");
    } catch (e) {
      swal.fire(
        "Lỗi",
        e.response?.data?.message || "Không thể lưu phiếu nhập.",
        "error",
      );
    }
  }
};
</script>

<style scoped>
.create-stock {
  font-size: 14px;
}
.form-sm label {
  margin-bottom: 2px;
}
.cursor-pointer {
  cursor: pointer;
}
input[type="number"]::-webkit-outer-spin-button,
input[type="number"]::-webkit-inner-spin-button {
  -webkit-appearance: none;
  margin: 0;
}
.product-search-result {
  max-height: 300px;
  overflow-y: auto;
  border-radius: 8px;
  border-color: #dee2e6;
}
</style>
