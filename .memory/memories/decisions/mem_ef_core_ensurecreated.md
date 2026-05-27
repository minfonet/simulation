---
id: mem_ef_core_ensurecreated
type: decision
tags:
  - backend
  - database
  - ef-core
---

# Context

Necesitábamos decidir cómo crear el esquema de base de datos en el MVP. Opciones: migrations de EF Core, EnsureCreated(), o scripts SQL manuales.

# Decision

Usar `EnsureCreated()` en `Program.cs` al iniciar la aplicación. Sin migrations en el MVP.

# Why

- EnsureCreated() crea todas las tablas automáticamente desde el DbContext
- Elimina la necesidad de ejecutar `dotnet ef migrations add` manualmente
- Para un MVP con 5 tablas y sin esquemas complejos, las migrations son overengineering
- Cuando el modelo madure, se migrará a migrations formales

# Consequences

- No hay historial de cambios de esquema (no aplica hasta que haya datos en producción)
- Para agregar una columna hay que borrar la BD o agregarla manualmente
- Swap a migrations será necesario antes de cualquier despliegue real
