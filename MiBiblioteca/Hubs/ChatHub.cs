using Microsoft.AspNetCore.SignalR;
using MiLibreria.Classes;
using System;

namespace MiLibreria.Hubs;

public class ChatHub : Hub<IChat>
{
    #region PROPIEDADES
    public static List<Connection> Conexiones { get; set; } = new List<Connection>();
    public List<string> PalabrasProhibidas { get; set; }= new() { "tonto","feo"};
    #endregion

    #region METODOS
    public async Task SendMessage(Message message)
    {
        if (!string.IsNullOrEmpty(message.Text))
        {
            foreach (string p in PalabrasProhibidas)
            {
                if (message.Text.ToLower().Contains(p))
                {
                    message.Text = $"Al usuario {message.User} se le ha anulado un mensaje por vocabulario inapropiado";
                    break;
                }
            }

            //await Clients.All.GetMessage(message);
            //await Clients.All.GetUsers(Conexiones);
            await Clients.Group(message.Room).GetMessage(message);
        }
        else if (!string.IsNullOrEmpty(message.User))
        {
            Conexiones.Add(new Connection 
            { 
                Id = Context.ConnectionId, 
                User = message.User, 
                AvatarUrl=message.AvatarUrl,
                Room = message.Room
            });
            await Groups.AddToGroupAsync(Context.ConnectionId, message.Room);
            //{ 
            //    User = message.User, 
            //    Text = " se ha conectado!" ,
            //    AvatarUrl = message.AvatarUrl
            //});
            await Clients.GroupExcept(message.Room, Context.ConnectionId)
                .GetMessage(new Message()
                {
                    User = message.User,
                    Text = " Se ha conectado!",
                    AvatarUrl = message.AvatarUrl,
                    Room = message.Room,
                });
            await Clients.All.GetUsers(Conexiones);
        }
    }

    // Sobrescribimos (override) algunos métodos para añadirle algo más de lógica
    public override async Task OnConnectedAsync()
    {
        var conexion = Conexiones.Find(x=> x.Id == Context.ConnectionId);
        // Cuando un usuario se conecta, se le da la bienvenida solo a ese por su id
        await Clients.Client(Context.ConnectionId)
            .GetMessage(new Message() 
            { 
                 User = "Host", 
                 Text = $"Hola {conexion?.User}, Bienvenido al Chat",
                 Room = conexion?.Room ?? string.Empty,
            });

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var conexion = Conexiones.Where(x => x.Id == Context.ConnectionId).FirstOrDefault();
        await Clients.GroupExcept(conexion!.Room, Context.ConnectionId)
            .GetMessage(new Message() 
            { 
                User = "Host", 
                Text = $"{conexion?.User} ha salido del chat",
                Room = conexion?.Room ?? string.Empty,
            });
        if (conexion is not null)
        { 
            Conexiones.Remove(conexion);
        }
        await Clients.All.GetUsers(Conexiones);
        await base.OnDisconnectedAsync(exception);
    }
    #endregion
}
