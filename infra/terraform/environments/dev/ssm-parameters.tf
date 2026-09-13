resource "aws_ssm_parameter" "function_arn" {
  name        = "${var.auth_lambda_ssm_prefix}/function_arn"
  description = "ARN da Auth Lambda usado pela esteira de API Gateway."
  type        = "String"
  value       = aws_lambda_function.auth.arn
}

resource "aws_ssm_parameter" "function_name" {
  name        = "${var.auth_lambda_ssm_prefix}/function_name"
  description = "Nome da Auth Lambda usado por esteiras dependentes."
  type        = "String"
  value       = aws_lambda_function.auth.function_name
}

resource "aws_ssm_parameter" "security_group_id" {
  name        = "${var.auth_lambda_ssm_prefix}/security_group_id"
  description = "Security group da Auth Lambda usado por esteiras dependentes."
  type        = "String"
  value       = aws_security_group.lambda.id
}

resource "aws_ssm_parameter" "status" {
  # Publicar ready somente após concluir a infraestrutura e seus contratos SSM.
  depends_on = [
    aws_lambda_function.auth,
    aws_ssm_parameter.function_arn,
    aws_ssm_parameter.function_name,
    aws_ssm_parameter.security_group_id,
  ]

  name        = var.auth_lambda_status_parameter_name
  description = "Status operacional da Auth Lambda."
  type        = "String"
  value       = "ready"
}
