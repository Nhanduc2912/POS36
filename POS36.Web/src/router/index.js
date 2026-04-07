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
  // THÊM ROUTE QUÊN MẬT KHẨU VÀO ĐÂY
  {
    path: "/forgot-password",
    name: "ForgotPassword",
    component: () => import("../views/ForgotPasswordView.vue"),
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
      {
        path: "ai-report",
        name: "AiReport",
        component: () => import("../views/AiReportView.vue"),
      },
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
      {
        path: "profile",
        name: "AdminProfile",
        component: () => import("../views/ProfileView.vue"),
      },
      {
        path: "store-info",
        name: "AdminStoreInfo",
        component: () => import("../views/StoreInfoView.vue"),
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

router.beforeEach((to, from) => {
  const token =
    localStorage.getItem("pos36_token") || localStorage.getItem("token");
  const role = localStorage.getItem("pos36_role");

  if (to.meta.requiresAuth && !token) {
    return "/login";
  }

  if (to.path.startsWith("/admin")) {
    if (!token) return "/login";

    if (role !== "Admin" && role !== "QuanLy" && role !== "ChuCuaHang") {
      if (role === "ThuNgan") return "/pos";
      if (role === "Order") return "/order";
      if (role === "Bep") return "/kitchen";
      return "/login";
    }
  }

  return true;
});

export default router;
