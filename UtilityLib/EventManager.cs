using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib
{
    // 全てボタンのイベントを管理するクラス
    public static class EventManager
    {
        // イベントの辞書
        private static Dictionary<string, Action> events = new Dictionary<string, Action>();

        // イベントの追加
        public static void AddEvent(string eventName, Action action)
        {
            Debug.WriteLine($"AddEvent {eventName}");
            if (events.ContainsKey(eventName))
            {
                events[eventName] = action;
            }
            else
            {
                events.Add(eventName, action);
            }
        }

        // イベントの削除
        public static void RemoveEvent(string eventName, Action action)
        {
            if (events.ContainsKey(eventName))
            {
                events[eventName] = action;

                // イベントに登録されたハンドラが無くなった場合、辞書から削除
                if (events[eventName] == null)
                {
                    events.Remove(eventName);
                }
            }
        }

        // イベントの発火
        public static void TriggerEvent(string eventName)
        {
            if (events.ContainsKey(eventName))
            {
                Debug.WriteLine($"TriggerEvent {eventName}");
                events[eventName]?.Invoke();
            }
        }
    }

    public static class EventManager2
    {
        public delegate void EventHandler(params object[] args);

        static Dictionary<string, List<EventHandler>> events = new Dictionary<string, List<EventHandler>>();


        static public void CreateEvent(string eventName)
        {
            if (events.ContainsKey(eventName))
            {
                return;
            }

            List<EventHandler> newEvent = new List<EventHandler>();
            events.Add(eventName, newEvent);
        }

        static public void Register(string eventName, EventHandler callback)
        {
            if (!events.ContainsKey(eventName))
            {
                CreateEvent(eventName);
            }

            events[eventName].Add(callback);
        }

        static public void Deregister(string eventName, EventHandler callback)
        {
            if (!events.ContainsKey(eventName))
            {
                return;
            }

            events[eventName].Remove(callback);
        }

        static public void Push(string eventName, params object[] args)
        {
            if (!events.ContainsKey(eventName))
            {
                CreateEvent(eventName);
            }

            EventHandler[] callbacks = events[eventName].ToArray();

            foreach (EventHandler callback in callbacks)
            {
                callback(args);
            }
        }
    }
}
