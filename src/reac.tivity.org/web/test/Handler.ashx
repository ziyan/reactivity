<%@ WebHandler Language="C#" Class="Handler" %>

using System;
using System.Web;

public class Handler : IHttpHandler {
    public static Random rand = new Random();
    public void ProcessRequest (HttpContext context) {
        context.Response.ContentType = "text/plain";
        int sleepy = rand.Next(1000);
        System.Threading.Thread.Sleep(sleepy);
        context.Response.Write(context.Request.QueryString[0] + ": " + sleepy + " " + Guid.NewGuid().ToString());
    }
 
    public bool IsReusable {
        get {
            return false;
        }
    }

}