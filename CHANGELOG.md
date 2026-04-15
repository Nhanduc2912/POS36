# Changelog

All notable changes to POS36 project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Planned Features
- Mobile app (iOS/Android)
- Multi-language support
- Advanced analytics dashboard
- Integration with accounting software
- Offline mode support

---

## [1.0.0] - 2026-04-16

### 🎉 Initial Release

#### Added

##### Core Features
- **Multi-tenant Architecture**: Support for multiple stores and branches
- **Role-Based Access Control (RBAC)**: 5 user roles with dedicated interfaces
  - Store Owner (Chủ Cửa Hàng)
  - Manager (Quản Lý)
  - Cashier (Thu Ngân)
  - Waiter (Phục Vụ)
  - Kitchen (Bếp)

##### Order Management
- Real-time order placement and tracking
- Table management (transfer, merge, split)
- Order modification and cancellation
- Kitchen Display System (KDS)
- Order status tracking

##### Payment System
- Multiple payment methods (Cash, Bank Transfer, Card)
- QR Code payment integration
- Bank webhook for automatic payment confirmation
- Receipt printing with customizable templates
- Split bill functionality

##### Inventory Management
- Stock tracking by branch
- Purchase order management
- Stock adjustment and inventory check
- Low stock alerts
- Stock movement history

##### Customer Management
- Customer loyalty program
- Points accumulation and redemption
- Customer purchase history
- Member registration

##### Reporting & Analytics
- Dashboard with 7-day revenue chart
- Sales reports (daily, monthly, yearly)
- Top-selling products
- Cash book (income/expense tracking)
- AI-powered business insights

##### AI Integration
- Google Gemini 2.5 Flash AI Copilot
- Natural language business queries
- Automated report generation
- Data analysis and recommendations

##### Real-time Features (SignalR)
- Live order updates to kitchen
- Real-time table status
- Payment notifications
- Multi-device synchronization

##### Technical Features
- JWT Authentication
- RESTful API with Swagger documentation
- Entity Framework Core with SQL Server
- Vue.js 3 with Composition API
- Responsive design with Bootstrap 5
- Docker support for easy deployment

#### Backend Stack
- ASP.NET Core 9.0
- Entity Framework Core 9.0
- SQL Server 2022
- SignalR for real-time communication
- BCrypt for password hashing
- Serilog for structured logging

#### Frontend Stack
- Vue.js 3.5.30
- Vue Router 5.0.3
- Axios 1.13.6
- Bootstrap 5.3.8
- Chart.js 4.5.1
- SweetAlert2 11.26.23
- Vite 8.0.0

#### Database Schema
- 20 tables across 6 subsystems
- Multi-tenant data isolation
- Optimized indexes for performance
- Foreign key constraints for data integrity

#### Documentation
- Comprehensive README with ERD and Use Case diagrams
- Quick Start guide
- API documentation with Swagger
- Contributing guidelines
- Docker deployment guide

#### DevOps
- Docker Compose configuration
- Automated setup scripts (Windows BAT files)
- Environment configuration templates
- CI/CD ready structure

---

## Version History

### Version Numbering

We use Semantic Versioning (MAJOR.MINOR.PATCH):

- **MAJOR**: Incompatible API changes
- **MINOR**: New features (backward compatible)
- **PATCH**: Bug fixes (backward compatible)

### Release Schedule

- **Major releases**: Every 6 months
- **Minor releases**: Every 2 months
- **Patch releases**: As needed for critical bugs

---

## Migration Guides

### Upgrading from Beta to 1.0.0

If you were using the beta version:

1. **Backup your database**
   ```bash
   sqlcmd -S localhost -U sa -P YourPassword -Q "BACKUP DATABASE POS36_Db TO DISK='C:\Backup\POS36_Db.bak'"
   ```

2. **Update dependencies**
   ```bash
   cd POS36.Api
   dotnet restore
   
   cd ../POS36.Web
   npm install
   ```

3. **Run migrations**
   ```bash
   cd POS36.Api
   dotnet ef database update
   ```

4. **Update configuration**
   - Check `appsettings.json` for new settings
   - Update environment variables if using Docker

---

## Breaking Changes

### 1.0.0
- Initial release - no breaking changes

---

## Deprecations

### 1.0.0
- None

---

## Security Updates

### 1.0.0
- Implemented JWT authentication
- Password hashing with BCrypt (cost factor: 12)
- SQL injection prevention with parameterized queries
- XSS protection
- CORS configuration
- HTTPS enforcement (production)

---

## Known Issues

### 1.0.0
- None reported yet

---

## Contributors

Thanks to all contributors who helped make POS36 possible!

- [@contributor1](https://github.com/contributor1) - Project Lead
- [@contributor2](https://github.com/contributor2) - Backend Developer
- [@contributor3](https://github.com/contributor3) - Frontend Developer
- [@contributor4](https://github.com/contributor4) - UI/UX Designer

---

## Support

For questions and support:
- 📧 Email: support@pos36.com
- 💬 Discord: [POS36 Community](https://discord.gg/pos36)
- 🐛 Issues: [GitHub Issues](https://github.com/your-repo/POS36/issues)
- 📖 Docs: [Documentation](https://docs.pos36.com)

---

<div align="center">

**[⬆ Back to Top](#changelog)**

</div>
