﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Results
{
    internal struct SerializationValue<E>
    {
        public bool IsFailure { get; }
        public E Error { get; }

        internal SerializationValue(bool isFailure, E error)
        {
            IsFailure = isFailure;
            Error = error;
        }
    }

    internal static class ResultCommonLogic
    {
        internal static void GetObjectDataCommon(IResult result, SerializationInfo info)
        {
            info.AddValue("IsFailure", result.IsFailure);
            info.AddValue("IsSuccess", result.IsSuccess);
        }

        internal static void GetObjectData(Result result, SerializationInfo info)
        {
            GetObjectDataCommon(result, info);
            if (result.IsFailure)
            {
                info.AddValue("Error", result.Error);
            }
        }

        internal static void GetObjectData<T>(Result<T> result, SerializationInfo info)
        {
            GetObjectDataCommon(result, info);
            if (result.IsFailure)
            {
                info.AddValue("Error", result.Error);
            }

            if (result.IsSuccess)
            {
                info.AddValue("Value", result.Value);
            }
        }

        internal static void GetObjectData<T, E>(Result<T, E> result, SerializationInfo info)
        {
            GetObjectDataCommon(result, info);
            if (result.IsFailure)
            {
                info.AddValue("Error", result.Error);
            }

            if (result.IsSuccess)
            {
                info.AddValue("Value", result.Value);
            }
        }

        internal static void GetObjectData<E>(UnitResult<E> result, SerializationInfo info)
        {
            GetObjectDataCommon(result, info);
            if (result.IsFailure)
            {
                info.AddValue("Error", result.Error);
            }
        }

        internal static bool ErrorStateGuard<E>(bool isFailure, E error)
        {
            //if (isFailure)
            //{
            //    if (error == null || (error is string && error.Equals(string.Empty)))
            //        throw new ArgumentNullException(nameof(error), Result.Messages.ErrorObjectIsNotProvidedForFailure);
            //}
            //else
            //{
            //    if (!EqualityComparer<E>.Default.Equals(error, default))
            //        throw new ArgumentException(Result.Messages.ErrorObjectIsProvidedForSuccess, nameof(error));
            //}

            return isFailure;
        }

        internal static E GetErrorWithSuccessGuard<E>(bool isFailure, E error) =>
            isFailure ? error : throw new ResultSuccessException();

        internal static SerializationValue<string> Deserialize(SerializationInfo info) => Deserialize<string>(info);

        internal static SerializationValue<E> Deserialize<E>(SerializationInfo info)
        {
            bool isFailure = info.GetBoolean("IsFailure");
            E error = isFailure ? (E)info.GetValue("Error", typeof(E)) : default;
            return new SerializationValue<E>(isFailure, error);
        }
    }

    [Serializable]
    internal class ResultSuccessException : Exception
    {
        public ResultSuccessException()
        {
        }

        public ResultSuccessException(string? message) : base(message)
        {
        }

        public ResultSuccessException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
