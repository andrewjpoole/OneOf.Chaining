using OneOf.Types;

namespace OneOf.Chaining.Tests;

public class AsynchronousMethodChainingWithCancellationSupportTests
{
    // In these fake chainable tasks, explicitly ignore the cancellation token passed in,
    // as most chainable tasks probably won't throw on Cancellation.
    static async Task<OneOf<StateStore, Error>> Job1(StateStore s, CancellationToken ct)
    {
        await Task.Delay(100, CancellationToken.None);
        TestContext.Out.WriteLine($"{DateTime.Now:T} Job1 after delay, ct.IsCancellationRequested: {ct.IsCancellationRequested}");
        s.Flag1 = true;
        return s;
    }

    static async Task<OneOf<StateStore, Error>> Job2(StateStore s, CancellationToken ct)
    {
        await Task.Delay(100, CancellationToken.None);
        TestContext.Out.WriteLine($"{DateTime.Now:T} Job2 after delay, ct.IsCancellationRequested: {ct.IsCancellationRequested}");
        s.Flag2 = true;
        return s;
    }

    static async Task<OneOf<StateStore, Error>> Job3(StateStore s, CancellationToken ct)
    {
        await Task.Delay(100, CancellationToken.None);
        TestContext.Out.WriteLine($"{DateTime.Now:T} Job3 after delay, ct.IsCancellationRequested: {ct.IsCancellationRequested}");
        s.Flag3 = true;
        return s;
    }

    static async Task<OneOf<StateStore, Error>> Job4(StateStore s, CancellationToken ct)
    {
        await Task.Delay(100, CancellationToken.None);
        TestContext.Out.WriteLine($"{DateTime.Now:T} Job4 after delay, ct.IsCancellationRequested: {ct.IsCancellationRequested}");
        s.Flag4 = true;
        return s;
    }

    static async Task<OneOf<StateStore, Error>> Job5(StateStore s, CancellationToken ct)
    {
        await Task.Delay(100, CancellationToken.None);
        TestContext.Out.WriteLine($"{DateTime.Now:T} Job5 after delay, ct.IsCancellationRequested: {ct.IsCancellationRequested}");
        s.Flag5 = true;
        return s;
    }

    [Test]
    public async Task A_chain_of_thens_completes_all_jobs_when_all_jobs_return_success()
    {
        var cts = new CancellationTokenSource();

        static Task<OneOf<StateStore, Error>> Create() => Task.FromResult((OneOf<StateStore, Error>)new StateStore());

        var result = await Create()
            .Then(Job1, cts.Token)
            .Then(Job2, cts.Token)
            .Then(Job3, cts.Token);

        Assert.That(result.AsT0.Flag1, Is.True);
        Assert.That(result.AsT0.Flag2, Is.True);
        Assert.That(result.AsT0.Flag3, Is.True);
    }

    [Test]
    public async Task A_chain_of_thens_returns_TFailure_as_soon_as_a_job_fails()
    {
        var cts = new CancellationTokenSource();

        var result = await Create()
            .Then(Job1, cts.Token)
            .Then(Job2WhichReturnsError, cts.Token)
            .Then(Job3WhichThrows, cts.Token);

        Assert.That(result.IsT1, Is.True);
        return;

        async Task<OneOf<StateStore, Error>> Job2WhichReturnsError(StateStore s, CancellationToken ct)
        {
            await Task.Delay(100, CancellationToken.None);
            return new Error();
        }

        async Task<OneOf<StateStore, Error>> Job3WhichThrows(StateStore s, CancellationToken ct)
        {
            await Task.Delay(100, CancellationToken.None);
            s.Flag3 = true;
            throw new Exception("Job3 should not be run");
        }

        Task<OneOf<StateStore, Error>> Create() => Task.FromResult((OneOf<StateStore, Error>)new StateStore());
    }

    [Test]
    public async Task In_a_chain_of_thens_onFailure_is_called_if_a_job_fails_and_then_the_Failure_is_returned()
    {
        var job1OnFailureCalled = false;
        var job2OnFailureCalled = false;
        var job3OnFailureCalled = false;

        async Task<OneOf<StateStore, Error>> Job1OnFailure(StateStore s, Error e, CancellationToken ct)
        {
            await Task.Delay(100, CancellationToken.None);
            job1OnFailureCalled = true;
            return e;
        }

        async Task<OneOf<StateStore, Error>> Job2WhichReturnsError(StateStore s, CancellationToken ct)
        {
            await Task.Delay(100, CancellationToken.None);
            return new Error();
        }

        async Task<OneOf<StateStore, Error>> Job2OnFailure(StateStore s, Error e, CancellationToken ct)
        {
            await Task.Delay(100, CancellationToken.None);
            job2OnFailureCalled = true;
            return new Error();
        }

        async Task<OneOf<StateStore, Error>> Job3WhichThrows(StateStore s, CancellationToken ct)
        {
            await Task.Delay(100, CancellationToken.None);
            s.Flag3 = true;
            throw new Exception("Job3 should not be run");
        }

        async Task<OneOf<StateStore, Error>> Job3OnFailure(StateStore s, Error e, CancellationToken ct)
        {
            await Task.Delay(100, CancellationToken.None);
            job3OnFailureCalled = true;
            return e;
        }

        var cts = new CancellationTokenSource();
        Task<OneOf<StateStore, Error>> Create() => Task.FromResult((OneOf<StateStore, Error>)new StateStore());

        var result = await Create()
            .Then(Job1, Job1OnFailure, cts.Token)
            .Then(Job2WhichReturnsError, Job2OnFailure, cts.Token)
            .Then(Job3WhichThrows, Job3OnFailure, cts.Token);

        Assert.That(result.IsT1, Is.True);
        Assert.That(job1OnFailureCalled, Is.False);
        Assert.That(job2OnFailureCalled, Is.True);
        Assert.That(job3OnFailureCalled, Is.False);
    }

    [Test]
    public async Task ToResult_converts_T_into_success_if_last_job_is_successful()
    {
        var cts = new CancellationTokenSource();

        static Task<OneOf<StateStore, Error>> Create() => Task.FromResult((OneOf<StateStore, Error>)new StateStore());

        var result = await Create().ToResult(_ => new Success(), cts.Token);

        Assert.That(result.IsT0, Is.True);
    }

    [Test]
    public async Task ToResult_returns_TFailure_if_last_job_returns_a_TFailure()
    {
        var cts = new CancellationTokenSource();
        static Task<OneOf<StateStore, Error>> Create() => Task.FromResult((OneOf<StateStore, Error>)new Error());

        var result = await Create().ToResult(_ => new Success(), cts.Token);

        Assert.That(result.IsT1, Is.True);
    }

    [Test]
    public async Task ThenWaitForAll_should_execute_all_tasks_before_returning()
    {
        var cts = new CancellationTokenSource();
        var state = Task.FromResult((OneOf<StateStore, Error>)new StateStore());

        var result = await state
            .Then(Job1, cts.Token)
            .ThenWaitForAll(cts.Token, true, Job2, (s, ct) => Job3(s, ct).Then(Job4, cts.Token))
            .Then(Job5, cts.Token);

        Assert.That(result.AsT0.Flag1, Is.True);
        Assert.That(result.AsT0.Flag2, Is.True);
        Assert.That(result.AsT0.Flag3, Is.True);
        Assert.That(result.AsT0.Flag4, Is.True);
        Assert.That(result.AsT0.Flag5, Is.True);
    }

    [Test]
    public async Task ThenWaitForAll_should_throw_when_the_cancellationToken_is_cancelled()
    {
        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(90));
        var state = Task.FromResult((OneOf<StateStore, Error>)new StateStore());
        
        await Assert.ThatAsync(async () => 
            await state.ThenWaitForAll(cts.Token, true, Job2, (s, ct) => Job3(s, ct).Then(Job4, cts.Token)), 
            Throws.TypeOf<OperationCanceledException>());
    }

    [Test]
    public async Task OperationCanceledException_is_thrown_from_within_Then_when_the_CancellationToken_is_cancelled_by_a_timeout()
    {
        static Task<OneOf<StateStore, Error>> Create() => Task.FromResult((OneOf<StateStore, Error>)new StateStore());
        
        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(120));

        await Assert.ThatAsync(async () => await Create()
            .Then(Job1, cts.Token)
            .Then(Job2, cts.Token)
            .Then(Job3, cts.Token), Throws.TypeOf<OperationCanceledException>());
    }

    [Test]
    public async Task ThenWaitForAll_should_execute_all_tasks_before_returning_Error_if_one_of_the_tasks_returns_an_error()
    {
        static async Task<OneOf<StateStore, Error>> Job2WhichReturnsError(StateStore s, CancellationToken ct)
        {
            await Task.Delay(100, CancellationToken.None);
            return new Error();
        }

        var job4Completed = false;
        async Task<OneOf<StateStore, Error>> Job4WhichSetsFlag(StateStore s, CancellationToken ct)
        {
            await Task.Delay(500, ct);
            job4Completed = true;
            return s;
        }

        var cts = new CancellationTokenSource();
        var state = Task.FromResult((OneOf<StateStore, Error>)new StateStore());

        var result = await state
            .Then(Job1, cts.Token)
            .ThenWaitForAll(cts.Token, true, Job2WhichReturnsError, (s, ct) => Job3(s, ct).Then(Job4WhichSetsFlag, cts.Token))
            .Then(Job5, cts.Token);

        Assert.That(result.IsT1);
        Assert.That(job4Completed);
    }

    [Test]
    public async Task ThenWaitForAll_should_execute_all_tasks_and_use_a_supplied_merging_strategy()
    {
        var cts = new CancellationTokenSource();
        var state = Task.FromResult((OneOf<StateStore, Error>)new StateStore());

        // todo think of more meaningful strategy to test...
        static OneOf<StateStore, Error> TaskResultMergingStrategy(StateStore input, CancellationToken ct, IEnumerable<OneOf<StateStore, Error>> results)
        {
            return results.First();
        }

        var result = await state
            .Then(Job1, cts.Token)
            .ThenWaitForAll(TaskResultMergingStrategy, cts.Token, true, Job2, (s, ct) => Job3(s, ct).Then(Job4, cts.Token))
            .Then(Job5, cts.Token);

        Assert.That(result.AsT0.Flag1, Is.True);
        Assert.That(result.AsT0.Flag2, Is.True);
        Assert.That(result.AsT0.Flag3, Is.True);
        Assert.That(result.AsT0.Flag4, Is.True);
        Assert.That(result.AsT0.Flag5, Is.True);
    }
    
    [Test]
    public async Task A_final_long_running_task_should_cancel_gracefully()
    {
        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(160));
        var state = Task.FromResult((OneOf<StateStore, Error>)new StateStore());

        static async Task<OneOf<StateStore, Error>> CancellableLongRunningJob(StateStore s, CancellationToken ct)
        {
            var count = 0;
            while (ct.IsCancellationRequested == false)
            {
                await Task.Delay(100, CancellationToken.None);
                count+= 1;
            }
            
            s.Count = count;
            return s;
        }

        var result = await state
            .Then(Job1, cts.Token)
            .Then(CancellableLongRunningJob, cts.Token);

        Assert.That(result.AsT0.Count, Is.GreaterThanOrEqualTo(1));
    }

    [Test]
    public async Task A_long_running_task_should_cancel_gracefully()
    {
        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(160));
        var state = Task.FromResult((OneOf<StateStore, Error>)new StateStore());

        static async Task<OneOf<StateStore, Error>> CancellableLongRunningJob(StateStore s, CancellationToken ct)
        {
            var count = 0;
            while (true)
            {
                if (ct.IsCancellationRequested)
                {
                    s.Count = count;
                    return s;
                }
                await Task.Delay(100, CancellationToken.None);
                count += 1;
            }
        }

        var result = await state
            .Then(Job1, cts.Token)
            .Then(CancellableLongRunningJob, cts.Token)
            .Then(Job2, cts.Token, throwOnCancellation:false);

        Assert.That(result.AsT0.Count, Is.GreaterThanOrEqualTo(1));
    }

    [Test]
    public async Task ThenWaitForFirst_should_return_after_first_task_completes()
    {
        static async Task<OneOf<StateStore, Error>> ReallyLongRunningJob3(StateStore s, CancellationToken ct)
        {
            await Task.Delay(3000, CancellationToken.None);
            s.Flag3 = true;
            return s;
        }

        var cts = new CancellationTokenSource();
        var state = Task.FromResult((OneOf<StateStore, Error>)new StateStore());

        var result = await state
            .Then(Job1, cts.Token)
            .ThenWaitForFirst(cts.Token, true, Job2, ReallyLongRunningJob3)
            .Then(Job4, cts.Token);

        Assert.That(result.AsT0.Flag1, Is.True, "Flag1 should be true");
        Assert.That(result.AsT0.Flag2, Is.True, "Flag2 should be true");
        Assert.That(result.AsT0.Flag3, Is.False, "Flag3 should be false");
        Assert.That(result.AsT0.Flag4, Is.True, "Flag4 should be true");
    }

    [Test]
    public async Task ThenWaitForFirst_should_cancel_others_after_first_task_completes()
    {
        static async Task<OneOf<StateStore, Error>> ReallyLongRunningJob3(StateStore s, CancellationToken ct)
        {
            await Task.Delay(3000, ct);
            s.Flag3 = true;
            return s;
        }

        var cts = new CancellationTokenSource();
        var state = Task.FromResult((OneOf<StateStore, Error>)new StateStore());

        var result = await state
            .ThenWaitForFirst(cts.Token, true, Job2, ReallyLongRunningJob3);
        
        Assert.That(result.AsT0.Flag2, Is.True, "Flag2 should be true");
        Assert.That(result.AsT0.Flag3, Is.False, "Flag3 should be false");
    }

    [Test]
    public async Task ThenWaitForFirst_should_return_Error_after_first_task_completes_if_it_returns_an_error()
    {
        async Task<OneOf<StateStore, Error>> Job2WhichReturnsError(StateStore s, CancellationToken ct)
        {
            await Task.Delay(100, CancellationToken.None);
            return new Error();
        }

        async Task<OneOf<StateStore, Error>> ReallyLongRunningJob3(StateStore s, CancellationToken ct)
        {
            await Task.Delay(3000, CancellationToken.None);
            s.Flag3 = true;
            return s;
        }

        var cts = new CancellationTokenSource();
        var state = Task.FromResult((OneOf<StateStore, Error>)new StateStore());

        var result = await state
            .Then(Job1, cts.Token)
            .ThenWaitForFirst(cts.Token, true, Job2WhichReturnsError, ReallyLongRunningJob3)
            .Then(Job4, cts.Token);

        Assert.That(result.IsT1);
    }
}