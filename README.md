# Oficina Mecânica — Auth Lambda

Autenticação da solução Oficina Mecânica por documento: emite JWT para o cliente
encontrado no RDS. A visão de entrada da solução está no
[README da API](https://github.com/geoscabio/oficina-mecanica-api#readme).

## Responsabilidade e fluxo

Este repositório provisiona a Lambda de autenticação e sua conectividade privada
com o RDS. A rota `POST /auth/documento` é exposta pelo API Gateway; não existe
Lambda Function URL pública.

`API Gateway -> Auth Lambda (VPC) -> RDS`

As respostas contratuais são `200`, `400`, `401` e `503`. O JWT é configurado
pelos parâmetros de issuer, audience e expiração abaixo.

## Repositórios da solução

| Repositório | Responsabilidade |
|---|---|
| [API](https://github.com/geoscabio/oficina-mecanica-api) | Aplicação .NET e ponto de entrada da documentação. |
| [Auth Lambda](https://github.com/geoscabio/oficina-mecanica-auth-lambda) | Autenticação por documento e emissão de JWT. |
| [VPC](https://github.com/geoscabio/oficina-mecanica-infra-vpc) | Rede base compartilhada. |
| [Kubernetes](https://github.com/geoscabio/oficina-mecanica-infra-kubernetes) | EKS, ECR, NLB interno e contrato NodePort. |
| [RDS](https://github.com/geoscabio/oficina-mecanica-infra-rds) | Banco SQL Server privado e segredo mestre. |
| [API Gateway](https://github.com/geoscabio/oficina-mecanica-infra-api-gateway) | Entrada HTTP, VPC Link e integrações. |

## Tecnologias e pré-requisitos

- .NET 10, AWS Lambda, Terraform e GitHub Actions.
- AWS CLI e Terraform instalados para execução local.
- Infraestrutura VPC e RDS aplicada e com contratos SSM disponíveis.
- Credenciais AWS com acesso aos recursos previstos pelo Terraform.

## Configuração, secrets e contratos

| Nome | Tipo e escopo | Obrigatório | Finalidade |
|---|---|---:|---|
| `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY` | GitHub Environment Secrets (`development`) | Sim | Credenciais do deploy AWS. |
| `AWS_SESSION_TOKEN` | GitHub Environment Secret (`development`) | Quando as credenciais forem temporárias | Token da sessão AWS. |
| `AUTH_LAMBDA_JWT_SECRET` | GitHub Environment Secret (`development`) | Sim | Chave de assinatura JWT; não versionar. |
| `AWS_REGION` | GitHub Variable, resolvida em `development` | Sim | Região AWS do deploy. |
| `AUTH_LAMBDA_EXECUTION_ROLE_NAME` | GitHub Variable, resolvida em `development` | Sim | Nome da execution role externa da Lambda. |
| `AUTH_LAMBDA_JWT_ISSUER`, `AUTH_LAMBDA_JWT_AUDIENCE`, `AUTH_LAMBDA_JWT_EXPIRATION_MINUTES` | GitHub Variables, resolvidas em `development` | Sim | Contrato de emissão do JWT. |
| `AUTO_PR_ENABLED`, `RELEASE_BRANCH` | GitHub Variables | Não | Promoção automática e branch de release. |

O workflow consome os contratos SSM `/oficina-mecanica/development/status/vpc`,
`/vpc/vpc_id`, `/vpc/private_subnet_ids`, `/status/rds` e
`/rds/security_group_id`. Ele publica `function_arn`, `function_name`,
`security_group_id` e `status/auth-lambda` sob
`/oficina-mecanica/development/auth-lambda`.

O RDS mantém suas credenciais mestre no AWS Secrets Manager. O deploy consulta o
endpoint e o ARN do segredo publicados pelo RDS em SSM; não há connection string
nem senha de banco em GitHub Secrets ou `tfvars`.

No `develop` atual não há `DD_API_KEY` nem configuração Datadog neste workflow.
Uma integração futura deve ser documentada apenas quando estiver presente no
branch implantado.

## Execução, CI/CD e deploy

O workflow `aws-deploy.yml` resolve a ação, executa `plan` e aplica ou destrói
conforme o fluxo já existente. Localmente, execute no diretório Terraform:

```powershell
terraform fmt -check
terraform validate
terraform plan
```

O deploy exige os contratos VPC/RDS acima. A promoção continua governada pelos
workflows do repositório; este README não substitui os controles de ambiente.

## Observabilidade, testes e documentação

A Lambda usa os logs nativos do CloudWatch. Não há integração Datadog declarada
neste branch. Execute `dotnet build` e `dotnet test` para validar o código, além
de `terraform fmt -check` e `terraform validate` para a infraestrutura.

Documentação relacionada: [API principal](https://github.com/geoscabio/oficina-mecanica-api#readme),
[AWS Lambda](https://docs.aws.amazon.com/lambda/) e
[Terraform AWS provider](https://registry.terraform.io/providers/hashicorp/aws/latest/docs).
