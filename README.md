# Jazatar and JaszCore

This repository contains two related template projects demonstrating common backend design patterns in .NET Core:

* **JaszCore**: A standalone class-library providing core services, models, and a `ServiceLocator` for dependency injection.
* **Jazatar**: A console application that consumes JaszCore, illustrating application wiring, environment configuration, and data persistence via `DbContext`.

---

## Common Features

* **Dependency Injection (DI)** via a custom `ServiceLocator` (interfaces + implementations).
* **Singleton support** for services decorated with `[Service(..., singleton: true)]`.
* **DbContext-based data access** (`JaszMain`, `JaszOuter`) using EF Core, including bulk insert/update/delete helpers.
* **Smart Save** methods that switch between single-entity saves and bulk operations based on configurable thresholds.
* **Attribute-driven configuration** (`[OrgTable]`, `[Column]`) for routing entities to the correct database.
* **Clean separation** of service layer (`DatabaseService`) and data layer (`JaszMain`).

---

## Getting Started

1. **Clone the repo** and open in Visual Studio or via the CLI.
2. **Restore dependencies**:

   ```bash
   dotnet restore
   ```
3. **Set environment variables** for your connection strings (to avoid checking secrets into source control):

   ```powershell
   # Windows PowerShell
   $Env:ConnectionStrings__MAIN_CONNECTION = 'Server=...;Database=...;User Id=...;Password=...;'
   $Env:ConnectionStrings__OUTER_CONNECTION = 'Server=...;Database=...;User Id=...;Password=...;'
   ```
4. **Build and run**:

   ```bash
   dotnet build
   cd Jazatar
   dotnet run -- DEV
   ```

---

## Project Structure

```
/jasznetcore.sln
/src
	/ JaszCore
	  / Common      # ServiceLocator, utilities, shared constants
	  / Models      # IDataModel<T>, entity definitions
	  / Services    # IDatabaseService, DatabaseService
	  / Databases   # JaszMain DbContext
	/ Jazatar
	  / Program.cs  # Entry point, configuration wiring
	  / AppClient   # Application orchestration
```

---
