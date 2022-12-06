using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Text;

namespace WinTail
{
    public class ValidationActor : UntypedActor
    {
        private readonly IActorRef _consoleWriterActor;
        public ValidationActor(IActorRef consoleWriterActor)
        {
            _consoleWriterActor = consoleWriterActor;
        }
        protected override void OnReceive(object message)
        {
            var msg = message as string;
            if (String.IsNullOrEmpty(msg))
            {
                //signal that the user needs to supply an input, as previously received input was blank
                _consoleWriterActor.Tell(new Messages.NullInputError("No input received"));
            }
            else
            {
                var valid = IsValid(msg);
                if (valid)
                {
                    //signal that input was valid
                    _consoleWriterActor.Tell(new Messages.InputSuccess("Thank you! Message was valid."));
                }
                else
                {
                    //signal that input was not valid
                    _consoleWriterActor.Tell(new Messages.ValidationError("Invalid: input had odd numbers of characters."));
                }
            }

            //tell sender to continue the process
            Sender.Tell(new Messages.ContinueProcessing());
        }

        /// <summary>
        /// Validates the message.
        /// Currently says messages are valid if contain even number of characters.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private static bool IsValid(string message)
        {
            var valid = message.Length % 2 == 0;
            return valid;
        }
    }
}
