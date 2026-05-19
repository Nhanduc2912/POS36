import { ref } from "vue";
import axios from "axios";

const THEMES = {
  dark:    { bg: "#0d1117", surface: "#161b22", text: "#c9d1d9", muted: "#8b949e", accent: "#f59e0b", border: "#30363d", user: "#1f6feb" },
  dracula: { bg: "#282a36", surface: "#44475a", text: "#f8f8f2", muted: "#6272a4", accent: "#bd93f9", border: "#6272a4", user: "#ff79c6" },
  monokai: { bg: "#272822", surface: "#3e3d32", text: "#f8f8f2", muted: "#75715e", accent: "#a6e22e", border: "#49483e", user: "#66d9ef" },
  nord:    { bg: "#2e3440", surface: "#3b4252", text: "#eceff4", muted: "#4c566a", accent: "#88c0d0", border: "#4c566a", user: "#5e81ac" },
  light:   { bg: "#f6f8fa", surface: "#ffffff", text: "#1f2328", muted: "#656d76", accent: "#0969da", border: "#d0d7de", user: "#0969da" },
};

export function useAIChat() {
  const messages = ref([]);
  const loading = ref(false);
  const models = ref([]);
  const selectedModel = ref("gemini-1.5-flash");
  const mode = ref("agent"); // agent | chat
  const theme = ref("dark");
  const sessionId = `sa-${Date.now()}`;
  const lastUsage = ref(null);
  const isConnected = ref(true);

  const currentTheme = () => THEMES[theme.value] || THEMES.dark;

  // Fetch models from API
  const fetchModels = async () => {
    try {
      const res = await axios.get("/api/AIChat/models");
      models.value = res.data;
      const def = res.data.find((m) => m.isDefault);
      if (def) selectedModel.value = def.id;
    } catch {
      models.value = [
        { id: "gemini-1.5-flash", displayName: "Gemini 1.5 Flash", description: "Nhanh, đa năng", isDefault: true },
        { id: "gemini-1.5-pro",   displayName: "Gemini 1.5 Pro",   description: "Thông minh hơn" },
        { id: "gemini-2.0-flash", displayName: "Gemini 2.0 Flash", description: "Thế hệ mới" },
      ];
    }
  };

  // Typewriter effect
  const typeText = async (msgObj, fullText) => {
    msgObj.text = "";
    const SPEED = 8; // ms per char
    for (const ch of fullText) {
      msgObj.text += ch;
      await new Promise((r) => setTimeout(r, SPEED));
    }
    msgObj.typing = false;
  };

  const sendMessage = async (prompt, onApproval) => {
    if (!prompt.trim() || loading.value) return;
    loading.value = true;
    isConnected.value = true;

    messages.value.push({ role: "user", text: prompt, time: new Date() });

    const aiMsg = { role: "ai", text: "", typing: true, time: new Date(), usage: null, approval: null };
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

      if (d.requiresAction) {
        aiMsg.typing = false;
        aiMsg.text = `⚡ AI muốn thực thi: **${d.functionName}**`;
        aiMsg.approval = {
          functionName: d.functionName,
          functionArgs: d.functionArgs,
          riskLevel: d.riskLevel,
        };
        if (onApproval) onApproval(aiMsg.approval);
      } else {
        await typeText(aiMsg, d.text || "(Không có phản hồi)");
      }
    } catch (e) {
      isConnected.value = false;
      aiMsg.typing = false;
      aiMsg.text = `❌ Lỗi: ${e.response?.data?.error || e.message}`;
    } finally {
      loading.value = false;
    }
  };

  const confirmAction = async (approval, confirmed) => {
    loading.value = true;
    const resultMsg = { role: "system", text: "", typing: true, time: new Date() };
    messages.value.push(resultMsg);
    try {
      const res = await axios.post("/api/AIChat/confirm", {
        confirmed,
        functionName: approval.functionName,
        functionArgs: approval.functionArgs,
      });
      const text = res.data.message || (confirmed ? "✅ Thực thi thành công" : "🚫 Đã hủy lệnh");
      await typeText(resultMsg, text);
    } catch (e) {
      resultMsg.typing = false;
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
