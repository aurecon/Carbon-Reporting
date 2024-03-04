using Objects;
using Objects.Geometry;
using Speckle.Automate.Sdk;
using Speckle.Core.Logging;
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

        var volumeObjects = commitObject
          .Flatten(b => false)
          .Where(b => b[volume] is not null)
          .Select(b => b);

		automationContext.AttachResultToObjects(
	        Speckle.Automate.Sdk.Schema.ObjectResultLevel.Info,
	        "Objects with Volume",
			volumeObjects.Select(x => x.id),
	        "Processed objects");

    automationContext.MarkRunSuccess($"Counted {volumeObjects.Count()} objects");
  }
}
