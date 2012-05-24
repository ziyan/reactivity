using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;


namespace Reactivity.Server
{
    class StatisticsManager : IDisposable
    {
        private static readonly StatisticsManager instance = new StatisticsManager();
        public static StatisticsManager Instance { get { return instance; } }

        private System.Threading.Thread thread;
        private bool safeToRun = true;
        private Queue<Objects.Data> queue = new Queue<Reactivity.Objects.Data>();
        private System.Threading.Mutex queue_mutex = new System.Threading.Mutex();
        private System.Threading.EventWaitHandle queue_wait = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.AutoReset);
        private DataTable stat;
        private System.Threading.Mutex stat_mutex = new System.Threading.Mutex();
        private System.Timers.Timer timer;

        private StatisticsManager()
        {
            stat_mutex.WaitOne();
            stat = new DataTable();
            stat.Columns.Add(new DataColumn("device", typeof(Guid)));
            stat.Columns.Add(new DataColumn("service", typeof(short)));
            stat.Columns.Add(new DataColumn("date", typeof(DateTime)));
            stat.Columns.Add(new DataColumn("count", typeof(long)));
            stat.Columns.Add(new DataColumn("value", typeof(double)));
            stat.PrimaryKey = new DataColumn[] { stat.Columns["device"], stat.Columns["service"], stat.Columns["date"]};
            stat_mutex.ReleaseMutex();

            thread = new System.Threading.Thread(new System.Threading.ThreadStart(Thread));
            thread.Start();

            timer = new System.Timers.Timer(Convert.ToDouble(System.Configuration.ConfigurationManager.AppSettings["Reactivity.Server.StatisticsManager.TimerInterval"]));
            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            timer.AutoReset = true;
            timer.Start();
        }

        public void Dispose()
        {
            safeToRun = false;
            thread.Join();
        }

        /// <summary>
        /// Enqueue a new piece of data
        /// </summary>
        /// <param name="data"></param>
        public void DataEnqueue(Objects.Data data)
        {
            if (data == null || data.Device == Guid.Empty || data.Service == 0) return;
            if (!Util.DataAdapter.IsGraphable(data)) return;
            queue_mutex.WaitOne();
            int max = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["Reactivity.Server.StatisticsManager.DataQueueLength"]);
            while (queue.Count >= max)
                queue.Dequeue();
            queue.Enqueue(data);
            queue_mutex.ReleaseMutex();
            queue_wait.Set();
        }

        /// <summary>
        /// Dequeue a data to process, called by Thread
        /// </summary>
        /// <returns></returns>
        public Objects.Data DataDequeue()
        {
            if (queue.Count <= 0)
                if (!queue_wait.WaitOne(Convert.ToInt32(
                System.Configuration.ConfigurationManager.AppSettings[
                    "Reactivity.Server.StatisticsManager.DataDequeueTimeout"]), true))
                    return null;
            queue_mutex.WaitOne();
            if (queue.Count <= 0)
            {
                queue_mutex.ReleaseMutex();
                return null;
            }
            Objects.Data data = queue.Dequeue();
            queue_mutex.ReleaseMutex();
            return data;
        }

        private void Thread()
        {
            while (safeToRun)
                New(DataDequeue());
        }

        private void New(Objects.Data data)
        {
            if(data == null) return;
            DateTime date = new DateTime(data.Timestamp.Year, data.Timestamp.Month, data.Timestamp.Day, data.Timestamp.Hour, data.Timestamp.Minute, 0);
            stat_mutex.WaitOne();
            DataRow[] rows = stat.Select("device = '" + data.Device.ToString() + "' and service = " + data.Service.ToString() + " and date = #" + date.ToString() + "#");
            if (rows.Length <= 0)
            {
                //new record
                DataRow row = stat.NewRow();
                row["device"] = data.Device;
                row["service"] = data.Service;
                row["date"] = date;
                row["count"] = 1;
                row["value"] = Util.DataAdapter.GetGraphableValue(data);
                stat.Rows.Add(row);
            }
            else
            {
                long count = Convert.ToInt64(rows[0]["count"]);
                double value = Convert.ToDouble(rows[0]["value"]);
                rows[0]["value"] = value + Util.DataAdapter.GetGraphableValue(data);
                rows[0]["count"] = count + 1;
            }
            stat_mutex.ReleaseMutex();
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                Data.Connection connection = new Reactivity.Data.Connection();
                stat_mutex.WaitOne();
                foreach (DataRow row in stat.Rows)
                    Data.StoredProcedure.StatisticsAdd(
                        new Guid(row["device"].ToString()),
                        Convert.ToInt16(row["service"]),
                        Convert.ToDateTime(row["date"]),
                        Convert.ToInt64(row["count"]),
                        Convert.ToDouble(row["value"]),
                        connection);
                stat.Rows.Clear();
                stat_mutex.ReleaseMutex();
                Data.StoredProcedure.StatisticsClean(
                    Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["Reactivity.Server.StatisticsManager.MinutesToKeep"]),
                    Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["Reactivity.Server.StatisticsManager.HoursToKeep"]),
                    Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["Reactivity.Server.StatisticsManager.DaysToKeep"]),
                    Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["Reactivity.Server.StatisticsManager.MonthsToKeep"]),
                    connection);
                connection.Dispose();
            }
            catch (Exception ee)
            {
                Util.EventLog.WriteEntry(this, "Exception: " + ee);
            }
        }
    }
}
