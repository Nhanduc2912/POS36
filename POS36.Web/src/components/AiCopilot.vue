<template>
  <div class="ai-chat-widget">
    <button
      @click="toggleChat"
      class="btn btn-primary rounded-circle shadow-lg d-flex align-items-center justify-content-center ai-fab"
      :class="isChatOpen ? 'bg-danger border-danger' : ''"
    >
      <i class="bi" :class="isChatOpen ? 'bi-x-lg' : 'bi-robot'"></i>
    </button>

    <div
      v-if="isChatOpen"
      class="card shadow-lg border-0 rounded-4 ai-chat-window fade-in-up"
    >
      <div
        class="card-header bg-primary text-white p-3 rounded-top-4 d-flex align-items-center"
      >
        <i class="bi bi-robot fs-4 me-2"></i>
        <div>
          <h6 class="mb-0 fw-bold">POS36 Copilot</h6>
          <small style="font-size: 0.7rem; opacity: 0.8"
            >Powered by Google Gemini</small
          >
        </div>
      </div>

      <div
        class="card-body p-3 overflow-auto custom-scrollbar bg-light d-flex flex-column gap-3"
        ref="chatBodyRef"
        style="height: 380px"
      >
        <div
          v-for="(msg, index) in chatMessages"
          :key="index"
          class="d-flex"
          :class="
            msg.role === 'user'
              ? 'justify-content-end'
              : 'justify-content-start'
          "
        >
          <div v-if="msg.role === 'ai'" class="me-2 mt-auto mb-auto">
            <div
              class="bg-white rounded-circle shadow-sm d-flex align-items-center justify-content-center text-primary"
              style="width: 30px; height: 30px"
            >
              <i class="bi bi-robot small"></i>
            </div>
          </div>

          <div 
            class="p-2 px-3 rounded-4 shadow-sm" 
            style="max-width: 85%; font-size: 0.9rem; line-height: 1.5;"
            :class="msg.role === 'user' ? 'bg-primary text-white text-end' : 'bg-white text-dark border'"
            v-html="formatMessage(msg.text)"
          >
          </div>
        </div>

        <div v-if="isAiTyping" class="d-flex justify-content-start">
          <div class="me-2 mt-auto mb-auto">
            <div
              class="bg-white rounded-circle shadow-sm d-flex align-items-center justify-content-center text-primary"
              style="width: 30px; height: 30px"
            >
              <i class="bi bi-robot small"></i>
            </div>
          </div>
          <div
            class="p-2 px-3 rounded-4 bg-white text-muted border shadow-sm d-flex align-items-center gap-1"
            style="font-size: 0.9rem"
          >
            Đang suy nghĩ
            <span
              class="spinner-grow text-primary"
              style="width: 0.5rem; height: 0.5rem"
              role="status"
            ></span>
          </div>
        </div>
      </div>

      <div class="card-footer bg-white p-2 rounded-bottom-4 border-top-0">
        <form
          @submit.prevent="sendAiMessage"
          class="input-group shadow-sm rounded-pill"
        >
          <input
            type="text"
            v-model="userMessage"
            class="form-control border-0 bg-light rounded-start-pill ps-4 py-2"
            placeholder="Hỏi AI cách dùng..."
            :disabled="isAiTyping"
          />
          <button
            type="submit"
            class="btn btn-light bg-light rounded-end-pill text-primary pe-3"
            :disabled="isAiTyping || !userMessage.trim()"
          >
            <i class="bi bi-send-fill"></i>
          </button>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, nextTick } from "vue";

// Nhớ sửa lại cổng 5198 cho đúng với backend C# của em nhé
const backendUrl = "http://localhost:5198";

const isChatOpen = ref(false);
const userMessage = ref("");
const isAiTyping = ref(false);
const chatBodyRef = ref(null);
const chatMessages = ref([
  {
    role: "ai",
    text: "Chào sếp! Mình là POS36 Copilot. Sếp cần hướng dẫn tính năng gì ạ?",
  },
]);

const toggleChat = () => {
  isChatOpen.value = !isChatOpen.value;
};

const scrollToBottom = () => {
  nextTick(() => {
    if (chatBodyRef.value) {
      chatBodyRef.value.scrollTop = chatBodyRef.value.scrollHeight;
    }
  });
};

const sendAiMessage = async () => {
  if (!userMessage.value.trim()) return;

  const question = userMessage.value;
  chatMessages.value.push({ role: "user", text: question });
  userMessage.value = "";
  isAiTyping.value = true;
  scrollToBottom();

  try {
    const token = localStorage.getItem("token") || "";
    // Lấy chức vụ thực tế của người đang đăng nhập
    const userRole = localStorage.getItem("pos36_role") || "ThuNgan";

    const response = await fetch(`${backendUrl}/api/AIChat/ask`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      // NHÉT THÊM ROLE VÀO ĐỂ C# BIẾT ĐƯỜNG ĐỌC FILE
      body: JSON.stringify({
        Question: question,
        Role: userRole,
      }),
    });

    if (!response.ok) throw new Error("Lỗi mạng");

    const data = await response.json();
    chatMessages.value.push({ role: "ai", text: data.answer });
  } catch (e) {
    chatMessages.value.push({
      role: "ai",
      text: "Xin lỗi sếp, đường truyền AI đang bị lỗi. Sếp thử lại sau nhé!",
    });
  } finally {
    isAiTyping.value = false;
    scrollToBottom();
  }
};

// HÀM DỊCH MARKDOWN CỦA AI THÀNH HTML (In đậm & Xuống dòng)
const formatMessage = (text) => {
  if (!text) return "";
  // 1. Biến **chữ** thành thẻ <b>chữ</b>
  let html = text.replace(/\*\*(.*?)\*\*/g, "<b>$1</b>");
  // 2. Biến dấu * (danh sách) thành dấu gạch đầu dòng
  html = html.replace(/\* (.*?)\n/g, "• $1<br/>");
  // 3. Biến phím Enter (\n) thành thẻ <br/>
  html = html.replace(/\n/g, "<br/>");
  return html;
};
</script>

<style scoped>
.ai-chat-widget {
  position: fixed;
  bottom: 20px;
  right: 20px;
  z-index: 1060; /* Cho nó nổi lên trên cả các modal */
  display: flex;
  flex-direction: column;
  align-items: flex-end;
}

.ai-fab {
  width: 55px;
  height: 55px;
  font-size: 1.5rem;
  transition: transform 0.2s;
}
.ai-fab:active {
  transform: scale(0.9);
}

.ai-chat-window {
  width: 360px;
  margin-bottom: 15px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2) !important;
}

.fade-in-up {
  animation: fadeInUp 0.3s cubic-bezier(0.165, 0.84, 0.44, 1);
}
@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(20px) scale(0.95);
  }
  to {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}

.custom-scrollbar::-webkit-scrollbar {
  width: 5px;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background: #cbd5e1;
  border-radius: 10px;
}
</style>
