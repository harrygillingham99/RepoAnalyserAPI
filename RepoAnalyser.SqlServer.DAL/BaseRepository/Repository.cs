using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;
using Dapper;
using Serilog;

namespace RepoAnalyser.SqlServer.DAL.BaseRepository
{
    public abstract class Repository
    {
        private const string InfoMessageTemplate = "SQL Info: operation completed in {0}";
        private const string ErrorMessageTemplate = "SQL Error: {0}.Invoke experienced a {1}";
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
                LogError(GetType().FullName, ex);
                throw;
            }
            finally
            {
                _stopwatch.Stop();
                LogInfo(_stopwatch.ElapsedMilliseconds);
            }
        }

        protected async Task<T> InvokeMultiQuery<T>(Func<IDbConnection, SqlMapper.GridReader, Task<T>> getData,
            string sql, object sqlParams)
        {
            _stopwatch.Start();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                using var multiReader = connection.QueryMultipleAsync(sql, sqlParams);
                return await getData(connection, await multiReader);
            }
            catch (Exception ex)
            {
                LogError(GetType().FullName, ex);
                throw;
            }
            finally
            {
                _stopwatch.Stop();
                LogInfo(_stopwatch.ElapsedMilliseconds);
            }
        }

        private static void LogInfo(long milliseconds)
        {
            var messageText = string.Format(InfoMessageTemplate, milliseconds);
            Debug.WriteLine(messageText);
            Log.Information(messageText);
        }

        private static void LogError(string method, Exception exception)
        {
            var exceptionMsg = string.Format(ErrorMessageTemplate, method, exception.GetType());
            Log.Error(exception, exceptionMsg);
        }
    }
}