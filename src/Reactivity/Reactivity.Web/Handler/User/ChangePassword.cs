using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Reactivity.Web.Json;

namespace Reactivity.Web.Handler.User
{
    class ChangePassword : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public bool IsReusable { get { return true; } }
        public void ProcessRequest(HttpContext context)
        {
            // Common settings
            context.Response.ContentEncoding = System.Text.Encoding.UTF8;
            context.Response.ContentType = "text/plain";
            context.Response.Expires = -1;

            // Get the request
            string old_password = context.Request["old_password"];
            string new_password = context.Request["new_password"];

            if (old_password == null || new_password == null || old_password.Length != Util.Hash.StringLength || new_password.Length != Util.Hash.StringLength)
            {
                ObjectCollection collection = new ObjectCollection();
                collection.Add(new StringValue("status"), new StringValue("InvalidPassword"));
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

            if (Common.Client.UserChangePassword(old_password, new_password))
            {
                Common.ClearSavedLogin();
                ObjectCollection collection = new ObjectCollection();
                collection.Add(new StringValue("status"), new StringValue("PasswordChanged"));
                context.Response.Write(collection);
                return;
            }
            else
            {
                ObjectCollection collection = new ObjectCollection();
                collection.Add(new StringValue("status"), new StringValue("InvalidPassword"));
                context.Response.Write(collection);
                return;
            }
        }
    }
}


