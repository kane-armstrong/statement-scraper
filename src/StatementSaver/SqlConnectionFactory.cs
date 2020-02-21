using Microsoft.Extensions.Options;
using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace StatementSaver
{
    public class SqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly string _connectionString;

        public SqlConnectionFactory(IOptions<SqlConnectionFactoryOptions> options)
        {
            _connectionString = options?.Value?.ConnectionString ??
                                throw new ArgumentNullException(nameof(SqlConnectionFactoryOptions.ConnectionString));
        }

        public async Task<SqlConnection> GetOpenConnection(CancellationToken cancellationToken)
        {
            var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            return connection;
        }
    }
}
