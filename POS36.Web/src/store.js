import { ref, watch } from "vue";

// Lấy ID chi nhánh cũ từ bộ nhớ (nếu có)
const savedBranchId = localStorage.getItem("pos36_active_branch");

export const globalState = ref({
  activeBranchId: savedBranchId ? parseInt(savedBranchId) : null,
  branches: [], // Danh sách chi nhánh tải từ API
});

// Tự động lưu vào LocalStorage mỗi khi đổi chi nhánh
watch(
  () => globalState.value.activeBranchId,
  (newId) => {
    if (newId) localStorage.setItem("pos36_active_branch", newId);
  },
);
