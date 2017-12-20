using Proto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HelloProto
{
    class ResponderActor : IActor
    {
        public Task ReceiveAsync(IContext context)
        {
            if (context.Message is string data)
            {
                context.Respond($"Echo: {data}");
            }
            return Actor.Done;
        }
    }
}
