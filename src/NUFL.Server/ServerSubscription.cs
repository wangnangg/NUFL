using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace NUFL.Server
{
    public class ServerSubscription:Service.GlobalInstanceServiceBase, IServerSubscription
    {
        public readonly ManualResetEvent NoSubscriber;

        HashSet<Guid> subscribers;

        public ServerSubscription(Guid guid)
        {
            NoSubscriber = new ManualResetEvent(false);
            subscribers = new HashSet<Guid>();
            Subscribe(guid);

        }
        public void Subscribe(Guid guid)
        {
            lock (this)
            {
                subscribers.Add(guid);
                Console.WriteLine(guid + " subscribed.");
                Console.WriteLine(subscribers.Count);
            }

        }

        public void UnSubscribe(Guid guid)
        {
            lock (this)
            {
                subscribers.Remove(guid);
                if (subscribers.Count == 0)
                {
                    NoSubscriber.Set();
                }
                Console.WriteLine(guid + " unsubscribed.");
                Console.WriteLine(subscribers.Count);
            }
        }
    }
}
