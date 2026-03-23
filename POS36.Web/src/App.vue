<template>
  <div v-if="globalState.isLoading" class="global-loader-overlay">
    <div
      class="d-flex flex-column align-items-center bg-white p-4 rounded-4 shadow-lg"
    >
      <div
        class="spinner-border text-warning"
        style="width: 3rem; height: 3rem; border-width: 0.3rem"
        role="status"
      ></div>
      <h6 class="mt-3 mb-0 fw-bold text-dark">Đang tải dữ liệu...</h6>
    </div>
  </div>

  <router-view></router-view>
</template>

<script setup>
import { globalState } from "./store"; // Import state để lấy cờ isLoading
</script>

<style>
/* CSS CHO MÀN HÌNH LOADING */
.global-loader-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background-color: rgba(0, 0, 0, 0.4); /* Nền đen mờ */
  backdrop-filter: blur(3px); /* Làm nhòe nội dung đằng sau */
  z-index: 99999; /* Đẩy lên lớp cao nhất, che mọi thứ */
  display: flex;
  justify-content: center;
  align-items: center;
}
</style>
