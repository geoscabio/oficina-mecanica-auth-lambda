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

  datadog_aws_account_id          = "464622532012"
  datadog_dotnet_layer_version    = 25
  datadog_extension_layer_version = 99

  datadog_dotnet_layer_arn    = "arn:aws:lambda:${var.aws_region}:${local.datadog_aws_account_id}:layer:dd-trace-dotnet:${local.datadog_dotnet_layer_version}"
  datadog_extension_layer_arn = "arn:aws:lambda:${var.aws_region}:${local.datadog_aws_account_id}:layer:Datadog-Extension:${local.datadog_extension_layer_version}"
}
