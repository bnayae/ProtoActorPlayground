using Proto;
using Proto.Mailbox;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

// Pattern matching
// https://docs.microsoft.com/en-us/dotnet/csharp/pattern-matching

namespace HelloProto
{
    class SupervisorActor : IActor
    {
        #region private static readonly Props PROPS = new Props()

        private static readonly Props PROPS = new Props()
             // the producer is a delegate that returns a new instance of an IActor
             .WithProducer(() => new SupervisedActor())
            .WithReceiveMiddleware(next => async c => 
            {
                Console.Write(".");
                await next(c);
            }) 
            .WithSenderMiddleware(
                 next => async (c, target, envelope) =>
                 {
                     Console.WriteLine($"{envelope.Sender.Id} -> {target.Id}: {c.Message}");
                     await next(c, target, envelope);
                 });

        #endregion // private static readonly Props PROPS = new Props()

        public Task ReceiveAsync(IContext context)
        {
            // Pattern Matching
            switch (context.Message)
            {
                case string data when data.Length < 10:
                    {
                        PID pid = context.SpawnPrefix(PROPS, $"child_");
                        pid.Tell(new string(data.Reverse().ToArray()));
                        break;
                    }
                case string data:
                    {
                        PID pid = context.SpawnPrefix(PROPS, $"child_");
                        pid.Tell(data);
                        break;
                    }
                case int data:
                    {
                        PID pid = context.SpawnPrefix(PROPS, $"child_");
                        pid.Tell(data);
                        break;
                    }
                default:
                    {
                        Console.WriteLine($"\r\n{context.Message}");
                        break;
                    }
            }
            return Actor.Done;
        }
    }
}
