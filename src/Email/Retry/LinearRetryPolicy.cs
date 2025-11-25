namespace BeChallenge.Email.Retry
{
    public class LinearRetryPolicy(int maxRetries = 3, int baseDelayMs = 500, Func<Exception, bool>? shouldRetry = null) : IRetryPolicy
    {
        private readonly int _maxRetries = maxRetries;
        private readonly int _baseDelayMs = baseDelayMs;
        private readonly Func<Exception, bool>? _shouldRetry = shouldRetry ?? (_ => true);

        public async Task ExecuteAsync(Func<Task> action, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(action);

            for (int attempt = 0; attempt <= _maxRetries; attempt++)
            {
                try
                {
                    await action();
                    return;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    if (!_shouldRetry(ex) || attempt == _maxRetries)
                    {
                        throw;
                    }

                    int delay = _baseDelayMs * (attempt + 1);
                    await Task.Delay(delay, ct);
                }
            }
        }
    }
}
