﻿using System;
﻿using Akka.Actor;

namespace WinTail
{
    #region Program
    class Program
    {
        public static ActorSystem MyActorSystem;

        static void Main(string[] args)
        {
            // initialize MyActorSystem
            MyActorSystem = ActorSystem.Create("MyActorSystem");

            // Create writer and reader actors
            Props consoleWriterProps = Props.Create<ConsoleWriterActor>();
            IActorRef consoleWriterActor = MyActorSystem.ActorOf(consoleWriterProps, "ConsoleWriter");

            Props validationProps = Props.Create<ValidationActor>(consoleWriterActor);
            IActorRef validationActor = MyActorSystem.ActorOf(validationProps, "Validation");

            Props consoleReaderProps = Props.Create(()=> new ConsoleReaderActor(validationActor));
            IActorRef consoleReaderActor = MyActorSystem.ActorOf(consoleReaderProps, "ConsoleReader");

            

            // tell console reader to begin
            consoleReaderActor.Tell(ConsoleReaderActor.StartCommand);

            // blocks the main thread from exiting until the actor system is shut down
            MyActorSystem.WhenTerminated.Wait();
        }

    }
    #endregion
}
