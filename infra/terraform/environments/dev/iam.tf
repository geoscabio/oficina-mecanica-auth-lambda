data "aws_iam_role" "lambda_execution" {
  name = var.lambda_execution_role_name
}

resource "aws_iam_role_policy" "datadog_api_key_read" {
  name = "${var.function_name}-datadog-api-key-read"
  role = data.aws_iam_role.lambda_execution.name

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect   = "Allow"
        Action   = "secretsmanager:GetSecretValue"
        Resource = aws_secretsmanager_secret.datadog_api_key.arn
      }
    ]
  })
}
