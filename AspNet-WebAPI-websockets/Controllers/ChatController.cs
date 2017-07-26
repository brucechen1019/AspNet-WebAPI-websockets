using Microsoft.Web.WebSockets;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace AspNet_WebAPI_websockets.Controllers
{
    public class ChatController : ApiController
    {
        public HttpResponseMessage Get(string username)
        {
            HttpContext.Current.AcceptWebSocketRequest(new ChatWebSocketHandler(username));
            return Request.CreateResponse(HttpStatusCode.SwitchingProtocols);
        }

        public class ChatWebSocketHandler : WebSocketHandler
        {
            public static readonly WebSocketCollection _chatClients = new WebSocketCollection();
            public string UserName { get; }

            public ChatWebSocketHandler(string username)
            {
                UserName = username;
            }

            public override void OnOpen()
            {
                _chatClients.Add(this);
                _chatClients.Broadcast($"[system]: {UserName} is online!");
            }

            public override void OnMessage(string message)
            {
                var msgObj=JObject.Parse(message);
                string receiver = msgObj.Value<string>("receiver");
                string msg= msgObj.Value<string>("msg");
                if (string.IsNullOrEmpty(receiver))
                    _chatClients.Broadcast($"[{UserName}]: {msg}");
                else
                {
                    var receiverClient=_chatClients.FirstOrDefault(c => (c as ChatWebSocketHandler).UserName == receiver);
                    if (receiverClient != null)
                        receiverClient.Send($"[{UserName}]: {msg}");
                    else
                        this.Send($"User [{receiver}] is offline.");
                }
            }
        }
    }
}