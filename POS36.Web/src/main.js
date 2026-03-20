import { createApp } from "vue";
import App from "./App.vue";
import router from "./router";
import "bootstrap/dist/css/bootstrap.min.css";
import "bootstrap-icons/font/bootstrap-icons.css";
import "bootstrap/dist/js/bootstrap.bundle.min.js";
import "sweetalert2/dist/sweetalert2.min.css";
import Swal from "sweetalert2";

// THÊM ĐOẠN NÀY: Cấu hình Axios tự động kẹp Token vào mọi Request
import axios from "axios";
axios.defaults.baseURL = "http://localhost:5198"; // Đặt URL gốc của Backend
axios.interceptors.request.use((config) => {
  const token = localStorage.getItem("pos36_token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
},error => {
    return Promise.reject(error);});

const app = createApp(App);
app.provide("$swal", Swal);
app.use(router);
app.mount("#app");
