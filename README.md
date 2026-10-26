# PetFinder

Aplicación ASP.NET Core MVC con .NET 8 para reportar mascotas perdidas o encontradas, sugerir coincidencias fotográficas y coordinar citas de adopción con refugios aliados. El almacenamiento actual es en memoria y las coincidencias son simuladas; Oracle y el análisis real de imágenes aún no están conectados.

## Estructura de la solución

El árbol actual refleja seis fases de implementación:

```text
PetFinder/
├── .gitignore
├── PetFinder.sln
├── README.md
├── src/
│   ├── ENTITY/
│   │   ├── Constants/
│   │   ├── Entities/
│   │   └── Enums/
│   ├── DATOS/
│   │   ├── Configurations/
│   │   ├── Context/
│   │   └── Seed/
│   ├── DAL/
│   │   ├── Interfaces/
│   │   └── Repositories/
│   ├── BLL/                         # Fase 2
│   │   ├── Dtos/
│   │   ├── Exceptions/
│   │   ├── Services/
│   │   └── Validators/
│   └── PetFinder.Web/               # Fases 3, 4 y 6
│       ├── Controllers/
│       ├── ViewModels/
│       ├── Views/
│       └── wwwroot/
│           ├── css/
│           ├── js/scroll/            # Fase 6
│           ├── js/three/
│           └── images/
└── tests/
    └── PetFinder.Tests/              # Fases 5 y 6
```

La solución incluye `ENTITY`, `DATOS`, `DAL`, `BLL`, `PetFinder.Web` y `PetFinder.Tests`; el catálogo, los reportes y los paneles usan repositorios en memoria en esta demo.

## Referencias entre proyectos

Reglas objetivo: `PetFinder.Web → BLL → DAL → DATOS`; `ENTITY` es compartido, pero no depende de ninguna otra capa. En concreto:

| Proyecto | Referencias |
|---|---|
| `ENTITY` | Ninguna de la solución |
| `DATOS` | `ENTITY` |
| `DAL` | `ENTITY`, `DATOS` |
| `BLL` | `DAL`, `ENTITY` |
| `PetFinder.Web` | `BLL`, `ENTITY` únicamente |
| `PetFinder.Tests` (Fase 5) | `BLL`, `ENTITY` |

La GUI no accederá directamente a `DAL` ni a `DATOS`. `AddBll()` llama internamente a `AddDal()`, que registra los cinco repositorios en memoria y un almacén singleton compartido.

## Crear o abrir la solución en Visual Studio

Para abrir el estado actual, abre `PetFinder.sln` con Visual Studio 2022 y selecciona el SDK de .NET 8. `src/PetFinder.Web` es la aplicación de inicio y conecta a BLL con `AddBll()`.

Para recrearla manualmente desde Visual Studio:

1. Crea una solución en blanco llamada `PetFinder` en la carpeta raíz del repositorio.
2. Crea dentro de `src` cuatro proyectos **Class Library** con framework `.NET 8.0` y estos nombres exactos: `ENTITY`, `DATOS`, `DAL` y `BLL`; crea además `PetFinder.Web` como proyecto **ASP.NET Core Web App (Model-View-Controller)** .NET 8.
3. En **Dependencies → Add Project Reference**, configura `DATOS → ENTITY`, `DAL → ENTITY, DATOS`, `BLL → DAL, ENTITY` y `PetFinder.Web → BLL, ENTITY`. `ENTITY` no debe referenciar otros proyectos.
4. En NuGet instala en `DATOS` `Microsoft.EntityFrameworkCore` y `Microsoft.EntityFrameworkCore.Relational` versión `8.0.10`; en `DAL`, `Microsoft.Extensions.DependencyInjection.Abstractions` versión `8.0.2`; y en `BLL`, `FluentValidation` `11.9.2`, `Microsoft.Extensions.DependencyInjection.Abstractions` `8.0.2` y `Microsoft.Extensions.Logging.Abstractions` `8.0.2`.
5. Organiza los archivos según el árbol anterior, configura `Program.cs` con `builder.Services.AddBll()` y autenticación por cookies y usa **Build → Build Solution**.

Las versiones de paquetes aquí fijadas compilan con .NET 8. No se agrega todavía `Oracle.EntityFrameworkCore`; el proveedor Oracle se incorpora cuando se sustituya el almacenamiento en memoria.

## Qué incluye la Fase 1

- Entidades `Usuario`, `Refugio`, `Mascota`, `ReporteMascota` y `Cita`; enums de roles, especies, estados de cita y tipo de reporte; límites centralizados de campos.
- `PetFinderDbContext`, cinco configuraciones Fluent API y datos seed deterministas compartidos por el modelo EF Core y el almacenamiento en memoria.
- Interfaces y repositorios `IUsuarioRepository`, `IRefugioRepository`, `IMascotaRepository`, `IReporteRepository` e `ICitaRepository`.
- Búsqueda inicial de candidatos de reporte filtrando por especie, ciudad y fecha reciente antes del cálculo de similitud, que se implementará en BLL.
- Consulta de intervalos de citas superpuestos para que BLL aplique las reglas de reserva en Fase 2.

Los correos `@petfinder.local` del seed son datos demostrativos. El campo `PasswordHash` queda vacío: no son credenciales y no permiten iniciar sesión.

## Qué incluye la Fase 2

- Servicios `AuthService`, `ReporteService`, `CoincidenciaService`, `AdopcionService` y `CitaService`, DTOs y validadores FluentValidation además de DataAnnotations.
- Registro de adoptantes e inicio de sesión mediante hash PBKDF2-SHA256 con sal aleatoria; el DTO de sesión nunca expone el hash.
- Coincidencias: primero el repositorio filtra por especie, ciudad y fecha reciente; después el mock genera porcentajes deterministas entre 55 y 95, que se ordenan de mayor a menor. El mock **no analiza imágenes reales**.
- Citas: verifica usuario, refugio y mascota activa; rechaza horarios pasados y cruces con citas pendientes o confirmadas; permite confirmar solo citas pendientes y cancelar citas pendientes o confirmadas con autorización del adoptante, refugio o administrador.
- `AddBll()` registra servicios, validadores, `TimeProvider` y la implementación simulada de `IImageSimilarityService`, y llama internamente a `AddDal()`.
- Para el panel web, `GestionRefugioService` limita la edición de mascotas al refugio propietario y `AdministracionService` controla cambios de estado de usuarios y refugios.

## Qué incluye la Fase 3

- Landing responsive en español, catálogo con filtros por refugio/especie/edad y ficha de mascota con solicitud de cita presencial.
- Registro de adoptantes, login PBKDF2 a través de BLL y cookies HttpOnly con roles; los accesos de demostración para Adoptante/Refugio/Administrador solo se habilitan en `Development`.
- Formularios Razor protegidos con antiforgery, reporte con carga de imagen JPG/PNG/WebP hasta 5 MB y vista previa local; archivos subidos se excluyen de Git.
- Resultados de coincidencia con advertencia de similitud simulada, página Mis citas, panel de refugio para crear/editar publicaciones y panel de administración para cuentas/refugios.
- Middleware global que traduce validación y reglas de negocio a respuestas HTTP seguras. El Web referencia solo `BLL` y `ENTITY`.

## Qué incluye la Fase 4

- Escena decorativa 3D de Three.js con avatares de perro y gato y huellas flotantes en la landing.
- El módulo usa una versión fija de Three.js por import map, solo se carga en la landing, limita la densidad de píxeles y suspende la animación cuando sale de pantalla o el usuario prefiere movimiento reducido.
- Si WebGL o el CDN no están disponibles, permanece la ilustración CSS/emoji estática como fallback; la escena no contiene controles ni contenido que requiera interacción.

## Qué incluye la Fase 5

- Proyecto `tests/PetFinder.Tests` con xUnit, Moq y referencias limitadas a BLL y ENTITY.
- 33 pruebas cubren autenticación/PBKDF2, validaciones, filtros y orden de coincidencias, disponibilidad del catálogo, reportes, reservas y cruces de horario, transiciones de citas, autorización de refugio/administración, coordenadas y estadísticas.
- Los casos usan repositorios simulados y un `TimeProvider` congelado para mantener resultados deterministas.

## Qué incluye la Fase 6

- La landing se compone de ocho parciales: impacto, pasos, mapa, mascotas destacadas, tablero comunitario, historias ficticias, preguntas frecuentes y llamado final; conserva la escena Three.js del hero.
- `IRefugioService` prepara DTOs públicos solo para refugios activos de Valledupar con latitud/longitud válidas, cuenta mascotas disponibles por especie y ordena por distancia Haversine. El endpoint `GET /api/refugios` usa ese servicio; la GUI no referencia DAL ni DATOS.
- `IEstadisticasService` calcula reencuentros, refugios aliados, adopciones y reportes activos desde estados explícitos del dominio. No genera cifras sintéticas.
- El mapa muestra lista sincronizada, búsqueda por refugio/dirección/ciudad, filtro por especie, popups seguros y enlace al catálogo. La geolocalización solo se solicita al pulsar “Usar mi ubicación”; sin permiso sigue disponible el directorio.
- Leaflet 1.9.4 carga mosaicos del servidor estándar de OpenStreetMap con atribución visible y filtro cálido solo en el mapa. Se requieren internet para el mapa y sus fuentes externas; el servidor de tiles es best-effort, sin SLA. La app no precarga ni descarga mosaicos fuera del viewport.
- Contadores, reveal, conector SVG, parallax, navbar inteligente y botón de regreso respetan `prefers-reduced-motion`. El contenido y los enlaces principales permanecen legibles si JavaScript, Leaflet o los mosaicos fallan.
- El centro, los límites de navegación del mapa, los seis refugios, las mascotas y los reportes seed corresponden a Valledupar, Cesar, Colombia; ubicaciones, direcciones, contactos y testimonios son ficticios. `MapaOptions` en `appsettings.json` configura el centro y zoom iniciales.

## Compilar

```powershell
dotnet build PetFinder.sln --configuration Release
```

Para ejecutar la suite de pruebas:

```powershell
dotnet test PetFinder.sln --configuration Release
```

Para iniciar la aplicación local en Development desde PowerShell:

```powershell
dotnet run --project src/PetFinder.Web/PetFinder.Web.csproj
```

Al iniciar en Development, usa “Acceso de demostración local” en el login para explorar los tres roles con los usuarios seed. Ese selector no está disponible fuera de Development y las cuentas seed no tienen contraseñas.

Con Visual Studio puedes compilar con **Build → Build Solution** o ejecutar los tests desde **Test → Run All Tests**. Para abrir el mapa necesitas conexión a internet; si Leaflet o OpenStreetMap no están disponibles, la landing conserva el directorio de refugios.

## Inicialización y flujo de GitHub

El repositorio remoto configurado es `https://github.com/hx4m099/PET-FINDER.git`. Para un clon local nuevo, estos son los comandos de inicialización solicitados (el commit inicial ya fue creado y `main` ya fue publicado en este entorno):

```powershell
dotnet new gitignore
git init
git branch -M main
git add .gitignore
git commit -m "chore: inicializar repositorio de PetFinder"
git remote add origin https://github.com/hx4m099/PET-FINDER.git
git push -u origin main
```

Cada fase se desarrolla en su propia rama. El flujo usado para la Fase 3 es:

```powershell
git switch -c feature/fase-1-datos
dotnet build PetFinder.sln --configuration Release
git add .
git commit -m "feat: capa ENTITY, DATOS y DAL con repositorios en memoria"
git push -u origin feature/fase-1-datos
git switch main
git pull --ff-only origin main
git merge --no-ff feature/fase-1-datos
git push origin main
```

```powershell
git switch -c feature/fase-2-bll
dotnet build PetFinder.sln --configuration Release
git add .
git commit -m "feat: servicios BLL y reglas de negocio"
git push -u origin feature/fase-2-bll
git switch main
git pull --ff-only origin main
git merge --no-ff feature/fase-2-bll
git push origin main
```

```powershell
git switch -c feature/fase-3-web
dotnet build PetFinder.sln --configuration Release
git add .
git commit -m "feat: aplicación web MVC PetFinder"
git push -u origin feature/fase-3-web
git switch main
git pull --ff-only origin main
git merge --no-ff feature/fase-3-web
git push origin main
```

El `.gitignore` excluye `appsettings.Development.json`, uploads locales y archivos de secretos; `src/PetFinder.Web/appsettings.example.json` permanece versionable sin credenciales ni cadenas de conexión.

## Estado de fases

| Fase | Alcance | Estado |
|---|---|---|
| 1 | ENTITY, DATOS y DAL; repositorios en memoria | Implementada y publicada |
| 2 | BLL, reglas, servicios, DTOs y similitud simulada | Implementada y verificada |
| 3 | ASP.NET Core MVC, vistas y autenticación simulada | Implementada; build y smoke HTTP verificados |
| 4 | Escenas Three.js y animaciones accesibles | Implementada; build y asset HTTP verificados |
| 5 | Pruebas unitarias xUnit + Moq sobre BLL | Implementada; 33/33 pruebas aprobadas |
| 6 | Landing por scroll, mapa sincronizado, API, accesibilidad y estilos | Implementada; build, tests y smoke HTTP verificados |

Al conectar Oracle en una fase posterior se configurará `PetFinderDbContext` con el proveedor `Oracle.EntityFrameworkCore` desde el registro de servicios de `AddDal()`, sustituyendo los repositorios en memoria sin cambiar BLL ni la GUI. Las credenciales de Oracle deberán ir en User Secrets o en configuración local ignorada por Git.
