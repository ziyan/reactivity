using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Reactivity.Objects;
using Reactivity.API;
using System.Net.Mail;

namespace Reactivity.Rules
{
    public class ThresholdEmailWarning : IRule
    {
        private double threshold = 0;
        private string comparison = "==";
        private Guid device = Guid.Empty;
        private short service = Util.ServiceType.Default;
        private string from, to, subject, body, server, username, password;
        private int port = 25;
        private DateTime timestamp = new DateTime(1970, 1, 1);
        private int interval = 60000;
        private bool ssl = false;
        public bool Initialize(Dictionary<string, string> settings)
        {
            if (settings == null || !settings.ContainsKey("threshold") ||
                !settings.ContainsKey("comparison") ||
                !settings.ContainsKey("device") ||
                !settings.ContainsKey("service") ||
                !settings.ContainsKey("from") ||
                !settings.ContainsKey("to") ||
                !settings.ContainsKey("subject") ||
                !settings.ContainsKey("body") ||
                !settings.ContainsKey("server") ||
                !settings.ContainsKey("username") ||
                !settings.ContainsKey("password") ||
                !settings.ContainsKey("ssl") ||
                !settings.ContainsKey("interval"))
                return false;
            interval = Convert.ToInt32(settings["interval"]);
            comparison = settings["comparison"];
            threshold = Convert.ToDouble(settings["threshold"]);
            device = new Guid(settings["device"]);
            service = Convert.ToInt16(settings["service"]);
            ssl = Convert.ToBoolean(settings["ssl"]);
            from = settings["from"];
            to = settings["to"];
            body = settings["body"];
            subject = settings["subject"];
            server = settings["server"];
            if (settings.ContainsKey("port"))
                port = Convert.ToInt32(settings["port"]);
            username = settings["username"];
            password = settings["password"];
            return true;
        }
        public bool Process(Data data, IAdapter adapter)
        {
            if (data.Device == device && (data.Service & service) > 0)
            {
                double value = Util.DataAdapter.GetGraphableValue(data);
                if (comparison == "<")
                {
                    if (value < threshold) Email(value, adapter);
                }
                else if (comparison == "<=")
                {
                    if (value <= threshold) Email(value, adapter);
                }
                else if (comparison == "==")
                {
                    if (value == threshold) Email(value, adapter);
                }
                else if (comparison == ">=")
                {
                    if (value >= threshold) Email(value, adapter);
                }
                else if (comparison == ">")
                {
                    if (value > threshold) Email(value, adapter);
                }
            }
            return true;
        }

        private void Email(double value, IAdapter adapter)
        {
            if ((DateTime.Now - timestamp).TotalSeconds < interval) return;
            try
            {
                SmtpClient client = new SmtpClient(server, port);
                client.EnableSsl = ssl;
                if (username != "")
                    client.Credentials = new System.Net.NetworkCredential(username, password);
                MailMessage msg = new MailMessage(from, to, subject.Replace("{value}", value.ToString()), body.Replace("{value}", value.ToString()));
                client.Send(msg);
                timestamp = DateTime.Now;
            }
            catch (Exception e)
            {
                adapter.Debug(this, e);
            }
        }

        public void Uninitialize()
        {
        }
    }
}
