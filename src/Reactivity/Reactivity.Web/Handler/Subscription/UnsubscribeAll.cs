using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Reactivity.Web.Json;

namespace Reactivity.Web.Handler.Subscription
{
    class UnsubscribeAll : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public bool IsReusable { get { return true; } }
        public void ProcessRequest(HttpContext context)
        {
            // Common settings
            context.Response.ContentEncoding = System.Text.Encoding.UTF8;
            context.Response.ContentType = "text/plain";
            context.Response.Expires = -1;

            if (!Common.IsClientConnected)
            {
                ObjectCollection collection = new ObjectCollection();
                collection.Add(new StringValue("status"), new StringValue("NotConnected"));
                context.Response.Write(collection);
                return;
            }
            try
            {
                Common.Client.UnsubscribeAll();
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


