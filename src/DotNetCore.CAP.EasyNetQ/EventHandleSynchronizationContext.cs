using System;
using System.Threading;

namespace DotNetCore.CAP.EasyNetQ
{
    public class EventHandleSynchronizationContext : SynchronizationContext
    {
        private readonly Action completed;

        public EventHandleSynchronizationContext(
          Action completed)
        {
            this.completed = completed;
        }

        public override SynchronizationContext CreateCopy()
        {
            return new EventHandleSynchronizationContext(
              this.completed);
        }

        public override void OperationStarted()
        {
            Console.WriteLine("SynchronizationContext: Started");
        }

        public override void OperationCompleted()
        {
            Console.WriteLine("SynchronizationContext: Completed");
            this.completed();
        }
    }
}