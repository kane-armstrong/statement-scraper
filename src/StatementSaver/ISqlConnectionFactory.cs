using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace StatementSaver
{
    public interface ISqlConnectionFactory
    {
        Task<SqlConnection> GetOpenConnection(CancellationToken cancellationToken);
    }
}
