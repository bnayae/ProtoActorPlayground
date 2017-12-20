using Proto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HelloProto
{
    class HelloActor : IActor
    {
        public Task ReceiveAsync(IContext context)
        {
            var msg = context.Message;
            if (msg is Hello r)
            {
                Console.WriteLine($"Hello {r.Who}, on {context.Self.Id}");
            }
            return Actor.Done;
        }
    }
}
