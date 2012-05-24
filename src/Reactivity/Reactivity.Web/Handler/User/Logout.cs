using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Reactivity.Web.Json;

namespace Reactivity.Web.Handler.User
{
    class Logout : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public bool IsReusable { get { return true; } }
        public void ProcessRequest(HttpContext context)
        {
            // Common settings
            context.Response.ContentEncoding = System.Text.Encoding.UTF8;
            context.Response.ContentType = "text/plain";
            context.Response.Expires = -1;

            string forget = context.Request["forget"];

            if (forget != null && forget == "true")
                Common.ClearSavedLogin();

            if (Common.IsUserLoggedIn)
            {
                Common.Client.UserLogout();
                ObjectCollection collection = new ObjectCollection();
                
                collection.Add(new StringValue("status"), new StringValue("LoggedOut"));
                context.Response.Write(collection);
                return;
            }
            else
            {
                ObjectCollection collection = new ObjectCollection();
                
                collection.Add(new StringValue("status"), new StringValue("NotLoggedIn"));
                context.Response.Write(collection);
                return;
            }

        }
    }
}


