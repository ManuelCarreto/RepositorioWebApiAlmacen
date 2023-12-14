using MiLibreria.Classes;

namespace MiLibreria.Hubs
{
    public interface IChat
    {
        Task GetMessage(Message message);
        Task GetUsers(List<Connection> connections);
    }
}

