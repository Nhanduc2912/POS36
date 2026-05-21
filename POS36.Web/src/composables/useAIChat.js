import { ref, reactive, watch } from "vue";
import axios from "axios";

const THEMES = {
  dark:    { bg: "#0d1117", surface: "#161b22", text: "#c9d1d9", muted: "#8b949e", accent: "#f59e0b", border: "#30363d", user: "#1f6feb" },
  dracula: { bg: "#282a36", surface: "#44475a", text: "#f8f8f2", muted: "#6272a4", accent: "#bd93f9", border: "#6272a4", user: "#ff79c6" },
  monokai: { bg: "#272822", surface: "#3e3d32", text: "#f8f8f2", muted: "#75715e", accent: "#a6e22e", border: "#49483e", user: "#66d9ef" },
  nord:    { bg: "#2e3440", surface: "#3b4252", text: "#eceff4", muted: "#4c566a", accent: "#88c0d0", border: "#4c566a", user: "#5e81ac" },
  light:   { bg: "#f6f8fa", surface: "#ffffff", text: "#1f2328", muted: "#656d76", accent: "#0969da", border: "#d0d7de", user: "#0969da" },
};

// GLOBAL STATE (Singletons) to survive component remounts
const messages = ref([]);
const loading = ref(false);
const models = ref([]);
const selectedModel = ref(localStorage.getItem("pos36_ai_model") || "gemini-1.5-flash");
const mode = ref(localStorage.getItem("pos36_ai_chat_mode") || "agent");
const theme = ref(localStorage.getItem("pos36_ai_theme") || "dark");
const sessionId = `sa-${Date.now()}`;
const lastUsage = ref(null);
const isConnected = ref(true);

// Store last user prompt for report generation after confirm
let lastUserPrompt = "";

// Navigation confirmation
const pendingNavigation = ref(null);

// Persist settings
watch(selectedModel, (val) => localStorage.setItem("pos36_ai_model", val));
watch(mode, (val) => localStorage.setItem("pos36_ai_chat_mode", val));
watch(theme, (val) => localStorage.setItem("pos36_ai_theme", val));

// Functions that produce reportable data - includes list queries too
const REPORT_FUNCTIONS = [
  "ThongKeSaaS", "ThongKe", "BaoCao", "XuatBaoCao",
  "Analytics", "DoanhThu", "TongHop", "PhanTich",
  "XuatBaoCaoAI", "DanhSachCuaHang", "XemNhatKy"
];
const isReportFunction = (name) => REPORT_FUNCTIONS.some(f => name?.toLowerCase().includes(f.toLowerCase()));

// Build minimal HTML from plain text result (fallback)
const buildHtmlFromResult = (functionName, resultText, userPrompt) => {
  const now = new Date().toLocaleString("vi-VN");
  const lines = resultText.split("\n").filter(l => l.trim());
  let body = "";
  lines.forEach(line => {
    if (line.startsWith("#")) {
      const lvl = Math.min((line.match(/^#+/) || [""])[0].length, 3);
      body += `<h${lvl} style="color:#333;margin-top:12px">${line.replace(/^#+\s*/, "")}</h${lvl}>`;
    } else if (line.startsWith("-") || line.startsWith("•") || line.startsWith("*")) {
      body += `<li style="margin:4px 0">${line.replace(/^[-•*]\s*/, "")}</li>`;
    } else if (line.trim()) {
      body += `<p style="margin:6px 0;color:#444">${line}</p>`;
    }
  });
  return `<div style="font-family:Arial,sans-serif;padding:16px;max-width:900px">
  <h2 style="color:#f59e0b;border-bottom:3px solid #f59e0b;padding-bottom:10px;margin-bottom:12px">
    📊 ${userPrompt || functionName}
  </h2>
  <p style="color:#888;font-size:0.82rem;margin-bottom:16px">🕐 Tạo lúc: ${now} &nbsp;|&nbsp; 📌 Nguồn: ${functionName}</p>
  <div>${body}</div>
</div>`;
};

export function useAIChat() {
  const currentTheme = () => THEMES[theme.value] || THEMES.dark;

  const fetchModels = async () => {
    try {
      const res = await axios.get("/api/AIChat/models");
      models.value = res.data;
      if (!models.value.some(m => m.id === selectedModel.value)) {
        const def = res.data.find((m) => m.isDefault);
        if (def) selectedModel.value = def.id;
        else if (res.data.length > 0) selectedModel.value = res.data[0].id;
      }
    } catch {
      models.value = [
        { id: "gemini-2.0-flash-lite", displayName: "Gemini 2.0 Flash Lite", description: "Nhanh, nhẹ (Mặc định)", isDefault: true },
        { id: "gemini-2.0-flash",      displayName: "Gemini 2.0 Flash",      description: "Cân bằng" },
        { id: "gemini-1.5-flash",      displayName: "Gemini 1.5 Flash",      description: "Ổn định" },
        { id: "gemini-1.5-pro",        displayName: "Gemini 1.5 Pro",        description: "Thông minh hơn" }
      ];
      if (!models.value.some(m => m.id === selectedModel.value)) selectedModel.value = "gemini-2.0-flash-lite";
    }
  };

  const typeText = async (msgObj, fullText) => {
    msgObj.loading = false;
    msgObj.text = "";
    const SPEED = 12;
    for (const ch of fullText) {
      msgObj.text += ch;
      await new Promise((r) => setTimeout(r, SPEED));
    }
    msgObj.typing = false;
  };

  const sendMessage = async (prompt) => {
    if (!prompt.trim() || loading.value) return;
    loading.value = true;
    isConnected.value = true;
    lastUserPrompt = prompt; // Save for use in confirmAction

    messages.value.push({ role: "user", text: prompt, time: new Date() });

    const aiMsg = reactive({
      role: "ai", text: "Đang suy nghĩ...", typing: true, loading: true,
      time: new Date(), usage: null, approval: null
    });
    messages.value.push(aiMsg);

    try {
      const res = await axios.post("/api/AIChat/chat", {
        prompt, sessionId, modelId: selectedModel.value, mode: mode.value,
      });
      const d = res.data;
      lastUsage.value = d.usage;
      aiMsg.usage = d.usage;
      aiMsg.loading = false;

      if (d.requiresAction) {
        if (d.functionName === "XuatBaoCaoAI") {
          try {
            const argsObj = JSON.parse(d.functionArgs);
            if (argsObj.htmlContent) {
              localStorage.setItem("pos36_ai_report_html", argsObj.htmlContent);
              localStorage.setItem("pos36_ai_report_prompt", prompt);
              aiMsg.typing = false;
              aiMsg.text = `✅ Báo cáo AI đã được tạo!\n\n📊 Bạn có muốn chuyển sang **Trang Báo Cáo** để xem không?`;
              pendingNavigation.value = {
                html: argsObj.htmlContent, prompt,
                route: "/super-admin/ai-report", label: "Trang Báo Cáo AI",
              };
              return;
            }
          } catch (e) { console.error("Parse XuatBaoCaoAI lỗi", e); }
        }

        aiMsg.typing = false;
        const reportHint = isReportFunction(d.functionName) ? "\n\n📊 Sau khi xác nhận, báo cáo sẽ được tạo và mở trang Báo Cáo AI." : "";
        aiMsg.text = `⚡ AI muốn thực thi: **${d.functionName}**${reportHint}`;

        let argsStr = d.functionArgs;
        try { argsStr = JSON.stringify(JSON.parse(d.functionArgs), null, 2); } catch {}
        aiMsg.approval = {
          functionName: d.functionName,
          functionArgsStr: argsStr,
          riskLevel: d.riskLevel,
          isReport: isReportFunction(d.functionName),
        };
      } else {
        await typeText(aiMsg, d.text || "(Không có phản hồi)");
      }
    } catch (e) {
      isConnected.value = false;
      aiMsg.typing = false;
      aiMsg.loading = false;
      const errorMsg = e.response?.data?.error || e.message;
      if (errorMsg.includes("Quota") || errorMsg.includes("429")) {
        aiMsg.text = `❌ **Lỗi Quota:** Hãy chọn \`Gemini 1.5 Flash\`.`;
      } else {
        aiMsg.text = `❌ Lỗi: ${errorMsg}`;
      }
    } finally {
      loading.value = false;
    }
  };

  const confirmAction = async (approval, confirmed) => {
    if (loading.value) return;
    loading.value = true;
    const resultMsg = reactive({ role: "system", text: "Đang xử lý...", typing: true, loading: true, time: new Date() });
    messages.value.push(resultMsg);

    if (confirmed) {
      try { JSON.parse(approval.functionArgsStr); }
      catch (e) {
        resultMsg.typing = false; resultMsg.loading = false;
        resultMsg.text = `❌ JSON không hợp lệ: ${e.message}`;
        loading.value = false; return;
      }
    }

    try {
      const res = await axios.post("/api/AIChat/confirm", {
        confirmed,
        functionName: approval.functionName,
        functionArgs: approval.functionArgsStr,
      });
      resultMsg.loading = false;
      const resultText = res.data.message || (confirmed ? "✅ Thực thi thành công" : "🚫 Đã hủy lệnh");

      // KEY FIX: Khi confirm hàm báo cáo → tạo HTML → trigger navigation
      if (confirmed && isReportFunction(approval.functionName)) {
        await typeText(resultMsg, resultText + "\n\n📊 Đang tạo báo cáo HTML...");

        const reportPrompt = lastUserPrompt || `Báo cáo: ${approval.functionName}`;
        let htmlReport = null;

        // Gọi /api/AIChat/report để tạo HTML báo cáo đẹp từ dữ liệu thực
        try {
          const reportRes = await axios.post("/api/AIChat/report", {
            prompt: reportPrompt,
            functionName: approval.functionName   // Giúp backend chọn đúng loại báo cáo
          });
          if (reportRes.data?.htmlReport) {
            htmlReport = reportRes.data.htmlReport;
          }
        } catch (err) {
          console.warn("Report API failed, using fallback:", err.message);
        }


        // Nếu không có API report, build HTML từ text
        if (!htmlReport) {
          htmlReport = buildHtmlFromResult(approval.functionName, resultText, reportPrompt);
        }

        localStorage.setItem("pos36_ai_report_html", htmlReport);
        localStorage.setItem("pos36_ai_report_prompt", reportPrompt);
        pendingNavigation.value = {
          html: htmlReport, prompt: reportPrompt,
          route: "/super-admin/ai-report", label: "Trang Báo Cáo AI",
        };
      } else {
        await typeText(resultMsg, resultText);
      }
    } catch (e) {
      resultMsg.typing = false; resultMsg.loading = false;
      resultMsg.text = `❌ ${e.message}`;
    } finally {
      loading.value = false;
    }
  };

  const clearPendingNavigation = () => { pendingNavigation.value = null; };
  const clearMessages = () => { messages.value = []; lastUsage.value = null; };

  return {
    messages, loading, models, selectedModel, mode, theme,
    lastUsage, isConnected, currentTheme, THEMES,
    pendingNavigation, clearPendingNavigation,
    fetchModels, sendMessage, confirmAction, clearMessages,
  };
}
