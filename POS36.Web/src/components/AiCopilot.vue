<template>
  <div>
    <div
      ref="bubbleRef"
      class="ai-bubble bg-primary text-white shadow-lg d-flex justify-content-center align-items-center cursor-pointer"
      :style="{
        left: pos.x + 'px',
        top: pos.y + 'px',
        transition: isDragging ? 'none' : 'all 0.3s ease',
      }"
      @mousedown="startDrag"
      @touchstart="startDrag"
    >
      <i class="bi bi-robot fs-3"></i>
      <span
        class="position-absolute top-0 start-100 translate-middle p-2 bg-danger border border-light rounded-circle"
      ></span>
    </div>

    <div
      v-if="isOpen"
      class="ai-chat-window shadow-lg d-flex flex-column bg-light rounded-4 overflow-hidden fade-in"
    >
      <div
        class="chat-header bg-primary text-white p-3 d-flex justify-content-between align-items-center"
      >
        <div class="d-flex align-items-center">
          <i class="bi bi-robot fs-4 me-2"></i>
          <div>
            <h6 class="mb-0 fw-bold">POS36 Copilot</h6>
            <small class="opacity-75" style="font-size: 0.75rem"
              >Powered by AI</small
            >
          </div>
        </div>
        <button
          @click="isOpen = false"
          class="btn text-white p-0 fs-4"
          style="line-height: 1"
        >
          <i class="bi bi-x-circle-fill"></i>
        </button>
      </div>

      <div
        class="chat-body flex-grow-1 p-3 overflow-auto d-flex flex-column gap-3"
        ref="chatBody"
      >
        <div v-if="messages.length === 0" class="text-center text-muted mt-4">
          <i class="bi bi-chat-dots fs-1 opacity-25 mb-2 d-block"></i>
          <small
            >Hỏi mình bất cứ điều gì về phần mềm hoặc doanh thu nhé Sếp!</small
          >
        </div>

        <div
          v-for="(msg, index) in messages"
          :key="index"
          class="d-flex"
          :class="
            msg.role === 'user'
              ? 'justify-content-end'
              : 'justify-content-start'
          "
        >
          <div
            class="p-2 px-3 rounded-4 shadow-sm msg-animate"
            style="
              max-width: 85%;
              font-size: 0.9rem;
              line-height: 1.5;
              word-wrap: break-word;
            "
            :class="
              msg.role === 'user'
                ? 'bg-primary text-white rounded-bottom-end-0'
                : 'bg-white text-dark border rounded-bottom-start-0'
            "
            v-html="
              msg.role === 'ai'
                ? formatMessage(msg.displayedText)
                : formatMessage(msg.text)
            "
          ></div>
        </div>

        <div v-if="isLoading" class="d-flex justify-content-start msg-animate">
          <div
            class="bg-white border text-muted p-2 px-3 rounded-4 shadow-sm rounded-bottom-start-0 d-flex align-items-center"
          >
            <span class="spinner-grow spinner-grow-sm text-primary me-2"></span>
            Đang suy nghĩ...
          </div>
        </div>
      </div>

      <div class="chat-footer p-3 bg-white border-top">
        <div class="input-group">
          <input
            type="text"
            class="form-control rounded-pill ps-3 bg-light border-0"
            placeholder="Hỏi AI cách dùng..."
            v-model="question"
            @keyup.enter="sendMessage"
            :disabled="isLoading || isTyping"
          />
          <button
            @click="sendMessage"
            class="btn btn-primary rounded-circle ms-2 d-flex align-items-center justify-content-center"
            style="width: 40px; height: 40px"
            :disabled="isLoading || isTyping || !question.trim()"
          >
            <i class="bi bi-send-fill"></i>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, nextTick } from "vue";

const isOpen = ref(false);
const question = ref("");
const messages = ref([]);
const isLoading = ref(false);
const isTyping = ref(false); // Khóa nút bấm khi AI đang gõ
const chatBody = ref(null);
const bubbleRef = ref(null);

const backendUrl = "http://localhost:5098";

// ==========================================
// THUẬT TOÁN GÕ CHỮ (TYPEWRITER EFFECT)
// ==========================================
const typeWriterEffect = async (messageIndex, fullText) => {
  isTyping.value = true;
  // Chia đoạn văn bản thành các khối nhỏ (mỗi lần gõ 3 ký tự cho mượt và nhanh)
  const chunkSize = 3;
  let currentText = "";

  for (let i = 0; i < fullText.length; i += chunkSize) {
    // Ép vòng lặp chờ 15 mili-giây (Có thể tăng lên để gõ chậm lại)
    await new Promise((resolve) => setTimeout(resolve, 15));

    currentText += fullText.substring(i, i + chunkSize);
    messages.value[messageIndex].displayedText = currentText; // Cập nhật biến hiển thị

    scrollToBottom(); // Vừa gõ vừa cuộn màn hình xuống
  }

  // Đảm bảo kết thúc không bị sót chữ nào
  messages.value[messageIndex].displayedText = fullText;
  isTyping.value = false;
  scrollToBottom();
};

const sendMessage = async () => {
  if (!question.value.trim() || isTyping.value) return;

  const userText = question.value;
  messages.value.push({ role: "user", text: userText });
  question.value = "";
  isLoading.value = true;
  scrollToBottom();

  try {
    const token =
      localStorage.getItem("pos36_token") ||
      localStorage.getItem("token") ||
      "";
    const userRole = localStorage.getItem("pos36_role") || "ThuNgan";

    const response = await fetch(`${backendUrl}/api/AIChat/ask`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify({
        Question: userText,
        Role: userRole,
      }),
    });

    if (!response.ok) throw new Error("Lỗi kết nối");

    const data = await response.json();

    // TẮT LOADING TRƯỚC KHI HIỆU ỨNG GÕ BẮT ĐẦU
    isLoading.value = false;

    // TẠO TIN NHẮN MỚI CỦA AI VỚI NỘI DUNG TRỐNG
    const newAiMessageIndex = messages.value.length;
    messages.value.push({ role: "ai", text: data.answer, displayedText: "" });

    // KÍCH HOẠT HIỆU ỨNG GÕ CHỮ VÀO TIN NHẮN VỪA TẠO
    await typeWriterEffect(newAiMessageIndex, data.answer);
  } catch (error) {
    isLoading.value = false;
    messages.value.push({
      role: "ai",
      text: "❌ Đứt cáp quang tới não AI rồi Sếp ạ!",
      displayedText: "❌ Đứt cáp quang tới não AI rồi Sếp ạ!",
    });
    scrollToBottom();
  }
};

const formatMessage = (text) => {
  if (!text) return "";
  let html = text.replace(/\*\*(.*?)\*\*/g, "<b>$1</b>"); // Chữ đậm
  html = html.replace(/\* (.*?)\n/g, "• $1<br/>"); // Dấu chấm đầu dòng
  html = html.replace(/\n/g, "<br/>"); // Xuống dòng
  return html;
};

const scrollToBottom = async () => {
  await nextTick();
  if (chatBody.value) {
    chatBody.value.scrollTop = chatBody.value.scrollHeight;
  }
};

// ==========================================
// VẬT LÝ KÉO THẢ BONG BÓNG CHAT
// ==========================================
const pos = ref({ x: window.innerWidth - 80, y: window.innerHeight - 100 });
const isDragging = ref(false);
let startX, startY, initialX, initialY;
let hasMoved = false;

const startDrag = (e) => {
  isDragging.value = true;
  hasMoved = false;

  let clientX = e.clientX || (e.touches && e.touches[0].clientX);
  let clientY = e.clientY || (e.touches && e.touches[0].clientY);

  startX = clientX;
  startY = clientY;
  initialX = pos.value.x;
  initialY = pos.value.y;

  document.addEventListener("mousemove", onDrag);
  document.addEventListener("mouseup", stopDrag);
  document.addEventListener("touchmove", onDrag, { passive: false });
  document.addEventListener("touchend", stopDrag);
};

const onDrag = (e) => {
  if (!isDragging.value) return;

  let clientX = e.clientX || (e.touches && e.touches[0].clientX);
  let clientY = e.clientY || (e.touches && e.touches[0].clientY);

  let dx = clientX - startX;
  let dy = clientY - startY;

  if (Math.abs(dx) > 5 || Math.abs(dy) > 5) {
    hasMoved = true;
  }

  pos.value.x = initialX + dx;
  pos.value.y = initialY + dy;
};

const stopDrag = () => {
  isDragging.value = false;

  document.removeEventListener("mousemove", onDrag);
  document.removeEventListener("mouseup", stopDrag);
  document.removeEventListener("touchmove", onDrag);
  document.removeEventListener("touchend", stopDrag);

  const screenWidth = window.innerWidth;
  const screenHeight = window.innerHeight;
  const bubbleSize = 60;

  if (pos.value.x + bubbleSize / 2 < screenWidth / 2) {
    pos.value.x = 20;
  } else {
    pos.value.x = screenWidth - bubbleSize - 20;
  }

  if (pos.value.y < 20) pos.value.y = 20;
  if (pos.value.y > screenHeight - bubbleSize - 20)
    pos.value.y = screenHeight - bubbleSize - 20;

  if (!hasMoved) {
    isOpen.value = !isOpen.value;
  }
};

window.addEventListener("resize", () => {
  pos.value = { x: window.innerWidth - 80, y: window.innerHeight - 100 };
});
</script>

<style scoped>
.ai-bubble {
  position: fixed;
  width: 60px;
  height: 60px;
  border-radius: 50%;
  z-index: 1050;
  user-select: none;
  touch-action: none;
}

.ai-chat-window {
  position: fixed;
  bottom: 90px;
  right: 20px;
  width: 360px;
  height: 500px;
  z-index: 1049;
  display: flex;
  flex-direction: column;
}

@media (max-width: 576px) {
  .ai-chat-window {
    width: calc(100% - 40px);
    right: 20px;
    bottom: 90px;
  }
}

/* ANIMATION HIỆN TỪ TRÊN XUỐNG & MỜ SANG RÕ (Yêu cầu của Sếp) */
.msg-animate {
  animation: slideDownFade 0.4s ease-out forwards;
}

@keyframes slideDownFade {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.fade-in {
  animation: fadeIn 0.3s ease-in-out;
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(20px) scale(0.95);
  }
  to {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}

.cursor-pointer {
  cursor: pointer;
}
</style>
