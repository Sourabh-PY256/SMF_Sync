using EWP.SF.KafkaSync.BusinessLayer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace EWP.SF.KafkaSync.Tests;

/// <summary>
/// Covers KafkaService.ProcessMessageWithRetryAsync (made `internal` for testability) in isolation,
/// without a real Kafka broker. This is the retry/backoff behavior the "give up and skip after max
/// retries, don't wedge the partition" dead-letter fix depends on.
/// </summary>
public class KafkaServiceRetryTests : IDisposable
{
    private readonly KafkaService _sut = new(new ConfigurationBuilder().Build(), NullLogger<KafkaService>.Instance);

    public void Dispose() => _sut.Dispose();

    [Fact]
    public async Task ProcessMessageWithRetryAsync_HandlerSucceedsFirstTry_CallsHandlerOnce()
    {
        // Arrange
        int callCount = 0;
        Task Handler(string key, string value)
        {
            callCount++;
            return Task.CompletedTask;
        }

        // Act
        await _sut.ProcessMessageWithRetryAsync("key-1", "value-1", Handler, maxRetries: 3, retryDelayMs: 1);

        // Assert
        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task ProcessMessageWithRetryAsync_HandlerFailsThenSucceeds_RetriesWithinBudgetAndSucceeds()
    {
        // Arrange
        int callCount = 0;
        Task Handler(string key, string value)
        {
            callCount++;
            if (callCount < 3)
            {
                throw new InvalidOperationException("Transient failure");
            }
            return Task.CompletedTask;
        }

        // Act
        await _sut.ProcessMessageWithRetryAsync("key-1", "value-1", Handler, maxRetries: 3, retryDelayMs: 1);

        // Assert - succeeded on the 3rd attempt (2 failures + 1 success), within the retry budget.
        Assert.Equal(3, callCount);
    }

    [Fact]
    public async Task ProcessMessageWithRetryAsync_HandlerAlwaysFails_ThrowsAfterExhaustingRetryBudget()
    {
        // Arrange - a genuinely poison message that never succeeds. The caller (KafkaService.StartConsumer)
        // relies on this throwing so it can commit the offset and dead-letter-log the message instead of
        // looping forever on it.
        int callCount = 0;
        Task Handler(string key, string value)
        {
            callCount++;
            throw new InvalidOperationException("Permanent failure");
        }

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.ProcessMessageWithRetryAsync("key-1", "value-1", Handler, maxRetries: 2, retryDelayMs: 1));

        // maxRetries=2 means 1 initial attempt + 2 retries = 3 total calls before giving up.
        Assert.Equal(3, callCount);
    }
}
