using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Reactivity.Web.Json;

namespace Reactivity.Web.Handler.User
{
    class Current : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public bool IsReusable { get { return true; } }
        public void ProcessRequest(HttpContext context)
        {
            // Common settings
            context.Response.ContentEncoding = System.Text.Encoding.UTF8;
            context.Response.ContentType = "text/plain";
            context.Response.Expires = -1;

            Common.CheckSavedLogin();

            if (Common.IsUserLoggedIn)
            {
                Objects.User user = Common.Client.UserCurrent;
                ObjectCollection collection = new ObjectCollection();

                collection.Add(new StringValue("status"), new StringValue("LoggedIn"));
                ObjectCollection userCollection = new ObjectCollection();
                userCollection.Add(new StringValue("id"), new NumberValue(user.ID));
                userCollection.Add(new StringValue("username"), new StringValue(user.Username));
                userCollection.Add(new StringValue("name"), new StringValue(user.Name));
                userCollection.Add(new StringValue("description"), new StringValue(user.Description));
                userCollection.Add(new StringValue("permission"), new NumberValue(user.Permission));
                collection.Add(new StringValue("user"), userCollection);
                context.Response.Write(collection);
            }
            else
            {
                ObjectCollection collection = new ObjectCollection();
                collection.Add(new StringValue("status"), new StringValue("NotLoggedIn"));
                context.Response.Write(collection);
            }
        }
    }
}


