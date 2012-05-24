using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Reactivity.Objects;
using Reactivity.Util;
using Reactivity.Web.Json;

namespace Reactivity.Web.Handler.Data
{
    class Send : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public bool IsReusable { get { return true; } }
        public void ProcessRequest(HttpContext context)
        {
            
            // Common settings
            context.Response.ContentEncoding = System.Text.Encoding.UTF8;
            context.Response.ContentType = "text/plain";
            context.Response.Expires = -1;

            string device = context.Request["device"];
            string type = context.Request["type"];
            string value = context.Request["value"];

            if (device == null || device == "")
            {
                ObjectCollection collection = new ObjectCollection();
                collection.Add(new StringValue("status"), new StringValue("InvalidDevice"));
                context.Response.Write(collection);
                return;
            }

            if (value == null || value == "")
            {
                ObjectCollection collection = new ObjectCollection();
                collection.Add(new StringValue("status"), new StringValue("InvalidValue"));
                context.Response.Write(collection);
                return;
            }

            if (type == null || type == "")
            {
                ObjectCollection collection = new ObjectCollection();
                collection.Add(new StringValue("status"), new StringValue("InvalidValue"));
                context.Response.Write(collection);
                return;
            }
            Guid device_guid = Guid.Empty;
            try
            {
                device_guid = new Guid(device);
            }
            catch { }
            if (device_guid == Guid.Empty)
            {
                ObjectCollection collection = new ObjectCollection();
                collection.Add(new StringValue("status"), new StringValue("InvalidDevice"));
                context.Response.Write(collection);
                return;
            }

            Objects.Data data = new Objects.Data { Device = device_guid, Timestamp = DateTime.Now };
            try
            {
                switch (type)
                {
                    case "Bool":
                        Util.DataAdapter.Encode(Convert.ToBoolean(value), data);
                        break;
                    case "String":
                        Util.DataAdapter.Encode(value, data);
                        break;
                    case "Double":
                        Util.DataAdapter.Encode(Convert.ToDouble(value), data);
                        break;
                    case "Float":
                        Util.DataAdapter.Encode(Convert.ToSingle(value), data);
                        break;
                    case "Int":
                        Util.DataAdapter.Encode(Convert.ToInt32(value), data);
                        break;
                    case "UInt":
                        Util.DataAdapter.Encode(Convert.ToUInt32(value), data);
                        break;
                    case "Long":
                        Util.DataAdapter.Encode(Convert.ToInt64(value), data);
                        break;
                    case "ULong":
                        Util.DataAdapter.Encode(Convert.ToUInt64(value), data);
                        break;
                    case "Short":
                        Util.DataAdapter.Encode(Convert.ToInt16(value), data);
                        break;
                    case "UShort":
                        Util.DataAdapter.Encode(Convert.ToUInt16(value), data);
                        break;
                    case "Byte":
                        Util.DataAdapter.Encode(Convert.ToByte(Convert.ToInt32(value)), data);
                        break;
                    case "Bytes":
                        {
                            string[] values = value.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries);
                            byte[] bytes = new byte[values.Length];
                            for (int i = 0; i < values.Length; i++)
                                bytes[i] = Convert.ToByte(Convert.ToInt32(values[i].Trim()) & 255);
                            Util.DataAdapter.Encode(bytes, data);
                        }
                        break;
                    default:
                        throw new InvalidCastException();
                }
            }
            catch
            {
                ObjectCollection collection = new ObjectCollection();
                collection.Add(new StringValue("status"), new StringValue("InvalidValue"));
                context.Response.Write(collection);
                return;
            }


            if (!Common.IsUserLoggedIn)
            {
                ObjectCollection collection = new ObjectCollection();
                collection.Add(new StringValue("status"), new StringValue("NotLoggedIn"));
                context.Response.Write(collection);
                return;
            }
            if(!Common.Client.UserHasPermission( Objects.User.PERMISSION_CONTROL))
            {
                ObjectCollection collection = new ObjectCollection();
                collection.Add(new StringValue("status"), new StringValue("PermissionDenied"));
                context.Response.Write(collection);
                return;
            }
            try
            {
                Common.Client.DataSend(data);
                ObjectCollection collection = new ObjectCollection();
                collection.Add(new StringValue("status"), new StringValue("OK"));
                context.Response.Write(collection);
                return;
            }
            catch
            {
                ObjectCollection collection = new ObjectCollection();
                collection.Add(new StringValue("status"), new StringValue("Error"));
                context.Response.Write(collection);
                return;
            }
        }
    }
}


