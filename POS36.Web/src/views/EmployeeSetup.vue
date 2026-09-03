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
      <div class="container-fluid text-start px-0">
        <div class="row">
          <div class="col-md-6 border-end pe-3">
            <div class="mb-3 fw-bold text-primary">1. Hồ sơ nhân sự & Pháp lý <span class="text-danger">*</span></div>
            <label class="form-label small mb-1">Họ và tên <span class="text-danger">*</span></label>
            <input id="swal-ten" class="form-control mb-2" placeholder="Họ và tên">
            <label class="form-label small mb-1">Số CCCD / Ngày cấp <span class="text-danger">*</span></label>
            <div class="input-group mb-2">
              <input id="swal-cccd" class="form-control" placeholder="Số CCCD (12 số)">
              <input id="swal-ngaycap-cccd" type="date" class="form-control" title="Ngày cấp CCCD">
            </div>
            <input id="swal-noicap-cccd" class="form-control mb-2" placeholder="Nơi cấp CCCD">
            <div class="row mb-2">
              <div class="col-6">
                <label class="form-label small mb-1">Ngày sinh <span class="text-danger">*</span></label>
                <input id="swal-ngaysinh" type="date" class="form-control">
              </div>
              <div class="col-6">
                <label class="form-label small mb-1">Giới tính <span class="text-danger">*</span></label>
                <select id="swal-gioitinh" class="form-select">
                  <option value="Nam">Nam</option>
                  <option value="Nữ">Nữ</option>
                  <option value="Khác">Khác</option>
                </select>
              </div>
            </div>
            <label class="form-label small mb-1">Địa chỉ thường trú <span class="text-danger">*</span></label>
            <input id="swal-thuongtru" class="form-control mb-2" placeholder="Theo CCCD/Sổ hộ khẩu">
            <label class="form-label small mb-1">Địa chỉ tạm trú (Lưu trú) <span class="text-danger">*</span></label>
            <input id="swal-tamtru" class="form-control mb-3" placeholder="Chỗ ở hiện tại">
          </div>
          <div class="col-md-6 ps-3">
            <div class="mb-3 fw-bold text-info">2. Thông tin liên hệ & Công việc</div>
            <label class="form-label small mb-1">Số điện thoại & Email</label>
            <input id="swal-sdt" class="form-control mb-2" placeholder="Số điện thoại *">
            <input id="swal-email" type="email" class="form-control mb-2" placeholder="Email (Không bắt buộc)">
            <label class="form-label small mb-1">Ngày vào làm</label>
            <input id="swal-ngayvaolam" type="date" class="form-control mb-3">

            <div class="mb-2 fw-bold text-warning border-top pt-3"><i class="bi bi-telephone-forward me-1"></i>Liên hệ khẩn cấp</div>
            <input id="swal-nguoilienhe" class="form-control mb-2" placeholder="Họ tên người liên hệ">
            <div class="input-group mb-2">
              <input id="swal-sdtkhancap" class="form-control" placeholder="SĐT khẩn cấp">
              <select id="swal-moiquanhe" class="form-select" style="max-width:130px">
                <option value="">Quan hệ</option>
                <option value="Cha/Mẹ">Cha/Mẹ</option>
                <option value="Vợ/Chồng">Vợ/Chồng</option>
                <option value="Anh/Chị/Em">Anh/Chị/Em</option>
                <option value="Bạn bè">Bạn bè</option>
                <option value="Khác">Khác</option>
              </select>
            </div>

            <div class="mb-3 fw-bold text-danger border-top pt-3">3. Cấp quyền phần mềm <span class="text-danger">*</span></div>
            <select id="swal-vaitro" class="form-select mb-2">
              <option value="">-- Chọn vai trò --</option>
              <option value="ThuNgan">Thu ngân (Tính tiền, In bill)</option>
              <option value="Order">Nhân viên Order (Ghi món, Phục vụ)</option>
              <option value="Bep">Bếp (Xem màn hình làm món)</option>
            </select>
            <input id="swal-user" class="form-control mb-2" placeholder="Tên đăng nhập *">
            <input id="swal-pass" class="form-control mb-3" type="password" placeholder="Mật khẩu *">

            <div class="border-top pt-3">
              <div class="form-check">
                <input class="form-check-input" type="checkbox" id="swal-consent">
                <label class="form-check-label small text-muted" for="swal-consent">
                  <i class="bi bi-shield-check text-success me-1"></i>
                  Nhân viên đồng ý cho phép thu thập và xử lý dữ liệu cá nhân phục vụ mục đích quản lý nhân sự theo <strong>Nghị định 13/2023/NĐ-CP</strong>. <span class="text-danger">*</span>
                </label>
              </div>
            </div>
          </div>
        </div>
      </div>
    `,
    width: 800,
    showCancelButton: true,
    confirmButtonText: "Lưu Hệ Thống",
    preConfirm: async () => {
      const ten = document.getElementById("swal-ten").value.trim();
      const email = document.getElementById("swal-email").value.trim();
      const sdt = document.getElementById("swal-sdt").value.trim();
      const cccd = document.getElementById("swal-cccd").value.trim();
      const ngaycap = document.getElementById("swal-ngaycap-cccd").value;
      const noicap = document.getElementById("swal-noicap-cccd").value.trim();
      const ngaysinh = document.getElementById("swal-ngaysinh").value;
      const gioitinh = document.getElementById("swal-gioitinh").value;
      const thuongtru = document.getElementById("swal-thuongtru").value.trim();
      const tamtru = document.getElementById("swal-tamtru").value.trim();
      const ngayvaolam = document.getElementById("swal-ngayvaolam").value;
      const nguoilienhe = document.getElementById("swal-nguoilienhe").value.trim();
      const sdtkhancap = document.getElementById("swal-sdtkhancap").value.trim();
      const moiquanhe = document.getElementById("swal-moiquanhe").value;
      const consent = document.getElementById("swal-consent").checked;
      const vaitro = document.getElementById("swal-vaitro").value;
      const user = document.getElementById("swal-user").value.trim();
      const pass = document.getElementById("swal-pass").value;

      if (!ten || !sdt || !cccd || !ngaysinh || !gioitinh || !thuongtru || !tamtru) {
        swal.showValidationMessage("Vui lòng nhập đầy đủ các thông tin bắt buộc (*)");
        return false;
      }
      if (!consent) {
        swal.showValidationMessage("Nhân viên phải đồng ý cho phép xử lý dữ liệu cá nhân trước khi lưu hồ sơ!");
        return false;
      }
      if (!vaitro) {
        swal.showValidationMessage("Vui lòng chọn Vai trò cho nhân viên!");
        return false;
      }
      if (!user || !pass) {
        swal.showValidationMessage("Vui lòng nhập Tên đăng nhập và Mật khẩu!");
        return false;
      }

      const payload = {
        chiNhanhId: globalState.value.activeBranchId,
        maNhanVien: "", // Backend sẽ tự sinh
        tenNhanVien: ten,
        email: email || null,
        soDienThoai: sdt,
        cccd: cccd,
        ngayCapCccd: ngaycap || null,
        noiCapCccd: noicap,
        ngaySinh: ngaysinh,
        gioiTinh: gioitinh,
        diaChiThuongTru: thuongtru,
        diaChiTamTru: tamtru,
        ngayVaoLam: ngayvaolam || null,
        nguoiLienHeKhanCap: nguoilienhe || null,
        sdtKhanCap: sdtkhancap || null,
        moiQuanHeKhanCap: moiquanhe || null,
        dongYXuLyDuLieu: consent,
        taoTaiKhoan: true,
        vaiTro: vaitro,
        tenDangNhap: user,
        matKhau: pass,
      };

      try {
        await axios.post("/api/NhanVien", payload);
        return true;
      } catch (e) {
        swal.showValidationMessage(e.response?.data?.message || "Không thể lưu nhân viên");
        return false; // Giữ modal mở
      }
    },
  });

  if (formValues) {
    swal.fire({
      icon: "success",
      title: "Hoàn tất",
      text: "Đã thêm nhân viên và cấp quyền thành công!",
      timer: 1800,
      showConfirmButton: false,
    });
    fetchEmployees();
  }
};

const handleEditEmployee = async (emp) => {
  const { value: formValues } = await swal.fire({
    title: "Sửa Thông Tin Nhân Viên",
    html: `
      <div class="container-fluid text-start px-0">
        <div class="row">
          <div class="col-md-6 border-end pe-3">
            <div class="mb-3 fw-bold text-primary">1. Hồ sơ nhân sự & Pháp lý <span class="text-danger">*</span></div>
            <label class="form-label small mb-1">Mã NV & Tên <span class="text-danger">*</span></label>
            <div class="input-group mb-2">
              <input class="form-control bg-light" value="${emp.maNhanVien}" disabled>
              <input id="swal-ten-edit" class="form-control w-50" value="${emp.tenNhanVien}" placeholder="Họ và tên">
            </div>
            <label class="form-label small mb-1">Số CCCD / Ngày cấp <span class="text-danger">*</span></label>
            <div class="input-group mb-2">
              <input id="swal-cccd-edit" class="form-control" value="${emp.cccd || ''}" placeholder="Số CCCD (12 số)">
              <input id="swal-ngaycap-cccd-edit" type="date" class="form-control" value="${emp.ngayCapCccd ? emp.ngayCapCccd.split('T')[0] : ''}" title="Ngày cấp CCCD">
            </div>
            <input id="swal-noicap-cccd-edit" class="form-control mb-2" value="${emp.noiCapCccd || ''}" placeholder="Nơi cấp CCCD">
            <div class="row mb-2">
              <div class="col-6">
                <label class="form-label small mb-1">Ngày sinh <span class="text-danger">*</span></label>
                <input id="swal-ngaysinh-edit" type="date" class="form-control" value="${emp.ngaySinh ? emp.ngaySinh.split('T')[0] : ''}">
              </div>
              <div class="col-6">
                <label class="form-label small mb-1">Giới tính <span class="text-danger">*</span></label>
                <select id="swal-gioitinh-edit" class="form-select">
                  <option value="Nam" ${emp.gioiTinh === 'Nam' ? 'selected' : ''}>Nam</option>
                  <option value="Nữ" ${emp.gioiTinh === 'Nữ' ? 'selected' : ''}>Nữ</option>
                  <option value="Khác" ${emp.gioiTinh === 'Khác' ? 'selected' : ''}>Khác</option>
                </select>
              </div>
            </div>
            <label class="form-label small mb-1">Địa chỉ thường trú <span class="text-danger">*</span></label>
            <input id="swal-thuongtru-edit" class="form-control mb-2" value="${emp.diaChiThuongTru || ''}" placeholder="Theo CCCD/Sổ hộ khẩu">
            <label class="form-label small mb-1">Địa chỉ tạm trú (Lưu trú) <span class="text-danger">*</span></label>
            <input id="swal-tamtru-edit" class="form-control mb-3" value="${emp.diaChiTamTru || ''}" placeholder="Chỗ ở hiện tại">
          </div>
          <div class="col-md-6 ps-3">
            <div class="mb-3 fw-bold text-info">2. Thông tin liên hệ & Công việc</div>
            <label class="form-label small mb-1">Số điện thoại & Email</label>
            <input id="swal-sdt-edit" class="form-control mb-2" value="${emp.soDienThoai}" placeholder="Số điện thoại *">
            <input id="swal-email-edit" type="email" class="form-control mb-2" value="${emp.email || ''}" placeholder="Email (Không bắt buộc)">
            <label class="form-label small mb-1">Ngày vào làm</label>
            <input id="swal-ngayvaolam-edit" type="date" class="form-control mb-3" value="${emp.ngayVaoLam ? emp.ngayVaoLam.split('T')[0] : ''}">

            <div class="mb-2 fw-bold text-warning border-top pt-3"><i class="bi bi-telephone-forward me-1"></i>Liên hệ khẩn cấp</div>
            <input id="swal-nguoilienhe-edit" class="form-control mb-2" value="${emp.nguoiLienHeKhanCap || ''}" placeholder="Họ tên người liên hệ">
            <div class="input-group mb-2">
              <input id="swal-sdtkhancap-edit" class="form-control" value="${emp.sdtKhanCap || ''}" placeholder="SĐT khẩn cấp">
              <select id="swal-moiquanhe-edit" class="form-select" style="max-width:130px">
                <option value="" ${!emp.moiQuanHeKhanCap ? 'selected' : ''}>Quan hệ</option>
                <option value="Cha/Mẹ" ${emp.moiQuanHeKhanCap === 'Cha/Mẹ' ? 'selected' : ''}>Cha/Mẹ</option>
                <option value="Vợ/Chồng" ${emp.moiQuanHeKhanCap === 'Vợ/Chồng' ? 'selected' : ''}>Vợ/Chồng</option>
                <option value="Anh/Chị/Em" ${emp.moiQuanHeKhanCap === 'Anh/Chị/Em' ? 'selected' : ''}>Anh/Chị/Em</option>
                <option value="Bạn bè" ${emp.moiQuanHeKhanCap === 'Bạn bè' ? 'selected' : ''}>Bạn bè</option>
                <option value="Khác" ${emp.moiQuanHeKhanCap === 'Khác' ? 'selected' : ''}>Khác</option>
              </select>
            </div>
          </div>
        </div>
      </div>
    `,
    width: 800,
    showCancelButton: true,
    confirmButtonText: "Cập nhật",
    preConfirm: async () => {
      const ten = document.getElementById("swal-ten-edit").value.trim();
      const email = document.getElementById("swal-email-edit").value.trim();
      const sdt = document.getElementById("swal-sdt-edit").value.trim();
      const cccd = document.getElementById("swal-cccd-edit").value.trim();
      const ngaycap = document.getElementById("swal-ngaycap-cccd-edit").value;
      const noicap = document.getElementById("swal-noicap-cccd-edit").value.trim();
      const ngaysinh = document.getElementById("swal-ngaysinh-edit").value;
      const gioitinh = document.getElementById("swal-gioitinh-edit").value;
      const thuongtru = document.getElementById("swal-thuongtru-edit").value.trim();
      const tamtru = document.getElementById("swal-tamtru-edit").value.trim();
      const ngayvaolam = document.getElementById("swal-ngayvaolam-edit").value;
      const nguoilienhe = document.getElementById("swal-nguoilienhe-edit").value.trim();
      const sdtkhancap = document.getElementById("swal-sdtkhancap-edit").value.trim();
      const moiquanhe = document.getElementById("swal-moiquanhe-edit").value;

      if (!ten || !sdt || !cccd || !ngaysinh || !gioitinh || !thuongtru || !tamtru) {
        swal.showValidationMessage("Vui lòng nhập đầy đủ các thông tin bắt buộc (*)");
        return false;
      }
      const payload = {
        chiNhanhId: globalState.value.activeBranchId,
        maNhanVien: emp.maNhanVien,
        tenNhanVien: ten,
        email: email || null,
        soDienThoai: sdt,
        cccd: cccd,
        ngayCapCccd: ngaycap || null,
        noiCapCccd: noicap,
        ngaySinh: ngaysinh,
        gioiTinh: gioitinh,
        diaChiThuongTru: thuongtru,
        diaChiTamTru: tamtru,
        ngayVaoLam: ngayvaolam || null,
        nguoiLienHeKhanCap: nguoilienhe || null,
        sdtKhanCap: sdtkhancap || null,
        moiQuanHeKhanCap: moiquanhe || null,
      };

      try {
        await axios.put(`/api/NhanVien/${emp.id}`, payload);
        return true;
      } catch (e) {
        swal.showValidationMessage(e.response?.data?.message || "Không thể sửa nhân viên");
        return false; // Giữ modal mở
      }
    },
  });

  if (formValues) {
    swal.fire({
      icon: "success",
      title: "Đã cập nhật",
      timer: 1000,
      showConfirmButton: false,
    });
    fetchEmployees();
  }
};

const handleToggleActive = async (emp) => {
  const isCurrentlyActive = emp.isActive !== false;
  const actionText = isCurrentlyActive ? "Khóa tài khoản" : "Mở khóa tài khoản";
  const confirmResult = await swal.fire({
    title: `${actionText} nhân viên này?`,
    text: isCurrentlyActive 
      ? "Tài khoản nhân viên này sẽ bị vô hiệu hóa ngay lập tức và không thể đăng nhập!" 
      : "Kích hoạt lại tài khoản cho nhân viên này đăng nhập hệ thống.",
    icon: "warning",
    showCancelButton: true,
    confirmButtonText: "Đồng ý",
    confirmButtonColor: isCurrentlyActive ? "#dc3545" : "#28a745"
  });

  if (confirmResult.isConfirmed) {
    try {
      const res = await axios.put(`/api/NhanVien/${emp.id}/toggle-active`);
      swal.fire({
        toast: true,
        position: "top-end",
        icon: "success",
        title: res.data.message || "Thành công!",
        timer: 1500,
        showConfirmButton: false,
      });
      fetchEmployees();
    } catch (e) {
      swal.fire("Lỗi", e.response?.data?.message || "Không thể thay đổi trạng thái", "error");
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
              <th>Pháp lý</th>
              <th>Liên hệ</th>
              <th>Tên đăng nhập</th>
              <th>Vai trò phần mềm</th>
              <th class="text-center" style="width: 130px">Trạng thái</th>
              <th class="text-center" style="width: 140px">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(emp, index) in employees" :key="emp.id">
              <td class="ps-4 fw-bold text-muted">{{ index + 1 }}</td>
              <td class="fw-bold text-primary">{{ emp.maNhanVien }}</td>
              <td class="fw-bold text-dark">
                {{ emp.tenNhanVien }}
                <div class="small fw-normal text-muted mt-1"><i class="bi bi-gender-ambiguous"></i> {{ emp.gioiTinh || '---' }}</div>
              </td>
              <td class="text-secondary small">
                <div>CCCD: <span class="fw-bold text-dark">{{ emp.cccd || "---" }}</span></div>
                <div>NS: {{ emp.ngaySinh ? new Date(emp.ngaySinh).toLocaleDateString('vi-VN') : '---' }}</div>
              </td>
              <td class="text-secondary small">
                <div><i class="bi bi-telephone text-primary"></i> <span class="fw-bold text-dark">{{ emp.soDienThoai }}</span></div>
                <div><i class="bi bi-envelope text-primary"></i> {{ emp.email || "---" }}</div>
                <div v-if="emp.nguoiLienHeKhanCap" class="mt-1 text-warning" style="font-size:0.75rem">
                  <i class="bi bi-telephone-forward"></i> {{ emp.nguoiLienHeKhanCap }} ({{ emp.moiQuanHeKhanCap || '---' }}) — {{ emp.sdtKhanCap }}
                </div>
              </td>

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
                <!-- Link phân quyền nhỏ gọn chỉ hiện với Thu ngân -->
                <div v-if="emp.vaiTro === 'ThuNgan'" class="mt-1">
                  <router-link
                    to="/admin/thiet-lap"
                    class="text-info small fw-semibold text-decoration-none"
                    style="font-size:0.72rem; letter-spacing:0.01em;"
                  >
                    <i class="bi bi-shield-lock me-1"></i>Thiết lập quyền →
                  </router-link>
                </div>
              </td>

              <td class="text-center">
                <span
                  v-if="emp.isActive !== false"
                  class="badge bg-success bg-opacity-10 text-success rounded-pill fw-semibold px-3 py-1.5"
                  ><i class="bi bi-patch-check-fill me-1"></i>Hoạt động</span
                >
                <span
                  v-else
                  class="badge bg-danger bg-opacity-10 text-danger rounded-pill fw-semibold px-3 py-1.5"
                  ><i class="bi bi-shield-slash-fill me-1"></i>Bị khóa</span
                >
              </td>

              <td class="text-center">
                <button
                  @click="handleToggleActive(emp)"
                  class="btn btn-sm btn-light me-1"
                  :class="emp.isActive !== false ? 'text-warning' : 'text-success'"
                  :title="emp.isActive !== false ? 'Khóa tài khoản' : 'Mở khóa tài khoản'"
                >
                  <i class="bi" :class="emp.isActive !== false ? 'bi-lock-fill' : 'bi-unlock-fill'"></i>
                </button>
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
              <td colspan="9" class="text-center py-5 text-muted">
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
