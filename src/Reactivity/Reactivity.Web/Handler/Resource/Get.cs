using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Reactivity.Objects;
using Reactivity.Util;
using Reactivity.Web.Json;

namespace Reactivity.Web.Handler.Resource
{
    class Get : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public bool IsReusable { get { return true; } }
        public void ProcessRequest(HttpContext context)
        {
            string guid = context.Request["guid"];
            string type = context.Request["type"];
            
            //check login
            if (!Common.IsUserLoggedIn)
            {
                context.Response.StatusCode = 401;
                return;
            }

            //check input
            if (guid == null || guid == "")
            {
                context.Response.StatusCode = 404;
                return;
            }
            Guid id;
            try { id = new Guid(guid); }
            catch
            {
                context.Response.StatusCode = 400;
                return;
            }

            // get the data
            byte[] buffer = Common.Client.ResourceGet(id);
            if (buffer == null)
            {
                context.Response.StatusCode = 404;
                return;
            }

            // allow specify types
            if (type == null || type == "")
                type = "image/jpeg";

            // output
            context.Response.ContentType = type;
            context.Response.BinaryWrite(buffer);
            context.Response.Flush();
        }
    }
}


