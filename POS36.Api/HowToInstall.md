** Cài 3 cái này đã **

dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.EntityFrameworkCore.Design

** Code fist **

dotnet ef migrations add InitialCreate
dotnet ef database update
