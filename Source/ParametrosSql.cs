using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AccesoDatos;

public class ParametrosSql {
    private readonly List<SqlParameter> _parametros;

    /// <summary>
    /// Inicializa una nueva instancia de la clase ParametrosSql
    /// </summary>
    public ParametrosSql() {
        _parametros = new List<SqlParameter>();
    }

    /// <summary>
    /// Agrega un nuevo parámetro a la lista
    /// </summary>
    /// <param name="nombre">Nombre del parámetro de SQL. Ej: @co_empr</param>
    /// <param name="valor">El valor del parámetro</param>
    /// <param name="tipoDato">Tipo de dato del parámetro</param>
    public void AgregarParametro(string nombre, object valor, SqlDbType tipoDato) {
        if (string.IsNullOrWhiteSpace(nombre)) {
            throw new ArgumentException("El nombre del parámetro no puede estar vacío o ser nulo.", nameof(nombre));
        }

        var parametro = new SqlParameter(nombre, tipoDato) {
            Value = valor ?? DBNull.Value
        };

        _parametros.Add(parametro);
    }
    /// <summary>
    /// Limpia la lista de parámetros
    /// </summary>
    public void Limpiar()
    {
        _parametros.Clear();
    }

    /// <summary>
    /// Retorna una lista de solo lectura con los parámetros
    /// </summary>
    public IReadOnlyList<SqlParameter> ObtenerParametros() {
        return _parametros.AsReadOnly();
    }
}