// Dtos/Account/MfaDtos.cs
namespace FZ.Auth.Dtos
{
    public class StartTotpResponse
    {
        public string secretBase32 { get; set; } = default!;
        public string otpauthUri { get; set; } = default!;
        public string? label { get; set; }
    }

    public class ConfirmTotpRequest
    {
        public string code { get; set; } = default!;
    }

    public class DisableMfaRequest
    {
        public string? confirmCode { get; set; } // tuỳ chọn: yêu cầu nhập code để tắt
    }
}


