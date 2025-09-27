using System;
using System.Collections.Generic;

namespace Events
{
    public class PriorityEvent : PriorityEventBase
    {
        private SortedList<int, Action> _events = new();
        private List<(int,Action)> _eventsToAdd = new();
        private List<(int,Action)> _eventsToRemove = new();
    
        public void AddListener(Action listener, int priority = 0)
        {
            if (_isInvoking)
            {
                _eventsToAdd.Add((priority, listener));
                return;
            }
            if (_events.ContainsKey(priority))
            {
                _events[priority] += listener;
            }
            else
            {
                _events.Add(priority, listener);
            }
        }
    
        public void RemoveListener(Action listener, int priority)
        {
            if (_isInvoking)
            {
                _eventsToRemove.Add((priority, listener));
                return;
            }
            if (_events.ContainsKey(priority))
            {
                _events[priority] -= listener;
            }
        }
    
        public void RemoveListener(Action listener)
        {
            if (_isInvoking)
            {
                _eventsToRemove.Add((0, listener));
                return;
            }
            var keys = new List<int>(_events.Keys);
            foreach (var k in keys)
            {
                _events[k] -= listener;
            }
        }
    
        public void ClearListeners(int priority)
        {
            if (_isInvoking)
            {
                _keysToClear.Add(priority);
                return;
            }
            if (_events.ContainsKey(priority))
            {
                _events.Remove(priority);
            }
        }
    
        public void ClearListeners()
        {
            if (_isInvoking)
            {
                _clearAll = true;
                return;
            }
            _events.Clear();
        }
    
        public void Invoke()
        {
            _isInvoking = true;
            foreach (Action e in _events.Values)
            {
                e?.Invoke();
            }
            _isInvoking = false;
        
            foreach (var (priority, listener) in _eventsToAdd)
            {
                AddListener(listener, priority);
            }
        
            foreach (var (priority, listener) in _eventsToRemove)
            {
                RemoveListener(listener, priority);
            }
        
            _eventsToAdd.Clear();
            _eventsToRemove.Clear();

            if (_clearAll)
            {
                _events.Clear();
            }

            foreach (var key in _keysToClear)
            {
                ClearListeners(key);
            }
            _keysToClear.Clear();
        
        
        }

        public override void Clear() => ClearListeners();
    }
}