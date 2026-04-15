# 🤝 Hướng Dẫn Đóng Góp cho POS36

Cảm ơn bạn đã quan tâm đến việc đóng góp cho POS36! Chúng tôi hoan nghênh mọi đóng góp từ cộng đồng.

## 📋 Mục Lục

- [Code of Conduct](#-code-of-conduct)
- [Làm Thế Nào Để Đóng Góp](#-làm-thế-nào-để-đóng-góp)
- [Quy Trình Phát Triển](#-quy-trình-phát-triển)
- [Coding Standards](#-coding-standards)
- [Commit Messages](#-commit-messages)
- [Pull Request Process](#-pull-request-process)
- [Báo Cáo Lỗi](#-báo-cáo-lỗi)
- [Đề Xuất Tính Năng](#-đề-xuất-tính-năng)

---

## 📜 Code of Conduct

### Cam Kết Của Chúng Tôi

Chúng tôi cam kết tạo ra một môi trường thân thiện, an toàn và chào đón cho tất cả mọi người, bất kể:
- Tuổi tác
- Giới tính
- Khuyết tật
- Dân tộc
- Kinh nghiệm
- Quốc tịch
- Tôn giáo

### Hành Vi Được Khuyến Khích

- ✅ Sử dụng ngôn ngữ thân thiện và chào đón
- ✅ Tôn trọng quan điểm và kinh nghiệm khác nhau
- ✅ Chấp nhận phê bình mang tính xây dựng
- ✅ Tập trung vào điều tốt nhất cho cộng đồng
- ✅ Thể hiện sự đồng cảm với các thành viên khác

### Hành Vi Không Được Chấp Nhận

- ❌ Sử dụng ngôn ngữ hoặc hình ảnh khiêu dâm
- ❌ Bình luận xúc phạm hoặc miệt thị
- ❌ Quấy rối công khai hoặc riêng tư
- ❌ Công bố thông tin cá nhân của người khác
- ❌ Hành vi không chuyên nghiệp khác

---

## 🚀 Làm Thế Nào Để Đóng Góp

### 1. Fork Repository

```bash
# Fork trên GitHub, sau đó clone về máy
git clone https://github.com/your-username/POS36.git
cd POS36
```

### 2. Tạo Branch Mới

```bash
# Tạo branch từ main
git checkout -b feature/your-feature-name

# Hoặc cho bugfix
git checkout -b fix/bug-description
```

### 3. Thực Hiện Thay Đổi

- Viết code theo [Coding Standards](#-coding-standards)
- Thêm tests nếu cần
- Cập nhật documentation

### 4. Commit Changes

```bash
git add .
git commit -m "feat: add new feature"
```

### 5. Push to GitHub

```bash
git push origin feature/your-feature-name
```

### 6. Tạo Pull Request

- Truy cập repository trên GitHub
- Click "New Pull Request"
- Điền thông tin chi tiết về thay đổi
- Đợi review từ maintainers

---

## 🔄 Quy Trình Phát Triển

### Branch Strategy

Chúng tôi sử dụng **Git Flow**:

```
main (production)
  ↓
develop (development)
  ↓
feature/* (new features)
fix/* (bug fixes)
hotfix/* (urgent fixes)
release/* (release preparation)
```

### Branch Naming Convention

| Type | Format | Example |
|------|--------|---------|
| Feature | `feature/description` | `feature/add-customer-loyalty` |
| Bug Fix | `fix/description` | `fix/payment-calculation-error` |
| Hotfix | `hotfix/description` | `hotfix/security-vulnerability` |
| Release | `release/version` | `release/v1.2.0` |
| Docs | `docs/description` | `docs/update-readme` |
| Refactor | `refactor/description` | `refactor/optimize-database-queries` |

---

## 💻 Coding Standards

### Backend (C# / .NET)

#### Naming Conventions

```csharp
// PascalCase cho classes, methods, properties
public class HoaDonController : ControllerBase
{
    public async Task<IActionResult> GetHoaDon(int id)
    {
        // camelCase cho local variables
        var hoaDon = await _context.HoaDons.FindAsync(id);
        return Ok(hoaDon);
    }
}

// Private fields với underscore prefix
private readonly AppDbContext _context;
private readonly ILogger<HoaDonController> _logger;
```

#### Code Style

```csharp
// ✅ GOOD
public async Task<ActionResult<HoaDon>> CreateHoaDon(HoaDonDto dto)
{
    if (dto == null)
        return BadRequest("Dữ liệu không hợp lệ");
    
    var hoaDon = new HoaDon
    {
        BanId = dto.BanId,
        TongTien = dto.TongTien,
        NgayTao = DateTime.Now
    };
    
    _context.HoaDons.Add(hoaDon);
    await _context.SaveChangesAsync();
    
    return CreatedAtAction(nameof(GetHoaDon), new { id = hoaDon.Id }, hoaDon);
}

// ❌ BAD
public async Task<ActionResult<HoaDon>> CreateHoaDon(HoaDonDto dto){
if(dto==null)return BadRequest("Dữ liệu không hợp lệ");
var hoaDon=new HoaDon{BanId=dto.BanId,TongTien=dto.TongTien,NgayTao=DateTime.Now};
_context.HoaDons.Add(hoaDon);await _context.SaveChangesAsync();return CreatedAtAction(nameof(GetHoaDon),new{id=hoaDon.Id},hoaDon);}
```

#### Comments

```csharp
// ✅ GOOD - Giải thích "tại sao", không phải "cái gì"
// Tính điểm tích lũy: 1 điểm cho mỗi 10,000 VNĐ
var diemTichLuy = (int)(tongTien / 10000);

// ❌ BAD - Chỉ mô tả code
// Chia tổng tiền cho 10000
var diemTichLuy = (int)(tongTien / 10000);
```

### Frontend (Vue.js / JavaScript)

#### Naming Conventions

```javascript
// PascalCase cho components
export default {
  name: 'DashboardOverview',
  components: {
    ChartComponent,
    StatCard
  }
}

// camelCase cho variables, functions
const totalRevenue = 0;
function calculateTotal() { }

// UPPER_SNAKE_CASE cho constants
const API_BASE_URL = 'http://localhost:5198/api';
const MAX_RETRY_ATTEMPTS = 3;
```

#### Vue Component Structure

```vue
<template>
  <!-- Template content -->
</template>

<script>
export default {
  name: 'ComponentName',
  
  // 1. Props
  props: {
    title: String,
    data: Array
  },
  
  // 2. Data
  data() {
    return {
      items: []
    }
  },
  
  // 3. Computed
  computed: {
    filteredItems() {
      return this.items.filter(item => item.active);
    }
  },
  
  // 4. Watch
  watch: {
    data(newVal) {
      this.items = newVal;
    }
  },
  
  // 5. Lifecycle Hooks
  mounted() {
    this.fetchData();
  },
  
  // 6. Methods
  methods: {
    async fetchData() {
      // Implementation
    }
  }
}
</script>

<style scoped>
/* Component styles */
</style>
```

#### Code Style

```javascript
// ✅ GOOD
async function fetchHoaDon(id) {
  try {
    const response = await axios.get(`/api/hoadon/${id}`);
    return response.data;
  } catch (error) {
    console.error('Lỗi khi lấy hóa đơn:', error);
    throw error;
  }
}

// ❌ BAD
async function fetchHoaDon(id){
try{const response=await axios.get(`/api/hoadon/${id}`);return response.data;}catch(error){console.error('Lỗi khi lấy hóa đơn:',error);throw error;}}
```

### Database

#### Table Naming
- PascalCase, số ít: `HoaDon`, `SanPham`, `NhanVien`

#### Column Naming
- PascalCase: `TenSanPham`, `GiaBan`, `NgayTao`

#### Foreign Keys
- Format: `{TableName}Id`
- Example: `CuaHangId`, `ChiNhanhId`, `BanId`

---

## 📝 Commit Messages

Chúng tôi sử dụng **Conventional Commits**:

### Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types

| Type | Mô Tả | Example |
|------|-------|---------|
| `feat` | Tính năng mới | `feat(order): add table transfer feature` |
| `fix` | Sửa lỗi | `fix(payment): correct total calculation` |
| `docs` | Cập nhật documentation | `docs(readme): add installation guide` |
| `style` | Format code (không ảnh hưởng logic) | `style(api): format code with prettier` |
| `refactor` | Refactor code | `refactor(database): optimize queries` |
| `test` | Thêm/sửa tests | `test(order): add unit tests` |
| `chore` | Maintenance tasks | `chore(deps): update dependencies` |
| `perf` | Cải thiện performance | `perf(api): add caching layer` |

### Examples

```bash
# Feature
git commit -m "feat(kitchen): add real-time order notification"

# Bug fix
git commit -m "fix(pos): resolve payment rounding error"

# Documentation
git commit -m "docs(api): update swagger documentation"

# Refactor
git commit -m "refactor(auth): simplify JWT token generation"

# Multiple lines
git commit -m "feat(customer): add loyalty points system

- Add points calculation logic
- Create customer rewards table
- Implement points redemption API"
```

---

## 🔍 Pull Request Process

### 1. Checklist Trước Khi Submit

- [ ] Code tuân thủ coding standards
- [ ] Đã test trên local
- [ ] Đã thêm/cập nhật tests (nếu cần)
- [ ] Đã cập nhật documentation
- [ ] Không có conflicts với branch `develop`
- [ ] Build thành công
- [ ] Đã self-review code

### 2. PR Template

```markdown
## Mô Tả
Mô tả ngắn gọn về thay đổi

## Loại Thay Đổi
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Checklist
- [ ] Code tuân thủ style guide
- [ ] Self-review hoàn tất
- [ ] Comments đã được thêm vào code phức tạp
- [ ] Documentation đã được cập nhật
- [ ] Không có warnings mới
- [ ] Tests đã được thêm/cập nhật
- [ ] Tests pass locally

## Screenshots (nếu có)
Thêm screenshots cho UI changes

## Related Issues
Closes #123
```

### 3. Review Process

1. **Automated Checks**: CI/CD sẽ chạy tự động
2. **Code Review**: Ít nhất 1 maintainer phải approve
3. **Testing**: QA team sẽ test (nếu cần)
4. **Merge**: Maintainer sẽ merge vào `develop`

### 4. Sau Khi Merge

- Branch sẽ được xóa tự động
- Thay đổi sẽ được deploy lên staging
- Release notes sẽ được cập nhật

---

## 🐛 Báo Cáo Lỗi

### Trước Khi Báo Cáo

1. Kiểm tra [Issues](https://github.com/your-repo/POS36/issues) xem lỗi đã được báo cáo chưa
2. Đảm bảo bạn đang sử dụng phiên bản mới nhất
3. Thu thập thông tin chi tiết về lỗi

### Bug Report Template

```markdown
## Mô Tả Lỗi
Mô tả rõ ràng và ngắn gọn về lỗi

## Các Bước Tái Hiện
1. Vào trang '...'
2. Click vào '...'
3. Scroll xuống '...'
4. Thấy lỗi

## Kết Quả Mong Đợi
Mô tả những gì bạn mong đợi sẽ xảy ra

## Kết Quả Thực Tế
Mô tả những gì thực sự xảy ra

## Screenshots
Nếu có, thêm screenshots

## Môi Trường
- OS: [e.g. Windows 11]
- Browser: [e.g. Chrome 120]
- Version: [e.g. 1.0.0]

## Thông Tin Bổ Sung
Thêm bất kỳ thông tin nào khác về vấn đề
```

---

## 💡 Đề Xuất Tính Năng

### Feature Request Template

```markdown
## Vấn Đề Cần Giải Quyết
Mô tả vấn đề bạn đang gặp phải

## Giải Pháp Đề Xuất
Mô tả giải pháp bạn muốn

## Giải Pháp Thay Thế
Mô tả các giải pháp thay thế bạn đã xem xét

## Thông Tin Bổ Sung
Thêm bất kỳ thông tin nào khác về feature request
```

---

## 🧪 Testing Guidelines

### Backend Tests

```csharp
[Fact]
public async Task CreateHoaDon_ValidData_ReturnsCreatedResult()
{
    // Arrange
    var dto = new HoaDonDto { BanId = 1, TongTien = 100000 };
    
    // Act
    var result = await _controller.CreateHoaDon(dto);
    
    // Assert
    var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
    var hoaDon = Assert.IsType<HoaDon>(createdResult.Value);
    Assert.Equal(100000, hoaDon.TongTien);
}
```

### Frontend Tests

```javascript
describe('DashboardOverview', () => {
  it('renders revenue chart', async () => {
    const wrapper = mount(DashboardOverview);
    await wrapper.vm.$nextTick();
    
    expect(wrapper.find('.revenue-chart').exists()).toBe(true);
  });
});
```

---

## 📚 Documentation

### Code Documentation

```csharp
/// <summary>
/// Tạo hóa đơn mới cho bàn
/// </summary>
/// <param name="dto">Thông tin hóa đơn</param>
/// <returns>Hóa đơn vừa tạo</returns>
/// <exception cref="ArgumentNullException">Khi dto là null</exception>
[HttpPost]
public async Task<ActionResult<HoaDon>> CreateHoaDon(HoaDonDto dto)
{
    // Implementation
}
```

### API Documentation

- Sử dụng Swagger/OpenAPI
- Thêm XML comments cho controllers
- Cập nhật Postman collection

---

## 🎯 Priorities

### High Priority
- 🔴 Security vulnerabilities
- 🔴 Critical bugs
- 🔴 Performance issues

### Medium Priority
- 🟡 Feature requests
- 🟡 UI/UX improvements
- 🟡 Documentation updates

### Low Priority
- 🟢 Code refactoring
- 🟢 Minor bugs
- 🟢 Style improvements

---

## 📞 Liên Hệ

- **Email**: dev@pos36.com
- **Discord**: [POS36 Community](https://discord.gg/pos36)
- **GitHub Discussions**: [Discussions](https://github.com/your-repo/POS36/discussions)

---

## 🙏 Cảm Ơn

Cảm ơn bạn đã dành thời gian đóng góp cho POS36! Mỗi đóng góp, dù lớn hay nhỏ, đều rất có ý nghĩa với chúng tôi.

---

<div align="center">

**Made with ❤️ by POS36 Community**

[⬆ Back to Top](#-hướng-dẫn-đóng-góp-cho-pos36)

</div>
