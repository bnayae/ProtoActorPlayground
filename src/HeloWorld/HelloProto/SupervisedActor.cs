using Proto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HelloProto
{
    class SupervisedActor : IActor
    {
        public Task ReceiveAsync(IContext context)
        {
            // Pattern Matching
            switch (context.Message)
            {
                case string data:
                    return ReceiveAsync(data);
                case int data:
                    return ReceiveAsync(data);
                case Proto.Started data:
                    Console.WriteLine("Started");
                    return Actor.Done;
                case Proto.Restart data:
                    Console.WriteLine("Restart");
                    return Actor.Done;
                case Proto.Restarting data:
                    Console.WriteLine("Restarting");
                    return Actor.Done;
                case Proto.Stopped data:
                    Console.WriteLine("Stopped");
                    return Actor.Done;
                default:
                    throw new NotImplementedException();
            }
        }

        public Task ReceiveAsync(string data)
        {
            Console.WriteLine(data);
            return Actor.Done;
        }

        public Task ReceiveAsync(int data)
        {
            if (data < 0)
                throw new ArgumentOutOfRangeException("Negative value not allowed");
            Console.WriteLine(data);
            return Actor.Done;
        }
    }
}
