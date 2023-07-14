using OneOf.Types;

namespace OneOf.Chaining.Tests;

public class AsynchronousMethodChainingTests
{
    [Test]
    public async Task A_chain_of_thens_completes_all_jobs_when_all_jobs_return_success()
    {
        async Task<OneOf<StateStore, Error>> Job1(StateStore s)
        {
            await Task.Delay(100);
            s.Flag1 = true;
            return s;
        }

        async Task<OneOf<StateStore, Error>> Job2(StateStore s)
        {
            await Task.Delay(100);
            s.Flag2 = true;
            return s;
        }

        async Task<OneOf<StateStore, Error>> Job3(StateStore s)
        {
            await Task.Delay(100);
            s.Flag3 = true;
            return s;
        }

        Task <OneOf<StateStore, Error>> Create() => Task.FromResult((OneOf<StateStore, Error>)new StateStore());

        var result = await Create()
            .Then(s => Job1(s))
            .Then(s => Job2(s))
            .Then(s => Job3(s));

        Assert.That(result.AsT0.Flag1, Is.True);
        Assert.That(result.AsT0.Flag2, Is.True);
        Assert.That(result.AsT0.Flag3, Is.True);
    }

    [Test]
    public async Task A_chain_of_thens_TFailure_as_soon_as_a_job_fails()
    {
        async Task<OneOf<StateStore, Error>> Job1(StateStore s)
        {
            await Task.Delay(100);
            s.Flag1 = true;
            return s;
        }

        async Task<OneOf<StateStore, Error>> Job2(StateStore s)
        {
            await Task.Delay(100);
            return new Error();
        }

        async Task<OneOf<StateStore, Error>> Job3(StateStore s)
        {
            await Task.Delay(100);
            s.Flag3 = true;
            throw new Exception("Job3 should not be run");
        }

        Task<OneOf<StateStore, Error>> Create() => Task.FromResult((OneOf<StateStore, Error>)new StateStore());

        var result = await Create()
            .Then(s => Job1(s))
            .Then(s => Job2(s))
            .Then(s => Job3(s));

        Assert.That(result.IsT1, Is.True);
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
}

public class StateStore
{
    public bool Flag1 { get; set; }
    public bool Flag2 { get; set; }
    public bool Flag3 { get; set; }
}

