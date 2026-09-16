# 🔐 Oficina Mecânica Auth Lambda

Serviço serverless responsável por autenticar clientes por documento e emitir JWT para integração com a API da Oficina Mecânica.

Este repositório faz parte da **Fase 3 do Tech Challenge FIAP** e evolui a arquitetura da solução separando a autenticação de cliente em uma Lambda própria, sem transferir a posse do schema, das migrations ou das regras gerais do domínio da API principal.

---

## 📌 Índice

- [✨ Visão geral](#visao-geral)
- [🎯 Objetivo da Lambda](#objetivo-da-lambda)
- [🏗️ Arquitetura](#arquitetura)
- [🧰 Tecnologias](#tecnologias)
- [📁 Estrutura do repositório](#estrutura-do-repositorio)
- [🔁 Fluxo de autenticação](#fluxo-de-autenticacao)
- [📡 Contrato HTTP esperado](#contrato-http-esperado)
- [⚙️ Variáveis de ambiente](#variaveis-de-ambiente)
- [🔒 Segurança](#seguranca)
- [🧪 Testes e qualidade](#testes-e-qualidade)
- [🌿 Git Flow e governança](#git-flow-e-governanca)
- [📍 Status atual](#status-atual)
- [🗺️ Próximos passos](#proximos-passos)
- [📝 Observações](#observacoes)

---

<a id="visao-geral"></a>

## ✨ Visão geral

A Auth Lambda complementa o ecossistema da Oficina Mecânica com um fluxo de autenticação específico para clientes. O serviço recebe um CPF/CNPJ, valida o documento, consulta o cliente na base de Atendimento e, quando o cliente está ativo, emite um token JWT compatível com a API principal.

| Responsabilidade | Descrição |
| --- | --- |
| 🔐 Autenticação | Autentica clientes por CPF/CNPJ. |
| 🧾 Consulta | Lê dados mínimos do cliente na base de Atendimento. |
| 🎫 Token | Emite JWT com `sub`, `role`, `jti` e `cliente_id`. |
| 🧱 Integração | Atende o `POST /auth/documento` exposto pelo API Gateway. |

---

<a id="objetivo-da-lambda"></a>

## 🎯 Objetivo da Lambda

O objetivo deste serviço é autenticar clientes por documento, sem duplicar regras ou ownership da API principal.

Este repo cuida de:

- receber uma requisição HTTP roteada pelo API Gateway;
- autenticar o cliente por CPF/CNPJ;
- consultar o cliente na base de Atendimento;
- validar se o cliente está ativo;
- emitir JWT HMAC-SHA256 para consumo pela `oficina-mecanica-api`;
- retornar respostas HTTP padronizadas.

O repositório também mantém a infraestrutura e o deploy da Lambda. VPC, RDS e API Gateway permanecem em esteiras próprias e são consumidos por contratos SSM.

---

<a id="arquitetura"></a>

## 🏗️ Arquitetura

O projeto segue **Clean Architecture** e preserva os bounded contexts usados no repositório principal, especialmente **Identidade** e **Atendimento**.

| Camada | Projeto | Responsabilidade |
| --- | --- | --- |
| ⚡ **Function** | `OficinaMecanica.AuthLambda.Function` | Adaptador Lambda, serialização JSON, mapeamento HTTP, inicialização da Lambda, injeção de dependências e tratamento sanitizado de erros. |
| 🧠 **Application** | `OficinaMecanica.AuthLambda.Application` | Use case de autenticação, contratos, validação, `Result<T>` e respostas de aplicação. |
| 💎 **Domain** | `OficinaMecanica.AuthLambda.Domain` | Value Object `CpfCnpj`, validação de CPF/CNPJ, mensagens e enums do contexto Atendimento. |
| 🧱 **Infrastructure** | `OficinaMecanica.AuthLambda.Infrastructure` | Repository SQL parametrizado, geração de JWT, `JwtOptions` e integrações técnicas. |

### Decisões aplicadas

| Decisão | Aplicação prática |
| --- | --- |
| **Lambda dedicada** | A autenticação de cliente por documento fica isolada em um serviço serverless. |
| **API dona do schema** | A Lambda consulta a base, mas não cria schema, migrations ou seeds. |
| **Clean Architecture** | Dependências apontam para dentro; Domain não depende de Infrastructure ou Function. |
| **DDD tático** | CPF/CNPJ é validado no domínio por Value Object. |
| **Use Case** | A regra de autenticação fica centralizada na camada Application. |
| **JWT compatível com a API** | Token emitido com issuer `oficina-mecanica-auth` e audience `oficina-mecanica-api`. |

---

<a id="tecnologias"></a>

## 🧰 Tecnologias

| Categoria | Tecnologias |
| --- | --- |
| Linguagem e plataforma | C#, .NET 10 |
| Serverless | AWS Lambda |
| Entrada HTTP planejada | Amazon API Gateway |
| Banco de dados | SQL Server |
| Acesso a dados | Microsoft.Data.SqlClient |
| Segurança | JWT HMAC-SHA256 |
| Validação | FluentValidation |
| Testes | xUnit, FluentAssertions, Moq e Coverlet |
| Qualidade | `dotnet format`, cobertura de testes e checagem de pacotes vulneráveis |
| Governança | Git Flow e GitHub Rulesets |

---

<a id="estrutura-do-repositorio"></a>

## 📁 Estrutura do repositório

```text
.
├── src/
│   ├── OficinaMecanica.AuthLambda.Function/
│   ├── OficinaMecanica.AuthLambda.Application/
│   ├── OficinaMecanica.AuthLambda.Domain/
│   └── OficinaMecanica.AuthLambda.Infrastructure/
├── tests/
│   ├── OficinaMecanica.AuthLambda.Application.UnitTests/
│   ├── OficinaMecanica.AuthLambda.Domain.UnitTests/
│   └── OficinaMecanica.AuthLambda.Function.UnitTests/
└── OficinaMecanica.AuthLambda.slnx
```

---

<a id="fluxo-de-autenticacao"></a>

## 🔁 Fluxo de autenticação

1. Lambda recebe JSON com `documento`.
2. Function desserializa o body e chama o use case.
3. Application valida a requisição.
4. Domain normaliza e valida CPF/CNPJ.
5. Infrastructure consulta o cliente em `Atendimento.Clientes`.
6. Se o cliente existir e estiver ativo, o TokenService gera o JWT.
7. Function retorna a resposta HTTP padronizada.

Clientes inexistentes e clientes inativos retornam a mesma resposta, evitando diferenciar publicamente esses estados.

---

<a id="contrato-http-esperado"></a>

## 📡 Contrato HTTP esperado

O contrato exposto pelo API Gateway é:

```http
POST /auth/documento
```

### Request

```json
{
  "documento": "529.982.247-25"
}
```

### Response 200

Exemplo com `Jwt__ExpirationMinutes=30`:

```json
{
  "accessToken": "...",
  "tokenType": "Bearer",
  "expiresIn": 1800
}
```

Com a configuração default atual de 60 minutos, `expiresIn` será `3600`.

### Response 400

```json
{
  "mensagem": "CPF/CNPJ inválido.",
  "tipo": "Validacao"
}
```

### Response 401

```json
{
  "mensagem": "Documento não autorizado.",
  "tipo": "NaoAutorizado"
}
```

### Response 503

```json
{
  "mensagem": "Serviço temporariamente indisponível.",
  "tipo": "DependenciaIndisponivel"
}
```

---

<a id="variaveis-de-ambiente"></a>

## ⚙️ Variáveis de ambiente

| Variável | Obrigatória | Uso |
| --- | --- | --- |
| `ConnectionStrings__SqlServer` | Sim | Connection string do SQL Server usado para consulta de cliente. |
| `Jwt__Issuer` | Sim | Issuer do JWT. Valor esperado: `oficina-mecanica-auth`. |
| `Jwt__Audience` | Sim | Audience do JWT. Valor esperado: `oficina-mecanica-api`. |
| `Jwt__Secret` | Sim | Chave HMAC-SHA256 usada para assinar o token. |
| `Jwt__ExpirationMinutes` | Não | Expiração em minutos. Usa `60` como default se ausente ou inválido. |
| `AWS_LAMBDA_EXEC_WRAPPER` | Sim no deploy AWS | Ativa o wrapper da layer Datadog. |
| `DD_API_KEY_SECRET_ARN` | Sim no deploy AWS | ARN do secret criado para a API key Datadog. |
| `DD_SITE` | Sim no deploy AWS | Site Datadog; a configuração atual usa `datadoghq.com`. |
| `DD_SERVICE`, `DD_ENV`, `DD_VERSION` | Sim no deploy AWS | Identificação e correlação da telemetria. |

Observações:

- `Jwt__Secret` deve ter pelo menos 32 bytes.
- Secrets reais não devem ser versionados.
- Credenciais de banco, tokens e chaves devem ser configurados em ambiente seguro na etapa de deploy.

### GitHub Environment `development`

Crie os itens abaixo em **Settings > Environments > development**:

| Nome | Tipo | Valor esperado em termos conceituais |
| --- | --- | --- |
| `AWS_ACCESS_KEY_ID` | Environment Secret | Access key temporária do AWS Academy. |
| `AWS_SECRET_ACCESS_KEY` | Environment Secret | Secret key temporária do AWS Academy. |
| `AWS_SESSION_TOKEN` | Environment Secret | Token temporário da sessão AWS Academy. |
| `AUTH_LAMBDA_JWT_SECRET` | Environment Secret | Chave HMAC forte, compatível com a validação da API. |
| `DD_API_KEY` | Environment Secret | API key do Datadog; o workflow grava o valor no AWS Secrets Manager. |
| `AWS_REGION` | Environment Variable | Região AWS, com fallback `us-east-1`. |
| `AUTH_LAMBDA_EXECUTION_ROLE_NAME` | Environment Variable | Nome da role externa do laboratório, normalmente `LabRole`. |
| `DD_SITE` | Environment Variable | Site Datadog aplicável; a configuração atual efetiva usa `datadoghq.com`. |
| `AUTO_PR_ENABLED` | Repository Variable | `true` somente quando as promoções automáticas estiverem habilitadas. |
| `RELEASE_BRANCH` | Repository Variable | Branch de promoção, com fallback `release`. |

O ambiente VocLabs fornece a `LabRole`; este repositório apenas consulta e associa essa role à função. Ele não cria nem gerencia policies IAM da role externa.

### Deploy, destroy e validação

O merge em `develop` chama o workflow AWS quando há mudança deployável. `infra/terraform/environments/dev/terraform-action.env` controla `apply` ou `destroy`; destroy deve ocorrer em PR dedicado. Após `apply`, a esteira valida configuração, VPC, layers, secret Datadog e contratos SSM. Após `destroy`, confirma a remoção da função e dos contratos próprios.

A Lambda consome os parâmetros da VPC e do RDS e publica `/oficina-mecanica/development/auth-lambda/function_arn`, `function_name` e `/oficina-mecanica/development/status/auth-lambda`.

---

<a id="seguranca"></a>

## 🔒 Segurança

Decisões já implementadas:

- Query SQL fixa e parametrizada.
- Sem uso de `AddWithValue`.
- JWT não contém CPF, CNPJ ou documento do cliente.
- Cliente inexistente e cliente inativo retornam resposta indistinguível.
- Erros inesperados são sanitizados e não retornam `exception.Message`.
- Validação de CPF/CNPJ fica no domínio.
- Secret JWT inválido falha em modo fail-fast.
- Checagem local de pacotes vulneráveis faz parte da validação do repositório.

O deploy usa as layers versionadas `dd-trace-dotnet` e `Datadog-Extension`. A instrumentação está configurada, mas a ingestão de logs e traces da Lambda no Datadog não foi evidenciada de ponta a ponta no ambiente acadêmico e permanece como evolução pós-entrega.

---

<a id="testes-e-qualidade"></a>

## 🧪 Testes e qualidade

O repositório possui testes unitários por camada, seguindo o padrão adotado na solução:

- nomes BDD no formato `Dado_Quando_Entao`;
- padrão AAA explícito com `Arrange`, `Act` e `Assert`;
- FluentAssertions para asserções;
- Moq quando há dependência a simular;
- factories de teste para dados comuns;
- cobertura relevante em Application, Domain e Function.

### Comandos principais

```powershell
dotnet restore
dotnet build -c Release --no-restore
dotnet test -c Release --collect:"XPlat Code Coverage" --no-restore
dotnet list package --vulnerable --include-transitive
```

### Validação antes de PR

```powershell
dotnet build -c Release --no-restore
dotnet test -c Release --collect:"XPlat Code Coverage" --no-restore
dotnet list package --vulnerable --include-transitive
git diff --check
```

---

<a id="git-flow-e-governanca"></a>

## 🌿 Git Flow e governança

O fluxo esperado segue os demais repositórios da Fase 3:

```text
branch de trabalho -> PR develop -> PR release -> PR main
```

Branches protegidas:

- `develop`
- `release`
- `release/*`
- `main`

Rulesets ativos:

- **Aprovação de PR:** exige revisão humana, descarta aprovações antigas e exige aprovação do último push.
- **Proteção Git Flow:** exige PR, resolução de conversas e bloqueia push direto, force push e deleção.

Os workflows de CI/CD validam código, testes, Terraform e o fluxo de branches antes das promoções.

---

<a id="status-atual"></a>

## 📍 Status atual

| Item | Status |
| --- | --- |
| Código da Auth Lambda | ✅ Implementado |
| Testes unitários | ✅ Implementados |
| Rulesets Git Flow | ✅ Configurados |
| CI/CD | ✅ Implementado |
| Infraestrutura Lambda | ✅ Implementada |
| API Gateway | ✅ Integrado por contrato |
| Deploy AWS | ✅ Pipeline implementado |
| Integração com RDS/API Gateway | ✅ Implementada |
| Telemetria Datadog da Lambda | ⚠️ Instrumentada; ingestão não evidenciada no ambiente acadêmico |

---

<a id="proximos-passos"></a>

## 🗺️ Evoluções pós-entrega

- evidenciar a ingestão completa de logs e traces da Lambda no Datadog;
- evoluir controles de observabilidade e segurança compatíveis com um ambiente de produção.

---

<a id="observacoes"></a>

## 📝 Observações

- Os ambientes `release` e `main` representam promoções lógicas; o ambiente físico AWS da entrega é `development`.
- Não versionar secrets, connection strings reais, tokens, senhas ou credenciais AWS.
- A API principal continua dona do schema, das migrations, dos seeds e das regras gerais de domínio.
- A Auth Lambda não referencia o projeto da API principal.
- Documentação central e arquitetura completa: [README da Oficina Mecânica API](https://github.com/geoscabio/oficina-mecanica-api).
