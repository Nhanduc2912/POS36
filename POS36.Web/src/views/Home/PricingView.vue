<template>
  <div>
    <PublicNavbar />
    <main class="bg-light text-dark font-body pt-5 mt-5">
      <section class="container max-w-7xl py-5 mt-4 text-center">
        <span class="text-orange font-headline fw-bold tracking-wide small text-uppercase mb-3 d-block">Đầu tư thông minh</span>
        <h1 class="display-4 font-headline fw-black tracking-tighter mb-4 text-dark lh-sm">
          Chọn gói phù hợp. <br class="d-none d-md-block" />
          <span class="text-orange fst-italic">Khởi nghiệp không giới hạn.</span>
        </h1>
        <p class="fs-5 text-secondary max-w-lg mx-auto mb-5">
          Bắt đầu miễn phí 7 ngày, không yêu cầu thẻ tín dụng. Chọn gói phù hợp với quy mô kinh doanh của bạn.
        </p>
      </section>

      <!-- 3 Plans -->
      <section class="container max-w-7xl mb-5">
        <div class="row g-4 justify-content-center">
          <div class="col-md-4" v-for="plan in plans" :key="plan.code">
            <div class="plan-box rounded-5 p-4 text-center ambient-shadow hover-lift h-100"
              :class="{ 'plan-featured': plan.featured }">
              <span v-if="plan.featured" class="plan-popular-badge">Phổ biến nhất</span>
              <div class="plan-icon-wrap mb-3">
                <i :class="'bi bi-' + plan.icon + ' fs-1'" :style="{ color: plan.color }"></i>
              </div>
              <h3 class="font-headline fw-black mb-1">{{ plan.name }}</h3>
              <p class="text-secondary small mb-3">{{ plan.desc }}</p>

              <div class="mb-3">
                <span class="display-4 fw-black" :style="{ color: plan.color }">{{ plan.price }}</span>
                <span class="text-secondary fs-6"> / {{ plan.period }}</span>
              </div>
              <div class="text-muted small mb-4">
                <span class="fw-bold">{{ plan.monthly }}</span>/tháng
              </div>

              <ul class="list-unstyled text-start small mb-4 px-3">
                <li v-for="f in plan.features" :key="f" class="mb-2">
                  <i class="bi bi-check-circle-fill text-success me-2"></i>{{ f }}
                </li>
                <li v-for="l in plan.limits" :key="l" class="mb-2 text-muted">
                  <i class="bi bi-dash-circle me-2"></i>{{ l }}
                </li>
              </ul>

              <router-link to="/register"
                class="btn w-100 py-3 fw-black rounded-4 fs-6"
                :class="plan.featured ? 'btn-orange text-white shadow-lg' : 'btn-outline-dark'">
                Dùng thử 7 ngày miễn phí
              </router-link>
            </div>
          </div>
        </div>
      </section>

      <!-- Features -->
      <section class="container max-w-7xl py-5 mb-5">
        <div class="text-center mb-5">
          <h2 class="font-headline fw-black display-6">Tất cả gói đều bao gồm</h2>
          <p class="text-secondary">Dù chọn gói nào, bạn vẫn được trải nghiệm đầy đủ sức mạnh POS36.</p>
        </div>
        <div class="row g-4">
          <div class="col-md-4" v-for="cat in featureCategories" :key="cat.title">
            <div class="bg-white p-4 rounded-4 ambient-shadow h-100 border-top border-4 border-orange">
              <div class="icon-box bg-orange-subtle rounded-circle d-flex align-items-center justify-content-center mb-4"
                style="width: 50px; height: 50px">
                <i :class="'bi bi-' + cat.icon + ' text-orange fs-4'"></i>
              </div>
              <h4 class="font-headline fw-bold mb-3">{{ cat.title }}</h4>
              <ul class="list-unstyled space-y-3 small text-secondary fw-medium">
                <li v-for="f in cat.items" :key="f" class="mb-2">
                  <i class="bi bi-check-lg text-success me-2 fs-5"></i>{{ f }}
                </li>
              </ul>
            </div>
          </div>
        </div>
      </section>

      <!-- FAQ -->
      <section class="container max-w-lg mx-auto py-5 mb-5">
        <h2 class="font-headline fw-bold text-center mb-5 display-6">Câu hỏi thường gặp</h2>
        <div class="accordion accordion-flush bg-transparent" id="pricingFAQ">
          <div v-for="(faq, i) in faqs" :key="i"
            class="accordion-item border-0 mb-3 bg-white rounded-4 ambient-shadow overflow-hidden">
            <h2 class="accordion-header">
              <button class="accordion-button collapsed fw-bold p-4 text-dark bg-white"
                type="button" data-bs-toggle="collapse" :data-bs-target="'#faq' + i">
                {{ faq.q }}
              </button>
            </h2>
            <div :id="'faq' + i" class="accordion-collapse collapse" data-bs-parent="#pricingFAQ">
              <div class="accordion-body px-4 pb-4 pt-0 text-secondary">{{ faq.a }}</div>
            </div>
          </div>
        </div>
      </section>

      <!-- CTA -->
      <section class="container max-w-7xl pb-5 mb-5 mt-4">
        <div class="bg-dark rounded-5 p-5 text-center text-white ambient-shadow position-relative overflow-hidden border-bottom border-5 border-orange">
          <div class="position-relative z-index-1 py-4">
            <h2 class="display-5 font-headline fw-black mb-4">Bắt đầu trong 5 phút</h2>
            <p class="fs-5 opacity-75 mb-4 max-w-lg mx-auto">Đăng ký tài khoản → Dùng thử 7 ngày → Mua gói khi sẵn sàng.</p>
            <router-link to="/register" class="btn culinary-gradient text-white rounded-pill px-5 py-3 fw-black fs-5 shadow-lg hover-scale">
              Tạo tài khoản ngay
            </router-link>
          </div>
        </div>
      </section>
    </main>
    <PublicFooter />
  </div>
</template>

<script setup>
import { ref } from "vue";
import PublicNavbar from "../../components/PublicNavbar.vue";
import PublicFooter from "../../components/PublicFooter.vue";

const plans = ref([
  {
    name: "Starter", code: "STARTER", price: "900k", period: "6 tháng", monthly: "150k",
    icon: "rocket-takeoff", color: "#3b82f6", featured: false,
    desc: "Phù hợp quán nhỏ mới khởi nghiệp",
    features: ["Đầy đủ tính năng POS", "Quản lý thực đơn & kho", "Thanh toán QR tự động", "Báo cáo doanh thu"],
    limits: ["Tối đa 200 hóa đơn/tháng", "Tối đa 2 nhân viên"],
  },
  {
    name: "Pro", code: "PRO", price: "1.500k", period: "1 năm", monthly: "125k",
    icon: "star-fill", color: "#f59e0b", featured: true,
    desc: "Lựa chọn tối ưu cho mọi quy mô",
    features: ["Không giới hạn hóa đơn", "Không giới hạn nhân viên", "Trợ lý AI phân tích", "Hỗ trợ ưu tiên 24/7"],
    limits: [],
  },
  {
    name: "Ultimate", code: "ULTIMATE", price: "2.800k", period: "2 năm", monthly: "117k",
    icon: "gem", color: "#a855f7", featured: false,
    desc: "Tiết kiệm nhất, cam kết dài hạn",
    features: ["Toàn bộ tính năng Pro", "Tiết kiệm 7% so với Pro", "Ưu tiên tính năng mới", "Hỗ trợ cài đặt & di chuyển dữ liệu"],
    limits: [],
  },
]);

const featureCategories = ref([
  { title: "Vận hành cốt lõi", icon: "shop", items: ["Bán hàng tại quầy & Order tại bàn", "Quản lý sơ đồ bàn trực quan", "Kết nối máy in bếp & tem nhãn"] },
  { title: "Quản lý chuyên sâu", icon: "box-seam", items: ["Quản lý kho nguyên vật liệu", "CRM & Tích điểm hội viên", "Quản lý thu chi & Công nợ"] },
  { title: "AI & Hỗ trợ", icon: "robot", items: ["Báo cáo doanh thu thông minh AI", "Tự động đề xuất chiến lược", "Dashboard quản trị từ xa"] },
]);

const faqs = ref([
  { q: "Tôi mở thêm chi nhánh thì tính phí thế nào?", a: "Mức giá trên tính cho 1 điểm bán. Khi mở rộng thêm chi nhánh, bạn chỉ cần mua thêm 1 gói cho chi nhánh đó." },
  { q: "Hết 7 ngày dùng thử, dữ liệu có bị mất không?", a: "Tuyệt đối không! Dữ liệu được lưu trữ an toàn. Khi nâng cấp, mọi dữ liệu từ quá trình dùng thử sẽ được giữ nguyên." },
  { q: "Hết hạn gói thì chuyện gì xảy ra?", a: "Bạn sẽ có 15 ngày ở chế độ Chỉ Đọc (vẫn xem được dữ liệu). Sau 15 ngày, tài khoản bị tạm khóa. Gia hạn bất cứ lúc nào để mở lại." },
  { q: "Thanh toán bằng cách nào?", a: "Chuyển khoản ngân hàng qua mã QR VietQR. Hệ thống tự động kích hoạt gói ngay sau khi nhận được thanh toán (webhook SePay)." },
]);
</script>

<style scoped>
@import url("https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@400;800&family=Manrope:wght@400;600&display=swap");
.font-headline { font-family: "Plus Jakarta Sans", sans-serif; }
.font-body { font-family: "Manrope", sans-serif; }
.fw-black { font-weight: 800; }
.tracking-tighter { letter-spacing: -1.5px; }
.tracking-wide { letter-spacing: 1.5px; }
.text-orange { color: #ea580c !important; }
.bg-orange { background-color: #ea580c !important; }
.bg-orange-subtle { background-color: #ffedd5 !important; }
.border-orange { border-color: #ea580c !important; }
.btn-orange { background-color: #ea580c; border-color: #ea580c; }
.btn-orange:hover { background-color: #c2410c; }
.culinary-gradient { background: linear-gradient(135deg, #994700 0%, #ff7a00 100%); border: none; }
.ambient-shadow { box-shadow: 0 40px 60px -20px rgba(26, 28, 28, 0.06); }
.hover-lift { transition: transform 0.4s cubic-bezier(0.2, 0.8, 0.2, 1), box-shadow 0.4s; }
.hover-lift:hover { transform: translateY(-10px) !important; box-shadow: 0 40px 60px -20px rgba(26, 28, 28, 0.15); }
.hover-scale { transition: transform 0.2s ease; }
.hover-scale:hover { transform: scale(1.05); }
.max-w-7xl { max-width: 1280px; margin: 0 auto; }
.max-w-lg { max-width: 800px; margin: 0 auto; }
.z-index-1 { z-index: 1; position: relative; }

/* Plans */
.plan-box { background: #fff; border: 2px solid #e5e7eb; position: relative; transition: all 0.3s; }
.plan-featured { border-color: #f59e0b; background: linear-gradient(180deg, #fffbeb 0%, #ffffff 30%); }
.plan-popular-badge { position: absolute; top: -1px; right: 20px; background: #f59e0b; color: white; padding: 4px 16px; border-radius: 0 0 10px 10px; font-size: 0.72rem; font-weight: 800; text-transform: uppercase; letter-spacing: 0.5px; }
.plan-icon-wrap { width: 64px; height: 64px; border-radius: 16px; background: #f8f9fa; display: flex; align-items: center; justify-content: center; margin: 0 auto; }

/* Accordion */
.accordion-button:focus { box-shadow: none; }
.accordion-button:not(.collapsed) { background-color: #fff; color: #ea580c; box-shadow: none; }
</style>
