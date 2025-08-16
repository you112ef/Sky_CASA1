using System;
using System.Collections.Generic;
using System.Linq;

namespace MedicalLabAnalyzer.Common.Results
{
    /// <summary>
    /// Represents the result of an operation that can succeed or fail
    /// </summary>
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string ErrorMessage { get; }
        public string ErrorCode { get; }
        public Exception Exception { get; }
        public DateTime Timestamp { get; }

        protected Result(bool isSuccess, string errorMessage = null, string errorCode = null, Exception exception = null)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
            Exception = exception;
            Timestamp = DateTime.UtcNow;
        }

        public static Result Success()
        {
            return new Result(true);
        }

        public static Result Failure(string errorMessage, string errorCode = null, Exception exception = null)
        {
            return new Result(false, errorMessage, errorCode, exception);
        }

        public static Result<T> Success<T>(T value)
        {
            return new Result<T>(value, true);
        }

        public static Result<T> Failure<T>(string errorMessage, string errorCode = null, Exception exception = null)
        {
            return new Result<T>(default(T), false, errorMessage, errorCode, exception);
        }
    }

    /// <summary>
    /// Represents the result of an operation that can succeed or fail with a return value
    /// </summary>
    /// <typeparam name="T">The type of the value returned on success</typeparam>
    public class Result<T> : Result
    {
        public T Value { get; }

        internal Result(T value, bool isSuccess, string errorMessage = null, string errorCode = null, Exception exception = null)
            : base(isSuccess, errorMessage, errorCode, exception)
        {
            Value = value;
        }

        public static implicit operator Result<T>(T value)
        {
            return Success(value);
        }

        public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, string, Exception, TResult> onFailure)
        {
            return IsSuccess ? onSuccess(Value) : onFailure(ErrorMessage, ErrorCode, Exception);
        }

        public Result<TResult> Map<TResult>(Func<T, TResult> mapper)
        {
            return IsSuccess ? Success(mapper(Value)) : Failure<TResult>(ErrorMessage, ErrorCode, Exception);
        }

        public Result<TResult> Bind<TResult>(Func<T, Result<TResult>> binder)
        {
            return IsSuccess ? binder(Value) : Failure<TResult>(ErrorMessage, ErrorCode, Exception);
        }
    }

    /// <summary>
    /// Represents validation result with multiple errors
    /// </summary>
    public class ValidationResult : Result
    {
        public IReadOnlyList<string> Errors { get; }

        private ValidationResult(bool isSuccess, IEnumerable<string> errors = null) 
            : base(isSuccess, errors?.FirstOrDefault())
        {
            Errors = errors?.ToList().AsReadOnly() ?? new List<string>().AsReadOnly();
        }

        public static ValidationResult Success()
        {
            return new ValidationResult(true);
        }

        public static ValidationResult Failure(params string[] errors)
        {
            return new ValidationResult(false, errors);
        }

        public static ValidationResult Failure(IEnumerable<string> errors)
        {
            return new ValidationResult(false, errors);
        }

        public ValidationResult Combine(ValidationResult other)
        {
            if (IsSuccess && other.IsSuccess)
                return Success();

            var combinedErrors = new List<string>();
            if (IsFailure) combinedErrors.AddRange(Errors);
            if (other.IsFailure) combinedErrors.AddRange(other.Errors);

            return Failure(combinedErrors);
        }
    }

    /// <summary>
    /// Result for operations that return data with validation
    /// </summary>
    /// <typeparam name="T">The type of data returned</typeparam>
    public class DataResult<T> : Result<T>
    {
        public IReadOnlyList<string> Warnings { get; }
        public IReadOnlyDictionary<string, object> Metadata { get; }

        internal DataResult(T value, bool isSuccess, IEnumerable<string> warnings = null, 
            IDictionary<string, object> metadata = null, string errorMessage = null, 
            string errorCode = null, Exception exception = null)
            : base(value, isSuccess, errorMessage, errorCode, exception)
        {
            Warnings = warnings?.ToList().AsReadOnly() ?? new List<string>().AsReadOnly();
            Metadata = metadata?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)?.AsReadOnly() ?? 
                       new Dictionary<string, object>().AsReadOnly();
        }

        public static DataResult<T> Success(T value, IEnumerable<string> warnings = null, 
            IDictionary<string, object> metadata = null)
        {
            return new DataResult<T>(value, true, warnings, metadata);
        }

        public static new DataResult<T> Failure(string errorMessage, string errorCode = null, 
            Exception exception = null, IDictionary<string, object> metadata = null)
        {
            return new DataResult<T>(default(T), false, null, metadata, errorMessage, errorCode, exception);
        }
    }

    /// <summary>
    /// Extensions for working with Result objects
    /// </summary>
    public static class ResultExtensions
    {
        public static Result<T> ToResult<T>(this T value)
        {
            return Result.Success(value);
        }

        public static Result Combine(this IEnumerable<Result> results)
        {
            var resultList = results.ToList();
            if (resultList.All(r => r.IsSuccess))
                return Result.Success();

            var errors = resultList.Where(r => r.IsFailure).Select(r => r.ErrorMessage).ToList();
            return Result.Failure(string.Join("; ", errors));
        }

        public static Result<IEnumerable<T>> Combine<T>(this IEnumerable<Result<T>> results)
        {
            var resultList = results.ToList();
            if (resultList.All(r => r.IsSuccess))
                return Result.Success(resultList.Select(r => r.Value));

            var errors = resultList.Where(r => r.IsFailure).Select(r => r.ErrorMessage).ToList();
            return Result.Failure<IEnumerable<T>>(string.Join("; ", errors));
        }

        public static async Task<Result<T>> TryCatchAsync<T>(Func<Task<T>> operation, 
            Func<Exception, string> errorMessageSelector = null, string errorCode = null)
        {
            try
            {
                var value = await operation();
                return Result.Success(value);
            }
            catch (Exception ex)
            {
                var errorMessage = errorMessageSelector?.Invoke(ex) ?? ex.Message;
                return Result.Failure<T>(errorMessage, errorCode, ex);
            }
        }

        public static Result<T> TryCatch<T>(Func<T> operation, 
            Func<Exception, string> errorMessageSelector = null, string errorCode = null)
        {
            try
            {
                var value = operation();
                return Result.Success(value);
            }
            catch (Exception ex)
            {
                var errorMessage = errorMessageSelector?.Invoke(ex) ?? ex.Message;
                return Result.Failure<T>(errorMessage, errorCode, ex);
            }
        }

        public static async Task<Result> TryCatchAsync(Func<Task> operation, 
            Func<Exception, string> errorMessageSelector = null, string errorCode = null)
        {
            try
            {
                await operation();
                return Result.Success();
            }
            catch (Exception ex)
            {
                var errorMessage = errorMessageSelector?.Invoke(ex) ?? ex.Message;
                return Result.Failure(errorMessage, errorCode, ex);
            }
        }

        public static Result TryCatch(Action operation, 
            Func<Exception, string> errorMessageSelector = null, string errorCode = null)
        {
            try
            {
                operation();
                return Result.Success();
            }
            catch (Exception ex)
            {
                var errorMessage = errorMessageSelector?.Invoke(ex) ?? ex.Message;
                return Result.Failure(errorMessage, errorCode, ex);
            }
        }
    }
}

namespace System.Collections.Generic
{
    public static class DictionaryExtensions
    {
        public static IReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            return new ReadOnlyDictionary<TKey, TValue>(dictionary);
        }
    }
}