using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Reactivity.Web.Json;

namespace Reactivity.Web.Handler.Subscription
{
    class Unsubscribe : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public bool IsReusable { get { return true; } }
        public void ProcessRequest(HttpContext context)
        {
            // Common settings
            context.Response.ContentEncoding = System.Text.Encoding.UTF8;
            context.Response.ContentType = "text/plain";
            context.Response.Expires = -1;

            string subscription = context.Request["subscription"];

            if (subscription == null || subscription == "")
            {
                ObjectCollection collection = new ObjectCollection();
                collection.Add(new StringValue("status"), new StringValue("InvalidSubscription"));
                context.Response.Write(collection);
                return;
            }

            Guid guid = Guid.Empty;
            try
            {
                guid = new Guid(subscription);
            }
            catch { }
            if (guid == Guid.Empty)
            {
                ObjectCollection collection = new ObjectCollection();
                collection.Add(new StringValue("status"), new StringValue("InvalidSubscription"));
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
            try
            {
                Common.Client.Unsubscribe(guid);
                ObjectCollection collection = new ObjectCollection();
                collection.Add(new StringValue("status"), new StringValue("OK"));
                context.Response.Write(collection);
                return;
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


