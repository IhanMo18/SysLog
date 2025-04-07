# Registro de Logs de Firewall - ASP.NET Core MVC

## 📌 Descripción
Este proyecto es una aplicación web desarrollada en **ASP.NET Core MVC** que permite el registro, almacenamiento y visualización de logs generados por un firewall. Está diseñado para propósitos académicos en la universidad y sigue la arquitectura **Modelo-Vista-Controlador (MVC)**.

## 🚀 Características
- Registro automático de eventos del firewall.
- Interfaz web para visualizar, filtrar y buscar logs.
- Almacenamiento seguro en una base de datos PostgreSQL.


## 🛠 Tecnologías Utilizadas
- **Framework:** ASP.NET Core MVC  
- **Base de Datos:** PostgreSQL 
- **ORM:** Entity Framework Core  
- **Frontend:** Bootstrap + Razor Views 
- **Autenticación:** Identity Framework  

## 📂 Estructura del Proyecto
📦 SysLog
├── 📁 SysLog.Data
│   ├── Data
│   ├── Migrations
│
├── 📁 SysLog.Domine
│   ├── Interface
│   ├── Model
│
├── 📁 SysLog.Repository
│   ├── Repositories
│
├── 📁 SysLog.Service
│   ├── Service.cs
│   ├── UdpLogListener.cs
│
├── 📁 SysLog.WebLogUdp
│   ├── Controllers
│   ├── Dtos
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── Program.cs
│
├── appsettings.json
├── Program.cs
├── Startup.cs

## 📜 Instalación y Configuración
1. Clonar el repositorio:
   ```sh
   git clone https://github.com/tu-usuario/SysLog.git
   cd SysLog
   ```
2. Configurar la cadena de conexión en `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=SysLogDB;Trusted_Connection=True;"
   }
   ```
3. Aplicar migraciones y crear la base de datos:
   ```sh
   dotnet ef database update
   ```
4. Ejecutar la aplicación:
   ```sh
   dotnet run
   ```

## 📌 Uso
- Acceder a la aplicación en `http://localhost:5000`.
- Iniciar sesión para gestionar los logs.
- Consultar, filtrar y exportar logs.

## 📄 Licencia
Este proyecto es de uso académico y no tiene licencia comercial.

---
🔹 Desarrollado por: Ihan Montalvan 
📧 Contacto: ihanmontalvan@gmail.com
