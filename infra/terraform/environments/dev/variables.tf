variable "aws_region" {
  description = "Região AWS onde a Auth Lambda será provisionada."
  type        = string
  default     = "us-east-1"
}

variable "environment" {
  description = "Nome do ambiente provisionado."
  type        = string
  default     = "development"
}

variable "function_name" {
  description = "Nome da função Lambda de autenticação."
  type        = string
  default     = "oficina-mecanica-auth-lambda-dev"
}

variable "lambda_package_file_path" {
  description = "Caminho do pacote .zip da Lambda gerado pelo CI/CD futuro."
  type        = string
  default     = "../../../../artifacts/auth-lambda/oficina-mecanica-auth-lambda.zip"
}

variable "lambda_package_source_code_hash" {
  description = "Hash base64 SHA256 do pacote .zip da Lambda, preenchido pelo CD futuro."
  type        = string
  default     = null
}

variable "lambda_memory_size" {
  description = "Memória configurada para a Lambda."
  type        = number
  default     = 256
}

variable "lambda_timeout_seconds" {
  description = "Timeout da Lambda em segundos."
  type        = number
  default     = 30
}

variable "datadog_version" {
  description = "Versão implantada publicada no unified service tagging do Datadog."
  type        = string
  default     = "unknown"
}

variable "lambda_execution_role_name" {
  description = "Nome da IAM Role existente usada pela Lambda. A role deve permitir execução Lambda, CloudWatch Logs e VPC ENI."
  type        = string
}

variable "connection_string" {
  description = "Connection string SQL Server usada pela Auth Lambda. Valor real deve vir de secret no CD futuro."
  type        = string
  sensitive   = true
}

variable "jwt_issuer" {
  description = "Issuer esperado para os tokens JWT emitidos pela Auth Lambda."
  type        = string
  default     = "oficina-mecanica-auth"
}

variable "jwt_audience" {
  description = "Audience esperada pela API para tokens JWT emitidos pela Auth Lambda."
  type        = string
  default     = "oficina-mecanica-api"
}

variable "jwt_secret" {
  description = "Segredo HMAC-SHA256 usado para assinatura dos tokens JWT. Valor real deve vir de secret no CD futuro."
  type        = string
  sensitive   = true
}

variable "jwt_expiration_minutes" {
  description = "Tempo de expiração dos tokens JWT emitidos pela Auth Lambda."
  type        = number
  default     = 60
}

variable "vpc_ssm_prefix" {
  description = "Prefixo SSM publicado pela esteira de VPC."
  type        = string
  default     = "/oficina-mecanica/development/vpc"
}

variable "vpc_status_parameter_name" {
  description = "Parâmetro SSM que marca a VPC como pronta."
  type        = string
  default     = "/oficina-mecanica/development/status/vpc"
}

variable "rds_ssm_prefix" {
  description = "Prefixo SSM publicado pela esteira de RDS."
  type        = string
  default     = "/oficina-mecanica/development/rds"
}

variable "rds_status_parameter_name" {
  description = "Parâmetro SSM que marca o RDS como pronto."
  type        = string
  default     = "/oficina-mecanica/development/status/rds"
}

variable "auth_lambda_ssm_prefix" {
  description = "Prefixo SSM usado para publicar outputs da Auth Lambda."
  type        = string
  default     = "/oficina-mecanica/development/auth-lambda"
}

variable "auth_lambda_status_parameter_name" {
  description = "Parâmetro SSM que marca a Auth Lambda como pronta."
  type        = string
  default     = "/oficina-mecanica/development/status/auth-lambda"
}
