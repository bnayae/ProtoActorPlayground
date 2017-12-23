using Proto;
using Proto.Mailbox;
using System;

namespace HelloProto
{
    class Program
    {
        #region private static readonly Props PROPS_SUPERVISOR = new Props()

        private static readonly Props PROPS_SUPERVISOR = new Props()
             // the producer is a delegate that returns a new instance of an IActor
             .WithProducer(() => new SupervisorActor())
             // the default strategy restarts child actors a maximum of 10 times within a 10 second window
             .WithChildSupervisorStrategy(
                        new OneForOneStrategy((who, reason) =>
                        {
                            Console.WriteLine($"\r\nERROR: {reason.GetBaseException().Message}, on {who.Id}");
                            return SupervisorDirective.Restart;
                        }, 10, TimeSpan.FromSeconds(10)));

        #endregion // private static readonly Props PROPS_SUPERVISOR = new Props()

        static void Main(string[] args)
        {
            //SimpleActor();
            //AnonymousActor();
            //HookedActor();
            //Supervisor();
            //RequestResponse();
            StateMachine();
            Console.ReadKey();
        }

        #region SimpleActor

        private static void SimpleActor()
        {
            // The Props are used to configure and construct the Actor and it's Context.
            Props props = Actor.FromProducer(() => new HelloActor());
            PID pid = Actor.Spawn(props); // create actor according to the properties definition
            Console.WriteLine($"{pid.Id} has created");
            PID pidPrefix = Actor.SpawnPrefix(props, "root_"); // create actor according to the properties definition
            Console.WriteLine($"{pidPrefix.Id} has created");
            PID pidNamed = Actor.SpawnNamed(props, "Singleton"); // create actor according to the properties definition
            Console.WriteLine($"{pidNamed.Id} has created");
            pid.Tell(new Hello { Who = "Alex" });
            pidPrefix.Tell(new Hello { Who = "Ben" });
            pidNamed.Tell(new Hello { Who = "Root" });
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
            PID anonymousActorPid = Actor.Spawn(anonymousActorProps); // create actor according to the properties definition
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

            PID pid = Actor.Spawn(props); // create actor according to the properties definition
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

        #region Supervisor

        private static void Supervisor()
        {
            // The Props are used to configure and construct the Actor and it's Context.
            Props props = PROPS_SUPERVISOR;// new Props().WithProducer(() => new SupervisorActor());

            PID pid = Actor.Spawn(props); // create actor according to the properties definition
            //pid.Tell("bnaya");
            //pid.Tell("Bnaya Eshet");
            for (int i = -12; i < 5; i++)
            {
                pid.Tell(i);
            }
        }

        #endregion // Supervisor

        #region RequestResponse

        private static void RequestResponse()
        {
            Props props = Actor.FromProducer(() => new RequesterActor());
            PID pid = Actor.Spawn(props); // create actor according to the properties definition
            pid.Tell("Hi");
        }

        #endregion // RequestResponse

        #region StateMachine

        private static void StateMachine()
        {
            Props props = Actor.FromProducer(() => new StateMachineActor());
            PID pid = Actor.Spawn(props); // create actor according to the properties definition
            var a = new StateA();
            var b = new StateB();
            var c = new StateC();
            var suspend = new Suspend();
            var resume = new Resume();
            pid.Tell(b); // A -> B
            pid.Tell(suspend);
            pid.Tell(c); // should be ignored
            pid.Tell(resume);
            pid.Tell(c); // B -> C
            pid.Tell(a); // C -> A
            pid.Tell(a); // A ignore
        }

        #endregion // StateMachine
    }
}
