namespace Robo_Tom.Utils;

public static class Tools
{
    public static Task RunInBackGround(Func<Task> task)
    {
        Task.Run(task).ContinueWith(t =>
        {
            if (t.IsFaulted)
                throw t.Exception!;
        });
        return Task.CompletedTask;
    }
}