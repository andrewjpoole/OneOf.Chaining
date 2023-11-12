namespace OneOf.Chaining.Examples.Domain.Outcomes;

public static class OneOfWrappers
{
    public static OneOf<TSuccess, Failure> ToFailure<TSuccess>(this TSuccess _, Failure failure) => OneOf<TSuccess, Failure>.FromT1(failure);
    public static OneOf<TSuccess, Failure> ToSuccess<TSuccess>(this TSuccess success) => OneOf<TSuccess, Failure>.FromT0(success);

    public static Task<OneOf<TSuccess, Failure>> ToFailureWrappedInTask<TSuccess>(this TSuccess _, Failure failure) => Task.FromResult(OneOf<TSuccess, Failure>.FromT1(failure));
    public static Task<OneOf<TSuccess, Failure>> ToSuccessWrappedInTask<TSuccess>(this TSuccess success) => Task.FromResult(OneOf<TSuccess, Failure>.FromT0(success));

}