using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace lunchin.Optimizely.Cloud.Extensions;

public static class DatabaseUtilities
{
    private static readonly Lazy<IConfiguration> _configuration = new (ServiceLocator.Current.GetInstance<IConfiguration>);
    private static readonly Lazy<ILogger> _logger = new(new LoggerFactory().CreateLogger(nameof(DatabaseUtilities)));
    private static readonly string[] _separator = ["\n", "\r"];

    public static async Task<bool> ExecuteNonQueryAsync(string conectionStringName,
        string commandText,
        CommandType commandType = CommandType.StoredProcedure,
        SqlParameter[]? sqlParameters = null)
    {
        try
        {
            await using (var connection = new SqlConnection(_configuration.Value.GetConnectionString(conectionStringName)))
            {
                await connection.OpenAsync();
                await using (var transaction = await connection.BeginTransactionAsync())
                {
                    var command = new SqlCommand
                    {
                        Connection = connection,
                        Transaction = transaction as SqlTransaction,
                        CommandType = commandType,
                        CommandText = commandText
                    };

                    if (sqlParameters != null)
                    {
                        command.Parameters.AddRange(sqlParameters);
                    }

                    await command.ExecuteNonQueryAsync();
                    await transaction.CommitAsync();
                }
            }
            return true;
        }
        catch (Exception exn)
        {
            _logger.Value.LogError(exn, exn.Message);
            throw;
        }
    }

    public static async Task<List<T>?> ExecuteReaderAsync<T>(string conectionStringName,
        string commandText,
        Func<SqlDataReader, T> populateItem,
        CommandType commandType = CommandType.StoredProcedure,
        SqlParameter[]? sqlParameters = null)
    {
        try
        {

            List<T> result = [];
            await using (var connection = new SqlConnection(_configuration.Value.GetConnectionString(conectionStringName)))
            {
                await connection.OpenAsync();
                var command = new SqlCommand
                {
                    Connection = connection,
                    CommandType = commandType,
                    CommandText = commandText
                };

                if (sqlParameters != null)
                {
                    command.Parameters.AddRange(sqlParameters);
                }

                await using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(populateItem(reader));
                    }
                }
            }

            return result;
        }
        catch (Exception exn)
        {
            _logger.Value.LogError(exn, exn.Message);
        }

        return default;
    }

    public static async Task<bool> RunUpgradeScript(string conectionStringName,
        string folderName,
        string latestVersion,
        string dbVersion = "0.0.0")
    {
        var assembly = Assembly.GetExecutingAssembly();
        var sqlFiles = assembly.GetManifestResourceNames()
            .Where(x => x.StartsWith(folderName) && x.EndsWith(".sql"))
            .OrderBy(x => x);

        var version = new Version(dbVersion);
        var lastestVersion = new Version(latestVersion);


        foreach (var file in sqlFiles)
        {
            var fileVersion = new Version(file.Replace($"{folderName}.v", "").Replace(".sql", ""));
            if (fileVersion <= version)
            {
                continue;
            }

            await using (var stream = assembly.GetManifestResourceStream(file))
            {
                if (stream == null)
                {
                    continue;
                }

                using (var reader = new StreamReader(stream))
                {
                    var sqlScript = await reader.ReadToEndAsync();
                    if (!string.IsNullOrWhiteSpace(sqlScript))
                    {
                        foreach (var batchedScript in CreateBatchCommands(sqlScript))
                        {
                            var result = await ExecuteNonQueryAsync(conectionStringName, batchedScript, CommandType.Text);
                            if (!result)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
        }
        return true;
    }

    private static List<string> CreateBatchCommands(string sqlScript)
    {
        var list = new List<string>();
        var sqlBatch = new StringBuilder();
        foreach (var line in sqlScript.Split(_separator, StringSplitOptions.RemoveEmptyEntries))
        {
            if (line.ToUpperInvariant().Trim() == "GO")
            {
                list.Add(sqlBatch.ToString());
                sqlBatch = new StringBuilder();
            }
            else
            {
                var unused = sqlBatch.AppendLine(line);
            }
        }
        return list;
    }
}
