using System.Runtime.CompilerServices;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]
[assembly: InternalsVisibleTo("OficinaMecanica.AuthLambda.Function.UnitTests")]
