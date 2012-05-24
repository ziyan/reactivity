using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Reactivity.Objects;
using Reactivity.Util;
using Reactivity.Web.Json;

namespace Reactivity.Web.Handler.Resource
{
    class GetIndex : IHttpHandler, System.Web.SessionState.IRequiresSessionState
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
            else
            {
                ResourceAdapter adapter = ResourceAdapter.CreateAdapter(Common.Client.ResourceGetIndex());
                if (!adapter.IsValid)
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

                    ArrayCollection buildingsArray = new ArrayCollection();
                    for (int i = 0; i < adapter.Buildings.Length; i++)
                    {
                        Building building = adapter.Buildings[i];
                        ObjectCollection buildingCollection = new ObjectCollection();
                        buildingCollection.Add(new StringValue("guid"), new StringValue(building.Guid.ToString()));
                        buildingCollection.Add(new StringValue("name"), new StringValue(building.Name));
                        buildingCollection.Add(new StringValue("description"), new StringValue(building.Description));
                        buildingCollection.Add(new StringValue("longitude"), new NumberValue(building.Longitude));
                        buildingCollection.Add(new StringValue("latitude"), new NumberValue(building.Latitude));
                        buildingCollection.Add(new StringValue("altitude"), new NumberValue(building.Altitude));
                        Floor[] floors = building.Floors;
                        if (floors != null)
                        {
                            ArrayCollection floorsArray = new ArrayCollection();
                            for (int j = 0; j < floors.Length; j++)
                            {
                                Floor floor = floors[j];
                                ObjectCollection floorCollection = new ObjectCollection();
                                floorCollection.Add(new StringValue("name"), new StringValue(floor.Name));
                                floorCollection.Add(new StringValue("description"), new StringValue(floor.Description));
                                floorCollection.Add(new StringValue("level"), new NumberValue(floor.Level));
                                floorCollection.Add(new StringValue("resource"), new StringValue(floor.Resource.ToString()));
                                floorsArray.Add(floorCollection);
                            }
                            buildingCollection.Add(new StringValue("floors"), floorsArray);
                        }
                        
                        buildingsArray.Add(buildingCollection);
                    }
                    collection.Add(new StringValue("buildings"), buildingsArray);
                    context.Response.Write(collection.ToString());
                    
                    return;
                }
            }
        }
    }
}


