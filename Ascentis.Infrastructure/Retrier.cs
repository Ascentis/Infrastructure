using System;

namespace Ascentis.Infrastructure
{
    public class Retrier<T>
    {
        public delegate void RetriableProcedureDelegate(T reference);
        public delegate TFnRetType RetriableFunctionDelegate<out TFnRetType>(T reference);
        public delegate bool CanRetry(Exception e, int retries);

        private readonly CanRetry _canRetry;

        public T Reference { get; }

        private static bool DefaultCanRetry(Exception e, int retries)
        {
            return retries == 0;
        }

        public Retrier(T reference)
        {
            Reference = reference;
            _canRetry = DefaultCanRetry;
        }

        public Retrier(T reference, CanRetry canRetry)
        {
            Reference = reference;
            _canRetry = canRetry;
        }

        public TFnRetType Retriable<TFnRetType>(RetriableFunctionDelegate<TFnRetType> functionDelegate, int initialRetryCount = 0)
        {
            var retries = initialRetryCount;
            while (true)
            {
                try
                {
                    return functionDelegate(Reference);
                }
                catch (Exception e)
                {
                    if (!_canRetry(e, retries++))
                        throw;
                }
            }
        }

        public void Retriable(RetriableProcedureDelegate procedureDelegate, int initialRetryCount = 0)
        {
            var retries = initialRetryCount;
            while (true)
            {
                try
                {
                    procedureDelegate(Reference);
                    break;
                }
                catch (Exception e)
                {
                    if (!_canRetry(e, retries++))
                        throw;
                }
            }
        }
    }
}
