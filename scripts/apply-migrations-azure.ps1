cd C:\Development\ExpenseTracker

# Limpiar builds anteriores
dotnet clean --configuration Debug
dotnet clean --configuration Release

# Setear connection string - REEMPLAZAR CON TUS CREDENCIALES
$env:ConnectionStrings__DefaultConnection="Server=tcp:YOUR_SERVER.database.windows.net,1433;Database=YOUR_DATABASE;User ID=YOUR_USER;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=False;"

# Aplicar migraciones con Release
dotnet ef database update --project src/ExpenseTracker.Infrastructure --startup-project src/ExpenseTracker.API --configuration Release
