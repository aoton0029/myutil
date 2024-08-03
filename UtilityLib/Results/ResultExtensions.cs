using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Results
{
    public static partial class ResultExtensions
    {
        public static T Finally<T>(this Result result, Func<Result, T> func)
            => func(result);

        public static K Finally<T, K>(this Result<T> result, Func<Result<T>, K> func)
            => func(result);

        public static K Finally<K, E>(this UnitResult<E> result, Func<UnitResult<E>, K> func)
            => func(result);

        public static K Finally<T, K, E>(this Result<T, E> result, Func<Result<T, E>, K> func)
            => func(result);

        public static T OnBoth<T>(this Result result, Func<Result, T> func)
            => Finally(result, func);

        public static K OnBoth<T, K>(this Result<T> result, Func<Result<T>, K> func)
            => Finally(result, func);

        public static K OnBoth<T, K, E>(this Result<T, E> result, Func<Result<T, E>, K> func)
            => Finally(result, func);

        public static K Match<T, K, E>(this Result<T, E> result, Func<T, K> onSuccess, Func<E, K> onFailure)
        {
            return result.IsSuccess
                ? onSuccess(result.Value)
                : onFailure(result.Error);
        }

        public static K Match<K, T>(this Result<T> result, Func<T, K> onSuccess, Func<string, K> onFailure)
        {
            return result.IsSuccess
                ? onSuccess(result.Value)
                : onFailure(result.Error);
        }

        public static T Match<T>(this Result result, Func<T> onSuccess, Func<string, T> onFailure)
        {
            return result.IsSuccess
                ? onSuccess()
                : onFailure(result.Error);
        }

        public static K Match<K, E>(this UnitResult<E> result, Func<K> onSuccess, Func<E, K> onFailure)
        {
            return result.IsSuccess
                ? onSuccess()
                : onFailure(result.Error);
        }

        public static void Match<T, E>(this Result<T, E> result, Action<T> onSuccess, Action<E> onFailure)
        {
            if (result.IsSuccess)
                onSuccess(result.Value);
            else
                onFailure(result.Error);
        }

        public static void Match<E>(this UnitResult<E> result, Action onSuccess, Action<E> onFailure)
        {
            if (result.IsSuccess)
                onSuccess();
            else
                onFailure(result.Error);
        }

        public static void Match<T>(this Result<T> result, Action<T> onSuccess, Action<string> onFailure)
        {
            if (result.IsSuccess)
                onSuccess(result.Value);
            else
                onFailure(result.Error);
        }

        public static void Match(this Result result, Action onSuccess, Action<string> onFailure)
        {
            if (result.IsSuccess)
                onSuccess();
            else
                onFailure(result.Error);
        }

        public static Result<K, E> Map<T, K, E>(this Result<T, E> result, Func<T, K> func)
        {
            if (result.IsFailure)
                return Result.Failure<K, E>(result.Error);

            return Result.Success<K, E>(func(result.Value));
        }

        public static Result<K, E> Map<K, E>(this UnitResult<E> result, Func<K> func)
        {
            if (result.IsFailure)
                return Result.Failure<K, E>(result.Error);

            return Result.Success<K, E>(func());
        }

        public static Result<K> Map<T, K>(this Result<T> result, Func<T, K> func)
        {
            if (result.IsFailure)
                return Result.Failure<K>(result.Error);

            return Result.Success(func(result.Value));
        }

        public static Result<K> Map<K>(this Result result, Func<K> func)
        {
            if (result.IsFailure)
                return Result.Failure<K>(result.Error);

            return Result.Success(func());
        }

        public static Result<T> Check<T>(this Result<T> result, Func<T, Result> func)
        {
            return result.Bind(func).Map(() => result.Value);
        }

        public static Result<T> Check<T, K>(this Result<T> result, Func<T, Result<K>> func)
        {
            return result.Bind(func).Map(_ => result.Value);
        }

        public static Result<T, E> Check<T, K, E>(this Result<T, E> result, Func<T, Result<K, E>> func)
        {
            return result.Bind(func).Map(_ => result.Value);
        }

        public static Result<T, E> Check<T, E>(this Result<T, E> result, Func<T, UnitResult<E>> func)
        {
            return result.Bind(func).Map(() => result.Value);
        }

        public static UnitResult<E> Check<E>(this UnitResult<E> result, Func<UnitResult<E>> func)
        {
            return result.Bind(func).Map(() => result);
        }

        public static Result<K, E> Bind<T, K, E>(this Result<T, E> result, Func<T, Result<K, E>> func)
        {
            if (result.IsFailure)
                return Result.Failure<K, E>(result.Error);

            return func(result.Value);
        }

        public static Result<K> Bind<T, K>(this Result<T> result, Func<T, Result<K>> func)
        {
            if (result.IsFailure)
                return Result.Failure<K>(result.Error);

            return func(result.Value);
        }

        public static Result<K> Bind<K>(this Result result, Func<Result<K>> func)
        {
            if (result.IsFailure)
                return Result.Failure<K>(result.Error);

            return func();
        }

        public static Result Bind<T>(this Result<T> result, Func<T, Result> func)
        {
            if (result.IsFailure)
                return result;

            return func(result.Value);
        }

        public static Result Bind(this Result result, Func<Result> func)
        {
            if (result.IsFailure)
                return result;

            return func();
        }

        public static UnitResult<E> Bind<E>(this UnitResult<E> result, Func<UnitResult<E>> func)
        {
            if (result.IsFailure)
                return result.Error;

            return func();
        }

        public static Result<T, E> Bind<T, E>(this UnitResult<E> result, Func<Result<T, E>> func)
        {
            if (result.IsFailure)
                return result.Error;

            return func();
        }

        public static UnitResult<E> Bind<T, E>(this Result<T, E> result, Func<T, UnitResult<E>> func)
        {
            if (result.IsFailure)
                return result.Error;

            return func(result.Value);
        }

    }

}
