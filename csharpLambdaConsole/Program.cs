using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace csharpLambdaConsole
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var f = new csharplambda.CSharpLambdaFunction();
            var t = f.CSharpLambdaFunctionHandler(null);
            t.Wait();
            Console.WriteLine("execution completed");
            Console.ReadKey();
        }
    }
}
