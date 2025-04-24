using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace AccesoDatos;
public class ConexionSql : IDisposable {
#region Propiedades
    private readonly SqlConnection _conexion;
    private SqlTransaction _transaccion;
    private readonly IConfiguration _configuration;
    private const int DefaultTimeout = 60;
#endregion Metodos Asíncronos
    public ConexionSql(IConfiguration configuration) {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("La cadena de conexión no está configurada correctamente.");

        _conexion = new SqlConnection(connectionString);
    }
    
    // Ejemplo de cómo usaríamos _configuration si necesitáramos otros valores:
    /* public int GetDefaultTimeoutFromConfig() {
        if (_configuration.GetSection("DatabaseSettings:DefaultTimeout").Exists()) {
            return _configuration.GetValue<int>("DatabaseSettings:DefaultTimeout");
        }
        return DefaultTimeout;
    } */
#region Metodos Síncronos
    /// <summary>
    /// Permite abrir la conexion en la base de datos
    /// </summary>
    public void AbrirConexion() {
        try {
            if (_conexion.State != ConnectionState.Open)
                _conexion.Open();
        } catch (SqlException ex) {
            throw new Exception($"Error al abrir la conexión: {ex.Message}");
        } catch (InvalidOperationException ex) {
            throw new Exception($"Error de operación inválida: {ex.Message}");
        } catch (System.Exception ex) {
            throw new Exception($"Error inesperado: {ex.Message}");
        }
    }

    /// <summary>
    /// Cierra la conexion con la base de datos
    /// </summary>
    public void CerrarConexion() {        
        try {
            if (_conexion.State == ConnectionState.Open)
                _conexion.Close();
        } catch (SqlException ex) {
            throw new Exception($"Error al cerrar la conexión: {ex.Message}");
        } catch (InvalidOperationException ex) {
            throw new Exception($"Error de operación inválida: {ex.Message}");
        } catch (System.Exception ex) {
            throw new Exception($"Error inesperado: {ex.Message}");
        }
    }

    /// <summary>
    /// Crea un Begin Transaction
    /// </summary>
    /// <param name="trx">Nombre de la transaccion</param>
    public void IniciarTransaccion() {
        AbrirConexion();
        try {
            _transaccion = _conexion.BeginTransaction();
        } catch (SqlException ex) {
            throw new Exception($"Error al iniciar la transaction: {ex.Message}");
        } catch (InvalidOperationException ex) {
            throw new Exception($"Error de operación inválida: {ex.Message}");
        } catch (System.Exception ex) {
            throw new Exception($"Error inesperado: {ex.Message}");
        }
    }

    /// <summary>
    /// Confirma la ejecucion de la transaccion
    /// </summary>
    public void CommitTransaccion() {
        _transaccion?.Commit();
    }

    /// <summary>
    /// Reversa la ejecucion de la transaccion
    /// </summary>
    public void RollbackTransaccion() {
        _transaccion?.Rollback();
    }

    /// <summary>
    /// Ejecuta una sentencia SQL y devuelve el número de registros afectados.
    /// Se recomienda usar este método para operaciones INSERT, UPDATE o DELETE.
    /// También es compatible con procedimientos almacenados que realicen modificaciones en la base de datos.
    /// </summary>
    /// <param name="sql">Sentencia SQL a ejecutar (INSERT, UPDATE, DELETE) o el nombre de un procedimiento almacenado.
    /// Ejemplo:
    ///    conexion.EjecutarSentencia("UPDATE Productos SET Nombre = 'Nuevo' WHERE Id = 1", CommandType.Text, null);
    ///    conexion.EjecutarSentencia("MyStoredProcedure", CommandType.StoredProcedure, parametros);
    /// </param>
    /// <param name="tipo">Define si la ejecución es un procedimiento almacenado o una consulta por texto.
    /// Ejemplo: CommandType.StoredProcedure o CommandType.Text.</param>
    /// <param name="parametros">Parámetros opcionales para la ejecución de la sentencia SQL.</param>    
     public int EjecutarSentencia(string sql, CommandType tipo, ParametrosSql parametros = null) {
        using (var comando = CrearComando(sql, tipo, parametros, DefaultTimeout)){
            return comando.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// Ejecuta una consulta SQL y devuelve un único valor.
    /// Ideal para operaciones SELECT que devuelven un solo campo, como conteos, precios o identificadores.
    /// No se recomienda su uso para INSERT, UPDATE o DELETE.
    /// </summary>
    /// <param name="sql">Sentencia SQL a ejecutar, preferiblemente un SELECT o el nombre de un procedimiento almacenado.
    /// Ejemplo:
    /// conexion.EjecutarEscalar("SELECT COUNT(*) FROM Usuarios", CommandType.Text, null);
    /// conexion.EjecutarEscalar("sp_ObtenerPrecioProducto", CommandType.StoredProcedure, parametros);
    /// </param>
    /// <param name="tipo">Define si la ejecución es un procedimiento almacenado o una consulta por texto.
    /// Ejemplo: CommandType.StoredProcedure o CommandType.Text.</param>
    /// <param name="parametros">Parámetros opcionales para la ejecución de la consulta.</param>
    public object EjecutarEscalar(string sql, CommandType tipo, ParametrosSql parametros = null) {
        using (var comando = CrearComando(sql, tipo, parametros, DefaultTimeout)){
            var resultado = comando.ExecuteScalar();
            return resultado ?? DBNull.Value;
        }
    }

    /// <summary>
    /// Ejecuta una consulta SQL y devuelve los resultados en un DataTable.
    /// Se recomienda usar este método solo para operaciones SELECT, ya que devuelve una tabla con los registros obtenidos.
    /// </summary>
    /// <param name="sql">Sentencia SQL a ejecutar (SELECT) o el nombre de un procedimiento almacenado.
    /// Ejemplo:
    ///    conexion.EjecutarConsultaDT("SELECT * FROM Productos", CommandType.Text, null);
    ///    conexion.EjecutarConsultaDT("sp_ObtenerProductos", CommandType.StoredProcedure, parametros);
    /// </param>
    /// <param name="tipo">Define si la ejecución es un procedimiento almacenado o una consulta por texto.
    /// Ejemplo: CommandType.StoredProcedure o CommandType.Text.</param>
    /// <param name="parametros">Parámetros opcionales para la ejecución de la consulta.</param>
     public DataTable EjecutarConsultaDT(string sql, CommandType tipo, ParametrosSql parametros = null) {
        using (var comando = CrearComando(sql, tipo, parametros, DefaultTimeout)){
            using (var adapter = new SqlDataAdapter(comando)){
                var resultado = new DataTable();
                adapter.Fill(resultado);
                return resultado;
            }
        }
    }

    /// <summary>
    /// Ejecuta una consulta SQL y devuelve un DataSet con uno o varios DataTables.
    /// Se recomienda utilizar este método únicamente para consultas (SELECT) o procedimientos almacenados que devuelvan múltiples conjuntos de datos.
    /// </summary>
    /// <param name="sql">Sentencia SQL a ejecutar (solo SELECT) o el nombre de un procedimiento almacenado.</param>
    /// <param name="tipo">Define si la ejecución es un procedimiento almacenado o una consulta por texto.
    /// Ejemplo: CommandType.StoredProcedure o CommandType.Text</param>
    /// <param name="parametros">Parámetros opcionales para la ejecución de la consulta o procedimiento almacenado.</param>
    public DataSet EjecutarConsultaDTS(string sql, CommandType tipo, ParametrosSql parametros = null) {
        using (var comando = CrearComando(sql, tipo, parametros,DefaultTimeout)){
            using (var adapter = new SqlDataAdapter(comando)){
                var resultado = new DataSet();
                adapter.Fill(resultado);
                return resultado;
            }
        }
    }
#endregion
#region Metodos Asíncronos
    /// <summary>
    /// Ejecuta una sentencia SQL de tipo INSERT, UPDATE o DELETE de manera asíncrona.
    /// Devuelve el número de registros afectados por la operación.
    /// </summary>
    /// <param name="sql">Sentencia SQL a ejecutar o el nombre de un procedimiento almacenado.
    /// Ejemplo:
    /// await conexion.EjecutarSentenciaAsync("UPDATE Productos SET Precio = 150 WHERE Id = 1", CommandType.Text, null);
    /// await conexion.EjecutarSentenciaAsync("sp_ActualizarPrecio", CommandType.StoredProcedure, parametros);
    /// </param>
    /// <param name="tipo">Define si la ejecución es un procedimiento almacenado o una consulta por texto.
    /// Ejemplo: CommandType.StoredProcedure o CommandType.Text.</param>
    /// <param name="parametros">Parámetros opcionales para la ejecución de la sentencia SQL.</param>
    /// <returns>Número de registros afectados por la operación.</returns>
    public async Task<int> EjecutarSentenciaAsync(string sql, CommandType tipo, ParametrosSql parametros = null) {
        using (var comando = CrearComando(sql, tipo, parametros,DefaultTimeout)) {
            return await comando.ExecuteNonQueryAsync();
        }
    }

    /// <summary>
    /// Ejecuta una consulta SQL de manera asíncrona y devuelve un único valor.
    /// Ideal para obtener identificadores, conteos o valores específicos de una base de datos.
    /// </summary>
    /// <param name="sql">Sentencia SQL a ejecutar o el nombre de un procedimiento almacenado.
    /// Ejemplo:
    /// await conexion.EjecutarEscalarAsync("SELECT COUNT(*) FROM Usuarios", CommandType.Text, null);
    /// await conexion.EjecutarEscalarAsync("sp_ObtenerPrecioProducto", CommandType.StoredProcedure, parametros);
    /// </param>
    /// <param name="tipo">Define si la ejecución es un procedimiento almacenado o una consulta por texto.
    /// Ejemplo: CommandType.StoredProcedure o CommandType.Text.</param>
    /// <param name="parametros">Parámetros opcionales para la ejecución de la consulta.</param>
    /// <returns>Valor único obtenido de la consulta.</returns>
    public async Task<object> EjecutarEscalarAsync(string sql, CommandType tipo, ParametrosSql parametros = null) {
        using (var comando = CrearComando(sql, tipo, parametros,DefaultTimeout))
        {
            var resultado = await comando.ExecuteScalarAsync();
            return resultado ?? DBNull.Value;
        }
    }

    /// <summary>
    /// Ejecuta una consulta SQL de manera asíncrona y devuelve los registros en un DataTable.
    /// Se recomienda usar este método únicamente para consultas SELECT.
    /// </summary>
    /// <param name="sql">Sentencia SQL a ejecutar o el nombre de un procedimiento almacenado.
    /// Ejemplo:
    /// DataTable productos = await conexion.EjecutarConsultaDTAsync("SELECT * FROM Productos", CommandType.Text, null);
    /// DataTable usuarios = await conexion.EjecutarConsultaDTAsync("sp_ObtenerUsuarios", CommandType.StoredProcedure, parametros);
    /// </param>
    /// <param name="tipo">Define si la ejecución es un procedimiento almacenado o una consulta por texto.
    /// Ejemplo: CommandType.StoredProcedure o CommandType.Text.</param>
    /// <param name="parametros">Parámetros opcionales para la ejecución de la consulta.</param>
    /// <returns>DataTable con los registros obtenidos.</returns>
    public async Task<DataTable> EjecutarConsultaDTAsync(string sql, CommandType tipo, ParametrosSql parametros = null){
        using (var comando = CrearComando(sql, tipo, parametros, DefaultTimeout)){
            using (var reader = await comando.ExecuteReaderAsync()){                
                var resultado = new DataTable();
                resultado.Load(reader);
                return resultado;
            }
        }
    }

    /// <summary>
    /// Ejecuta una consulta SQL de manera asíncrona y devuelve los resultados en un DataSet.
    /// Este método es útil cuando la consulta devuelve múltiples conjuntos de datos (varias tablas).
    /// </summary>
    /// <param name="sql">Sentencia SQL a ejecutar o el nombre de un procedimiento almacenado.
    /// Ejemplo:
    /// DataSet reportes = await conexion.EjecutarConsultaDTSAsync("EXEC sp_ObtenerReportes", CommandType.StoredProcedure, null);
    /// </param>
    /// <param name="tipo">Define si la ejecución es un procedimiento almacenado o una consulta por texto.
    /// Ejemplo: CommandType.StoredProcedure o CommandType.Text.</param>
    /// <param name="parametros">Parámetros opcionales para la ejecución de la consulta.</param>
    /// <returns>DataSet con las tablas obtenidas.</returns>
    public async Task<DataSet> EjecutarConsultaDTSAsync(string sql, CommandType tipo, ParametrosSql parametros = null) {
        using (var comando = CrearComando(sql, tipo, parametros, DefaultTimeout)){
            using (var reader = await comando.ExecuteReaderAsync()){
                var resultado = new DataSet();
                do{
                    var table = new DataTable();
                    table.Load(reader); // Carga los resultados en un DataTable
                    resultado.Tables.Add(table); // Agrega cada tabla al DataSet
                } while (await reader.NextResultAsync()); // Avanza a la siguiente tabla (si existe)
                return resultado;
            }
        }
    }


#endregion

    /// <summary>
    /// Metodo para realizar inserciones masivas en una tabla específica de la base de datos
    /// ⚠️ ALERTA: Este método aún está en desarrollo.
    /// No debe ser utilizado en producción hasta que su implementación
    /// y seguridad sean completamente validadas.
    /// Puede sufrir cambios o ser eliminado en futuras versiones.
    /// </summary>
    /// <param name="nombreTabla">Nombre completo de la tabla de destino (ej: "dbo.Clientes")</param>
    /// <param name="dtTabla">DataTable con los datos a insertar</param>
    /// <param name="timeout">Timeout en segundos para la operación (opcional)</param>
    /// <param name="opciones">Opciones adicionales para SqlBulkCopy (opcional)</param>
    /// <exception cref="ArgumentException">Cuando los datos de entrada no son válidos</exception>
    /// <exception cref="SqlException">Cuando ocurre un error en la base de datos</exception>
    /// <exception cref="InvalidOperationException">Cuando no se puede realizar la operación</exception>
    public void EjecutarMasivoDT(string nombreTabla, DataTable dtTabla, 
                            int? timeout = null, 
                            SqlBulkCopyOptions? opciones = null)
    {
        // Validaciones
        if (string.IsNullOrWhiteSpace(nombreTabla.Trim()))
            throw new ArgumentException("El nombre de la tabla no puede estar vacío.", nameof(nombreTabla));

        if (dtTabla == null || dtTabla.Rows.Count == 0)
            throw new ArgumentException("Los datos a insertar no son válidos.", nameof(dtTabla));

        try {
            // Configuración de SqlBulkCopy
            var bulkOptions = opciones ?? SqlBulkCopyOptions.Default;
            using (var bulkCopy = new SqlBulkCopy(_conexion, bulkOptions, _transaccion)) {
                bulkCopy.DestinationTableName = nombreTabla;
                bulkCopy.BulkCopyTimeout = timeout ?? DefaultTimeout;
                bulkCopy.BatchSize = 5000; // Tamaño de lote óptimo para mayoría de escenarios

                // Mapeo automático de columnas
                foreach (DataColumn col in dtTabla.Columns) {
                    bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                }
                // Notificación de progreso (opcional)
                bulkCopy.SqlRowsCopied += (sender, e) =>  {
                    Console.WriteLine($"Filas copiadas: {e.RowsCopied}");
                };
                bulkCopy.NotifyAfter = 1000; // Notificar cada 1000 filas
                // Ejecución
                bulkCopy.WriteToServer(dtTabla);
            }
        }
        catch (SqlException ex) {
            // Log específico para errores de SQL
            throw new Exception($"Error en inserción masiva (SQL Error {ex.Number}): {ex.Message}", ex);
        } catch (InvalidOperationException ex){
            throw new Exception("Operación de inserción masiva no válida. Verifique la conexión y transacción.", ex);
        } catch (Exception ex){
            throw new Exception("Error inesperado en inserción masiva", ex);
        }
    }

    // Helper method
    private SqlCommand CrearComando(string sql, CommandType tipo, ParametrosSql parametros, int? timeout) {
        if (string.IsNullOrWhiteSpace(sql))
            throw new ArgumentException("El comando SQL no puede estar vacío.", nameof(sql));

        var comando = new SqlCommand(sql, _conexion, _transaccion) {
            CommandType = tipo,
            CommandTimeout = timeout ?? DefaultTimeout
        };

        if (parametros != null) {
            foreach (var param in parametros.ObtenerParametros()) {
                comando.Parameters.Add(new SqlParameter {
                    ParameterName = param.ParameterName,
                    SqlDbType = param.SqlDbType,
                    Size = param.Size,
                    Value = param.Value
                });
            }
        }
        return comando;
    }
    public void Dispose() {
        _transaccion?.Dispose();
        
        if (_conexion.State != ConnectionState.Closed)
            _conexion.Close();
            
        _conexion.Dispose();
        GC.SuppressFinalize(this);
    }
}
