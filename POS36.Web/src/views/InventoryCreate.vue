<template>
  <div class="inventory-create bg-light h-100 d-flex flex-column">
    <div
      class="bg-white p-3 border-bottom d-flex align-items-center justify-content-between shadow-sm z-1"
    >
      <h5 class="mb-0 text-danger fw-bold text-uppercase">
        Thêm mới phiếu kiểm kê
      </h5>
      <span class="badge bg-warning text-dark fs-6 px-3"
        ><i class="bi bi-geo-alt-fill"></i> Nhánh hiện tại</span
      >
    </div>

    <div class="d-flex flex-grow-1 overflow-hidden">
      <div class="flex-grow-1 d-flex flex-column bg-white">
        <div class="p-3 bg-light border-bottom d-flex gap-2">
          <div class="input-group w-50 position-relative">
            <span class="input-group-text bg-white"
              ><i class="bi bi-search"></i
            ></span>
            <input
              type="text"
              class="form-control"
              placeholder="Tìm kiếm mặt hàng..."
              v-model="searchQuery"
              @focus="showDropdown = true"
              @blur="hideDropdownDelay"
            />
            <ul
              v-if="showDropdown && filteredProducts.length > 0"
              class="list-group position-absolute w-100 shadow z-3"
              style="top: 100%; max-height: 250px; overflow-y: auto"
            >
              <li
                v-for="p in filteredProducts"
                :key="p.id"
                class="list-group-item list-group-item-action cursor-pointer d-flex justify-content-between"
                @click="addToCheckList(p)"
              >
                <span
                  ><b>{{ p.maSanPham || "SP" }}</b> - {{ p.tenSanPham }}</span
                >
                <span class="badge bg-secondary">Tồn: {{ p.tonKho }}</span>
              </li>
            </ul>
          </div>
          <button class="btn btn-warning text-white fw-bold">
            <i class="bi bi-plus-circle"></i> Thêm theo Nhóm
          </button>
        </div>

        <div class="flex-grow-1 overflow-auto">
          <table class="table table-hover align-middle mb-0">
            <thead class="table-light text-muted">
              <tr>
                <th class="ps-3">Mã hàng hóa</th>
                <th>Tên hàng hóa</th>
                <th class="text-center">Tồn kho</th>
                <th class="text-center" style="width: 150px">SL Kiểm kê</th>
                <th class="text-center" style="width: 50px"></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(item, index) in checkList" :key="item.sanPhamId">
                <td class="ps-3 fw-bold text-danger">
                  {{ item.maSanPham || "SP" }}<br /><span
                    class="badge bg-danger small"
                    >Chai/Lon</span
                  >
                </td>
                <td class="fw-bold">
                  {{ item.tenSanPham }}<br /><span
                    class="text-muted small fst-italic"
                    ><i class="bi bi-pencil"></i> Ghi chú</span
                  >
                </td>

                <td class="text-center fs-5">
                  {{ item.tonKhoHienTai }}
                  <span
                    v-if="getLech(item) !== 0"
                    class="badge ms-2"
                    :class="
                      getLech(item) > 0 ? 'bg-success' : 'bg-warning text-dark'
                    "
                  >
                    {{ getLech(item) > 0 ? "+" : "" }}{{ getLech(item) }}
                  </span>
                </td>

                <td class="text-center">
                  <div class="input-group input-group-sm">
                    <button
                      class="btn btn-outline-secondary"
                      @click="item.soLuongKiemKe--"
                    >
                      -
                    </button>
                    <input
                      type="number"
                      class="form-control text-center fw-bold text-primary"
                      v-model="item.soLuongKiemKe"
                      min="0"
                    />
                    <button
                      class="btn btn-outline-secondary"
                      @click="item.soLuongKiemKe++"
                    >
                      +
                    </button>
                  </div>
                </td>

                <td
                  class="text-center text-danger cursor-pointer"
                  @click="checkList.splice(index, 1)"
                >
                  <i class="bi bi-x-lg fs-5"></i>
                </td>
              </tr>
              <tr v-if="checkList.length === 0">
                <td colspan="5" class="text-center py-5 text-muted">
                  Vui lòng tìm kiếm và chọn mặt hàng cần kiểm kê.
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <div
        class="bg-light border-start d-flex flex-column"
        style="width: 320px"
      >
        <div class="d-flex bg-white border-bottom">
          <div
            class="flex-fill text-center py-2 fw-bold text-danger border-bottom border-danger border-3"
          >
            Chi tiết
          </div>
          <div
            class="flex-fill text-center py-2 fw-bold text-muted cursor-pointer"
          >
            Ghi chú
          </div>
        </div>

        <div class="p-3 flex-grow-1 overflow-auto">
          <div class="mb-3">
            <label class="form-label small text-muted fw-bold"
              >Mã chứng từ</label
            >
            <input
              type="text"
              class="form-control form-control-sm"
              placeholder="Tự động tạo mã"
              disabled
            />
          </div>
          <div class="mb-3">
            <label class="form-label small text-muted fw-bold"
              >Ngày cân bằng kho</label
            >
            <input
              type="text"
              class="form-control form-control-sm"
              :value="new Date().toLocaleString('vi-VN')"
              disabled
            />
          </div>
          <div class="mb-3">
            <label class="form-label small text-muted fw-bold"
              >Ghi chú phiếu</label
            >
            <textarea
              class="form-control form-control-sm"
              rows="2"
              v-model="ghiChuPhieu"
              placeholder="Nhập ghi chú..."
            ></textarea>
          </div>
          <div class="d-flex justify-content-between border-bottom pb-2 mb-2">
            <span class="fw-bold">Trạng thái</span>
            <span class="badge bg-primary">Đang xử lý</span>
          </div>
          <div class="d-flex justify-content-between">
            <span class="fw-bold">Tổng số lượng kiểm</span>
            <span class="badge bg-primary fs-6">{{ totalKiemKe }}</span>
          </div>
        </div>

        <div
          class="p-3 bg-white border-top d-flex flex-wrap gap-2 justify-content-end"
        >
          <button
            class="btn btn-success fw-bold flex-grow-1"
            @click="submitPhieu('Hoàn thành')"
          >
            <i class="bi bi-check2-circle"></i> Hoàn thành
          </button>
          <div class="w-100 d-flex gap-2 mt-1">
            <router-link
              to="/admin/inventory"
              class="btn btn-warning text-white fw-bold w-50"
              ><i class="bi bi-chevron-double-left"></i> Thoát</router-link
            >
            <button
              class="btn btn-warning text-white fw-bold w-50"
              @click="submitPhieu('Đang xử lý')"
            >
              <i class="bi bi-floppy"></i> Lưu tạm
            </button>
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

const products = ref([]);
const searchQuery = ref("");
const showDropdown = ref(false);

const checkList = ref([]);
const ghiChuPhieu = ref("");

// Tính độ lệch: Lệch = SL Kiểm kê - Tồn hệ thống
const getLech = (item) => item.soLuongKiemKe - item.tonKhoHienTai;

const totalKiemKe = computed(() =>
  checkList.value.reduce((sum, item) => sum + item.soLuongKiemKe, 0),
);

// Lọc sản phẩm tìm kiếm
const filteredProducts = computed(() => {
  if (!searchQuery.value) return [];
  return products.value.filter(
    (p) =>
      p.tenSanPham.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      (p.maSanPham &&
        p.maSanPham.toLowerCase().includes(searchQuery.value.toLowerCase())),
  );
});

const fetchProducts = async () => {
  try {
    const res = await axios.get(
      `/api/KiemKe/san-pham-ton-kho?chiNhanhId=${globalState.value.activeBranchId || 0}`,
    );
    products.value = res.data;
  } catch (e) {
    console.error(e);
  }
};

onMounted(fetchProducts);

const hideDropdownDelay = () => {
  setTimeout(() => {
    showDropdown.value = false;
  }, 200);
};

// Chọn sản phẩm đưa vào danh sách kiểm kê
const addToCheckList = (prod) => {
  const exist = checkList.value.find((c) => c.sanPhamId === prod.id);
  if (!exist) {
    checkList.value.unshift({
      sanPhamId: prod.id,
      maSanPham: prod.maSanPham,
      tenSanPham: prod.tenSanPham,
      tonKhoHienTai: prod.tonKho || 0,
      soLuongKiemKe: prod.tonKho || 0, // Mặc định gán bằng tồn kho hệ thống cho nhanh
    });
  }
  searchQuery.value = "";
  showDropdown.value = false;
};

// Hàm submit Phiếu (Lưu tạm hoặc Hoàn thành)
const submitPhieu = async (trangThai) => {
  if (checkList.value.length === 0)
    return swal.fire(
      "Cảnh báo",
      "Vui lòng chọn ít nhất 1 mặt hàng để kiểm kê!",
      "warning",
    );

  let isConfirm = true;
  if (trangThai === "Hoàn thành") {
    const result = await swal.fire({
      title: "Chốt cân bằng kho?",
      text: "Hệ thống sẽ cập nhật lại Tồn kho thực tế theo số lượng bạn vừa kiểm. Hành động này không thể hoàn tác!",
      icon: "warning",
      showCancelButton: true,
      confirmButtonText: "Đồng ý cân bằng",
    });
    isConfirm = result.isConfirmed;
  }

  if (isConfirm) {
    try {
      const payload = {
        chiNhanhId: globalState.value.activeBranchId || 0,
        ghiChu: ghiChuPhieu.value,
        trangThai: trangThai,
        chiTiets: checkList.value.map((c) => ({
          sanPhamId: c.sanPhamId,
          tonKhoHienTai: c.tonKhoHienTai,
          soLuongKiemKe: c.soLuongKiemKe,
        })),
      };

      await axios.post("/api/KiemKe", payload);

      swal.fire({
        toast: true,
        position: "top-end",
        icon: "success",
        title: trangThai === "Hoàn thành" ? "Đã Cân Bằng Kho!" : "Đã Lưu Nháp!",
        timer: 1500,
        showConfirmButton: false,
      });
      router.push("/admin/inventory"); // Quay lại trang danh sách
    } catch (e) {
      swal.fire("Lỗi", e.response?.data || "Không thể lưu phiếu", "error");
    }
  }
};
// Tính năng thêm nhanh nhiều món theo Nhóm (Danh mục)
const addByGroup = async () => {
  // Lọc ra các tên nhóm duy nhất từ danh sách sản phẩm
  const groups = [...new Set(products.value.map((p) => p.tenDanhMuc))];
  const inputOptions = {};
  groups.forEach((g) => (inputOptions[g] = g));

  const { value: selectedGroup } = await swal.fire({
    title: "Thêm theo nhóm hàng",
    input: "select",
    inputOptions,
    inputPlaceholder: "-- Chọn nhóm hàng hóa --",
    showCancelButton: true,
    confirmButtonText: "Thêm tất cả",
  });

  if (selectedGroup) {
    // Lấy tất cả sp thuộc nhóm đó
    const itemsToAdd = products.value.filter(
      (p) => p.tenDanhMuc === selectedGroup,
    );
    let addedCount = 0;

    itemsToAdd.forEach((prod) => {
      // Chỉ thêm nếu chưa có trong danh sách kiểm kê
      if (!checkList.value.find((c) => c.sanPhamId === prod.id)) {
        checkList.value.unshift({
          sanPhamId: prod.id,
          maSanPham: prod.maSanPham,
          tenSanPham: prod.tenSanPham,
          tonKhoHienTai: prod.tonKho || 0,
          soLuongKiemKe: prod.tonKho || 0,
        });
        addedCount++;
      }
    });
    swal.fire({
      toast: true,
      position: "top-end",
      icon: "success",
      title: `Đã thêm ${addedCount} mặt hàng nhóm ${selectedGroup}`,
      timer: 2000,
      showConfirmButton: false,
    });
  }
};
</script>

<style scoped>
.cursor-pointer {
  cursor: pointer;
}
/* Ẩn mũi tên tăng giảm mặc định của thẻ input type="number" */
input[type="number"]::-webkit-outer-spin-button,
input[type="number"]::-webkit-inner-spin-button {
  -webkit-appearance: none;
  margin: 0;
}
</style>
