<template>
  <div
    class="kitchen-container bg-dark text-white vh-100 d-flex flex-column font-monospace"
  >
    <div
      class="p-3 bg-black d-flex justify-content-between align-items-center border-bottom border-secondary shadow"
    >
      <div class="d-flex align-items-center gap-4">
        <h3 class="mb-0 fw-bold text-danger">
          <i class="bi bi-fire me-2"></i> BẾP TRUNG TÂM
        </h3>
        <div class="btn-group" role="group">
          <button
            @click="activeTab = 'pending'"
            class="btn fw-bold"
            :class="
              activeTab === 'pending'
                ? 'btn-danger'
                : 'btn-outline-secondary text-light'
            "
          >
            ĐANG CHỜ ({{ pendingItems.length }})
          </button>
          <button
            @click="activeTab = 'history'"
            class="btn fw-bold"
            :class="
              activeTab === 'history'
                ? 'btn-success'
                : 'btn-outline-secondary text-light'
            "
          >
            LỊCH SỬ ({{ historyItems.length }})
          </button>
        </div>
      </div>
      <div class="d-flex align-items-center gap-4">
        <div class="fs-4 fw-bold text-warning">
          <i class="bi bi-clock"></i> {{ currentTime }}
        </div>
        <button @click="logout" class="btn btn-outline-light btn-sm">
          <i class="bi bi-power"></i>
        </button>
      </div>
    </div>

    <div class="flex-grow-1 p-4 overflow-auto bg-dark">
      <div v-if="loading" class="text-center mt-5">
        <div
          class="spinner-border text-danger"
          style="width: 3rem; height: 3rem"
        ></div>
      </div>

      <div v-else-if="activeTab === 'pending'" class="row g-3">
        <div
          v-for="item in pendingItems"
          :key="item.chiTietId"
          class="col-xl-3 col-lg-4 col-md-6 fade-in"
        >
          <div
            class="card bg-secondary text-white border-0 shadow-lg h-100 ticket-card"
          >
            <div
              class="card-header bg-black bg-opacity-50 d-flex justify-content-between align-items-center py-2 border-0"
            >
              <h4 class="mb-0 fw-bold text-warning">{{ item.tenBan }}</h4>
              <span
                class="badge bg-danger fs-6 pulse"
                v-if="item.thoiGianCho > 15"
                >{{ item.thoiGianCho }} phút</span
              >
              <span class="badge bg-info fs-6 text-dark" v-else
                >{{ item.thoiGianCho }} phút</span
              >
            </div>
            <div class="card-body">
              <div class="d-flex align-items-start gap-3">
                <div
                  class="fw-bold text-center border border-2 border-white rounded p-2 bg-dark"
                  style="min-width: 60px; font-size: 1.8rem"
                >
                  {{ item.soLuong }}
                </div>
                <div>
                  <h4 class="fw-bold mb-1 lh-sm">{{ item.tenMon }}</h4>
                  <p
                    class="text-warning fst-italic mt-2 mb-0"
                    v-if="item.ghiChu"
                  >
                    <i class="bi bi-exclamation-triangle-fill"></i> Ghi chú:
                    {{ item.ghiChu }}
                  </p>
                </div>
              </div>
            </div>
            <div class="card-footer border-0 bg-transparent p-3 pt-0">
              <button
                @click="markAsDone(item)"
                class="btn btn-success w-100 fw-bold fs-4 py-2 shadow"
              >
                <i class="bi bi-check2-all"></i> ĐÃ XONG
              </button>
            </div>
          </div>
        </div>
        <div
          v-if="pendingItems.length === 0"
          class="col-12 text-center text-muted mt-5"
        >
          <i class="bi bi-cup-hot display-1 opacity-50"></i>
          <h2 class="mt-4 fw-bold">Bếp đang rảnh rỗi!</h2>
        </div>
      </div>

      <div v-else-if="activeTab === 'history'" class="row g-3">
        <div class="col-12">
          <div class="table-responsive bg-secondary rounded-3 p-3 shadow">
            <table class="table table-dark table-hover mb-0">
              <thead>
                <tr class="text-muted">
                  <th>Thời gian xong</th>
                  <th>Bàn</th>
                  <th>Tên món</th>
                  <th>SL</th>
                  <th>Ghi chú</th>
                  <th>Trạng thái</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="h in historyItems" :key="h.chiTietId">
                  <td class="text-info fw-bold">{{ h.thoiGianXong }}</td>
                  <td class="text-warning fw-bold">{{ h.tenBan }}</td>
                  <td>{{ h.tenMon }}</td>
                  <td class="fs-5 fw-bold">{{ h.soLuong }}</td>
                  <td class="fst-italic text-secondary">
                    {{ h.ghiChu || "-" }}
                  </td>
                  <td>
                    <span class="badge bg-success"
                      ><i class="bi bi-check-circle"></i> Đã trả món</span
                    >
                  </td>
                </tr>
                <tr v-if="historyItems.length === 0">
                  <td colspan="6" class="text-center text-muted py-4">
                    Chưa có món nào hoàn thành.
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, inject } from "vue";
import { useRouter } from "vue-router";
import axios from "axios";
import * as signalR from "@microsoft/signalr";
import { globalState } from "../store";

const router = useRouter();
const swal = inject("$swal");
const backendUrl = "http://localhost:5198";

const activeTab = ref("pending");
const pendingItems = ref([]);
const historyItems = ref([]);
const loading = ref(true);
const currentTime = ref("");

setInterval(() => {
  currentTime.value = new Date().toLocaleTimeString("vi-VN");
}, 1000);

const connection = new signalR.HubConnectionBuilder()
  .withUrl(`${backendUrl}/kitchenHub`)
  .withAutomaticReconnect()
  .build();

// --- 1. SỬA HÀM LẤY MÓN ĐỂ NHẬN ID CHI NHÁNH ---
const fetchPendingItems = async (branchId) => {
  loading.value = true;
  try {
    const res = await axios.get(
      `/api/HoaDon/bep/danh-sach?chiNhanhId=${branchId}`,
    );
    pendingItems.value = res.data;
  } catch (e) {
    console.error(e);
  } finally {
    loading.value = false;
  }
};

// --- 2. HÀM TỰ ĐỘNG TÌM ĐÚNG CHI NHÁNH ---
const getBranchIdAndFetch = async () => {
  let branchId =
    globalState.value.activeBranchId ||
    localStorage.getItem("pos36_active_branch");

  if (!branchId || branchId === "null") {
    try {
      const res = await axios.get("/api/ChiNhanh");
      if (res.data && res.data.length > 0) {
        branchId = res.data[0].id;
        globalState.value.activeBranchId = branchId;
        localStorage.setItem("pos36_active_branch", branchId);
      }
    } catch (e) {
      console.error(e);
    }
  }

  if (branchId) {
    globalState.value.activeBranchId = branchId;
    await fetchPendingItems(branchId); // Gọi đúng tên hàm fetchPendingItems
  } else {
    loading.value = false;
  }
};

onMounted(async () => {
  try {
    await connection.start();
    connection.on("CoDonHangMoi", () => {
      swal.fire({
        toast: true,
        position: "top",
        title: "🔔 CÓ MÓN MỚI!",
        background: "#dc3545",
        color: "#fff",
        showConfirmButton: false,
        timer: 2000,
      });
      // Gọi đúng hàm kèm ID
      if (globalState.value.activeBranchId) {
        fetchPendingItems(globalState.value.activeBranchId);
      }
    });
  } catch (err) {
    console.error(err);
  }

  // --- 3. CHẠY HÀM NÀY LÚC VỪA MỞ TRANG BẾP LÊN ---
  await getBranchIdAndFetch();

  // Load lại đều đặn mỗi 30s
  setInterval(() => {
    if (globalState.value.activeBranchId) {
      fetchPendingItems(globalState.value.activeBranchId);
    }
  }, 30000);
});

onUnmounted(() => {
  connection.stop();
});

const markAsDone = async (item) => {
  try {
    await axios.put(`/api/HoaDon/bep/xong/${item.chiTietId}`);

    item.thoiGianXong = new Date().toLocaleTimeString("vi-VN");
    historyItems.value.unshift(item);
    pendingItems.value = pendingItems.value.filter(
      (i) => i.chiTietId !== item.chiTietId,
    );
  } catch (e) {
    swal.fire("Lỗi", "Không thể cập nhật trạng thái", "error");
  }
};

const logout = () => {
  localStorage.clear();
  window.location.href = "/login"; // Dùng window.location.href để xóa sạch RAM Vue
};
</script>

<style scoped>
.ticket-card {
  border-radius: 12px;
  transition: transform 0.2s;
}
.ticket-card:hover {
  transform: translateY(-5px);
}
.fade-in {
  animation: fadeIn 0.3s ease-in-out;
}
@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
.pulse {
  animation: pulse-red 1.5s infinite;
}
@keyframes pulse-red {
  0% {
    transform: scale(1);
    box-shadow: 0 0 0 0 rgba(220, 53, 69, 0.7);
  }
  70% {
    transform: scale(1.1);
    box-shadow: 0 0 0 10px rgba(220, 53, 69, 0);
  }
  100% {
    transform: scale(1);
    box-shadow: 0 0 0 0 rgba(220, 53, 69, 0);
  }
}
</style>
