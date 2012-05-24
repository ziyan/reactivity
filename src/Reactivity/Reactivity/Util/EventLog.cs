using System;
using System.IO;

namespace Reactivity.Util
{
    public static class EventLog
    {
        private static StreamWriter log = null;

        static EventLog()
        {
            string path = System.Configuration.ConfigurationManager.AppSettings["Reactivity.Util.EventLog.LogFilePath"];
            if (path != null && path != "")
                log = new StreamWriter(new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read), System.Text.Encoding.UTF8);
            else
                if (!System.Diagnostics.EventLog.Exists("Reactivity"))
                    System.Diagnostics.EventLog.CreateEventSource("Reactivity", "Reactivity");
        }

        public static void WriteEntry(object source, object message)
        {
            WriteEntry(source, message, 0, System.Diagnostics.EventLogEntryType.Information);
        }

        public static void WriteEntry(object source, object message, int eventId)
        {
            WriteEntry(source, message, eventId, System.Diagnostics.EventLogEntryType.Information);
        }

        public static void WriteEntry(object source, object message, System.Diagnostics.EventLogEntryType type)
        {
            WriteEntry(source, message, 0, type);
        }

        public static void WriteEntry(object source, object message, int eventId, System.Diagnostics.EventLogEntryType type)
        {
            string sourceName = source is string ? source.ToString() : source.GetType().ToString();
            if (log != null)
            {
                log.WriteLine("[" + DateTime.Now.ToString() + "]\t" +
                    eventId.ToString() + "\t<" +
                    type.ToString() + ">\t" +
                    sourceName + ":\t" +
                    message.ToString().Replace("\r\n", "\\n"));
                log.Flush();
            }
            else
            {
                if(!System.Diagnostics.EventLog.SourceExists(sourceName))
                    System.Diagnostics.EventLog.CreateEventSource(sourceName, "Reactivity");
                System.Diagnostics.EventLog.WriteEntry(sourceName, message.ToString(), type, eventId);
            }
        }

        public static bool RegisterEventSource(string source)
        {
            if (log != null) return true;
            try
            {
                string sourceName = source is string ? source.ToString() : source.GetType().ToString();
                if (!System.Diagnostics.EventLog.SourceExists(sourceName))
                    System.Diagnostics.EventLog.CreateEventSource(sourceName, "Reactivity");
                return true;
            }
            catch { return false; }
        }
    }
}
