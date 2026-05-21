<template>
  <div class="sa-report-root">
    <!-- Page Header -->
    <div class="sa-report-header">
      <div>
        <h5 class="fw-bold mb-1" style="color:var(--sa-text)">
          <i class="bi bi-robot me-2" style="color:var(--sa-accent)"></i>Báo Cáo Thông Minh AI
        </h5>
        <p class="mb-0" style="color:var(--sa-text-faint);font-size:.82rem">
          Gõ lệnh để AI tự tạo báo cáo HTML chuyên nghiệp từ dữ liệu thực tế
        </p>
      </div>
      <div class="d-flex gap-2 align-items-center">
        <span style="color:var(--sa-text-faint);font-size:.78rem">
          <i class="bi bi-file-earmark-text me-1"></i>{{ reports.length }} báo cáo
        </span>
        <button class="sa-action-btn-sm" @click="clearAll" v-if="reports.length">
          <i class="bi bi-trash3 me-1"></i>Xóa tất cả
        </button>
      </div>
    </div>

    <!-- Prompt Box -->
    <div class="sa-prompt-card">
      <div class="sa-prompt-icon">
        <i class="bi bi-stars"></i>
      </div>
      <div class="sa-prompt-body">
        <label class="sa-prompt-label">Bạn muốn tạo báo cáo gì?</label>
        <div class="sa-prompt-input-wrap">
          <textarea
            v-model="prompt"
            class="sa-prompt-input"
            placeholder="VD: Hãy tạo bảng phân tích doanh thu tháng này, so sánh với tháng trước và đưa ra 3 gợi ý tăng doanh số..."
            rows="3"
            @keydown.ctrl.enter.prevent="generateReport"
            :disabled="isLoading"
          ></textarea>
          <button class="sa-prompt-btn" @click="generateReport" :disabled="isLoading || !prompt.trim()">
            <span v-if="isLoading" class="spinner-border spinner-border-sm me-1"></span>
            <i v-else class="bi bi-send-fill me-1"></i>
            {{ isLoading ? 'Đang tạo...' : 'Tạo Báo Cáo' }}
          </button>
        </div>
        <!-- Suggestions -->
        <div class="sa-suggestions">
          <span class="sa-sug-label"><i class="bi bi-lightbulb me-1"></i>Gợi ý nhanh:</span>
          <button
            v-for="s in suggestions"
            :key="s.text"
            class="sa-sug-chip"
            @click="prompt = s.prompt; generateReport()"
            :disabled="isLoading"
          >
            <i :class="'bi bi-' + s.icon + ' me-1'"></i>{{ s.text }}
          </button>
        </div>
      </div>
    </div>

    <!-- Report Board -->
    <div class="sa-report-board" v-if="reports.length">
      <div class="sa-board-header">
        <h6 class="fw-bold mb-0" style="color:var(--sa-text-faint);font-size:.75rem;text-transform:uppercase;letter-spacing:.5px">
          <i class="bi bi-layout-masonry me-2"></i>Bảng Báo Cáo ({{ reports.length }})
        </h6>
        <div class="d-flex gap-2">
          <select v-model="viewMode" class="sa-mini-select">
            <option value="grid">Dạng lưới</option>
            <option value="list">Dạng danh sách</option>
          </select>
        </div>
      </div>

      <div :class="viewMode === 'grid' ? 'sa-reports-grid' : 'sa-reports-list'">
        <div
          v-for="(report, idx) in reports"
          :key="report.id"
          class="sa-report-card"
          :class="{ 'sa-report-card-expanded': report.expanded, 'sa-report-card-newest': idx === 0 }"
        >
          <!-- Card Header -->
          <div class="sa-rcard-header" @click="report.expanded = !report.expanded">
            <div class="sa-rcard-meta">
              <span class="sa-rcard-num">#{{ reports.length - idx }}</span>
              <div>
                <div class="sa-rcard-title">{{ report.title }}</div>
                <div class="sa-rcard-time">
                  <i class="bi bi-clock me-1"></i>{{ formatTime(report.createdAt) }}
                </div>
              </div>
            </div>
            <div class="d-flex gap-2 align-items-center">
              <button class="sa-rcard-btn" @click.stop="exportReport(report, 'excel')" title="Xuất Excel">
                <i class="bi bi-file-earmark-excel"></i>
              </button>
              <button class="sa-rcard-btn" @click.stop="exportReport(report, 'word')" title="Xuất Word">
                <i class="bi bi-file-earmark-word"></i>
              </button>
              <button class="sa-rcard-btn" @click.stop="printReport(report)" title="In báo cáo">
                <i class="bi bi-printer"></i>
              </button>
              <button class="sa-rcard-btn danger" @click.stop="removeReport(report.id)" title="Xóa">
                <i class="bi bi-trash3"></i>
              </button>
              <i :class="report.expanded ? 'bi bi-chevron-up' : 'bi bi-chevron-down'"
                style="color:var(--sa-text-faint);font-size:.8rem"></i>
            </div>
          </div>

          <!-- Card Body (collapsible) -->
          <div class="sa-rcard-body" v-show="report.expanded">
            <div class="sa-rcard-prompt" v-if="report.prompt">
              <i class="bi bi-chat-dots me-2" style="color:var(--sa-accent)"></i>
              <em>{{ report.prompt }}</em>
            </div>
            <div class="sa-rcard-content" v-html="report.html"></div>
            <!-- AI Suggestions for this report -->
            <div class="sa-rcard-suggestions" v-if="report.suggestions?.length">
              <div class="sa-rsug-title"><i class="bi bi-lightbulb-fill me-2" style="color:#f59e0b"></i>AI gợi ý thêm:</div>
              <div class="d-flex flex-wrap gap-2 mt-2">
                <button
                  v-for="sug in report.suggestions"
                  :key="sug"
                  class="sa-rsug-chip"
                  @click="prompt = sug; generateReport()"
                >
                  {{ sug }}
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div class="sa-empty-state" v-else-if="!isLoading">
      <i class="bi bi-file-earmark-bar-graph display-1 opacity-15"></i>
      <p class="fw-bold mt-3 mb-1" style="color:var(--sa-text)">Chưa có báo cáo nào</p>
      <p style="color:var(--sa-text-faint);font-size:.85rem">Gõ lệnh vào ô trên để AI tạo báo cáo đầu tiên cho bạn</p>
    </div>

    <!-- Loading Overlay -->
    <div class="sa-loading-report" v-if="isLoading">
      <div class="sa-loading-inner">
        <div class="sa-ai-pulse">
          <i class="bi bi-robot"></i>
        </div>
        <p class="mt-3 fw-bold" style="color:var(--sa-accent)">AI đang phân tích dữ liệu...</p>
        <p style="color:var(--sa-text-faint);font-size:.8rem">{{ loadingTip }}</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue';

const prompt = ref('');
const isLoading = ref(false);
const viewMode = ref('grid');
const reports = ref([]);
const backendUrl = 'http://localhost:5098';

const loadingTips = [
  'Đang thu thập dữ liệu từ hệ thống...',
  'Đang phân tích xu hướng...',
  'Đang tạo bảng biểu HTML...',
  'Đang tổng hợp gợi ý thông minh...',
  'Sắp xong rồi...',
];
const loadingTip = ref(loadingTips[0]);
let tipInterval = null;

const suggestions = [
  { text: 'Doanh thu hôm nay', icon: 'cash-stack', prompt: 'Tạo bảng phân tích doanh thu hôm nay, so sánh với hôm qua và đưa ra 3 gợi ý cải thiện' },
  { text: 'Top cửa hàng', icon: 'shop', prompt: 'Liệt kê top 10 cửa hàng có doanh thu cao nhất trong hệ thống, tạo bảng xếp hạng chuyên nghiệp' },
  { text: 'Đơn đăng ký mới', icon: 'person-plus', prompt: 'Tạo báo cáo tổng hợp các đơn đăng ký gói dịch vụ mới trong tháng này, phân loại theo gói' },
  { text: 'Thống kê hệ thống', icon: 'speedometer2', prompt: 'Tạo báo cáo tổng quan hệ thống: số cửa hàng, doanh thu, tình trạng hoạt động và các chỉ số quan trọng' },
  { text: 'Gói sắp hết hạn', icon: 'calendar-x', prompt: 'Liệt kê các cửa hàng có gói dịch vụ sắp hết hạn trong 30 ngày tới, sắp xếp theo ngày hết hạn' },
  { text: 'Báo cáo AI theo yêu cầu', icon: 'magic', prompt: '' },
];

const generateReport = async () => {
  if (!prompt.value.trim() || isLoading.value) return;
  isLoading.value = true;

  // Rotate loading tips
  let tipIdx = 0;
  tipInterval = setInterval(() => {
    tipIdx = (tipIdx + 1) % loadingTips.length;
    loadingTip.value = loadingTips[tipIdx];
  }, 1500);

  try {
    const token = localStorage.getItem('pos36_token') || localStorage.getItem('token') || '';
    const response = await fetch(`${backendUrl}/api/AIChat/report`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify({ Prompt: prompt.value }),
    });

    if (!response.ok) throw new Error('Lỗi gọi AI');
    const data = await response.json();

    // Extract title from HTML (first h1/h2/h3)
    let title = prompt.value.slice(0, 60) + (prompt.value.length > 60 ? '...' : '');
    const titleMatch = data.htmlReport?.match(/<h[1-3][^>]*>([^<]+)<\/h[1-3]>/i);
    if (titleMatch) title = titleMatch[1].trim();

    // Generate AI suggestions based on the report context
    const aiSuggestions = generateAISuggestions(prompt.value);

    // Add to the TOP of reports board
    reports.value.unshift({
      id: Date.now(),
      title,
      prompt: prompt.value,
      html: data.htmlReport,
      createdAt: new Date(),
      expanded: true,
      suggestions: aiSuggestions,
    });

    // Save to localStorage
    saveReports();
    prompt.value = '';
  } catch (error) {
    alert('Lỗi tạo báo cáo: ' + error.message);
  } finally {
    isLoading.value = false;
    clearInterval(tipInterval);
    loadingTip.value = loadingTips[0];
  }
};

const generateAISuggestions = (originalPrompt) => {
  const lower = originalPrompt.toLowerCase();
  const suggestions = [];
  if (lower.includes('doanh thu')) {
    suggestions.push('So sánh doanh thu theo từng giờ trong ngày');
    suggestions.push('Phân tích tỷ lệ đơn hàng thành công và hủy');
  }
  if (lower.includes('cửa hàng')) {
    suggestions.push('Xem chi tiết từng cửa hàng theo khu vực');
    suggestions.push('Phân tích cửa hàng nào có tỷ lệ tăng trưởng cao nhất');
  }
  if (lower.includes('đăng ký') || lower.includes('gói')) {
    suggestions.push('Xem tỷ lệ gia hạn theo từng gói dịch vụ');
    suggestions.push('Phân tích xu hướng chuyển đổi gói');
  }
  // Default suggestions
  if (suggestions.length === 0) {
    suggestions.push('Tạo biểu đồ xu hướng cho dữ liệu này');
    suggestions.push('So sánh với cùng kỳ năm trước');
  }
  return suggestions.slice(0, 3);
};

const removeReport = (id) => {
  reports.value = reports.value.filter(r => r.id !== id);
  saveReports();
};

const clearAll = () => {
  if (confirm('Xóa tất cả báo cáo?')) {
    reports.value = [];
    saveReports();
  }
};

const saveReports = () => {
  try {
    const toSave = reports.value.map(r => ({
      ...r,
      html: r.html?.slice(0, 50000) // Limit size
    }));
    localStorage.setItem('pos36_sa_reports', JSON.stringify(toSave));
  } catch (e) { /* ignore quota errors */ }
};

const loadReports = () => {
  try {
    const saved = localStorage.getItem('pos36_sa_reports');
    if (saved) {
      const parsed = JSON.parse(saved);
      reports.value = parsed.map(r => ({ ...r, createdAt: new Date(r.createdAt) }));
    }
  } catch (e) { reports.value = []; }
};

// Check if AITerminal sent a report to this page
const checkIncomingReport = () => {
  const incoming = localStorage.getItem('pos36_ai_report_html');
  const incomingPrompt = localStorage.getItem('pos36_ai_report_prompt');
  if (incoming) {
    localStorage.removeItem('pos36_ai_report_html');
    localStorage.removeItem('pos36_ai_report_prompt');

    const titleMatch = incoming.match(/<h[1-3][^>]*>([^<]+)<\/h[1-3]>/i);
    const title = titleMatch ? titleMatch[1].trim() : (incomingPrompt?.slice(0, 60) || 'Báo cáo AI');

    reports.value.unshift({
      id: Date.now(),
      title,
      prompt: incomingPrompt || '',
      html: incoming,
      createdAt: new Date(),
      expanded: true,
      suggestions: generateAISuggestions(incomingPrompt || ''),
    });
    saveReports();
  }
};

const exportReport = (report, type) => {
  const htmlContent = report.html;
  if (type === 'excel') {
    const content = `<html xmlns:x="urn:schemas-microsoft-com:office:excel"><head><meta charset="utf-8"></head><body>${htmlContent.replace(/<table/g, '<table border="1"')}</body></html>`;
    const blob = new Blob([content], { type: 'application/vnd.ms-excel' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a'); a.href = url; a.download = `BaoCao_SA_${Date.now()}.xls`; a.click();
  } else {
    const content = `<html xmlns:w="urn:schemas-microsoft-com:office:word"><head><meta charset="utf-8"></head><body>${htmlContent}</body></html>`;
    const blob = new Blob([content], { type: 'application/msword' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a'); a.href = url; a.download = `BaoCao_SA_${Date.now()}.doc`; a.click();
  }
};

const printReport = (report) => {
  const win = window.open('', '_blank');
  win.document.write(`<!DOCTYPE html><html><head><meta charset="utf-8"><title>${report.title}</title>
  <style>body{font-family:Arial,sans-serif;padding:20px}table{border-collapse:collapse;width:100%}th,td{border:1px solid #ddd;padding:8px}th{background:#f2f2f2}</style>
  </head><body>${report.html}</body></html>`);
  win.document.close();
  win.print();
};

const formatTime = (d) => {
  if (!d) return '';
  const now = new Date();
  const diff = now - new Date(d);
  if (diff < 60000) return 'Vừa xong';
  if (diff < 3600000) return Math.floor(diff / 60000) + ' phút trước';
  return new Date(d).toLocaleString('vi-VN');
};

// Listen for AI Terminal events (report created by AITerminal)
const handleStorageChange = (e) => {
  if (e.key === 'pos36_ai_report_html') {
    checkIncomingReport();
  }
};

onMounted(() => {
  loadReports();
  checkIncomingReport();
  window.addEventListener('storage', handleStorageChange);
});

onUnmounted(() => {
  window.removeEventListener('storage', handleStorageChange);
  clearInterval(tipInterval);
});
</script>

<style scoped>
@import "./sa-shared.css";

.sa-report-root {
  position: relative;
  min-height: 100%;
}

/* Header */
.sa-report-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 20px;
  flex-wrap: wrap;
  gap: 12px;
}

.sa-action-btn-sm {
  display: flex; align-items: center; gap: 5px;
  background: rgba(239,68,68,.1); border: 1px solid rgba(239,68,68,.3);
  color: #ef4444; padding: 6px 12px; border-radius: 8px;
  font-size: .8rem; font-weight: 700; cursor: pointer; transition: .2s;
}
.sa-action-btn-sm:hover { background: rgba(239,68,68,.2); }

/* Prompt Card */
.sa-prompt-card {
  display: flex; gap: 16px;
  background: var(--sa-surface);
  border: 1px solid var(--sa-border);
  border-radius: 16px;
  padding: 20px;
  margin-bottom: 24px;
  position: relative;
  overflow: hidden;
}
.sa-prompt-card::before {
  content: '';
  position: absolute;
  top: 0; left: 0; right: 0;
  height: 2px;
  background: linear-gradient(90deg, var(--sa-accent), #3b82f6, var(--sa-accent));
  background-size: 200% 100%;
  animation: gradientShift 3s linear infinite;
}
@keyframes gradientShift {
  0% { background-position: 0% 0%; }
  100% { background-position: 200% 0%; }
}
.sa-prompt-icon {
  width: 44px; height: 44px;
  background: rgba(245,158,11,.12);
  border: 1px solid rgba(245,158,11,.25);
  border-radius: 12px;
  display: flex; align-items: center; justify-content: center;
  font-size: 1.3rem; color: var(--sa-accent);
  flex-shrink: 0;
}
.sa-prompt-body { flex: 1; min-width: 0; }
.sa-prompt-label {
  display: block;
  font-size: .82rem; font-weight: 700;
  color: var(--sa-text-muted);
  margin-bottom: 10px;
  text-transform: uppercase; letter-spacing: .5px;
}
.sa-prompt-input-wrap {
  display: flex; gap: 10px; align-items: flex-end;
}
.sa-prompt-input {
  flex: 1;
  background: var(--sa-surface-2);
  border: 1px solid var(--sa-border);
  color: var(--sa-text);
  border-radius: 12px;
  padding: 12px 16px;
  font-size: .9rem;
  font-family: 'Inter', sans-serif;
  resize: none;
  outline: none;
  line-height: 1.5;
  transition: border-color .2s;
}
.sa-prompt-input:focus { border-color: var(--sa-accent); }
.sa-prompt-input::placeholder { color: var(--sa-text-faint); }
.sa-prompt-btn {
  display: flex; align-items: center; gap: 6px;
  background: var(--sa-accent);
  border: none;
  color: #000;
  font-weight: 800;
  font-size: .85rem;
  padding: 12px 22px;
  border-radius: 12px;
  cursor: pointer;
  transition: all .2s;
  white-space: nowrap;
  flex-shrink: 0;
}
.sa-prompt-btn:hover:not(:disabled) { filter: brightness(1.1); transform: translateY(-1px); }
.sa-prompt-btn:disabled { opacity: .5; cursor: not-allowed; transform: none; }

/* Suggestions */
.sa-suggestions {
  display: flex; gap: 8px; flex-wrap: wrap;
  margin-top: 12px; align-items: center;
}
.sa-sug-label { font-size: .75rem; color: var(--sa-text-faint); white-space: nowrap; }
.sa-sug-chip {
  background: rgba(245,158,11,.08);
  border: 1px solid rgba(245,158,11,.2);
  color: var(--sa-accent);
  padding: 4px 12px; border-radius: 20px;
  font-size: .75rem; font-weight: 600;
  cursor: pointer; transition: .15s;
}
.sa-sug-chip:hover:not(:disabled) { background: rgba(245,158,11,.18); }
.sa-sug-chip:disabled { opacity: .4; }

/* Board */
.sa-report-board {
  background: var(--sa-surface);
  border: 1px solid var(--sa-border);
  border-radius: 16px;
  overflow: hidden;
}
.sa-board-header {
  display: flex; justify-content: space-between; align-items: center;
  padding: 14px 20px;
  border-bottom: 1px solid var(--sa-border);
  background: var(--sa-surface-2);
}
.sa-mini-select {
  background: var(--sa-surface);
  border: 1px solid var(--sa-border);
  color: var(--sa-text-muted);
  padding: 4px 10px; border-radius: 6px;
  font-size: .8rem; cursor: pointer; outline: none;
}

/* Reports Grid/List */
.sa-reports-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(480px, 1fr));
  gap: 1px;
  background: var(--sa-border);
}
.sa-reports-list {
  display: flex; flex-direction: column;
  gap: 0;
}

/* Report Card */
.sa-report-card {
  background: var(--sa-surface);
  transition: background .2s;
}
.sa-report-card:hover { background: var(--sa-surface-2); }
.sa-report-card-newest .sa-rcard-header {
  border-left: 3px solid var(--sa-accent);
}

.sa-rcard-header {
  display: flex; justify-content: space-between; align-items: center;
  padding: 14px 18px;
  cursor: pointer;
  user-select: none;
  transition: background .15s;
}
.sa-rcard-header:hover { background: rgba(245,158,11,.04); }
.sa-rcard-meta { display: flex; gap: 12px; align-items: center; min-width: 0; }
.sa-rcard-num {
  width: 32px; height: 32px;
  background: rgba(245,158,11,.12);
  border: 1px solid rgba(245,158,11,.25);
  color: var(--sa-accent);
  border-radius: 8px;
  display: flex; align-items: center; justify-content: center;
  font-size: .8rem; font-weight: 800; flex-shrink: 0;
}
.sa-rcard-title {
  font-weight: 700; font-size: .9rem;
  color: var(--sa-text);
  white-space: nowrap; overflow: hidden; text-overflow: ellipsis;
  max-width: 300px;
}
.sa-rcard-time { font-size: .72rem; color: var(--sa-text-faint); margin-top: 2px; }

.sa-rcard-btn {
  width: 30px; height: 30px;
  background: rgba(127,127,127,.08);
  border: 1px solid var(--sa-border);
  color: var(--sa-text-muted);
  border-radius: 7px; cursor: pointer;
  display: flex; align-items: center; justify-content: center;
  font-size: .85rem; transition: .15s; flex-shrink: 0;
}
.sa-rcard-btn:hover { background: rgba(245,158,11,.12); color: var(--sa-accent); border-color: rgba(245,158,11,.3); }
.sa-rcard-btn.danger:hover { background: rgba(239,68,68,.12); color: #ef4444; border-color: rgba(239,68,68,.3); }

.sa-rcard-body {
  padding: 0 18px 18px;
  border-top: 1px solid var(--sa-border);
}
.sa-rcard-prompt {
  background: rgba(245,158,11,.06);
  border: 1px solid rgba(245,158,11,.15);
  border-radius: 8px;
  padding: 10px 14px;
  margin: 14px 0;
  font-size: .82rem;
  color: var(--sa-text-muted);
  display: flex; align-items: flex-start; gap: 6px;
}
.sa-rcard-content {
  background: var(--sa-surface-2);
  border: 1px solid var(--sa-border);
  border-radius: 12px;
  padding: 16px;
  overflow-x: auto;
}

/* Content styling for HTML reports */
.sa-rcard-content :deep(table) {
  width: 100%; border-collapse: collapse; margin-bottom: 16px;
  font-size: .85rem; border-radius: 8px; overflow: hidden;
}
.sa-rcard-content :deep(th) {
  background: rgba(245,158,11,.12); color: var(--sa-accent);
  padding: 10px 14px; font-weight: 700; text-align: left;
  border-bottom: 2px solid rgba(245,158,11,.2);
  white-space: nowrap;
}
.sa-rcard-content :deep(td) {
  padding: 9px 14px; border-bottom: 1px solid var(--sa-border);
  color: var(--sa-text);
}
.sa-rcard-content :deep(tr:last-child td) { border-bottom: none; }
.sa-rcard-content :deep(tr:hover td) { background: rgba(245,158,11,.04); }
.sa-rcard-content :deep(h1), .sa-rcard-content :deep(h2), .sa-rcard-content :deep(h3) {
  color: var(--sa-text); margin-bottom: 12px; margin-top: 16px;
}
.sa-rcard-content :deep(p) { color: var(--sa-text-muted); line-height: 1.6; }
.sa-rcard-content :deep(ul), .sa-rcard-content :deep(ol) {
  color: var(--sa-text-muted); padding-left: 20px;
}
.sa-rcard-content :deep(strong) { color: var(--sa-text); font-weight: 700; }
.sa-rcard-content :deep(.highlight), .sa-rcard-content :deep(.accent) { color: var(--sa-accent); }

/* AI Suggestions for report */
.sa-rcard-suggestions {
  margin-top: 16px;
  background: rgba(245,158,11,.05);
  border: 1px solid rgba(245,158,11,.15);
  border-radius: 10px;
  padding: 14px;
}
.sa-rsug-title { font-size: .82rem; font-weight: 700; color: var(--sa-text-muted); }
.sa-rsug-chip {
  background: rgba(245,158,11,.1);
  border: 1px solid rgba(245,158,11,.25);
  color: var(--sa-accent);
  padding: 5px 14px; border-radius: 20px;
  font-size: .78rem; font-weight: 600;
  cursor: pointer; transition: .15s;
}
.sa-rsug-chip:hover { background: rgba(245,158,11,.2); }

/* Empty State */
.sa-empty-state {
  text-align: center;
  padding: 80px 20px;
  color: var(--sa-text-faint);
}

/* Loading Overlay */
.sa-loading-report {
  position: fixed;
  inset: 0;
  background: rgba(0,0,0,.7);
  backdrop-filter: blur(4px);
  display: flex; align-items: center; justify-content: center;
  z-index: 1000;
}
.sa-loading-inner {
  text-align: center;
  background: var(--sa-surface);
  border: 1px solid var(--sa-border);
  border-radius: 20px;
  padding: 40px 60px;
}
.sa-ai-pulse {
  width: 70px; height: 70px;
  background: rgba(245,158,11,.12);
  border: 2px solid var(--sa-accent);
  border-radius: 50%;
  display: flex; align-items: center; justify-content: center;
  font-size: 2rem; color: var(--sa-accent);
  margin: 0 auto;
  animation: aiPulse 2s infinite;
}
@keyframes aiPulse {
  0%, 100% { box-shadow: 0 0 0 0 rgba(245,158,11,.4); }
  50% { box-shadow: 0 0 0 20px rgba(245,158,11,0); }
}
</style>
