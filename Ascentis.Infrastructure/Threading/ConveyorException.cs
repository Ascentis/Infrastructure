using System;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class ConveyorException : AggregateException
    {
        public ConveyorException(Exception innerException) : base(innerException) {}
    }
}

