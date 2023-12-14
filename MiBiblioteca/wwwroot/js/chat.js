const conexion = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();
const txtSala = document.getElementById("txtSala");
// Recibimos el mensaje del hub
conexion.on("GetMessage", (message) => {
    const li = document.createElement("li");

    li.innerHTML = `<img src="${message.avatarUrl}" alt="Avatar de ${message.user}" heigth="25" width="25"/> ${message.user} - ${message.text}`;

    document.getElementById("lstMensajes").appendChild(li);
});
conexion.on("GetUsers", (users) => {
    document.getElementById("lstUsuarios").innerHTML = "";
    document.getElementById("lstUsuariosTotales").innerHTML = "";

    users.forEach(x => {
        const li = document.createElement("li");
        li.innerHTML = `<img src="${x.avatarUrl}" alt="Avatar de ${x.user}" heigth="25" width="25"/> ${x.user}`;
        console.log(txtSala.value)
        console.log(x.room)
        if (x.room === txtSala.value)
        {
            document.getElementById("lstUsuarios").appendChild(li);
            console.log("soy el if",li)
        }

        document.getElementById("lstUsuariosTotales").appendChild(li);
    })
});

document.getElementById("txtUsuario").addEventListener("input", (event) => {
    document.getElementById("btnConectar").disabled = event.target.value === "";
});

document.getElementById("txtMensaje").addEventListener("input", (event) => {
    document.getElementById("btnEnviar").disabled = event.target.value === "";
});
// aqui tengo que hacer otro como el de arriba invocando al elemento txtSala para el boton conectar (btnConectar) 
//document.getElementById("txtSala").addEventListener("select", (event) => {
//    document.getElementById("btnConectar").disabled = event.target.value === "";
//});
document.getElementById("btnConectar").addEventListener("click", (event) => {
    if (conexion.state === signalR.HubConnectionState.Disconnected) {
        const sala = txtSala.value;
        if (sala === "Elige sala") { alert("Elige una sala para conectarte") }
        else {
            conexion.start().then(() => {
                const li = document.createElement("li");
                li.textContent = "Conectado con el servidor en tiempo real";
                document.getElementById("lstMensajes").appendChild(li);
                document.getElementById("btnConectar").textContent = "Desconectar";
                document.getElementById("txtUsuario").disabled = true;
                document.getElementById("txtMensaje").disabled = false;
                txtSala.disabled = true;
                //document.getElementById("btnEnviar").disabled = false;
                const avatar = document.getElementById("avatarUsuario").value;
                const usuario = document.getElementById("txtUsuario").value;

                const message = {
                    user: usuario,
                    ...(avatar !== "" && { avatarUrl: avatar}),
                    text: "",
                    room: txtSala.value,
                }

                conexion.invoke("SendMessage", message).catch(function (error) {
                    console.error(error);
                });

            }).catch(function (error) {
                console.error(error);
            });
        }
    }

    else if (conexion.state === signalR.HubConnectionState.Connected) {
        conexion.stop();

        const li = document.createElement("li");
        li.textContent = "Has salido del chat";
        document.getElementById("lstMensajes").appendChild(li);

        document.getElementById("btnConectar").textContent = "Conectar";
        document.getElementById("txtUsuario").disabled = false;
        document.getElementById("txtMensaje").disabled = true;
        document.getElementById("btnEnviar").disabled = true;
        txtSala.disabled = false;

        document.getElementById("lstMensajes").innerHTML = "";
        document.getElementById("lstUsuarios").innerHTML = "";
    }
});

document.getElementById("btnEnviar").addEventListener("click", (event) =>
    {
    const usuario = document.getElementById("txtUsuario").value;
    const texto = document.getElementById("txtMensaje").value;
    const avatar = document.getElementById("avatarUsuario").value;
    const data = {
        user: usuario,
        ...(avatar !== "" && { avatarUrl: avatar }),
        text: texto,
        room: txtSala.value, 
    }

    // invoke nos va a comunicar con el hub y el evento para pasarle el mensaje
    conexion.invoke("SendMessage", data).catch((error) => {
        console.error(error);
    });
    document.getElementById("txtMensaje").value = "";
    document.getElementById("btnEnviar").disabled = true;
    event.preventDefault(); // Para evitar el submit
})