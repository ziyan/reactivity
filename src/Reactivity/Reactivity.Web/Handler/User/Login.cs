using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Reactivity.Web.Json;

namespace Reactivity.Web.Handler.User
{
    class Login : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public bool IsReusable { get { return true; } }
        public void ProcessRequest(HttpContext context)
        {
            // Common settings
            context.Response.ContentEncoding = System.Text.Encoding.UTF8;
            context.Response.ContentType = "text/plain";
            context.Response.Expires = -1;

            // Get the request
            string uri = context.Request["uri"];
            string username = context.Request["username"];
            string password = context.Request["password"];
            string remember = context.Request["remember"];

            // Check input
            if (uri == null || !Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            {
                ObjectCollection collection = new ObjectCollection();
                
                collection.Add(new StringValue("status"), new StringValue("InvalidUri"));
                context.Response.Write(collection);
                return;
            }
            if (!Util.Validator.IsUsername(username))
            {
                ObjectCollection collection = new ObjectCollection();
                
                collection.Add(new StringValue("status"), new StringValue("InvalidUsername"));
                context.Response.Write(collection);
                return;
            }
            if (password == null || password.Length != Util.Hash.StringLength)
            {
                ObjectCollection collection = new ObjectCollection();
                
                collection.Add(new StringValue("status"), new StringValue("InvalidPassword"));
                context.Response.Write(collection);
                return;
            }
            if (Common.IsUserLoggedIn)
            {
                Objects.User user = Common.Client.UserCurrent;
                ObjectCollection collection = new ObjectCollection();
                collection.Add(new StringValue("status"), new StringValue("AlreadyLoggedIn"));
                ObjectCollection userCollection = new ObjectCollection();
                userCollection.Add(new StringValue("id"), new NumberValue(user.ID));
                userCollection.Add(new StringValue("username"), new StringValue(user.Username));
                userCollection.Add(new StringValue("name"), new StringValue(user.Name));
                userCollection.Add(new StringValue("description"), new StringValue(user.Description));
                userCollection.Add(new StringValue("permission"), new NumberValue(user.Permission));
                collection.Add(new StringValue("user"), userCollection);
                context.Response.Write(collection);
                return;
            }

            // Now try to connect to the server
            if (Common.Client == null || Common.Client.Uri.ToLower() != uri.ToLower())
            {
                if (Common.Client != null)
                {
                    try { Common.Client.Close(); }
                    finally { Common.Client = null; }
                }

                try { Common.Client = new Reactivity.Clients.Client(uri); }
                catch (Exception e)
                {
                    ObjectCollection collection = new ObjectCollection();
                    collection.Add(new StringValue("status"), new StringValue("ConnectionFailed"));
                    collection.Add(new StringValue("error"), new StringValue(e.ToString()));
                    context.Response.Write(collection);
                    return;
                }
            }

            // Now try to login
            try
            {
                if (Common.Client.UserLogin(username, password))
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
                    if (remember != null && remember == "true")
                    {
                        HttpCookie cookie_uri = new HttpCookie("reactivity_uri", uri.ToLower());
                        HttpCookie cookie_username = new HttpCookie("reactivity_username", username);
                        HttpCookie cookie_password = new HttpCookie("reactivity_password", password);
                        cookie_uri.Expires = DateTime.Now.AddMonths(1);
                        cookie_username.Expires = DateTime.Now.AddMonths(1);
                        cookie_password.Expires = DateTime.Now.AddMonths(1);
                        cookie_uri.Path = "/";
                        cookie_username.Path = "/";
                        cookie_password.Path = "/";
                        HttpContext.Current.Response.Cookies.Add(cookie_uri);
                        HttpContext.Current.Response.Cookies.Add(cookie_username);
                        HttpContext.Current.Response.Cookies.Add(cookie_password);
                    }
                    context.Response.Write(collection);
                }
                else
                {
                    ObjectCollection collection = new ObjectCollection();
                    
                    collection.Add(new StringValue("status"), new StringValue("InvalidPassword"));
                    context.Response.Write(collection);
                }
                return;
            }
            catch(Exception e)
            {
                ObjectCollection collection = new ObjectCollection();
                
                collection.Add(new StringValue("status"), new StringValue("Error"));
                collection.Add(new StringValue("error"), new StringValue(e.ToString()));
                context.Response.Write(collection);
                return;
            }
           
        }
    }
}


