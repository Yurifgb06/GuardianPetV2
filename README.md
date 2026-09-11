# GuardianPet

API REST desenvolvida em **ASP.NET Core 8** para gerenciamento de uma clínica veterinária.

O projeto permite o gerenciamento de usuários, pets, veterinários e consultas, utilizando **Oracle Database**, **Entity Framework Core**, autenticação **JWT**, testes automatizados e recursos de monitoramento e observabilidade.

## Integrantes

| Nome | RM | Turma |
|---|---|---|
| Yuri Fuzinatto Garzoli Barreto | 561450 | 2TDSPX |
| Nicolas de Oliveira Jacob | 564205 | 2TDSPX |
| Gabriel Ambrósio Saraiva | 566552 | 2TDSPV |
| Vinicius Romaguera Cardozo | 562308 | 2TDSPX |
| João Victor Vendrameto | 563665 | 2TDSPV |

## Repositório

GitHub: https://github.com/Yurifgb06/GuardianPetV2

---

## Sobre o projeto

O **GuardianPet** foi desenvolvido com o objetivo de organizar informações relacionadas ao atendimento veterinário.

A API permite realizar operações de cadastro, consulta, atualização e exclusão das principais entidades da aplicação:

- Usuários
- Pets
- Veterinários
- Consultas

Também foram implementados recursos de segurança, documentação, monitoramento e testes automatizados.

Entre as principais funcionalidades estão:

- CRUD completo das entidades;
- relacionamento entre usuários, pets, veterinários e consultas;
- busca de usuários por nome e e-mail;
- busca de pets por espécie e porte;
- busca de veterinários por nome e especialidade;
- autenticação utilizando JWT;
- armazenamento seguro das senhas;
- documentação interativa com Swagger;
- Health Checks;
- logs estruturados;
- Correlation ID;
- tracing com OpenTelemetry;
- métricas de latência e erros;
- testes unitários e de integração.

---

## Tecnologias utilizadas

- .NET 8
- C#
- ASP.NET Core Web API
- Entity Framework Core 8
- Oracle.EntityFrameworkCore
- Oracle Database Free
- Swagger / OpenAPI
- Serilog
- OpenTelemetry
- JWT Bearer
- Docker
- Docker Compose
- xUnit
- Moq
- Microsoft.AspNetCore.Mvc.Testing

---

## Arquitetura

O projeto utiliza separação em camadas:

```text
HTTP Request
     |
     v
Controller
     |
     v
Service
     |
     v
Repository
     |
     v
Entity Framework Core
     |
     v
Oracle Database
```

### Responsabilidade das camadas

**Controllers**

Recebem as requisições HTTP, trabalham com os DTOs e retornam as respostas da API.

**Services**

Concentram as regras de negócio e validações da aplicação.

**Repositories**

Realizam o acesso aos dados através do `AppDbContext`.

**DTOs**

Separam os dados recebidos e retornados pela API das entidades persistidas no banco.

**Middlewares**

São utilizados para tratamento global de erros, Correlation ID e coleta de métricas.

**Observability**

Centraliza os componentes utilizados pelo OpenTelemetry para tracing e métricas.

---

## Estrutura da solução

A solução principal é:

```text
GuardianPet.sln
```

Ela contém três projetos:

```text
GuardianPet.sln
│
├── GuardianPet
│   ├── Controllers
│   ├── Data
│   ├── Documentation
│   ├── DTOs
│   ├── Enums
│   ├── Exceptions
│   ├── HealthChecks
│   ├── Middlewares
│   ├── Migrations
│   ├── Models
│   ├── Observability
│   ├── Repositories
│   ├── Security
│   └── Services
│
├── GuardianPet.Tests.Unit
│
└── GuardianPet.Tests.Integration
```

---

# Executando o projeto

## Pré-requisitos

Para executar o projeto utilizando Docker:

- Docker Desktop
- Docker Compose

Para executar comandos .NET diretamente na máquina:

- SDK .NET 8 ou compatível

---

## Executando com Docker

Na pasta onde está o arquivo `docker-compose.yml`, execute:

```bash
docker compose up -d --build
```

Para verificar os containers:

```bash
docker compose ps
```

São iniciados dois containers:

```text
guardianpet-api
oracle-db
```

A API fica disponível na porta:

```text
8080
```

O Oracle utiliza:

```text
Porta: 1521
PDB: FREEPDB1
```

Os dados do Oracle são mantidos através do volume:

```text
oracle-data
```

### Acessos

Swagger:

```text
http://localhost:8080/swagger
```

Health Check:

```text
http://localhost:8080/health
```

Readiness:

```text
http://localhost:8080/health/ready
```

Para visualizar os logs:

```bash
docker compose logs --tail=100 guardianpet-api
```

Para parar os containers mantendo os dados:

```bash
docker compose down
```

> A remoção do volume do Oracle apaga os dados armazenados e não faz parte da execução normal do projeto.

---

## Executando a API pelo .NET

Também é possível executar somente o Oracle no Docker:

```bash
docker compose up -d oracle-db
```

Depois:

```bash
dotnet restore GuardianPet.sln
dotnet build GuardianPet.sln
dotnet run --project GuardianPet.csproj --launch-profile http
```

Nesse perfil, a aplicação utiliza o ambiente `Development`.

O Swagger local fica disponível em:

```text
http://localhost:5202/swagger
```

---

# Banco de dados

O projeto utiliza **Oracle Database** com **Entity Framework Core**.

As migrations ficam na pasta:

```text
Migrations/
```

Na inicialização, a aplicação tenta aplicar as migrations automaticamente antes de começar a atender as requisições.

Como o Oracle pode levar alguns segundos para inicializar, a API realiza novas tentativas de conexão antes de desistir.

O banco possui as principais entidades da aplicação:

- Users
- Pets
- Veterinarians
- Consultations

---

# Autenticação e autorização

A aplicação utiliza autenticação através de **JWT Bearer**.

## Cadastro

```http
POST /api/users
```

O cadastro inicial é público.

As senhas não são armazenadas em texto puro. O projeto utiliza:

```text
PasswordHasher<User>
```

para gerar o hash seguro da senha.

## Login

```http
POST /api/auth/login
```

O login recebe e-mail e senha e, quando as credenciais são válidas, retorna:

- token JWT;
- data de expiração;
- ID do usuário.

Nos endpoints protegidos, envie:

```http
Authorization: Bearer <token>
```

### Respostas de segurança

| Status | Significado |
|---|---|
| 401 | Usuário não autenticado, token inválido ou credenciais incorretas |
| 403 | Usuário autenticado, mas sem a permissão necessária |

A policy:

```text
ManageUsers
```

exige a claim:

```text
permission=users.manage
```

para operações administrativas de usuários.

Os administradores são configurados através de:

```text
Jwt:AdminUserIds
```

Pets, veterinários e consultas exigem autenticação.

Mais informações estão disponíveis em:

```text
AUTHENTICATION.md
```

---

# Swagger

A documentação da API utiliza **Swagger / OpenAPI**.

Com Docker:

```text
http://localhost:8080/swagger
```

Os endpoints possuem documentação XML com:

- descrição da operação;
- parâmetros;
- corpo da requisição;
- retornos;
- códigos HTTP;
- requisitos de autenticação.

Para testar endpoints protegidos:

1. realize o login;
2. copie o token retornado;
3. clique em **Authorize** no Swagger;
4. informe o token no esquema Bearer;
5. execute o endpoint desejado.

Login, cadastro e Health Checks permanecem públicos.

---

# Endpoints principais

| Recurso | Método | Endpoint |
|---|---|---|
| Autenticação | POST | `/api/auth/login` |
| Usuários | POST / GET | `/api/users` |
| Usuários | GET / PUT / DELETE | `/api/users/{id}` |
| Usuários | GET | `/api/users/search?name=...` |
| Usuários | GET | `/api/users/email?email=...` |
| Pets | POST / GET | `/api/pets` |
| Pets | GET / PUT / DELETE | `/api/pets/{id}` |
| Pets | GET | `/api/pets/search?species=...&petSize=...` |
| Veterinários | POST / GET | `/api/veterinarians` |
| Veterinários | GET / PUT / DELETE | `/api/veterinarians/{id}` |
| Veterinários | GET | `/api/veterinarians/search/name?name=...` |
| Veterinários | GET | `/api/veterinarians/search/specialty?specialty=...` |
| Consultas | POST / GET | `/api/consultations` |
| Consultas | GET / PUT / DELETE | `/api/consultations/{id}` |
| Health | GET | `/health` |
| Readiness | GET | `/health/ready` |

Os schemas, campos obrigatórios e valores possíveis dos enums podem ser consultados diretamente no Swagger.

---

# Monitoramento e observabilidade

A Sprint 3 adiciona recursos de monitoramento para facilitar a identificação de falhas e acompanhar o comportamento da API.

## Health Checks

### Liveness

```http
GET /health
```

Verifica se a aplicação está funcionando.

O liveness não depende da conexão com o Oracle.

Resposta esperada:

```text
200 Healthy
```

### Readiness

```http
GET /health/ready
```

Verifica se a aplicação está pronta para receber requisições e se consegue acessar o Oracle através do `AppDbContext`.

Possíveis respostas:

```text
200 Healthy
503 Unhealthy
```

---

## Logs

O projeto utiliza **Serilog** para geração de logs estruturados.

Os logs são enviados para:

- Console;
- arquivo diário.

Arquivos:

```text
logs/guardianpet-.log
```

A retenção configurada é de 14 arquivos.

São utilizados os níveis:

- Information
- Warning
- Error

Informações sensíveis como senhas, tokens JWT e Authorization Header não são registradas.

---

## Correlation ID

Cada requisição pode possuir o header:

```http
X-Correlation-ID
```

Esse identificador é incluído nos logs e também devolvido na resposta.

Quando nenhum identificador válido é enviado, a aplicação gera automaticamente um GUID.

Isso facilita acompanhar todos os registros relacionados à mesma requisição.

---

## OpenTelemetry e tracing

O projeto utiliza **OpenTelemetry** para tracing.

Existe instrumentação automática para:

- ASP.NET Core;
- HttpClient.

O fluxo utilizado como demonstração de tracing é:

```text
HTTP Request
     |
     v
Controller
     |
     v
PetService
     |
     v
PetRepository
     |
     v
Oracle
```

Os spans compartilham o mesmo `TraceId`, permitindo acompanhar a requisição entre as camadas.

Também são utilizados:

```text
TraceId
SpanId
CorrelationId
```

O acesso EF Core/Oracle fica dentro da duração do span do Repository.

O Console Exporter está habilitado em `Development`.

---

## Métricas

Foram criadas duas métricas principais.

### Latência

```text
guardianpet.http.request.duration
```

Histograma que registra a duração das requisições HTTP em milissegundos.

### Erros

```text
guardianpet.http.errors
```

Contador das respostas HTTP entre `400` e `599`.

As métricas utilizam tags como:

- método HTTP;
- status HTTP;
- padrão da rota.

As rotas são normalizadas para evitar que IDs diferentes criem séries diferentes.

Exemplo:

```text
/api/pets/{id}
```

em vez de:

```text
/api/pets/1
/api/pets/2
```

Em `Development`, as métricas são exportadas para o console periodicamente.

---

# Testes automatizados

A solução possui dois projetos separados de testes:

```text
GuardianPet.Tests.Unit
GuardianPet.Tests.Integration
```

## Testes unitários

Utilizam:

- xUnit;
- Moq;
- padrão AAA.

São testados os cinco Services principais:

- UserService;
- PetService;
- VeterinarianService;
- ConsultationService;
- AuthService.

Entre os cenários testados estão:

- operações válidas;
- recursos inexistentes;
- duplicidade de e-mail;
- duplicidade de CPF;
- duplicidade de CRMV;
- relacionamentos inválidos;
- hash de senha;
- autenticação;
- geração de JWT.

Os testes seguem a nomenclatura:

```text
MetodoTestado_Cenario_ResultadoEsperado
```

---

## Testes de integração

Os testes de integração utilizam:

- xUnit;
- `WebApplicationFactory`;
- `CustomWebApplicationFactory`;
- Fixtures;
- Collection Fixture;
- Oracle.

A integração cobre respostas:

```text
200
201
204
400
401
403
404
500
```

Também são testados:

- login válido;
- login inválido;
- autenticação;
- autorização;
- CRUD de Pets;
- Correlation ID;
- Health Checks;
- Swagger.

O cenário `500 Internal Server Error` é provocado somente no ambiente de teste através da substituição do `IPetRepository`, sem criar endpoint artificial na aplicação.

### Isolamento do banco

Cada execução dos testes de integração cria um schema Oracle temporário e isolado:

```text
GPTEST_*
```

As migrations são aplicadas nesse schema e ele é removido ao término dos testes.

Dessa forma, os testes não dependem dos dados utilizados manualmente na aplicação.

---

## Executando os testes

Os testes de integração precisam que o **Docker esteja disponível e o Oracle esteja ativo**.

Primeiro:

```bash
docker compose up -d oracle-db
```

Depois:

```bash
dotnet restore GuardianPet.sln
dotnet build GuardianPet.sln
dotnet test GuardianPet.sln
```

### Resultado da validação

| Suíte | Total | Aprovados | Falhas | Ignorados |
|---|---:|---:|---:|---:|
| Unitários | 48 | 48 | 0 | 0 |
| Integração | 32 | 32 | 0 | 0 |
| **Total** | **80** | **80** | **0** | **0** |

Build final:

```text
0 erros
0 warnings
```

---

# Configuração por ambiente

As configurações da aplicação podem ser sobrescritas através de variáveis de ambiente.

O arquivo:

```text
.env.example
```

contém exemplos das variáveis necessárias.

Para configuração local personalizada, ele pode ser copiado para:

```text
.env
```

O `.env` não deve ser enviado para o repositório.

Principais configurações:

| Variável | Finalidade |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | Ambiente da aplicação |
| `ORACLE_PASSWORD` | Senha administrativa utilizada na inicialização do Oracle |
| `APP_USER` | Usuário Oracle da aplicação |
| `APP_USER_PASSWORD` | Senha do usuário Oracle |
| `ConnectionStrings__OracleConnection` | Connection String utilizada pela API |
| `Jwt__Key` | Chave privada do JWT |
| `Jwt__Issuer` | Emissor do token |
| `Jwt__Audience` | Destinatário do token |
| `Jwt__ExpirationMinutes` | Tempo de validade |
| `Jwt__AdminUserIds__0` | Usuário com permissão administrativa |

Fora do ambiente de desenvolvimento, a chave JWT de exemplo é recusada pela aplicação.

Senhas, tokens e credenciais privadas não devem ser adicionados ao repositório.

---

# Docker e integração com DevOps

O projeto já está preparado para ser utilizado na etapa de DevOps.

A imagem da API:

- utiliza HTTP;
- escuta na porta `8080`;
- recebe configurações por variáveis de ambiente;
- utiliza o Oracle através da connection string;
- possui Health Checks para liveness e readiness.

Endpoints para probes:

```text
GET /health
GET /health/ready
```

No Docker Compose, a API acessa o Oracle utilizando o hostname:

```text
oracle-db
```

e não `localhost`.

A configuração de infraestrutura em nuvem, Azure, ACR e ACI fica separada da implementação da aplicação .NET.

---

# Códigos HTTP

Os principais códigos utilizados pela API são:

| Código | Situação |
|---:|---|
| 200 | Consulta ou atualização realizada |
| 201 | Recurso criado |
| 204 | Recurso excluído |
| 400 | Dados inválidos |
| 401 | Não autenticado |
| 403 | Sem permissão |
| 404 | Recurso não encontrado |
| 500 | Erro interno inesperado |
| 503 | Banco indisponível no readiness |

Em erros inesperados, a API retorna:

```text
Erro interno do servidor.
```

Detalhes técnicos permanecem somente nos logs.

---

# Observações

- O primeiro início do Oracle pode levar alguns segundos.
- A API aplica as migrations automaticamente na inicialização.
- Os testes de integração precisam do Oracle disponível.
- Usuários antigos que possuam senha armazenada em texto puro precisam ter a senha redefinida.
- Não há refresh token ou revogação de JWT nesta versão.
- Arquivos de build, logs, resultados de testes e configurações locais estão ignorados pelo Git.

---

## Projeto acadêmico

Projeto desenvolvido para fins educacionais como parte das atividades do curso de **Análise e Desenvolvimento de Sistemas FIAP**.