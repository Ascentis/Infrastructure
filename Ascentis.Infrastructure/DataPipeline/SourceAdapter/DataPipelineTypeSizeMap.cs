using System;
using System.Collections.Generic;

namespace Ascentis.Infrastructure.DataPipeline.SourceAdapter
{
    public class DataPipelineTypeSizeMap
    {
        public static Dictionary<Type, int> Map { get; }

        static DataPipelineTypeSizeMap()
        {
            Map = new Dictionary<Type, int>
            {
                {typeof(char), 1},
                {typeof(byte), 3},
                {typeof(bool), 5},
                {typeof(ushort), 5},
                {typeof(short), 6},
                {typeof(uint), 13},
                {typeof(int), 14},
                {typeof(string), 16}
            };
        }
    }
}
