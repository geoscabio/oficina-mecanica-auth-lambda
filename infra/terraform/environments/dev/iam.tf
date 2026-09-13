data "aws_iam_role" "lambda_execution" {
  name = var.lambda_execution_role_name
}
