using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.DesignPatterns.Observer
{
    internal class Program
    {
        public Program()
        {

            MessagePublisher publisher = new MessagePublisher();

            // Create subscribers
            UserSubscriber alice = new UserSubscriber("Alice");
            UserSubscriber bob = new UserSubscriber("Bob");

            // Subscribe users to receive updates
            publisher.Subscribe(alice);
            publisher.Subscribe(bob);

            // Post a message - all subscribers get notified
            publisher.PostMessage("Hello, World!");

            // Unsubscribe a user
            publisher.Unsubscribe(bob);

            // Post another message - only subscribed users get notified
            publisher.PostMessage("Second message");
        }
    }
}
