using OneOf.Chaining.Examples.Application.Models.Requests;

namespace OneOf.Chaining.Examples.Application.Services;

public class WeatherDataValidator : IWeatherDataValidator
{
    public bool Validate(CollectedWeatherDataModel model, out IDictionary<string, string[]> errors)
    {
        errors = new Dictionary<string, string[]>();

        if (model == null)
        {
            errors.Add("model", new []{ "must not be null." });
            return false;
        }

        if (model.Points is null)
        {
            errors.Add("model.Points", new []{ "must not be null." });
            return false;
        }

        for (var index = 0; index < model.Points.Count; index++)
        {
            var point = model.Points[index];
            if (point is null)
            {
                errors.Add($"model.Points[{index}]", new []{ "must not be null." });
                continue;
            }

            var errorsForThisPoint = new List<string>();

            if(string.IsNullOrWhiteSpace(point.WindDirection))
                errorsForThisPoint.Add("WindDirection must not be null or whitespace.");

            if(point.HumidityReadingInPercent <= 0)
                errorsForThisPoint.Add("HumidityReadingInPercent must be positive.");

            if (point.WindSpeedInMetersPerSecond <= 0)
                errorsForThisPoint.Add("WindSpeedInMetersPerSecond must be positive.");

            if(point.time == DateTimeOffset.MinValue)
                errorsForThisPoint.Add("time must be valid.");

            errors.Add($"model.Points[{index}]", errorsForThisPoint.ToArray());
        }
        
        return true;
    }
}

public interface IWeatherDataValidator
{
    bool Validate(CollectedWeatherDataModel model, out IDictionary<string, string[]> errors);
}