using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Reactivity.Objects;
using Reactivity.Util;
using Reactivity.Web.Json;

namespace Reactivity.Web.Handler.Device
{
    class List : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public bool IsReusable { get { return true; } }
        public void ProcessRequest(HttpContext context)
        {
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
            Objects.Device[] devices = Common.Client.DeviceList();
            if (devices == null)
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
                ArrayCollection devicesCollection = new ArrayCollection();
                for (int i = 0; i < devices.Length; i++)
                {
                    Util.DeviceProfileAdapter profile = Util.DeviceProfileAdapter.CreateAdapter(devices[i].Profile);
                    if (!profile.IsValid) continue;
                    ObjectCollection deviceCollection = new ObjectCollection();
                    deviceCollection.Add(new StringValue("guid"), new StringValue(devices[i].Guid.ToString()));
                    deviceCollection.Add(new StringValue("name"), new StringValue(devices[i].Name));
                    deviceCollection.Add(new StringValue("description"), new StringValue(devices[i].Description));
                    deviceCollection.Add(new StringValue("type"), new StringValue(devices[i].Type.ToString()));
                    deviceCollection.Add(new StringValue("status"), new StringValue(devices[i].Status.ToString()));
                    deviceCollection.Add(new StringValue("building"), new StringValue(profile.Building.ToString()));
                    deviceCollection.Add(new StringValue("floor"), new NumberValue(profile.Floor));
                    deviceCollection.Add(new StringValue("x"), new NumberValue(profile.X));
                    deviceCollection.Add(new StringValue("y"), new NumberValue(profile.Y));
                    deviceCollection.Add(new StringValue("z"), new NumberValue(profile.Z));
                    devicesCollection.Add(deviceCollection);
                }
                collection.Add(new StringValue("devices"), devicesCollection);
                context.Response.Write(collection);
                return;
            }
        }
    }
}


