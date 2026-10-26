# PetFinder

Aplicación ASP.NET Core MVC con .NET 8 para reportar mascotas perdidas o encontradas, sugerir coincidencias fotográficas y coordinar citas de adopción con refugios aliados. El alcance actual es progresivo: esta entrega implementa las capas de dominio y persistencia en memoria; todavía no hay autenticación, interfaz web ni conexión real a Oracle.

## Estructura de la solución

El árbol objetivo al completar las cinco fases será:

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
│   └── PetFinder.Web/               # Fases 3 y 4
│       ├── Controllers/
│       ├── ViewModels/
│       ├── Views/
│       └── wwwroot/
│           ├── css/
│           ├── js/three/
│           └── images/
└── tests/
    └── PetFinder.Tests/              # Fase 5
```

En esta **Fase 1** ya existen la solución y los proyectos `ENTITY`, `DATOS` y `DAL`; `BLL`, `PetFinder.Web` y `PetFinder.Tests` se agregarán en sus fases indicadas.

## Referencias entre proyectos

Reglas objetivo: `PetFinder.Web → BLL → DAL → DATOS`; `ENTITY` es compartido, pero no depende de ninguna otra capa. En concreto:

| Proyecto | Referencias |
|---|---|
| `ENTITY` | Ninguna de la solución |
| `DATOS` | `ENTITY` |
| `DAL` | `ENTITY`, `DATOS` |
| `BLL` (Fase 2) | `DAL`, `ENTITY` |
| `PetFinder.Web` (Fase 3) | `BLL`, `ENTITY` únicamente |
| `PetFinder.Tests` (Fase 5) | `BLL`, `ENTITY` |

La GUI no accederá directamente a `DAL` ni a `DATOS`. `AddDal()` registra los cinco repositorios en memoria y un almacén singleton compartido.

## Crear o abrir la solución en Visual Studio

Para abrir el estado actual, abre `PetFinder.sln` con Visual Studio 2022 y selecciona el SDK de .NET 8. La solución de esta fase contiene únicamente bibliotecas y compila; aún no tiene un proyecto web de inicio.

Para recrearla manualmente desde Visual Studio:

1. Crea una solución en blanco llamada `PetFinder` en la carpeta raíz del repositorio.
2. Crea dentro de `src` tres proyectos **Class Library** con framework `.NET 8.0` y estos nombres exactos: `ENTITY`, `DATOS` y `DAL`.
3. En **Dependencies → Add Project Reference**, configura `DATOS → ENTITY` y `DAL → ENTITY, DATOS`. `ENTITY` no debe referenciar otros proyectos.
4. En NuGet instala en `DATOS` `Microsoft.EntityFrameworkCore` y `Microsoft.EntityFrameworkCore.Relational` versión `8.0.10`; en `DAL`, `Microsoft.Extensions.DependencyInjection.Abstractions` versión `8.0.2`.
5. Organiza los archivos según el árbol anterior y usa **Build → Build Solution**.

Las versiones de paquetes aquí fijadas compilan con .NET 8. No se agrega todavía `Oracle.EntityFrameworkCore`; el proveedor Oracle se incorpora cuando se sustituya el almacenamiento en memoria.

## Qué incluye la Fase 1

- Entidades `Usuario`, `Refugio`, `Mascota`, `ReporteMascota` y `Cita`; enums de roles, especies, estados de cita y tipo de reporte; límites centralizados de campos.
- `PetFinderDbContext`, cinco configuraciones Fluent API y datos seed deterministas compartidos por el modelo EF Core y el almacenamiento en memoria.
- Interfaces y repositorios `IUsuarioRepository`, `IRefugioRepository`, `IMascotaRepository`, `IReporteRepository` e `ICitaRepository`.
- Búsqueda inicial de candidatos de reporte filtrando por especie, ciudad y fecha reciente antes del cálculo de similitud, que se implementará en BLL.
- Consulta de intervalos de citas superpuestos para que BLL aplique las reglas de reserva en Fase 2.

Los correos `@petfinder.local` del seed son datos demostrativos. El campo `PasswordHash` queda vacío: no son credenciales y no permiten iniciar sesión.

## Compilar

```powershell
dotnet build PetFinder.sln --configuration Release
```

Con Visual Studio también puedes compilar con **Build → Build Solution**. La Fase 1 no incluye proyecto de pruebas; las pruebas xUnit/Moq están previstas para la Fase 5.

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

Cada fase se desarrolla en su propia rama. Para Fase 1:

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

El `.gitignore` excluye `appsettings.Development.json`, otros archivos locales de configuración y archivos de secretos; `appsettings.example.json` permanece versionable para una fase posterior. No se incluyen contraseñas, claves ni cadenas de conexión.

## Estado de fases

| Fase | Alcance | Estado |
|---|---|---|
| 1 | ENTITY, DATOS y DAL; repositorios en memoria | Implementada; compilación verificada |
| 2 | BLL, reglas, servicios, DTOs y similitud simulada | Pendiente de confirmación |
| 3 | ASP.NET Core MVC, vistas y autenticación simulada | Pendiente |
| 4 | Escenas Three.js y animaciones accesibles | Pendiente |
| 5 | Pruebas unitarias xUnit + Moq sobre BLL | Pendiente |

Al conectar Oracle en una fase posterior se configurará `PetFinderDbContext` con el proveedor `Oracle.EntityFrameworkCore` desde el registro de servicios de `AddDal()`, sustituyendo los repositorios en memoria sin cambiar BLL ni la GUI. Las credenciales de Oracle deberán ir en User Secrets o en configuración local ignorada por Git; se añadirá un `appsettings.example.json` sin secretos cuando exista el proyecto web.
