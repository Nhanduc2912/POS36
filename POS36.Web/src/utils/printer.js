import axios from "axios";

export const printReceipt = (orderData, branchInfo) => {
  // Ghi nhận log in hóa đơn lên backend
  try {
    axios.post("/api/HoaDon/log-in-bill", {
      banId: orderData.banId || 0,
      chiNhanhId: orderData.chiNhanhId || 0,
      tenBan: orderData.tenBan || "",
      maChungTu: orderData.maChungTu || "",
      loaiIn: orderData.loaiIn || "In hóa đơn",
      tongTien: orderData.tongTien || 0,
    }).catch(e => console.warn("Lỗi ghi log in bill", e));
  } catch (e) {
    console.warn("Lỗi ghi log in bill", e);
  }

  let template = localStorage.getItem("pos36_print_template");
  if (!template) {
    template = `
<div style="width: 100%; font-family: 'Arial', sans-serif; font-size: 14px; color: #000;">
    <div style="text-align: center; margin-bottom: 10px;">
        <h2 style="margin: 0; font-size: 20px; font-weight: bold;">{TEN_CUA_HANG}</h2>
        <div style="font-size: 12px; margin-top: 5px;">Đ/c: {DIA_CHI}</div>
        <div style="font-size: 12px;">ĐT: {DIEN_THOAI}</div>
    </div>
    <div style="text-align: center; margin: 15px 0; border-top: 1px dashed #000; border-bottom: 1px dashed #000; padding: 10px 0;">
        <h3 style="margin: 0; font-size: 18px; font-weight: bold;">HÓA ĐƠN THANH TOÁN</h3>
        <div style="font-size: 12px; margin-top: 5px;">Số: {MA_CHUNG_TU}</div>
        <div style="font-size: 12px;">Ngày: {NGAY_IN} {GIO_IN}</div>
    </div>
    <div style="font-size: 12px; margin-bottom: 10px; display: flex; justify-content: space-between;">
        <div><b>Bàn:</b> {TEN_BAN}</div>
        <div><b>Thu ngân:</b> {THU_NGAN}</div>
    </div>
    {GHI_CHU}
    <table style="width: 100%; border-collapse: collapse; font-size: 13px; margin-bottom: 15px;">
        <thead style="border-bottom: 1px solid #000;">
            <tr>
                <th style="text-align: left; padding-bottom: 5px;">Tên món</th>
                <th style="text-align: center; padding-bottom: 5px;">SL</th>
                <th style="text-align: right; padding-bottom: 5px;">T.Tiền</th>
            </tr>
        </thead>
        <tbody>
            <tr><td colspan="3" style="text-align: center; color: #f37021; padding: 10px 0;">{DANH_SACH_MON_AN}</td></tr>
        </tbody>
    </table>
    <div style="border-top: 1px dashed #000; padding-top: 10px;">
        <div style="display: flex; justify-content: space-between; font-weight: bold; font-size: 16px;">
            <span>TỔNG THANH TOÁN:</span>
            <span>{KHACH_CAN_TRA}</span>
        </div>
    </div>
    <div style="text-align: center; margin-top: 20px; font-size: 12px; font-style: italic;">
        Xin cảm ơn và hẹn gặp lại quý khách!
    </div>
</div>`;
  }

  const formatPrice = (price) => new Intl.NumberFormat("vi-VN").format(price);
  const now = new Date();

  let itemsHtml = orderData.items
    .map(
      (item) => `
        <tr>
            <td style="padding: 5px 0; border-bottom: 1px dotted #ccc; text-align: left;">
              ${item.name} 
              <br> 
              <small style="color:#666">${formatPrice(item.price)}</small>
              ${item.ghiChu ? `<br><small style="color:#e67e22; font-style:italic;">*Ghi chú: ${item.ghiChu}</small>` : ''}
            </td>
            <td style="text-align: center; padding: 5px 0; border-bottom: 1px dotted #ccc;">${item.qty}</td>
            <td style="text-align: right; padding: 5px 0; border-bottom: 1px dotted #ccc; font-weight: bold;">${formatPrice(item.price * item.qty)}</td>
        </tr>
    `,
    )
    .join("");

  // Thay thế các Mã Nhúng cơ bản
  let htmlContent = template
    .replace(/{TEN_CUA_HANG}/g, branchInfo.name || "POS36 RESTAURANT")
    .replace(/{DIA_CHI}/g, branchInfo.address || "Đà Nẵng, Việt Nam")
    .replace(/{DIEN_THOAI}/g, branchInfo.phone || "0901234567")
    .replace(
      /{MA_CHUNG_TU}/g,
      orderData.maChungTu || `HD${Date.now().toString().slice(-6)}`,
    )
    .replace(/{NGAY_IN}/g, now.toLocaleDateString("vi-VN"))
    .replace(/{GIO_IN}/g, now.toLocaleTimeString("vi-VN"))
    .replace(/{TEN_BAN}/g, orderData.tenBan || "Mang về")
    .replace(/{THU_NGAN}/g, localStorage.getItem("tenNhanVien") || "Admin")
    .replace(/{TONG_CONG}/g, formatPrice(orderData.tongTien))
    .replace(/{KHACH_CAN_TRA}/g, formatPrice(orderData.tongTien))
    .replace(/{GHI_CHU}/g, orderData.ghiChu ? `<div style="font-size: 12px; border-bottom: 1px dashed #000; padding-bottom: 8px; margin-bottom: 10px; font-style: italic; color: #333;"><b>Ghi chú HD:</b> ${orderData.ghiChu}</div>` : "");

  // THUẬT TOÁN THAY THẾ DANH SÁCH MÓN ĂN THÔNG MINH
  const rowRegex =
    /<tr[^>]*>[\s\S]*?<td[^>]*>[\s\S]*?{DANH_SACH_MON_AN}[\s\S]*?<\/td>[\s\S]*?<\/tr>/gi;
  if (rowRegex.test(htmlContent)) {
    htmlContent = htmlContent.replace(rowRegex, itemsHtml);
  } else {
    htmlContent = htmlContent.replace(/{DANH_SACH_MON_AN}/g, itemsHtml);
  }

  const iframe = document.createElement("iframe");
  iframe.style.display = "none";
  document.body.appendChild(iframe);

  iframe.contentWindow.document.open();
  iframe.contentWindow.document.write(`
        <html>
        <head>
            <title>In Hóa Đơn</title>
            <style>
                @media print {
                    @page { margin: 0; }
                    body { margin: 0.5cm; }
                }
            </style>
        </head>
        <body><div style="width: 80mm; max-width: 100%;">${htmlContent}</div></body>
        </html>
    `);
  iframe.contentWindow.document.close();

  setTimeout(() => {
    iframe.contentWindow.focus();
    iframe.contentWindow.print();
    setTimeout(() => document.body.removeChild(iframe), 1000);
  }, 500);
};
