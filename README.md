# 🔐 Oficina Mecânica Auth Lambda

Serviço serverless responsável por autenticar clientes por documento e emitir JWT para integração com a API da Oficina Mecânica.

Este repositório faz parte da **Fase 3 do Tech Challenge FIAP** e evolui a arquitetura da solução separando a autenticação de cliente em uma Lambda própria, sem transferir a posse do schema, das migrations ou das regras gerais do domínio da API principal.

---

## 📌 Índice

- [✨ Visão geral](#visao-geral)
- [🎯 Objetivo da Lambda](#objetivo-da-lambda)
- [🏗️ Arquitetura](#arquitetura)
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
| 🧱 Integração | Prepara o fluxo para exposição futura via API Gateway. |

---

<a id="objetivo-da-lambda"></a>

## 🎯 Objetivo da Lambda

O objetivo deste serviço é autenticar clientes por documento, sem duplicar regras ou ownership da API principal.

Este repo cuida de:

- receber uma requisição HTTP futuramente roteada pelo API Gateway;
- autenticar o cliente por CPF/CNPJ;
- consultar o cliente na base de Atendimento;
- validar se o cliente está ativo;
- emitir JWT HMAC-SHA256 para consumo pela `oficina-mecanica-api`;
- retornar respostas HTTP padronizadas.

Infraestrutura, deploy, API Gateway, VPC, secrets de ambiente e automações de CI/CD serão tratados em etapas posteriores.

---

<a id="arquitetura"></a>

## 🏗️ Arquitetura

O projeto segue **Clean Architecture** e preserva os bounded contexts usados no repositório principal, especialmente **Identidade** e **Atendimento**.

| Camada | Projeto | Responsabilidade |
| --- | --- | --- |
| ⚡ **Function** | `OficinaMecanica.AuthLambda.Function` | Adaptador Lambda, serialização JSON, mapeamento HTTP, bootstrap, DI e tratamento sanitizado de erros. |
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

O endpoint definitivo depende da etapa futura de API Gateway/infraestrutura. O contrato planejado para exposição é:

```http
POST /auth/cliente
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

### Response 500

```json
{
  "mensagem": "Erro interno inesperado.",
  "tipo": "ErroInterno"
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

Observações:

- `Jwt__Secret` deve ter pelo menos 32 bytes.
- Secrets reais não devem ser versionados.
- Credenciais de banco, tokens e chaves devem ser configurados em ambiente seguro na etapa de deploy.

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

Itens como WAF, rate limit, Secrets Manager, API Gateway authorizer e configuração de rede serão avaliados nas etapas futuras de infraestrutura e deploy.

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

Neste momento, os rulesets ainda não exigem status checks porque o repositório não possui CI/CD configurado. Essa exigência será adicionada depois que os workflows existirem e os nomes reais dos checks forem confirmados.

---

<a id="status-atual"></a>

## 📍 Status atual

| Item | Status |
| --- | --- |
| Código da Auth Lambda | ✅ Implementado |
| Testes unitários | ✅ Implementados |
| Rulesets Git Flow | ✅ Configurados |
| CI/CD | ⏳ Pendente |
| Infraestrutura Lambda | ⏳ Pendente |
| API Gateway | ⏳ Pendente |
| Deploy AWS | ⏳ Pendente |
| Integração final com RDS/API Gateway | ⏳ Pendente |

---

<a id="proximos-passos"></a>

## 🗺️ Próximos passos

- Criar CI/CD seguindo o padrão real dos repositórios maduros.
- Atualizar o ruleset para exigir os checks corretos.
- Criar infraestrutura da Lambda.
- Configurar API Gateway.
- Configurar variáveis e secrets em ambiente seguro.
- Validar integração com banco/RDS.
- Integrar o endpoint final ao fluxo da API Gateway.

---

<a id="observacoes"></a>

## 📝 Observações

- Este repositório ainda não possui deploy em produção.
- Este repositório ainda não possui workflows de CI/CD.
- Não versionar secrets, connection strings reais, tokens, senhas ou credenciais AWS.
- A API principal continua dona do schema, das migrations, dos seeds e das regras gerais de domínio.
- A Auth Lambda não referencia o projeto da API principal.
