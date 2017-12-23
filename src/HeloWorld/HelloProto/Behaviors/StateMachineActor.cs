using Proto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HelloProto
{
    class StateMachineActor : IActor
    {
        private readonly Behavior _behavior = new Behavior();

        #region Ctor

        public StateMachineActor()
        {
            _behavior.Become(StateAAsync);
        }

        #endregion // Ctor

        #region ReceiveAsync

        public Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case Suspend _:
                    _behavior.BecomeStacked(ResumeAsync);
                    Console.WriteLine("Stacked");
                    return Actor.Done;
            }
            return _behavior.ReceiveAsync(context);
        }

        #endregion // ReceiveAsync

        #region ResumeAsync

        /// <summary>
        /// Resumes the ignore anything but resume.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public Task ResumeAsync(IContext context)
        {
            switch (context.Message)
            {
                case Resume _:
                    Console.WriteLine($"Resumed");
                    _behavior.UnbecomeStacked();
                    break;
                default:
                    Console.WriteLine($"Resume ignore: {context.Message}");
                    break;
            }
            return Actor.Done;

        }

        #endregion // ResumeAsync

        #region StateAAsync

        public Task StateAAsync(IContext context)
        {
            switch (context.Message)
            {
                case StateB data:
                    Console.WriteLine("A -> B");
                    _behavior.Become(StateBAsync);
                    break;
                case StateC data:
                    _behavior.Become(StateCAsync);
                    Console.WriteLine("A -> C");
                    break;
                default:
                    Console.WriteLine($"A ignore: {context.Message}");
                    break;
            }
            return Actor.Done;
        }

        #endregion // StateAAsync

        #region StateBAsync

        public Task StateBAsync(IContext context)
        {
            switch (context.Message)
            {
                case StateA data:
                    Console.WriteLine("B -> A");
                    _behavior.Become(StateAAsync);
                    break;
                case StateC data:
                    Console.WriteLine("B -> C");
                    _behavior.Become(StateCAsync);
                    break;
                default:
                    Console.WriteLine($"B ignore: {context.Message}");
                    break;
            }
            return Actor.Done;
        }

        #endregion // StateBAsync

        #region StateCAsync

        public Task StateCAsync(IContext context)
        {
            switch (context.Message)
            {
                case StateA data:
                    Console.WriteLine("C -> A");
                    _behavior.Become(StateAAsync);
                    break;
                case StateB data:
                    Console.WriteLine("C -> B");
                    _behavior.Become(StateBAsync);
                    break;
                default:
                    Console.WriteLine($"C ignore: {context.Message}");
                    break;
            }
            return Actor.Done;
        }

        #endregion // StateCAsync
    }
}
