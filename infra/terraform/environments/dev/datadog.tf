resource "aws_secretsmanager_secret" "datadog_api_key" {
  name                    = "oficina-mecanica/${var.environment}/datadog/auth-lambda-api-key"
  description             = "API key do Datadog usada exclusivamente pela Auth Lambda"
  recovery_window_in_days = 0

  tags = merge(local.common_tags, {
    Name = "oficina-mecanica-${var.environment}-datadog-auth-lambda-api-key"
  })
}
