// src/composables/useSignalR.js
// Shared SINGLETON composable cho KitchenHub SignalR connection
// Dùng trong: PosView.vue, OrderView.vue, KitchenView.vue
// ====================================================================
// FIX: Trước đây mỗi component tạo 1 connection mới → 3 tabs = 3 connections
// Giờ: 1 connection duy nhất được share giữa tất cả components
// ====================================================================

import * as signalR from "@microsoft/signalr";
import { ref } from "vue";

let rawBackendUrl = import.meta.env.VITE_BACKEND_URL || "http://localhost:5098";

// Tự động chuyển đổi localhost thành IP LAN thực tế của máy chủ nếu đang kết nối từ xa
if (rawBackendUrl.includes("localhost") || rawBackendUrl.includes("127.0.0.1")) {
  const currentHostname = window.location.hostname;
  if (currentHostname !== "localhost" && currentHostname !== "127.0.0.1") {
    rawBackendUrl = rawBackendUrl
      .replace("localhost", currentHostname)
      .replace("127.0.0.1", currentHostname);
  }
}
const backendUrl = rawBackendUrl;

const connectionStatus = ref("disconnected"); // 'connected' | 'connecting' | 'disconnected'

// ========== SINGLETON CONNECTION — tạo 1 lần duy nhất ==========
const connection = new signalR.HubConnectionBuilder()
  .withUrl(`${backendUrl}/kitchenHub`, {
    // Gửi JWT token để Hub xác thực và join đúng Group theo CuaHangId
    accessTokenFactory: () => localStorage.getItem("pos36_token") || "",
  })
  .withAutomaticReconnect({
    nextRetryDelayInMilliseconds: (retryContext) => {
      // Retry: 0s, 2s, 5s, 10s, 30s, rồi mỗi 60s
      const delays = [0, 2000, 5000, 10000, 30000];
      return delays[retryContext.previousRetryCount] ?? 60000;
    },
  })
  .configureLogging(signalR.LogLevel.Warning)
  .build();

// Theo dõi trạng thái kết nối
connection.onreconnecting(() => {
  connectionStatus.value = "connecting";
});
connection.onreconnected(() => {
  connectionStatus.value = "connected";
});
connection.onclose(() => {
  connectionStatus.value = "disconnected";
});

// Track xem đã start chưa để tránh start nhiều lần
let _startPromise = null;

const startConnection = async () => {
  // Nếu đã kết nối rồi thì bỏ qua
  if (connection.state === signalR.HubConnectionState.Connected) {
    connectionStatus.value = "connected";
    return;
  }
  // Nếu đang start thì chờ promise cũ
  if (_startPromise) return _startPromise;

  connectionStatus.value = "connecting";
  _startPromise = connection.start()
    .then(() => {
      connectionStatus.value = "connected";
      _startPromise = null;
    })
    .catch((err) => {
      connectionStatus.value = "disconnected";
      _startPromise = null;
      console.error("[SignalR] Lỗi kết nối:", err);
    });

  return _startPromise;
};

const stopConnection = async () => {
  // Không thực sự stop vì connection được share
  // Chỉ stop khi logout (trang bị reload hoàn toàn)
  // Nếu cần force stop, gọi connection.stop() trực tiếp
};

export function useSignalR() {
  return {
    connection,
    connectionStatus,
    backendUrl,
    startConnection,
    stopConnection,
  };
}
