# ExpenseTracker - Estado del Proyecto

## Resumen
Aplicación de control de gastos con:
- **Backend**: .NET 10 (Clean Architecture + CQRS con MediatR)
- **Frontend**: Angular 19 (Standalone Components + Signals)
- **Base de datos**: SQL Server (local) / Azure SQL (producción)
- **Deploy**: Azure Container Apps + GitHub Actions

---

## URLs de Producción

| Recurso | URL |
|---------|-----|
| **API** | https://expense-tracker-api.kindmoss-4320f913.eastus.azurecontainerapps.io |
| **GitHub Repo** | github.com/hdepaul/ExpenseTracker |
| **GitHub Packages** | github.com/hdepaul?tab=packages |

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
- [x] CORS configurado para desarrollo y producción

#### Frontend (Angular)
- [x] Login / Register
- [x] Lista de gastos con paginación
- [x] Crear / Editar / Eliminar gastos
- [x] Modal de confirmación custom
- [x] Navbar con menú hamburguesa (mobile)
- [x] Filtro por mes
- [x] Totales y breakdown por categoría
- [x] Internacionalización (i18n) - Español/Inglés con ngx-translate
- [x] Gráfico de torta (Reports) con ng2-charts
- [x] Responsive/Mobile - Todas las páginas adaptadas
- [x] Interceptor que maneja token expirado (redirige a login)
- [x] Environments configurados (dev/prod)

#### DevOps / Azure
- [x] Dockerfile para la API
- [x] docker-compose.yml para desarrollo local
- [x] GitHub Actions CI (build + test)
- [x] GitHub Actions CD (build + push imagen a GHCR + deploy a Azure)
- [x] Azure Container Apps funcionando
- [x] Azure SQL Database funcionando
- [x] EF Core Migrations en el repo
- [x] Secret AZURE_CREDENTIALS configurado en GitHub
- [x] Deploy automático en cada push a main

---

## Arquitectura de Deploy

```
┌─────────────────────────────────────────────────────────────────────┐
│                         TU MÁQUINA (Dev)                            │
├─────────────────────────────────────────────────────────────────────┤
│  Código → git push                                                  │
└─────────────────────────────────────────────────────────────────────┘
                                 │
                                 ▼
┌─────────────────────────────────────────────────────────────────────┐
│                      GITHUB ACTIONS (CI/CD)                         │
├─────────────────────────────────────────────────────────────────────┤
│  1. Build + Tests (.NET)                                            │
│  2. Build imagen Docker                                             │
│  3. Push a GitHub Container Registry (GHCR)                         │
│  4. Deploy a Azure Container Apps                                   │
└─────────────────────────────────────────────────────────────────────┘
                                 │
                                 ▼
┌─────────────────────────────────────────────────────────────────────┐
│                           AZURE                                      │
├─────────────────────────────────────────────────────────────────────┤
│  Container Apps ← descarga imagen de GHCR                           │
│       │                                                              │
│       ├── MigrateAsync() → aplica migraciones                       │
│       ├── SeedAsync() → crea categorías                             │
│       └── Escucha en puerto 8080                                    │
│                                                                      │
│  Azure SQL Database ← expense-tracker-sql-hernan                    │
└─────────────────────────────────────────────────────────────────────┘
```

---

## Recursos Azure

| Recurso | Nombre |
|---------|--------|
| Resource Group | expense-tracker-rg |
| SQL Server | expense-tracker-sql-hernan.database.windows.net |
| SQL Database | ExpenseTrackerDb |
| Container App | expense-tracker-api |
| Container Environment | expense-tracker-env |

---

## Comandos Útiles

### Desarrollo Local
```powershell
# Backend
cd C:\Development\ExpenseTracker
dotnet run --project src/ExpenseTracker.API

# Frontend (API local)
cd C:\Development\angular\test1\expense-tracker-app
ng serve

# Frontend (API Azure)
ng serve --configuration=production

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
az containerapp logs show --name expense-tracker-api --resource-group expense-tracker-rg --tail 50

# Reiniciar container
az containerapp revision restart --name expense-tracker-api --resource-group expense-tracker-rg

# Actualizar imagen manualmente
az containerapp update --name expense-tracker-api --resource-group expense-tracker-rg --image ghcr.io/hdepaul/expense-tracker-api:latest
```

### Migraciones EF Core
```powershell
# Crear migración
dotnet ef migrations add NombreMigracion --project src/ExpenseTracker.Infrastructure --startup-project src/ExpenseTracker.API --output-dir Data/Migrations

# Aplicar a Azure (editar scripts/apply-migrations-azure.ps1 con password)
.\scripts\apply-migrations-azure.ps1
```

---

## Estructura de Carpetas

```
C:\Development\
├── ExpenseTracker\                    # Backend .NET
│   ├── src\
│   │   ├── ExpenseTracker.API\
│   │   ├── ExpenseTracker.Application\
│   │   ├── ExpenseTracker.Domain\
│   │   └── ExpenseTracker.Infrastructure\
│   │       └── Data\Migrations\       # EF Migrations
│   ├── scripts\
│   │   ├── azure-container-apps-setup.ps1
│   │   ├── apply-migrations-azure.ps1
│   │   └── update-password.ps1        # (gitignore)
│   └── .github\workflows\
│       ├── ci.yml
│       └── cd.yml
│
└── angular\test1\expense-tracker-app\ # Frontend Angular
    └── src\
        ├── app\
        │   ├── components\
        │   ├── pages\
        │   ├── services\
        │   ├── interceptors\
        │   └── models\
        ├── assets\i18n\               # en.json, es.json
        └── environments\              # environment.ts, environment.prod.ts
```

---

## Configuración

### Backend (Variables de entorno en Azure)
- `ConnectionStrings__DefaultConnection` → Azure SQL connection string
- `Jwt__Key` → Clave secreta JWT
- `Jwt__Issuer` → ExpenseTrackerAPI
- `Jwt__Audience` → ExpenseTrackerClient
- `Jwt__ExpirationMinutes` → 480

### Frontend (environments)
- `environment.ts` → `apiUrl: 'http://localhost:5189/api'`
- `environment.prod.ts` → `apiUrl: 'https://expense-tracker-api.kindmoss-4320f913.eastus.azurecontainerapps.io/api'`

### GitHub Secrets
- `AZURE_CREDENTIALS` → JSON del service principal

---

## Pendiente / Ideas Futuras

- [ ] Deploy del frontend Angular a Azure Static Web Apps
- [ ] Dominio personalizado
- [ ] HTTPS con certificado propio
- [ ] Refresh tokens (JWT)
- [ ] Tests unitarios frontend
- [ ] PWA (Progressive Web App)
- [ ] Notificaciones push
- [ ] Export a Excel/PDF

---

## Troubleshooting

### Error 401 en API
- El token JWT expiró (8 horas)
- Solución: Hacer login de nuevo

### Error CORS
- Verificar que CORS permita el origen
- En producción usa "AllowAll"
- En desarrollo usa "AllowAngular" (localhost:4200)

### Migraciones no se aplican
- Verificar que los archivos de migración estén en el repo
- Las migraciones estaban en .gitignore (ya corregido)

### Container App no arranca
- Ver logs: `az containerapp logs show --name expense-tracker-api --resource-group expense-tracker-rg`
- Verificar connection string
- Verificar firewall de Azure SQL
