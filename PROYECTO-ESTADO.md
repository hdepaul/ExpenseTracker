# ExpenseTracker - Estado del Proyecto

## Resumen
Aplicación de control de gastos con:
- **Backend**: .NET 10 (Clean Architecture + CQRS con MediatR)
- **Frontend**: Angular 19 (Standalone Components + Signals)
- **Base de datos**: SQL Server (local) / Azure SQL (producción)

---

## Estado Actual (Febrero 2026)

### ✅ Completado

#### Backend (.NET)
- [x] Clean Architecture (Domain, Application, Infrastructure, API)
- [x] CQRS con MediatR
- [x] JWT Authentication
- [x] CRUD de Expenses con paginación
- [x] Filtro por mes
- [x] Totales y agrupado por categoría
- [x] Categorías: Food, Transportation, Housing, Entertainment, Shopping, Healthcare, Utilities, Taxes, Services, Subscriptions, Credit Card, Nafta, Comida, Other

#### Frontend (Angular)
- [x] Login / Register
- [x] Lista de gastos con paginación
- [x] Crear / Editar / Eliminar gastos
- [x] Modal de confirmación custom
- [x] Navbar con menú
- [x] Filtro por mes
- [x] Totales y breakdown por categoría
- [x] Internacionalización (i18n) - Español/Inglés con ngx-translate
- [x] Gráfico de torta (Reports) con ng2-charts
- [x] **Responsive/Mobile** - Todas las páginas adaptadas
- [x] Interceptor que maneja token expirado (redirige a login)

#### DevOps
- [x] Dockerfile para la API
- [x] docker-compose.yml para desarrollo local
- [x] GitHub Actions CI (build + test)
- [x] GitHub Actions CD (build + push imagen a GHCR)
- [x] CD actualizado para deploy a Azure Container Apps (pendiente probar)

---

### 🔄 En Progreso - Deploy a Azure

#### Recursos Azure creados:
- Resource Group: `expense-tracker-rg`
- SQL Server: `expense-tracker-sql-hernan` (en alguna región que funcionó)
- SQL Database: `ExpenseTrackerDb`
- Container App Environment: `expense-tracker-env` (posiblemente)
- Container App: `expense-tracker-api` (pendiente crear)

#### Pendiente para completar deploy:
1. **Correr el script** `scripts/azure-container-apps-setup.ps1` para crear el Container App
2. **Crear secret en GitHub**: `AZURE_CREDENTIALS` con el JSON del service principal
3. **Probar el flujo completo**: push → build → deploy automático

#### Cómo obtener AZURE_CREDENTIALS:
```powershell
az ad sp create-for-rbac --name "github-actions-expense-tracker" --role contributor --scopes /subscriptions/TU_SUBSCRIPTION_ID/resourceGroups/expense-tracker-rg --sdk-auth
```
El JSON que devuelve va como secret `AZURE_CREDENTIALS` en GitHub → Settings → Secrets → Actions.

---

### ❌ Pendiente / Ideas Futuras
- [ ] Refresh tokens (JWT)
- [ ] Deploy del frontend Angular a Azure Static Web Apps
- [ ] Dominio personalizado
- [ ] HTTPS/SSL
- [ ] Validaciones más robustas
- [ ] Tests unitarios frontend
- [ ] PWA (Progressive Web App)

---

## Estructura de Carpetas

```
C:\Development\
├── ExpenseTracker\              # Backend .NET
│   ├── src\
│   │   ├── ExpenseTracker.API\
│   │   ├── ExpenseTracker.Application\
│   │   ├── ExpenseTracker.Domain\
│   │   └── ExpenseTracker.Infrastructure\
│   ├── scripts\
│   │   ├── azure-setup.ps1                    # (primer intento, App Service)
│   │   ├── azure-container-apps-setup.ps1     # (actual, Container Apps)
│   │   └── cleanup.txt
│   └── .github\workflows\
│       ├── ci.yml
│       └── cd.yml
│
└── angular\test1\my-app\        # Frontend Angular
    └── src\
        ├── app\
        │   ├── components\      # navbar, confirm-modal
        │   ├── pages\           # home, login, register, expenses, reports
        │   ├── services\        # auth, expense
        │   ├── interceptors\    # auth interceptor
        │   └── models\
        └── assets\i18n\         # en.json, es.json
```

---

## Comandos Útiles

### Desarrollo Local
```powershell
# Backend
cd C:\Development\ExpenseTracker
dotnet run --project src/ExpenseTracker.API

# Frontend
cd C:\Development\angular\test1\my-app
ng serve

# Docker local
docker-compose up
```

### Azure CLI
```powershell
# Login
az login --use-device-code

# Ver recursos
az group list -o table
az containerapp list -o table

# Logs del container
az containerapp logs show --name expense-tracker-api --resource-group expense-tracker-rg

# Actualizar imagen manualmente
az containerapp update --name expense-tracker-api --resource-group expense-tracker-rg --image ghcr.io/hdepaul/expense-tracker-api:latest

# Borrar todo y empezar de nuevo
az group delete --name expense-tracker-rg --yes
```

---

## Configuración

### Backend (appsettings.json)
- JWT Key, Issuer, Audience, ExpirationMinutes
- ConnectionString a SQL Server

### Frontend
- API URL en environment.ts (actualmente localhost:5000)
- Cuando esté en Azure, cambiar a la URL del Container App

---

## URLs
- **GitHub Repo**: github.com/hdepaul/ExpenseTracker
- **GitHub Packages**: github.com/hdepaul?tab=packages
- **Azure Portal**: portal.azure.com
- **Container App URL**: (pendiente) https://expense-tracker-api.xxxxx.azurecontainerapps.io

---

## Próxima Sesión
1. Terminar deploy a Azure (crear Container App, configurar secrets)
2. Probar que el flujo push → deploy funcione
3. Configurar el frontend para apuntar a la API en Azure
4. (Opcional) Deploy del frontend a Azure Static Web Apps
