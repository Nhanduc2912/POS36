export const printReceipt = (orderData, branchInfo) => {
  let template = localStorage.getItem("pos36_print_template");
  if (!template) {
    alert("Chưa thiết lập mẫu in. Vui lòng vào Cài đặt -> Thiết lập mẫu in.");
    return;
  }

  const formatPrice = (price) => new Intl.NumberFormat("vi-VN").format(price);
  const now = new Date();

  let itemsHtml = orderData.items
    .map(
      (item) => `
        <tr>
            <td style="padding: 5px 0; border-bottom: 1px dotted #ccc; text-align: left;">${item.name} <br> <small style="color:#666">${formatPrice(item.price)}</small></td>
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
    .replace(/{GHI_CHU}/g, "");

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
