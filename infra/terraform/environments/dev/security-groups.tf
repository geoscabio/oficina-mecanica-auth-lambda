resource "aws_security_group" "lambda" {
  name        = "${var.function_name}-sg"
  description = "Acesso de saida da Auth Lambda da Oficina Mecanica"
  vpc_id      = data.aws_ssm_parameter.vpc_id.value
  tags        = local.common_tags
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

resource "aws_vpc_security_group_ingress_rule" "rds_sql_server_from_lambda" {
  description                  = "SQL Server do security group da Auth Lambda para o RDS"
  security_group_id            = data.aws_ssm_parameter.rds_security_group_id.value
  referenced_security_group_id = aws_security_group.lambda.id
  ip_protocol                  = "tcp"
  from_port                    = 1433
  to_port                      = 1433

  depends_on = [terraform_data.rds_ready]
}
