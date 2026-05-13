// src/composables/useSignalR.js
// Shared composable cho KitchenHub SignalR connection
// Dùng trong: PosView.vue, OrderView.vue, KitchenView.vue

import * as signalR from "@microsoft/signalr";
import { ref } from "vue";

export function useSignalR() {
  const backendUrl = import.meta.env.VITE_BACKEND_URL || "http://localhost:5098";

  const connectionStatus = ref("disconnected"); // 'connected' | 'connecting' | 'disconnected'

  const connection = new signalR.HubConnectionBuilder()
    .withUrl(`${backendUrl}/kitchenHub`, {
      // Gửi token nếu có (cho các hub cần auth sau này)
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

  const startConnection = async () => {
    connectionStatus.value = "connecting";
    try {
      await connection.start();
      connectionStatus.value = "connected";
    } catch (err) {
      connectionStatus.value = "disconnected";
      console.error("[SignalR] Lỗi kết nối:", err);
    }
  };

  const stopConnection = async () => {
    try {
      await connection.stop();
    } catch (e) {
      // ignore
    }
    connectionStatus.value = "disconnected";
  };

  return {
    connection,
    connectionStatus,
    backendUrl,
    startConnection,
    stopConnection,
  };
}
