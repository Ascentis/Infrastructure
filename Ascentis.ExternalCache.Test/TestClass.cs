﻿using System;

namespace Ascentis.Infrastructure.Test
{
    // ReSharper disable once InconsistentNaming
    public class TestClass : IDisposable
    {
        public string Prop1 { get; set; }
        public string Prop2 { get; set; }
        public string Prop3 { get; set; }
        public int Prop4 { get; set; }
        public void Dispose()
        {
            Console.WriteLine("Disposing");
        }
    }
}
