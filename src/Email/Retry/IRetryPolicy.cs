namespace BeChallenge.Email.Retry
{
    public interface IRetryPolicy
    {
        Task ExecuteAsync(Func<Task> action, CancellationToken ct = default);
    }
}
