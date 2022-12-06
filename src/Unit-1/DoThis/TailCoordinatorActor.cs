using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Text;

namespace WinTail
{
    public class TailCoordinatorActor : UntypedActor
    {

        #region Message types

        /// <summary>
        /// Start tailing the file at user-specified file
        /// </summary>
        public class StartTail
        {
            public string FilePath { get; set; }
            public IActorRef ReporterActor { get; set; }

            public StartTail(string filePath, IActorRef reporterActor)
            {
                FilePath = filePath;
                ReporterActor = reporterActor;
            }
        }

        /// <summary>
        /// Stop tailing the file at user-specified path
        /// </summary>
        public class StopTail
        {
            public string FilePath { get; set; }

            public StopTail(string filePath)
            {
                FilePath = filePath;
            }
        }

        #endregion

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(10, TimeSpan.FromSeconds(30), x =>
            {
                if (x is ArithmeticException) return Directive.Resume;
                else if (x is NotSupportedException) return Directive.Stop;
                else return Directive.Restart;
            });
        }

        protected override void OnReceive(object message)
        {
            if(message is StartTail)
            {
                var msg = message as StartTail;

                //Creating child actor TailActor
                Context.ActorOf(Props.Create<TailActor>(msg.ReporterActor, msg.FilePath));
            }
        }
    }
}
