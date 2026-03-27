<template>
  <div class="container-fluid px-4 py-4 bg-light min-vh-100">
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h5 class="fw-bold text-danger mb-0">
        <i class="bi bi-printer-fill me-2"></i> THIẾT LẬP MẪU IN
      </h5>
      <div class="d-flex gap-2">
        <button
          @click="handleTestPrint"
          class="btn btn-warning fw-bold text-dark px-4 shadow-sm"
        >
          <i class="bi bi-printer me-2"></i> In thử
        </button>
        <button
          @click="saveTemplate"
          class="btn btn-primary fw-bold px-4 shadow-sm"
        >
          <i class="bi bi-save me-2"></i> Lưu mẫu in
        </button>
      </div>
    </div>

    <div class="row g-4">
      <div class="col-lg-7 d-flex flex-column gap-4">
        <div class="card border-0 shadow-sm">
          <div
            class="card-header bg-white border-bottom py-2 d-flex justify-content-between align-items-center"
          >
            <ul class="nav nav-pills card-header-pills" role="tablist">
              <li class="nav-item" role="presentation">
                <button
                  class="nav-link fw-bold py-1 px-3"
                  :class="{ active: activeTab === 'visual' }"
                  @click="switchTab('visual')"
                >
                  Soạn thảo Trực quan
                </button>
              </li>
              <li class="nav-item" role="presentation">
                <button
                  class="nav-link fw-bold py-1 px-3 text-secondary"
                  :class="{ active: activeTab === 'html' }"
                  @click="switchTab('html')"
                >
                  Mã HTML
                </button>
              </li>
            </ul>

            <select
              v-model="selectedPreset"
              @change="applyPreset"
              class="form-select form-select-sm w-auto fw-bold text-primary shadow-none border-primary"
            >
              <option value="none" disabled>-- Chọn mẫu gợi ý --</option>
              <option value="standard">1. Mẫu Tiêu chuẩn (F&B)</option>
              <option value="minimalist">
                2. Mẫu Tối giản (Tiết kiệm giấy)
              </option>
              <option value="takeaway">3. Mẫu Takeaway / Trà sữa</option>
              <option value="proforma">
                4. Mẫu Tạm tính (Chưa thanh toán)
              </option>
              <option value="elegant">5. Mẫu Nhà hàng Sang trọng</option>
              <option value="custom6">6. Mẫu Cổ điển (Bố cục rộng)</option>
            </select>
          </div>

          <div class="card-body p-0 d-flex flex-column">
            <div
              v-show="activeTab === 'visual'"
              class="bg-light border-bottom p-2 d-flex gap-1"
            >
              <button
                class="btn btn-sm btn-white border shadow-sm px-3"
                @click="formatText('bold')"
                title="In đậm"
              >
                <i class="bi bi-type-bold"></i>
              </button>
              <button
                class="btn btn-sm btn-white border shadow-sm px-3"
                @click="formatText('italic')"
                title="In nghiêng"
              >
                <i class="bi bi-type-italic"></i>
              </button>
              <button
                class="btn btn-sm btn-white border shadow-sm px-3"
                @click="formatText('underline')"
                title="Gạch chân"
              >
                <i class="bi bi-type-underline"></i>
              </button>
              <div class="vr mx-1"></div>
              <button
                class="btn btn-sm btn-white border shadow-sm px-3"
                @click="formatText('justifyLeft')"
                title="Căn trái"
              >
                <i class="bi bi-text-left"></i>
              </button>
              <button
                class="btn btn-sm btn-white border shadow-sm px-3"
                @click="formatText('justifyCenter')"
                title="Căn giữa"
              >
                <i class="bi bi-text-center"></i>
              </button>
              <button
                class="btn btn-sm btn-white border shadow-sm px-3"
                @click="formatText('justifyRight')"
                title="Căn phải"
              >
                <i class="bi bi-text-right"></i>
              </button>
              <div class="ms-auto">
                <button
                  @click="resetToDefault"
                  class="btn btn-sm btn-outline-danger fw-bold"
                >
                  <i class="bi bi-arrow-counterclockwise"></i> Khôi phục gốc
                </button>
              </div>
            </div>

            <div
              v-show="activeTab === 'visual'"
              ref="visualEditor"
              class="editor-area p-4 bg-white overflow-auto custom-scrollbar"
              contenteditable="true"
              @input="syncToHtml"
              @blur="syncToHtml"
              style="height: 450px; outline: none"
            ></div>

            <textarea
              v-show="activeTab === 'html'"
              v-model="printTemplate"
              @input="syncToVisual"
              class="form-control border-0 rounded-0 font-monospace bg-dark text-success p-4 custom-scrollbar"
              style="
                height: 490px;
                font-size: 13px;
                line-height: 1.6;
                resize: none;
              "
              spellcheck="false"
            ></textarea>
          </div>
        </div>

        <div class="card border-0 shadow-sm flex-grow-1">
          <div
            class="card-header bg-white border-bottom py-2 d-flex justify-content-between align-items-center"
          >
            <h6 class="fw-bold text-dark mb-0 small">
              <i class="bi bi-code-slash me-2"></i> MÃ NHÚNG (CLICK ĐỂ CHÈN)
            </h6>
            <span class="small text-muted fst-italic"
              >Bấm vào mã để tự chèn vào vị trí con trỏ chuột</span
            >
          </div>
          <div
            class="card-body p-0 overflow-auto custom-scrollbar"
            style="max-height: 300px"
          >
            <table class="table table-hover table-sm mb-0 small align-middle">
              <thead class="table-light text-muted">
                <tr>
                  <th class="ps-3 py-2">Ý nghĩa thông tin</th>
                  <th class="text-end pe-3 py-2">Mã nhúng</th>
                </tr>
              </thead>
              <tbody>
                <tr
                  v-for="(tag, index) in availableTags"
                  :key="index"
                  @click="insertTag(tag.code)"
                  class="cursor-pointer"
                >
                  <td class="ps-3 py-2 fw-bold text-dark">{{ tag.name }}</td>
                  <td class="text-end pe-3 py-2 text-primary font-monospace">
                    {{ tag.code }}
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>

      <div class="col-lg-5">
        <div
          class="card border-0 shadow-sm h-100 bg-secondary bg-opacity-10 d-flex flex-column"
        >
          <div
            class="card-header bg-white border-bottom py-3 d-flex justify-content-between align-items-center"
          >
            <h6 class="fw-bold text-dark mb-0">
              <i class="bi bi-eye-fill text-primary me-2"></i> KẾT QUẢ XEM TRƯỚC
            </h6>
            <span class="badge bg-primary px-3 py-2">Khổ 80mm</span>
          </div>
          <div
            class="card-body d-flex justify-content-center align-items-start p-4 overflow-auto custom-scrollbar flex-grow-1"
          >
            <div
              class="receipt-paper shadow-lg bg-white"
              v-html="previewHtml"
            ></div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed, inject } from "vue";
import axios from "axios";
const swal = inject("$swal");
const activeTab = ref("visual");
const visualEditor = ref(null);
const selectedPreset = ref("none");

const availableTags = [
  { name: "Tên cửa hàng", code: "{TEN_CUA_HANG}" },
  { name: "Địa chỉ cửa hàng", code: "{DIA_CHI}" },
  { name: "Điện thoại", code: "{DIEN_THOAI}" },
  { name: "Mã hóa đơn", code: "{MA_CHUNG_TU}" },
  { name: "Ngày in", code: "{NGAY_IN}" },
  { name: "Giờ in", code: "{GIO_IN}" },
  { name: "Tên bàn/Phòng", code: "{TEN_BAN}" },
  { name: "Tên thu ngân", code: "{THU_NGAN}" },
  { name: "Bảng danh sách món ăn", code: "{DANH_SACH_MON_AN}" },
  { name: "Tổng cộng", code: "{TONG_CONG}" },
  { name: "Khách cần trả", code: "{KHACH_CAN_TRA}" },
  { name: "Ghi chú đơn hàng", code: "{GHI_CHU}" },
];

// Mẫu in đã được bọc <tr><td> để chống vỡ khung
const presets = {
  standard: `
<div style="font-family: Arial, sans-serif; font-size: 14px; color: #000;">
  <div style="text-align: center; margin-bottom: 10px;">
      <h2 style="margin: 0; font-size: 20px; font-weight: bold;">{TEN_CUA_HANG}</h2>
      <div style="font-size: 12px; margin-top: 5px;">Đ/c: {DIA_CHI}</div>
      <div style="font-size: 12px;">ĐT: {DIEN_THOAI}</div>
  </div>
  <div style="text-align: center; margin: 15px 0; border-top: 1px dashed #000; border-bottom: 1px dashed #000; padding: 10px 0;">
      <h3 style="margin: 0; font-size: 18px; font-weight: bold;">HÓA ĐƠN THANH TOÁN</h3>
      <div style="font-size: 12px; margin-top: 5px;">Số: {MA_CHUNG_TU} | Ngày: {NGAY_IN} {GIO_IN}</div>
  </div>
  <div style="font-size: 12px; margin-bottom: 10px; display: flex; justify-content: space-between;">
      <div><b>Bàn:</b> {TEN_BAN}</div>
      <div><b>Thu ngân:</b> {THU_NGAN}</div>
  </div>
  <table style="width: 100%; border-collapse: collapse; font-size: 13px; margin-bottom: 15px;">
      <thead style="border-bottom: 1px solid #000;">
          <tr><th style="text-align: left; padding-bottom: 5px;">Tên món</th><th style="text-align: center;">SL</th><th style="text-align: right;">T.Tiền</th></tr>
      </thead>
      <tbody>
          <tr><td colspan="3" style="text-align: center; color: #f37021; padding: 10px 0;">{DANH_SACH_MON_AN}</td></tr>
      </tbody>
  </table>
  <div style="border-top: 1px dashed #000; padding-top: 10px;">
      <div style="display: flex; justify-content: space-between; font-weight: bold; font-size: 16px;">
          <span>TỔNG CỘNG:</span><span>{KHACH_CAN_TRA}</span>
      </div>
  </div>
  <div style="text-align: center; margin-top: 20px; font-size: 12px; font-style: italic;">Xin cảm ơn và hẹn gặp lại quý khách!</div>
</div>`,
  minimalist: `
<div style="font-family: Arial, sans-serif; font-size: 12px; color: #000;">
  <h3 style="margin: 0; text-align: center; font-size: 16px;">{TEN_CUA_HANG}</h3>
  <div style="text-align: center; border-bottom: 1px solid #000; padding-bottom: 5px; margin-bottom: 5px;">
      <div>HÓA ĐƠN: {MA_CHUNG_TU}</div>
      <div>{NGAY_IN} {GIO_IN} - {TEN_BAN}</div>
  </div>
  <table style="width: 100%; font-size: 12px; margin-bottom: 5px; border-collapse: collapse;">
      <tbody>
          <tr><td colspan="3" style="text-align: center; color: #f37021; padding: 10px 0;">{DANH_SACH_MON_AN}</td></tr>
      </tbody>
  </table>
  <div style="border-top: 1px solid #000; padding-top: 5px; display: flex; justify-content: space-between; font-weight: bold; font-size: 14px;">
      <span>TỔNG:</span><span>{KHACH_CAN_TRA}</span>
  </div>
  <div style="text-align: center; font-size: 10px; margin-top: 10px;">Cảm ơn quý khách!</div>
</div>`,
  takeaway: `
<div style="font-family: Arial, sans-serif; font-size: 14px; color: #000; text-align: center;">
  <h2 style="margin: 0; font-size: 18px;">{TEN_CUA_HANG}</h2>
  <div style="margin: 15px 0; padding: 10px; border: 2px solid #000;">
      <div style="font-size: 12px;">MÃ GỌI ĐỒ</div>
      <h1 style="margin: 5px 0; font-size: 32px;">{MA_CHUNG_TU}</h1>
      <div style="font-size: 12px;">{NGAY_IN} {GIO_IN}</div>
  </div>
  <table style="width: 100%; font-size: 13px; text-align: left; margin-bottom: 10px; border-bottom: 1px dashed #000; border-collapse: collapse;">
      <tbody>
          <tr><td colspan="3" style="text-align: center; color: #f37021; padding: 10px 0;">{DANH_SACH_MON_AN}</td></tr>
      </tbody>
  </table>
  <div style="display: flex; justify-content: space-between; font-weight: bold; font-size: 16px;">
      <span>TỔNG CỘNG:</span><span>{KHACH_CAN_TRA}</span>
  </div>
</div>`,
  proforma: `
<div style="font-family: Arial, sans-serif; font-size: 14px; color: #000;">
  <div style="text-align: center; margin-bottom: 15px;">
      <h2 style="margin: 0; font-size: 18px;">{TEN_CUA_HANG}</h2>
      <h3 style="margin: 10px 0 0 0; font-size: 20px; text-decoration: underline;">PHIẾU TẠM TÍNH</h3>
      <div style="font-size: 11px; font-style: italic;">(Đây không phải là hóa đơn VAT)</div>
  </div>
  <div style="font-size: 13px; margin-bottom: 10px; padding-bottom: 10px; border-bottom: 1px solid #000;">
      <div>Bàn: <b>{TEN_BAN}</b> | Giờ in: {GIO_IN}</div>
  </div>
  <table style="width: 100%; border-collapse: collapse; font-size: 13px; margin-bottom: 15px;">
      <tbody>
          <tr><td colspan="3" style="text-align: center; color: #f37021; padding: 10px 0;">{DANH_SACH_MON_AN}</td></tr>
      </tbody>
  </table>
  <div style="border-top: 2px solid #000; padding-top: 10px; text-align: right; font-weight: bold; font-size: 18px;">
      SỐ TIỀN: {KHACH_CAN_TRA}
  </div>
  <div style="text-align: center; margin-top: 20px; font-size: 12px;">Quý khách vui lòng kiểm tra lại đồ uống.</div>
</div>`,
  elegant: `
<div style="font-family: 'Times New Roman', serif; font-size: 14px; color: #000;">
  <div style="text-align: center; margin-bottom: 15px;">
      <h2 style="margin: 0; font-size: 22px; text-transform: uppercase; letter-spacing: 2px;">{TEN_CUA_HANG}</h2>
      <div style="font-size: 12px; margin-top: 5px;">{DIA_CHI}</div>
  </div>
  <div style="border-top: 3px double #000; border-bottom: 3px double #000; padding: 10px 0; text-align: center; margin-bottom: 15px;">
      <div style="font-size: 18px; font-weight: bold; letter-spacing: 1px;">GUEST RECEIPT</div>
      <div style="font-size: 12px;">Table: {TEN_BAN} - {NGAY_IN}</div>
  </div>
  <table style="width: 100%; border-collapse: collapse; font-size: 14px; margin-bottom: 15px;">
      <tbody>
          <tr><td colspan="3" style="text-align: center; color: #f37021; padding: 10px 0;">{DANH_SACH_MON_AN}</td></tr>
      </tbody>
  </table>
  <div style="border-top: 1px solid #000; padding-top: 10px; display: flex; justify-content: space-between; font-weight: bold; font-size: 18px;">
      <span>TOTAL:</span><span>{KHACH_CAN_TRA}</span>
  </div>
  <div style="text-align: center; margin-top: 30px; font-style: italic;">Thank you for dining with us!</div>
</div>`,
  custom6: `
<div style="width: 100%; font-family: 'Arial', sans-serif; font-size: 14px; color: #000;">
    <div style="text-align: center; margin-bottom: 10px;">
        <h2 style="margin: 0; font-size: 20px; font-weight: bold;">{TEN_CUA_HANG}</h2>
        <div style="font-size: 12px; margin-top: 5px;">Đ/c: {DIA_CHI}</div>
        <div style="font-size: 12px;">ĐT: {DIEN_THOAI}</div>
    </div>
    <div style="text-align: center; margin: 15px 0; border-top: 1px dashed #000; border-bottom: 1px dashed #000; padding: 10px 0;">
        <h3 style="margin: 0; font-size: 18px; font-weight: bold;">HÓA ĐƠN THANH TOÁN</h3>
        <div style="font-size: 12px; margin-top: 5px;">Số: {MA_CHUNG_TU}</div>
        <div style="font-size: 12px;">Ngày: {NGAY_IN} {GIO_IN}</div>
    </div>
    <div style="font-size: 12px; margin-bottom: 10px; display: flex; justify-content: space-between;">
        <div><b>Bàn:</b> {TEN_BAN}</div>
        <div><b>Thu ngân:</b> {THU_NGAN}</div>
    </div>
    <table style="width: 100%; border-collapse: collapse; font-size: 13px; margin-bottom: 15px;">
        <thead style="border-bottom: 1px solid #000;">
            <tr>
                <th style="text-align: left; padding-bottom: 5px;">Tên món</th>
                <th style="text-align: center; padding-bottom: 5px;">SL</th>
                <th style="text-align: right; padding-bottom: 5px;">T.Tiền</th>
            </tr>
        </thead>
        <tbody>
            <tr><td colspan="3" style="text-align: center; color: #f37021; padding: 10px 0;">{DANH_SACH_MON_AN}</td></tr>
        </tbody>
    </table>
    <div style="border-top: 1px dashed #000; padding-top: 10px;">
        <div style="display: flex; justify-content: space-between; font-weight: bold; font-size: 16px;">
            <span>TỔNG THANH TOÁN:</span>
            <span>{KHACH_CAN_TRA}</span>
        </div>
    </div>
    <div style="text-align: center; margin-top: 20px; font-size: 12px; font-style: italic;">
        Xin cảm ơn và hẹn gặp lại quý khách!
    </div>
</div>`,
};

const printTemplate = ref("");

const mockOrder = {
  maChungTu: "HD260324-001",
  tenBan: "Bàn VIP 01",
  tongTien: 125000,
  items: [
    { name: "Cafe Sữa Đá", qty: 2, price: 25000 },
    { name: "Sinh Tố Bơ", qty: 1, price: 40000 },
    { name: "Bánh Mì Chảo", qty: 1, price: 35000 },
  ],
};
const mockBranch = {
  name: "POS36 ĐÀ NẴNG",
  address: "123 Lê Duẩn, Đà Nẵng",
  phone: "0905.123.456",
};
const formatPrice = (price) => new Intl.NumberFormat("vi-VN").format(price);

// HÀM LIVE PREVIEW ĐÃ ĐƯỢC NÂNG CẤP REGEX THÔNG MINH
const previewHtml = computed(() => {
  let itemsHtml = mockOrder.items
    .map(
      (item) => `
        <tr>
            <td style="padding: 5px 0; border-bottom: 1px dotted #ccc; text-align: left;">${item.name} <br> <small style="color:#666">${formatPrice(item.price)}</small></td>
            <td style="text-align: center; padding: 5px 0; border-bottom: 1px dotted #ccc;">${item.qty}</td>
            <td style="text-align: right; padding: 5px 0; border-bottom: 1px dotted #ccc; font-weight: bold;">${formatPrice(item.price * item.qty)}</td>
        </tr>
    `,
    )
    .join("");

  const now = new Date();
  let html = printTemplate.value
    .replace(/{TEN_CUA_HANG}/g, mockBranch.name)
    .replace(/{DIA_CHI}/g, mockBranch.address)
    .replace(/{DIEN_THOAI}/g, mockBranch.phone)
    .replace(/{MA_CHUNG_TU}/g, mockOrder.maChungTu)
    .replace(/{NGAY_IN}/g, now.toLocaleDateString("vi-VN"))
    .replace(/{GIO_IN}/g, now.toLocaleTimeString("vi-VN"))
    .replace(/{TEN_BAN}/g, mockOrder.tenBan)
    .replace(/{THU_NGAN}/g, "Admin_Duc")
    .replace(/{TONG_CONG}/g, formatPrice(mockOrder.tongTien))
    .replace(/{KHACH_CAN_TRA}/g, formatPrice(mockOrder.tongTien))
    .replace(/{GHI_CHU}/g, "");

  // Thuật toán: Tìm xem {DANH_SACH_MON_AN} có đang nằm trong thẻ <tr><td> nào không, nếu có thì nuốt trọn cả thẻ đó!
  const rowRegex =
    /<tr[^>]*>[\s\S]*?<td[^>]*>[\s\S]*?{DANH_SACH_MON_AN}[\s\S]*?<\/td>[\s\S]*?<\/tr>/gi;
  if (rowRegex.test(html)) {
    html = html.replace(rowRegex, itemsHtml);
  } else {
    html = html.replace(/{DANH_SACH_MON_AN}/g, itemsHtml);
  }

  return html;
});

onMounted(async () => {
  try {
    const res = await axios.get("/api/ThietLap/PrintTemplate");
    if (res.data && res.data.duLieu) {
      printTemplate.value = res.data.duLieu;
      localStorage.setItem("pos36_print_template", res.data.duLieu);
    } else {
      printTemplate.value = presets.custom6; // Gán mẫu mặc định nếu CSDL trống
    }
    if (visualEditor.value) {
      visualEditor.value.innerHTML = printTemplate.value;
    }
  } catch (e) {}
});

const saveTemplate = async () => {
  if (activeTab.value === "visual") syncToHtml();

  try {
    await axios.post("/api/ThietLap", {
      maThietLap: "PrintTemplate",
      duLieu: printTemplate.value,
    });
    localStorage.setItem("pos36_print_template", printTemplate.value);
    swal.fire({
      toast: true,
      position: "top-end",
      icon: "success",
      title: "Đã lưu mẫu in lên CSDL!",
      timer: 1500,
      showConfirmButton: false,
    });
  } catch (e) {
    swal.fire("Lỗi", "Lưu thiết lập thất bại", "error");
  }
};

const applyPreset = () => {
  if (selectedPreset.value !== "none") {
    printTemplate.value = presets[selectedPreset.value];
    if (visualEditor.value) visualEditor.value.innerHTML = printTemplate.value;
    syncToHtml();
  }
};

const switchTab = (tab) => {
  activeTab.value = tab;
  if (tab === "visual") {
    visualEditor.value.innerHTML = printTemplate.value;
  } else {
    printTemplate.value = visualEditor.value.innerHTML;
  }
};

const syncToHtml = () => {
  if (activeTab.value === "visual")
    printTemplate.value = visualEditor.value.innerHTML;
};
const syncToVisual = () => {
  if (activeTab.value === "html" && visualEditor.value)
    visualEditor.value.innerHTML = printTemplate.value;
};

const formatText = (command) => {
  document.execCommand(command, false, null);
  visualEditor.value.focus();
  syncToHtml();
};

const insertTag = (code) => {
  if (activeTab.value === "visual") {
    visualEditor.value.focus();
    document.execCommand("insertText", false, code);
    syncToHtml();
  } else {
    printTemplate.value += code;
  }
  swal.fire({
    toast: true,
    position: "top-end",
    icon: "success",
    title: `Đã chèn ${code}`,
    timer: 1000,
    showConfirmButton: false,
  });
};

const handleTestPrint = () => {
  const iframe = document.createElement("iframe");
  iframe.style.display = "none";
  document.body.appendChild(iframe);
  iframe.contentWindow.document.open();
  iframe.contentWindow.document.write(`
        <html><head><title>In Thử</title><style>@media print { @page { margin: 0; } body { margin: 0.5cm; } }</style></head>
        <body><div style="width: 80mm; max-width: 100%;">${previewHtml.value}</div></body></html>
    `);
  iframe.contentWindow.document.close();
  setTimeout(() => {
    iframe.contentWindow.focus();
    iframe.contentWindow.print();
    setTimeout(() => document.body.removeChild(iframe), 1000);
  }, 500);
};
</script>

<style scoped>
.cursor-pointer {
  cursor: pointer;
}
.cursor-pointer:hover td {
  background-color: #f8f9fa;
}
.custom-scrollbar::-webkit-scrollbar {
  width: 6px;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: #f1f1f1;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background: #c1c1c1;
  border-radius: 4px;
}
.custom-scrollbar::-webkit-scrollbar-thumb:hover {
  background: #a8a8a8;
}
.receipt-paper {
  width: 100%;
  max-width: 320px;
  margin: 0 auto;
  padding: 20px;
  border-top: 5px dashed #ccc;
  border-bottom: 5px dashed #ccc;
  color: #000;
}
.editor-area {
  border: 1px solid transparent;
  transition: border-color 0.2s;
}
.editor-area:focus {
  border-color: #f37021;
}
.editor-area table {
  width: 100% !important;
  border-collapse: collapse !important;
}
.editor-area th,
.editor-area td {
  padding: 5px !important;
}
.editor-area th:nth-child(1),
.editor-area td:nth-child(1) {
  text-align: left !important;
}
.editor-area th:nth-child(2),
.editor-area td:nth-child(2) {
  text-align: center !important;
}
.editor-area th:nth-child(3),
.editor-area td:nth-child(3) {
  text-align: right !important;
}
</style>
