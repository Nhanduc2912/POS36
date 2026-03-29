<template>
  <div class="container-fluid p-4 bg-light min-vh-100">
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h3 class="fw-bold text-primary">
        <i class="bi bi-robot me-2"></i>Báo Cáo Thông Minh AI
      </h3>
    </div>

    <div class="card border-0 shadow-sm rounded-4 mb-4">
      <div class="card-body">
        <label class="fw-bold mb-2 text-secondary"
          >Bạn muốn xem báo cáo gì?</label
        >
        <div class="input-group input-group-lg">
          <input
            type="text"
            class="form-control bg-light border-0"
            v-model="prompt"
            placeholder="VD: Hãy tạo bảng phân tích doanh thu hôm nay và xuất ra các gợi ý bán hàng..."
            @keyup.enter="generateReport"
            :disabled="isLoading"
          />
          <button
            class="btn btn-primary px-4 fw-bold"
            @click="generateReport"
            :disabled="isLoading || !prompt.trim()"
          >
            <span
              v-if="isLoading"
              class="spinner-border spinner-border-sm me-2"
            ></span>
            Tạo Báo Cáo
          </button>
        </div>
        <div class="d-flex gap-2 mt-3">
          <span
            class="badge bg-info bg-opacity-10 text-info cursor-pointer"
            @click="prompt = 'Lập bảng doanh thu hôm nay và cho 3 lời khuyên'"
            >Gợi ý: Doanh thu & Lời khuyên</span
          >
          <span
            class="badge bg-info bg-opacity-10 text-info cursor-pointer"
            @click="prompt = 'Viết một email báo cáo ngắn gọn gửi cho Giám đốc'"
            >Gợi ý: Viết Email báo cáo</span
          >
        </div>
      </div>
    </div>

    <div v-if="reportHtml" class="card border-0 shadow-sm rounded-4 fade-in">
      <div
        class="card-header bg-white border-bottom py-3 d-flex justify-content-between align-items-center"
      >
        <h5 class="fw-bold mb-0 text-success">
          <i class="bi bi-check-circle-fill me-2"></i>Kết quả phân tích
        </h5>
        <div class="d-flex gap-2">
          <button
            @click="exportToExcel"
            class="btn btn-sm btn-outline-success fw-bold"
          >
            <i class="bi bi-file-earmark-excel me-1"></i> Tải Excel
          </button>
          <button
            @click="exportToWord"
            class="btn btn-sm btn-outline-primary fw-bold"
          >
            <i class="bi bi-file-earmark-word me-1"></i> Tải Word
          </button>
        </div>
      </div>

      <div
        class="card-body p-4 ai-result-content"
        ref="reportContainer"
        v-html="reportHtml"
      ></div>
    </div>
  </div>
</template>

<script setup>
import { ref } from "vue";
import axios from "axios";

const prompt = ref("");
const reportHtml = ref("");
const isLoading = ref(false);
const reportContainer = ref(null);

// Lấy Token từ LocalStorage để bảo mật API
const getAuthHeaders = () => {
  const token = localStorage.getItem("token");
  return { headers: { Authorization: `Bearer ${token}` } };
};

const generateReport = async () => {
  if (!prompt.value.trim()) return;
  isLoading.value = true;
  reportHtml.value = "";

  try {
    const token =
      localStorage.getItem("token") ||
      localStorage.getItem("pos36_token") ||
      "";

    // DÙNG LỆNH FETCH NGUYÊN THỦY ĐỂ NÉ AXIOS INTERCEPTOR (Tiêu diệt cục loading ở giữa)
    const response = await fetch("/api/AIChat/report", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify({ Prompt: prompt.value }),
    });

    if (!response.ok) throw new Error("Lỗi gọi AI");

    const data = await response.json();
    reportHtml.value = data.htmlReport;
  } catch (error) {
    alert("Lỗi tạo báo cáo. Vui lòng thử lại!");
  } finally {
    isLoading.value = false;
  }
};

// ==========================================
// TUYỆT KỸ XUẤT FILE TỪ TRÌNH DUYỆT (KHÔNG CẦN THƯ VIỆN)
// ==========================================
const exportToExcel = () => {
  // Thêm style cho thẻ table để Excel nhận diện đường viền
  let htmlContent = reportContainer.value.innerHTML;
  let excelFormat = `
        <html xmlns:x="urn:schemas-microsoft-com:office:excel">
            <head><meta charset="utf-8"></head>
            <body>
               ${htmlContent.replace(/<table/g, '<table border="1"')}
            </body>
        </html>`;

  let blob = new Blob([excelFormat], { type: "application/vnd.ms-excel" });
  let link = document.createElement("a");
  link.href = URL.createObjectURL(blob);
  link.download = `BaoCao_POS36_${new Date().getTime()}.xls`;
  link.click();
};

const exportToWord = () => {
  let htmlContent = reportContainer.value.innerHTML;
  let wordFormat = `
        <html xmlns:w="urn:schemas-microsoft-com:office:word">
            <head><meta charset="utf-8"></head>
            <body>${htmlContent}</body>
        </html>`;

  let blob = new Blob([wordFormat], { type: "application/msword" });
  let link = document.createElement("a");
  link.href = URL.createObjectURL(blob);
  link.download = `BaoCao_POS36_${new Date().getTime()}.doc`;
  link.click();
};
</script>

<style>
/* CSS làm đẹp cho cái Bảng HTML do AI sinh ra */
.ai-result-content table {
  width: 100%;
  border-collapse: collapse;
  margin-bottom: 20px;
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 0 0 1px #dee2e6;
}
.ai-result-content th {
  background-color: #f8f9fa;
  padding: 12px;
  font-weight: bold;
  border-bottom: 2px solid #dee2e6;
}
.ai-result-content td {
  padding: 12px;
  border-bottom: 1px solid #dee2e6;
}
.ai-result-content tr:last-child td {
  border-bottom: none;
}
.cursor-pointer {
  cursor: pointer;
}
.fade-in {
  animation: fadeIn 0.4s ease-in;
}
@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>
