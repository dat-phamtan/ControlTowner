using System;
using System.Collections.Generic;
using System.Text;

namespace ControlTowner.Utility
{
    public interface ILogger
    {
        void Log(string message);
    }

    public interface ILogSource
    {
        public event Action<string>? OnLog;
    }

    public class EventLogger : ILogger, ILogSource
    {
        public event Action<string>? OnLog;
        public void Log(string message)
        {
            OnLog?.Invoke(message);
        }
    }
}
