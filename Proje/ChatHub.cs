using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace Proje
{
    public class ChatHub : Hub
    {
        public void Send(string username, string message)
        {
            Clients.All.send(username, message);
        }
    }
}