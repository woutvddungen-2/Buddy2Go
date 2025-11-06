run the following code in Server folder:

```
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Pomelo.EntityFrameworkCore.MySql
dotnet add package Microsoft.IdentityModel.Tokens
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package System.IdentityModel.Tokens.Jwt
dotnet add package Swashbuckle.AspNetCore

```

create database: 
```
dev.mysql.com/downloads/mysql
install mysql.

got to project folder with terminal:
cd .\Server\
dotnet tool restore
dotnet ef migrations add initialcreate
dotnet ef database update
```

