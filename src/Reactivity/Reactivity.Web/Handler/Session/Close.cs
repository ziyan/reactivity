using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Reactivity.Objects;
using Reactivity.Util;
using Reactivity.Web.Json;

namespace Reactivity.Web.Handler.Session
{
    class Close : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public bool IsReusable { get { return true; } }
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentEncoding = System.Text.Encoding.UTF8;
            context.Response.ContentType = "text/plain";
            context.Response.Expires = -1;
            if (Common.IsClientConnected)
                Common.Client.Close();
            Common.Client = null;
            ObjectCollection collection = new ObjectCollection();
            collection.Add(new StringValue("status"), new StringValue("OK"));
            context.Response.Write(collection);
        }
    }
}


