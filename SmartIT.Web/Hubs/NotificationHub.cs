using Microsoft.AspNetCore.SignalR; namespace SmartIT.Web.Hubs; public class NotificationHub:Hub { public Task Notify(string message)=>Clients.All.SendAsync("notification",message); }
