using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.DesignPatterns.Mediator
{
    abstract class Mediator
    {
        public abstract void Send(string message, Colleague colleague);
    }

    class ManagerMediator : Mediator
    {
        public Colleague Customer { get; set; }
        public Colleague Programmer { get; set; }
        public Colleague Tester { get; set; }

        public override void Send(string message, Colleague colleague)
        {
            if (colleague == Customer)
            {
                Programmer.Notify(message);
            }
            else if (colleague == Programmer)
            {
                Tester.Notify(message);
            }
            else Customer.Notify(message);
        }
    }

    abstract class Colleague
    {
        protected Mediator mediator;

        public Colleague(Mediator mediator) => this.mediator = mediator;

        public virtual void Send(string message) => this.mediator.Send(message, this);

        public abstract void Notify(string message);

    }

    class Customer : Colleague
    {
        public Customer(Mediator mediator) : base(mediator) { }

        public override void Notify(string message)
        {
            Console.WriteLine($"Message to customer: {message}");
        }
    }

    class Programmer : Colleague
    {
        public Programmer(Mediator mediator) : base(mediator) { }

        public override void Notify(string message)
        {
            Console.WriteLine($"Message to programmer: {message}");
        }
    }

    class Tester : Colleague
    {
        public Tester(Mediator mediator) : base(mediator) { }

        public override void Notify(string message)
        {
            Console.WriteLine($"Message to tester: {message}");
        }
    }
}
