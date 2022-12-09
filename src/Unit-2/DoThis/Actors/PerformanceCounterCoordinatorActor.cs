using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms.DataVisualization.Charting;

namespace ChartApp.Actors
{
    public class PerformanceCounterCoordinatorActor : ReceiveActor
    {
        #region MessageTypes

        /// <summary>
        /// Subscribe the charting actor to updates for the counter
        /// </summary>
        public class Watch
        {
            public CounterType Counter { get; set; }

            public Watch(CounterType counter)
            {
                Counter = counter;
            }
        }

        /// <summary>
        /// Unsubscribe the charting actor to updates for the counter
        /// </summary>
        public class Unwatch
        {
            public CounterType Counter { get; set; }

            public Unwatch(CounterType counter)
            {
                Counter = counter;
            }
        }
        #endregion

        private static readonly Dictionary<CounterType, Func<PerformanceCounter>> CounterGenerators = new Dictionary<CounterType, Func<PerformanceCounter>>()
        {
            {
                CounterType.Cpu, () => new PerformanceCounter("Processor", "% Processor Time", "_Total", true)
            },
            {
                CounterType.Memory, () => new PerformanceCounter("Memory", "% Committed Bytes In Use", true)
            },
            {
                CounterType.Disk, () => new PerformanceCounter("LogicalDisk", "% Disk Time", "_Total", true)
            },
        };

        private static readonly Dictionary<CounterType, Func<Series>> CounterSeries = new Dictionary<CounterType, Func<Series>>()
        {
            {
                CounterType.Cpu, () => new Series(CounterType.Cpu.ToString())
                {
                    ChartType = SeriesChartType.SplineArea,
                    Color = Color.DarkGreen
                }
            },
            {
                CounterType.Memory, () => new Series(CounterType.Memory.ToString())
                {
                    ChartType = SeriesChartType.FastLine,
                    Color = Color.MediumBlue
                }
            },
            {
                CounterType.Disk, () => new Series(CounterType.Disk.ToString())
                {
                    ChartType = SeriesChartType.SplineArea,
                    Color = Color.DarkRed
                }
            },
        };

        private Dictionary<CounterType, IActorRef> _counterActors;

        private IActorRef _chartingActor;

        public PerformanceCounterCoordinatorActor(IActorRef chartingActor) :  this(chartingActor, new Dictionary<CounterType, IActorRef>())
        {
        }

        public PerformanceCounterCoordinatorActor(IActorRef chartingActor, Dictionary<CounterType, IActorRef> counterActors)
        {
            _chartingActor = chartingActor;
            _counterActors = counterActors;

            Receive<Watch>(watch =>
            {
                if (!_counterActors.ContainsKey(watch.Counter))
                {
                    //Create a child actor to monitor this counter if one doesn't exist already
                    var counterActor = Context.ActorOf(Props.Create(() => new PerformanceCounterActor(watch.Counter.ToString(), CounterGenerators[watch.Counter])));
                    
                    //add this counter actor to our index
                    _counterActors[watch.Counter] = counterActor;
                }

                //register this series with the charting actor
                _chartingActor.Tell(new ChartingActor.AddSeries(CounterSeries[watch.Counter]()));

                //tell the counter actor to begin publishing its statistics to the charting actor
                _counterActors[watch.Counter].Tell(new SubscribeCounter(watch.Counter, _chartingActor));
            });

            Receive<Unwatch>(unwatch =>
            {
                //stop if there is no subscriber for this counter
                if (!_counterActors.ContainsKey(unwatch.Counter))
                {
                    return;
                }

                //unsubscribe the charting actor from receiving more updates
                _counterActors[unwatch.Counter].Tell(new UnsubscribeCounter(unwatch.Counter, _chartingActor));

                //remove this series from the charting actor
                _chartingActor.Tell(new ChartingActor.RemoveSeries(unwatch.Counter.ToString()));
            });
        }
    }
}
