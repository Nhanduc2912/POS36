** Cài 3 cái này đã **

dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 9.0.0
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 9.0.0
dotnet add package Microsoft.EntityFrameworkCore.Design --version 9.0.0

** Code fist **

dotnet ef migrations add InitialCreate
dotnet ef database update
