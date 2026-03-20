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
        path: "import-stock",
        name: "ImportStock",
        component: () => import("../views/ImportStock.vue"),
      },
      {
        path: "employees",
        name: "EmployeeSetup",
        component: () => import("../views/EmployeeSetup.vue"),
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
router.beforeEach((to, from) => {
  const token = localStorage.getItem("pos36_token");

  // Nếu vào trang cần Auth mà không có Token -> Trả về Login
  if (to.meta.requiresAuth && !token) {
    return { name: "Login" };
  }
  // Nếu đã có Token mà lại vào Login, Register, Landing -> Trả về Admin
  if (
    (to.name === "Login" ||
      to.name === "Register" ||
      to.name === "LandingPage") &&
    token
  ) {
    return { name: "AdminOverview" };
  }
  // Hợp lệ thì tự động cho qua (Không cần viết gì thêm)
});

export default router;
