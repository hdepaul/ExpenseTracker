# ============================================
# Azure Container Apps Setup for ExpenseTracker
# ============================================
# Alternativa sin restricciones de cuota de VMs
# ============================================

# Variables - MODIFICAR ESTOS VALORES
$SUFFIX = "hernan"
$RESOURCE_GROUP = "expense-tracker-rg"
$LOCATION = "eastus"  # Container Apps tiene mejor disponibilidad
$CONTAINER_ENV = "expense-tracker-env"
$CONTAINER_APP_NAME = "expense-tracker-api"
$SQL_SERVER_NAME = "expense-tracker-sql-$SUFFIX"
$SQL_ADMIN_USER = "sqladmin"
$SQL_ADMIN_PASSWORD = "Kakaroto2030!"
$SQL_DB_NAME = "ExpenseTrackerDb"
$GITHUB_USER = "hdepaul"

# JWT Settings
$JWT_KEY = "YourSuperSecretKeyThatIsAtLeast32CharactersLong!"
$JWT_ISSUER = "ExpenseTrackerAPI"
$JWT_AUDIENCE = "ExpenseTrackerClient"

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Azure Container Apps Setup" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan

# 1. Login
Write-Host ""
Write-Host "[1/8] Login a Azure..." -ForegroundColor Yellow
az login --use-device-code

# 2. Mostrar subscription
Write-Host ""
Write-Host "[2/8] Tu subscription:" -ForegroundColor Yellow
$SUBSCRIPTION_ID = az account show --query "id" -o tsv
Write-Host "Subscription ID: $SUBSCRIPTION_ID" -ForegroundColor Green

# 3. Registrar providers
Write-Host ""
Write-Host "[3/8] Registrando providers..." -ForegroundColor Yellow
az provider register --namespace Microsoft.App
az provider register --namespace Microsoft.OperationalInsights
az provider register --namespace Microsoft.Sql
Write-Host "Esperando 20 segundos..." -ForegroundColor Gray
Start-Sleep -Seconds 20

# 4. Crear/usar Resource Group
Write-Host ""
Write-Host "[4/8] Creando Resource Group..." -ForegroundColor Yellow
az group create --name $RESOURCE_GROUP --location $LOCATION

# 5. Crear SQL Server (probamos varias regiones)
Write-Host ""
Write-Host "[5/8] Creando SQL Server..." -ForegroundColor Yellow
$SQL_LOCATIONS = @("westus2", "centralus", "northeurope", "westeurope", "eastus2")
$SQL_CREATED = $false

foreach ($sqlLocation in $SQL_LOCATIONS) {
    Write-Host "  Probando region: $sqlLocation" -ForegroundColor Gray
    $result = az sql server create `
        --name $SQL_SERVER_NAME `
        --resource-group $RESOURCE_GROUP `
        --location $sqlLocation `
        --admin-user $SQL_ADMIN_USER `
        --admin-password $SQL_ADMIN_PASSWORD 2>&1

    if ($LASTEXITCODE -eq 0) {
        Write-Host "  SQL Server creado en $sqlLocation" -ForegroundColor Green
        $SQL_CREATED = $true
        $SQL_LOCATION = $sqlLocation
        break
    }
}

if (-not $SQL_CREATED) {
    Write-Host "ERROR: No se pudo crear SQL Server en ninguna region." -ForegroundColor Red
    Write-Host "Alternativa: Usa Azure SQL Database serverless o SQLite local." -ForegroundColor Yellow
    exit 1
}

# Firewall
Write-Host "  Configurando firewall..." -ForegroundColor Gray
az sql server firewall-rule create `
    --resource-group $RESOURCE_GROUP `
    --server $SQL_SERVER_NAME `
    --name AllowAzureServices `
    --start-ip-address 0.0.0.0 `
    --end-ip-address 0.0.0.0

# Database
Write-Host "  Creando database..." -ForegroundColor Gray
az sql db create `
    --resource-group $RESOURCE_GROUP `
    --server $SQL_SERVER_NAME `
    --name $SQL_DB_NAME `
    --service-objective Basic

# 6. Crear Container Apps Environment
Write-Host ""
Write-Host "[6/8] Creando Container Apps Environment..." -ForegroundColor Yellow
az containerapp env create `
    --name $CONTAINER_ENV `
    --resource-group $RESOURCE_GROUP `
    --location $LOCATION

# 7. Crear Container App
Write-Host ""
Write-Host "[7/8] Creando Container App..." -ForegroundColor Yellow

$CONNECTION_STRING = "Server=tcp:$SQL_SERVER_NAME.database.windows.net,1433;Database=$SQL_DB_NAME;User ID=$SQL_ADMIN_USER;Password=$SQL_ADMIN_PASSWORD;Encrypt=True;TrustServerCertificate=False;"

az containerapp create `
    --name $CONTAINER_APP_NAME `
    --resource-group $RESOURCE_GROUP `
    --environment $CONTAINER_ENV `
    --image "ghcr.io/$GITHUB_USER/expense-tracker-api:latest" `
    --target-port 8080 `
    --ingress external `
    --min-replicas 0 `
    --max-replicas 1 `
    --env-vars `
        "ConnectionStrings__DefaultConnection=$CONNECTION_STRING" `
        "Jwt__Key=$JWT_KEY" `
        "Jwt__Issuer=$JWT_ISSUER" `
        "Jwt__Audience=$JWT_AUDIENCE" `
        "Jwt__ExpirationMinutes=480" `
        "ASPNETCORE_ENVIRONMENT=Production"

# 8. Obtener URL
Write-Host ""
Write-Host "[8/8] Obteniendo URL de la app..." -ForegroundColor Yellow
$APP_URL = az containerapp show `
    --name $CONTAINER_APP_NAME `
    --resource-group $RESOURCE_GROUP `
    --query "properties.configuration.ingress.fqdn" -o tsv

# Resumen
Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "SETUP COMPLETADO!" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Recursos creados:" -ForegroundColor White
Write-Host "  - Resource Group: $RESOURCE_GROUP" -ForegroundColor Gray
Write-Host "  - SQL Server: $SQL_SERVER_NAME.database.windows.net" -ForegroundColor Gray
Write-Host "  - SQL Database: $SQL_DB_NAME" -ForegroundColor Gray
Write-Host "  - Container App: https://$APP_URL" -ForegroundColor Green
Write-Host ""
Write-Host "Para actualizar la app despues de un push:" -ForegroundColor Yellow
Write-Host "  az containerapp update --name $CONTAINER_APP_NAME --resource-group $RESOURCE_GROUP --image ghcr.io/$GITHUB_USER/expense-tracker-api:latest" -ForegroundColor White
Write-Host ""
