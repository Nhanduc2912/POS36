<script setup>
import { ref } from "vue";
import axios from "axios";
// Lát nữa mình setup Router sau, tạm thời dùng console.log để test
// import { useRouter } from 'vue-router'

const username = ref("");
const password = ref("");
const errorMessage = ref("");
const isLoading = ref(false);

const handleLogin = async () => {
  if (!username.value || !password.value) {
    errorMessage.value = "Vui lòng nhập đầy đủ thông tin!";
    return;
  }

  isLoading.value = true;
  errorMessage.value = "";

  try {
    // Gọi API Đăng nhập của Backend
    const response = await axios.post("http://localhost:5198/api/Auth/login", {
      tenDangNhap: username.value,
      matKhau: password.value,
    });

    // Lấy Token từ Server trả về và lưu vào LocalStorage (Két sắt của trình duyệt)
    const token = response.data.token;
    localStorage.setItem("pos36_token", token);

    alert("Đăng nhập thành công! Đã lấy được Token.");
    console.log("Token của bạn:", token);

    // Chỗ này sau sẽ chuyển hướng sang trang Dashboard
    // router.push('/')
  } catch (error) {
    if (error.response && error.response.status === 400) {
      errorMessage.value = "Sai tên đăng nhập hoặc mật khẩu!";
    } else {
      errorMessage.value =
        "Không thể kết nối đến Server (Nhớ bật Backend nhé!)";
    }
  } finally {
    isLoading.value = false;
  }
};
</script>

<template>
  <div class="login-container">
    <div class="login-box">
      <h2>POS36 LOGIN</h2>
      <p class="subtitle">Hệ thống quản lý bán hàng</p>

      <form @submit.prevent="handleLogin">
        <div class="input-group">
          <label>Tên đăng nhập</label>
          <input
            type="text"
            v-model="username"
            placeholder="Nhập tài khoản (VD: admin_lauduc)"
          />
        </div>

        <div class="input-group">
          <label>Mật khẩu</label>
          <input
            type="password"
            v-model="password"
            placeholder="Nhập mật khẩu (VD: 123456)"
          />
        </div>

        <p v-if="errorMessage" class="error">{{ errorMessage }}</p>

        <button type="submit" :disabled="isLoading">
          {{ isLoading ? "Đang kiểm tra..." : "ĐĂNG NHẬP" }}
        </button>
      </form>
    </div>
  </div>
</template>

<style scoped>
.login-container {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 100vh;
  background-color: #f0f2f5;
  font-family: Arial, sans-serif;
}

.login-box {
  background: white;
  padding: 40px;
  border-radius: 10px;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.1);
  width: 100%;
  max-width: 350px;
  text-align: center;
}

h2 {
  margin-bottom: 5px;
  color: #333;
}
.subtitle {
  color: #888;
  margin-bottom: 25px;
  font-size: 14px;
}

.input-group {
  text-align: left;
  margin-bottom: 15px;
}

.input-group label {
  display: block;
  margin-bottom: 5px;
  font-size: 14px;
  color: #555;
  font-weight: bold;
}

.input-group input {
  width: 100%;
  padding: 10px;
  border: 1px solid #ddd;
  border-radius: 5px;
  box-sizing: border-box;
}

button {
  width: 100%;
  padding: 12px;
  background-color: #1890ff;
  color: white;
  border: none;
  border-radius: 5px;
  font-size: 16px;
  font-weight: bold;
  cursor: pointer;
  margin-top: 10px;
}

button:disabled {
  background-color: #91caff;
  cursor: not-allowed;
}

.error {
  color: #ff4d4f;
  font-size: 14px;
  margin-bottom: 15px;
}
</style>
