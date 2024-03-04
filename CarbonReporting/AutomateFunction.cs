using CarbonReporting;
using Newtonsoft.Json.Serialization;
using Objects;
using Objects.Geometry;
using Speckle.Automate.Sdk;
using Speckle.Core.Logging;
using Speckle.Core.Models;
using Speckle.Core.Models.Extensions;

public static class AutomateFunction
{
  public static async Task Run(
    AutomationContext automationContext,
    FunctionInputs functionInputs
  )
  {
    Console.WriteLine("Starting execution");
    _ = typeof(ObjectsKit).Assembly; // INFO: Force objects kit to initialize

    Console.WriteLine("Receiving version");
    var commitObject = await automationContext.ReceiveVersion();

    Console.WriteLine("Received version: " + commitObject);

        var volume = "volume";
        var parameters = "parameters";
        var parameterVolume = "Volume";

		List<Base> volumeObjects = new();

		foreach (var b in commitObject.Flatten())
		{
			if (b[volume] is not null)
			{
				volumeObjects.Add(b);
				continue;
			}

			if (b[parameters] is Base @params && @params[parameterVolume] is not null)
			{
				volumeObjects.Add(b);
				continue;
			}
		}

		Dictionary<Base, double> calculatedVolume = new();
		List<Task> calculationTasks = new();

		foreach (Mesh m in volumeObjects.Where(b => b[volume] is double d && d == 0 && b is Mesh m).Cast<Mesh>())
		{
			Task calculationTask = Task.Run(() =>
			{
				double volume = MeshCalcs.CalculateVolume(m);
				lock (calculatedVolume)
				{
					calculatedVolume[m] = volume;
				}
			});

			calculationTasks.Add(calculationTask);
		}

		automationContext.AttachResultToObjects(
	        Speckle.Automate.Sdk.Schema.ObjectResultLevel.Info,
	        "Objects with Volume",
			volumeObjects
                .Where(x => 
                    (x[volume] is double d && d > 0) ||
                    (x[parameters] is Base b && b[parameterVolume] is double v && v > 0))
                .Select(x => x.id),
	        "Processed objects");

		await Task.WhenAll(calculationTasks);

		automationContext.AttachResultToObjects(
	        Speckle.Automate.Sdk.Schema.ObjectResultLevel.Warning,
	        "Objects with calculated volumes",
			calculatedVolume				
				.Select(x => x.Key.id),
			"Processed objects",
			calculatedVolume
				.ToDictionary(
					x => "CalculatedVolume",
					x => x.Value as object));

		automationContext.MarkRunSuccess($"Counted {volumeObjects.Count()} objects");
  }
}
