import { defineConfig } from "vite";
import vue from "@vitejs/plugin-vue";

// https://vite.dev/config/
export default defineConfig({
  plugins: [vue()],
  server: {
    proxy: {
      // Proxy tất cả request bắt đầu bằng /api sang API Backend (http://localhost:5098)
      "/api": {
        target: "http://localhost:5098",
        changeOrigin: true,
        // Không dùng rewrite xóa /api vì backend của bạn (AuthController) dùng route [Route("api/[controller]")]
      },
    },
  },
});
