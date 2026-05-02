namespace DuoCareAPI.Dtos
{
    // Email: solicitud de cambio (manda email con link de confirmación)
    public class RequestEmailChangeDto
    {
        public string NewEmail { get; set; }
    }

    // Email: confirmación (se usa al abrir el link)
    public class ConfirmEmailChangeDto
    {
        public string UserId { get; set; }
        public string NewEmail { get; set; }
        public string Token { get; set; }
    }

    // Nombre (FullName)
    public class ChangeNameDto
    {
        public string NewName { get; set; }
    }

    // Password
    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }

    // Foto: mandamos Base64 para que sea multi-dispositivo
    public class ChangePhotoDto
    {
        public string Base64Image { get; set; }  // puede ser "" para borrar
    }
}