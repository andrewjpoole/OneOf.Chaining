using System.Collections;
using OneOf.Types;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace OneOf.Chaining.Tests;

public class AsynchronousMethodChainingTests
{
    async Task<OneOf<StateStore, Error>> Job1(StateStore s)
    {
        await Task.Delay(100);
        s.Flag1 = true;
        return s;
    }

    async Task<OneOf<StateStore, Error>> Job2(StateStore s)
    {
        await Task.Delay(200);
        s.Flag2 = true;
        return s;
    }

    async Task<OneOf<StateStore, Error>> Job3(StateStore s)
    {
        await Task.Delay(300);
        s.Flag3 = true;
        return s;
    }

    async Task<OneOf<StateStore, Error>> Job4(StateStore s)
    {
        await Task.Delay(400);
        s.Flag4 = true;
        return s;
    }

    async Task<OneOf<StateStore, Error>> Job5(StateStore s)
    {
        await Task.Delay(500);
        s.Flag5 = true;
        return s;
    }

    [Test]
    public async Task A_chain_of_thens_completes_all_jobs_when_all_jobs_return_success()
    {
        Task<OneOf<StateStore, Error>> Create() => Task.FromResult((OneOf<StateStore, Error>)new StateStore());

        var result = await Create()
            .Then(s => Job1(s))
            .Then(s => Job2(s))
            .Then(s => Job3(s));

        Assert.That(result.AsT0.Flag1, Is.True);
        Assert.That(result.AsT0.Flag2, Is.True);
        Assert.That(result.AsT0.Flag3, Is.True);
    }

    [Test]
    public async Task A_chain_of_thens_returns_TFailure_as_soon_as_a_job_fails()
    {
        async Task<OneOf<StateStore, Error>> Job2WhichReturnsError(StateStore s)
        {
            await Task.Delay(100);
            return new Error();
        }

        async Task<OneOf<StateStore, Error>> Job3WhichThrows(StateStore s)
        {
            await Task.Delay(100);
            s.Flag3 = true;
            throw new Exception("Job3 should not be run");
        }

        Task<OneOf<StateStore, Error>> Create() => Task.FromResult((OneOf<StateStore, Error>)new StateStore());

        var result = await Create()
            .Then(s => Job1(s))
            .Then(s => Job2WhichReturnsError(s))
            .Then(s => Job3WhichThrows(s));

        Assert.That(result.IsT1, Is.True);
    }

    [Test]
    public async Task In_a_chain_of_thens_onFailure_is_called_if_a_job_fails_and_then_the_Failure_is_returned()
    {
        var job1OnFailureCalled = false;
        var job2OnFailureCalled = false;
        var job3OnFailureCalled = false;

        async Task<OneOf<StateStore, Error>> Job1OnFailure(StateStore s, Error e)
        {
            await Task.Delay(100);
            job1OnFailureCalled = true;
            return e;
        }

        async Task<OneOf<StateStore, Error>> Job2WhichReturnsError(StateStore s)
        {
            await Task.Delay(100);
            return new Error();
        }

        async Task<OneOf<StateStore, Error>> Job2OnFailure(StateStore s, Error e)
        {
            await Task.Delay(100);
            job2OnFailureCalled = true;
            return new Error();
        }

        async Task<OneOf<StateStore, Error>> Job3WhichThrows(StateStore s)
        {
            await Task.Delay(100);
            s.Flag3 = true;
            throw new Exception("Job3 should not be run");
        }

        async Task<OneOf<StateStore, Error>> Job3OnFailure(StateStore s, Error e)
        {
            await Task.Delay(100);
            job3OnFailureCalled = true;
            return e;
        }

        Task<OneOf<StateStore, Error>> Create() => Task.FromResult((OneOf<StateStore, Error>)new StateStore());

        var result = await Create()
            .Then(Job1, Job1OnFailure)
            .Then(Job2WhichReturnsError, Job2OnFailure)
            .Then(Job3WhichThrows, Job3OnFailure);

        Assert.That(result.IsT1, Is.True);
        Assert.That(job1OnFailureCalled, Is.False);
        Assert.That(job2OnFailureCalled, Is.True);
        Assert.That(job3OnFailureCalled, Is.False);
    }

    [Test]
    public async Task ToResult_converts_T_into_success_if_last_job_is_successful()
    {
        Task<OneOf<StateStore, Error>> Create() => Task.FromResult((OneOf<StateStore, Error>)new StateStore());

        var result = await Create().ToResult();

        Assert.That(result.IsT0, Is.True);
    }

    [Test]
    public async Task ToResult_converts_returns_TFailure_if_last_job_returns_a_TFailure()
    {
        Task<OneOf<StateStore, Error>> Create() => Task.FromResult((OneOf<StateStore, Error>)new Error());

        var result = await Create().ToResult();

        Assert.That(result.IsT1, Is.True);
    }

    [Test]
    public async Task ThenWaitForAll_should_execute_all_tasks_before_returning()
    {
        var state = Task.FromResult((OneOf<StateStore, Error>)new StateStore());
        
        var result = await state
            .Then(Job1)
            .ThenWaitForAll(null, Job2, s => Job3(s).Then(Job4))
            .Then(Job5);

        Assert.That(result.AsT0.Flag1, Is.True);
        Assert.That(result.AsT0.Flag2, Is.True);
        Assert.That(result.AsT0.Flag3, Is.True);
        Assert.That(result.AsT0.Flag4, Is.True);
        Assert.That(result.AsT0.Flag5, Is.True);
    }

    [Test]
    public async Task ThenWaitForAll_should_execute_all_tasks_before_returning_Error_if_one_of_the_tasks_returns_an_error()
    {
        async Task<OneOf<StateStore, Error>> Job2WhichReturnsError(StateStore s)
        {
            await Task.Delay(100);
            return new Error();
        }

        var job4Completed = false;
        async Task<OneOf<StateStore, Error>> Job4WhichSetsFlag(StateStore s)
        {
            await Task.Delay(500);
            job4Completed = true;
            return s;
        }

        var state = Task.FromResult((OneOf<StateStore, Error>)new StateStore());
        
        var result = await state
            .Then(Job1)
            .ThenWaitForAll(null, Job2WhichReturnsError, s => Job3(s).Then(Job4WhichSetsFlag))
            .Then(Job5);

        Assert.That(result.IsT1);
        Assert.That(job4Completed);
    }

    [Test]
    public async Task ThenWaitForAll_should_execute_all_tasks_and_use_a_supplied_merging_strategy()
    {
        var state = Task.FromResult((OneOf<StateStore, Error>)new StateStore());

        // todo think of more meaningful strategy...
        OneOf<StateStore, Error> TaskResultMergingStrategy(IEnumerable<OneOf<StateStore, Error>> results)
        {
            return results.First();
        }

        var result = await state
            .Then(Job1)
            .ThenWaitForAll((Func<IEnumerable<OneOf<StateStore, Error>>, OneOf<StateStore, Error>>?)TaskResultMergingStrategy, Job2, s => Job3(s).Then(Job4))
            .Then(Job5);

        Assert.That(result.AsT0.Flag1, Is.True);
        Assert.That(result.AsT0.Flag2, Is.True);
        Assert.That(result.AsT0.Flag3, Is.True);
        Assert.That(result.AsT0.Flag4, Is.True);
        Assert.That(result.AsT0.Flag5, Is.True);
    }

    [Test]
    public async Task ThenWaitForFirst_should_return_after_first_task_completes()
    {
        async Task<OneOf<StateStore, Error>> ReallyLongRunningJob3(StateStore s)
        {
            await Task.Delay(3000);
            s.Flag3 = true;
            return s;
        }

        var state = Task.FromResult((OneOf<StateStore, Error>)new StateStore());

        var result = await state
            .Then(Job1)
            .ThenWaitForFirst(Job2, ReallyLongRunningJob3)
            .Then(Job4);

        Assert.That(result.AsT0.Flag1, Is.True, "Flag1 should be true");
        Assert.That(result.AsT0.Flag2, Is.True, "Flag2 should be true");
        Assert.That(result.AsT0.Flag3, Is.False, "Flag3 should be false");
        Assert.That(result.AsT0.Flag4, Is.True, "Flag4 should be true");

    }

    [Test]
    public async Task ThenWaitForFirst_should_return_Error_after_first_task_completes_if_it_returns_an_error()
    {
        async Task<OneOf<StateStore, Error>> Job2WhichReturnsError(StateStore s)
        {
            await Task.Delay(100);
            return new Error();
        }

        async Task<OneOf<StateStore, Error>> ReallyLongRunningJob3(StateStore s)
        {
            await Task.Delay(3000);
            s.Flag3 = true;
            return s;
        }

        var state = Task.FromResult((OneOf<StateStore, Error>)new StateStore());

        var result = await state
            .Then(Job1)
            .ThenWaitForFirst(Job2WhichReturnsError, ReallyLongRunningJob3)
            .Then(Job4);

        Assert.That(result.IsT1);
    }
}

public class StateStore
{
    public bool Flag1 { get; set; }
    public bool Flag2 { get; set; }
    public bool Flag3 { get; set; }
    public bool Flag4 { get; set; }
    public bool Flag5 { get; set; }
}