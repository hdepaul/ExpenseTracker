# ============================================
# Azure Setup Script for ExpenseTracker
# ============================================
# Ejecutar en PowerShell como administrador
# ============================================

# Variables - MODIFICAR ESTOS VALORES
$SUFFIX = "hernan"                              # Cambiar por tu nombre/identificador único
$RESOURCE_GROUP = "expense-tracker-rg"
$LOCATION = "westus2"
$SQL_SERVER_NAME = "expense-tracker-sql-$SUFFIX"
$SQL_ADMIN_USER = "sqladmin"
$SQL_ADMIN_PASSWORD = "Kakaroto2030!"     # Cambiar por password seguro
$SQL_DB_NAME = "ExpenseTrackerDb"
$APP_SERVICE_PLAN = "expense-tracker-plan"
$WEB_APP_NAME = "expense-tracker-api-$SUFFIX"
$GITHUB_USER = "hdepaul"                 # Cambiar por tu usuario de GitHub

# JWT Settings
$JWT_KEY = "YourSuperSecretKeyThatIsAtLeast32CharactersLong!"
$JWT_ISSUER = "ExpenseTrackerAPI"
$JWT_AUDIENCE = "ExpenseTrackerClient"

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Azure ExpenseTracker Setup" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# 1. Login a Azure
Write-Host "[1/10] Iniciando sesion en Azure..." -ForegroundColor Yellow
az login

# 2. Mostrar subscription
Write-Host ""
Write-Host "[2/10] Tu subscription:" -ForegroundColor Yellow
az account show --query "{SubscriptionId:id, Name:name}" -o table

# Guardar subscription ID para después
$SUBSCRIPTION_ID = az account show --query "id" -o tsv
Write-Host "Subscription ID: $SUBSCRIPTION_ID" -ForegroundColor Green

# 2.5 Registrar providers necesarios
Write-Host ""
Write-Host "[2.5/10] Registrando providers (puede tardar unos minutos)..." -ForegroundColor Yellow
az provider register --namespace Microsoft.Sql
az provider register --namespace Microsoft.Web
Write-Host "Esperando que se registren..." -ForegroundColor Gray
Start-Sleep -Seconds 30

# 3. Crear Resource Group
Write-Host ""
Write-Host "[3/10] Creando Resource Group..." -ForegroundColor Yellow
az group create --name $RESOURCE_GROUP --location $LOCATION

# 4. Crear SQL Server
Write-Host ""
Write-Host "[4/10] Creando SQL Server (puede tardar unos minutos)..." -ForegroundColor Yellow
az sql server create `
  --name $SQL_SERVER_NAME `
  --resource-group $RESOURCE_GROUP `
  --location $LOCATION `
  --admin-user $SQL_ADMIN_USER `
  --admin-password $SQL_ADMIN_PASSWORD

# 5. Configurar Firewall del SQL Server
Write-Host ""
Write-Host "[5/10] Configurando firewall del SQL Server..." -ForegroundColor Yellow
az sql server firewall-rule create `
  --resource-group $RESOURCE_GROUP `
  --server $SQL_SERVER_NAME `
  --name AllowAzureServices `
  --start-ip-address 0.0.0.0 `
  --end-ip-address 0.0.0.0

# 6. Crear SQL Database
Write-Host ""
Write-Host "[6/10] Creando SQL Database..." -ForegroundColor Yellow
az sql db create `
  --resource-group $RESOURCE_GROUP `
  --server $SQL_SERVER_NAME `
  --name $SQL_DB_NAME `
  --service-objective Basic

# 7. Crear App Service Plan (Linux, Basic tier B1 ~$13/mes)
Write-Host ""
Write-Host "[7/10] Creando App Service Plan..." -ForegroundColor Yellow
az appservice plan create `
  --name $APP_SERVICE_PLAN `
  --resource-group $RESOURCE_GROUP `
  --location $LOCATION `
  --sku B1 `
  --is-linux

# 8. Crear Web App
Write-Host ""
Write-Host "[8/10] Creando Web App..." -ForegroundColor Yellow
az webapp create `
  --name $WEB_APP_NAME `
  --resource-group $RESOURCE_GROUP `
  --plan $APP_SERVICE_PLAN `
  --container-image-name "ghcr.io/$GITHUB_USER/expense-tracker-api:latest"

# 9. Configurar App Settings
Write-Host ""
Write-Host "[9/10] Configurando App Settings..." -ForegroundColor Yellow
az webapp config appsettings set `
  --name $WEB_APP_NAME `
  --resource-group $RESOURCE_GROUP `
  --settings `
    Jwt__Key="$JWT_KEY" `
    Jwt__Issuer="$JWT_ISSUER" `
    Jwt__Audience="$JWT_AUDIENCE" `
    Jwt__ExpirationMinutes="480" `
    WEBSITES_PORT="8080"

# Configurar Connection String
$CONNECTION_STRING = "Server=tcp:$SQL_SERVER_NAME.database.windows.net,1433;Database=$SQL_DB_NAME;User ID=$SQL_ADMIN_USER;Password=$SQL_ADMIN_PASSWORD;Encrypt=True;TrustServerCertificate=False;"

az webapp config connection-string set `
  --name $WEB_APP_NAME `
  --resource-group $RESOURCE_GROUP `
  --connection-string-type SQLAzure `
  --settings DefaultConnection="$CONNECTION_STRING"

# 10. Crear credenciales para GitHub Actions
Write-Host ""
Write-Host "[10/10] Creando credenciales para GitHub Actions..." -ForegroundColor Yellow
Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host "IMPORTANTE: Copia el JSON que aparece abajo" -ForegroundColor Green
Write-Host "y guardalo como secret AZURE_CREDENTIALS en GitHub" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host ""

az ad sp create-for-rbac `
  --name "github-actions-expense-tracker" `
  --role contributor `
  --scopes "/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP" `
  --sdk-auth

# Resumen final
Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "SETUP COMPLETADO!" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Recursos creados:" -ForegroundColor White
Write-Host "  - Resource Group: $RESOURCE_GROUP" -ForegroundColor Gray
Write-Host "  - SQL Server: $SQL_SERVER_NAME.database.windows.net" -ForegroundColor Gray
Write-Host "  - SQL Database: $SQL_DB_NAME" -ForegroundColor Gray
Write-Host "  - App Service: https://$WEB_APP_NAME.azurewebsites.net" -ForegroundColor Gray
Write-Host ""
Write-Host "Proximos pasos:" -ForegroundColor Yellow
Write-Host "  1. Copia el JSON de arriba" -ForegroundColor White
Write-Host "  2. Ve a GitHub > tu repo > Settings > Secrets > Actions" -ForegroundColor White
Write-Host "  3. Crea un secret llamado AZURE_CREDENTIALS con el JSON" -ForegroundColor White
Write-Host "  4. Actualiza el archivo cd.yml para habilitar el deploy" -ForegroundColor White
Write-Host ""
