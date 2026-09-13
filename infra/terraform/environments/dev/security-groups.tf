resource "aws_security_group" "lambda" {
  name        = "${var.function_name}-sg"
  description = "Acesso de saida da Auth Lambda da Oficina Mecanica"
  vpc_id      = data.aws_ssm_parameter.vpc_id.value
}

resource "aws_vpc_security_group_egress_rule" "lambda_sql_server_to_rds" {
  description                  = "SQL Server da Auth Lambda para o security group do RDS"
  security_group_id            = aws_security_group.lambda.id
  referenced_security_group_id = data.aws_ssm_parameter.rds_security_group_id.value
  ip_protocol                  = "tcp"
  from_port                    = 1433
  to_port                      = 1433

  depends_on = [terraform_data.rds_ready]
}
