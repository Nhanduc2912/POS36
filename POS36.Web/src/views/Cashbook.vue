<template>
  <div class="cashbook-container d-flex bg-white h-100 flex-column">
    <div class="summary-cards bg-light border-bottom p-3 d-flex gap-3">
      <div class="card flex-fill border-0 shadow-sm rounded-0">
        <div class="card-body d-flex align-items-center p-3">
          <div
            class="icon-box bg-danger bg-opacity-10 text-danger rounded-circle d-flex justify-content-center align-items-center me-3"
            style="width: 50px; height: 50px"
          >
            <i class="bi bi-bank fs-3"></i>
          </div>
          <div>
            <h4 class="mb-0 fw-bold text-danger">
              {{ formatPrice(summary.dauKy) }}
            </h4>
            <small
              class="text-muted fw-bold text-uppercase"
              style="font-size: 0.7rem"
              >Quỹ đầu kỳ</small
            >
          </div>
        </div>
      </div>
      <div class="card flex-fill border-0 shadow-sm rounded-0">
        <div class="card-body d-flex align-items-center p-3">
          <div
            class="icon-box bg-primary text-white rounded-circle d-flex justify-content-center align-items-center me-3"
            style="width: 50px; height: 50px"
          >
            <i class="bi bi-arrow-down-right fs-3"></i>
          </div>
          <div>
            <h4 class="mb-0 fw-bold text-primary">
              {{ formatPrice(summary.tongThu) }}
            </h4>
            <small
              class="text-muted fw-bold text-uppercase"
              style="font-size: 0.7rem"
              >Tổng thu</small
            >
          </div>
        </div>
      </div>
      <div class="card flex-fill border-0 shadow-sm rounded-0">
        <div class="card-body d-flex align-items-center p-3">
          <div
            class="icon-box bg-success text-white rounded-circle d-flex justify-content-center align-items-center me-3"
            style="width: 50px; height: 50px"
          >
            <i class="bi bi-arrow-up-left fs-3"></i>
          </div>
          <div>
            <h4 class="mb-0 fw-bold text-success">
              {{ formatPrice(summary.tongChi) }}
            </h4>
            <small
              class="text-muted fw-bold text-uppercase"
              style="font-size: 0.7rem"
              >Tổng chi</small
            >
          </div>
        </div>
      </div>
      <div class="card flex-fill border-0 shadow-sm rounded-0">
        <div class="card-body d-flex align-items-center p-3">
          <div
            class="icon-box bg-warning text-white rounded-circle d-flex justify-content-center align-items-center me-3"
            style="width: 50px; height: 50px"
          >
            <i class="bi bi-currency-dollar fs-3"></i>
          </div>
          <div>
            <h4 class="mb-0 fw-bold text-warning">
              {{ formatPrice(summary.tonQuy) }}
            </h4>
            <small
              class="text-muted fw-bold text-uppercase"
              style="font-size: 0.7rem"
              >Tồn quỹ</small
            >
          </div>
        </div>
      </div>
    </div>

    <div class="d-flex flex-grow-1 overflow-hidden">
      <div
        class="sidebar-filter border-end bg-light p-0 d-flex flex-column overflow-auto"
        style="width: 260px"
      >
        <div class="filter-group border-bottom">
          <div class="filter-header bg-light text-danger fw-bold p-2 small">
            <i class="bi bi-list-ul me-1"></i> TÌM KIẾM
          </div>
          <div class="p-2">
            <input
              v-model="searchMa"
              type="text"
              class="form-control form-control-sm mb-2 shadow-none"
              placeholder="Mã chứng từ"
            />
            <input
              v-model="searchDoiTac"
              type="text"
              class="form-control form-control-sm mb-2 shadow-none"
              placeholder="Tìm đối tác"
            />
          </div>
        </div>

        <div class="filter-group border-bottom">
          <div
            class="filter-header bg-light text-danger fw-bold p-2 small d-flex justify-content-between align-items-center"
          >
            <span><i class="bi bi-wallet2 me-1"></i> TÀI KHOẢN</span>
            <div>
              <i
                class="bi bi-pencil-square text-warning me-1 cursor-pointer"
              ></i
              ><i class="bi bi-plus-square text-warning cursor-pointer"></i>
            </div>
          </div>
          <div class="p-0">
            <div
              class="list-group list-group-flush small fw-bold text-secondary"
            >
              <button
                @click="filterTaiKhoan = ''"
                :class="{'active bg-light text-primary': filterTaiKhoan === ''}"
                class="list-group-item list-group-item-action border-0"
              >
                Tất cả
              </button>
              <button 
                @click="filterTaiKhoan = 'Tiền mặt'"
                :class="{'active bg-light text-primary': filterTaiKhoan === 'Tiền mặt'}"
                class="list-group-item list-group-item-action border-0"
              >
                TIỀN MẶT
              </button>
              <button 
                @click="filterTaiKhoan = 'Chuyển khoản'"
                :class="{'active bg-light text-primary': filterTaiKhoan === 'Chuyển khoản'}"
                class="list-group-item list-group-item-action border-0"
              >
                CHUYỂN KHOẢN
              </button>
              <button 
                @click="filterTaiKhoan = 'VNPAY-QR'"
                :class="{'active bg-light text-primary': filterTaiKhoan === 'VNPAY-QR'}"
                class="list-group-item list-group-item-action border-0"
              >
                VNPAY-QR
              </button>
            </div>
          </div>
        </div>

        <div class="filter-group border-bottom">
          <div class="filter-header bg-light text-danger fw-bold p-2 small">
            <i class="bi bi-funnel me-1"></i> PHƯƠNG THỨC
          </div>
          <div class="p-2">
            <div class="form-check mb-1">
              <input
                v-model="filterLoaiPhieu"
                value=""
                class="form-check-input"
                type="radio"
                name="pt"
              /><label class="form-check-label small">Tất cả</label>
            </div>
            <div class="form-check mb-1">
              <input 
                v-model="filterLoaiPhieu"
                value="Thu"
                class="form-check-input" type="radio" name="pt" /><label
                class="form-check-label small"
                >Phiếu thu</label
              >
            </div>
            <div class="form-check mb-1">
              <input 
                v-model="filterLoaiPhieu"
                value="Chi"
                class="form-check-input" type="radio" name="pt" /><label
                class="form-check-label small"
                >Phiếu chi</label
              >
            </div>
          </div>
        </div>
      </div>

      <div class="main-content flex-grow-1 p-3 bg-white overflow-auto">
        <div class="d-flex justify-content-end mb-2 gap-2">
          <button @click="openTransactionModal('Chuyển khoản')" class="btn btn-warning text-white fw-bold btn-sm px-3">
            <i class="bi bi-arrow-left-right me-1"></i> CHUYỂN KHOẢN
          </button>
          <button @click="openTransactionModal('Thu')" class="btn btn-warning text-white fw-bold btn-sm px-3">
            <i class="bi bi-plus-circle me-1"></i> PHIẾU THU
          </button>
          <button @click="openTransactionModal('Chi')" class="btn btn-warning text-white fw-bold btn-sm px-3">
            <i class="bi bi-dash-circle me-1"></i> PHIẾU CHI
          </button>
        </div>

        <div class="table-responsive">
          <table
            class="table table-hover table-bordered align-middle"
            style="font-size: 0.85rem"
          >
            <thead class="table-light text-muted align-middle">
              <tr>
                <th class="text-center" style="width: 40px"></th>
                <th>Mã chứng từ</th>
                <th>Người nộp/nhận</th>
                <th>Hạng mục Thu/Chi</th>
                <th>Lý do</th>
                <th class="text-center">Ngày giao dịch</th>
                <th class="text-end">Giá trị</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="loading" class="text-center">
                <td colspan="7" class="py-4">
                  <div
                    class="spinner-border text-danger spinner-border-sm"
                  ></div>
                </td>
              </tr>
              <tr v-else-if="filteredTransactions.length === 0" class="text-center">
                <td colspan="7" class="py-4 text-muted">
                  Không tìm thấy giao dịch thu chi nào.
                </td>
              </tr>

              <template v-else v-for="t in filteredTransactions" :key="t.id">
                <tr class="cursor-pointer" @click="toggleDetail(t.id)">
                  <td class="text-center text-muted">
                    <i
                      class="bi"
                      :class="
                        expandedRowId === t.id
                          ? 'bi-caret-down-fill text-danger'
                          : 'bi-caret-right-fill'
                      "
                    ></i>
                  </td>
                  <td>
                    <div class="fw-bold text-dark">{{ t.maChungTu }}</div>
                    <div
                      class="small fw-bold"
                      :class="
                        t.loaiPhieu === 'Thu' ? 'text-danger' : 'text-success'
                      "
                    >
                      {{ t.phuongThuc }}
                    </div>
                  </td>
                  <td>{{ t.nguoiNopNhan || "---" }}</td>
                  <td>{{ t.hangMuc }}</td>
                  <td class="text-truncate" style="max-width: 250px">
                    {{ t.lyDo }}
                  </td>
                  <td class="text-center">{{ formatDate(t.ngayGiaoDich) }}</td>
                  <td class="text-end fw-bold text-dark">
                    {{ formatPrice(t.giaTri) }}
                  </td>
                </tr>

                <tr v-if="expandedRowId === t.id" class="bg-light detail-row">
                  <td colspan="7" class="p-0">
                    <div class="p-3 border-start border-danger border-4">
                      <ul
                        class="nav nav-tabs mb-3"
                        style="border-bottom: 2px solid #dc3545"
                      >
                        <li class="nav-item">
                          <a
                            class="nav-link active fw-bold text-danger border-danger border-bottom-0 py-1"
                            >Chi tiết</a
                          >
                        </li>
                      </ul>
                      <div class="row g-3 bg-white p-3 border mb-2">
                        <div class="col-md-6">
                          <div class="d-flex mb-2">
                            <div class="fw-bold w-25">Mã chứng từ</div>
                            <div>{{ t.maChungTu }}</div>
                          </div>
                          <div class="d-flex mb-2">
                            <div class="fw-bold w-25">Người nộp/nhận</div>
                            <div>{{ t.nguoiNopNhan || "---" }}</div>
                          </div>
                          <div class="d-flex mb-2 align-items-center">
                            <div class="fw-bold w-25">Giá trị</div>
                            <div>
                              <span class="badge bg-primary px-3 py-2 fs-6">{{
                                formatPrice(t.giaTri)
                              }}</span>
                            </div>
                          </div>
                          <div class="d-flex mb-2">
                            <div class="fw-bold w-25">Người sửa</div>
                            <div></div>
                          </div>
                          <div class="d-flex mb-2">
                            <div class="fw-bold w-25">Lý do</div>
                            <div class="w-75">{{ t.lyDo }}</div>
                          </div>
                        </div>
                        <div class="col-md-6">
                          <div class="d-flex mb-2">
                            <div class="fw-bold w-25">Phương thức</div>
                            <div>Phiếu {{ t.loaiPhieu.toLowerCase() }}</div>
                          </div>
                          <div class="d-flex mb-2">
                            <div class="fw-bold w-25">Ngày giao dịch</div>
                            <div>{{ formatDate(t.ngayGiaoDich) }}</div>
                          </div>
                          <div class="d-flex mb-2">
                            <div class="fw-bold w-25">Người tạo</div>
                            <div>{{ t.nguoiTao }}</div>
                          </div>
                          <div class="d-flex mb-2">
                            <div class="fw-bold w-25">Ngày sửa</div>
                            <div></div>
                          </div>
                          <div class="d-flex mb-2">
                            <div class="fw-bold w-25">Hạng mục Thu/Chi</div>
                            <div>{{ t.hangMuc }}</div>
                          </div>
                        </div>
                      </div>
                      <div class="d-flex justify-content-end">
                        <button class="btn btn-warning text-white fw-bold">
                          <i class="bi bi-printer"></i> In
                        </button>
                      </div>
                    </div>
                  </td>
                </tr>
              </template>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch, computed, inject } from "vue";
import axios from "axios";
import { globalState } from "../store";

const swal = inject("$swal");
const transactions = ref([]);
const summary = ref({ dauKy: 0, tongThu: 0, tongChi: 0, tonQuy: 0 });
const loading = ref(false);
const expandedRowId = ref(null);

const searchMa = ref("");
const searchDoiTac = ref("");
const filterTaiKhoan = ref("");
const filterLoaiPhieu = ref("");

const filteredTransactions = computed(() => {
  return transactions.value.filter((t) => {
    if (searchMa.value && !t.maChungTu?.toLowerCase().includes(searchMa.value.toLowerCase())) return false;
    if (searchDoiTac.value && !t.nguoiNopNhan?.toLowerCase().includes(searchDoiTac.value.toLowerCase())) return false;
    if (filterTaiKhoan.value && t.phuongThuc !== filterTaiKhoan.value) return false;
    if (filterLoaiPhieu.value && t.loaiPhieu !== filterLoaiPhieu.value) return false;
    return true;
  });
});


const formatPrice = (price) =>
  new Intl.NumberFormat("vi-VN").format(price || 0);
const formatDate = (dateString) => {
  const d = new Date(dateString);
  return `${String(d.getDate()).padStart(2, "0")}/${String(d.getMonth() + 1).padStart(2, "0")}/${d.getFullYear()} ${String(d.getHours()).padStart(2, "0")}:${String(d.getMinutes()).padStart(2, "0")}`;
};

const fetchTransactions = async () => {
  loading.value = true;
  try {
    const res = await axios.get(
      `/api/ThuChi/danh-sach?chiNhanhId=${globalState.value.activeBranchId || 0}`,
    );
    transactions.value = res.data.danhSach;
    summary.value = res.data.thongKe;
  } catch (e) {
    console.error(e);
  } finally {
    loading.value = false;
  }
};

const toggleDetail = (id) => {
  expandedRowId.value = expandedRowId.value === id ? null : id;
};

const openTransactionModal = async (type) => {
  const isTransfer = type === 'Chuyển khoản';
  const title = isTransfer ? 'Chuyển Khoản' : `Tạo Phiếu ${type === 'Thu' ? 'Thu' : 'Chi'}`;
  
  const { value: formValues } = await swal.fire({
    title: title,
    html: `
      <div class="text-start">
        <label class="form-label small fw-bold mb-1">Người nộp/nhận ${isTransfer ? '(Tùy chọn)' : '*'}</label>
        <input id="swal-nguoi" class="form-control mb-2" placeholder="Ví dụ: Nguyễn Văn A">
        <label class="form-label small fw-bold mb-1">Giá trị (VNĐ) *</label>
        <input id="swal-giatri" type="number" class="form-control mb-2" placeholder="VD: 500000">
        <label class="form-label small fw-bold mb-1">Phương thức *</label>
        <select id="swal-phuongthuc" class="form-select mb-2">
          <option value="Tiền mặt">Tiền mặt</option>
          <option value="Chuyển khoản" ${isTransfer ? 'selected' : ''}>Chuyển khoản</option>
          <option value="VNPAY-QR">VNPAY-QR</option>
        </select>
        <label class="form-label small fw-bold mb-1">Lý do</label>
        <textarea id="swal-lydo" class="form-control" rows="2" placeholder="Ghi chú thêm..."></textarea>
      </div>
    `,
    showCancelButton: true,
    cancelButtonText: "Hủy",
    confirmButtonText: "Xác nhận",
    preConfirm: () => {
      const nguoiNopNhan = document.getElementById('swal-nguoi').value;
      const giaTri = document.getElementById('swal-giatri').value;
      const phuongThuc = document.getElementById('swal-phuongthuc').value;
      const lyDo = document.getElementById('swal-lydo').value;

      if (!giaTri || giaTri <= 0) {
        swal.showValidationMessage("Giá trị phải lớn hơn 0");
        return false;
      }
      if (!isTransfer && !nguoiNopNhan.trim()) {
        swal.showValidationMessage("Vui lòng nhập người nộp/nhận");
        return false;
      }
      return {
        loaiPhieu: isTransfer ? 'Chi' : type,
        phuongThuc,
        nguoiNopNhan,
        giaTri: parseFloat(giaTri),
        lyDo,
        hangMuc: isTransfer ? 'Chuyển khoản' : (type === 'Thu' ? 'Thu khác' : 'Chi khác'),
        chiNhanhId: globalState.value.activeBranchId || 0
      };
    }
  });

  if (formValues) {
    try {
      // Send directly to the new POST endpoint API
      await axios.post('/api/ThuChi', formValues);
      swal.fire({
        icon: 'success',
        title: 'Thành công!',
        timer: 1500,
        showConfirmButton: false
      });
      fetchTransactions();
    } catch (e) {
      swal.fire("Lỗi", e.response?.data || "Không thể tạo phiếu", "error");
    }
  }
};

watch(() => globalState.value.activeBranchId, fetchTransactions);
onMounted(fetchTransactions);
</script>

<style scoped>
.filter-header {
  border-left: 3px solid #f37021;
}
.list-group-item.active {
  background-color: #f8f9fa !important;
  border-left: 3px solid #0d6efd !important;
}
.cursor-pointer {
  cursor: pointer;
}
.detail-row td {
  box-shadow: inset 0 3px 6px -3px rgba(0, 0, 0, 0.2);
}
</style>
