using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Threading;

namespace Reactivity.Web.Handler.Event
{
    class GetAsyncResult : IAsyncResult
    {
        private HttpContext context;
		private AsyncCallback callback;

		private ManualResetEvent completeEvent = null;
		private object state;
		private object objLock = new object();
		private bool isComplete = false;
		
		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="ctx">Current Http Context</param>
		/// <param name="cb">Callback used by ASP.NET</param>
		/// <param name="d">Data set by the calling thread</param>
        public GetAsyncResult(HttpContext context, AsyncCallback callback, object state)
		{
			this.context = context;
			this.callback = callback;
			this.state = state;
		}

		/// <summary>
		/// Gets the current HttpContext associated with the request
		/// </summary>
		public HttpContext Context
		{
			get { return this.context; }
		}

		/// <summary>
		/// Completes the request and tells the ASP.NET pipeline that the 
		/// execution is complete.
		/// </summary>
		public void Complete()
		{
			isComplete = true;

			// Complete any manually registered events
			lock(objLock)
			{
				if(completeEvent != null)
				{
					completeEvent.Set();
				}
			}

			// Call any registered callback handers
			// (ASP.NET pipeline)
			if (callback != null)
			{
				callback(this);
			}
		}

		#region IAsyncResult Members

		/// <summary>
		/// Gets the object on which one could perform a lock
		/// </summary>
		public object AsyncState
		{
			get { return this.state; }
		}

		/// <summary>
		/// Always returns false
		/// </summary>
		public bool CompletedSynchronously
		{
			get { return false; }
		}

		/// <summary>
		/// Gets a handle that a monitor could lock on.
		/// </summary>
		public WaitHandle AsyncWaitHandle
		{
			get
			{
				lock(objLock)
				{
					if (completeEvent == null)
						completeEvent = new ManualResetEvent(false);

					return completeEvent;
				}
			}
		}

		/// <summary>
		/// Gets the current status of the request
		/// </summary>
		public bool IsCompleted
		{
			get { return this.isComplete; }
		}
		#endregion
    }
}
