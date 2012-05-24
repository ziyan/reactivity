using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Reactivity.Objects;
using Reactivity.Util;
using Reactivity.Web.Json;

namespace Reactivity.Web.Handler.Resource
{
    class GetStream : IHttpHandler, System.Web.SessionState.IRequiresSessionState
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

            // Get the data
            System.IO.Stream stream = Common.Client.ResourceGetStream(id);
            if (stream == null)
            {
                context.Response.StatusCode = 404;
                return;
            }

            // allow specify types
            if (type == null || type == "")
                type = "image/jpeg";

            // output
            context.Response.ContentType = type;

            byte[] buffer = new byte[10240];
            while (true)
            {
                int read = stream.Read(buffer, 0, buffer.Length);
                if (read <= 0) break;
                byte[] output = new byte[read];
                Array.Copy(buffer, output, output.Length);
                context.Response.BinaryWrite(output);
            }
            context.Response.Flush();
        }
    }
}


