output "auth_lambda_function_arn" {
  description = "ARN da Auth Lambda."
  value       = aws_lambda_function.auth.arn
}

output "auth_lambda_function_name" {
  description = "Nome da Auth Lambda."
  value       = aws_lambda_function.auth.function_name
}

output "auth_lambda_security_group_id" {
  description = "Security group da Auth Lambda."
  value       = aws_security_group.lambda.id
}

output "ssm_auth_lambda_prefix" {
  description = "Prefixo dos parâmetros SSM publicados pela Auth Lambda."
  value       = var.auth_lambda_ssm_prefix
}

output "ssm_auth_lambda_status_parameter_name" {
  description = "Nome do parâmetro SSM que marca a Auth Lambda como pronta."
  value       = var.auth_lambda_status_parameter_name
}
