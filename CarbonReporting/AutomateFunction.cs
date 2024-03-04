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

		var volumeObjects = commitObject
          .Flatten(b => false)
          .Where(b => 
            (b[volume] is not null) ||
			(b[parameters] is Base revitB && revitB[parameterVolume] is not null));
          //.Select<Base>(b => b);

		automationContext.AttachResultToObjects(
	        Speckle.Automate.Sdk.Schema.ObjectResultLevel.Info,
	        "Objects with Volume",
			volumeObjects
                .Where(x => 
                    (x[volume] is double d && d > 0) ||
                    (x[parameters] is Base b && b[parameterVolume] is double v && v > 0))
                .Select(x => x.id),
	        "Processed objects");

		automationContext.AttachResultToObjects(
	        Speckle.Automate.Sdk.Schema.ObjectResultLevel.Error,
	        "Objects with 0 or negative volume",
	        volumeObjects
				.Where(x => x[volume] is double d && d <= 0)
				.Select(x => x.id),
			"Processed objects");

		automationContext.MarkRunSuccess($"Counted {volumeObjects.Count()} objects");
  }
}
