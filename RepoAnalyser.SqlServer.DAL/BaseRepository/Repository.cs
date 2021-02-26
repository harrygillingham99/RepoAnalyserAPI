using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Serilog;

namespace RepoAnalyser.SqlServer.DAL.BaseRepository
{
    public abstract class Repository
    {
        private readonly string _connectionString;

        protected Repository(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected async Task<T> Invoke<T>(Func<IDbConnection, Task<T>> getData)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                return await getData(connection);
            }
            catch (Exception ex)
            {
                var exceptionMsg = $"{GetType().FullName}.Invoke experienced a {ex.GetType()}";
                Log.Error(ex, exceptionMsg);
                throw new Exception(exceptionMsg, ex);
            }
        }

        protected async Task InvokeWithTransaction<T>(Func<IDbConnection, Task<T>> dbOperation)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var tran = connection.BeginTransaction();
            try
            {
                await dbOperation(connection);
                tran.Commit();
            }
            catch (Exception ex)
            {
                tran.Rollback();
                var exceptionMsg = $"{GetType().FullName}.Invoke experienced a {ex.GetType()}";
                Log.Error(ex, exceptionMsg);
                throw new Exception(exceptionMsg, ex);
            }
        }
    }
}
