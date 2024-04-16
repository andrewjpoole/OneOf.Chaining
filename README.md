![Nuget](https://img.shields.io/nuget/v/OneOf.Chaining?label=nuget%20version)
![Nuget](https://img.shields.io/nuget/dt/OneOf.Chaining?label=nuget%20downloads)

# OneOf.Chaining
Extension methods which enable method chaining in C#, using the excellent OneOf library.

Turn this:
```csharp
public async Task<OneOf<WeatherReport, Failure>> Handle(string requestedRegion, DateTime requestedDate)
{
	var isValidRequest = await regionValidator.Validate(requestedRegion);
	if (!isValidRequest)
		return new UnsupportedRegionFailure();

	var dateCheckPassed = await dateChecker.CheckDate(requestedDate);
	if (!dateCheckPassed)
		return new InvalidRequestFailure();

	var cacheCheckResult = CheckCache(requestedRegion, requestedDate);
	if (cacheCheckResult.Hit)
		return cacheCheckResult.Data;
	else
		return weatherForecastGenerator.Generate(requestedRegion, requestedDate);
}
```

into this:
```csharp
public async Task<OneOf<WeatherReport, Failure>> Handle(string requestedRegion, DateTime requestedDate)
{
	return await WeatherReport.Create(requestedRegion, requestedDate)
		.Then(regionValidator.ValidateRegion)
		.Then(dateChecker.CheckDate)
		.Then(CheckCache)
		.IfThen(report => report.PopulatedFromCache is false, 
			weatherForecastGenerator.Generate);
}
```

For an explanation, see my blog post [here](https://forkinthecode.net/2023/07/19/async-method-chaining.html) or on [ClearBank's Medium publication here](https://medium.com/clearbank/async-method-chaining-in-c-8f15d162bcee)

Includes 
* `Then()` which enables fluent chaining of any method which returns a `Task<OneOf<Tsuccess, Tfailure>>`.
* Overload of `Then()` which takes an onFailure func, useful for tidying up previous tasks. Also able to mutate the Tfailure result.
* `IfThen()` which takes a func of Tsuccess and bool, which should return true in order to invoke the nextJob.
* `ThenWaitForAll()` and `ThenWaitForFirst()` methods which accept a list of tasks to be executed in parallel.
* Versions of all extension methods with Cancellation support
