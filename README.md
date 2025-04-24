# 🔹 Proyecto - 🔐 SafeSQL 🔹  
⚡ **A robust SQL connection system with optimized transactions and queries.**  

---

## 📌 **Description**  
SafeSQL provides a **structured and secure** way to interact with SQL databases in .NET applications.  
It includes **transaction handling, asynchronous queries, and protection against SQL injection**, ensuring **efficient and reliable execution**.  

---

## 🚀 **Key Features**  
✅ **Centralized SQL connection** management via dependency injection.  
✅ **Transaction support** (`Commit` and `Rollback`).  
✅ **Secure queries** using `ParametrosSql` to prevent **SQL Injection**.  
✅ **Asynchronous query execution** (`async/await`).  
✅ **Efficient bulk insertion** with `SqlBulkCopy`.  
✅ **Flexible method** `EjecutarConsultaListaAsync<T>` for direct object list retrieval.  

---

## 📂 **Project Structure**  
📦 **SafeSQL**  
 ┣ 📂 **Documentation**  
 ┃ ┣ 📜 `98 - Usage Documentation.txt`  
 ┃ ┣ 📜 `99 - API Usage Example.txt`  
 ┣ 📂 **Source**  
 ┃ ┣ 📜 `ConexionSql.cs` - Main class for database access  
 ┃ ┣ 📜 `ParametrosSql.cs` - Auxiliary class for SQL parameters  
 ┣ 📜 `README.md` - English version of project documentation  
 ┣ 📜 `README_es.md` - Documentación del proyecto en Español

---

## ⚡ **Initial Setup**  
### 🔹 **1. Inject dependencies in `Program.cs`**  
```csharp
      builder.Services.AddScoped<ConexionSql>();         // Manages the database connection  
      builder.Services.AddScoped<ArticuloLogicaNego>(); // Handles business logic  
```  
📌 **This ensures `ConexionSql` and `ArticuloLogicaNego` are available throughout the application.**  

### 🔹 **2. Configure the connection in `appsettings.json`**  
```json  
      {
        "ConnectionStrings": {
          "DefaultConnection": "Server=myServer;Database=myDB;User Id=myUser;Password=myPassword;"
        }
      }  
```  
📌 **Modify the connection string according to your environment.**  

---

## 📌 **Method Usage**  
### 🔹 **Example Query with `EjecutarConsultaListaAsync<T>`**  
```csharp  
      var articles = await _conexion.EjecutarConsultaListaAsync(  
          "SELECT * FROM Articulos", CommandType.Text, null,  
          row => new Articulo  
          {  
              Id = row.Field<int>("Id"),  
              Nombre = row.Field<string>("Nombre"),  
              Precio = row.Field<decimal>("Precio")  
          });  
```  
✅ **Retrieves a list of `Articulo` objects directly from the database.**  

### 🔹 **Example `INSERT` with `EjecutarSentenciaAsync`**  
```csharp  
      var parameters = new ParametrosSql();  
      parameters.AgregarParametro("@nombre", "New Product", SqlDbType.NVarChar);  
      parameters.AgregarParametro("@precio", 99.99m, SqlDbType.Decimal);  

      int affectedRows = await _conexion.EjecutarSentenciaAsync(  
          "INSERT INTO Productos (Nombre, Precio) VALUES (@nombre, @precio)",  
          CommandType.Text, parameters);  
```  
📌 **`ParametrosSql` is used to prevent SQL Injection.**  

---

## 🔄 **Transaction Handling**  
### 📌 **Ensuring changes are committed only if everything succeeds**  
```csharp  
      conexion.IniciarTransaccion();  
      try  {  
          // 🛠️ Database updates...  
          conexion.CommitTransaccion(); // ✅ Commits changes if successful  
      } catch {  
          conexion.RollbackTransaccion(); // 🔄 Reverts changes if an error occurs  
          throw;  
      }  
```  
✅ **Rollback ensures database consistency in case of failure.**  

---

## 📌 **Bulk Insert (`SqlBulkCopy`)**  
```csharp  
      var dtProducts = new DataTable();  
      dtProducts.Columns.Add("Nombre", typeof(string));  
      dtProducts.Columns.Add("Precio", typeof(decimal));  

      dtProducts.Rows.Add("Product 1", 10.99m);  
      dtProducts.Rows.Add("Product 2", 20.50m);  

      conexion.IniciarTransaccion();  
      try {  
          conexion.EjecutarMasivoDT("dbo.Productos", dtProducts, timeout: 120, opciones: SqlBulkCopyOptions.TableLock);  
          conexion.CommitTransaccion();  
      } catch {  
          conexion.RollbackTransaccion();  
          throw;  
      }  
```  
📌 **Ideal for efficiently handling large volumes of data.**  

---

## 🛠 **Best Practices**  
✔️ **Use `using` to automatically release resources.**  
✔️ **Always use transactions for critical operations.**  
✔️ **Utilize `ParametrosSql` to prevent SQL Injection.**  
✔️ **Optimize asynchronous execution (`async/await`) for improved concurrency.**  
✔️ **Set appropriate timeouts for long-running operations.**  

---

## 📢 **Contributing**  
🎯 **We welcome contributions! If you'd like to contribute:**  
1️⃣ **Fork the repository.**  
2️⃣ **Create a new branch (`feature-my-enhancement`).**  
3️⃣ **Submit a pull request with your changes.**  

---

## 📜 **License**  
This project is licensed under **MIT License**.  
📌 **You are free to use, modify, and enhance it without restrictions.** 🔥  

---
