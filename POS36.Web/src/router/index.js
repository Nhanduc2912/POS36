import { createRouter, createWebHistory } from "vue-router";

const routes = [
  {
    path: "/",
    name: "LandingPage",
    component: () => import("../views/LandingPage.vue"),
  },
  {
    path: "/login",
    name: "Login",
    component: () => import("../views/Login.vue"),
  },
  {
    path: "/register",
    name: "Register",
    component: () => import("../views/Register.vue"),
  },

  // --- GIAO DIỆN QUẢN TRỊ (CÓ MENU CAM) ---
  {
    path: "/admin",
    component: () => import("../views/AdminLayout.vue"), // File Layout chứa thanh Menu
    meta: { requiresAuth: true },
    children: [
      // Mặc định vào /admin sẽ load trang Tổng quan
      {
        path: "",
        name: "AdminOverview",
        component: () => import("../views/DashboardOverview.vue"),
      },
      // Đường dẫn /admin/tables sẽ load chức năng Thiết lập bàn
      {
        path: "tables",
        name: "TableSetup",
        component: () => import("../views/TableSetup.vue"),
      },
      {
        path: "products",
        name: "ProductSetup",
        component: () => import("../views/ProductSetup.vue"),
      },
      // Đường dẫn Thiết lập giá
      {
        path: "prices",
        name: "PriceSetup",
        component: () => import("../views/PriceSetup.vue"),
      },
      {
        path: "import-stock", // Trang Danh sách
        name: "ImportStock",
        component: () => import("../views/ImportStock.vue"),
      },
      {
        path: "import-create", // Trang Thêm mới
        name: "ImportStockCreate",
        component: () => import("../views/CreateStock.vue"),
      },
      {
        path: "employees",
        name: "EmployeeSetup",
        component: () => import("../views/EmployeeSetup.vue"),
      },
      {
        path: "orders", // Thêm đường dẫn này
        name: "admin-orders",
        component: () => import("../views/OrderList.vue"), // Trỏ đến file vừa tạo
      },
      {
        path: "inventory",
        name: "admin-inventory",
        component: () => import("../views/InventoryCheck.vue"),
      },
      {
        path: "inventory-create",
        name: "admin-inventory-create",
        component: () => import("../views/InventoryCreate.vue"),
      },
      {
        path: "cashbook",
        name: "admin-cashbook",
        component: () => import("../views/Cashbook.vue"),
      },
      {
        path: "print-setup",
        name: "admin-print-setup",
        component: () => import("../views/PrintSetup.vue"),
      },
      {
        path: "bank-setup",
        name: "admin-bank-setup",
        component: () => import("../views/BankSetup.vue"),
      },
    ],
  },

  // --- CÁC MÀN HÌNH TÁC NGHIỆP FULLSCREEN ---
  {
    path: "/pos",
    name: "PosSystem",
    component: () => import("../views/PosView.vue"),
    meta: { requiresAuth: true },
  },
  {
    path: "/order",
    name: "StaffOrder",
    component: () => import("../views/OrderView.vue"),
    meta: { requiresAuth: true },
  },
  {
    path: "/kitchen",
    name: "KitchenDisplay",
    component: () => import("../views/KitchenView.vue"),
    meta: { requiresAuth: true },
  },

  // Đường dẫn quản lý Thực đơn
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

// Vệ sĩ kiểm tra quyền truy cập (Chuẩn Vue Router 4 mới nhất)
// NGƯỜI GÁC CỔNG (ROUTER GUARD) CHỐNG VƯỢT QUYỀN
router.beforeEach((to, from, next) => {
  const token = localStorage.getItem("pos36_token");
  const role = localStorage.getItem("pos36_role");

  // 1. Nếu trang cần bảo mật (tất cả trừ Login/Register) mà chưa có Token -> Đuổi ra Login
  if (to.path !== "/login" && to.path !== "/register" && !token) {
    return next("/login");
  }

  // 2. CHẶN VƯỢT QUYỀN VÀO ADMIN
  if (to.path.startsWith("/admin")) {
    // 2. CHẶN VƯỢT QUYỀN VÀO ADMIN
    if (to.path.startsWith("/admin")) {
      // NẾU KHÔNG PHẢI LÀ Admin, QuanLy, HOẶC ChuCuaHang THÌ BỊ ĐUỔI
      if (role !== "Admin" && role !== "QuanLy" && role !== "ChuCuaHang") {
        if (role === "ThuNgan") return next("/pos");
        if (role === "Order") return next("/order");
        if (role === "Bep") return next("/kitchen");
        return next("/login"); // Không có quyền rõ ràng thì đuổi ra
      }
    }
  }

  // Cho phép đi tiếp
  next();
});

export default router;
