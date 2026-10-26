using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using UniSmartLMS.Services.Interfaces;

namespace UniSmartLMS.Services.Implementations
{
    public class AccessionService : IAccessionService
    {
        private readonly string _connectionString;

        public AccessionService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<string> GenerateAccessionNumberAsync(int branchId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // Get next sequence value
            var cmd = new SqlCommand("SELECT NEXT VALUE FOR AccessionSequence", connection);
            var seq = await cmd.ExecuteScalarAsync();

            return $"ACC-{branchId}-{DateTime.Now.Year}-{seq:D6}";
        }
    }

    public interface IAccessionService
    {
        Task<string> GenerateAccessionNumberAsync(int branchId);
    }
}