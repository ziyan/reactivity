using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Reactivity.Web.Json;

namespace Reactivity.Web.Handler.Statistics
{
    class Query : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public bool IsReusable { get { return true; } }
        public void ProcessRequest(HttpContext context)
        {
            // Common settings
            context.Response.ContentEncoding = System.Text.Encoding.UTF8;
            context.Response.ContentType = "text/plain";
            context.Response.Expires = -1;

            string device = context.Request["device"];
            string service = context.Request["service"];
            string start_date = context.Request["start_date"];
            string end_date = context.Request["end_date"];
            string type = context.Request["type"];

            // Parse input
            Guid device_guid = Guid.Empty;
            try
            {
                if (device != null && device != "")
                    device_guid = new Guid(device);
            }
            catch { }
            if (device_guid == Guid.Empty)
            {
                ObjectCollection collection = new ObjectCollection();
                collection.Add(new StringValue("status"), new StringValue("InvalidDevice"));
                context.Response.Write(collection);
                return;
            }

            short service_short = 0;
            try
            {
                if (service != null && service != "")
                    service_short = Convert.ToInt16(service);
            }
            catch { }
            if (service_short == 0)
            {
                ObjectCollection collection = new ObjectCollection();
                collection.Add(new StringValue("status"), new StringValue("InvalidService"));
                context.Response.Write(collection);
                return;
            }

            Objects.StatisticsType type_type;
            if(type == "1")
                type_type = Reactivity.Objects.StatisticsType.Minutely;
            else if(type == "2")
                type_type = Reactivity.Objects.StatisticsType.Hourly;
            else if(type == "3")
                type_type = Reactivity.Objects.StatisticsType.Daily;
            else if (type == "4")
                type_type = Reactivity.Objects.StatisticsType.Monthly;
            else
            {
                ObjectCollection collection = new ObjectCollection();
                collection.Add(new StringValue("status"), new StringValue("InvalidType"));
                context.Response.Write(collection);
                return;
            }

            DateTime start_date_datetime = new DateTime(1970, 1, 1);
            DateTime end_date_datetime = new DateTime(1970, 1, 1);
            try
            {
                if (start_date != null && start_date != "" && end_date != null && end_date != "")
                {
                    start_date_datetime = start_date_datetime.AddMilliseconds(Convert.ToDouble(start_date));
                    end_date_datetime = end_date_datetime.AddMilliseconds(Convert.ToDouble(end_date));
                    start_date_datetime = start_date_datetime.ToLocalTime();
                    end_date_datetime = end_date_datetime.ToLocalTime();
                    if (start_date_datetime > end_date_datetime)
                    {
                        ObjectCollection collection = new ObjectCollection();
                        collection.Add(new StringValue("status"), new StringValue("InvalidDateRange"));
                        context.Response.Write(collection);
                        return;
                    }
                }
                else
                {
                    ObjectCollection collection = new ObjectCollection();
                    collection.Add(new StringValue("status"), new StringValue("InvalidDateRange"));
                    context.Response.Write(collection);
                    return;
                }
            }
            catch
            {
                ObjectCollection collection = new ObjectCollection();
                collection.Add(new StringValue("status"), new StringValue("InvalidDateRange"));
                context.Response.Write(collection);
                return;
            }

            if (!Common.IsUserLoggedIn)
            {
                ObjectCollection collection = new ObjectCollection();
                collection.Add(new StringValue("status"), new StringValue("NotLoggedIn"));
                context.Response.Write(collection);
                return;
            }
            if (!Common.Client.UserHasPermission(Objects.User.PERMISSION_SUBSCRIBE))
            {
                ObjectCollection collection = new ObjectCollection();
                collection.Add(new StringValue("status"), new StringValue("PermissionDenied"));
                context.Response.Write(collection);
                return;
            }

            try
            {
                Objects.Statistics[] stats = Common.Client.StatisticsQuery(device_guid, service_short, start_date_datetime, end_date_datetime, type_type);
                if (stats != null)
                {
                    Array.Sort<Objects.Statistics>(stats);
                    Array.Reverse(stats);
                    ObjectCollection collection = new ObjectCollection();
                    collection.Add(new StringValue("status"), new StringValue("OK"));
                    collection.Add(new StringValue("date"), new StringValue(start_date_datetime.ToString()));
                    ArrayCollection array = new ArrayCollection();
                    for (int i = 0; i < stats.Length; i++)
                    {
                        ObjectCollection stat = new ObjectCollection();
                        stat.Add(new StringValue("device"), new StringValue(stats[i].Device.ToString()));
                        stat.Add(new StringValue("service"), new NumberValue(stats[i].Service));
                        stat.Add(new StringValue("type"), new NumberValue(Convert.ToByte(stats[i].Type)));
                        stat.Add(new StringValue("date"), new DateValue(stats[i].Date));
                        stat.Add(new StringValue("count"), new NumberValue(stats[i].Count));
                        stat.Add(new StringValue("value"), new NumberValue(stats[i].Value));
                        array.Add(stat);
                    }
                    collection.Add(new StringValue("statistics"), array);
                    context.Response.Write(collection);
                    return;
                }
            }
            catch { }
            {
                ObjectCollection collection = new ObjectCollection();
                collection.Add(new StringValue("status"), new StringValue("Error"));
                context.Response.Write(collection);
                return;
            }
        }
    }
}


