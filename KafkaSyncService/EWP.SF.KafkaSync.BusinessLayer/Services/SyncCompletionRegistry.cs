using System.Collections.Concurrent;
using EWP.SF.KafkaSync.BusinessEntities;

namespace EWP.SF.KafkaSync.BusinessLayer.Services;

public interface ISyncCompletionRegistry
{
    TaskCompletionSource<DataSyncHttpResponse> Register(string correlationId, CancellationToken ct);
    void Complete(string correlationId, DataSyncHttpResponse response);
}

public class SyncCompletionRegistry : ISyncCompletionRegistry
{
    private readonly ConcurrentDictionary<string, TaskCompletionSource<DataSyncHttpResponse>> _pending
        = new();

    public TaskCompletionSource<DataSyncHttpResponse> Register(string correlationId, CancellationToken ct)
    {
        var tcs = new TaskCompletionSource<DataSyncHttpResponse>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        _pending[correlationId] = tcs;

        ct.Register(() =>
        {
            if (_pending.TryRemove(correlationId, out var task))
                task.TrySetCanceled();
        });

        return tcs;
    }

    public void Complete(string correlationId, DataSyncHttpResponse response)
    {
        if (_pending.TryRemove(correlationId, out var tcs))
        {
            tcs.TrySetResult(response);
        }
    }
}