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
            class="p-2 px-3 rounded-4 shadow-sm"
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
            v-html="formatMessage(msg.text)"
          ></div>
        </div>

        <div v-if="isLoading" class="d-flex justify-content-start">
          <div
            class="bg-white border text-muted p-2 px-3 rounded-4 shadow-sm rounded-bottom-start-0"
          >
            <span class="spinner-grow spinner-grow-sm text-primary me-1"></span>
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
            :disabled="isLoading"
          />
          <button
            @click="sendMessage"
            class="btn btn-primary rounded-circle ms-2 d-flex align-items-center justify-content-center"
            style="width: 40px; height: 40px"
            :disabled="isLoading || !question.trim()"
          >
            <i class="bi bi-send-fill"></i>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, nextTick } from "vue";

const isOpen = ref(false);
const question = ref("");
const messages = ref([]);
const isLoading = ref(false);
const chatBody = ref(null);
const bubbleRef = ref(null);

// ==========================================
// FIX LỖI 401: GỌI API CHUẨN XÁC
// ==========================================
const backendUrl = "http://localhost:5198"; // Đổi IP LAN vào đây nếu sếp test điện thoại

const sendMessage = async () => {
  if (!question.value.trim()) return;

  const userText = question.value;
  messages.value.push({ role: "user", text: userText });
  question.value = "";
  isLoading.value = true;
  scrollToBottom();

  try {
    // 1. LẤY ĐÚNG TÊN TOKEN TỪ LOCALSTORAGE
    const token =
      localStorage.getItem("pos36_token") ||
      localStorage.getItem("token") ||
      "";
    const userRole = localStorage.getItem("pos36_role") || "ThuNgan";

    // 2. GỬI KÈM TOKEN LÊN C# (BEARER TOKEN) ĐỂ KHÔNG BỊ BÁO LỖI 401
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

    if (!response.ok) throw new Error("Unauthorized hoặc Lỗi mạng");

    const data = await response.json();
    messages.value.push({ role: "ai", text: data.answer });
  } catch (error) {
    messages.value.push({
      role: "ai",
      text: "❌ Đứt cáp quang tới não AI rồi Sếp ạ! Vui lòng thử lại sau.",
    });
  } finally {
    isLoading.value = false;
    scrollToBottom();
  }
};

// ==========================================
// FORMAT MARKDOWN: DỊCH DẤU ** THÀNH CHỮ IN ĐẬM
// ==========================================
const formatMessage = (text) => {
  if (!text) return "";
  let html = text.replace(/\*\*(.*?)\*\*/g, "<b>$1</b>");
  html = html.replace(/\* (.*?)\n/g, "• $1<br/>");
  html = html.replace(/\n/g, "<br/>");
  return html;
};

const scrollToBottom = async () => {
  await nextTick();
  if (chatBody.value) {
    chatBody.value.scrollTop = chatBody.value.scrollHeight;
  }
};

// ==========================================
// VẬT LÝ NÂNG CAO: BONG BÓNG KÉO THẢ & HÍT CẠNH
// ==========================================
const pos = ref({ x: window.innerWidth - 80, y: window.innerHeight - 100 });
const isDragging = ref(false);
let startX, startY, initialX, initialY;
let hasMoved = false;

const startDrag = (e) => {
  isDragging.value = true;
  hasMoved = false; // Đặt lại trạng thái di chuyển

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

  // Nếu di chuyển quá 5px thì mới tính là Đang Kéo (để phân biệt với Click)
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

  // LOGIC CHỌN CẠNH ĐỂ HÍT: Nếu bóng nằm nửa trái màn hình -> Hít lề trái. Nếu nửa phải -> Hít lề phải.
  const screenWidth = window.innerWidth;
  const screenHeight = window.innerHeight;
  const bubbleSize = 60; // Kích thước bong bóng

  if (pos.value.x + bubbleSize / 2 < screenWidth / 2) {
    pos.value.x = 20; // Hít trái
  } else {
    pos.value.x = screenWidth - bubbleSize - 20; // Hít phải
  }

  // Khống chế không cho bay ra khỏi lề trên/dưới màn hình
  if (pos.value.y < 20) pos.value.y = 20;
  if (pos.value.y > screenHeight - bubbleSize - 20)
    pos.value.y = screenHeight - bubbleSize - 20;

  // NẾU KHÔNG KÉO MÀ CHỈ BẤM (Click) -> Mở cửa sổ chat
  if (!hasMoved) {
    isOpen.value = !isOpen.value;
  }
};

// Cập nhật lại vị trí nếu sếp xoay dọc/ngang thiết bị
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
  touch-action: none; /* Khống chế scroll khi chạm trên điện thoại */
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
