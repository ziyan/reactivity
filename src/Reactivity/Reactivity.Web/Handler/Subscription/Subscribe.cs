using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Reactivity.Web.Json;

namespace Reactivity.Web.Handler.Subscription
{
    class Subscribe : IHttpHandler, System.Web.SessionState.IRequiresSessionState
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

            if (device == null || device == "")
            {
                ObjectCollection collection = new ObjectCollection();
                collection.Add(new StringValue("status"), new StringValue("InvalidDevice"));
                context.Response.Write(collection);
                return;
            }

            Guid device_guid = Guid.Empty;
            try
            {
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
                Objects.Subscription subscription;
                if (service != null)
                    subscription = Common.Client.Subscribe(device_guid, Convert.ToInt16(service));
                else
                    subscription = Common.Client.Subscribe(device_guid);
                if (subscription != null)
                {
                    ObjectCollection collection = new ObjectCollection();
                    collection.Add(new StringValue("status"), new StringValue("OK"));
                    ObjectCollection subscriptionCollection = new ObjectCollection();
                    subscriptionCollection.Add(new StringValue("guid"), new StringValue(subscription.Guid.ToString()));
                    subscriptionCollection.Add(new StringValue("device"), new StringValue(subscription.Device.ToString()));
                    subscriptionCollection.Add(new StringValue("status"), new StringValue(subscription.Status.ToString()));
                    collection.Add(new StringValue("subscription"), subscriptionCollection);
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


