using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;


namespace FunctionApp.Function
{
	public static class Function
	{
		[FunctionName("Function")]
		public static void Run(
				[CosmosDBTrigger(
						databaseName: "database",
						collectionName: "collection",
						ConnectionStringSetting = "CosmosConnection",
						LeaseCollectionName = "leases")]
				IReadOnlyList<Document> input,
				TraceWriter log)
		{
			log.Verbose("Modified Documents: " + (input?.Count.ToString() ?? "null!"));
			log.Verbose(
					"First document Id: "
					+ (input?[0]
							.Id
					?? "null!"));
		}
	}
}
