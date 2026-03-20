<script setup>
import { ref, onMounted, watch, inject } from "vue";
import axios from "axios";
import { globalState } from "../store";

const swal = inject("$swal");
const employees = ref([]);

const fetchEmployees = async () => {
  if (!globalState.value.activeBranchId) {
    employees.value = [];
    return;
  }
  try {
    const res = await axios.get(
      `/api/NhanVien/danh-sach?chiNhanhId=${globalState.value.activeBranchId}`,
    );
    employees.value = res.data;
  } catch (error) {
    console.error("Lỗi tải danh sách nhân viên", error);
  }
};

onMounted(() => fetchEmployees());
watch(
  () => globalState.value.activeBranchId,
  () => fetchEmployees(),
);

const handleAddEmployee = async () => {
  if (!globalState.value.activeBranchId)
    return swal.fire("Lỗi", "Vui lòng chọn chi nhánh trước!", "warning");

  const { value: formValues } = await swal.fire({
    title: "Thêm Nhân Viên & Cấp Quyền",
    html: `
      <div class="text-start mb-2 fw-bold text-primary">1. Hồ sơ nhân sự</div>
      <input id="swal-ma" class="form-control mb-2" placeholder="Mã NV (VD: NV001)">
      <input id="swal-ten" class="form-control mb-2" placeholder="Tên nhân viên">
      <input id="swal-sdt" class="form-control mb-4" placeholder="Số điện thoại">
      
      <div class="text-start mb-2 fw-bold text-danger">2. Cấp quyền phần mềm</div>
      <select id="swal-vaitro" class="form-select mb-2">
        <option value="">-- Chỉ tạo hồ sơ, KHÔNG cấp quyền --</option>
        <option value="ThuNgan">Thu ngân (Tính tiền, In bill)</option>
        <option value="Order">Nhân viên Order (Ghi món, Phục vụ)</option>
        <option value="Bep">Bếp (Xem màn hình làm món)</option>
      </select>
      <input id="swal-user" class="form-control mb-2" placeholder="Tên đăng nhập (Bỏ trống nếu không cấp)">
      <input id="swal-pass" class="form-control" type="password" placeholder="Mật khẩu (Bỏ trống nếu không cấp)">
    `,
    showCancelButton: true,
    confirmButtonText: "Lưu Hệ Thống",
    preConfirm: () => {
      const ma = document.getElementById("swal-ma").value;
      const ten = document.getElementById("swal-ten").value;
      const sdt = document.getElementById("swal-sdt").value;

      const vaitro = document.getElementById("swal-vaitro").value;
      const user = document.getElementById("swal-user").value;
      const pass = document.getElementById("swal-pass").value;

      if (!ma || !ten || !sdt) {
        swal.showValidationMessage("Vui lòng nhập đủ Hồ sơ nhân sự!");
        return false;
      }
      if (vaitro !== "" && (!user || !pass)) {
        swal.showValidationMessage(
          "Đã cấp quyền thì phải nhập Tên đăng nhập và Mật khẩu!",
        );
        return false;
      }

      return {
        chiNhanhId: globalState.value.activeBranchId,
        maNhanVien: ma,
        tenNhanVien: ten,
        soDienThoai: sdt,
        taoTaiKhoan: vaitro !== "",
        vaiTro: vaitro,
        tenDangNhap: user,
        matKhau: pass,
      };
    },
  });

  if (formValues) {
    try {
      await axios.post("/api/NhanVien", formValues);
      swal.fire({
        icon: "success",
        title: "Hoàn tất",
        timer: 1500,
        showConfirmButton: false,
      });
      fetchEmployees();
    } catch (e) {
      swal.fire(
        "Lỗi Server",
        e.response?.data?.message || "Không thể lưu",
        "error",
      );
    }
  }
};

const handleEditEmployee = async (emp) => {
  const { value: formValues } = await swal.fire({
    title: "Sửa Thông Tin Nhân Viên",
    html: `
      <input id="swal-ma-edit" class="form-control mb-3" value="${emp.maNhanVien}">
      <input id="swal-ten-edit" class="form-control mb-3" value="${emp.tenNhanVien}">
      <input id="swal-sdt-edit" class="form-control" value="${emp.soDienThoai}">
    `,
    showCancelButton: true,
    confirmButtonText: "Cập nhật",
    preConfirm: () => {
      const ma = document.getElementById("swal-ma-edit").value;
      const ten = document.getElementById("swal-ten-edit").value;
      const sdt = document.getElementById("swal-sdt-edit").value;
      if (!ma || !ten || !sdt) {
        swal.showValidationMessage("Vui lòng nhập đủ thông tin!");
        return false;
      }
      return {
        chiNhanhId: globalState.value.activeBranchId,
        maNhanVien: ma,
        tenNhanVien: ten,
        soDienThoai: sdt,
      };
    },
  });

  if (formValues) {
    try {
      await axios.put(`/api/NhanVien/${emp.id}`, formValues);
      swal.fire({
        icon: "success",
        title: "Đã cập nhật",
        timer: 1000,
        showConfirmButton: false,
      });
      fetchEmployees();
    } catch (e) {
      swal.fire("Lỗi", "Không thể sửa nhân viên", "error");
    }
  }
};

const handleDeleteEmployee = (id) => {
  swal
    .fire({
      title: "Xóa nhân viên này?",
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#d33",
      confirmButtonText: "Xóa",
    })
    .then(async (result) => {
      if (result.isConfirmed) {
        try {
          await axios.delete(`/api/NhanVien/${id}`);
          fetchEmployees();
        } catch (e) {
          swal.fire("Lỗi", "Không thể xóa", "error");
        }
      }
    });
};
</script>

<template>
  <div class="container-fluid px-4 py-4">
    <div class="card border-0 shadow-sm rounded-3">
      <div
        class="card-header bg-white border-bottom py-3 d-flex justify-content-between align-items-center"
      >
        <h5 class="fw-bold text-dark mb-0">
          <i class="bi bi-people-fill text-primary me-2"></i> DANH SÁCH NHÂN
          VIÊN
        </h5>
        <button
          @click="handleAddEmployee"
          class="btn btn-primary btn-sm fw-bold px-4 rounded-pill shadow-sm"
        >
          <i class="bi bi-person-plus-fill me-1"></i> THÊM MỚI
        </button>
      </div>

      <div class="card-body p-0 table-responsive">
        <table class="table table-hover align-middle mb-0">
          <thead class="table-light text-muted small text-uppercase">
            <tr>
              <th class="ps-4" style="width: 50px">#</th>
              <th>Mã NV</th>
              <th>Tên nhân viên</th>
              <th>Số điện thoại</th>
              <th>Tên đăng nhập</th>
              <th>Vai trò phần mềm</th>
              <th class="text-center" style="width: 120px">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(emp, index) in employees" :key="emp.id">
              <td class="ps-4 fw-bold text-muted">{{ index + 1 }}</td>
              <td class="fw-bold text-primary">{{ emp.maNhanVien }}</td>
              <td class="fw-bold text-dark">{{ emp.tenNhanVien }}</td>
              <td>{{ emp.soDienThoai }}</td>

              <td class="fw-bold text-secondary">
                {{ emp.tenDangNhap || "---" }}
              </td>

              <td>
                <span
                  v-if="emp.vaiTro === 'ThuNgan'"
                  class="badge bg-success rounded-pill fw-normal px-3 py-2"
                  ><i class="bi bi-cash-coin me-1"></i> Thu Ngân</span
                >
                <span
                  v-else-if="emp.vaiTro === 'Order'"
                  class="badge bg-warning text-dark rounded-pill fw-normal px-3 py-2"
                  ><i class="bi bi-journal-text me-1"></i> Order</span
                >
                <span
                  v-else-if="emp.vaiTro === 'Bep'"
                  class="badge bg-danger rounded-pill fw-normal px-3 py-2"
                  ><i class="bi bi-fire me-1"></i> Bếp</span
                >
                <span v-else class="text-muted small fst-italic"
                  >Không có quyền</span
                >
              </td>

              <td class="text-center">
                <button
                  @click="handleEditEmployee(emp)"
                  class="btn btn-sm btn-light text-primary me-1"
                >
                  <i class="bi bi-pencil-square"></i>
                </button>
                <button
                  @click="handleDeleteEmployee(emp.id)"
                  class="btn btn-sm btn-light text-danger"
                >
                  <i class="bi bi-trash"></i>
                </button>
              </td>
            </tr>
            <tr v-if="employees.length === 0">
              <td colspan="7" class="text-center py-5 text-muted">
                <i class="bi bi-inbox fs-1 d-block mb-2"></i> Chi nhánh này chưa
                có nhân viên nào.
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
