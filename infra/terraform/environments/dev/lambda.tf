resource "aws_lambda_function" "auth" {
  function_name    = var.function_name
  description      = "Auth Lambda da Oficina Mecanica para autenticacao de Cliente por documento"
  role             = aws_iam_role.lambda.arn
  handler          = local.lambda_handler
  runtime          = "dotnet10"
  filename         = var.lambda_package_file_path
  source_code_hash = var.lambda_package_source_code_hash
  memory_size      = var.lambda_memory_size
  timeout          = var.lambda_timeout_seconds

  environment {
    variables = {
      "ConnectionStrings__SqlServer" = var.connection_string
      "Jwt__Issuer"                  = var.jwt_issuer
      "Jwt__Audience"                = var.jwt_audience
      "Jwt__Secret"                  = var.jwt_secret
      "Jwt__ExpirationMinutes"       = tostring(var.jwt_expiration_minutes)
    }
  }

  vpc_config {
    subnet_ids         = local.private_subnet_ids
    security_group_ids = [aws_security_group.lambda.id]
  }

  depends_on = [
    aws_cloudwatch_log_group.lambda,
    aws_iam_role_policy_attachment.lambda_basic_execution,
    aws_iam_role_policy_attachment.lambda_vpc_access_execution,
    aws_vpc_security_group_egress_rule.lambda_sql_server_to_rds,
    terraform_data.vpc_ready,
    terraform_data.rds_ready,
  ]
}
