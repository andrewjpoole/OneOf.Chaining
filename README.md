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
