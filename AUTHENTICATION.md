# Autenticação e testes — Sprint 3

`GuardianPet.sln` contém a API na raiz e os projetos `GuardianPet.Tests.Unit`
e `GuardianPet.Tests.Integration`, todos net8.0. Os projetos de testes estão
implementados: 48 testes unitários e 32 de integração, todos aprovados.
`Program` é público e parcial para WebApplicationFactory. A integração usa
ApiFixture (IAsyncLifetime), CustomWebApplicationFactory e Collection Fixture
"Oracle integration", com schema Oracle isolado e descartado após a execução.
A inicialização exige Oracle e aplica as migrations existentes.
Os diretórios de testes são excluídos dos itens da API e do contexto Docker.

## Login e cadastro

- `POST /api/users` é público para cadastro inicial.
- `POST /api/auth/login` aceita email e password; sucesso retorna token,
  expiration (UTC) e userId. Credenciais inválidas retornam 401 sem distinguir
  usuário inexistente, senha errada ou formato legado.
- O Swagger é público. Em Authorize, selecionar Bearer e colar apenas o token.
- `/health` e `/health/ready` continuam públicos.
- Pets, consultas e veterinários exigem JWT válido.
- Todas as operações de usuários, exceto cadastro, exigem a policy ManageUsers.
  Isso impede que um usuário comum altere a senha de outra conta.

## Hash e usuários antigos

Usamos apenas PasswordHasher<User> (PBKDF2 com salt aleatório e formato
versionado), disponível no framework compartilhado; não há configuração do
ASP.NET Identity nem tabelas adicionais. Criação/atualização sempre geram hash.
Login verifica hash e atualiza hashes válidos que precisem de rehash.

Senhas antigas em texto puro não são aceitas, mesmo quando correspondem à senha
enviada. Nenhuma conversão automática, exclusão ou migration foi executada.
Em ambiente acadêmico, crie uma nova conta com email/CPF diferentes, ou peça a
um administrador para redefinir a senha usando o PUT existente (DTO completo).

## JWT e administração

HS256; issuer/audience GuardianPet; validade padrão 60 minutos; validação de
assinatura, algoritmo, issuer, audience e expiração, com tolerância de 30 segundos.
O token contém sub (ID), jti e, somente para administradores configurados,
permission=users.manage. Não contém senha, CPF ou email.

`Jwt:AdminUserIds` inicia vazio. Para promover uma conta criada pelo operador,
configure seu ID por `Jwt__AdminUserIds__0` (e índices seguintes), reinicie a API
e faça login novamente. O cliente não pode escolher essa permissão no cadastro.
O ID deve ser conferido pelo operador, nunca concedido automaticamente ao primeiro
cadastro. A alteração da lista não revoga tokens já emitidos: expiram normalmente.

Sem token válido: 401. Com token válido de usuário comum em `/api/users`: 403.
É uma restrição funcional de administração de contas, sem endpoint artificial.
Não há autorização por proprietário nos outros CRUDs nesta etapa acadêmica.

A chave incluída em appsettings.Development.json e no Compose é pública e
EXCLUSIVA de desenvolvimento. appsettings.json mantém a chave vazia.
Fora de Development a aplicação recusa essa chave. Substitua `Jwt__Key` por um
segredo aleatório forte (pelo menos 32 bytes), via secret/environment, antes de
produção. Também são configuráveis `Jwt__Issuer`, `Jwt__Audience` e
`Jwt__ExpirationMinutes`. O Compose permite substituir esses valores pelo ambiente
ou por .env (ignorado), conforme .env.example. Para IDs administrativos, adicione
Jwt__AdminUserIds__0 ao environment da API em um override local do Compose.
Não grave chaves
reais, senhas ou tokens em arquivos versionados. Não há refresh token/revogação.

## Validação

```powershell
docker compose up -d oracle-db
dotnet restore GuardianPet.sln
dotnet build GuardianPet.sln
dotnet test GuardianPet.sln
docker compose up -d --build
docker compose ps
```

Os projetos usam xunit 2.9.3, runner 3.1.5 e Test SDK 17.14.1.
Unit usa Moq 4.20.72; Integration usa Mvc.Testing 8.0.31.
A API usa JwtBearer 8.0.31 e referência explícita a EF Core Relational 8.0.11
para manter a resolução de dependências igual na API e nos testes.

A integração exige Docker acessível e Oracle ativo com PDB FREEPDB1. A fixture
gera credenciais e JWT de teste, cria seu próprio administrador e não depende de
usuário ID 1 inserido manualmente. Os testes cobrem login válido/inválido,
401/403, CRUD, 200/201/204/400/404/500, Correlation ID, Health Checks e Swagger.
O cenário 500 substitui IPetRepository somente no host de testes e valida a
mensagem pública genérica, sem exposição de detalhes internos.

AAA explícito e nomenclatura MetodoTestado_Cenario_ResultadoEsperado.
Resultado: 80 passed, 0 failed, 0 skipped; build com 0 erros e 0 warnings.
