using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using FluentAssertions;
using OficinaMecanica.AuthLambda.Function;

namespace OficinaMecanica.AuthLambda.Function.UnitTests;

public sealed class LambdaSerializerConfigurationTests
{
    [Fact]
    public void Assembly_DaFunction_DeveRegistrarDefaultLambdaJsonSerializer()
    {
        var serializer = typeof(Function).Assembly
            .GetCustomAttributes(typeof(LambdaSerializerAttribute), inherit: false)
            .Cast<LambdaSerializerAttribute>()
            .SingleOrDefault();

        serializer.Should().NotBeNull();
        serializer!.SerializerType.Should().Be(typeof(DefaultLambdaJsonSerializer));
    }
}
