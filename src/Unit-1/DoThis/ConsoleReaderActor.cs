using System;
using Akka.Actor;

namespace WinTail
{
    /// <summary>
    /// Actor responsible for reading FROM the console. 
    /// Also responsible for calling <see cref="ActorSystem.Terminate"/>.
    /// </summary>
    class ConsoleReaderActor : UntypedActor
    {
        public const string ExitCommand = "exit";
        public const string StartCommand = "start";

        protected override void OnReceive(object message)
        {
            if (message.Equals(StartCommand))
            {
                DoPrintInstructions();
            }

            GetAndValidateInput();
        }

        #region Internal methods
        private void DoPrintInstructions()
        {
            Console.WriteLine("Please provide the URI of a log file on disk.\n");
        }

        /// <summary>
        /// Reads input from console, validates it, then signals appropriate response
        /// (continue processing, error, success etc.).
        /// </summary>
        private void GetAndValidateInput()
        {
            var message = Console.ReadLine();
            if (String.Equals(message, ExitCommand, StringComparison.OrdinalIgnoreCase))
            {
                //if user types exit command shut down the entire actor system
                Context.System.Terminate();
                return;
            }

            //if the system does not need to shutdown tell the message to the validation actor
            Context.ActorSelection("akka://MyActorSystem/user/validationActor").Tell(message);
        }

        #endregion

    }
}