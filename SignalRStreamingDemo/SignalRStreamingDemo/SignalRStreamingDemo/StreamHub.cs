using Microsoft.AspNetCore.SignalR;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace SignalRStreamingDemo;

public class StreamHub : Hub
{
    // This code was adapted from the following documentation:
    // https://docs.microsoft.com/en-us/aspnet/core/signalr/streaming?view=aspnetcore-6.0

    /* 
      A channel is simply a data structure that’s used to store 
      produced data for a consumer to retrieve, and an appropriate 
      synchronization to enable that to happen safely, while also 
      enabling appropriate notifications in both directions.
      Ref: https://devblogs.microsoft.com/dotnet/an-introduction-to-system-threading-channels/
     
      The Channel method has been available since ASP.NET Core 2.1
     
      ASP.NET Core 3.0 introduced streaming from the client to the server.

      C# 8 introduced asynchronous streaming using IAsyncEnumerable, available
      in .NET Core 3.0 +
     */

    #region "Stream channel to client"
    public ChannelReader<int> GetChannelStream(
        int count,
        int delay,
        CancellationToken cancellationToken)
    {
        var channel = Channel.CreateBounded<int>(10);

        // The underscore _ is a "discard", declaring your intent to
        // ignore the return value. It's the polite thing to do.
        // Ref: https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/functional/discards

        // We don't want to await WriteItemsAsync, otherwise we'd end up waiting 
        // for all the items to be written before returning the channel back to
        // the client.

        _ = WriteItemsAsync(channel.Writer, count, delay, cancellationToken);

        return channel.Reader;
    }

    private async Task WriteItemsAsync(
        ChannelWriter<int> writer,
        int count,
        int delay,
        CancellationToken cancellationToken)
    {
        // Use the "damn-it" operator (!) to appease the compiler
        // Ref: https://codeblog.jonskeet.uk/category/c-8/
        Exception localException = null!;
        try
        {
            for (var i = 0; i < count; i++)
            {
                await writer.WriteAsync(i, cancellationToken);

                // Use the cancellationToken in other APIs that accept cancellation
                // tokens so the cancellation can flow down to them.

                // https://docs.microsoft.com/en-us/dotnet/standard/threading/cancellation-in-managed-threads

                await Task.Delay(delay, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            localException = ex;
        }
        finally
        {
            writer.Complete(localException);
        }
    }
    #endregion

    #region "Stream async stream to client"
    public async IAsyncEnumerable<int> GetAsyncStream(
        int count,
        int delay,
        [EnumeratorCancellation]
        CancellationToken cancellationToken)
    {
        for (var i = 0; i < count; i++)
        {
            // Check the cancellation token regularly so that the server will stop
            // producing items if the client disconnects.
            cancellationToken.ThrowIfCancellationRequested();

            yield return i;

            // Use the cancellationToken in other APIs that accept cancellation
            // tokens so the cancellation can flow down to them.
            await Task.Delay(delay, cancellationToken);
        }
    }
    #endregion

    #region "Receive channel stream from client"
    public async Task UploadStreamToChannel(ChannelReader<string> stream)
    {
        while (await stream.WaitToReadAsync())
        {
            while (stream.TryRead(out var item))
            {
                await Clients.All.SendAsync("ReceiveChannelStreamData", item);
            }
        }
    }
    #endregion

    #region "Receive async stream from client"
    // Streaming from client to server using newer Async Stream
    public async Task UploadAsyncStream(IAsyncEnumerable<string> stream)
    {
        await foreach (var item in stream)
        {
            await Clients.All.SendAsync("ReceiveAsyncStreamData", item);
        }
    }
    #endregion

}