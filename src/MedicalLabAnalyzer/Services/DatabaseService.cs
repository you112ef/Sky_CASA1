using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MedicalLabAnalyzer.Common.Exceptions;
using MedicalLabAnalyzer.Common.Results;
using MedicalLabAnalyzer.Common.ErrorHandling;

namespace MedicalLabAnalyzer.Services
{
    public interface IDatabaseService
    {
        Task<Result<IDbConnection>> GetConnectionAsync();
        Result<IDbConnection> GetConnection();
        Task<Result> TestConnectionAsync();
        Task<Result> InitializeDatabaseAsync();
        Task<Result<bool>> DatabaseExistsAsync();
        Task<Result> BackupDatabaseAsync(string backupPath);
        Task<Result> RestoreDatabaseAsync(string backupPath);
        Task<Result<T>> ExecuteScalarAsync<T>(string sql, object parameters = null);
        Task<Result<int>> ExecuteNonQueryAsync(string sql, object parameters = null);
        Task<Result> HealthCheckAsync();
        string DatabasePath { get; }
        string ConnectionString { get; }
    }

    public class DatabaseService : IDatabaseService
    {
        private readonly string _dbPath;
        private readonly string _connStr;
        private readonly ILogger<DatabaseService> _logger;
        private readonly IErrorHandlerService _errorHandler;
        private readonly object _initLock = new object();
        private bool _isInitialized = false;

        public string DatabasePath => _dbPath;
        public string ConnectionString => _connStr;

        public DatabaseService(ILogger<DatabaseService> logger = null, IErrorHandlerService errorHandler = null, string dbPath = null)
        {
            _logger = logger;
            _errorHandler = errorHandler;
            
            try
            {
                _dbPath = dbPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "medical_lab.db");
                var dir = Path.GetDirectoryName(_dbPath);
                
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                    _logger?.LogInformation("Created database directory: {DatabaseDir}", dir);
                }

                _connStr = $"Data Source={_dbPath};Version=3;Pooling=True;Max Pool Size=100;Connection Timeout=30;";
                
                // Initialize database if it doesn't exist
                var initResult = InitializeDatabaseAsync().GetAwaiter().GetResult();
                if (!initResult.IsSuccess)
                {
                    _logger?.LogError("Failed to initialize database: {Error}", initResult.ErrorMessage);
                    throw new DatabaseException($"Database initialization failed: {initResult.ErrorMessage}", "INIT");
                }
            }
            catch (Exception ex) when (!(ex is DatabaseException))
            {
                _logger?.LogError(ex, "Error initializing DatabaseService");
                throw new DatabaseException($"Failed to initialize database service: {ex.Message}", "INIT", ex);
            }
        }

        public async Task<Result<IDbConnection>> GetConnectionAsync()
        {
            return await (_errorHandler?.HandleAsync(async () =>
            {
                var conn = new SQLiteConnection(_connStr);
                await conn.OpenAsync();
                
                // Test the connection
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT 1";
                await cmd.ExecuteScalarAsync();
                
                return (IDbConnection)conn;
            }, "GetConnectionAsync", new { DatabasePath = _dbPath }) ?? 
            Result.TryCatchAsync(async () =>
            {
                var conn = new SQLiteConnection(_connStr);
                await conn.OpenAsync();
                
                // Test the connection
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT 1";
                await cmd.ExecuteScalarAsync();
                
                return (IDbConnection)conn;
            }, ex => $"Failed to create database connection: {ex.Message}", "DB_CONNECTION_FAILED"));
        }

        public Result<IDbConnection> GetConnection()
        {
            return _errorHandler?.Handle(() =>
            {
                var conn = new SQLiteConnection(_connStr);
                conn.Open();
                
                // Test the connection
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT 1";
                cmd.ExecuteScalar();
                
                return (IDbConnection)conn;
            }, "GetConnection", new { DatabasePath = _dbPath }) ?? 
            Result.TryCatch(() =>
            {
                var conn = new SQLiteConnection(_connStr);
                conn.Open();
                
                // Test the connection
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT 1";
                cmd.ExecuteScalar();
                
                return (IDbConnection)conn;
            }, ex => $"Failed to create database connection: {ex.Message}", "DB_CONNECTION_FAILED");
        }

        public async Task<Result> TestConnectionAsync()
        {
            return await (_errorHandler?.HandleAsync(async () =>
            {
                using var conn = await GetConnectionAsync();
                if (conn.IsFailure)
                    throw new DatabaseException(conn.ErrorMessage, "TEST_CONNECTION");
                    
                using var cmd = conn.Value.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table'";
                await ((SQLiteCommand)cmd).ExecuteScalarAsync();
                
                _logger?.LogInformation("Database connection test successful");
            }, "TestConnectionAsync", new { DatabasePath = _dbPath }) ??
            Result.TryCatchAsync(async () =>
            {
                using var conn = await GetConnectionAsync();
                if (conn.IsFailure)
                    throw new DatabaseException(conn.ErrorMessage, "TEST_CONNECTION");
                    
                using var cmd = conn.Value.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table'";
                await ((SQLiteCommand)cmd).ExecuteScalarAsync();
                
                _logger?.LogInformation("Database connection test successful");
            }, ex => $"Database connection test failed: {ex.Message}", "DB_TEST_FAILED"));
        }

        public async Task<Result> InitializeDatabaseAsync()
        {
            if (_isInitialized) return Result.Success();

            lock (_initLock)
            {
                if (_isInitialized) return Result.Success();

                return _errorHandler?.Handle(() =>
                {
                    if (!File.Exists(_dbPath))
                    {
                        _logger?.LogInformation("Creating new database file: {DatabasePath}", _dbPath);
                        SQLiteConnection.CreateFile(_dbPath);
                        
                        // Execute init script if present
                        var initSqlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "init_schema_full.sql");
                        if (File.Exists(initSqlPath))
                        {
                            _logger?.LogInformation("Executing database initialization script: {ScriptPath}", initSqlPath);
                            
                            using var conn = new SQLiteConnection(_connStr);
                            conn.Open();
                            
                            var sql = File.ReadAllText(initSqlPath);
                            if (string.IsNullOrWhiteSpace(sql))
                                throw new DatabaseException("Database initialization script is empty", "INIT");
                            
                            using var cmd = conn.CreateCommand();
                            cmd.CommandText = sql;
                            cmd.CommandTimeout = 120; // 2 minutes timeout for init
                            cmd.ExecuteNonQuery();
                            
                            _logger?.LogInformation("Database initialization completed successfully");
                        }
                        else
                        {
                            _logger?.LogWarning("Database initialization script not found: {ScriptPath}", initSqlPath);
                        }
                    }
                    else
                    {
                        _logger?.LogInformation("Database file already exists: {DatabasePath}", _dbPath);
                    }
                    
                    _isInitialized = true;
                }, "InitializeDatabaseAsync", new { DatabasePath = _dbPath }) ??
                Result.TryCatch(() =>
                {
                    if (!File.Exists(_dbPath))
                    {
                        _logger?.LogInformation("Creating new database file: {DatabasePath}", _dbPath);
                        SQLiteConnection.CreateFile(_dbPath);
                        
                        // Execute init script if present
                        var initSqlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "init_schema_full.sql");
                        if (File.Exists(initSqlPath))
                        {
                            _logger?.LogInformation("Executing database initialization script: {ScriptPath}", initSqlPath);
                            
                            using var conn = new SQLiteConnection(_connStr);
                            conn.Open();
                            
                            var sql = File.ReadAllText(initSqlPath);
                            if (string.IsNullOrWhiteSpace(sql))
                                throw new DatabaseException("Database initialization script is empty", "INIT");
                            
                            using var cmd = conn.CreateCommand();
                            cmd.CommandText = sql;
                            cmd.CommandTimeout = 120; // 2 minutes timeout for init
                            cmd.ExecuteNonQuery();
                            
                            _logger?.LogInformation("Database initialization completed successfully");
                        }
                        else
                        {
                            _logger?.LogWarning("Database initialization script not found: {ScriptPath}", initSqlPath);
                        }
                    }
                    else
                    {
                        _logger?.LogInformation("Database file already exists: {DatabasePath}", _dbPath);
                    }
                    
                    _isInitialized = true;
                }, ex => $"Failed to initialize database: {ex.Message}", "DB_INIT_FAILED");
            }
        }

        public async Task<Result<bool>> DatabaseExistsAsync()
        {
            return await (_errorHandler?.HandleAsync(async () =>
            {
                if (!File.Exists(_dbPath))
                    return false;

                // Try to open and verify database structure
                using var conn = await GetConnectionAsync();
                if (conn.IsFailure)
                    return false;
                    
                using var cmd = conn.Value.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table'";
                var tableCount = Convert.ToInt32(await ((SQLiteCommand)cmd).ExecuteScalarAsync());
                
                return tableCount > 0;
            }, "DatabaseExistsAsync") ??
            Result.TryCatchAsync(async () =>
            {
                if (!File.Exists(_dbPath))
                    return false;

                // Try to open and verify database structure
                using var conn = await GetConnectionAsync();
                if (conn.IsFailure)
                    return false;
                    
                using var cmd = conn.Value.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table'";
                var tableCount = Convert.ToInt32(await ((SQLiteCommand)cmd).ExecuteScalarAsync());
                
                return tableCount > 0;
            }, ex => $"Failed to check database existence: {ex.Message}", "DB_EXISTS_CHECK_FAILED"));
        }

        public async Task<Result> BackupDatabaseAsync(string backupPath)
        {
            return await (_errorHandler?.HandleAsync(async () =>
            {
                if (string.IsNullOrWhiteSpace(backupPath))
                    throw new ArgumentException("Backup path cannot be null or empty", nameof(backupPath));

                var backupDir = Path.GetDirectoryName(backupPath);
                if (!Directory.Exists(backupDir))
                    Directory.CreateDirectory(backupDir);

                if (!File.Exists(_dbPath))
                    throw new FileOperationException("Source database file does not exist", _dbPath, "BACKUP");

                // Create backup with timestamp
                var timestampedBackupPath = Path.Combine(
                    backupDir, 
                    $"{Path.GetFileNameWithoutExtension(backupPath)}_{DateTime.Now:yyyyMMdd_HHmmss}{Path.GetExtension(backupPath)}");

                await Task.Run(() => File.Copy(_dbPath, timestampedBackupPath, true));
                
                _logger?.LogInformation("Database backed up successfully to: {BackupPath}", timestampedBackupPath);
            }, "BackupDatabaseAsync", new { SourcePath = _dbPath, BackupPath = backupPath }) ??
            Result.TryCatchAsync(async () =>
            {
                if (string.IsNullOrWhiteSpace(backupPath))
                    throw new ArgumentException("Backup path cannot be null or empty", nameof(backupPath));

                var backupDir = Path.GetDirectoryName(backupPath);
                if (!Directory.Exists(backupDir))
                    Directory.CreateDirectory(backupDir);

                if (!File.Exists(_dbPath))
                    throw new FileOperationException("Source database file does not exist", _dbPath, "BACKUP");

                // Create backup with timestamp
                var timestampedBackupPath = Path.Combine(
                    backupDir, 
                    $"{Path.GetFileNameWithoutExtension(backupPath)}_{DateTime.Now:yyyyMMdd_HHmmss}{Path.GetExtension(backupPath)}");

                await Task.Run(() => File.Copy(_dbPath, timestampedBackupPath, true));
                
                _logger?.LogInformation("Database backed up successfully to: {BackupPath}", timestampedBackupPath);
            }, ex => $"Failed to backup database: {ex.Message}", "DB_BACKUP_FAILED"));
        }

        public async Task<Result> RestoreDatabaseAsync(string backupPath)
        {
            return await (_errorHandler?.HandleAsync(async () =>
            {
                if (string.IsNullOrWhiteSpace(backupPath))
                    throw new ArgumentException("Backup path cannot be null or empty", nameof(backupPath));

                if (!File.Exists(backupPath))
                    throw new FileOperationException("Backup file does not exist", backupPath, "RESTORE");

                // Create a backup of current database before restoring
                if (File.Exists(_dbPath))
                {
                    var currentBackupPath = $"{_dbPath}.backup_{DateTime.Now:yyyyMMdd_HHmmss}";
                    await Task.Run(() => File.Copy(_dbPath, currentBackupPath, true));
                    _logger?.LogInformation("Current database backed up to: {BackupPath}", currentBackupPath);
                }

                await Task.Run(() => File.Copy(backupPath, _dbPath, true));
                
                // Test the restored database
                var testResult = await TestConnectionAsync();
                if (!testResult.IsSuccess)
                    throw new DatabaseException($"Restored database is corrupted: {testResult.ErrorMessage}", "RESTORE");
                
                _logger?.LogInformation("Database restored successfully from: {BackupPath}", backupPath);
            }, "RestoreDatabaseAsync", new { BackupPath = backupPath, TargetPath = _dbPath }) ??
            Result.TryCatchAsync(async () =>
            {
                if (string.IsNullOrWhiteSpace(backupPath))
                    throw new ArgumentException("Backup path cannot be null or empty", nameof(backupPath));

                if (!File.Exists(backupPath))
                    throw new FileOperationException("Backup file does not exist", backupPath, "RESTORE");

                // Create a backup of current database before restoring
                if (File.Exists(_dbPath))
                {
                    var currentBackupPath = $"{_dbPath}.backup_{DateTime.Now:yyyyMMdd_HHmmss}";
                    await Task.Run(() => File.Copy(_dbPath, currentBackupPath, true));
                    _logger?.LogInformation("Current database backed up to: {BackupPath}", currentBackupPath);
                }

                await Task.Run(() => File.Copy(backupPath, _dbPath, true));
                
                // Test the restored database
                var testResult = await TestConnectionAsync();
                if (!testResult.IsSuccess)
                    throw new DatabaseException($"Restored database is corrupted: {testResult.ErrorMessage}", "RESTORE");
                
                _logger?.LogInformation("Database restored successfully from: {BackupPath}", backupPath);
            }, ex => $"Failed to restore database: {ex.Message}", "DB_RESTORE_FAILED"));
        }

        public async Task<Result<T>> ExecuteScalarAsync<T>(string sql, object parameters = null)
        {
            return await (_errorHandler?.HandleAsync(async () =>
            {
                if (string.IsNullOrWhiteSpace(sql))
                    throw new ArgumentException("SQL cannot be null or empty", nameof(sql));

                using var conn = await GetConnectionAsync();
                if (conn.IsFailure)
                    throw new DatabaseException(conn.ErrorMessage, "EXECUTE_SCALAR");

                using var cmd = conn.Value.CreateCommand();
                cmd.CommandText = sql;
                cmd.CommandTimeout = 30;

                // Add parameters if provided
                if (parameters != null)
                {
                    foreach (var prop in parameters.GetType().GetProperties())
                    {
                        var param = cmd.CreateParameter();
                        param.ParameterName = $"@{prop.Name}";
                        param.Value = prop.GetValue(parameters) ?? DBNull.Value;
                        cmd.Parameters.Add(param);
                    }
                }

                var result = await ((SQLiteCommand)cmd).ExecuteScalarAsync();
                return result == null || result == DBNull.Value ? default(T) : (T)Convert.ChangeType(result, typeof(T));
            }, "ExecuteScalarAsync", new { Sql = sql, Parameters = parameters }) ??
            Result.TryCatchAsync(async () =>
            {
                if (string.IsNullOrWhiteSpace(sql))
                    throw new ArgumentException("SQL cannot be null or empty", nameof(sql));

                using var conn = await GetConnectionAsync();
                if (conn.IsFailure)
                    throw new DatabaseException(conn.ErrorMessage, "EXECUTE_SCALAR");

                using var cmd = conn.Value.CreateCommand();
                cmd.CommandText = sql;
                cmd.CommandTimeout = 30;

                // Add parameters if provided
                if (parameters != null)
                {
                    foreach (var prop in parameters.GetType().GetProperties())
                    {
                        var param = cmd.CreateParameter();
                        param.ParameterName = $"@{prop.Name}";
                        param.Value = prop.GetValue(parameters) ?? DBNull.Value;
                        cmd.Parameters.Add(param);
                    }
                }

                var result = await ((SQLiteCommand)cmd).ExecuteScalarAsync();
                return result == null || result == DBNull.Value ? default(T) : (T)Convert.ChangeType(result, typeof(T));
            }, ex => $"Failed to execute scalar query: {ex.Message}", "DB_EXECUTE_SCALAR_FAILED"));
        }

        public async Task<Result<int>> ExecuteNonQueryAsync(string sql, object parameters = null)
        {
            return await (_errorHandler?.HandleAsync(async () =>
            {
                if (string.IsNullOrWhiteSpace(sql))
                    throw new ArgumentException("SQL cannot be null or empty", nameof(sql));

                using var conn = await GetConnectionAsync();
                if (conn.IsFailure)
                    throw new DatabaseException(conn.ErrorMessage, "EXECUTE_NON_QUERY");

                using var cmd = conn.Value.CreateCommand();
                cmd.CommandText = sql;
                cmd.CommandTimeout = 30;

                // Add parameters if provided
                if (parameters != null)
                {
                    foreach (var prop in parameters.GetType().GetProperties())
                    {
                        var param = cmd.CreateParameter();
                        param.ParameterName = $"@{prop.Name}";
                        param.Value = prop.GetValue(parameters) ?? DBNull.Value;
                        cmd.Parameters.Add(param);
                    }
                }

                return await ((SQLiteCommand)cmd).ExecuteNonQueryAsync();
            }, "ExecuteNonQueryAsync", new { Sql = sql, Parameters = parameters }) ??
            Result.TryCatchAsync(async () =>
            {
                if (string.IsNullOrWhiteSpace(sql))
                    throw new ArgumentException("SQL cannot be null or empty", nameof(sql));

                using var conn = await GetConnectionAsync();
                if (conn.IsFailure)
                    throw new DatabaseException(conn.ErrorMessage, "EXECUTE_NON_QUERY");

                using var cmd = conn.Value.CreateCommand();
                cmd.CommandText = sql;
                cmd.CommandTimeout = 30;

                // Add parameters if provided
                if (parameters != null)
                {
                    foreach (var prop in parameters.GetType().GetProperties())
                    {
                        var param = cmd.CreateParameter();
                        param.ParameterName = $"@{prop.Name}";
                        param.Value = prop.GetValue(parameters) ?? DBNull.Value;
                        cmd.Parameters.Add(param);
                    }
                }

                return await ((SQLiteCommand)cmd).ExecuteNonQueryAsync();
            }, ex => $"Failed to execute non-query: {ex.Message}", "DB_EXECUTE_NON_QUERY_FAILED"));
        }

        public async Task<Result> HealthCheckAsync()
        {
            return await (_errorHandler?.HandleAsync(async () =>
            {
                // Test basic connection
                var connectionTest = await TestConnectionAsync();
                if (!connectionTest.IsSuccess)
                    throw new DatabaseException($"Connection test failed: {connectionTest.ErrorMessage}", "HEALTH_CHECK");

                // Check database file integrity
                var integrityResult = await ExecuteScalarAsync<string>("PRAGMA integrity_check");
                if (!integrityResult.IsSuccess || integrityResult.Value != "ok")
                    throw new DatabaseException($"Database integrity check failed: {integrityResult.ErrorMessage}", "HEALTH_CHECK");

                // Check if critical tables exist
                var tablesResult = await ExecuteScalarAsync<int>("SELECT COUNT(*) FROM sqlite_master WHERE type='table'");
                if (!tablesResult.IsSuccess || tablesResult.Value == 0)
                    throw new DatabaseException("No tables found in database", "HEALTH_CHECK");

                _logger?.LogInformation("Database health check passed - {TableCount} tables found", tablesResult.Value);
            }, "HealthCheckAsync") ??
            Result.TryCatchAsync(async () =>
            {
                // Test basic connection
                var connectionTest = await TestConnectionAsync();
                if (!connectionTest.IsSuccess)
                    throw new DatabaseException($"Connection test failed: {connectionTest.ErrorMessage}", "HEALTH_CHECK");

                // Check database file integrity
                var integrityResult = await ExecuteScalarAsync<string>("PRAGMA integrity_check");
                if (!integrityResult.IsSuccess || integrityResult.Value != "ok")
                    throw new DatabaseException($"Database integrity check failed: {integrityResult.ErrorMessage}", "HEALTH_CHECK");

                // Check if critical tables exist
                var tablesResult = await ExecuteScalarAsync<int>("SELECT COUNT(*) FROM sqlite_master WHERE type='table'");
                if (!tablesResult.IsSuccess || tablesResult.Value == 0)
                    throw new DatabaseException("No tables found in database", "HEALTH_CHECK");

                _logger?.LogInformation("Database health check passed - {TableCount} tables found", tablesResult.Value);
            }, ex => $"Database health check failed: {ex.Message}", "DB_HEALTH_CHECK_FAILED"));
        }
    }
}