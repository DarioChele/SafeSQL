# 🔹 Proyecto - 🔐 SafeSQL 🔹  
⚡ **Sistema de conexión a SQL con transacciones y consultas optimizadas.**  

---

## 📌 **Descripción**  
Este proyecto proporciona una **estructura robusta** para conectarse a una base de datos SQL en aplicaciones .NET.  
Incluye el manejo de **transacciones, consultas asíncronas y seguridad contra SQL Injection**, garantizando una ejecución **eficiente y confiable**.  

---

## 🚀 **Características Principales**  
✅ **Conexión centralizada** utilizando `SqlConnection` inyectado en la aplicación.  
✅ **Soporte para transacciones** (`Commit` y `Rollback`).  
✅ **Consultas seguras** con `ParametrosSql` para prevenir **SQL Injection**.  
✅ **Ejecución de consultas asíncronas** (`async/await`).  
✅ **Inserción masiva eficiente** con `SqlBulkCopy`.  
✅ **Método flexible** `EjecutarConsultaListaAsync<T>` para devolver listas de objetos directamente.  

---

## 📂 **Estructura del Proyecto**  
📦 **SafeSQL**  
 ┣ 📂 **Documentation**  
 ┃ ┣ 📜 `98 - Documentación de Uso.txt`  
 ┃ ┣ 📜 `99 - EjemploUsoApi.txt`  
 ┣ 📂 **Source**  
 ┃ ┣ 📜 `ConexionSql.cs` - Clase principal para el acceso a datos  
 ┃ ┣ 📜 `ParametrosSql.cs` - Clase auxiliar para parámetros en consultas SQL  
 ┣ 📜 `README.md` - English version of project documentation  
 ┣ 📜 `README_es.md` - Documentación del proyecto en Español

---

## ⚡ **Configuración Inicial**  
### 🔹 **1. Inyectar dependencias en `Program.cs`**  
```csharp
      builder.Services.AddScoped<ConexionSql>();         // Gestiona la conexión con la base de datos  
      builder.Services.AddScoped<ArticuloLogicaNego>(); // Maneja la lógica de negocio
```
📌 **Esto permite que `ConexionSql` y `ArticuloLogicaNego` sean utilizadas en cualquier parte de la aplicación.**  

### 🔹 **2. Configurar la conexión en `appsettings.json`**  
```json
      {
        "ConnectionStrings": {
          "DefaultConnection": "Server=myServer;Database=myDB;User Id=myUser;Password=myPassword;"
        }
      }  
```  
📌 **Recuerda modificar la cadena de conexión según tu entorno.**  

---

## 📌 **Uso de Métodos**  
### 🔹 **Ejemplo de Consulta con `EjecutarConsultaListaAsync<T>`**  
```csharp
      var articulos = await _conexion.EjecutarConsultaListaAsync(  
          "SELECT * FROM Articulos", CommandType.Text, null,  
          row => new Articulo  
          {  
              Id = row.Field<int>("Id"),  
              Nombre = row.Field<string>("Nombre"),  
              Precio = row.Field<decimal>("Precio")  
          });
```  
✅ **Recupera una lista de objetos `Articulo` directamente desde la base de datos.**  

### 🔹 **Ejemplo de `INSERT` con `EjecutarSentenciaAsync`**  
```csharp  
      var parametros = new ParametrosSql();  
      parametros.AgregarParametro("@nombre", "Nuevo Producto", SqlDbType.NVarChar);  
      parametros.AgregarParametro("@precio", 99.99m, SqlDbType.Decimal);  
      
      int filasAfectadas = await _conexion.EjecutarSentenciaAsync(  
          "INSERT INTO Productos (Nombre, Precio) VALUES (@nombre, @precio)",  
          CommandType.Text, parametros);
```  
📌 **Se usa `ParametrosSql` para prevenir SQL Injection.**  

---

## 🔄 **Manejo de Transacciones**  
### 📌 **Garantizando que los cambios se confirmen solo si todo es exitoso**  
```csharp  
      conexion.IniciarTransaccion();  
      try {  
          // 🛠️ Actualización en la base de datos...  
          conexion.CommitTransaccion(); // ✅ Confirma los cambios si todo sale bien  
      } catch {  
          conexion.RollbackTransaccion(); // 🔄 Revierte cambios si hay error  
          throw;  
      }
```  
✅ **Si algo falla, `Rollback` evita que los cambios afecten la base de datos.**  

---

## 📌 **Inserción Masiva (`Bulk Insert`)**  
```csharp  
      var dtProductos = new DataTable();
      dtProductos.Columns.Add("Nombre", typeof(string));
      dtProductos.Columns.Add("Precio", typeof(decimal));
      
      dtProductos.Rows.Add("Producto 1", 10.99m);
      dtProductos.Rows.Add("Producto 2", 20.50m);
      
      conexion.IniciarTransaccion();
      try {
          conexion.EjecutarMasivoDT("dbo.Productos", dtProductos, timeout: 120, opciones: SqlBulkCopyOptions.TableLock);
          conexion.CommitTransaccion();
      } catch {
          conexion.RollbackTransaccion();
          throw;  
      }
```
📌 **Ideal para procesar grandes volúmenes de datos sin afectar el rendimiento.**  

---

## 🛠 **Buenas Prácticas**  
✔️ **Usar `using` para liberar recursos automáticamente.**  
✔️ **Siempre manejar transacciones cuando las operaciones sean críticas.**  
✔️ **Utilizar `ParametrosSql` para evitar SQL Injection.**  
✔️ **Optimizar el uso de `async/await` para mejorar la concurrencia.**  
✔️ **Configurar `timeout` adecuado para operaciones largas.**  

---

## 📢 **Contribuciones**  
🎯 ¡Este proyecto está abierto a mejoras! Si deseas contribuir:  
1️⃣ **Haz un fork del repositorio.**  
2️⃣ **Crea una nueva rama (`feature-mi-mejora`).**  
3️⃣ **Realiza tus cambios y envía un pull request.**  

---

## 📜 **Licencia**  
Este proyecto está bajo la **Licencia MIT**.  
📌 **Eres libre de usarlo, modificarlo y mejorarlo sin restricciones.** 🔥  

---