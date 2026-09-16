resource "aws_lambda_function" "auth" {
  function_name    = var.function_name
  description      = "Auth Lambda da Oficina Mecanica para autenticacao de Cliente por documento"
  role             = data.aws_iam_role.lambda_execution.arn
  handler          = local.lambda_handler
  runtime          = "dotnet10"
  filename         = var.lambda_package_file_path
  source_code_hash = var.lambda_package_source_code_hash
  memory_size      = var.lambda_memory_size
  timeout          = var.lambda_timeout_seconds
  layers = [
    local.datadog_dotnet_layer_arn,
    local.datadog_extension_layer_arn,
  ]
  tags = local.common_tags

  environment {
    variables = {
      "ConnectionStrings__SqlServer" = var.connection_string
      "Jwt__Issuer"                  = var.jwt_issuer
      "Jwt__Audience"                = var.jwt_audience
      "Jwt__Secret"                  = var.jwt_secret
      "Jwt__ExpirationMinutes"       = tostring(var.jwt_expiration_minutes)
      "AWS_LAMBDA_EXEC_WRAPPER"      = "/opt/datadog_wrapper"
      "DD_API_KEY_SECRET_ARN"        = aws_secretsmanager_secret.datadog_api_key.arn
      "DD_ENV"                       = var.environment
      "DD_LOGS_INJECTION"            = "true"
      "DD_SERVICE"                   = "oficina-mecanica-auth-lambda"
      "DD_SITE"                      = "datadoghq.com"
      "DD_TRACE_ENABLED"             = "true"
      "DD_VERSION"                   = var.datadog_version
    }
  }

  vpc_config {
    subnet_ids         = local.private_subnet_ids
    security_group_ids = [aws_security_group.lambda.id]
  }

  depends_on = [
    aws_cloudwatch_log_group.lambda,
    aws_iam_role_policy.datadog_api_key_read,
    aws_vpc_security_group_egress_rule.lambda_sql_server_to_rds,
    terraform_data.vpc_ready,
    terraform_data.rds_ready,
  ]
}
