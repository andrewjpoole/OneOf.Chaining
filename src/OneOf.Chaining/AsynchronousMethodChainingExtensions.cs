using OneOf.Types;

// ReSharper disable InconsistentNaming

namespace OneOf.Chaining;

public static class AsynchronousMethodChainingExtensions
{
    /// <summary>
    /// Extension method which enables method chaining. Each method in the chain must accept a T and return a Task of OneOf of T or TFailure.<br/>
    /// The result of the currentJob Task is evaluated and if it contains a T (signifying success), the result is passed to the nextJob Func to be executed.<br/>
    /// If at any point in the chain, the result of the currentJob Task contains a TFailure, then that TFailure is immediately passed through the chain, i.e. no more nextJob Func's are executed.
    /// </summary>
    /// <typeparam name="T">Represents success, also likely contains any state needed to perform any operations and possibly store any results from processing in the chain.</typeparam>
    /// <typeparam name="TFailure">Represents a failure at some point in the chain.</typeparam>
    /// <param name="currentJobResult">The resulting Task of the previous link in the chain.</param>
    /// <param name="nextJob">A Func containing the next piece of work in the chain.</param>
    /// <returns></returns>
    public static async Task<OneOf<T, TFailure>> Then<T, TFailure>(
        this Task<OneOf<T, TFailure>> currentJobResult,
        Func<T, Task<OneOf<T, TFailure>>> nextJob)
    {
        // Inspect result of (probably already awaited) currentJobResult, if its a TFailure return it...
        var TOrFailure = await currentJobResult;
        if (TOrFailure.IsT1)
            return TOrFailure;

        // ...Otherwise do your next job
        var currentT = TOrFailure.AsT0;
        return await nextJob(currentT);
    }

    /// <summary>
    /// Extension method which enables method chaining. Each method in the chain must accept a T and return a Task of OneOf of T or TFailure.<br/>
    /// The result of the currentJob Task is evaluated and if it contains a T (signifying success), the result is passed to the nextJob Func to be executed.<br/>
    /// If at any point in the chain, the result of the currentJob Task contains a TFailure, then that TFailure is immediately passed through the chain, i.e. no more nextJob Func's are executed.
    /// This overload has an OnFailure func (which will be run if nextJob returns a TFailure) which is able to mutate the TFailure that will be returned.
    /// </summary>
    /// <typeparam name="T">Represents success, also likely contains any state needed to perform any operations and possibly store any results from processing in the chain.</typeparam>
    /// <typeparam name="TFailure">Represents a failure at some point in the chain.</typeparam>
    /// <param name="currentJobResult">The resulting Task of the previous link in the chain.</param>
    /// <param name="nextJob">A Func containing the next piece of work in the chain.</param>
    /// <param name="onFailure">A func which will be invoked if nextJob returns a TFailure, which receives said TFailure and can determine what TFailure is returned down the chain.</param>
    /// <returns></returns>
    public static async Task<OneOf<T, TFailure>> Then<T, TFailure>(
        this Task<OneOf<T, TFailure>> currentJobResult,
        Func<T, Task<OneOf<T, TFailure>>> nextJob,
        Func<T, TFailure, Task<OneOf<T,TFailure>>> onFailure)
    {
        // Inspect result of (probably already awaited) currentJobResult, if its a TFailure return it...
        var TOrFailure = await currentJobResult;
        if (TOrFailure.IsT1)
            return TOrFailure;

        // ...Otherwise do your next job ... 
        var currentT = TOrFailure.AsT0;
        var result = await nextJob(currentT);

        // If next job returned a T (Success) return it...
        if (result.IsT0)
            return result;
        
        // Otherwise invoke onFailure and return the final resulting TFailure...
        var finalFailure = await onFailure(currentT, result.AsT1);
        return finalFailure;
    }

    /// <summary>
    /// Extension method which enables method chaining. Each method in the chain must accept a T and return a Task of OneOf of T or TFailure.<br/>
    /// The result of the currentJob Task is evaluated and if it contains a T (signifying success), the result is passed to the nextJob Func to be executed.<br/>
    /// If at any point in the chain, the result of the currentJob Task contains a TFailure, then that TFailure is immediately passed through the chain, i.e. no more nextJob Func's are executed.
    /// This overload takes a func which will be invoked on failure before returning the Failure as normal, the func is passed the T and the TFailure and can mutate the TFailure that will be returned.
    /// </summary>
    /// <typeparam name="T">Represents success, also likely contains any state needed to perform any operations and possibly store any results from processing in the chain.</typeparam>
    /// <typeparam name="TFailure">Represents a failure at some point in the chain.</typeparam>
    /// <param name="currentJobResult">The resulting Task of the previous link in the chain.</param>
    /// <param name="condition">This func will be invoked first, only if bool True is returned will nextJob be invoked, otherwise the current T will be passed to the next link in the chain.</param>
    /// <param name="nextJob">A Func containing the next piece of work in the chain.</param>
    /// <param name="onFailure">An Action which will be invoked if nextJob fails, before TFailure is returned.</param>
    /// <returns></returns>
    public static async Task<OneOf<T, TFailure>> IfThen<T, TFailure>(
        this Task<OneOf<T, TFailure>> currentJobResult,
        Func<T, bool> condition,
        Func<T, Task<OneOf<T, TFailure>>> nextJob,
        Func<T, TFailure, Task<OneOf<T, TFailure>>>? onFailure = null)
    {
        // Inspect result of (probably already awaited) currentJobResult, if its a TFailure return it...
        var TOrFailure = await currentJobResult;
        if (TOrFailure.IsT1)
            return TOrFailure;

        // Otherwise invoke the condition to decide whether to skip...
        var currentT = TOrFailure.AsT0;
        if (condition(currentT) is false)
            return currentT;

        // Or run the next job...
        var result = await nextJob(currentT);

        // Return if successful
        if (result.IsT0)
            return result;

        // Otherwise return failure...
        if (onFailure is null)
            return result;

        // Unless we have an OnFailure job to do...
        var finalFailure = await onFailure(currentT, result.AsT1);
        return finalFailure;
    }

    /// <summary>
    /// Extension method which enables the result of a chain of methods to be finally converted to a OneOf.Success if all operations in the chain have been successful.<br/>
    /// If at any point in the chain, the result of the currentJob Task has contained a TFailure, then that TFailure which has been passed down through the chain is finally returned.
    /// </summary>
    /// <typeparam name="T">Represents success, also likely contains any state needed to perform any operations and possibly store any results from processing in the chain.</typeparam>
    /// <typeparam name="TFailure">Represents a failure at some point in the chain.</typeparam>
    /// <param name="currentJobResult">The resulting Task of the previous link in the chain.</param>
    /// <returns></returns>
    public static async Task<OneOf<Success, TFailure>> ToResult<T, TFailure>(this Task<OneOf<T, TFailure>> currentJobResult)
    {
        // Await final job, return cascaded TFailure or new Success.
        var TOrFailure = await currentJobResult;
        return TOrFailure.Match<OneOf<Success, TFailure>>(
            _ => new Success(),
            failure => failure);
    }

    /// <summary>
    /// Extension method which enables method chaining. This method accepts an array of tasks which will be executed in parallel, the method will return once all tasks have completed.
    /// See the other Then extension methods for an explanation of how to chain a flow of tasks.
    /// </summary>
    /// <typeparam name="T">Represents success, also likely contains any state needed to perform any operations and possibly store any results from processing in the chain. This will be passed to all tasks.</typeparam>
    /// <typeparam name="TFailure">Represents a failure at some point in the chain.</typeparam>
    /// <param name="currentJobResult">The resulting Task of the previous link in the chain.</param>
    /// <param name="tasks">A list a tasks to execute in parallel, </param>
    /// <returns></returns>
    public static async Task<OneOf<T, TFailure>> ThenWaitForAll<T, TFailure>(
        this Task<OneOf<T, TFailure>> currentJobResult, params Func<T, Task>[] tasks)
    {
        // Inspect result of (probably already awaited) currentJobResult, if its a TFailure return it...
        var TOrFailure = await currentJobResult;
        if (TOrFailure.IsT1)
            return TOrFailure;

        if(tasks.Length == 0)
            return TOrFailure;

        await Task.WhenAll(tasks.Select(async task => await task(TOrFailure.AsT0)));

        return TOrFailure;
    }

    /// <summary>
    /// Extension method which enables method chaining. This method accepts an array of tasks which will be executed in parallel but the method will return immediately once the first task has completed.
    /// See the other Then extension methods for an explanation of how to chain a flow of tasks.
    /// </summary>
    /// <typeparam name="T">Represents success, also likely contains any state needed to perform any operations and possibly store any results from processing in the chain. This will be passed to all tasks.</typeparam>
    /// <typeparam name="TFailure">Represents a failure at some point in the chain.</typeparam>
    /// <param name="currentJobResult">The resulting Task of the previous link in the chain.</param>
    /// <param name="tasks">A list a tasks to execute in parallel, </param>
    /// <returns></returns>
    public static async Task<OneOf<T, TFailure>> ThenWaitForFirst<T, TFailure>(
        this Task<OneOf<T, TFailure>> currentJobResult, params Func<T, Task>[] tasks)
    {
        // Inspect result of (probably already awaited) currentJobResult, if its a TFailure return it...
        var TOrFailure = await currentJobResult;
        if (TOrFailure.IsT1)
            return TOrFailure;

        if (tasks.Length == 0)
            return TOrFailure;

        await Task.WhenAny(tasks.Select(async task => await task(TOrFailure.AsT0)));

        return TOrFailure;
    }
}