<template>
  <div>
    <div class="d-flex justify-content-between align-items-center mb-4">
      <select v-model.number="days" class="sa-select" @change="load">
        <option :value="7">7 ngày</option>
        <option :value="30">30 ngày</option>
        <option :value="90">90 ngày</option>
      </select>
    </div>
    <div class="row g-3 mb-4">
      <div class="col-md-4"><div class="stat-card"><div class="stat-value">{{ data.tongLuot }}</div><div class="stat-label">Tổng lượt truy cập</div></div></div>
      <div class="col-md-4"><div class="stat-card"><div class="stat-value text-info">{{ data.desktop }}</div><div class="stat-label">Desktop</div></div></div>
      <div class="col-md-4"><div class="stat-card"><div class="stat-value text-warning">{{ data.mobile }}</div><div class="stat-label">Mobile</div></div></div>
    </div>
    <div class="row g-3">
      <div class="col-lg-8">
        <div class="dash-card">
          <h6 class="dash-title">Lượt truy cập theo ngày</h6>
          <div class="chart-bars" v-if="data.luotTheoNgay?.length">
            <div v-for="(item, i) in data.luotTheoNgay" :key="i" class="chart-bar-wrap">
              <div class="chart-bar" :style="{ height: barH(item.value) + '%' }"></div>
              <span class="chart-lbl">{{ item.label }}</span>
            </div>
          </div>
        </div>
      </div>
      <div class="col-lg-4">
        <div class="dash-card h-100">
          <h6 class="dash-title">Top trang phổ biến</h6>
          <div v-for="(p, i) in data.topPages" :key="i" class="top-item">
            <span class="top-rank">#{{ i+1 }}</span>
            <span class="top-url">{{ p.url }}</span>
            <span class="top-cnt">{{ p.count }}</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
<script setup>
import { ref, onMounted } from "vue";
import axios from "axios";
const days = ref(30);
const data = ref({ tongLuot:0, mobile:0, desktop:0, luotTheoNgay:[], topPages:[] });
const barH = (v) => { const mx = Math.max(...(data.value.luotTheoNgay||[]).map(d=>d.value),1); return Math.max((v/mx)*100,3); };
const load = async () => { try { const r = await axios.get("/api/SuperAdmin/analytics",{params:{days:days.value}}); data.value = r.data; } catch(e){} };
onMounted(load);
</script>
<style scoped>
.sa-select{background:#1a1c23;color:#e4e4e7;border:1px solid rgba(255,255,255,.1);border-radius:8px;padding:8px 14px;font-size:.85rem}
.stat-card{background:#1a1c23;border:1px solid rgba(255,255,255,.06);border-radius:12px;padding:22px;text-align:center}
.stat-value{font-size:2rem;font-weight:800;color:#f4f4f5}.stat-label{font-size:.82rem;color:#6b7280}
.dash-card{background:#1a1c23;border:1px solid rgba(255,255,255,.06);border-radius:14px;padding:22px}
.dash-title{font-weight:700;font-size:.82rem;color:#9ca3af;margin-bottom:16px;text-transform:uppercase;letter-spacing:.5px}
.chart-bars{display:flex;align-items:flex-end;gap:6px;height:200px}
.chart-bar-wrap{flex:1;display:flex;flex-direction:column;align-items:center}
.chart-bar{width:100%;max-width:30px;background:linear-gradient(180deg,#3b82f6,#1d4ed8);border-radius:4px 4px 0 0;transition:height .6s;min-height:3px}
.chart-lbl{font-size:.6rem;color:#6b7280;margin-top:4px}
.top-item{display:flex;align-items:center;gap:8px;padding:8px 10px;border-bottom:1px solid rgba(255,255,255,.04);font-size:.82rem}
.top-rank{color:#f59e0b;font-weight:800;min-width:28px}.top-url{flex:1;color:#d1d5db;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}.top-cnt{color:#9ca3af;font-weight:700}
</style>
