using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Reactivity.Objects;
using Reactivity.Util;
using Reactivity.Web.Json;

namespace Reactivity.Web.Handler.Event
{
    class Get : IHttpHandler, System.Web.SessionState.IRequiresSessionState
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
                    switch (e.Type)
                    {
                        case ClientEventType.DeviceCreation:
                        case ClientEventType.DeviceUpdate:
                        case ClientEventType.DeviceRemoval:
                        case ClientEventType.SubscriptionNotification:
                        case ClientEventType.SubscriptionUpdate:
                        case ClientEventType.UserUpdate:
                        case ClientEventType.UserLogout:
                            break;
                        default:
                            continue;
                    }
                    ObjectCollection eventCollection = new ObjectCollection();
                    eventCollection.Add(new StringValue("type"), new StringValue(e.Type.ToString()));
                    if (e.Timestamp != null)
                        eventCollection.Add(new StringValue("timestamp"), new DateValue(e.Timestamp));
                    if (e.Type == ClientEventType.DeviceCreation || e.Type == ClientEventType.DeviceUpdate)
                    {
                        if (e.Device == null || e.Device.Guid == Guid.Empty) continue;
                        Util.DeviceProfileAdapter profile = Util.DeviceProfileAdapter.CreateAdapter(e.Device.Profile);
                        if (!profile.IsValid) continue;
                        ObjectCollection deviceCollection = new ObjectCollection();
                        deviceCollection.Add(new StringValue("guid"), new StringValue(e.Device.Guid.ToString()));
                        deviceCollection.Add(new StringValue("name"), new StringValue(e.Device.Name));
                        deviceCollection.Add(new StringValue("description"), new StringValue(e.Device.Description));
                        deviceCollection.Add(new StringValue("type"), new StringValue(e.Device.Type.ToString()));
                        deviceCollection.Add(new StringValue("status"), new StringValue(e.Device.Status.ToString()));
                        deviceCollection.Add(new StringValue("building"), new StringValue(profile.Building.ToString()));
                        deviceCollection.Add(new StringValue("floor"), new NumberValue(profile.Floor));
                        deviceCollection.Add(new StringValue("x"), new NumberValue(profile.X));
                        deviceCollection.Add(new StringValue("y"), new NumberValue(profile.Y));
                        deviceCollection.Add(new StringValue("z"), new NumberValue(profile.Z));
                        eventCollection.Add(new StringValue("device"), deviceCollection);
                    }
                    else if (e.Type == ClientEventType.DeviceRemoval)
                    {
                        if (e.Device != null && e.Device.Guid != Guid.Empty)
                            eventCollection.Add(new StringValue("guid"), new StringValue(e.Device.Guid.ToString()));
                        else if (e.Guid != Guid.Empty)
                            eventCollection.Add(new StringValue("guid"), new StringValue(e.Guid.ToString()));
                        else
                            continue;
                    }
                    else if (e.Type == ClientEventType.UserUpdate)
                    {
                        if (e.User == null || !e.User.IsValid || e.User.ID != Common.Client.UserCurrent.ID)
                            continue;
                        ObjectCollection userCollection = new ObjectCollection();
                        userCollection.Add(new StringValue("id"), new NumberValue(e.User.ID));
                        userCollection.Add(new StringValue("username"), new StringValue(e.User.Username));
                        userCollection.Add(new StringValue("name"), new StringValue(e.User.Name));
                        userCollection.Add(new StringValue("description"), new StringValue(e.User.Description));
                        userCollection.Add(new StringValue("permission"), new NumberValue(e.User.Permission));
                        eventCollection.Add(new StringValue("user"), userCollection);
                    }
                    else if (e.Type == ClientEventType.UserLogout)
                    {
                        //no argument
                    }
                    else if (e.Type == ClientEventType.SubscriptionUpdate)
                    {
                        if (e.Subscription == null || e.Subscription.Guid == Guid.Empty || e.Subscription.Device == Guid.Empty)
                            continue;
                        ObjectCollection subscriptionCollection = new ObjectCollection();
                        subscriptionCollection.Add(new StringValue("guid"), new StringValue(e.Subscription.Guid.ToString()));
                        subscriptionCollection.Add(new StringValue("device"), new StringValue(e.Subscription.Device.ToString()));
                        subscriptionCollection.Add(new StringValue("status"), new StringValue(e.Subscription.Status.ToString()));
                        eventCollection.Add(new StringValue("subscription"), subscriptionCollection);
                    }
                    else if (e.Type == ClientEventType.SubscriptionNotification)
                    {
                        if (e.Data == null || e.Data.Device == Guid.Empty || e.Guid == Guid.Empty)
                            continue;
                        eventCollection.Add(new StringValue("guid"), new StringValue(e.Guid.ToString()));
                        ObjectCollection dataCollection = new ObjectCollection();
                        dataCollection.Add(new StringValue("device"), new StringValue(e.Data.Device.ToString()));
                        dataCollection.Add(new StringValue("timestamp"), new DateValue(e.Data.Timestamp));
                        dataCollection.Add(new StringValue("service"), new NumberValue(e.Data.Service));
                        dataCollection.Add(new StringValue("type"), new StringValue(e.Data.Type.ToString()));
                        Value value = null;
                        object o = Util.DataAdapter.Decode(e.Data);
                        if (o.GetType() == typeof(int))
                            value = new NumberValue((int)o);
                        else if (o.GetType() == typeof(double))
                            value = new NumberValue((double)o);
                        else if (o.GetType() == typeof(float))
                            value = new NumberValue((float)o);
                        else if (o.GetType() == typeof(bool))
                            value = new BoolValue((bool)o);
                        else if (o.GetType() == typeof(string))
                            value = new StringValue(o.ToString());
                        else if (o.GetType() == typeof(byte))
                            value = new NumberValue((byte)o);
                        else if (o.GetType() == typeof(byte[]))
                        {
                            ArrayCollection array = new ArrayCollection();
                            byte[] bytes = (byte[])o;
                            for (int j = 0; j < bytes.Length; j++)
                                array.Add(new NumberValue(bytes[j]));
                            value = array;
                        }
                        else
                            value = new NumberValue(o.ToString());
                        dataCollection.Add(new StringValue("value"), value);
                        eventCollection.Add(new StringValue("data"), dataCollection);
                    }
                    eventArray.Add(eventCollection);
                }
                collection.Add(new StringValue("events"), eventArray);
                context.Response.Write(collection);
                return;
            }
        }
    }
}


