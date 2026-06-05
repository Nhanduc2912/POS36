<template>
  <div class="container-fluid px-4 py-4 bg-light min-vh-100">
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h5 class="fw-bold text-danger mb-0">
        <i class="bi bi-pie-chart-fill me-2"></i> TỔNG QUAN HÔM NAY
      </h5>
      <div
        v-if="isLoading"
        class="spinner-border text-primary spinner-border-sm"
      ></div>
    </div>

    <div class="row g-3 mb-3">
      <div class="col-xl-3 col-md-6">
        <div
          class="card border-0 shadow-sm h-100 card-hover"
          @click="router.push('/admin/orders')"
        >
          <div
            class="card-body d-flex align-items-center justify-content-between"
          >
            <div>
              <span class="text-muted fw-bold small d-block mb-1"
                >DOANH THU (VNĐ)</span
              >
              <h3 class="mb-0 fw-bolder text-dark">
                {{ summary.doanhThu.toLocaleString("vi-VN") }}
              </h3>
            </div>
            <div
              class="icon-box bg-success bg-opacity-10 text-success rounded-3 fs-3"
            >
              <i class="bi bi-cash-stack"></i>
            </div>
          </div>
        </div>
      </div>

      <div class="col-xl-3 col-md-6">
        <div
          class="card border-0 shadow-sm h-100 card-hover"
          @click="router.push('/admin/orders')"
        >
          <div
            class="card-body d-flex align-items-center justify-content-between"
          >
            <div>
              <span class="text-muted fw-bold small d-block mb-1"
                >TẠM TÍNH HIỆN TẠI</span
              >
              <h3 class="mb-0 fw-bolder text-primary">
                {{ summary.tamTinh.toLocaleString("vi-VN") }}
              </h3>
            </div>
            <div
              class="icon-box bg-primary bg-opacity-10 text-primary rounded-3 fs-3"
            >
              <i class="bi bi-calculator"></i>
            </div>
          </div>
        </div>
      </div>

      <div class="col-xl-3 col-md-6">
        <div
          class="card border-0 shadow-sm h-100 card-hover"
          @click="router.push('/admin/orders')"
        >
          <div
            class="card-body d-flex align-items-center justify-content-between"
          >
            <div>
              <span class="text-muted fw-bold small d-block mb-1"
                >SỐ ĐƠN HÀNG</span
              >
              <h3 class="mb-0 fw-bolder text-dark">
                {{ summary.tongDonHang }}
              </h3>
            </div>
            <div
              class="icon-box bg-info bg-opacity-10 text-info rounded-3 fs-3"
            >
              <i class="bi bi-receipt"></i>
            </div>
          </div>
        </div>
      </div>

      <div class="col-xl-3 col-md-6">
        <div
          class="card border-0 shadow-sm h-100 card-hover"
          @click="router.push('/admin/tables')"
        >
          <div
            class="card-body d-flex align-items-center justify-content-between"
          >
            <div>
              <span class="text-muted fw-bold small d-block mb-1"
                >BÀN ĐANG PHỤC VỤ</span
              >
              <h3 class="mb-0 fw-bolder text-dark">
                {{ summary.banDangDung }}
                <span class="fs-6 text-muted fw-normal"
                  >/ {{ summary.tongBan }}</span
                >
              </h3>
            </div>
            <div
              class="icon-box bg-warning bg-opacity-10 text-warning rounded-3 fs-3"
            >
              <i class="bi bi-grid-fill"></i>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="row g-3 mb-4">
      <div class="col-xl-3 col-md-6">
        <div
          class="card border-0 shadow-sm h-100 card-hover"
          @click="router.push('/admin/cashbook')"
        >
          <div class="card-body d-flex align-items-center p-3">
            <i class="bi bi-wallet2 fs-2 text-secondary me-3"></i>
            <div>
              <h5 class="mb-0 fw-bold">
                {{ summary.tienMat.toLocaleString("vi-VN") }}
              </h5>
              <small class="text-muted fw-bold">TIỀN MẶT</small>
            </div>
          </div>
        </div>
      </div>

      <div class="col-xl-3 col-md-6">
        <div
          class="card border-0 shadow-sm h-100 card-hover"
          @click="router.push('/admin/cashbook')"
        >
          <div class="card-body d-flex align-items-center p-3">
            <i class="bi bi-qr-code-scan fs-2 text-primary me-3"></i>
            <div>
              <h5 class="mb-0 fw-bold">
                {{ summary.chuyenKhoan.toLocaleString("vi-VN") }}
              </h5>
              <small class="text-muted fw-bold">CHUYỂN KHOẢN</small>
            </div>
          </div>
        </div>
      </div>

      <div class="col-xl-3 col-md-6">
        <div
          class="card border-0 shadow-sm h-100 card-hover"
          @click="router.push('/admin/orders')"
        >
          <div class="card-body d-flex align-items-center p-3">
            <i class="bi bi-arrow-return-left fs-2 text-danger me-3"></i>
            <div>
              <h5 class="mb-0 fw-bold">{{ summary.donHuy }}</h5>
              <small class="text-muted fw-bold">MÓN HỦY/TRẢ</small>
            </div>
          </div>
        </div>
      </div>

      <div class="col-xl-3 col-md-6">
        <div
          class="card border-0 shadow-sm h-100 card-hover border-start border-danger border-4"
          @click="router.push('/admin/inventory')"
        >
          <div class="card-body d-flex align-items-center p-3">
            <i
              class="bi bi-exclamation-triangle-fill fs-2 text-danger me-3 pulse-icon"
            ></i>
            <div>
              <h5 class="mb-0 fw-bold text-danger">
                {{ summary.canhBaoKho }} SP
              </h5>
              <small class="text-danger fw-bold">SẮP HẾT HÀNG</small>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="row g-4">
      <div class="col-lg-12">
        <div class="card border-0 shadow-sm h-100 rounded-3 overflow-hidden">
          <div
            class="card-header bg-white border-bottom py-3 d-flex justify-content-between align-items-center"
          >
            <h6 class="fw-bold text-dark mb-0">
              <i class="bi bi-bar-chart-fill text-primary me-2"></i> BIỂU ĐỒ
              DOANH THU 7 NGÀY QUA
            </h6>
            <select
              class="form-select form-select-sm w-auto shadow-none fw-bold text-secondary border-0 bg-light"
            >
              <option>Theo Doanh thu</option>
              <option>Theo Đơn hàng</option>
            </select>
          </div>
          <div class="card-body p-4">
            <div v-if="chartData.labels.length > 0" style="height: 350px">
              <Bar :data="chartData" :options="chartOptions" />
            </div>
            <div
              v-else
              class="d-flex flex-column justify-content-center align-items-center"
              style="
                height: 350px;
                background-color: #fcfcfc;
                border: 2px dashed #eaeaea;
                border-radius: 12px;
              "
            >
              <div class="bg-white p-3 rounded-circle shadow-sm mb-3">
                <i class="bi bi-inbox fs-2 text-muted"></i>
              </div>
              <h6 class="text-muted fw-bold">Chưa có dữ liệu giao dịch</h6>
              <p class="small text-muted">
                Các biểu đồ sẽ xuất hiện khi có phát sinh doanh thu.
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Onboarding Modal -->
    <div v-if="showOnboarding" class="onboarding-overlay d-flex justify-content-center align-items-center">
      <div class="onboarding-card card border-0 shadow-lg rounded-4 p-4 text-dark position-relative overflow-hidden" style="width: 100%; max-width: 550px;">
        <div class="decor-circle bg-danger opacity-10 position-absolute rounded-circle" style="width: 200px; height: 200px; top: -50px; right: -50px;"></div>
        
        <div class="position-relative z-1">
          <!-- Step 1: Khảo sát Mô hình -->
          <div v-if="onboardingStep === 1">
            <div class="text-center mb-4">
              <span class="badge bg-danger bg-opacity-10 text-danger px-3 py-2 rounded-pill fw-bold mb-2"><i class="bi bi-stars me-1"></i> CHÀO MỪNG ĐẾN VỚI POS36</span>
              <h4 class="fw-bold text-dark mb-1">Cấu hình Cửa hàng của bạn</h4>
              <p class="text-muted small">Vui lòng chọn mô hình kinh doanh để bắt đầu thiết lập nhanh.</p>
            </div>

            <div class="mb-4">
              <label class="form-label fw-bold text-secondary small">1. Mô hình kinh doanh của quán?</label>
              <div class="row g-2">
                <div class="col-6">
                  <div class="model-option p-3 border rounded-3 text-center cursor-pointer" :class="{ 'active': form.modelType === 'cafe' }" @click="form.modelType = 'cafe'">
                    <i class="bi bi-cup-hot fs-3 d-block mb-1 text-danger"></i>
                    <span class="fw-bold small">Cafe & Trà sữa</span>
                  </div>
                </div>
                <div class="col-6">
                  <div class="model-option p-3 border rounded-3 text-center cursor-pointer" :class="{ 'active': form.modelType === 'restaurant' }" @click="form.modelType = 'restaurant'">
                    <i class="bi bi-egg-fried fs-3 d-block mb-1 text-danger"></i>
                    <span class="fw-bold small">Nhà hàng / Quán ăn</span>
                  </div>
                </div>
                <div class="col-6">
                  <div class="model-option p-3 border rounded-3 text-center cursor-pointer" :class="{ 'active': form.modelType === 'pub' }" @click="form.modelType = 'pub'">
                    <i class="bi bi-cup fs-3 d-block mb-1 text-danger"></i>
                    <span class="fw-bold small">Quán nhậu & Bar</span>
                  </div>
                </div>
                <div class="col-6">
                  <div class="model-option p-3 border rounded-3 text-center cursor-pointer" :class="{ 'active': form.modelType === 'fastfood' }" @click="form.modelType = 'fastfood'">
                    <i class="bi bi-bag-heart fs-3 d-block mb-1 text-danger"></i>
                    <span class="fw-bold small">Thức ăn nhanh (Fast food)</span>
                  </div>
                </div>
              </div>
            </div>

            <div class="mb-4">
              <label class="form-label fw-bold text-secondary small">2. Bạn có sử dụng sơ đồ bàn ghế không?</label>
              <div class="form-check p-3 border rounded-3 mb-2 cursor-pointer" @click="form.hasTables = true">
                <input class="form-check-input ms-0 me-2" type="radio" :checked="form.hasTables" />
                <label class="form-check-label fw-bold small text-dark cursor-pointer">
                  Có, phục vụ tại bàn chuyên nghiệp
                </label>
                <div class="text-muted small ps-4" style="font-size: 0.75rem;">Sử dụng sơ đồ phòng/bàn chi tiết cho nhân viên order và thu ngân.</div>
              </div>
              <div class="form-check p-3 border rounded-3 cursor-pointer" @click="form.hasTables = false">
                <input class="form-check-input ms-0 me-2" type="radio" :checked="!form.hasTables" />
                <label class="form-check-label fw-bold small text-dark cursor-pointer">
                  Không, chủ yếu bán tại quầy / mang đi
                </label>
                <div class="text-muted small ps-4" style="font-size: 0.75rem;">
                  <span v-if="form.modelType === 'fastfood'" class="text-danger fw-bold">[Khuyên dùng]</span>
                  Tự động cấu hình in QR Code thanh toán tại quầy thu ngân và tạo 1-3 bàn phụ trợ nếu khách muốn ngồi lại.
                </div>
              </div>
            </div>

            <!-- 3. Thiết lập vận hành nâng cao -->
            <div class="mb-4">
              <button class="btn btn-sm btn-outline-secondary w-100 fw-bold d-flex align-items-center justify-content-between px-3 py-2 rounded-3 text-start" type="button" data-bs-toggle="collapse" data-bs-target="#advancedSettingsCollapse" aria-expanded="false" aria-controls="advancedSettingsCollapse">
                <span><i class="bi bi-gear-fill me-1"></i> Tùy chỉnh Cấu hình & Bảo mật Nâng cao</span>
                <i class="bi bi-chevron-down"></i>
              </button>
              
              <div class="collapse mt-3" id="advancedSettingsCollapse">
                <div class="p-3 bg-light rounded-3 border" style="font-size: 0.8rem;">
                  <!-- In ấn & QR -->
                  <div class="fw-bold text-secondary mb-2 border-bottom pb-1">🖨️ IN ẤN & QR CODE</div>
                  <div class="form-check form-switch mb-2 text-start">
                    <input class="form-check-input" type="checkbox" id="onbTuDongIn" v-model="form.posTuDongIn" />
                    <label class="form-check-label fw-semibold text-dark cursor-pointer" for="onbTuDongIn">Tự động in bill ngay khi thanh toán xong</label>
                  </div>
                  <div class="form-check form-switch mb-3 text-start">
                    <input class="form-check-input" type="checkbox" id="onbQrOnly" v-model="form.posHienQrThuNganOnly" />
                    <label class="form-check-label fw-semibold text-dark cursor-pointer" for="onbQrOnly">Chỉ hiện mã QR tại màn hình Thu Ngân ở quầy</label>
                  </div>

                  <!-- Bảo mật & Chống gian lận -->
                  <div class="fw-bold text-secondary mb-2 border-bottom pb-1">🛡️ BẢO MẬT & PHÂN QUYỀN</div>
                  <div class="form-check form-switch mb-2 text-start">
                    <input class="form-check-input" type="checkbox" id="onbXoaBill" v-model="form.permThuNganXoaHoaDon" />
                    <label class="form-check-label fw-semibold text-dark cursor-pointer" for="onbXoaBill">Cho phép Thu ngân tự xóa bàn/hủy hóa đơn trống</label>
                  </div>
                  <div class="form-check form-switch mb-2 text-start">
                    <input class="form-check-input" type="checkbox" id="onbPinHuy" v-model="form.posYeuCauMatKhauHuyMon" />
                    <label class="form-check-label fw-semibold text-dark cursor-pointer" for="onbPinHuy">Yêu cầu mã PIN khi xóa/hủy bàn (Chống gian lận)</label>
                  </div>
                  <div class="form-check form-switch mb-3 text-start">
                    <input class="form-check-input" type="checkbox" id="onbHuyMonGui" v-model="form.permThuNganHuyMonDaGui" />
                    <label class="form-check-label fw-semibold text-dark cursor-pointer" for="onbHuyMonGui">Cho phép Thu ngân hủy món đã báo làm (chưa ra đồ)</label>
                  </div>

                  <!-- Vận hành nâng cao -->
                  <div class="fw-bold text-secondary mb-2 border-bottom pb-1">⚡ VẬN HÀNH THÔNG MINH</div>
                  <div class="form-check form-switch mb-2 text-start">
                    <input class="form-check-input" type="checkbox" id="onbKhoaSo" v-model="form.posTuDongKhoaSo" />
                    <label class="form-check-label fw-semibold text-dark cursor-pointer" for="onbKhoaSo">Tự động khóa sổ lúc 23:59 để khóa sổ kế toán</label>
                  </div>
                  <div class="form-check form-switch mb-2 text-start">
                    <input class="form-check-input" type="checkbox" id="onbChonMon" v-model="form.posThanhToanBatBuocChonMon" />
                    <label class="form-check-label fw-semibold text-dark cursor-pointer" for="onbChonMon">Bắt buộc phải chọn món mới cho phép tính tiền</label>
                  </div>
                  
                  <div class="mt-2 pt-2 border-top text-start">
                    <label class="form-label fw-bold text-secondary mb-1 d-block">Mã PIN xác thực nhanh của Admin/Chủ quán (4 số):</label>
                    <input type="text" class="form-control form-control-sm text-center fw-bold w-50" maxlength="4" v-model="form.adminPin" placeholder="1234" />
                  </div>
                </div>
              </div>
            </div>

            <div class="d-flex justify-content-between mt-4">
              <button class="btn btn-link text-secondary text-decoration-none fw-bold" @click="submitOnboarding(false)">Bỏ qua & Sử dụng sạch</button>
              <button class="btn btn-danger fw-bold px-4 rounded-3" @click="onboardingStep = 2">
                Tiếp tục <i class="bi bi-arrow-right"></i>
              </button>
            </div>
          </div>

          <!-- Step 2: Cấu hình dữ liệu mẫu -->
          <div v-if="onboardingStep === 2">
            <div class="text-center mb-4">
              <h4 class="fw-bold text-dark mb-1">Cấu hình Dữ liệu mẫu</h4>
              <p class="text-muted small">Tùy chọn chi tiết các loại dữ liệu bạn muốn sinh để chạy thử hệ thống.</p>
            </div>

            <div class="alert alert-warning small mb-4" style="font-size: 0.8rem;">
              <i class="bi bi-info-circle-fill me-1"></i> Dữ liệu mẫu giúp bạn có sẵn báo cáo bán hàng, giá vốn, sơ đồ bàn để thử nghiệm mà không cần tốn công nhập thủ công.
            </div>

            <div class="mb-4 bg-light p-3 rounded-3 border">
              <h6 class="fw-bold text-dark mb-3">Tùy chọn loại dữ liệu sẽ tạo:</h6>
              
              <!-- Checkbox 1: Sản phẩm -->
              <div class="mb-3">
                <div class="d-flex align-items-center justify-content-between">
                  <div class="form-check mb-0">
                    <input class="form-check-input" type="checkbox" id="chkProducts" v-model="form.importProducts" />
                    <label class="form-check-label fw-bold small cursor-pointer" for="chkProducts">Danh mục & Sản phẩm mẫu</label>
                  </div>
                  <div v-if="form.importProducts" class="d-flex align-items-center gap-1">
                    <span class="small text-muted text-nowrap">Số lượng:</span>
                    <input type="number" class="form-control form-control-sm text-center fw-bold" style="width: 65px;" v-model="form.productCount" min="1" max="30" />
                  </div>
                </div>
              </div>

              <!-- Checkbox 2: Sơ đồ bàn -->
              <div class="mb-3" v-if="form.hasTables || form.modelType === 'fastfood'">
                <div class="d-flex align-items-center justify-content-between">
                  <div class="form-check mb-0">
                    <input class="form-check-input" type="checkbox" id="chkTables" v-model="form.importTables" />
                    <label class="form-check-label fw-bold small cursor-pointer" for="chkTables">Sơ đồ bàn ghế</label>
                  </div>
                  <div v-if="form.importTables" class="d-flex align-items-center gap-1">
                    <span class="small text-muted text-nowrap">Số lượng:</span>
                    <input type="number" class="form-control form-control-sm text-center fw-bold" style="width: 65px;" v-model="form.tableCount" :min="1" :max="form.modelType === 'fastfood' ? 3 : 20" :disabled="form.modelType === 'fastfood'" />
                  </div>
                </div>
                <div v-if="form.modelType === 'fastfood' && form.importTables" class="text-danger small fst-italic mt-1" style="font-size: 0.75rem;">
                  * Đã khóa 3 bàn phụ trợ cho mô hình Thức ăn nhanh (gồm Quầy thu ngân & 2 bàn ăn).
                </div>
              </div>

              <!-- Checkbox 3: Giao dịch giả lập -->
              <div class="mb-3">
                <div class="d-flex align-items-center justify-content-between">
                  <div class="form-check mb-0">
                    <input class="form-check-input" type="checkbox" id="chkTransactions" v-model="form.importTransactions" />
                    <label class="form-check-label fw-bold small cursor-pointer" for="chkTransactions">Hóa đơn & Doanh thu thử nghiệm</label>
                  </div>
                  <div v-if="form.importTransactions" class="d-flex align-items-center gap-1">
                    <span class="small text-muted text-nowrap">Số đơn:</span>
                    <input type="number" class="form-control form-control-sm text-center fw-bold" style="width: 65px;" v-model="form.transactionCount" min="1" max="50" />
                  </div>
                </div>
              </div>
              
              <!-- Validation Alert -->
              <div v-if="!isSampleDataValid" class="text-danger small fw-bold mt-2">
                ⚠️ Bạn phải chọn ít nhất 1 loại dữ liệu mẫu để khởi tạo!
              </div>
            </div>

            <div class="d-flex justify-content-between align-items-center mt-4">
              <button class="btn btn-outline-secondary fw-bold px-3 rounded-3" @click="onboardingStep = 1">
                <i class="bi bi-arrow-left"></i> Quay lại
              </button>
              <div class="d-flex gap-2">
                <button class="btn btn-link text-secondary text-decoration-none fw-bold" @click="submitOnboarding(false)">Bỏ qua</button>
                <button class="btn btn-success fw-bold px-4 rounded-3 d-flex align-items-center gap-2" :disabled="!isSampleDataValid || isSubmittingOnboarding" @click="submitOnboarding(true)">
                  <span v-if="isSubmittingOnboarding" class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
                  Khởi tạo ngay <i class="bi bi-check-lg"></i>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch, computed, inject } from "vue";
import { useRouter } from "vue-router";
import axios from "axios";
import { Bar } from "vue-chartjs";
import {
  Chart as ChartJS,
  Title,
  Tooltip,
  Legend,
  BarElement,
  CategoryScale,
  LinearScale,
} from "chart.js";
import { globalState } from "../store";

ChartJS.register(
  Title,
  Tooltip,
  Legend,
  BarElement,
  CategoryScale,
  LinearScale,
);

const router = useRouter();
const swal = inject("$swal");
const isLoading = ref(true);

const showOnboarding = ref(false);
const onboardingStep = ref(1);
const isSubmittingOnboarding = ref(false);

const form = ref({
  modelType: "cafe",
  hasTables: true,
  tableCount: 6,
  importProducts: true,
  productCount: 10,
  importTables: true,
  importTransactions: true,
  transactionCount: 10,
  
  // Thiết lập nâng cao bổ sung
  posTuDongIn: true,
  posHienQrThuNganOnly: false,
  permThuNganXoaHoaDon: false,
  posYeuCauMatKhauHuyMon: true,
  posTuDongKhoaSo: true,
  permThuNganHuyMonDaGui: false,
  posThanhToanBatBuocChonMon: true,
  posChoPhepHoanTraMon: true,
  securityYeuCauPIN: true,
  adminPin: "1234",
});

const isSampleDataValid = computed(() => {
  return form.value.importProducts || form.value.importTables || form.value.importTransactions;
});

watch(() => form.value.modelType, (newModel) => {
  if (newModel === "fastfood") {
    form.value.hasTables = false;
    form.value.tableCount = 3;
    form.value.posTuDongIn = false;
    form.value.posHienQrThuNganOnly = true;
    form.value.permThuNganXoaHoaDon = true;
  } else {
    form.value.hasTables = true;
    form.value.tableCount = 6;
    form.value.posTuDongIn = true;
    form.value.posHienQrThuNganOnly = false;
    form.value.permThuNganXoaHoaDon = false;
  }
});

const checkOnboardingStatus = async () => {
  try {
    const res = await axios.get("/api/SampleData/check-status");
    if (res.data && res.data.hasData === false) {
      const dismissed = localStorage.getItem("pos36_onboarding_dismissed");
      if (!dismissed) {
        showOnboarding.value = true;
      }
    }
  } catch (e) {
    console.error("Lỗi check onboarding status:", e);
  }
};

const submitOnboarding = async (useSampleData) => {
  if (useSampleData) {
    if (!isSampleDataValid.value) return;
    
    isSubmittingOnboarding.value = true;
    try {
      const payload = {
        modelType: form.value.modelType,
        hasTables: form.value.hasTables,
        tableCount: form.value.tableCount,
        importProducts: form.value.importProducts,
        productCount: form.value.productCount,
        importTables: form.value.importTables,
        importTransactions: form.value.importTransactions,
        transactionCount: form.value.transactionCount,
        customSettings: {
          "POS_TuDongIn": String(form.value.posTuDongIn),
          "POS_HienQrThuNganOnly": String(form.value.posHienQrThuNganOnly),
          "Perm_ThuNgan_XoaHoaDon": String(form.value.permThuNganXoaHoaDon),
          "POS_YeuCauMatKhauHuyBill": String(form.value.posYeuCauMatKhauHuyMon),
          "POS_TuDongKhoaSo": String(form.value.posTuDongKhoaSo),
          "Perm_ThuNgan_HuyMonDaGui": String(form.value.permThuNganHuyMonDaGui),
          "POS_ThanhToanBatBuocChonMon": String(form.value.posThanhToanBatBuocChonMon),
          "POS_ChoPhepHoanTraMon": String(form.value.posChoPhepHoanTraMon),
          "Security_YeuCauPIN": String(form.value.securityYeuCauPIN),
          "Security_AdminPIN": String(form.value.adminPin || "1234"),
          "POS_HienQR": "true",
        }
      };
      
      await axios.post("/api/SampleData/generate", payload);
      
      localStorage.setItem("pos36_onboarding_dismissed", "true");
      showOnboarding.value = false;
      
      await swal.fire({
        icon: "success",
        title: "Khởi tạo thành công!",
        text: "Hệ thống đã chuẩn bị xong dữ liệu mẫu cho bạn.",
        timer: 2000,
        showConfirmButton: false,
      });
      
      fetchDashboardData();
    } catch (e) {
      console.error(e);
      swal.fire("Lỗi", e.response?.data?.message || "Không thể tạo dữ liệu mẫu", "error");
    } finally {
      isSubmittingOnboarding.value = false;
    }
  } else {
    localStorage.setItem("pos36_onboarding_dismissed", "true");
    showOnboarding.value = false;
    swal.fire({
      toast: true,
      position: "top-end",
      icon: "info",
      title: "Cửa hàng sạch đã sẵn sàng!",
      timer: 1500,
      showConfirmButton: false,
    });
  }
};

// Cấu trúc Data mới theo yêu cầu
const summary = ref({
  tongDonHang: 0,
  doanhThu: 0,
  tamTinh: 0,
  tienMat: 0,
  chuyenKhoan: 0,
  donHuy: 0,
  canhBaoKho: 0,
  banDangDung: 0,
  tongBan: 0,
});

const chartData = ref({
  labels: [],
  datasets: [
    {
      label: "Doanh thu",
      backgroundColor: "#4e73df", // Đổi màu chart cho sang trọng
      hoverBackgroundColor: "#2e59d9",
      borderRadius: 6,
      data: [],
    },
  ],
});

const chartOptions = ref({
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: { display: false },
    tooltip: {
      backgroundColor: "rgba(0, 0, 0, 0.8)",
      padding: 12,
      titleFont: { size: 14 },
      bodyFont: { size: 14, weight: "bold" },
      callbacks: {
        label: function (context) {
          let label = context.dataset.label || "";
          if (label) {
            label += ": ";
          }
          if (context.parsed.y !== null) {
            label += new Intl.NumberFormat("vi-VN", {
              style: "currency",
              currency: "VND",
            }).format(context.parsed.y);
          }
          return label;
        },
      },
    },
  },
  scales: {
    y: {
      beginAtZero: true,
      grid: { borderDash: [5, 5], color: "#f0f0f0" },
      ticks: {
        callback: function (value) {
          if (value >= 1000000) return value / 1000000 + " Tr";
          if (value >= 1000) return value / 1000 + " k";
          return value;
        },
      },
    },
    x: { grid: { display: false } },
  },
});

const fetchDashboardData = async () => {
  if (!globalState.value.activeBranchId) return;

  isLoading.value = true;
  try {
    const response = await axios.get(
      `/api/Dashboard/summary?chiNhanhId=${globalState.value.activeBranchId}`,
    );

    // Nối dữ liệu từ API vào UI. Nếu API chưa có đủ cột thì tạm thời để giá trị gốc 0
    summary.value = { ...summary.value, ...response.data.summary };

    chartData.value = {
      labels: response.data.chart.labels,
      datasets: [
        {
          label: "Doanh thu",
          backgroundColor: "#4e73df",
          hoverBackgroundColor: "#2e59d9",
          borderRadius: 6,
          data: response.data.chart.data,
        },
      ],
    };
  } catch (error) {
    console.error("Lỗi lấy dữ liệu dashboard:", error);
  } finally {
    isLoading.value = false;
  }
};

onMounted(() => {
  fetchDashboardData();
  checkOnboardingStatus();
});

watch(
  () => globalState.value.activeBranchId,
  (newId) => {
    if (newId) fetchDashboardData();
  },
);
</script>

<style scoped>
.onboarding-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background-color: rgba(0, 0, 0, 0.45);
  backdrop-filter: blur(8px);
  z-index: 9999;
}
.onboarding-card {
  background: rgba(255, 255, 255, 0.95);
  border: 1px solid rgba(255, 255, 255, 0.3);
}
.model-option {
  transition: all 0.2s ease-in-out;
  background-color: #ffffff;
}
.model-option:hover {
  transform: translateY(-2px);
  border-color: #dc3545 !important;
}
.model-option.active {
  background-color: rgba(220, 53, 69, 0.08) !important;
  border-color: #dc3545 !important;
  box-shadow: 0 4px 12px rgba(220, 53, 69, 0.15);
}
.model-option.active i {
  color: #dc3545 !important;
}
.model-option.active span {
  color: #dc3545 !important;
}
.cursor-pointer {
  cursor: pointer;
}

.icon-box {
  width: 55px;
  height: 55px;
  display: flex;
  align-items: center;
  justify-content: center;
}
.card-hover {
  transition: all 0.2s ease-in-out;
  cursor: pointer;
}
.card-hover:hover {
  transform: translateY(-4px);
  box-shadow: 0 0.5rem 1rem rgba(0, 0, 0, 0.1) !important;
  border-color: #dee2e6 !important;
}
.pulse-icon {
  animation: pulse 2s infinite;
}
@keyframes pulse {
  0% {
    transform: scale(1);
    opacity: 1;
  }
  50% {
    transform: scale(1.2);
    opacity: 0.7;
  }
  100% {
    transform: scale(1);
    opacity: 1;
  }
}
</style>
