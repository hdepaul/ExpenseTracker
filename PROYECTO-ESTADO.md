# ExpenseTracker - Estado del Proyecto

## Resumen
Aplicación de control de gastos con:
- **Backend**: .NET 10 (Clean Architecture + CQRS con MediatR)
- **Frontend**: Angular 21 (Standalone Components + Signals)
- **Base de datos**: SQL Server (local) / Azure SQL (producción)
- **Deploy**: Azure Container Apps + GitHub Actions

---

## URLs de Producción

| Recurso | URL |
|---------|-----|
| **Frontend (Dominio)** | https://enquegasto.com |
| **Frontend (www)** | https://www.enquegasto.com |
| **Frontend (Azure)** | https://delightful-sky-08733d10f.6.azurestaticapps.net |
| **Backend (API)** | https://expense-tracker-api.kindmoss-4320f913.eastus.azurecontainerapps.io |
| **GitHub Backend** | github.com/hdepaul/ExpenseTracker |
| **GitHub Frontend** | github.com/hdepaul/expense-tracker-app |

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
- [x] **AI Agent** - Gastos por lenguaje natural con Claude Haiku 3.5 (Agentic Tool Use)
- [x] Rate limiting de mensajes AI por usuario/día (configurable, default 30)
- [x] Multi-turno: Claude pregunta si falta info (categoría, monto, etc.)
- [x] **AI Resumen** - Tool `query_expenses` para consultar gastos por fecha/categoría, Claude genera resumen en lenguaje natural

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
- [x] **Mini-chat AI** en lista de gastos (burbujas, loading animado, auto-refresh)
- [x] **Input por voz** - Web Speech API (micrófono), auto-envía transcripción
- [x] **Text-to-Speech** - Botón parlante en respuestas de Claude (opcional)
- [x] **Chat UX mejorado** - Mensaje de bienvenida + chips de ejemplo tocables
- [x] **Landing page** - Hero, features, how-it-works, CTA para usuarios no logueados
- [x] **Toggle password** - Ojito show/hide en login y register
- [x] **Navegación por mes** - Flechitas en vez de dropdown, click en mes para "Todo el tiempo"
- [x] **Íconos de acción** - Edit/Delete con emojis compactos en vez de botones de texto
- [x] **PWA icons** - Todos los tamaños (72-512px) generados desde favicon SVG
- [x] Logout redirige a landing page

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
- [x] Dominio custom enquegasto.com + www (Cloudflare DNS → Azure Static Web Apps)
- [x] SSL/TLS automático (Azure managed certificate)

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
| Dominio | enquegasto.com (Cloudflare DNS) |

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
│   │   │   └── Controllers\           # Expenses, Auth, AI
│   │   ├── ExpenseTracker.Application\
│   │   │   ├── Features\Expenses\     # CRUD gastos
│   │   │   ├── Features\AIAgent\      # Chat con Claude (Agentic Tool Use)
│   │   │   └── Common\Interfaces\     # IClaudeAgentService, etc.
│   │   ├── ExpenseTracker.Domain\
│   │   │   └── Entities\              # Expense, Category, User, AIUsageLog
│   │   └── ExpenseTracker.Infrastructure\
│   │       ├── Services\              # ClaudeAgentService, JwtService, etc.
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
- `Claude__ApiKey` → API key de Anthropic
- `Claude__Model` → claude-haiku-4-5-20251001
- `Claude__MaxTokens` → 1024
- `Claude__DailyMessageLimit` → 30

### Frontend (environments)
- `environment.ts` → `apiUrl: 'http://localhost:5189/api'`
- `environment.prod.ts` → `apiUrl: 'https://expense-tracker-api.kindmoss-4320f913.eastus.azurecontainerapps.io/api'`

### GitHub Secrets
- `AZURE_CREDENTIALS` → JSON del service principal

---

## Pendiente / Ideas Futuras

- [x] Deploy del frontend Angular a Azure Static Web Apps ✅
- [x] PWA básico (manifest, meta tags) ✅
- [x] Favicon personalizado ✅
- [x] SPA routing en Azure ✅
- [x] Dominio personalizado (enquegasto.com + www) via Cloudflare DNS ✅
- [x] **AI Agent** - Agregar gastos por lenguaje natural ("Agregá $50 de nafta de hoy") ✅
### Próximas features (en orden de prioridad)
- [ ] **WhatsApp Bot** - Registrar gastos por WhatsApp con Twilio (mismo endpoint `/api/ai/chat`)
- [ ] **Presupuestos por categoría** - Límite mensual + barra de progreso + alerta al 80%
- [ ] **Foto del ticket** - OCR con Claude Vision para extraer monto/comercio
- [ ] **Gastos recurrentes** - Auto-registro mensual, Claude detecta patrones

### Backlog técnico
- [ ] Refresh tokens (JWT)
- [ ] Tests unitarios frontend
- [ ] Service Worker (offline cache)
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
