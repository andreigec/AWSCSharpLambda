using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization;
using csharplambda.Helpers;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializerAttribute(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace csharplambda
{
    public class CSharpLambdaFunction
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task CSharpLambdaFunctionHandler(ILambdaContext context)
        {
            //hook into the lambda logging with an nlog stub class
            //ideally would just use nlog, but this has less packages/overhead :)
            csharplambda.Helpers.Logger.context = context;
            var l = LogManager.GetLogger("csharp test function");
            l.Info("Hello");

            var result = await SQSQueue.Read("sqs.../csharpLambdaSQS");
            var resulttext = result.GetData<string>();
            l.Info($"input is {resulttext}, transform is {resulttext.ToUpper()}");
            //will remove item from queue
            await result.Accept();
        }
    }
}
