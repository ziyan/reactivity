using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Reactivity.Objects;
using Reactivity.Util;
using Reactivity.Web.Json;

namespace Reactivity.Web.Handler.Event
{
    class GetAsyncRequest
    {
        private GetAsyncResult result;
		
		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="ar">Reponse result on which to perform operations.</param>
        public GetAsyncRequest(GetAsyncResult result)
		{
            this.result = result;
		}

		/// <summary>
		/// Gets the current IAsyncResult object associated with the request.
		/// </summary>
        public GetAsyncResult Result
		{
			get { return this.result; }
		}

		/// <summary>
		/// Uses the IAsyncResult object to call a web service and writes
		/// back the response to the caller
		/// </summary>
		public void Process()
		{
			try
			{
                DoWork();
			}
			finally
			{
				// Tell ASP.NET that the request is complete
				Result.Complete();
			}
		}
        private void DoWork()
        {
            HttpContext context = Result.Context;
            HttpContext.Current = context;

            // Common settings
            context.Response.ContentEncoding = System.Text.Encoding.UTF8;
            context.Response.ContentType = "text/plain";
            context.Response.Expires = -1;

            if (!Common.IsUserLoggedIn)
            {
                ObjectCollection collection = new ObjectCollection();

                collection.Add(new StringValue("status"), new StringValue("NotLoggedIn"));
                context.Response.Write(collection);
                return;
            }

            ClientEvent[] events = Common.Client.ClientEventGet(0);
            if (events == null)
            {
                ObjectCollection collection = new ObjectCollection();

                collection.Add(new StringValue("status"), new StringValue("Error"));
                context.Response.Write(collection);
                return;
            }
            else
            {
                ObjectCollection collection = new ObjectCollection();

                collection.Add(new StringValue("status"), new StringValue("OK"));
                ArrayCollection eventArray = new ArrayCollection();
                for (int i = 0; i < events.Length; i++)
                {
                    ClientEvent e = events[i];
                    ObjectCollection ec = new ObjectCollection();
                    ec.Add(new StringValue("type"), new StringValue(e.Type.ToString()));
                    eventArray.Add(ec);
                }
                collection.Add(new StringValue("events"), eventArray);
                context.Response.Write(collection);
                return;
            }
        }
    }
}
