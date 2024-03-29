// ReSharper disable InconsistentNaming

namespace OneOf.Chaining;

/// <summary>
/// A set of extension methods which allow chaining of methods in a nice fluent, english sentence like chain or flow.
/// In order to be chained a method need only return a Task Of OneOf Ts or Tf
/// </summary>
public static class AsynchronousMethodChainingExtensions
{
    /// <summary>
    /// Extension method which enables method chaining. Each method in the chain must accept a Ts and return a Task of OneOf of Ts or Tf, where Ts == Success and Tf == Failure.<br/>
    /// The result of the previousJobResult Task is evaluated and if it contains a Ts (signifying success), the result is passed to the nextJob Func to be executed.<br/>
    /// If at any point in the chain, the result of the previousJobResult Task contains a Tf, then that Tf is immediately passed through the chain, i.e. no more nextJob Func's are executed.
    /// </summary>
    /// <typeparam name="Ts">Represents success, also likely contains any state needed to perform any operations and possibly store any results from processing in the chain.</typeparam>
    /// <typeparam name="Tf">Represents a failure at some point in the chain.</typeparam>
    /// <param name="previousJobResult">The resulting Task of the previous link in the chain.</param>
    /// <param name="nextJob">A Func containing the next piece of work in the chain.</param>
    /// <returns>A Task of OneOf Ts or Tf, which enables these extension methods to form a chain.</returns>
    public static async Task<OneOf<Ts, Tf>> Then<Ts, Tf>(
        this Task<OneOf<Ts, Tf>> previousJobResult,
        Func<Ts, Task<OneOf<Ts, Tf>>> nextJob)
    {
        // Inspect result of (probably already awaited) previousJobResult, if it's a Tf return it...
        var TOrFailure = await previousJobResult;
        if (TOrFailure.IsT1)
            return TOrFailure;

        // ...Otherwise, do your next job
        return await nextJob(TOrFailure.AsT0);
    }

    /// <summary>
    /// Extension method which enables method chaining. Each method in the chain must accept a Ts and return a Task of OneOf of Ts or Tf, where Ts == Success and Tf == Failure.<br/>
    /// The result of the previousJobResult Task is evaluated and if it contains a  Ts (signifying success), the result is passed to the nextJob Func to be executed.<br/>
    /// If at any point in the chain, the result of the previousJobResult Task contains a Tf, then that Tf is immediately passed through the chain, i.e. no more nextJob Func's are executed.
    /// This overload has an OnFailure func (which will be run if nextJob returns a Tf) which is able to mutate the Tf that will be returned.
    /// </summary>
    /// <typeparam name="Ts">Represents success, also likely contains any state needed to perform any operations and possibly store any results from processing in the chain.</typeparam>
    /// <typeparam name="Tf">Represents a failure at some point in the chain.</typeparam>
    /// <param name="previousJobResult">The resulting Task of the previous link in the chain.</param>
    /// <param name="nextJob">A Func containing the next piece of work in the chain.</param>
    /// <param name="onFailure">A func which will be invoked if nextJob returns a Tf, which receives said Tf and can determine what Tf is returned down the chain.</param>
    /// <returns>A Task of OneOf Ts or Tf, which enables these extension methods to form a chain.</returns>
    public static async Task<OneOf<Ts, Tf>> Then<Ts, Tf>(
        this Task<OneOf<Ts, Tf>> previousJobResult,
        Func<Ts, Task<OneOf<Ts, Tf>>> nextJob,
        Func<Ts, Tf, Task<OneOf<Ts,Tf>>> onFailure)
    {
        // Inspect result of (probably already awaited) previousJobResult, if it's a Tf return it...
        var TOrFailure = await previousJobResult;
        if (TOrFailure.IsT1)
            return TOrFailure;

        // ...Otherwise, do your next job ... 
        var currentTs = TOrFailure.AsT0;
        var result = await nextJob(currentTs);

        // If next job returned a  Ts (Success) return it...
        if (result.IsT0)
            return result;
        
        // Otherwise invoke onFailure...
        var finalFailure = await onFailure(currentTs, result.AsT1);

        // and return the final resulting Tf as long as it is a Tf!
        return finalFailure.IsT1 ? finalFailure : result.AsT1;
    }

    /// <summary>
    /// Extension method which enables method chaining. Each method in the chain must accept a Ts and return a Task of OneOf of Ts or Tf, where Ts == Success and Tf == Failure.<br/>
    /// The result of the previousJobResult Task is evaluated and if it contains a  Ts (signifying success), the result is passed to the nextJob Func to be executed.<br/>
    /// If at any point in the chain, the result of the previousJobResult Task contains a Tf, then that Tf is immediately passed through the chain, i.e. no more nextJob Func's are executed.
    /// This overload takes a func which will be invoked on failure before returning the Failure as normal, the func is passed the  Ts and the Tf and can mutate the Tf that will be returned.
    /// </summary>
    /// <typeparam name="Ts">Represents success, also likely contains any state needed to perform any operations and possibly store any results from processing in the chain.</typeparam>
    /// <typeparam name="Tf">Represents a failure at some point in the chain.</typeparam>
    /// <param name="previousJobResult">The resulting Task of the previous link in the chain.</param>
    /// <param name="condition">This func will be invoked first, only if bool True is returned will nextJob be invoked, otherwise the current  Ts will be passed to the next link in the chain.</param>
    /// <param name="nextJob">A Func containing the next piece of work in the chain.</param>
    /// <param name="onFailure">A Func which will be invoked if nextJob returns a Tf, which receives said Tf and can determine what Tf is returned down the chain.</param>
    /// <returns>A Task of OneOf Ts or Tf, which enables these extension methods to form a chain.</returns>
    public static async Task<OneOf<Ts, Tf>> IfThen<Ts, Tf>(
        this Task<OneOf<Ts, Tf>> previousJobResult,
        Func<Ts, bool> condition,
        Func<Ts, Task<OneOf<Ts, Tf>>> nextJob,
        Func<Ts, Tf, Task<OneOf<Ts, Tf>>>? onFailure = null)
    {
        // Inspect result of (probably already awaited) previousJobResult, if it's a Tf return it...
        var TOrFailure = await previousJobResult;
        if (TOrFailure.IsT1)
            return TOrFailure;

        // Otherwise invoke the condition to decide whether to skip...
        var currentTs = TOrFailure.AsT0;
        if (condition(currentTs) is false)
            return currentTs;

        // Or run the next job...
        var result = await nextJob(currentTs);

        // Return if successful
        if (result.IsT0)
            return result;

        // Otherwise return failure...
        if (onFailure is null)
            return result;

        // Unless we have an OnFailure job to do...
        var finalFailure = await onFailure(currentTs, result.AsT1);

        // and return the final resulting Tf as long as it is a Tf!
        return finalFailure.IsT1 ? finalFailure : result.AsT1;
    }

    /// <summary>
    /// Extension method which enables the result of a chain of methods to be finally converted to a new type (TResult) if all operations in the chain have been successful.<br/>
    /// If at any point in the chain, the result of the previousJobResult Task has contained a Tf (a Failure), then that Failure which has been passed down through the chain is finally returned.
    /// </summary>
    /// <typeparam name="Ts">Represents success, also likely contains any state needed to perform any operations and possibly store any results from processing in the chain.</typeparam>
    /// <typeparam name="Tf">Represents a failure at some point in the chain.</typeparam>
    /// <typeparam name="TResult">The new type to convert the  Ts into.</typeparam>
    /// <param name="previousJobResult">The resulting Task of the previous link in the chain.</param>
    /// <param name="convertToResult">A func provided to do the conversion.</param>
    /// <returns>A Task of OneOf TResult or Tf, where the TResult has been converted from T.</returns>
    public static async Task<OneOf<TResult, Tf>> ToResult<TResult, Ts, Tf>(this Task<OneOf<Ts, Tf>> previousJobResult, Func<Ts, TResult> convertToResult)
    {
        // Await final job, return cascaded Tf or new Success.
        var TOrFailure = await previousJobResult;
        return TOrFailure.Match<OneOf<TResult, Tf>>(
            _ => convertToResult(TOrFailure.AsT0),
            failure => failure);
    }

    /// <summary>
    /// Extension method which enables method chaining. This method accepts an array of tasks which will be executed in parallel. This method will return once all tasks have completed.
    /// Ts == Success and Tf == Failure.
    /// Please note, the  Ts is passed into each task by ref, so care must be taken around any mutation of any state on the Ts, i.e. a property should probably not be mutated by more than one task.
    /// This method cannot know how to merge the result of each task. A strategy can be provided. Another overload of this method provides a default merging strategy.
    /// See the other 'Then' extension methods for an explanation of how to chain a flow of tasks. 
    /// </summary>
    /// <typeparam name="Ts">Represents success, also likely contains any state needed to perform any operations and possibly store any results from processing in the chain.
    /// This will be passed to all tasks.</typeparam>
    /// <typeparam name="Tf">Represents a failure at some point in the chain.</typeparam>
    /// <param name="previousJobResult">The resulting Task of the previous link in the chain.</param>
    /// <param name="resultMergingStrategy">A func which is passed the original  Ts and a list of results from the Tasks,
    /// it should decide how to merge the results once they have all returned i.e. what to return from this method.</param>
    /// <param name="tasks">A list a tasks to execute in parallel, </param>
    /// <returns>A Task of OneOf Ts or Tf, which enables these extension methods to form a chain.</returns>
    public static async Task<OneOf<Ts, Tf>> ThenWaitForAll<Ts, Tf>(
        this Task<OneOf<Ts, Tf>> previousJobResult, 
        Func<Ts, List<OneOf<Ts, Tf>>, OneOf<Ts, Tf>> resultMergingStrategy, 
        params Func<Ts, Task<OneOf<Ts, Tf>>>[] tasks)
    {
        // Inspect result of (probably already awaited) previousJobResult, if it's a Tf return it...
        var TOrFailure = await previousJobResult;
        if (TOrFailure.IsT1)
            return TOrFailure;

        if (tasks.Length == 0)
            return TOrFailure;

        var taskResults = new List<OneOf<Ts, Tf>>();
        await Task.WhenAll(tasks.Select(async task =>
        {
            taskResults.Add(await task(TOrFailure.AsT0));
        }));
        
        var finalResult = resultMergingStrategy(TOrFailure.AsT0, taskResults);

        return finalResult;
    }

    /// <summary>
    /// Extension method which enables method chaining. This method accepts an array of tasks which will be executed in parallel. This method will return once all tasks have completed.
    /// Ts == Success and Tf == Failure.
    /// Please note, the Ts is passed into each task by ref, so care must be taken around any mutation of any state on the Ts, i.e. a property should probably not be mutated by more than one task.
    /// This method cannot know how to merge the result of each task. A strategy can be provided using an overload of this method.
    /// The default behaviour is to return the first Tf if any tasks return a Tf, otherwise return the original TOrFailure passed into this method.
    /// See the other 'Then' extension methods for an explanation of how to chain a flow of tasks. 
    /// </summary>
    /// <typeparam name="Ts">Represents success, also likely contains any state needed to perform any operations and possibly store any results from processing in the chain.
    /// This will be passed to all tasks.</typeparam>
    /// <typeparam name="Tf">Represents a failure at some point in the chain.</typeparam>
    /// <param name="previousJobResult">The resulting Task of the previous link in the chain.</param>
    /// <param name="tasks">A list a tasks to execute in parallel, </param>
    /// <returns>A Task of OneOf Ts or Tf, which enables these extension methods to form a chain.</returns>
    public static async Task<OneOf<Ts, Tf>> ThenWaitForAll<Ts, Tf>(
        this Task<OneOf<Ts, Tf>> previousJobResult, 
        params Func<Ts, Task<OneOf<Ts, Tf>>>[] tasks)
    {
        static OneOf<Ts, Tf> DefaultResultMergingStrategy(Ts inputT, List<OneOf<Ts, Tf>> results)
        {
            return results.Any(x => x.IsT1) ? results.First(x => x.IsT1) : inputT;
        }

        return await previousJobResult.ThenWaitForAll(DefaultResultMergingStrategy, tasks);
    }

    /// <summary>
    /// Extension method which enables method chaining. This method accepts an array of tasks which will be executed in parallel. This method will return immediately once the first task has completed.
    /// Ts == Success and Tf == Failure
    /// The result (Ts or Tf) of the first completed task will be returned. The results of all other tasks are ignored.
    /// Please note, the Ts is passed into each task by ref, so care must be taken around any mutation of any state on the Ts, i.e. a property should probably not be mutated by more than one task.
    /// See the other 'Then' extension methods for an explanation of how to chain a flow of tasks.
    /// </summary>
    /// <typeparam name="Ts">Represents success, also likely contains any state needed to perform any operations and possibly store any results from processing in the chain.
    /// This will be passed to all tasks.</typeparam>
    /// <typeparam name="Tf">Represents a failure at some point in the chain.</typeparam>
    /// <param name="previousJobResult">The resulting Task of the previous link in the chain.</param>
    /// <param name="tasks">A list a tasks to execute in parallel, </param>
    /// <returns>A Task of OneOf Ts or Tf, which enables these extension methods to form a chain.</returns>
    public static async Task<OneOf<Ts, Tf>> ThenWaitForFirst<Ts, Tf>(
    this Task<OneOf<Ts, Tf>> previousJobResult, params Func<Ts, Task<OneOf<Ts, Tf>>>[] tasks)
    {
        // Inspect result of (probably already awaited) previousJobResult, if it's a Tf return it...
        var TOrFailure = await previousJobResult;
        if (TOrFailure.IsT1)
            return TOrFailure;

        if (tasks.Length == 0)
            return TOrFailure;
        
        return await await Task.WhenAny(tasks.Select(async task => await task(TOrFailure.AsT0)));
    }
}

