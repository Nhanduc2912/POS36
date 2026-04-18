<script setup>
import { ref, onMounted, inject } from "vue";
import axios from "axios";

const swal = inject("$swal");
const customers = ref([]);
const searchText = ref("");

const fetchCustomers = async () => {
  try {
    let url = "/api/KhachHang/danh-sach";
    if (searchText.value)
      url += `?search=${encodeURIComponent(searchText.value)}`;
    const res = await axios.get(url);
    customers.value = res.data;
  } catch (error) {
    console.error("Lỗi tải danh sách khách hàng", error);
  }
};

onMounted(() => fetchCustomers());

const handleSearch = () => fetchCustomers();

const formatDate = (d) => {
  if (!d) return "---";
  return new Date(d).toLocaleDateString("vi-VN");
};

const getPointBadgeClass = (diem) => {
  if (diem >= 200) return "bg-danger";
  if (diem >= 50) return "bg-warning text-dark";
  return "bg-success";
};

const handleAddCustomer = async () => {
  const { value: formValues } = await swal.fire({
    title: "Thêm Khách Hàng Mới",
    html: `
      <input id="swal-ten" class="form-control mb-2" placeholder="Tên khách hàng *">
      <input id="swal-sdt" class="form-control mb-2" placeholder="Số điện thoại *">
      <input id="swal-email" class="form-control mb-2" type="email" placeholder="Email">
      <input id="swal-diachi" class="form-control mb-2" placeholder="Địa chỉ (Không bắt buộc)">
      <input id="swal-ngaysinh" class="form-control mb-2" type="date" title="Ngày sinh">
      <textarea id="swal-ghichu" class="form-control" rows="2" placeholder="Ghi chú (Không bắt buộc)"></textarea>
    `,
    showCancelButton: true,
    cancelButtonText: "Hủy",
    confirmButtonText: "Lưu khách hàng",
    preConfirm: () => {
      const ten = document.getElementById("swal-ten").value;
      const sdt = document.getElementById("swal-sdt").value;
      const email = document.getElementById("swal-email").value;
      const diachi = document.getElementById("swal-diachi").value;
      const ngaysinh = document.getElementById("swal-ngaysinh").value;
      const ghichu = document.getElementById("swal-ghichu").value;

      if (!ten || !sdt) {
        swal.showValidationMessage("Vui lòng nhập Tên và Số điện thoại!");
        return false;
      }
      // Kiểm tra ô Tên không phải số điện thoại
      if (/^[0-9]+$/.test(ten.trim())) {
        swal.showValidationMessage(
          "Ô Tên không được nhập số điện thoại! Hãy nhập đúng họ tên khách hàng.",
        );
        return false;
      }
      // Kiểm tra SĐT chỉ gồm chữ số và độ dài hợp lệ (8-11 số)
      if (!/^[0-9]{8,11}$/.test(sdt.trim())) {
        swal.showValidationMessage(
          "Số điện thoại chỉ gồm chữ số, độ dài 8–11 ký tự!",
        );
        return false;
      }

      return {
        tenKhachHang: ten,
        soDienThoai: sdt,
        email: email || null,
        diaChi: diachi || null,
        ngaySinh: ngaysinh || null,
        ghiChu: ghichu || null,
      };
    },
  });

  if (formValues) {
    try {
      await axios.post("/api/KhachHang", formValues);
      swal.fire({
        icon: "success",
        title: "Đã thêm khách hàng!",
        timer: 1500,
        showConfirmButton: false,
      });
      fetchCustomers();
    } catch (e) {
      swal.fire(
        "Lỗi",
        e.response?.data?.message ||
          e.response?.data ||
          "Không thể thêm khách hàng",
        "error",
      );
    }
  }
};

const handleEditCustomer = async (cust) => {
  const ngaySinhStr = cust.ngaySinh
    ? new Date(cust.ngaySinh).toISOString().split("T")[0]
    : "";

  const { value: formValues } = await swal.fire({
    title: "Sửa Thông Tin Khách Hàng",
    html: `
      <input id="swal-ten-edit" class="form-control mb-2" value="${cust.tenKhachHang}" placeholder="Tên khách hàng">
      <input id="swal-sdt-edit" class="form-control mb-2" value="${cust.soDienThoai}" placeholder="Số điện thoại">
      <input id="swal-email-edit" class="form-control mb-2" type="email" value="${cust.email || ""}" placeholder="Email">
      <input id="swal-diachi-edit" class="form-control mb-2" value="${cust.diaChi || ""}" placeholder="Địa chỉ">
      <input id="swal-ngaysinh-edit" class="form-control mb-2" type="date" value="${ngaySinhStr}">
      <textarea id="swal-ghichu-edit" class="form-control" rows="2" placeholder="Ghi chú">${cust.ghiChu || ""}</textarea>
    `,
    showCancelButton: true,
    cancelButtonText: "Hủy",
    confirmButtonText: "Cập nhật",
    preConfirm: () => {
      const ten = document.getElementById("swal-ten-edit").value;
      const sdt = document.getElementById("swal-sdt-edit").value;
      const email = document.getElementById("swal-email-edit").value;
      const diachi = document.getElementById("swal-diachi-edit").value;
      const ngaysinh = document.getElementById("swal-ngaysinh-edit").value;
      const ghichu = document.getElementById("swal-ghichu-edit").value;

      if (!ten || !sdt) {
        swal.showValidationMessage("Vui lòng nhập Tên và Số điện thoại!");
        return false;
      }
      // Kiểm tra ô Tên không phải số điện thoại
      if (/^[0-9]+$/.test(ten.trim())) {
        swal.showValidationMessage(
          "Ô Tên không được nhập số điện thoại! Hãy nhập đúng họ tên khách hàng.",
        );
        return false;
      }
      // Kiểm tra SĐT chỉ gồm chữ số và độ dài hợp lệ (8-11 số)
      if (!/^[0-9]{8,11}$/.test(sdt.trim())) {
        swal.showValidationMessage(
          "Số điện thoại chỉ gồm chữ số, độ dài 8–11 ký tự!",
        );
        return false;
      }

      return {
        tenKhachHang: ten,
        soDienThoai: sdt,
        email: email || null,
        diaChi: diachi || null,
        ngaySinh: ngaysinh || null,
        ghiChu: ghichu || null,
      };
    },
  });

  if (formValues) {
    try {
      await axios.put(`/api/KhachHang/${cust.id}`, formValues);
      swal.fire({
        icon: "success",
        title: "Đã cập nhật!",
        timer: 1000,
        showConfirmButton: false,
      });
      fetchCustomers();
    } catch (e) {
      swal.fire("Lỗi", e.response?.data || "Không thể sửa", "error");
    }
  }
};

const handleDeleteCustomer = (id) => {
  swal
    .fire({
      title: "Xóa khách hàng này?",
      text: "Hóa đơn cũ sẽ không bị ảnh hưởng.",
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#d33",
      confirmButtonText: "Xóa",
      cancelButtonText: "Hủy",
    })
    .then(async (result) => {
      if (result.isConfirmed) {
        try {
          await axios.delete(`/api/KhachHang/${id}`);
          swal.fire({
            icon: "success",
            title: "Đã xóa!",
            timer: 1000,
            showConfirmButton: false,
          });
          fetchCustomers();
        } catch (e) {
          swal.fire("Lỗi", "Không thể xóa khách hàng", "error");
        }
      }
    });
};
</script>

<template>
  <div class="container-fluid px-4 py-4">
    <div class="card border-0 shadow-sm rounded-3">
      <div
        class="card-header bg-white border-bottom py-3 d-flex justify-content-between align-items-center flex-wrap gap-2"
      >
        <h5 class="fw-bold text-dark mb-0">
          <i class="bi bi-person-hearts text-primary me-2"></i> KHÁCH HÀNG THÂN
          THIẾT
        </h5>
        <div class="d-flex gap-2 align-items-center">
          <div class="input-group" style="max-width: 280px">
            <span class="input-group-text bg-light border-end-0">
              <i class="bi bi-search text-muted"></i>
            </span>
            <input
              v-model="searchText"
              @input="handleSearch"
              type="text"
              class="form-control border-start-0 bg-light"
              placeholder="Tìm tên hoặc SĐT..."
            />
          </div>
          <button
            @click="handleAddCustomer"
            class="btn btn-primary btn-sm fw-bold px-4 rounded-pill shadow-sm"
          >
            <i class="bi bi-person-plus-fill me-1"></i> THÊM MỚI
          </button>
        </div>
      </div>

      <div class="card-body p-0 table-responsive">
        <table class="table table-hover align-middle mb-0">
          <thead class="table-light text-muted small text-uppercase">
            <tr>
              <th class="ps-4" style="width: 50px">#</th>
              <th>Tên khách hàng</th>
              <th>Số điện thoại</th>
              <th>Email</th>
              <th class="text-center">Điểm hiện tại</th>
              <th class="text-center">Tổng tích lũy</th>
              <th>Ngày tạo</th>
              <th class="text-center" style="width: 120px">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(cust, index) in customers" :key="cust.id">
              <td class="ps-4 fw-bold text-muted">{{ index + 1 }}</td>
              <td class="fw-bold text-dark">{{ cust.tenKhachHang }}</td>
              <td>
                <i class="bi bi-telephone me-1 text-muted"></i
                >{{ cust.soDienThoai }}
              </td>
              <td class="text-secondary">{{ cust.email || "---" }}</td>
              <td class="text-center">
                <span
                  class="badge rounded-pill fw-normal px-3 py-2"
                  :class="getPointBadgeClass(cust.diemHienTai)"
                >
                  <i class="bi bi-star-fill me-1"></i>
                  {{ cust.diemHienTai }} điểm
                </span>
              </td>
              <td class="text-center fw-bold text-primary">
                {{ cust.tongDiemTichLuy }}
              </td>
              <td class="text-muted small">{{ formatDate(cust.ngayTao) }}</td>
              <td class="text-center">
                <button
                  @click="handleEditCustomer(cust)"
                  class="btn btn-sm btn-light text-primary me-1"
                >
                  <i class="bi bi-pencil-square"></i>
                </button>
                <button
                  @click="handleDeleteCustomer(cust.id)"
                  class="btn btn-sm btn-light text-danger"
                >
                  <i class="bi bi-trash"></i>
                </button>
              </td>
            </tr>
            <tr v-if="customers.length === 0">
              <td colspan="8" class="text-center py-5 text-muted">
                <i class="bi bi-person-x fs-1 d-block mb-2"></i>
                Chưa có khách hàng nào. Bấm "Thêm mới" để bắt đầu!
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<style scoped>
.table th {
  font-weight: 600;
  font-size: 13px;
}
.table td {
  font-size: 14px;
}
</style>
