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

		Dictionary<string, double> calculatedVolume = new();
		List<Task> calculationTasks = new();

		foreach (Mesh m in volumeObjects.Where(b => b[volume] is double d && d == 0 && b is Mesh m).Cast<Mesh>())
		{
			Task calculationTask = Task.Run(() =>
			{
				var id = m.id;
				double volume = MeshCalcs.CalculateVolume(m);
				lock (calculatedVolume)
				{
					calculatedVolume[id] = volume;
				}
			});

			calculationTasks.Add(calculationTask);
		}

		var outObjects = volumeObjects
				.Where(x =>
					(x[volume] is double d && d > 0))
				//(x[parameters] is Base b && b[parameterVolume] is double v && v > 0))
				.Select(x => x);

		automationContext.AttachResultToObjects(
			Speckle.Automate.Sdk.Schema.ObjectResultLevel.Info,
			"Objects with Volume",
			outObjects.Select(x => x.id),
			"Processed objects",

			new Dictionary<string, object>()
			{
				{"Object ID Mapping","One-to-one" },
				{"Volume",outObjects.Select(x => x[volume])}
			}

			);

		await Task.WhenAll(calculationTasks);

		automationContext.AttachResultToObjects(
			Speckle.Automate.Sdk.Schema.ObjectResultLevel.Warning,
			"Objects with calculated volumes",
			calculatedVolume
				.Select(x => x.Key),
			"Processed objects",

			new Dictionary<string, object>()
			{
				{"Object ID Mapping","One-to-one" },
				{"Volume",calculatedVolume.Select(x => x.Value)}
			});

		automationContext.MarkRunSuccess($"Counted {volumeObjects.Count} objects");
	}
}
