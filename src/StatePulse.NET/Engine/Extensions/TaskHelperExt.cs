namespace StatePulse.Net.Engine.Extensions;
internal static class TaskHelperExt
{
    public static async Task RunWithAsyncLocal<T>(this AsyncLocal<T> asyncLocal, T value, Func<Task> func)
    {
        // Capture current context value
        var original = asyncLocal.Value;
        try
        {
            // Set the AsyncLocal to the desired value
            asyncLocal.Value = value;
            // Await the async function inside the restored AsyncLocal context
            await func();
        }
        finally
        {
            // Restore original value to avoid leaking context
            asyncLocal.Value = original;
        }
    }

}
