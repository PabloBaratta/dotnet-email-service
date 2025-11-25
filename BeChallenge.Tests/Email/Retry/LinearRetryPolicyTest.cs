using BeChallenge.Email.Retry;
using BeChallenge.Email;

namespace BeChallenge.Tests.Email.Retry
{
    public class LinearRetryPolicyTest
    {
        [Fact]
        public async Task ExecuteAsyncSucceedsOnFirstAttempt()
        {
            LinearRetryPolicy policy = new(maxRetries: 3, baseDelayMs: 1);
            int calls = 0;

            await policy.ExecuteAsync(() => { calls++; return Task.CompletedTask; });

            Assert.Equal(1, calls);
        }

        [Fact]
        public async Task ExecuteAsyncRetriesUntilSuccess()
        {
            LinearRetryPolicy policy = new(maxRetries: 5, baseDelayMs: 1);
            int calls = 0;
            int fails = 2;

            await policy.ExecuteAsync(() =>
            {
                calls++;
                return calls <= fails ? throw new TransientEmailException("transient") : Task.CompletedTask;
            });

            Assert.Equal(fails + 1, calls);
        }

        [Fact]
        public async Task ExecuteAsyncThrowsAfterMaxRetries()
        {
            LinearRetryPolicy policy = new(maxRetries: 2, baseDelayMs: 1);
            int calls = 0;

            async Task Action()
            {
                calls++;
                await Task.Yield();
                throw new InvalidOperationException("boom");
            }

            InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(() => policy.ExecuteAsync(Action));
            Assert.Equal("boom", ex.Message);
            Assert.Equal(3, calls); // maxRetries + 1
        }

        [Fact]
        public async Task ExecuteAsyncDoesNotRetryWhenPredicateReturnsFalse()
        {
            LinearRetryPolicy policy = new(maxRetries: 5, baseDelayMs: 1, shouldRetry: ex => ex is not InvalidOperationException);
            int calls = 0;

            async Task Action()
            {
                calls++;
                await Task.Yield();
                throw new InvalidOperationException("fatal");
            }

            _ = await Assert.ThrowsAsync<InvalidOperationException>(() => policy.ExecuteAsync(Action));
            Assert.Equal(1, calls);
        }
    }
}

