using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Reactivity.Web
{
    public class HttpModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.PostAcquireRequestState += new EventHandler(context_PostAcquireRequestState);
        }
        void context_PostAcquireRequestState(object sender, EventArgs e)
        {
            if (HttpContext.Current.Request.Path.ToLower() == "/default.aspx")
            {
                if (HttpContext.Current.Request.UserAgent.IndexOf("iPhone") > -1 ||
                    HttpContext.Current.Request.UserAgent.IndexOf("iPod") > -1)
                {
                    HttpContext.Current.Response.Redirect("/iphone/");
                }
                else
                {
                    HttpContext.Current.Response.Redirect("/web/");
                }
            }
        }
        public void Dispose()
        {
        }
    }
}