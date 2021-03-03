using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;
using Serilog;

namespace RepoAnalyser.SqlServer.DAL.BaseRepository
{
    public abstract class Repository
    {
        private readonly string _connectionString;
        private readonly Stopwatch _stopwatch;

        protected Repository(string connectionString)
        {
            _connectionString = connectionString;
            _stopwatch = new Stopwatch();
        }

        protected async Task<T> Invoke<T>(Func<IDbConnection, Task<T>> getData)
        {
            _stopwatch.Start();
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
            finally
            {
                _stopwatch.Stop();
                Debug.WriteLine($"SQL Operation completed in {_stopwatch.ElapsedMilliseconds}");
            }
        }

        protected async Task InvokeWithTransaction<T>(Func<IDbConnection, Task<T>> dbOperation, bool rollbackOnFault = true)
        {
            _stopwatch.Start();
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
                if (rollbackOnFault)
                {
                    tran.Rollback();
                }
                else
                {
                    Log.Information(ex, $"Rollback skipped after exception for {dbOperation.GetType()}");
                }
                var exceptionMsg = $"{GetType().FullName}.InvokeWithTransaction experienced a {ex.GetType()}";
                Log.Error(ex, exceptionMsg);
                throw new Exception(exceptionMsg, ex);
            }
            finally
            {
                _stopwatch.Stop();
                Debug.WriteLine($"SQL Transaction Operation completed in {_stopwatch.ElapsedMilliseconds}");
            }
        }
    }
}
