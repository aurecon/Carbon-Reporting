using Speckle.Automate.Sdk;
using Speckle.Automate.Sdk.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonReporting.Tests
{

	[TestFixture]
	public class AutomateTest2
	{

		[Test]
		public async Task Do()
		{
			AutomationRunData runData = new()
			{
				ProjectId = "",
				ModelId = "",
				BranchName = "",
				VersionId = "",
				SpeckleServerUrl = "",
				AutomationId = "",
				AutomationRevisionId = "",
				AutomationRunId = "",
				FunctionId = "",
				FunctionRelease = "",
				FunctionName = "",
				FunctionLogo = "",
			};
			string token = "";

			var context = await AutomationContext.Initialize(runData, token);

			var inputs = new FunctionInputs();

			await AutomateFunction.Run(context, inputs);
		}
	}
}
