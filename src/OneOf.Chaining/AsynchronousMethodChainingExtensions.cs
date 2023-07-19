using OneOf;
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
    public static async Task<OneOf<T, TFailure>> Then<T, TFailure>(this Task<OneOf<T, TFailure>> currentJobResult, Func<T, Task<OneOf<T, TFailure>>> nextJob)
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
    /// </summary>
    /// <typeparam name="T">Represents success, also likely contains any state needed to perform any operations and possibly store any results from processing in the chain.</typeparam>
    /// <typeparam name="TFailure">Represents a failure at some point in the chain.</typeparam>
    /// <param name="currentJobResult">The resulting Task of the previous link in the chain.</param>
    /// <param name="nextJob">A Func containing the next piece of work in the chain.</param>
    /// <param name="onFailure">An Action which will be invoked if nextJob fails, before TFailure is returned.</param>
    /// <returns></returns>
    public static async Task<OneOf<T, TFailure>> Then<T, TFailure>(this Task<OneOf<T, TFailure>> currentJobResult, Func<T, Task<OneOf<T, TFailure>>> nextJob, Func<T, Task> onFailure)
    {
        // Inspect result of (probably already awaited) currentJobResult, if its a TFailure return it...
        var TOrFailure = await currentJobResult;
        if (TOrFailure.IsT1)
            return TOrFailure;

        // ...Otherwise do your next job ... 
        var currentT = TOrFailure.AsT0;
        var result = await nextJob(currentT);

        if (result.IsT0)
            return result;

        await onFailure(currentT);
        return result;
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
            t => new Success(),
            failure => failure);
    }
}