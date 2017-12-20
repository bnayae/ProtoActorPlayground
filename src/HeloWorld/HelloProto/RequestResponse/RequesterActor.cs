using Proto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HelloProto
{
    class RequesterActor : IActor
    {
        public async Task ReceiveAsync(IContext context)
        {
            if (!(context.Message is string))
                return;
            var prop = new Props().WithProducer(() => new ResponderActor());
            PID pid = context.SpawnPrefix(prop, "child_");
            string data = await pid.RequestAsync<string>(context.Message, TimeSpan.FromSeconds(15));
            Console.WriteLine($"Respond: {data}");
        }
    }
}
