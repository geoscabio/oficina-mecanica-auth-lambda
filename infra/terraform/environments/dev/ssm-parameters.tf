resource "aws_ssm_parameter" "function_arn" {
  name        = "${var.auth_lambda_ssm_prefix}/function_arn"
  description = "ARN da Auth Lambda usado pela esteira de API Gateway."
  type        = "String"
  value       = aws_lambda_function.auth.arn
  tags        = local.common_tags
}

resource "aws_ssm_parameter" "function_name" {
  name        = "${var.auth_lambda_ssm_prefix}/function_name"
  description = "Nome da Auth Lambda usado por esteiras dependentes."
  type        = "String"
  value       = aws_lambda_function.auth.function_name
  tags        = local.common_tags
}

resource "aws_ssm_parameter" "security_group_id" {
  name        = "${var.auth_lambda_ssm_prefix}/security_group_id"
  description = "Security group da Auth Lambda usado por esteiras dependentes."
  type        = "String"
  value       = aws_security_group.lambda.id
  tags        = local.common_tags
}

resource "aws_ssm_parameter" "status" {
  # Publicar ready somente após criar a infraestrutura da Lambda e seus outputs SSM.
  # Este status não indica que o API Gateway público ou o fluxo end-to-end estão prontos.
  depends_on = [
    aws_lambda_function.auth,
    aws_ssm_parameter.function_arn,
    aws_ssm_parameter.function_name,
    aws_ssm_parameter.security_group_id,
  ]

  name        = var.auth_lambda_status_parameter_name
  description = "Status da infraestrutura da Auth Lambda e seus outputs SSM; não representa API Gateway público pronto."
  type        = "String"
  value       = "ready"
  tags        = local.common_tags
}
