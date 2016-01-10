using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUFL.Service
{
    public class GlobalEvent
    {
        public EventEnum Name { set; get; }
        public object Argument { set; get; }
        public object Sender { set; get; }
    }
  
    public enum EventEnum
    {
        ProgramChanged,
        RankListChanged,
        StatusChanged,
    }

    public delegate void GlobalEventHandler(GlobalEvent @event);
    public class GlobalEventManager
    {
        static GlobalEventManager _instance = null;
        public static GlobalEventManager Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new GlobalEventManager();
                }
                return _instance;
            }
        }


        Dictionary<EventEnum, GlobalEventHandler> _handler_mapping = new Dictionary<EventEnum, GlobalEventHandler>();
        Dictionary<EventEnum, GlobalEvent> _last_event_mapping = new Dictionary<EventEnum, GlobalEvent>();
        private GlobalEventManager()
        {

        }

        public void SubscribeEventByName(EventEnum event_name, GlobalEventHandler handler)
        {
            if(_handler_mapping.ContainsKey(event_name))
            {
                _handler_mapping[event_name] += handler;
            } else
            {
                _handler_mapping[event_name] = handler;
            }
            if(_last_event_mapping.ContainsKey(event_name))
            {
                try
                {
                    handler(_last_event_mapping[event_name]);
                }
                catch (Exception)
                {
                    System.Diagnostics.Debug.WriteLine("Error in handing in message " + event_name);
                }
            }
        }

        public void UnsubscribeEventByName(EventEnum event_name, GlobalEventHandler handler)
        {
            _handler_mapping[event_name] -= handler;
        }

        public void RaiseEvent(GlobalEvent @event)
        {
            _last_event_mapping[@event.Name] = @event;
            if (_handler_mapping.ContainsKey(@event.Name))
            {
                try
                {
                    _handler_mapping[@event.Name](@event);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Error in handing in message " + @event.Name);
                }
               
            }
        }
    }
}
