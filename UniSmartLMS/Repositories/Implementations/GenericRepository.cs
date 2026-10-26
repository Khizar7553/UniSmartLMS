using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Repositories.Implementations
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly string _connectionString;
        private readonly string _tableName;

        public GenericRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");

            // ✅ SOLUTION: Get the EXACT table name from [Table] attribute
            var tableAttribute = typeof(T).GetCustomAttribute<TableAttribute>();

            if (tableAttribute != null)
            {
                // Use what's in [Table("Universities")] - EXACT match!
                _tableName = tableAttribute.Name;
            }
            else
            {
                // FALLBACK: Only if no [Table] attribute exists
                // Use a proper pluralization logic
                string className = typeof(T).Name;

                // Common pluralization rules
                if (className.EndsWith("y"))
                    _tableName = className.Substring(0, className.Length - 1) + "ies"; // Library -> Libraries
                else if (className.EndsWith("s"))
                    _tableName = className + "es"; // Class -> Classes
                else
                    _tableName = className + "s"; // Default: add 's'
            }

            // Optional: Log for debugging
            System.Diagnostics.Debug.WriteLine($"Entity: {typeof(T).Name} → Table: {_tableName}");
        }

        // GET BY ID - Uses sp_Generic_GetById
        public async Task<T> GetByIdAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new { TableName = _tableName, Id = id };

                return await connection.QueryFirstOrDefaultAsync<T>(
                    "sp_Generic_GetById",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        // GET ALL - Uses sp_Generic_GetAll
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new { TableName = _tableName };

                return await connection.QueryAsync<T>(
                    "sp_Generic_GetAll",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        // ADD - Uses sp_Generic_Insert
        public async Task<T> AddAsync(T entity)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                // Convert entity to JSON
                var jsonData = JsonSerializer.Serialize(entity);

                var parameters = new DynamicParameters();
                parameters.Add("@TableName", _tableName);
                parameters.Add("@JsonData", jsonData);
                parameters.Add("@NewId", dbType: DbType.Int32, direction: ParameterDirection.Output);

                await connection.ExecuteAsync(
                    "sp_Generic_Insert",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                var newId = parameters.Get<int>("@NewId");

                // Return the newly created entity
                return await GetByIdAsync(newId);
            }
        }

        // UPDATE - Uses sp_Generic_Update
        public async Task UpdateAsync(T entity)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                // Get ID property value
                var idProperty = typeof(T).GetProperty(typeof(T).Name + "Id")
                    ?? typeof(T).GetProperty("Id");

                if (idProperty == null)
                    throw new Exception("ID property not found");

                var idValue = (int)idProperty.GetValue(entity);

                // Convert entity to JSON
                var jsonData = JsonSerializer.Serialize(entity);

                var parameters = new
                {
                    TableName = _tableName,
                    Id = idValue,
                    JsonData = jsonData
                };

                await connection.ExecuteAsync(
                    "sp_Generic_Update",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        // DELETE - Uses sp_Generic_Delete
        public async Task DeleteAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new { TableName = _tableName, Id = id };

                await connection.ExecuteAsync(
                    "sp_Generic_Delete",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        // EXISTS - Uses sp_Generic_Exists
        public async Task<bool> ExistsAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@TableName", _tableName);
                parameters.Add("@Id", id);
                parameters.Add("@Exists", dbType: DbType.Boolean, direction: ParameterDirection.Output);

                await connection.ExecuteAsync(
                    "sp_Generic_Exists",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return parameters.Get<bool>("@Exists");
            }
        }

        // COUNT - Uses sp_Generic_Count
        public async Task<int> CountAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@TableName", _tableName);
                parameters.Add("@RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

                await connection.ExecuteAsync(
                    "sp_Generic_Count",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return parameters.Get<int>("@RowCount");
            }
        }

        // SEARCH - Uses sp_Generic_Search
        public async Task<IEnumerable<T>> SearchAsync(string columnName, string searchTerm)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new
                {
                    TableName = _tableName,
                    SearchColumn = columnName,
                    SearchTerm = searchTerm
                };

                return await connection.QueryAsync<T>(
                    "sp_Generic_Search",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
        }
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = $"SELECT * FROM {_tableName} WHERE IsActive = 1";
            var result = await connection.QueryAsync<T>(sql);
            return result.AsQueryable().Where(predicate).ToList();
        }

        // DIRECT STORED PROCEDURE EXECUTION (for custom SPs)
        public async Task<IEnumerable<T>> ExecuteProcAsync(string procName, object parameters = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QueryAsync<T>(
                    procName,
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        public async Task<T> ExecuteProcSingleAsync(string procName, object parameters = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QueryFirstOrDefaultAsync<T>(
                    procName,
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        public async Task<int> ExecuteProcNonQueryAsync(string procName, object parameters = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.ExecuteAsync(
                    procName,
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
        }
    }
}