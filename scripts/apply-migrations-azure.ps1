cd C:\Development\ExpenseTracker

# Limpiar builds anteriores
dotnet clean --configuration Debug
dotnet clean --configuration Release

# Setear connection string
$env:ConnectionStrings__DefaultConnection="Server=tcp:expense-tracker-sql-hernan.database.windows.net,1433;Database=ExpenseTrackerDb;User ID=sqladmin;Password=Kakaroto2030!;Encrypt=True;TrustServerCertificate=False;"

# Aplicar migraciones con Release
dotnet ef database update --project src/ExpenseTracker.Infrastructure --startup-project src/ExpenseTracker.API --configuration Release
