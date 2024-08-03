using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Results
{
    public interface IResult
    {
        bool IsFailure { get; }
        bool IsSuccess { get; }
    }

    public interface IValue<out T>
    {
        T Value { get; }
    }

    public interface IError<out E>
    {
        E Error { get; }
    }

    public interface IResult<out T, out E> : IValue<T>, IUnitResult<E>
    {
    }

    public interface IResult<out T> : IResult<T, string>
    {
    }

    public interface IUnitResult<out E> : IResult, IError<E>
    {
    }

    public readonly partial struct Result : IResult, ISerializable
    {
        public bool IsFailure { get; }
        public bool IsSuccess => !IsFailure;

        private readonly string _error;
        public string Error => ResultCommonLogic.GetErrorWithSuccessGuard(IsFailure, _error);

        private Result(bool isFailure, string error)
        {
            IsFailure = ResultCommonLogic.ErrorStateGuard(isFailure, error);
            _error = error;
        }

        private Result(SerializationInfo info, StreamingContext context)
        {
            SerializationValue<string> values = ResultCommonLogic.Deserialize(info);
            IsFailure = values.IsFailure;
            _error = values.Error;
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ResultCommonLogic.GetObjectData(this, info);
        }
        
        public static implicit operator UnitResult<string>(Result result)
        {
            if (result.IsSuccess)
                return UnitResult.Success<string>();
            else
                return UnitResult.Failure(result.Error);
        }
    }

    public readonly partial struct Result<T> : IResult<T>, ISerializable
    {
        public bool IsFailure { get; }
        public bool IsSuccess => !IsFailure;

        private readonly string _error;
        public string Error => ResultCommonLogic.GetErrorWithSuccessGuard(IsFailure, _error);

        private readonly T _value;
        public T Value => IsSuccess ? _value : throw new ResultFailureException(Error);

        internal Result(bool isFailure, string error, T value)
        {
            IsFailure = ResultCommonLogic.ErrorStateGuard(isFailure, error);
            _error = error;
            _value = value;
        }

        private Result(SerializationInfo info, StreamingContext context)
        {
            SerializationValue<string> values = ResultCommonLogic.Deserialize(info);
            IsFailure = values.IsFailure;
            _error = values.Error;
            _value = IsFailure ? default : (T)info.GetValue("Value", typeof(T));
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ResultCommonLogic.GetObjectData(this, info);
        }

        public T GetValueOrDefault(T defaultValue = default)
        {
            if (IsFailure)
                return defaultValue;

            return Value;
        }

        public static implicit operator Result<T>(T value)
        {
            if (value is IResult<T> result)
            {
                string resultError = result.IsFailure ? result.Error : default;
                T resultValue = result.IsSuccess ? result.Value : default;

                return new Result<T>(result.IsFailure, resultError, resultValue);
            }

            return Result.Success(value);
        }

        public static implicit operator Result(Result<T> result)
        {
            if (result.IsSuccess)
                return Result.Success();
            else
                return Result.Failure(result.Error);
        }

        public static implicit operator UnitResult<string>(Result<T> result)
        {
            if (result.IsSuccess)
                return UnitResult.Success<string>();
            else
                return UnitResult.Failure(result.Error);
        }
    }


    public readonly partial struct Result<T, E> : IResult<T, E>, ISerializable
    {
        public bool IsFailure { get; }
        public bool IsSuccess => !IsFailure;

        private readonly E _error;
        public E Error => ResultCommonLogic.GetErrorWithSuccessGuard(IsFailure, _error);

        private readonly T _value;
        public T Value => IsSuccess ? _value : throw new ResultFailureException<E>(Error);

        internal Result(bool isFailure, E error, T value)
        {
            IsFailure = ResultCommonLogic.ErrorStateGuard(isFailure, error);
            _error = error;
            _value = value;
        }

        private Result(SerializationInfo info, StreamingContext context)
        {
            SerializationValue<E> values = ResultCommonLogic.Deserialize<E>(info);
            IsFailure = values.IsFailure;
            _error = values.Error;
            _value = IsFailure ? default : (T)info.GetValue("Value", typeof(T));
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ResultCommonLogic.GetObjectData(this, info);
        }

        public static implicit operator Result<T, E>(T value)
        {
            if (value is IResult<T, E> result)
            {
                E resultError = result.IsFailure ? result.Error : default;
                T resultValue = result.IsSuccess ? result.Value : default;

                return new Result<T, E>(result.IsFailure, resultError, resultValue);
            }

            return Result.Success<T, E>(value);
        }

        public static implicit operator Result<T, E>(E error)
        {
            if (error is IResult<T, E> result)
            {
                E resultError = result.IsFailure ? result.Error : default;
                T resultValue = result.IsSuccess ? result.Value : default;

                return new Result<T, E>(result.IsFailure, resultError, resultValue);
            }

            return Result.Failure<T, E>(error);
        }

        public static implicit operator UnitResult<E>(Result<T, E> result)
        {
            if (result.IsSuccess)
                return UnitResult.Success<E>();
            else
                return UnitResult.Failure(result.Error);
        }
    }

}
