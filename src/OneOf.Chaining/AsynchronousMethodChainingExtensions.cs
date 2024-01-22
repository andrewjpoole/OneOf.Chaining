// ReSharper disable InconsistentNaming

namespace OneOf.Chaining;

/// <summary>
/// A set of extension methods which allow chaining of methods in a nice fluent, english sentance like chain or flow.
/// In order to be chained a method need only return a Task Of OneOf TSuccess or TFailure
/// </summary>
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
        
        // Otherwise invoke onFailure...
        var finalFailure = await onFailure(currentT, result.AsT1);

        // and return the final resulting TFailure as long as it is a TFailure!
        return finalFailure.IsT1 ? finalFailure : result.AsT1;
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

        // and return the final resulting TFailure as long as it is a TFailure!
        return finalFailure.IsT1 ? finalFailure : result.AsT1;
    }
    
    /// <summary>
    /// Extension method which enables the result of a chain of methods to be finally converted to a new type (TResult) if all operations in the chain have been successful.<br/>
    /// If at any point in the chain, the result of the currentJob Task has contained a TFailure, then that TFailure which has been passed down through the chain is finally returned.
    /// </summary>
    /// <typeparam name="T">Represents success, also likely contains any state needed to perform any operations and possibly store any results from processing in the chain.</typeparam>
    /// <typeparam name="TFailure">Represents a failure at some point in the chain.</typeparam>
    /// <typeparam name="TResult">The new type to convert the T into.</typeparam>
    /// <param name="currentJobResult">The resulting Task of the previous link in the chain.</param>
    /// <param name="convertToResult">A func provided to do the conversion.</param>
    /// <returns></returns>
    public static async Task<OneOf<TResult, TFailure>> ToResult<TResult, T, TFailure>(this Task<OneOf<T, TFailure>> currentJobResult, Func<T, TResult> convertToResult)
    {
        // Await final job, return cascaded TFailure or new Success.
        var TOrFailure = await currentJobResult;
        return TOrFailure.Match<OneOf<TResult, TFailure>>(
            _ => convertToResult(TOrFailure.AsT0),
            failure => failure);
    }

    /// <summary>
    /// Extension method which enables method chaining. This method accepts an array of tasks which will be executed in parallel. This method will return once all tasks have completed.
    /// Please note, the T is passed into each task by ref, so care must be taken around any mutation of any state on the T, i.e. a property should probably not be mutated by more than one task.
    /// This method cannot know how to merge the result of each task. A strategy can be provided. Another overload of this method provides a default merging strategy.
    /// See the other Then extension methods for an explanation of how to chain a flow of tasks. 
    /// </summary>
    /// <typeparam name="T">Represents success, also likely contains any state needed to perform any operations and possibly store any results from processing in the chain. This will be passed to all tasks.</typeparam>
    /// <typeparam name="TFailure">Represents a failure at some point in the chain.</typeparam>
    /// <param name="currentJobResult">The resulting Task of the previous link in the chain.</param>
    /// <param name="resultMergingStrategy">A func which is passed the original T and a list of results from the Tasks, it should decide how to merge the results once they have all returned i.e. what to return from this method..</param>
    /// <param name="tasks">A list a tasks to execute in parallel, </param>
    /// <returns></returns>
    public static async Task<OneOf<T, TFailure>> ThenWaitForAll<T, TFailure>(
        this Task<OneOf<T, TFailure>> currentJobResult, 
        Func<T, List<OneOf<T, TFailure>>, OneOf<T, TFailure>> resultMergingStrategy, 
        params Func<T, Task<OneOf<T, TFailure>>>[] tasks)
    {
        // Inspect result of (probably already awaited) currentJobResult, if its a TFailure return it...
        var TOrFailure = await currentJobResult;
        if (TOrFailure.IsT1)
            return TOrFailure;

        if (tasks.Length == 0)
            return TOrFailure;

        var taskResults = new List<OneOf<T, TFailure>>();
        await Task.WhenAll(tasks.Select(async task =>
        {
            taskResults.Add(await task(TOrFailure.AsT0));
        }));
        
        var finalResult = resultMergingStrategy(TOrFailure.AsT0, taskResults);

        return finalResult;
    }

    /// <summary>
    /// Extension method which enables method chaining. This method accepts an array of tasks which will be executed in parallel. This method will return once all tasks have completed.
    /// Please note, the T is passed into each task by ref, so care must be taken around any mutation of any state on the T, i.e. a property should probably not be mutated by more than one task.
    /// This method cannot know how to merge the result of each task. A strategy can be provided using an overload of this method. The default behaviour is to return the first TFailure if any tasks return a TFailure, otherwise return the original TOrFailure passed into this method.
    /// See the other Then extension methods for an explanation of how to chain a flow of tasks. 
    /// </summary>
    /// <typeparam name="T">Represents success, also likely contains any state needed to perform any operations and possibly store any results from processing in the chain. This will be passed to all tasks.</typeparam>
    /// <typeparam name="TFailure">Represents a failure at some point in the chain.</typeparam>
    /// <param name="currentJobResult">The resulting Task of the previous link in the chain.</param>
    /// <param name="tasks">A list a tasks to execute in parallel, </param>
    /// <returns></returns>
    public static async Task<OneOf<T, TFailure>> ThenWaitForAll<T, TFailure>(
        this Task<OneOf<T, TFailure>> currentJobResult, 
        params Func<T, Task<OneOf<T, TFailure>>>[] tasks)
    {
        static OneOf<T, TFailure> DefaultResultMergingStrategy(T inputT, List<OneOf<T, TFailure>> results)
        {
            return results.Any(x => x.IsT1) ? results.First(x => x.IsT1) : inputT;
        }

        return await currentJobResult.ThenWaitForAll(DefaultResultMergingStrategy, tasks);
    }

    /// <summary>
    /// Extension method which enables method chaining. This method accepts an array of tasks which will be executed in parallel. This method will return immediately once the first task has completed.
    /// The result (T or TFailure) of the first completed task will be returned. The results of all other tasks are ignored.
    /// Please note, the T is passed into each task by ref, so care must be taken around any mutation of any state on the T, i.e. a property should probably not be mutated by more than one task.
    /// See the other Then extension methods for an explanation of how to chain a flow of tasks.
    /// </summary>
    /// <typeparam name="T">Represents success, also likely contains any state needed to perform any operations and possibly store any results from processing in the chain. This will be passed to all tasks.</typeparam>
    /// <typeparam name="TFailure">Represents a failure at some point in the chain.</typeparam>
    /// <param name="currentJobResult">The resulting Task of the previous link in the chain.</param>
    /// <param name="tasks">A list a tasks to execute in parallel, </param>
    /// <returns></returns>
    public static async Task<OneOf<T, TFailure>> ThenWaitForFirst<T, TFailure>(
    this Task<OneOf<T, TFailure>> currentJobResult, params Func<T, Task<OneOf<T, TFailure>>>[] tasks)
    {
        // Inspect result of (probably already awaited) currentJobResult, if its a TFailure return it...
        var TOrFailure = await currentJobResult;
        if (TOrFailure.IsT1)
            return TOrFailure;

        if (tasks.Length == 0)
            return TOrFailure;
        
        return await await Task.WhenAny(tasks.Select(async task => await task(TOrFailure.AsT0)));
    }
}