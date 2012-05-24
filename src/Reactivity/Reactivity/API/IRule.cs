using System;
using Reactivity.Objects;
using System.Collections.Generic;

namespace Reactivity.API
{
    /// <summary>
    /// Every rule need to inherit from this interface
    /// </summary>
    public interface IRule
    {
        /// <summary>
        /// Initialize your rule here
        /// </summary>
        /// <param name="settings"></param>
        /// <returns>Status</returns>
        bool Initialize(Dictionary<string, string> settings);

        /// <summary>
        /// This will get called for each data
        /// Be careful that your rule will run across threads
        /// and this method is called simultaneously, so use
        /// mutex when needed.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="adapter">A useful adapter to manipulate data</param>
        /// <returns>Whether to continue the rule chain or not</returns>
        bool Process(Data data, IAdapter adapter);

        /// <summary>
        /// Do uninitialization here
        /// </summary>
        void Uninitialize();
    }
}
