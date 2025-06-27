using System.Diagnostics;

namespace TestingCommons.Core.Utils;

public static class WaitHelper
{
    /// <summary>
    /// Block the calling thread by calling `Task.Delay(<see cref="sleepPeriodInSeconds"/>)` until the passed predicate evaluate to true based on the result of the passed action
    /// </summary>
    /// <typeparam name="T">The type of the result of the passed action</typeparam>
    /// <param name="action">Action to evaluate each iteration</param>
    /// <param name="predicate">The predicate to evaluate the result of the action for each iteration</param>
    /// <param name="sleepPeriodInSeconds">time to wait before next call to action</param>
    /// <param name="maxTimeoutInSeconds">The maximum amount of time for polling before timeout and throwing exception</param>
    /// <returns></returns>
    [Obsolete("This method is deprecated and will be removed in future versions.")]
    public static async Task<T> PollForAsync<T>(
        Func<Task<T>> action,
        Func<T, bool> predicate,
        int sleepPeriodInSeconds = 1,
        int maxTimeoutInSeconds = 60)
    {
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(maxTimeoutInSeconds));

        while (true)
        {
            cancellationTokenSource.Token.ThrowIfCancellationRequested();
            var actionResult = await Task.Run(action, cancellationTokenSource.Token);
            if (predicate(actionResult))
            {
                return actionResult;
            }
            await Task.Delay(TimeSpan.FromSeconds(sleepPeriodInSeconds), cancellationToken: cancellationTokenSource.Token);
        }
    }

    /// <summary>
    /// Waits until the specified condition is met or the timeout is reached.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="timeout">The maximum time to wait in seconds.</param>
    /// <param name="errorMessage">The error message to throw if the timeout is reached.</param>
    public static void Wait(Func<bool> condition, int timeout, string errorMessage)
    {
        Wait(condition, timeout, errorMessage, _ => false);
    }

    /// <summary>
    /// Waits until the specified condition is met or the timeout is reached asynchronously. 
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="timeout">The maximum time to wait in seconds.</param>
    /// <param name="errorMessage">The error message to throw if the timeout is reached.</param>
    public static async Task WaitAsync(Func<Task<bool>> condition, int timeout, string errorMessage)
    {
        await WaitAsync(condition, timeout, errorMessage, _ => false);
    }

    public static void Wait(Func<bool> condition, int timeout, string errorMessage, Func<Exception, bool> ignoreException)
    {
        if (condition == null)
        {
            throw new ArgumentNullException(nameof(condition));
        }
        var sw = Stopwatch.StartNew();
        var count = 0;

        Console.WriteLine($"Check waiting condition timeout: {timeout} seconds");
        while (sw.Elapsed < TimeSpan.FromSeconds(timeout))
        {
            count++;
            try
            {
                var isCondition = condition();
                if (isCondition)
                {
                    Console.WriteLine($"Succeeded, waiting time: {sw.Elapsed} retry count: {count}");
                    return;
                }
            }
            catch (Exception ex) when (ignoreException(ex))
            {
                Console.WriteLine("Exception occurred: " + ex);
            }
            Thread.Sleep(1000);
        }
        Console.WriteLine($"False, waiting time: {sw.Elapsed} retry count: {count}");
        throw new InvalidOperationException(errorMessage);
    }

    public static async Task WaitAsync(Func<Task<bool>> condition, int timeout, string errorMessage, Func<Exception, bool> ignoreException)
    {
        var sw = Stopwatch.StartNew();
        var count = 0;

        Console.WriteLine($"Check waiting condition timeout: {timeout} seconds");
        while (sw.Elapsed < TimeSpan.FromSeconds(timeout))
        {
            count++;
            try
            {
                Console.WriteLine($"Run condition check count = {count}, total elapsed = {sw.Elapsed}");
                var result = await condition();
                if (result)
                {
                    Console.WriteLine($"Succeeded, waiting time: {sw.Elapsed} retry count: {count}");
                    return;
                }
            }
            catch (Exception ex) when (ignoreException(ex))
            {
                Console.WriteLine("Exception occurred: " + ex);
            }

            await Task.Delay(1000);
        }

        Console.WriteLine($"False, waiting time: {sw.Elapsed} retry count: {count}");
        throw new InvalidOperationException(errorMessage);
    }
}
