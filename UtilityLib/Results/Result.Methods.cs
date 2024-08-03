using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Results
{
    public partial struct Result
    {
        /// <summary>
        ///     Creates a success result.
        /// </summary>
        public static Result Success()
        {
            return new Result(false, default);
        }

        /// <summary>
        ///     Creates a success result containing the given value.
        /// </summary>
        public static Result<T> Success<T>(T value)
        {
            return new Result<T>(false, default, value);
        }

        /// <summary>
        ///     Creates a success result containing the given value.
        /// </summary>
        public static Result<T, E> Success<T, E>(T value)
        {
            return new Result<T, E>(false, default, value);
        }

        /// <summary>
        ///     Creates a success result containing the given error.
        /// </summary>
        public static UnitResult<E> Success<E>()
        {
            return new UnitResult<E>(false, default);
        }

        /// <summary>
        ///     Creates a failure result with the given error message.
        /// </summary>
        public static Result Failure(string error)
        {
            return new Result(true, error);
        }

        /// <summary>
        ///     Creates a failure result with the given error message.
        /// </summary>
        public static Result<T> Failure<T>(string error)
        {
            return new Result<T>(true, error, default);
        }

        /// <summary>
        ///     Creates a failure result with the given error.
        /// </summary>
        public static Result<T, E> Failure<T, E>(E error)
        {
            return new Result<T, E>(true, error, default);
        }

        /// <summary>
        ///     Creates a result whose success/failure reflects the supplied condition. Opposite of FailureIf().
        /// </summary>
        public static Result SuccessIf(bool isSuccess, string error)
        {
            return isSuccess
                ? Success()
                : Failure(error);
        }

        /// <summary>
        ///     Creates a result whose success/failure depends on the supplied predicate. Opposite of FailureIf().
        /// </summary>
        public static Result SuccessIf(Func<bool> predicate, string error)
        {
            return SuccessIf(predicate(), error);
        }

        /// <summary>
        ///     Creates a result whose success/failure reflects the supplied condition. Opposite of FailureIf().
        /// </summary>
        public static Result<T> SuccessIf<T>(bool isSuccess, in T value, string error)
        {
            return isSuccess
                ? Success(value)
                : Failure<T>(error);
        }

        /// <summary>
        ///     Creates a result whose success/failure depends on the supplied predicate. Opposite of FailureIf().
        /// </summary>
        public static Result<T> SuccessIf<T>(Func<bool> predicate, in T value, string error)
        {
            return SuccessIf(predicate(), value, error);
        }

        /// <summary>
        ///     Creates a result whose success/failure reflects the supplied condition. Opposite of FailureIf().
        /// </summary>
        public static Result<T, E> SuccessIf<T, E>(bool isSuccess, in T value, in E error)
        {
            return isSuccess
                ? Success<T, E>(value)
                : Failure<T, E>(error);
        }

        /// <summary>
        ///     Creates a result whose success/failure depends on the supplied predicate. Opposite of FailureIf().
        /// </summary>
        public static Result<T, E> SuccessIf<T, E>(Func<bool> predicate, in T value, in E error)
        {
            return SuccessIf(predicate(), value, error);
        }

        public static Result FirstFailureOrSuccess(params Result[] results)
        {
            foreach (Result result in results)
            {
                if (result.IsFailure)
                    return result;
            }

            return Success();
        }
    }

}
