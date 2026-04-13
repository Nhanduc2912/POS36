<template>
  <div class="kitchen-container bg-dark text-white vh-100 d-flex flex-column font-monospace">

    <!-- ===== HEADER ===== -->
    <div class="px-4 py-2 bg-black d-flex justify-content-between align-items-center border-bottom border-secondary shadow">
      <div class="d-flex align-items-center gap-3">
        <h4 class="mb-0 fw-bold text-danger">
          <i class="bi bi-fire me-2"></i>BẾP TRUNG TÂM
        </h4>
        <!-- Tab pending / history -->
        <div class="btn-group btn-group-sm" role="group">
          <button @click="activeTab = 'pending'" class="btn fw-bold px-3"
            :class="activeTab === 'pending' ? 'btn-danger' : 'btn-outline-secondary text-light'">
            <i class="bi bi-hourglass-split me-1"></i>ĐANG CHỜ
            <span class="badge rounded-pill ms-1" :class="activeTab === 'pending' ? 'bg-white text-danger' : 'bg-danger'">
              {{ totalPendingCount }}
            </span>
          </button>
          <button @click="activeTab = 'history'" class="btn fw-bold px-3"
            :class="activeTab === 'history' ? 'btn-success' : 'btn-outline-secondary text-light'">
            <i class="bi bi-check2-all me-1"></i>LỊCH SỬ ({{ historyItems.length }})
          </button>
        </div>
      </div>

      <div class="d-flex align-items-center gap-3">
        <div class="fs-5 fw-bold text-warning">
          <i class="bi bi-clock me-1"></i>{{ currentTime }}
        </div>
        <!-- Nút chuyển chế độ hiển thị -->
        <div class="btn-group btn-group-sm" v-if="activeTab === 'pending'">
          <button @click="displayMode = 'list'" class="btn px-3"
            :class="displayMode === 'list' ? 'btn-info text-dark fw-bold' : 'btn-outline-secondary text-light'"
            title="Hiển thị danh sách">
            <i class="bi bi-list-ul"></i>
          </button>
          <button @click="displayMode = 'grid'" class="btn px-3"
            :class="displayMode === 'grid' ? 'btn-info text-dark fw-bold' : 'btn-outline-secondary text-light'"
            title="Hiển thị theo bàn">
            <i class="bi bi-grid-3x3-gap"></i>
          </button>
        </div>
        <!-- Nút thiết lập -->
        <button class="btn btn-outline-warning btn-sm fw-bold" @click="showSettings = true" v-if="activeTab === 'pending'">
          <i class="bi bi-gear-fill me-1"></i>Thiết lập
        </button>
        <button @click="logout" class="btn btn-outline-light btn-sm">
          <i class="bi bi-power"></i>
        </button>
      </div>
    </div>

    <!-- ===== MAIN CONTENT ===== -->
    <div class="flex-grow-1 overflow-auto p-3">

      <!-- LOADING -->
      <div v-if="loading" class="text-center mt-5">
        <div class="spinner-border text-danger" style="width:3rem;height:3rem"></div>
        <p class="mt-3 text-muted">Đang tải dữ liệu bếp...</p>
      </div>

      <!-- TAB: ĐANG CHỜ -->
      <template v-else-if="activeTab === 'pending'">

        <!-- RẢNH -->
        <div v-if="groupedByTable.length === 0" class="text-center text-muted mt-5 fade-in">
          <i class="bi bi-cup-hot display-1 opacity-50 d-block mb-3"></i>
          <h3 class="fw-bold">Bếp đang rảnh rỗi!</h3>
          <p class="text-secondary">Tất cả món đã được phục vụ</p>
        </div>

        <!-- ===== CHẾ ĐỘ: DANH SÁCH (list) ===== -->
        <div v-else-if="displayMode === 'list'" class="list-mode fade-in">
          <div v-for="group in groupedByTable" :key="group.banId" class="mb-3">
            <div class="table-group-card rounded-3 overflow-hidden shadow-lg">
              <!-- Header bàn -->
              <div class="d-flex justify-content-between align-items-center px-3 py-2 table-header">
                <div class="d-flex align-items-center gap-2">
                  <i class="bi bi-table fs-5 text-warning"></i>
                  <span class="fw-bold fs-5 text-warning">{{ group.tenBan }}</span>
                  <span class="badge bg-danger rounded-pill">{{ group.items.length }} món</span>
                  <span class="badge" :class="group.maxWait > 15 ? 'bg-danger pulse' : 'bg-info text-dark'">
                    <i class="bi bi-clock me-1"></i>{{ group.maxWait }} phút
                  </span>
                </div>
                <!-- Nút hoàn thành cả bàn (chỉ khi mode = per-table) -->
                <button v-if="completionMode === 'table'"
                  @click="markTableDone(group)"
                  class="btn btn-success btn-sm fw-bold px-3">
                  <i class="bi bi-check2-all me-1"></i>XONG HẾT BÀN
                </button>
                <!-- Nút xong cho những món đã check (chỉ khi mode = per-item) -->
                <button v-if="completionMode === 'item' && getCheckedItems(group.banId).length > 0"
                  @click="markCheckedItemsDone(group.banId)"
                  class="btn btn-success btn-sm fw-bold px-3 fade-in">
                  <i class="bi bi-check2 me-1"></i>XONG ({{ getCheckedItems(group.banId).length }})
                </button>
              </div>

              <!-- Danh sách món trong bàn -->
              <div>
                <div v-for="item in group.items" :key="item.chiTietId"
                  class="list-item d-flex align-items-center gap-3 px-3 py-2 border-top border-dark"
                  :class="{ 'item-checked': isChecked(item.chiTietId) }">

                  <!-- Checkbox (chỉ hiện khi per-item) -->
                  <div v-if="completionMode === 'item'" class="form-check mb-0">
                    <input class="form-check-input kitchen-check" type="checkbox" :id="`chk-${item.chiTietId}`"
                      :checked="isChecked(item.chiTietId)"
                      @change="toggleCheck(item.chiTietId)">
                  </div>

                  <!-- Số lượng -->
                  <div class="qty-badge text-center text-dark fw-bold rounded"
                    :class="item.thoiGianCho > 15 ? 'bg-danger text-white' : 'bg-warning'">
                    {{ item.soLuong }}
                  </div>

                  <!-- Tên món -->
                  <div class="flex-grow-1">
                    <div class="fw-bold text-white fs-6">{{ item.tenMon }}</div>
                    <div v-if="item.ghiChu" class="text-warning small fst-italic">
                      <i class="bi bi-exclamation-triangle-fill me-1"></i>{{ item.ghiChu }}
                    </div>
                  </div>

                  <!-- Thời gian -->
                  <div class="text-end">
                    <span class="badge small" :class="item.thoiGianCho > 15 ? 'bg-danger' : 'bg-secondary'">
                      {{ item.thoiGianCho }} phút
                    </span>
                  </div>

                  <!-- Nút xong cá nhân (chỉ khi per-table và không check) -->
                  <button v-if="completionMode === 'table'"
                    @click="markItemDone(item)"
                    class="btn btn-outline-success btn-sm rounded-circle"
                    title="Xong món này">
                    <i class="bi bi-check2"></i>
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- ===== CHẾ ĐỘ: LƯỚI BÀN (grid) ===== -->
        <div v-else-if="displayMode === 'grid'" class="row g-3 fade-in">
          <div v-for="ban in allTablesForGrid" :key="ban.id" class="col-xl-3 col-lg-4 col-md-6">
            <div class="card border-0 shadow-lg h-100 rounded-3 overflow-hidden"
              :class="ban.hasItems ? 'bg-secondary' : 'bg-dark border border-secondary'">

              <!-- Header bàn -->
              <div class="card-header d-flex justify-content-between align-items-center py-2 border-0"
                :class="ban.hasItems ? 'bg-black bg-opacity-40' : 'bg-black bg-opacity-20'">
                <div class="d-flex align-items-center gap-2">
                  <span class="fw-bold" :class="ban.hasItems ? 'text-warning fs-6' : 'text-muted'">
                    {{ ban.tenBan }}
                  </span>
                  <span v-if="ban.hasItems" class="badge bg-danger rounded-pill">{{ ban.items.length }}</span>
                </div>
                <span v-if="!ban.hasItems" class="badge bg-dark text-muted border border-secondary">Trống</span>
                <span v-else class="badge" :class="ban.maxWait > 15 ? 'bg-danger pulse' : 'bg-info text-dark'">
                  {{ ban.maxWait }}ph
                </span>
              </div>

              <!-- Empty state -->
              <div v-if="!ban.hasItems" class="card-body d-flex align-items-center justify-content-center py-3">
                <p class="text-muted small mb-0 fst-italic">Chưa có món</p>
              </div>

              <!-- Danh sách món trong bàn khi có items -->
              <div v-else class="card-body p-2">
                <div v-for="item in ban.items" :key="item.chiTietId"
                  class="d-flex align-items-center gap-2 py-1 border-bottom border-dark"
                  :class="{ 'text-muted': isChecked(item.chiTietId) }">

                  <!-- Checkbox (per-item mode) -->
                  <input v-if="completionMode === 'item'" type="checkbox"
                    :id="`g-chk-${item.chiTietId}`" class="form-check-input kitchen-check"
                    :checked="isChecked(item.chiTietId)"
                    @change="toggleCheck(item.chiTietId)" />

                  <span class="badge rounded-pill fw-bold"
                    :class="item.thoiGianCho > 15 ? 'bg-danger' : 'bg-warning text-dark'">
                    {{ item.soLuong }}
                  </span>
                  <span class="fw-bold small flex-grow-1 text-truncate" style="max-width:120px">{{ item.tenMon }}</span>

                  <!-- Nút xong (per-table mode) -->
                  <button v-if="completionMode === 'table'"
                    @click="markItemDone(item)"
                    class="btn btn-outline-success btn-sm py-0 px-1" title="Xong">
                    <i class="bi bi-check2"></i>
                  </button>
                </div>
              </div>

              <!-- Footer: Nút xong cả bàn (per-table) hoặc xong đã check (per-item) -->
              <div v-if="ban.hasItems" class="card-footer border-0 p-2 bg-transparent">
                <button v-if="completionMode === 'table'"
                  @click="markTableDone(ban)"
                  class="btn btn-success w-100 fw-bold py-2 shadow">
                  <i class="bi bi-check2-all me-1"></i>XONG HẾT
                </button>
                <button v-if="completionMode === 'item' && getCheckedItems(ban.banId || ban.id).length > 0"
                  @click="markCheckedItemsDone(ban.banId || ban.id)"
                  class="btn btn-success w-100 fw-bold py-2 shadow fade-in">
                  <i class="bi bi-check2 me-1"></i>XONG ({{ getCheckedItems(ban.banId || ban.id).length }})
                </button>
              </div>
            </div>
          </div>
        </div>
      </template>

      <!-- TAB: LỊCH SỬ -->
      <div v-else-if="activeTab === 'history'" class="fade-in">
        <div class="bg-secondary rounded-3 p-3 shadow">
          <div class="table-responsive">
            <table class="table table-dark table-hover mb-0 align-middle">
              <thead>
                <tr class="text-muted small text-uppercase">
                  <th>Thời gian xong</th>
                  <th>Bàn</th>
                  <th>Tên món</th>
                  <th class="text-center">SL</th>
                  <th>Ghi chú</th>
                  <th>Trạng thái</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="h in historyItems" :key="h.chiTietId">
                  <td class="text-info fw-bold small">{{ h.thoiGianXong }}</td>
                  <td class="text-warning fw-bold">{{ h.tenBan }}</td>
                  <td class="fw-bold">{{ h.tenMon }}</td>
                  <td class="text-center"><span class="badge bg-warning text-dark fs-6">{{ h.soLuong }}</span></td>
                  <td class="fst-italic text-secondary small">{{ h.ghiChu || "—" }}</td>
                  <td><span class="badge bg-success"><i class="bi bi-check-circle me-1"></i>Đã trả</span></td>
                </tr>
                <tr v-if="historyItems.length === 0">
                  <td colspan="6" class="text-center text-muted py-5">Chưa có món nào hoàn thành</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>

    <!-- ===== MODAL THIẾT LẬP ===== -->
    <div v-if="showSettings" class="modal-backdrop-custom" @click.self="showSettings = false">
      <div class="settings-modal bg-dark text-white rounded-3 shadow-lg p-4 fade-in" style="width:480px;max-width:95vw">
        <div class="d-flex justify-content-between align-items-center mb-4">
          <h5 class="fw-bold mb-0">
            <i class="bi bi-gear-fill text-warning me-2"></i>Thiết lập Màn hình Bếp
          </h5>
          <button class="btn btn-sm btn-outline-secondary rounded-circle" @click="showSettings = false">
            <i class="bi bi-x-lg"></i>
          </button>
        </div>

        <!-- Chế độ hoàn thành -->
        <div class="mb-4">
          <label class="text-warning fw-bold mb-2 d-block">
            <i class="bi bi-check2-square me-1"></i>Chế độ hoàn thành món
          </label>
          <div class="row g-2">
            <div class="col-6">
              <div class="setting-option rounded-3 p-3 text-center cursor-pointer border border-2"
                :class="completionMode === 'item' ? 'border-success bg-success bg-opacity-25' : 'border-secondary bg-secondary bg-opacity-10'"
                @click="completionMode = 'item'; checkedItems = {}">
                <i class="bi bi-check2-square fs-2 d-block mb-2" :class="completionMode === 'item' ? 'text-success' : 'text-muted'"></i>
                <div class="fw-bold small" :class="completionMode === 'item' ? 'text-success' : 'text-secondary'">Tick từng món</div>
                <div class="text-muted" style="font-size:0.72rem">Chọn checkbox từng món, bấm nút gửi khi xong</div>
              </div>
            </div>
            <div class="col-6">
              <div class="setting-option rounded-3 p-3 text-center cursor-pointer border border-2"
                :class="completionMode === 'table' ? 'border-warning bg-warning bg-opacity-25' : 'border-secondary bg-secondary bg-opacity-10'"
                @click="completionMode = 'table'; checkedItems = {}">
                <i class="bi bi-table fs-2 d-block mb-2" :class="completionMode === 'table' ? 'text-warning' : 'text-muted'"></i>
                <div class="fw-bold small" :class="completionMode === 'table' ? 'text-warning' : 'text-secondary'">Xong cả bàn</div>
                <div class="text-muted" style="font-size:0.72rem">Bấm nút xong toàn bộ bàn 1 lần</div>
              </div>
            </div>
          </div>
        </div>

        <!-- Chế độ hiển thị -->
        <div class="mb-4">
          <label class="text-warning fw-bold mb-2 d-block">
            <i class="bi bi-layout-three-columns me-1"></i>Chế độ hiển thị
          </label>
          <div class="row g-2">
            <div class="col-6">
              <div class="setting-option rounded-3 p-3 text-center cursor-pointer border border-2"
                :class="displayMode === 'list' ? 'border-info bg-info bg-opacity-25' : 'border-secondary bg-secondary bg-opacity-10'"
                @click="displayMode = 'list'">
                <i class="bi bi-list-ul fs-2 d-block mb-2" :class="displayMode === 'list' ? 'text-info' : 'text-muted'"></i>
                <div class="fw-bold small" :class="displayMode === 'list' ? 'text-info' : 'text-secondary'">Danh sách dọc</div>
                <div class="text-muted" style="font-size:0.72rem">Chỉ hiện bàn đang có món, dạng cột</div>
              </div>
            </div>
            <div class="col-6">
              <div class="setting-option rounded-3 p-3 text-center cursor-pointer border border-2"
                :class="displayMode === 'grid' ? 'border-info bg-info bg-opacity-25' : 'border-secondary bg-secondary bg-opacity-10'"
                @click="displayMode = 'grid'">
                <i class="bi bi-grid-3x3-gap fs-2 d-block mb-2" :class="displayMode === 'grid' ? 'text-info' : 'text-muted'"></i>
                <div class="fw-bold small" :class="displayMode === 'grid' ? 'text-info' : 'text-secondary'">Sơ đồ bàn</div>
                <div class="text-muted" style="font-size:0.72rem">Hiện tất cả bàn, bàn có món được tô sáng</div>
              </div>
            </div>
          </div>
        </div>

        <div class="d-flex gap-2 justify-content-end">
          <button class="btn btn-secondary px-4" @click="showSettings = false">Đóng</button>
          <button class="btn btn-warning fw-bold px-4" @click="applySettings">
            <i class="bi bi-check2 me-1"></i>Áp dụng
          </button>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, inject } from "vue";
import { useRouter } from "vue-router";
import axios from "axios";
import * as signalR from "@microsoft/signalr";
import { globalState } from "../store";

const router = useRouter();
const swal = inject("$swal");
const backendUrl = "http://localhost:5098";

// ===== STATE =====
const activeTab = ref("pending");
const loading = ref(true);
const currentTime = ref("");
const pendingItems = ref([]);   // Flat list từ API
const historyItems = ref([]);   // Lịch sử đã xong
const allTables = ref([]);      // Tất cả bàn (cho grid mode)

// Thiết lập
const showSettings = ref(false);
const displayMode = ref(localStorage.getItem("kitchen_display") || "list");    // 'list' | 'grid'
const completionMode = ref(localStorage.getItem("kitchen_completion") || "table"); // 'item' | 'table'

// Checkbox state: { [chiTietId]: true }
const checkedItems = ref({});

// ===== ĐỒNG HỒ =====
const clockTimer = setInterval(() => {
  currentTime.value = new Date().toLocaleTimeString("vi-VN");
}, 1000);

// ===== SIGNALR =====
const connection = new signalR.HubConnectionBuilder()
  .withUrl(`${backendUrl}/kitchenHub`)
  .withAutomaticReconnect()
  .build();

// ===== COMPUTED =====

// Nhóm món theo bàn cho chế độ LIST (chỉ bàn có món)
const groupedByTable = computed(() => {
  const map = {};
  for (const item of pendingItems.value) {
    if (!map[item.banId]) {
      map[item.banId] = {
        banId: item.banId,
        tenBan: item.tenBan,
        items: [],
        maxWait: 0,
      };
    }
    map[item.banId].items.push(item);
    if (item.thoiGianCho > map[item.banId].maxWait) {
      map[item.banId].maxWait = item.thoiGianCho;
    }
  }
  // Sắp xếp theo thời gian chờ giảm dần
  return Object.values(map).sort((a, b) => b.maxWait - a.maxWait);
});

// Tất cả bàn kèm món cho chế độ GRID (hiện tất cả bàn)
const allTablesForGrid = computed(() => {
  const map = {};
  for (const item of pendingItems.value) {
    if (!map[item.banId]) {
      map[item.banId] = {
        banId: item.banId,
        id: item.banId,
        tenBan: item.tenBan,
        hasItems: true,
        items: [],
        maxWait: 0,
      };
    }
    map[item.banId].items.push(item);
    if (item.thoiGianCho > map[item.banId].maxWait) {
      map[item.banId].maxWait = item.thoiGianCho;
    }
  }

  // Merge với danh sách tất cả bàn
  const result = allTables.value.map((ban) => {
    if (map[ban.id]) return map[ban.id];
    return { ...ban, banId: ban.id, hasItems: false, items: [], maxWait: 0 };
  });

  // Sắp xếp: bàn có món lên trước
  return result.sort((a, b) => {
    if (a.hasItems && !b.hasItems) return -1;
    if (!a.hasItems && b.hasItems) return 1;
    return b.maxWait - a.maxWait;
  });
});

// Tổng số món đang chờ
const totalPendingCount = computed(() => pendingItems.value.length);

// ===== HELPERS =====
const isChecked = (chiTietId) => !!checkedItems.value[chiTietId];

const toggleCheck = (chiTietId) => {
  if (checkedItems.value[chiTietId]) {
    delete checkedItems.value[chiTietId];
  } else {
    checkedItems.value[chiTietId] = true;
  }
};

const getCheckedItems = (banId) => {
  const group = groupedByTable.value.find((g) => g.banId === banId)
    || allTablesForGrid.value.find((g) => g.banId === banId || g.id === banId);
  if (!group) return [];
  return group.items.filter((i) => checkedItems.value[i.chiTietId]);
};

// ===== API CALLS =====
const fetchPendingItems = async (branchId) => {
  loading.value = true;
  try {
    const res = await axios.get(`/api/HoaDon/bep/danh-sach?chiNhanhId=${branchId}`);
    pendingItems.value = res.data;
  } catch (e) {
    console.error(e);
  } finally {
    loading.value = false;
  }
};

const fetchAllTables = async (branchId) => {
  try {
    const res = await axios.get(`/api/Ban/danh-sach-pos?chiNhanhId=${branchId}`);
    allTables.value = res.data;
  } catch (e) {
    console.error(e);
  }
};

const getBranchIdAndFetch = async () => {
  let branchId = globalState.value.activeBranchId || localStorage.getItem("pos36_active_branch");
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
    await Promise.all([fetchPendingItems(branchId), fetchAllTables(branchId)]);
  } else {
    loading.value = false;
  }
};

// ===== ACTIONS =====

// Đánh dấu xong 1 món lẻ
const markItemDone = async (item) => {
  try {
    await axios.put(`/api/HoaDon/bep/xong/${item.chiTietId}`);
    item.thoiGianXong = new Date().toLocaleTimeString("vi-VN");
    historyItems.value.unshift({ ...item });
    pendingItems.value = pendingItems.value.filter((i) => i.chiTietId !== item.chiTietId);
    delete checkedItems.value[item.chiTietId];
    swal.fire({ toast: true, position: "top-end", icon: "success", title: `✅ ${item.tenMon} đã xong`, timer: 1500, showConfirmButton: false });
  } catch (e) {
    swal.fire("Lỗi", "Không thể cập nhật trạng thái", "error");
  }
};

// Đánh dấu xong toàn bộ bàn
const markTableDone = async (group) => {
  const items = group.items;
  if (!items || items.length === 0) return;
  try {
    await Promise.all(items.map((item) => axios.put(`/api/HoaDon/bep/xong/${item.chiTietId}`)));
    const now = new Date().toLocaleTimeString("vi-VN");
    items.forEach((item) => {
      historyItems.value.unshift({ ...item, thoiGianXong: now });
      delete checkedItems.value[item.chiTietId];
    });
    const ids = new Set(items.map((i) => i.chiTietId));
    pendingItems.value = pendingItems.value.filter((i) => !ids.has(i.chiTietId));
    swal.fire({ toast: true, position: "top-end", icon: "success", title: `✅ ${group.tenBan} đã hoàn thành!`, timer: 2000, showConfirmButton: false });
  } catch (e) {
    swal.fire("Lỗi", "Không thể cập nhật", "error");
  }
};

// Đánh dấu xong các món đã tick
const markCheckedItemsDone = async (banId) => {
  const checked = getCheckedItems(banId);
  if (checked.length === 0) return;
  try {
    await Promise.all(checked.map((item) => axios.put(`/api/HoaDon/bep/xong/${item.chiTietId}`)));
    const now = new Date().toLocaleTimeString("vi-VN");
    checked.forEach((item) => {
      historyItems.value.unshift({ ...item, thoiGianXong: now });
      delete checkedItems.value[item.chiTietId];
    });
    const ids = new Set(checked.map((i) => i.chiTietId));
    pendingItems.value = pendingItems.value.filter((i) => !ids.has(i.chiTietId));
    swal.fire({ toast: true, position: "top-end", icon: "success", title: `✅ Xong ${checked.length} món!`, timer: 1500, showConfirmButton: false });
  } catch (e) {
    swal.fire("Lỗi", "Không thể cập nhật", "error");
  }
};

// Áp dụng thiết lập
const applySettings = () => {
  localStorage.setItem("kitchen_display", displayMode.value);
  localStorage.setItem("kitchen_completion", completionMode.value);
  checkedItems.value = {};
  showSettings.value = false;
  swal.fire({ toast: true, position: "top-end", icon: "success", title: "Đã lưu thiết lập!", timer: 1500, showConfirmButton: false });
};

const logout = () => {
  localStorage.clear();
  window.location.href = "/login";
};

// ===== LIFECYCLE =====
onMounted(async () => {
  try {
    await connection.start();
    connection.on("CoDonHangMoi", (data) => {
      // Kiểm tra xem có phải sự kiện hủy món không
      const isCancel = data?.message === "Hủy món";
      if (!isCancel) {
        swal.fire({
          toast: true, position: "top",
          title: "🔔 CÓ MÓN MỚI!",
          background: "#dc3545", color: "#fff",
          showConfirmButton: false, timer: 2500,
        });
      }
      if (globalState.value.activeBranchId) {
        fetchPendingItems(globalState.value.activeBranchId);
      }
    });
  } catch (err) {
    console.error(err);
  }

  await getBranchIdAndFetch();

  // Refresh định kỳ
  setInterval(() => {
    if (globalState.value.activeBranchId) {
      fetchPendingItems(globalState.value.activeBranchId);
    }
  }, 30000);
});

onUnmounted(() => {
  connection.stop();
  clearInterval(clockTimer);
});
</script>

<style scoped>
/* --- LAYOUT --- */
.kitchen-container { font-family: 'Courier New', monospace; }
.cursor-pointer { cursor: pointer; }

/* --- TABLE GROUP CARD (list mode) --- */
.table-group-card {
  background: #2a2a2a;
  border: 1px solid #444;
}
.table-header {
  background: linear-gradient(90deg, #1a1a1a, #333);
  border-bottom: 2px solid #555;
}
.list-item {
  background: #2a2a2a;
  transition: background 0.2s;
}
.list-item:hover { background: #333; }
.list-item.item-checked { background: #1e3a1e; opacity: 0.8; }

/* --- QTY BADGE --- */
.qty-badge {
  min-width: 44px;
  min-height: 44px;
  font-size: 1.3rem;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 8px;
  flex-shrink: 0;
}

/* --- CHECKBOX KITCHEN --- */
.kitchen-check {
  width: 1.3em;
  height: 1.3em;
  cursor: pointer;
  flex-shrink: 0;
}

/* --- SETTING OPTION --- */
.setting-option {
  cursor: pointer;
  transition: all 0.2s;
}
.setting-option:hover { transform: translateY(-2px); }

/* --- MODAL BACKDROP --- */
.modal-backdrop-custom {
  position: fixed;
  inset: 0;
  background: rgba(0,0,0,0.75);
  z-index: 9999;
  display: flex;
  align-items: center;
  justify-content: center;
}
.settings-modal { max-height: 92vh; overflow-y: auto; }

/* --- ANIMATIONS --- */
.fade-in { animation: fadeIn 0.3s ease-in-out; }
@keyframes fadeIn {
  from { opacity: 0; transform: translateY(8px); }
  to { opacity: 1; transform: translateY(0); }
}
.pulse { animation: pulse-red 1.5s infinite; }
@keyframes pulse-red {
  0% { transform: scale(1); box-shadow: 0 0 0 0 rgba(220,53,69,0.7); }
  70% { transform: scale(1.08); box-shadow: 0 0 0 8px rgba(220,53,69,0); }
  100% { transform: scale(1); box-shadow: 0 0 0 0 rgba(220,53,69,0); }
}
</style>
