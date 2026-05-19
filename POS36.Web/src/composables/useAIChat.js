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
const mode = ref(localStorage.getItem("pos36_ai_chat_mode") || "agent"); // agent | chat
const theme = ref(localStorage.getItem("pos36_ai_theme") || "dark");
const sessionId = `sa-${Date.now()}`;
const lastUsage = ref(null);
const isConnected = ref(true);

// Persist settings
watch(selectedModel, (val) => localStorage.setItem("pos36_ai_model", val));
watch(mode, (val) => localStorage.setItem("pos36_ai_chat_mode", val));
watch(theme, (val) => localStorage.setItem("pos36_ai_theme", val));

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
        { id: "gemini-1.5-flash", displayName: "Gemini 1.5 Flash", description: "Nhanh, đa năng (Khuyên dùng)", isDefault: true },
        { id: "gemini-1.5-pro",   displayName: "Gemini 1.5 Pro",   description: "Thông minh hơn" }
      ];
      if (!models.value.some(m => m.id === selectedModel.value)) selectedModel.value = "gemini-1.5-flash";
    }
  };

  const typeText = async (msgObj, fullText) => {
    msgObj.loading = false;
    msgObj.text = "";
    const SPEED = 15; // ms per char
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

    messages.value.push({ role: "user", text: prompt, time: new Date() });

    // Using `reactive` to ensure nested properties trigger re-renders instantly
    const aiMsg = reactive({ 
      role: "ai", 
      text: "Đang suy nghĩ...", 
      typing: true, 
      loading: true,
      time: new Date(), 
      usage: null, 
      approval: null 
    });
    messages.value.push(aiMsg);

    try {
      const res = await axios.post("/api/AIChat/chat", {
        prompt,
        sessionId,
        modelId: selectedModel.value,
        mode: mode.value,
      });
      const d = res.data;
      lastUsage.value = d.usage;
      aiMsg.usage = d.usage;
      aiMsg.loading = false;

      if (d.requiresAction) {
        aiMsg.typing = false;
        aiMsg.text = `⚡ AI muốn thực thi lệnh hệ thống: **${d.functionName}**`;
        
        let argsStr = d.functionArgs;
        try { argsStr = JSON.stringify(JSON.parse(d.functionArgs), null, 2); } catch {}

        aiMsg.approval = {
          functionName: d.functionName,
          functionArgsStr: argsStr, // Editable
          riskLevel: d.riskLevel,
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
        aiMsg.text = `❌ **Lỗi Quota:** Model hiện tại đã hết lượt dùng. Hãy chọn \`Gemini 1.5 Flash\`.`;
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
    
    // Validate JSON if confirmed
    if (confirmed) {
      try {
        JSON.parse(approval.functionArgsStr);
      } catch (e) {
        resultMsg.typing = false;
        resultMsg.loading = false;
        resultMsg.text = `❌ Lỗi: Cấu trúc JSON tham số không hợp lệ.\n\nChi tiết: ${e.message}`;
        loading.value = false;
        return;
      }
    }

    try {
      const res = await axios.post("/api/AIChat/confirm", {
        confirmed,
        functionName: approval.functionName,
        functionArgs: approval.functionArgsStr,
      });
      resultMsg.loading = false;
      await typeText(resultMsg, res.data.message || (confirmed ? "✅ Thực thi thành công" : "🚫 Đã hủy lệnh"));
    } catch (e) {
      resultMsg.typing = false;
      resultMsg.loading = false;
      resultMsg.text = `❌ ${e.message}`;
    } finally {
      loading.value = false;
    }
  };

  const clearMessages = () => { messages.value = []; lastUsage.value = null; };

  return {
    messages, loading, models, selectedModel, mode, theme,
    lastUsage, isConnected, currentTheme, THEMES,
    fetchModels, sendMessage, confirmAction, clearMessages,
  };
}
