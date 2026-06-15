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

  // --- SUPER ADMIN PORTAL (Tách biệt hoàn toàn) ---
  {
    path: "/super-admin",
    component: () => import("../views/SuperAdmin/SuperAdminLayout.vue"),
    meta: { requiresAuth: true, requiresSuperAdmin: true },
    children: [
      {
        path: "",
        name: "SuperDashboard",
        component: () => import("../views/SuperAdmin/SuperDashboard.vue"),
      },
      {
        path: "stores",
        name: "StoreManagement",
        component: () => import("../views/SuperAdmin/StoreManagement.vue"),
      },
      {
        path: "subscriptions",
        name: "SubscriptionRequests",
        component: () =>
          import("../views/SuperAdmin/SubscriptionRequests.vue"),
      },
      {
        path: "plans",
        name: "PlanManagement",
        component: () => import("../views/SuperAdmin/PlanManagement.vue"),
      },
      {
        path: "analytics",
        name: "AnalyticsView",
        component: () => import("../views/SuperAdmin/AnalyticsView.vue"),
      },
      {
        path: "notifications",
        name: "NotificationCenter",
        component: () => import("../views/SuperAdmin/NotificationCenter.vue"),
      },
      {
        path: "config",
        name: "ConfigView",
        component: () => import("../views/SuperAdmin/ConfigView.vue"),
      },
      {
        path: "ai-report",
        name: "SuperAdminAiReport",
        component: () => import("../views/SuperAdmin/SuperAdminReport.vue"),
      },
    ],
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
        path: "customers",
        name: "CustomerSetup",
        component: () => import("../views/CustomerSetup.vue"),
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
        path: "lai-gop",
        name: "LaiGopReport",
        component: () => import("../views/LaiGopReport.vue"),
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
      // SaaS: Trang quản lý gói dịch vụ cho chủ cửa hàng
      {
        path: "subscription",
        name: "AdminSubscription",
        component: () => import("../views/SubscriptionView.vue"),
      },
      {
        path: "thiet-lap",
        name: "ThietLap",
        component: () => import("../views/ThietLapView.vue"),
      },
      {
        path: "audit-log",
        name: "AuditLog",
        component: () => import("../views/AuditLogView.vue"),
      },
    ],
  },

  // --- CÁC MÀN HÌNH TÁC NGHIỆP FULLSCREEN ---
  {
    path: "/pos",
    name: "PosSystem",
    component: () => import("../views/PosView.vue"),
    meta: { requiresAuth: true, roles: ["ChuCuaHang", "Admin", "QuanLy", "ThuNgan", "SuperAdmin"] },
  },
  {
    path: "/order",
    name: "StaffOrder",
    component: () => import("../views/OrderView.vue"),
    meta: { requiresAuth: true, roles: ["ChuCuaHang", "Admin", "QuanLy", "Order", "SuperAdmin"] },
  },
  {
    path: "/kitchen",
    name: "KitchenDisplay",
    component: () => import("../views/KitchenView.vue"),
    meta: { requiresAuth: true, roles: ["ChuCuaHang", "Admin", "QuanLy", "Bep", "SuperAdmin"] },
  },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

// Map từng route Admin → mã quyền cần thiết (chỉ dành cho ThuNgan)
const ROUTE_QUYEN_MAP = {
  "/admin/orders": "view_orders",
  "/admin/cashbook": "view_cashbook",
  "/admin/daily-summary": "view_daily_summary",
  "/admin/sales-report": "view_sales_report",
  "/admin/lai-gop": "view_lai_gop",
  "/admin/customers": "view_customers",
  "/admin/import-stock": "view_import_stock",
  "/admin/import-create": "view_import_stock",
  "/admin/inventory": "view_inventory",
  "/admin/inventory-create": "view_inventory",
  "/admin/products": "view_products",
  "/admin/ai-report": "view_ai_report",
  // Các route sau luôn mở cho Admin (không cần map)
  "/admin": null,
  "/admin/profile": null,
};

router.beforeEach((to, from) => {
  const token =
    localStorage.getItem("pos36_token") || localStorage.getItem("token");
  const role = localStorage.getItem("pos36_role");
  const quyenRaw = localStorage.getItem("pos36_quyen_thungan") || "";
  const danhSachQuyen = quyenRaw ? quyenRaw.split(",").map(q => q.trim()) : [];

  if (to.meta.requiresAuth && !token) {
    return "/login";
  }

  // Guard: Super Admin portal chỉ cho SuperAdmin
  if (to.meta.requiresSuperAdmin && role !== "SuperAdmin") {
    return "/login";
  }

  if (to.path.startsWith("/admin")) {
    if (!token) return "/login";

    // *** THU NGÂN: Cho phép vào nếu có ít nhất 1 quyền ***
    if (role === "ThuNgan") {
      if (danhSachQuyen.length === 0) {
        // Không có quyền gì → về POS
        return "/pos";
      }

      // Kiểm tra quyền cụ thể của từng route
      const exactPath = to.path;
      // Tìm mapping phù hợp nhất (khớp exact hoặc prefix)
      const requiredQuyen = ROUTE_QUYEN_MAP[exactPath];
      if (requiredQuyen === undefined) {
        // Route nhạy cảm không có trong map → chặn
        return "/pos";
      }
      if (requiredQuyen !== null && !danhSachQuyen.includes(requiredQuyen)) {
        // Route yêu cầu quyền cụ thể nhưng không có → về trang admin chính
        return "/admin";
      }
      // OK: có quyền → cho vào
      return true;
    }

    if (
      role !== "Admin" &&
      role !== "QuanLy" &&
      role !== "ChuCuaHang" &&
      role !== "SuperAdmin"
    ) {
      if (role === "Order") return "/order";
      if (role === "Bep") return "/kitchen";
      return "/login";
    }
  }

  // Guard: Chặn phân quyền chéo giữa POS, Order, Kitchen
  if (to.meta.roles && !to.meta.roles.includes(role)) {
    if (role === "ThuNgan") {
      // ThuNgan được vào /pos
      if (to.path === "/pos") return true;
      // Và được vào /admin nếu có quyền
      if (to.path.startsWith("/admin") && danhSachQuyen.length > 0) return true;
      return "/pos";
    }
    if (role === "Order") return "/order";
    if (role === "Bep") return "/kitchen";
    if (
      role === "Admin" ||
      role === "ChuCuaHang" ||
      role === "QuanLy" ||
      role === "SuperAdmin"
    ) {
      return "/admin";
    }
    return "/login";
  }

  return true;
});

export default router;

