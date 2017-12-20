using Proto;
using Proto.Mailbox;
using System;

namespace HelloProto
{
    class Program
    {
        static void Main(string[] args)
        {
            //SimpleActor();
            //AnonymousActor();
            HookedActor();

            Console.ReadKey();
        }

        #region SimpleActor

        private static void SimpleActor()
        {
            // The Props are used to configure and construct the Actor and it's Context.
            Props props = Actor.FromProducer(() => new HelloActor());
            PID pid = Actor.Spawn(props);
            pid.Tell(new Hello
            {
                Who = "Alex"
            });
        }

        #endregion // SimpleActor

        #region AnonymousActor

        private static void AnonymousActor()
        {
            var anonymousActorProps = Actor.FromFunc(context =>
            {
                var msg = context.Message;
                if (msg is string s)
                {
                    Console.WriteLine($"Hello anonymous {s}");
                }
                return Actor.Done;
            });
            PID anonymousActorPid = Actor.Spawn(anonymousActorProps);
            anonymousActorPid.Tell("Hi");
        }

        #endregion // AnonymousActor

        #region HookedActor

        private static void HookedActor()
        {
            // The Props are used to configure and construct the Actor and it's Context.
            Props props = new Props()
               // the producer is a delegate that returns a new instance of an IActor
               .WithProducer(() => new HelloActor())
               // the default dispatcher uses the thread pool and limits throughput to 300 messages per mailbox run
               .WithDispatcher(new ThreadPoolDispatcher { Throughput = 300 })
               // the default mailbox uses unbounded queues
               .WithMailbox(() => UnboundedMailbox.Create())
               // the default strategy restarts child actors a maximum of 10 times within a 10 second window
               .WithChildSupervisorStrategy(new OneForOneStrategy((who, reason) => 
                        SupervisorDirective.Restart, 10, TimeSpan.FromSeconds(10)))
               // middlewares can be chained to intercept incoming and outgoing messages
               // receive middlewares are invoked before the actor receives the message
               // sender middlewares are invoked before the message is sent to the target PID
               .WithReceiveMiddleware(
                   next => async c =>
                   {
                       Console.WriteLine($"RECEIVING {c.Message.GetType()}:{c.Message}");
                       await next(c);
                       Console.WriteLine($"RECEIVED");
                   },
                   next => async c =>
                   {
                       Console.WriteLine($"GETTING {c.Message.GetType()}:{c.Message}");
                       await next(c);
                       Console.WriteLine($"GOTTEN");
                   })
               .WithSenderMiddleware(
                   next => async (c, target, envelope) =>
                   {
                       Console.WriteLine($"SENDING  {c.Message.GetType()}:{c.Message}");
                       await next(c, target, envelope);
                       Console.WriteLine($"SENT     {c.Message.GetType()}:{c.Message}");
                   },
                   next => async (c, target, envelope) =>
                   {
                       Console.WriteLine($"POSTING  {c.Message.GetType()}:{c.Message}");
                       await next(c, target, envelope);
                       Console.WriteLine($"POSTED   {c.Message.GetType()}:{c.Message}");
                   })
               // the default spawner constructs the Actor, Context and Process
               .WithSpawner(Props.DefaultSpawner);

            PID pid = Actor.Spawn(props);
            pid.Tell(new Hello
            {
                Who = "Bnaya"
            });
            pid.Tell(new Hello
            {
                Who = "Bnaya"
            });
        }

        #endregion // HookedActor
    }
}
