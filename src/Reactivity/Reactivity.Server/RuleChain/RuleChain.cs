using System;
using System.Collections.Generic;
using System.Xml;
using System.Text.RegularExpressions;
using System.Reflection;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using Reactivity.Objects;
using Reactivity.API;
using Reactivity.Util;

namespace Reactivity.Server
{
    class RuleChain : IDisposable
    {
        private static readonly RuleChain instance = new RuleChain();
        public static RuleChain Instance { get { return instance; } }

        private List<Rule> rules = new List<Rule>();
        private System.Threading.Mutex rules_mutex = new System.Threading.Mutex();
        private Dictionary<int, Assembly> assemblies = new Dictionary<int, Assembly>();
        private Dictionary<int, string> hashes = new Dictionary<int, string>();
        private List<IRule> modules = new List<IRule>();
        private System.Threading.Mutex modules_mutex = new System.Threading.Mutex();
        private Queue<Objects.Data> queue = new Queue<Reactivity.Objects.Data>();
        private System.Threading.Mutex queue_mutex = new System.Threading.Mutex();
        private System.Threading.EventWaitHandle queue_wait = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.AutoReset);
        private List<RuleChainThread> threads = new List<RuleChainThread>();

        private RuleChain()
        {
            // instantiate all threads
            int threadCount = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings[
                    "Reactivity.Server.RuleChain.ThreadCount"]);
            for (int i = 0; i < threadCount; i++)
                threads.Add(new RuleChainThread());
            ReloadFromDatabase();
        }

        public void Dispose()
        {
            // first send signal to stop all threads
            for (int i = 0; i < threads.Count; i++)
                threads[i].Stop();
            // then wait for all threads to stop
            for (int i = 0; i < threads.Count; i++)
                threads[i].Join();

            modules_mutex.WaitOne();
            // then unload the rule chain
            for (int i = 0; i < modules.Count; i++)
                modules[i].Uninitialize();
            modules.Clear();
            modules_mutex.ReleaseMutex();
        }
       
        /// <summary>
        /// Enqueue a new piece of data
        /// </summary>
        /// <param name="data"></param>
        public void DataEnqueue(Objects.Data data)
        {
            if (data == null) return;
            queue_mutex.WaitOne();
            int max = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["Reactivity.Server.RuleChain.DataQueueLength"]);
            while (queue.Count >= max)
                queue.Dequeue();
            queue.Enqueue(data);
            queue_mutex.ReleaseMutex();
            queue_wait.Set();
        }

        /// <summary>
        /// Dequeue a data to process, called by RuleChainThread
        /// </summary>
        /// <returns></returns>
        public Objects.Data DataDequeue()
        {
            if (queue.Count <= 0)
                if (!queue_wait.WaitOne(Convert.ToInt32(
                System.Configuration.ConfigurationManager.AppSettings[
                    "Reactivity.Server.RuleChain.DataDequeueTimeout"]), true))
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

        /// <summary>
        /// Return the entire chain, called by RuleChainThread
        /// </summary>
        /// <returns></returns>
        public List<IRule> Get()
        {
            return modules;
        }

        /// <summary>
        /// Reload chain (Recompile)
        /// </summary>
        public void Reload()
        {
            Clients.ClientSession.Notify(new ClientEvent { Type = ClientEventType.RuleChainReloadBegin, Timestamp = DateTime.Now });

            // first send signal to stop all threads
            for (int i = 0; i < threads.Count; i++)
                threads[i].Stop();
            // then wait for all threads to stop
            for (int i = 0; i < threads.Count; i++)
                threads[i].Join();

            modules_mutex.WaitOne();
            // then unload the rule chain
            for (int i = 0; i < modules.Count; i++)
                modules[i].Uninitialize();
            modules.Clear();

            // then repopulate the rule chain
            rules_mutex.WaitOne();
            rules.Sort();
            for (int i = 0; i < rules.Count; i++)
            {
                Rule rule = rules[i];
                if (rule.ID <= 0 || rule.Configuration == "") continue;
                if (!rule.IsEnabled)
                {
                    if (rule.Status != RuleStatus.Stopped)
                    {
                        rule.Status = RuleStatus.Stopped;
                        Clients.ClientSession.Notify(new ClientEvent { Type = ClientEventType.RuleUpdate, Rule = rule, Timestamp = DateTime.Now });
                    }
                    continue;
                }
                try
                {
                    XmlDocument configuration = Util.Xml.Read(rule.Configuration);
                    Assembly assembly = null;
                    if (configuration["rule"]["code"].HasAttribute("path"))
                        assembly = Assembly.LoadFrom(System.Configuration.ConfigurationManager.AppSettings["Reactivity.Server.RuleChain.AssembliesStoragePath"] + @"\" +
                            configuration["rule"]["code"].Attributes["path"].Value);
                    else
                    {
                        string source = configuration["rule"]["code"].InnerText;
                        string hash = Util.Hash.ToString(source);

                        if (!assemblies.ContainsKey(rule.ID) || !hashes.ContainsKey(rule.ID) || hashes[rule.ID] != hash)
                        {
                            // Compile
                            assembly = RuleChainCompiler.Compile(source);
                            if (assembly != null)
                            {
                                hashes[rule.ID] = hash;
                                assemblies[rule.ID] = assembly;
                            }
                        }
                        else
                        {
                            // load assembly from cache since there is no change at all
                            assembly = assemblies[rule.ID];
                        }
                    }

                    // instantiate the code
                    if (assembly != null)
                    {
                        object instance = assembly.CreateInstance(configuration["rule"]["code"].Attributes["type"].Value);
                        if (instance is IRule)
                        {
                            modules.Add((IRule)instance);
                            Dictionary<string, string> settings = null;
                            if (configuration["rule"].GetElementsByTagName("settings").Count > 0)
                            {
                                settings = new Dictionary<string, string>();
                                XmlNodeList settings_list = configuration["rule"]["settings"].GetElementsByTagName("add");
                                for (int j = 0; j < settings_list.Count; j++)
                                    settings[settings_list[j].Attributes["key"].Value] = settings_list[j].Attributes["value"].Value;

                            }
                            // Initialize
                            if (!((IRule)instance).Initialize(settings))
                            {
                                try { ((IRule)instance).Uninitialize(); }
                                catch { }
                                if (rule.Status != RuleStatus.InitError)
                                {
                                    rule.Status = RuleStatus.InitError;
                                    Clients.ClientSession.Notify(new ClientEvent { Type = ClientEventType.RuleUpdate, Rule = rule, Timestamp = DateTime.Now });
                                }
                            }
                            if (rule.Status != RuleStatus.Running)
                            {
                                rule.Status = RuleStatus.Running;
                                Clients.ClientSession.Notify(new ClientEvent { Type = ClientEventType.RuleUpdate, Rule = rule, Timestamp = DateTime.Now });
                            }
                            continue;
                        }
                    }
                }
                catch (Exception e)
                {
                    Clients.ClientSession.Notify(new ClientEvent { Type = ClientEventType.RuleChainReloadError, String = e.ToString(), Timestamp = DateTime.Now });
                    Util.EventLog.WriteEntry("Reactivity.Server.RuleChain", e);
                }
                //Compile Error, because type is not an IRule
                if (rule.Status != RuleStatus.CompileError)
                {
                    rule.Status = RuleStatus.CompileError;
                    Clients.ClientSession.Notify(new ClientEvent { Type = ClientEventType.RuleUpdate, Rule = rule, Timestamp = DateTime.Now });
                }
            }
            rules_mutex.ReleaseMutex();
            modules_mutex.ReleaseMutex();

            // at last, start all thread
            for (int i = 0; i < threads.Count; i++)
                threads[i].Start();

            Clients.ClientSession.Notify(new ClientEvent { Type = ClientEventType.RuleChainReloadEnd, Timestamp = DateTime.Now });
        }

        /// <summary>
        /// Reload the entire chain from database
        /// </summary>
        public void ReloadFromDatabase()
        {
            Rule[] results = Data.StoredProcedure.RuleListAll(new Reactivity.Data.Connection());
            rules_mutex.WaitOne();
            rules.Clear();
            for (int i = 0; i < results.Length; i++)
                if (results[i].ID > 0)
                    rules.Add(results[i]);
            rules.Sort();
            rules_mutex.ReleaseMutex();
            Reload();
        }

        /// <summary>
        /// List all rules
        /// </summary>
        /// <returns></returns>
        public Rule[] RuleList()
        {
            rules_mutex.WaitOne();
            Rule[] results = rules.ToArray();
            rules_mutex.ReleaseMutex();
            return results;
        }

        /// <summary>
        /// Retrieve detail about one rule
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Rule RuleGet(int id)
        {
            rules_mutex.WaitOne();
            Rule rule = null;
            //find it first
            for (int i = 0; i < rules.Count; i++)
                if (rules[i].ID == id)
                {
                    rule = rules[i];
                    break;
                }
            rules_mutex.ReleaseMutex();
            return rule;
        }

        /// <summary>
        /// Add a new rule to the chain
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public int RuleCreate(Rule rule, Data.Connection connection)
        {
            if (rule == null) return 0;
            rules_mutex.WaitOne();
            rule.ID = Data.StoredProcedure.RuleCreate(rule.Name, rule.Description, rule.Configuration, rule.Precedence, rule.IsEnabled, connection);
            if (rule.ID > 0)
            {
                rule.Status = RuleStatus.Unknown;
                rules.Add(rule);
                rules.Sort();
            }
            rules_mutex.ReleaseMutex();
            if (rule.ID > 0)
            {
                Clients.ClientSession.Notify(new ClientEvent { Type = ClientEventType.RuleCreation, Rule = rule, Timestamp = DateTime.Now });
                Reload();
            }
            return rule.ID;
        }

        /// <summary>
        /// Remove a rule
        /// </summary>
        /// <param name="id"></param>
        /// <param name="connection"></param>
        public bool RuleRemove(int id, Data.Connection connection)
        {
            if (id <= 0) return false;
            rules_mutex.WaitOne();
            Rule rule = null;
            //find it first
            for (int i = 0; i < rules.Count; i++)
                if (rules[i].ID == id)
                {
                    rule = rules[i];
                    break;
                }
            if (rule != null)
            {
                //remove from buffer and database
                if (Data.StoredProcedure.RuleRemoveById(id, connection))
                {
                    rules.Remove(rule);
                    rules.Sort();
                    rules_mutex.ReleaseMutex();
                    Clients.ClientSession.Notify(new ClientEvent { Type = ClientEventType.RuleRemoval, Rule = rule, Timestamp = DateTime.Now });
                    Reload();
                    // clear out cache
                    modules_mutex.WaitOne();
                    hashes.Remove(rule.ID);
                    assemblies.Remove(rule.ID);
                    modules_mutex.ReleaseMutex();
                    return true;
                }
            }
            rules_mutex.ReleaseMutex();
            return false;
        }

        /// <summary>
        /// Update the rule
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public bool RuleUpdate(Rule rule, Data.Connection connection)
        {
            if (rule == null || rule.ID <= 0) return false;
            rules_mutex.WaitOne();
            Rule oldRule = null;
            //find it first
            for (int i = 0; i < rules.Count; i++)
                if (rules[i].ID == rule.ID)
                {
                    oldRule = rules[i];
                    break;
                }
            if (oldRule != null)
            {
                // update rule in database
                if (Data.StoredProcedure.RuleUpdateById(rule.ID, rule.Name, rule.Description, rule.Configuration, rule.Precedence, rule.IsEnabled, connection))
                {
                    rules.Remove(oldRule);
                    rules.Add(rule);
                    rules.Sort();
                    rules_mutex.ReleaseMutex();
                    Clients.ClientSession.Notify(new ClientEvent { Type = ClientEventType.RuleUpdate, Rule = rule, Timestamp = DateTime.Now });
                    Reload();
                    return true;
                }
            }
            rules_mutex.ReleaseMutex();
            return false;
        }
    }
}
