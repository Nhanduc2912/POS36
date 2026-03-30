import { createRouter, createWebHistory } from "vue-router";

const routes = [
  {
    path: "/",
    name: "LandingPage",
    component: () => import("../views/Home/LandingPage.vue"),
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
  {
    path: "/features",
    name: "Features",
    component: () => import("../views/Home/FeaturesView.vue"),
  },
  {
    path: "/pricing",
    name: "Pricing",
    component: () => import("../views/Home/PricingView.vue"),
  },
  {
    path: "/about",
    name: "About",
    component: () => import("../views/Home/AboutView.vue"),
  },
  {
    path: "/privacy",
    name: "Privacy",
    component: () => import("../views/Home/PrivacyView.vue"),
  },
  {
    path: "/solutions",
    name: "Solutions",
    component: () => import("../views/Home/SolutionsView.vue"),
  },

  // --- GIAO DIỆN QUẢN TRỊ (CÓ MENU CAM) ---
  {
    path: "/admin",
    component: () => import("../views/AdminLayout.vue"),
    meta: { requiresAuth: true },
    children: [
      {
        path: "",
        name: "AdminOverview",
        component: () => import("../views/DashboardOverview.vue"),
      },
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
      {
        path: "prices",
        name: "PriceSetup",
        component: () => import("../views/PriceSetup.vue"),
      },
      {
        path: "import-stock",
        name: "ImportStock",
        component: () => import("../views/ImportStock.vue"),
      },
      {
        path: "import-create",
        name: "ImportStockCreate",
        component: () => import("../views/CreateStock.vue"),
      },
      {
        path: "employees",
        name: "EmployeeSetup",
        component: () => import("../views/EmployeeSetup.vue"),
      },
      {
        path: "orders",
        name: "admin-orders",
        component: () => import("../views/OrderList.vue"),
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
      // ĐÃ FIX LỖI TÊN FILE AI REPORT
      {
        path: "ai-report",
        name: "AiReport",
        component: () => import("../views/AiReportView.vue"),
      },
      // THÊM 2 TRANG BÁO CÁO MỚI
      {
        path: "daily-summary",
        name: "DailySummary",
        component: () => import("../views/DailySummary.vue"),
      },
      {
        path: "sales-report",
        name: "SalesReport",
        component: () => import("../views/SalesReport.vue"),
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
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

// NGƯỜI GÁC CỔNG (ROUTER GUARD) CHỐNG VƯỢT QUYỀN (Đã dọn dẹp code thừa)
router.beforeEach((to, from, next) => {
  const token = localStorage.getItem("pos36_token");
  const role = localStorage.getItem("pos36_role");
  if (to.meta.requiresAuth && !token) {
    // Trả về thẳng đường dẫn thay vì dùng hàm next()
    return "/login";
  }
  // 1. Nếu trang cần bảo mật mà chưa có Token -> Đuổi ra Login
  if (to.path !== "/login" && to.path !== "/register" && !token) {
    return true;
  }

  // 2. CHẶN VƯỢT QUYỀN VÀO ADMIN
  if (to.path.startsWith("/admin")) {
    // NẾU KHÔNG PHẢI LÀ Admin, QuanLy, HOẶC ChuCuaHang THÌ BỊ ĐUỔI
    if (role !== "Admin" && role !== "QuanLy" && role !== "ChuCuaHang") {
      if (role === "ThuNgan") return next("/pos");
      if (role === "Order") return next("/order");
      if (role === "Bep") return next("/kitchen");
      return next("/login");
    }
  }

  next();
});

export default router;
