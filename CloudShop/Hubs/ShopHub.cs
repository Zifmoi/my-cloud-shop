using Microsoft.AspNetCore.SignalR;

namespace CloudShop.Hubs
{
    // Наследуемся от базового класса Hub — это превращает класс в диспетчер WebSocket-соединений
    public class ShopHub : Hub
    {
        // Метод для отправки мгновенных сообщений в живой чат или ленту событий магазина
        public async Task SendAnnouncement(string user, string message)
        {
            // Рассылаем полученное сообщение ВСЕМ подключённым клиентам, вызывая у них JS-функцию "ReceiveMessage"
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}