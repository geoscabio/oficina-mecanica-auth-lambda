locals {
  project_name = "OficinaMecanica"

  common_tags = {
    Project     = local.project_name
    Environment = var.environment
    ManagedBy   = "Terraform"
    Repository  = "oficina-mecanica-auth-lambda"
  }

  lambda_handler     = "OficinaMecanica.AuthLambda.Function::OficinaMecanica.AuthLambda.Function.Function::Handler"
  private_subnet_ids = split(",", data.aws_ssm_parameter.private_subnet_ids.value)

  datadog_dotnet_layer_arn    = "arn:aws:lambda:${var.aws_region}:464622532012:layer:dd-trace-dotnet:25"
  datadog_extension_layer_arn = "arn:aws:lambda:${var.aws_region}:464622532012:layer:Datadog-Extension:99"
}
