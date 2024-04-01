// ReSharper disable InconsistentNaming

namespace OneOf.Chaining;

/// <summary>
/// A set of extension methods which allow chaining of methods in a nice fluent, english sentence like chain or flow.
/// In order to be chained a method need only return a Task Of OneOf Ts or Tf, where Ts == Success and Tf == Failure.
/// </summary>
public static class AsynchronousMethodChainingExtensions
{
    /// <summary>
    /// Extension method which enables method chaining.
    /// Each method in the chain must accept a <typeparamref name="Ts"/> and return a Task&lt;OneOf&lt;<typeparamref name="Ts"/>, <typeparamref name="Tf"/>&gt;&gt;,
    /// where <typeparamref name="Ts"/> == Success and <typeparamref name="Tf"/> == Failure.<br/>
    /// The result of the <paramref name="previousJobResult"/> Task is evaluated and if it contains a <typeparamref name="Tf"/>, that <typeparam name="Tf"></typeparam> will be immediately returned.<br/>
    /// Otherwise, the <typeparamref name="Ts"/> is passed to the <paramref name="nextJob"/> Func to be executed and the new result is returned.
    /// </summary>
    /// <typeparam name="Ts">Represents success, also likely contains any required state/results for processing in the chain.</typeparam>
    /// <typeparam name="Tf">Represents a failure at some point in the chain.</typeparam>
    /// <param name="previousJobResult">The resulting Task of the previous link in the chain.</param>
    /// <param name="nextJob">A Func containing the next piece of work in the chain.</param>
    /// <returns>A Task&lt;OneOf&lt;<typeparamref name="Ts"/>, <typeparamref name="Tf"/>&gt;&gt;, which enables these extension methods to form a chain.</returns>
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
    /// Extension method which enables method chaining.
    /// Each method in the chain must accept a <typeparamref name="Ts"/> and return a Task&lt;OneOf&lt;<typeparamref name="Ts"/>, <typeparamref name="Tf"/>&gt;&gt;,
    /// where <typeparamref name="Ts"/> == Success and <typeparamref name="Tf"/> == Failure.<br/>
    /// The result of the <paramref name="previousJobResult"/> Task is evaluated and if it contains a <typeparamref name="Tf"/>, that <typeparam name="Tf"></typeparam> will be immediately returned.<br/>
    /// Otherwise, the <typeparamref name="Ts"/> is passed to the <paramref name="nextJob"/> Func to be executed and the new result is returned.
    /// The <paramref name="onFailure"/> Func enables tidying up tasks to be performed if <paramref name="nextJob"/> fails.
    /// </summary>
    /// <typeparam name="Ts">Represents success, also likely contains any required state/results for processing in the chain.</typeparam>
    /// <typeparam name="Tf">Represents a failure at some point in the chain.</typeparam>
    /// <param name="previousJobResult">The resulting Task of the previous link in the chain.</param>
    /// <param name="nextJob">A Func containing the next piece of work in the chain.</param>
    /// <param name="onFailure">A Func which will be invoked if a <typeparamref name="Tf"/> has been returned, it is passed the <typeparamref name="Ts"/> and the <typeparamref name="Tf"/>
    /// and should perform any tidying up tasks as a result of the Failure, before returning the final <typeparamref name="Tf"/> to be passed down the chain.</param>
    /// <returns>A Task&lt;OneOf&lt;<typeparamref name="Ts"/>, <typeparamref name="Tf"/>&gt;&gt;, which enables these extension methods to form a chain.</returns>
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
    /// Extension method which enables method chaining. This version evaluates an additional Func <paramref name="condition"/> and only invokes <paramref name="nextJob"/> if it returns True.<br/>
    /// Each method in the chain must accept a <typeparamref name="Ts"/> and return a Task&lt;OneOf&lt;<typeparamref name="Ts"/>, <typeparamref name="Tf"/>&gt;&gt;,
    /// where <typeparamref name="Ts"/> == Success and <typeparamref name="Tf"/> == Failure.<br/>
    /// The result of the <paramref name="previousJobResult"/> Task is evaluated and if it contains a <typeparamref name="Tf"/>, that <typeparam name="Tf"></typeparam> will be immediately returned.<br/>
    /// Otherwise, if the <paramref name="condition"/> func evaluates to True, the <typeparamref name="Ts"/> is passed to the <paramref name="nextJob"/> Func to be executed and the new result is returned.
    /// If the <paramref name="condition"/> func evaluates to False, the <typeparamref name="Ts"/> is returned.<br/>
    /// The <paramref name="onFailure"/> Func enables tidying up tasks to be performed if <paramref name="nextJob"/> fails.
    /// </summary>
    /// <typeparam name="Ts">Represents success, also likely contains any required state/results for processing in the chain.</typeparam>
    /// <typeparam name="Tf">Represents a failure at some point in the chain.</typeparam>
    /// <param name="previousJobResult">The resulting Task of the previous link in the chain.</param>
    /// <param name="condition">This func will be invoked first, only if bool True is returned will <paramref name="nextJob"/> be invoked, otherwise the current <typeparamref name="Ts"/> will be passed to the next link in the chain.</param>
    /// <param name="nextJob">A Func containing the next piece of work in the chain.</param>
    /// <param name="onFailure">A Func which will be invoked if a <typeparamref name="Tf"/> has been returned, it is passed the <typeparamref name="Ts"/> and the <typeparamref name="Tf"/>
    /// and should perform any tidying up tasks as a result of the Failure, before returning the final <typeparamref name="Tf"/> to be passed down the chain.</param>
    /// <returns>A Task&lt;OneOf&lt;<typeparamref name="Ts"/>, <typeparamref name="Tf"/>&gt;&gt;, which enables these extension methods to form a chain.</returns>
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
    /// Extension method which enables method chaining. This version enables looping over a collection of <typeparamref name="Titem"/>s.<br/>
    /// Each method in the chain must accept a <typeparamref name="Ts"/> and return a Task&lt;OneOf&lt;<typeparamref name="Ts"/>, <typeparamref name="Tf"/>&gt;&gt;,
    /// where <typeparamref name="Ts"/> == Success and <typeparamref name="Tf"/> == Failure.<br/>
    /// The result of the <paramref name="previousJobResult"/> Task is evaluated and if it contains a <typeparamref name="Tf"/>, that <typeparam name="Tf"></typeparam> will be immediately returned.<br/>
    /// Otherwise, Items can be fetched and <paramref name="taskForEachItem"/> called for each Item.
    /// If <paramref name="taskForEachItem"/> returns a <typeparamref name="Tf"/> for any item, then that will be immediately returned.
    /// Otherwise, successful results are each passed to the next task and the loop continues until the final result is returned.
    /// The <paramref name="onFailure"/> Func enables tidying up tasks to be performed if any tasks fail.
    /// </summary>
    /// <typeparam name="Ts">Represents success, also likely contains any required state/results for processing in the chain.</typeparam>
    /// <typeparam name="Tf">Represents a failure at some point in the chain.</typeparam>
    /// <typeparam name="Titem">The Type to be iterated over.</typeparam>
    /// <param name="previousJobResult">The resulting Task of the previous link in the chain.</param>
    /// <param name="itemsToIterateOver">A Func which should produce an IEnumerable of <typeparamref name="Titem"/></param>
    /// <param name="taskForEachItem">A Func which will be call for each <typeparamref name="Titem"/> and should return a Task of OneOf <typeparamref name="Ts"/> or <typeparamref name="Tf"/>.</param>
    /// <param name="onFailure">A Func which will be invoked if a <typeparamref name="Tf"/> has been returned, it is passed the <typeparamref name="Ts"/> and the <typeparamref name="Tf"/>
    /// and should perform any tidying up tasks as a result of the Failure, before returning the final <typeparamref name="Tf"/> to be passed down the chain.</param>
    /// <returns>A Task&lt;OneOf&lt;<typeparamref name="Ts"/>, <typeparamref name="Tf"/>&gt;&gt;, which enables these extension methods to form a chain.</returns>
    public static async Task<OneOf<Ts, Tf>> ThenForEach<Ts, Tf, Titem>(
        this Task<OneOf<Ts, Tf>> previousJobResult,
        Func<Ts, IEnumerable<Titem>> itemsToIterateOver,
        Func<Ts, Titem, Task<OneOf<Ts, Tf>>> taskForEachItem,
        Func<Ts, Tf, Task<OneOf<Ts, Tf>>>? onFailure = null)
    {
        // Inspect result of (probably already awaited) previousJobResult, if it's a Tf return it...
        var TOrFailure = await previousJobResult;
        if (TOrFailure.IsT1)
            return TOrFailure;

        // Fetch items to iterate over
        var currentTs = TOrFailure.AsT0;
        var items = itemsToIterateOver(currentTs);

        // foreach over, keep the result and break out if one returns a Tf...
        OneOf<Ts, Tf> itemResult = currentTs;
        foreach (Titem item in items)
        {
            // Pass previous itemResult into the task...
            itemResult = await taskForEachItem(itemResult.AsT0, item);
            
            if (itemResult.IsT1)
                break;
        }

        // Return if successful
        if (itemResult.IsT0)
            return itemResult;

        // Otherwise return failure...
        if (onFailure is null)
            return itemResult;

        // Unless we have an OnFailure job to do...
        var finalFailure = await onFailure(currentTs, itemResult.AsT1);

        // and return the final resulting Tf as long as it is a Tf!
        return finalFailure.IsT1 ? finalFailure : itemResult.AsT1;
    }

    /// <summary>
    /// Extension method which enables the result of a chain of methods to be finally converted to a new type <typeparamref name="TResult"/> if all operations in the chain have been successful.<br/>
    /// Each method in the chain must accept a <typeparamref name="Ts"/> and return a Task&lt;OneOf&lt;<typeparamref name="Ts"/>, <typeparamref name="Tf"/>&gt;&gt;,
    /// where <typeparamref name="Ts"/> == Success and <typeparamref name="Tf"/> == Failure.<br/> 
    /// <typeparamref name="Ts"/> == Success and <typeparamref name="Tf"/> == Failure.<br/>
    /// </summary>
    /// <typeparam name="Ts">Represents success, also likely contains any required state/results for processing in the chain.</typeparam>
    /// <typeparam name="Tf">Represents a failure at some point in the chain.</typeparam>
    /// <typeparam name="TResult">The new type to convert the <typeparamref name="Ts"/> into.</typeparam>
    /// <param name="previousJobResult">The resulting Task of the previous link in the chain.</param>
    /// <param name="convertToResult">A func provided to do the conversion.</param>
    /// <returns>A Task&lt;OneOf&lt;<typeparamref name="Ts"/>, <typeparamref name="Tf"/>&gt;&gt;, which enables these extension methods to form a chain.</returns>
    public static async Task<OneOf<TResult, Tf>> ToResult<TResult, Ts, Tf>(this Task<OneOf<Ts, Tf>> previousJobResult, Func<Ts, TResult> convertToResult)
    {
        // Await final job, return cascaded Tf or new Success.
        var TOrFailure = await previousJobResult;
        return TOrFailure.Match<OneOf<TResult, Tf>>(
            _ => convertToResult(TOrFailure.AsT0),
            failure => failure);
    }

    /// <summary>
    /// Extension method which enables method chaining. This method accepts an array of tasks which will be executed in parallel. This method will return once all tasks have completed.<br/>
    /// Each method in the chain must accept a <typeparamref name="Ts"/> and return a Task&lt;OneOf&lt;<typeparamref name="Ts"/>, <typeparamref name="Tf"/>&gt;&gt;,
    /// where <typeparamref name="Ts"/> == Success and <typeparamref name="Tf"/> == Failure.<br/>
    /// Please note, the <typeparamref name="Ts"/> is passed into each task by ref, so care must be taken around any mutation of any state on the <typeparamref name="Ts"/>.<br/>
    /// This library cannot know how to merge the result of each task. A strategy must be provided. Another overload of this method provides a naive default merging strategy.
    /// </summary>
    /// <typeparam name="Ts">Represents success, also likely contains any required state/results for processing in the chain.</typeparam>
    /// <typeparam name="Tf">Represents a failure at some point in the chain.</typeparam>
    /// <param name="previousJobResult">The resulting Task of the previous link in the chain.</param>
    /// <param name="resultMergingStrategy">A func which is passed the original <typeparamref name="Ts"/> and a list of results from the Tasks,
    /// it should decide how to merge the results once they have all returned i.e. what to return from this method.</param>
    /// <param name="tasks">A list a tasks to execute in parallel.</param>
    /// <returns>A Task&lt;OneOf&lt;<typeparamref name="Ts"/>, <typeparamref name="Tf"/>&gt;&gt;, which enables these extension methods to form a chain.</returns>
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
    /// Extension method which enables method chaining. This method accepts an array of tasks which will be executed in parallel. This method will return once all tasks have completed.<br/>
    /// Each method in the chain must accept a <typeparamref name="Ts"/> and return a Task&lt;OneOf&lt;<typeparamref name="Ts"/>, <typeparamref name="Tf"/>&gt;&gt;,
    /// where <typeparamref name="Ts"/> == Success and <typeparamref name="Tf"/> == Failure.<br/>
    /// Please note, the <typeparamref name="Ts"/> is passed into each task by ref, so care must be taken around any mutation of any state on the <typeparamref name="Ts"/>.<br/>
    /// This library cannot know how to merge the result of each task. 
    /// The naive default result merging strategy is to return the first <typeparamref name="Tf"/> if any tasks return a <typeparamref name="Tf"/>, otherwise return the original
    /// <c>TOrFailure</c> passed into this method.<br/>
    /// A better strategy can <i>and should</i> be provided using the overload of this method.
    /// </summary>
    /// <typeparam name="Ts">Represents success, also likely contains any required state/results for processing in the chain.</typeparam>
    /// <typeparam name="Tf">Represents a failure at some point in the chain.</typeparam>
    /// <param name="previousJobResult">The resulting Task of the previous link in the chain.</param>
    /// <param name="tasks">A list a tasks to execute in parallel, </param>
    /// <returns>A Task&lt;OneOf&lt;<typeparamref name="Ts"/>, <typeparamref name="Tf"/>&gt;&gt;, which enables these extension methods to form a chain.</returns>
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
    /// Extension method which enables method chaining. This method accepts an array of tasks which will be executed in parallel. This method will return immediately once the first task has completed.<br/>
    /// Each method in the chain must accept a <typeparamref name="Ts"/> and return a Task&lt;OneOf&lt;<typeparamref name="Ts"/>, <typeparamref name="Tf"/>&gt;&gt;,
    /// where <typeparamref name="Ts"/> == Success and <typeparamref name="Tf"/> == Failure.<br/>
    /// The result of the first completed task will be returned. The other task's results are ignored.
    /// Please note, the <typeparamref name="Ts"/> is passed into each task by ref, so care must be taken around any mutation of any state on the <typeparamref name="Ts"/>.<br/>
    /// </summary>
    /// <typeparam name="Ts">Represents success, also likely contains any required state/results for processing in the chain.</typeparam>
    /// <typeparam name="Tf">Represents a failure at some point in the chain.</typeparam>
    /// <param name="previousJobResult">The resulting Task of the previous link in the chain.</param>
    /// <param name="tasks">A list a tasks to execute in parallel.</param>
    /// <returns>A Task&lt;OneOf&lt;<typeparamref name="Ts"/>, <typeparamref name="Tf"/>&gt;&gt;, which enables these extension methods to form a chain.</returns>
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

/// <summary>
/// A set of extension methods which allow chaining of methods in a nice fluent, english sentence like chain or flow, with cancellation support.
/// In order to be chained a method need only return a Task Of OneOf Ts or Tf, where Ts == Success and Tf == Failure.
/// </summary>
public static class AsynchronousMethodChainingExtensionsWithCancellationSupport
{
    /// <summary>
    /// Extension method which enables method chaining. Supports cancellation.
    /// Each method in the chain must accept a <typeparamref name="Ts"/> and return a Task&lt;OneOf&lt;<typeparamref name="Ts"/>, <typeparamref name="Tf"/>&gt;&gt;,
    /// where <typeparamref name="Ts"/> == Success and <typeparamref name="Tf"/> == Failure.<br/>
    /// The result of the <paramref name="previousJobResult"/> Task is evaluated and if it contains a <typeparamref name="Tf"/>, that <typeparam name="Tf"></typeparam> will be immediately returned.<br/>
    /// Otherwise, the <typeparamref name="Ts"/> is passed to the <paramref name="nextJob"/> Func to be executed and the new result is returned.
    /// </summary>
    /// <typeparam name="Ts">Represents success, also likely contains any required state/results for processing in the chain.</typeparam>
    /// <typeparam name="Tf">Represents a failure at some point in the chain.</typeparam>
    /// <param name="previousJobResult">The resulting Task of the previous link in the chain.</param>
    /// <param name="nextJob">A Func containing the next piece of work in the chain.</param>
    /// <param name="ct">A token which enables cancellation, checked immediately and also passed into the lambda to enable checking for cancellation at more granular levels.</param>
    /// <param name="throwOnCancellation">Flag which can be used to disable calling of ThrowIfCancellationRequested(), useful for cancelling gracefully while still returning some result etc.</param>
    /// <returns>A Task&lt;OneOf&lt;<typeparamref name="Ts"/>, <typeparamref name="Tf"/>&gt;&gt;, which enables these extension methods to form a chain.</returns>
    public static async Task<OneOf<Ts, Tf>> Then<Ts, Tf>(
        this Task<OneOf<Ts, Tf>> previousJobResult,
        Func<Ts, CancellationToken, Task<OneOf<Ts, Tf>>> nextJob,
        CancellationToken ct, bool throwOnCancellation = true)
    {
        // Inspect result of (probably already awaited) previousJobResult, if it's a Tf return it...
        var TOrFailure = await previousJobResult;
        if (TOrFailure.IsT1)
            return TOrFailure;

        if (throwOnCancellation)
            ct.ThrowIfCancellationRequested();

        // ...Otherwise, do your next job
        return await nextJob(TOrFailure.AsT0, ct);
    }

    /// <summary>
    /// Extension method which enables method chaining. Supports cancellation.
    /// Each method in the chain must accept a <typeparamref name="Ts"/> and return a Task&lt;OneOf&lt;<typeparamref name="Ts"/>, <typeparamref name="Tf"/>&gt;&gt;,
    /// where <typeparamref name="Ts"/> == Success and <typeparamref name="Tf"/> == Failure.<br/>
    /// The result of the <paramref name="previousJobResult"/> Task is evaluated and if it contains a <typeparamref name="Tf"/>, that <typeparam name="Tf"></typeparam> will be immediately returned.<br/>
    /// Otherwise, the <typeparamref name="Ts"/> is passed to the <paramref name="nextJob"/> Func to be executed and the new result is returned.
    /// The <paramref name="onFailure"/> Func enables tidying up tasks to be performed if <paramref name="nextJob"/> fails.
    /// </summary>
    /// <typeparam name="Ts">Represents success, also likely contains any required state/results for processing in the chain.</typeparam>
    /// <typeparam name="Tf">Represents a failure at some point in the chain.</typeparam>
    /// <param name="previousJobResult">The resulting Task of the previous link in the chain.</param>
    /// <param name="nextJob">A Func containing the next piece of work in the chain.</param>
    /// <param name="onFailure">A Func which will be invoked if a <typeparamref name="Tf"/> has been returned, it is passed the <typeparamref name="Ts"/> and the <typeparamref name="Tf"/>
    /// and should perform any tidying up tasks as a result of the Failure, before returning the final <typeparamref name="Tf"/> to be passed down the chain.</param>
    /// <param name="ct">A token which enables cancellation, checked immediately and also passed into the lambda to enable checking for cancellation at more granular levels.</param>
    /// <param name="throwOnCancellation">Flag which can be used to disable calling of ThrowIfCancellationRequested(), useful for cancelling gracefully while still returning some result etc.</param>
    /// <returns>A Task&lt;OneOf&lt;<typeparamref name="Ts"/>, <typeparamref name="Tf"/>&gt;&gt;, which enables these extension methods to form a chain.</returns>
    public static async Task<OneOf<Ts, Tf>> Then<Ts, Tf>(
        this Task<OneOf<Ts, Tf>> previousJobResult,
        Func<Ts, CancellationToken, Task<OneOf<Ts, Tf>>> nextJob,
        Func<Ts, Tf, CancellationToken, Task<OneOf<Ts, Tf>>> onFailure,
        CancellationToken ct, bool throwOnCancellation = true)
    {
        // Inspect result of (probably already awaited) previousJobResult, if it's a Tf return it...
        var TOrFailure = await previousJobResult;
        if (TOrFailure.IsT1)
            return TOrFailure;

        if (throwOnCancellation)
            ct.ThrowIfCancellationRequested();

        // ...Otherwise, do your next job ... 
        var currentTs = TOrFailure.AsT0;
        var result = await nextJob(currentTs, ct);

        // If next job returned a  Ts (Success) return it...
        if (result.IsT0)
            return result;

        // Otherwise invoke onFailure...
        var finalFailure = await onFailure(currentTs, result.AsT1, ct);

        // and return the final resulting Tf as long as it is a Tf!
        return finalFailure.IsT1 ? finalFailure : result.AsT1;
    }

    /// <summary>
    /// Extension method which enables method chaining. Supports cancellation. This version evaluates an additional Func <paramref name="condition"/> and only invokes <paramref name="nextJob"/> if it returns True.<br/>
    /// Each method in the chain must accept a <typeparamref name="Ts"/> and return a Task&lt;OneOf&lt;<typeparamref name="Ts"/>, <typeparamref name="Tf"/>&gt;&gt;,
    /// where <typeparamref name="Ts"/> == Success and <typeparamref name="Tf"/> == Failure.<br/>
    /// The result of the <paramref name="previousJobResult"/> Task is evaluated and if it contains a <typeparamref name="Tf"/>, that <typeparam name="Tf"></typeparam> will be immediately returned.<br/>
    /// Otherwise, if the <paramref name="condition"/> func evaluates to True, the <typeparamref name="Ts"/> is passed to the <paramref name="nextJob"/> Func to be executed and the new result is returned.
    /// If the <paramref name="condition"/> func evaluates to False, the <typeparamref name="Ts"/> is returned.<br/>
    /// The <paramref name="onFailure"/> Func enables tidying up tasks to be performed if <paramref name="nextJob"/> fails.
    /// </summary>
    /// <typeparam name="Ts">Represents success, also likely contains any required state/results for processing in the chain.</typeparam>
    /// <typeparam name="Tf">Represents a failure at some point in the chain.</typeparam>
    /// <param name="previousJobResult">The resulting Task of the previous link in the chain.</param>
    /// <param name="condition">This func will be invoked first, only if bool True is returned will <paramref name="nextJob"/> be invoked, otherwise the current <typeparamref name="Ts"/> will be passed to the next link in the chain.</param>
    /// <param name="nextJob">A Func containing the next piece of work in the chain.</param>
    /// <param name="onFailure">A Func which will be invoked if a <typeparamref name="Tf"/> has been returned, it is passed the <typeparamref name="Ts"/> and the <typeparamref name="Tf"/>
    /// and should perform any tidying up tasks as a result of the Failure, before returning the final <typeparamref name="Tf"/> to be passed down the chain.</param>
    /// <param name="ct">A token which enables cancellation, checked immediately and also passed into the lambda to enable checking for cancellation at more granular levels.</param>
    /// <param name="throwOnCancellation">Flag which can be used to disable calling of ThrowIfCancellationRequested(), useful for cancelling gracefully while still returning some result etc.</param>
    /// <returns>A Task&lt;OneOf&lt;<typeparamref name="Ts"/>, <typeparamref name="Tf"/>&gt;&gt;, which enables these extension methods to form a chain.</returns>
    public static async Task<OneOf<Ts, Tf>> IfThen<Ts, Tf>(
        this Task<OneOf<Ts, Tf>> previousJobResult,
        Func<Ts, CancellationToken, bool> condition,
        Func<Ts, CancellationToken, Task<OneOf<Ts, Tf>>> nextJob,
        CancellationToken ct, bool throwOnCancellation = true,
        Func<Ts, Tf, CancellationToken, Task<OneOf<Ts, Tf>>>? onFailure = null)
    {
        // Inspect result of (probably already awaited) previousJobResult, if it's a Tf return it...
        var TOrFailure = await previousJobResult;
        if (TOrFailure.IsT1)
            return TOrFailure;

        if (throwOnCancellation)
            ct.ThrowIfCancellationRequested();

        // Otherwise invoke the condition to decide whether to skip...
        var currentTs = TOrFailure.AsT0;
        if (condition(currentTs, ct) is false)
            return currentTs;

        // Or run the next job...
        var result = await nextJob(currentTs, ct);

        // Return if successful
        if (result.IsT0)
            return result;

        // Otherwise return failure...
        if (onFailure is null)
            return result;

        // Unless we have an OnFailure job to do...
        var finalFailure = await onFailure(currentTs, result.AsT1, ct);

        // and return the final resulting Tf as long as it is a Tf!
        return finalFailure.IsT1 ? finalFailure : result.AsT1;
    }

    /// <summary>
    /// Extension method which enables method chaining. Supports cancellation. This version enables looping over a collection of <typeparamref name="Titem"/>s.<br/>
    /// Each method in the chain must accept a <typeparamref name="Ts"/> and return a Task&lt;OneOf&lt;<typeparamref name="Ts"/>, <typeparamref name="Tf"/>&gt;&gt;,
    /// where <typeparamref name="Ts"/> == Success and <typeparamref name="Tf"/> == Failure.<br/>
    /// The result of the <paramref name="previousJobResult"/> Task is evaluated and if it contains a <typeparamref name="Tf"/>, that <typeparam name="Tf"></typeparam> will be immediately returned.<br/>
    /// Otherwise, Items can be fetched and <paramref name="taskForEachItem"/> called for each Item.
    /// If <paramref name="taskForEachItem"/> returns a <typeparamref name="Tf"/> for any item, then that will be immediately returned.
    /// Otherwise, successful results are each passed to the next task and the loop continues until the final result is returned.
    /// The <paramref name="onFailure"/> Func enables tidying up tasks to be performed if any tasks fail.
    /// </summary>
    /// <typeparam name="Ts">Represents success, also likely contains any required state/results for processing in the chain.</typeparam>
    /// <typeparam name="Tf">Represents a failure at some point in the chain.</typeparam>
    /// <typeparam name="Titem">The Type to be iterated over.</typeparam>
    /// <param name="previousJobResult">The resulting Task of the previous link in the chain.</param>
    /// <param name="itemsToIterateOver">A Func which should produce an IEnumerable of <typeparamref name="Titem"/></param>
    /// <param name="taskForEachItem">A Func which will be call for each <typeparamref name="Titem"/> and should return a Task of OneOf <typeparamref name="Ts"/> or <typeparamref name="Tf"/>.</param>
    /// <param name="onFailure">A Func which will be invoked if a <typeparamref name="Tf"/> has been returned, it is passed the <typeparamref name="Ts"/> and the <typeparamref name="Tf"/>
    /// and should perform any tidying up tasks as a result of the Failure, before returning the final <typeparamref name="Tf"/> to be passed down the chain.</param>
    /// /// <param name="ct">A token which enables cancellation, checked immediately and also passed into the lambda to enable checking for cancellation at more granular levels.</param>
    /// <param name="throwOnCancellation">Flag which can be used to disable calling of ThrowIfCancellationRequested(), useful for cancelling gracefully while still returning some result etc.</param>
    /// <returns>A Task&lt;OneOf&lt;<typeparamref name="Ts"/>, <typeparamref name="Tf"/>&gt;&gt;, which enables these extension methods to form a chain.</returns>
    public static async Task<OneOf<Ts, Tf>> ThenForEach<Ts, Tf, Titem>(
        this Task<OneOf<Ts, Tf>> previousJobResult,
        Func<Ts, IEnumerable<Titem>> itemsToIterateOver,
        Func<Ts, Titem, CancellationToken, Task<OneOf<Ts, Tf>>> taskForEachItem,
        CancellationToken ct, bool throwOnCancellation = true,
        Func<Ts, Tf, Task<OneOf<Ts, Tf>>>? onFailure = null)
    {
        // Inspect result of (probably already awaited) previousJobResult, if it's a Tf return it...
        var TOrFailure = await previousJobResult;
        if (TOrFailure.IsT1)
            return TOrFailure;
        
        // Fetch items to iterate over
        var currentTs = TOrFailure.AsT0;
        var items = itemsToIterateOver(currentTs);

        // foreach over, keep the result and break out if one returns a Tf...
        OneOf<Ts, Tf> itemResult = currentTs;
        foreach (Titem item in items)
        {
            if (throwOnCancellation)
                ct.ThrowIfCancellationRequested();

            // Pass previous itemResult into the task...
            itemResult = await taskForEachItem(itemResult.AsT0, item, ct);

            if (itemResult.IsT1)
                break;
        }

        // Return if successful
        if (itemResult.IsT0)
            return itemResult;

        // Otherwise return failure...
        if (onFailure is null)
            return itemResult;

        // Unless we have an OnFailure job to do...
        var finalFailure = await onFailure(currentTs, itemResult.AsT1);

        // and return the final resulting Tf as long as it is a Tf!
        return finalFailure.IsT1 ? finalFailure : itemResult.AsT1;
    }

    /// <summary>
    /// Extension method which enables the result of a chain of methods to be finally converted to a new type <typeparamref name="TResult"/> if all operations in the chain have been successful.<br/>
    /// Each method in the chain must accept a <typeparamref name="Ts"/> and return a Task&lt;OneOf&lt;<typeparamref name="Ts"/>, <typeparamref name="Tf"/>&gt;&gt;,
    /// where <typeparamref name="Ts"/> == Success and <typeparamref name="Tf"/> == Failure.<br/> 
    /// <typeparamref name="Ts"/> == Success and <typeparamref name="Tf"/> == Failure.<br/>
    /// </summary>
    /// <typeparam name="Ts">Represents success, also likely contains any required state/results for processing in the chain.</typeparam>
    /// <typeparam name="Tf">Represents a failure at some point in the chain.</typeparam>
    /// <typeparam name="TResult">The new type to convert the <typeparamref name="Ts"/> into.</typeparam>
    /// <param name="previousJobResult">The resulting Task of the previous link in the chain.</param>
    /// <param name="convertToResult">A func provided to do the conversion.</param>
    /// <param name="ct">A token which enables cancellation, checked immediately and also passed into the lambda to enable checking for cancellation at more granular levels.</param>
    /// <param name="throwOnCancellation">Flag which can be used to disable calling of ThrowIfCancellationRequested(), useful for cancelling gracefully while still returning some result etc.</param>
    /// <returns>A Task of OneOf TResult or Tf, where the TResult has been converted from T.</returns>
    public static async Task<OneOf<TResult, Tf>> ToResult<TResult, Ts, Tf>(
        this Task<OneOf<Ts, Tf>> previousJobResult, 
        Func<Ts, TResult> convertToResult, 
        CancellationToken ct, bool throwOnCancellation = true)
    {
        if (throwOnCancellation)
            ct.ThrowIfCancellationRequested();

        // Await final job, return cascaded Tf or new Success.
        var TOrFailure = await previousJobResult;
        return TOrFailure.Match<OneOf<TResult, Tf>>(
            _ => convertToResult(TOrFailure.AsT0),
            failure => failure);
    }

    /// <summary>
    /// Extension method which enables method chaining. Supports cancellation. This method accepts an array of tasks which will be executed in parallel. This method will return once all tasks have completed.
    /// Each method in the chain must accept a <typeparamref name="Ts"/> and return a Task&lt;OneOf&lt;<typeparamref name="Ts"/>, <typeparamref name="Tf"/>&gt;&gt;,
    /// where <typeparamref name="Ts"/> == Success and <typeparamref name="Tf"/> == Failure.<br/>
    /// Please note, the <typeparamref name="Ts"/> is passed into each task by ref, so care must be taken around any mutation of any state on the <typeparamref name="Ts"/>.<br/>
    /// This library cannot know how to merge the result of each task. A strategy must be provided. Another overload of this method provides a naive default merging strategy.
    /// </summary>
    /// <typeparam name="Ts">Represents success, also likely contains any required state/results for processing in the chain.</typeparam>
    /// <typeparam name="Tf">Represents a failure at some point in the chain.</typeparam>
    /// <param name="previousJobResult">The resulting Task of the previous link in the chain.</param>
    /// <param name="resultMergingStrategy">A func which is passed the original <typeparamref name="Ts"/> and a list of results from the Tasks,
    /// it should decide how to merge the results once they have all returned i.e. what to return from this method.</param>
    /// <param name="tasks">A list a tasks to execute in parallel.</param>
    /// <param name="ct">A token which enables cancellation, checked immediately and also passed into the lambda to enable checking for cancellation at more granular levels.</param>
    /// <param name="throwOnCancellation">Flag which can be used to disable calling of ThrowIfCancellationRequested(), useful for cancelling gracefully while still returning some result etc.</param>
    /// <returns>A Task&lt;OneOf&lt;<typeparamref name="Ts"/>, <typeparamref name="Tf"/>&gt;&gt;, which enables these extension methods to form a chain.</returns>
    public static async Task<OneOf<Ts, Tf>> ThenWaitForAll<Ts, Tf>(
        this Task<OneOf<Ts, Tf>> previousJobResult,
        Func<Ts, CancellationToken, List<OneOf<Ts, Tf>>, OneOf<Ts, Tf>> resultMergingStrategy,
        CancellationToken ct, bool throwOnCancellation = true,
        params Func<Ts, CancellationToken, Task<OneOf<Ts, Tf>>>[] tasks)
    {
        // Inspect result of (probably already awaited) previousJobResult, if it's a Tf return it...
        var TOrFailure = await previousJobResult;
        if (TOrFailure.IsT1)
            return TOrFailure;

        if (throwOnCancellation)
            ct.ThrowIfCancellationRequested();

        if (tasks.Length == 0)
            return TOrFailure;

        var taskResults = new List<OneOf<Ts, Tf>>();
        await Task.WhenAll(tasks.Select(async task =>
        {
            taskResults.Add(await task(TOrFailure.AsT0, ct));
        }));

        var finalResult = resultMergingStrategy(TOrFailure.AsT0, ct, taskResults);

        return finalResult;
    }

    /// <summary>
    /// Extension method which enables method chaining. Supports cancellation. This method accepts an array of tasks which will be executed in parallel. This method will return once all tasks have completed.<br/>
    /// Each method in the chain must accept a <typeparamref name="Ts"/> and return a Task&lt;OneOf&lt;<typeparamref name="Ts"/>, <typeparamref name="Tf"/>&gt;&gt;,
    /// where <typeparamref name="Ts"/> == Success and <typeparamref name="Tf"/> == Failure.<br/>
    /// Please note, the <typeparamref name="Ts"/> is passed into each task by ref, so care must be taken around any mutation of any state on the <typeparamref name="Ts"/>.<br/>
    /// This library cannot know how to merge the result of each task. 
    /// The naive default result merging strategy is to return the first <typeparamref name="Tf"/> if any tasks return a <typeparamref name="Tf"/>, otherwise return the original
    /// <c>TOrFailure</c> passed into this method.<br/>
    /// A better strategy can <i>and should</i> be provided using the overload of this method.
    /// </summary>
    /// <typeparam name="Ts">Represents success, also likely contains any required state/results for processing in the chain.</typeparam>
    /// <typeparam name="Tf">Represents a failure at some point in the chain.</typeparam>
    /// <param name="previousJobResult">The resulting Task of the previous link in the chain.</param>
    /// <param name="ct">A token which enables cancellation, checked immediately and also passed into the lambda to enable checking for cancellation at more granular levels.</param>
    /// <param name="throwOnCancellation">Flag which can be used to disable calling of ThrowIfCancellationRequested(), useful for cancelling gracefully while still returning some result etc.</param>
    /// <param name="tasks">A list a tasks to execute in parallel, </param>
    /// <returns>A Task&lt;OneOf&lt;<typeparamref name="Ts"/>, <typeparamref name="Tf"/>&gt;&gt;, which enables these extension methods to form a chain.</returns>
    public static async Task<OneOf<Ts, Tf>> ThenWaitForAll<Ts, Tf>(
        this Task<OneOf<Ts, Tf>> previousJobResult,
        CancellationToken ct, bool throwOnCancellation = true,
        params Func<Ts, CancellationToken, Task<OneOf<Ts, Tf>>>[] tasks)
    {
        static OneOf<Ts, Tf> DefaultResultMergingStrategy(Ts inputT, CancellationToken ct, List<OneOf<Ts, Tf>> results)
        {
            return results.Any(x => x.IsT1) ? results.First(x => x.IsT1) : inputT;
        }

        return await previousJobResult.ThenWaitForAll(DefaultResultMergingStrategy, ct, throwOnCancellation, tasks);
    }

    /// <summary>
    /// Extension method which enables method chaining. This method accepts an array of tasks which will be executed in parallel. This method will return immediately once the first task has completed.<br/>
    /// Each method in the chain must accept a <typeparamref name="Ts"/> and return a Task&lt;OneOf&lt;<typeparamref name="Ts"/>, <typeparamref name="Tf"/>&gt;&gt;,
    /// where <typeparamref name="Ts"/> == Success and <typeparamref name="Tf"/> == Failure.<br/>
    /// The result of the first completed task will be returned. The other task's results are ignored.
    /// Please note, the <typeparamref name="Ts"/> is passed into each task by ref, so care must be taken around any mutation of any state on the <typeparamref name="Ts"/>.<br/>
    /// </summary>
    /// <typeparam name="Ts">Represents success, also likely contains any required state/results for processing in the chain.</typeparam>
    /// <typeparam name="Tf">Represents a failure at some point in the chain.</typeparam>
    /// <param name="previousJobResult">The resulting Task of the previous link in the chain.</param>
    /// <param name="ct">A token which enables cancellation, checked immediately and also passed into the lambda to enable checking for cancellation at more granular levels.</param>
    /// <param name="throwOnCancellation">Flag which can be used to disable calling of ThrowIfCancellationRequested(), useful for cancelling gracefully while still returning some result etc.</param>
    /// <param name="tasks">A list a tasks to execute in parallel.</param>
    /// <returns>A Task&lt;OneOf&lt;<typeparamref name="Ts"/>, <typeparamref name="Tf"/>&gt;&gt;, which enables these extension methods to form a chain.</returns>
    public static async Task<OneOf<Ts, Tf>> ThenWaitForFirst<Ts, Tf>(
        this Task<OneOf<Ts, Tf>> previousJobResult,
        CancellationToken ct, bool throwOnCancellation = true,
        params Func<Ts, CancellationToken, Task<OneOf<Ts, Tf>>>[] tasks)
    {
        // Inspect result of (probably already awaited) previousJobResult, if it's a Tf return it...
        var TOrFailure = await previousJobResult;
        if (TOrFailure.IsT1)
            return TOrFailure;

        if (throwOnCancellation)
            ct.ThrowIfCancellationRequested();

        if (tasks.Length == 0)
            return TOrFailure;

        var remainingTasksCanceller = CancellationTokenSource.CreateLinkedTokenSource(ct);

        var result = await await Task.WhenAny(tasks.Select(async task => await task(TOrFailure.AsT0, remainingTasksCanceller.Token)));

        remainingTasksCanceller.Cancel();

        return result;
    }
}

