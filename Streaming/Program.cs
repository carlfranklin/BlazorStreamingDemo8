// Async Streaming uses the Iterator pattern
// (yield return) WITH async!

// Without async streams, you can NOT do yield return
// from an async method.

// Because it's async, your UI can start working
// as soon as the first record arrives.

// async streaming is a PULL operation, not a PUSH.

// Async streaming is NOT about performance.
// It's about scalability.

// display messages using 'await foreach'
await foreach (var msg in GetMessages())
{
    Console.WriteLine(msg.Text);
}

// and we're done
Console.WriteLine("Done");

// IAsyncEnumerable is a ValueTask, not a Task
// ValueTask should be used when lots of data is
// to be streamed and therefore memory needs to 
// be conserved. ValueTask creates 0 Gen 1 allocations
// whereas Task creates Gen 1 allocations.
async IAsyncEnumerable<Message> GetMessages()
{
    // just to have something async in here!
    await Task.Delay(0);

    for (int i = 0; i < 3; i++)
    {
        var msg = new Message() { Text = $"Hello {i}" };
        yield return msg;
    }
}

public class Message
{
    public string? Text { get; set; }
}