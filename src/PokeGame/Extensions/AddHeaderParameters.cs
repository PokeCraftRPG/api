using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PokeGame.Extensions;

internal class AddHeaderParameters : IOperationFilter
{
  public void Apply(OpenApiOperation operation, OperationFilterContext context)
  {
    operation.Parameters?.Add(new OpenApiParameter
    {
      In = ParameterLocation.Header,
      Name = "X-World", // TODO(fpion): ResolveWorld.Header,
      Description = "Enter your world ID in the input below:"
    });
  }
}
