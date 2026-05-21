import { createApp } from "vue";
import App from "./App.vue";
import router from "./router";
import "bootstrap/dist/css/bootstrap.min.css";
import "bootstrap-icons/font/bootstrap-icons.css";
import "bootstrap/dist/js/bootstrap.bundle.min.js";
import "sweetalert2/dist/sweetalert2.min.css";
import Swal from "sweetalert2";
import { globalState } from "./store"; // Nhớ import globalState

// THÊM ĐOẠN NÀY: Cấu hình Axios tự động kẹp Token vào mọi Request
import axios from "axios";
// Sửa lỗi CORS: Không gọi thẳng đến localhost:5098 nữa, mà gọi qua proxy của Vite
// axios.defaults.baseURL = "http://localhost:5098"; 
axios.interceptors.request.use(
  (config) => {
    // Bỏ qua global loader cho các endpoint AI (có loading riêng trong AITerminal)
    const isAiEndpoint = config.url && (
      config.url.includes("/api/AIChat/") ||
      config.url.includes("/api/ai")
    );
    if (!isAiEndpoint) {
      globalState.value.isLoading = true;
    }

    // Gắn Token vào Header
    const token = localStorage.getItem("pos36_token");
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    globalState.value.isLoading = false;
    return Promise.reject(error);
  },
);

// ----------------------------------------------------
// TRẠM KIỂM SOÁT 2: KHI BACKEND TRẢ DỮ LIỆU VỀ
// ----------------------------------------------------
axios.interceptors.response.use(
  (response) => {
    // Lấy được data -> Tắt Loader
    globalState.value.isLoading = false;
    return response;
  },
  (error) => {
    // Lỗi cũng phải tắt Loader không là nó xoay mãi
    globalState.value.isLoading = false;

    // NẾU BACKEND BÁO 401 (HẾT HẠN TOKEN HOẶC THIẾU TOKEN) -> ĐÁ VỀ TRANG LOGIN
    if (error.response && error.response.status === 401) {
      localStorage.clear();
      window.location.href = "/login";
    }
    return Promise.reject(error);
  },
);

const app = createApp(App);
app.provide("$swal", Swal);
app.use(router);
app.mount("#app");
