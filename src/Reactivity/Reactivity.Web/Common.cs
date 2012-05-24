using System;
using System.Collections.Generic;
using System.Web;
using System.Text;
using Reactivity.Clients;

namespace Reactivity.Web
{
    static class Common
    {
        public static Client Client
        {
            get
            {
                if (HttpContext.Current != null && HttpContext.Current.Session != null && HttpContext.Current.Session[Session.ToString()] != null)
                    return (Client)HttpContext.Current.Session[Session.ToString()];
                return null;
            }
            set
            {
                if (HttpContext.Current != null && HttpContext.Current.Session !=null)
                    HttpContext.Current.Session[Session.ToString()] = value;
            }
        }
        public static Guid Session
        {
            get
            {
                if (HttpContext.Current == null) return Guid.Empty;
                string session = HttpContext.Current.Request["session"];
                if (session == null) return Guid.Empty;
                try { return new Guid(session); }
                catch { return Guid.Empty; }
            }
        }
        public static bool IsClientConnected
        {
            get { return Client != null && Client.IsOpen; }
        }
        public static bool IsUserLoggedIn
        {
            get { return IsClientConnected && Client.UserIsLoggedIn; }
        }
        public static void CheckSavedLogin()
        {
            if (IsUserLoggedIn) return;
            if (HttpContext.Current == null || HttpContext.Current.Session == null) return;
            HttpCookie uri = HttpContext.Current.Request.Cookies["reactivity_uri"];
            HttpCookie username = HttpContext.Current.Request.Cookies["reactivity_username"];
            HttpCookie password = HttpContext.Current.Request.Cookies["reactivity_password"];
            if (uri == null || username == null || password == null || password.Value.Length != Util.Hash.StringLength || !Uri.IsWellFormedUriString(uri.Value, UriKind.Absolute))
            {
                ClearSavedLogin();
                return;
            }
            Client client = Client;
            if (client != null && client.Uri.ToLower() != uri.Value)
            {
                ClearSavedLogin();
                return;
            }
            if (client == null)
            {
                try { Client = new Reactivity.Clients.Client(uri.Value); }
                catch
                {
                    ClearSavedLogin();
                    return;
                }
            }
            try
            {
                if (Common.Client.UserLogin(username.Value, password.Value))
                    return;
            }
            catch { }
            ClearSavedLogin();

        }
        public static void ClearSavedLogin()
        {
            if (HttpContext.Current.Request.Cookies["reactivity_uri"] != null)
            {
                HttpCookie cookie = new HttpCookie("reactivity_uri");
                cookie.Expires = DateTime.Now.AddYears(-1);
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
            if (HttpContext.Current.Request.Cookies["reactivity_username"] != null)
            {
                HttpCookie cookie = new HttpCookie("reactivity_username");
                cookie.Expires = DateTime.Now.AddYears(-1);
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
            if (HttpContext.Current.Request.Cookies["reactivity_password"] != null)
            {
                HttpCookie cookie = new HttpCookie("reactivity_password");
                cookie.Expires = DateTime.Now.AddYears(-1);
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
        }
    }
}
