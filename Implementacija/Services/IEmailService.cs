namespace bibliotecha.Services
{
    public interface IEmailService
    {
        Task<bool> SendAsync(string recipient, string subject, string body, CancellationToken cancellationToken = default);
    }
}
