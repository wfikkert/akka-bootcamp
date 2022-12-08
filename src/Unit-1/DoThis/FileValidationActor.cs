using Akka.Actor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace WinTail
{
    public class FileValidationActor : UntypedActor
    {

        private readonly IActorRef _consoleWriterActor;

        public FileValidationActor(IActorRef consoleWriterActor)
        {
            _consoleWriterActor = consoleWriterActor;
        }

        protected override void OnReceive(object message)
        {
            var msg = message as string;
            if (string.IsNullOrEmpty(msg))
            {
                // signal that the user needs to supply an input
                _consoleWriterActor.Tell(new Messages.NullInputError("Input was blank. Please try again.\n"));

                // tell sender to continue doing its thing 
                Sender.Tell(new Messages.ContinueProcessing());
            }
            else
            {
                var valid = IsFileUri(msg);
                if (valid)
                {
                    //signal that input was valid
                    _consoleWriterActor.Tell(new Messages.InputSuccess(string.Format($"Starting processing for {msg}")));

                    //start coordinator
                    Context.ActorSelection("akka://MyActorSystem/user/tailCoordinatorActor").Tell(new TailCoordinatorActor.StartTail(msg, _consoleWriterActor));
                }
                else
                {
                    //signal that input was invalid
                    _consoleWriterActor.Tell(new Messages.ValidationError(string.Format($"{msg} is not an exisiting URI on disk")));

                    //tell sender to continue process
                    Sender.Tell(new Messages.ContinueProcessing());
                }
            }
        }

        private static bool IsFileUri(string path)
        {
            return File.Exists(path);
        }
    }
}
