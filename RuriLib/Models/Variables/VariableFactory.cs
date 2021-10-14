using System;
using System.Collections.Generic;

namespace RuriLib.Models.Variables
{
    public class VariableFactory
    {
        public static Variable FromObject(object obj) => obj switch
        {
            bool x => new BoolVariable(x),
            byte[] x => new ByteArrayVariable(x),
            Dictionary<string, string> x => new DictionaryOfStringsVariable(x),
            float x => new FloatVariable(x),
            int x => new IntVariable(x),
            List<string> x => new ListOfStringsVariable(x),
            string x => new StringVariable(x),
            _ => throw new NotSupportedException("Type: " + obj.GetType().FullName)
        };
    }
}
