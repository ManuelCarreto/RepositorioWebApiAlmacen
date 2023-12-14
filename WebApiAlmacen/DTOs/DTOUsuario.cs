namespace WebApiAlmacen.DTOs
{
    public class DTOUsuario
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class DTOUsuarioLinkChangePassword
    {
        public string Email { get; set; }
    }

    public class DTOUsuarioChangePassword
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Enlace { get; set; }
    }
}
