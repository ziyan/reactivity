using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reactivity.API
{
    public interface IAdapter
    {
        /// <summary>
        /// Send data to another device
        /// </summary>
        /// <param name="data"></param>
        void SendData(Objects.Data data);
        void SendData(Objects.Data[] data);

        /// <summary>
        /// Feed client subscription
        /// </summary>
        /// <param name="data"></param>
        void FeedSubscription(Objects.Data data);
        void FeedSubscription(Objects.Data[] data);

        /// <summary>
        /// Add data to statistics
        /// </summary>
        /// <param name="data"></param>
        void AddToStatistics(Objects.Data data);
        void AddToStatistics(Objects.Data[] data);

        /// <summary>
        /// Log to system
        /// </summary>
        /// <param name="source"></param>
        /// <param name="message"></param>
        void Log(object source, object message);

        /// <summary>
        /// Debug Log to client
        /// </summary>
        /// <param name="source"></param>
        /// <param name="message"></param>
        void Debug(object source, object message);

    }
}
