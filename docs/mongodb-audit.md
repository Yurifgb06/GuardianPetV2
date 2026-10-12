# Auditoria MongoDB local

Oracle continua sendo o banco principal. Os decoradores AuditedPetRepository,
AuditedUserRepository, AuditedVeterinarianRepository e AuditedConsultationRepository
delegam o CRUD ao repositório Oracle e somente depois registram Created, Updated ou
Deleted no MongoDB. Leituras não geram eventos. Não há transação distribuída, outbox,
retry durável nem recuperação automática de eventos perdidos: a auditoria é best-effort.
Falha ou timeout da auditoria não desfazem o Oracle e não mudam o retorno do CRUD.
Um warning seguro é registrado, sem exceção, credenciais, payload ou connection string.

O documento contém somente id, entityType, entityId, action, occurredAtUtc e
correlationId. Não contém snapshots, usuários, senhas, tokens nem dados médicos.
O correlationId é o mesmo gerado/validado pelo middleware HTTP existente.

## Iniciar localmente

~~~powershell
docker compose --profile local-audit up -d --no-deps mongodb
~~~

O serviço local usa mongo:8.0, publica apenas 127.0.0.1:27017, não exige autenticação
e utiliza volume próprio. O comando não inicia nem recria Oracle ou a API.
Esse serviço é exclusivo de desenvolvimento/testes e não define implantação de produção.

Para executar a API no host, as opções padrão apontam para localhost:27017,
database guardianpet_audit e collection audit_events. Também podem ser configuradas
pelas variáveis MongoDB__ConnectionString, MongoDB__DatabaseName,
MongoDB__CollectionName e MongoDB__TimeoutMilliseconds (100 a 10000, padrão 1000).
Não salvar credenciais em arquivos versionados. O cliente é singleton; repositório,
serviço de auditoria e decoradores são scoped.

## Consulta administrativa

GET /api/audit-events exige Bearer JWT e a política ManageUsers (users.manage).
Filtros combináveis: entityType (Pet/User/Veterinarian/Consultation), entityId,
action (Created/Updated/Deleted), correlationId, fromUtc e toUtc (ISO 8601 UTC).
page começa em 1; pageSize é 1..100; padrões 1/20. Ordenação fixa por
occurredAtUtc e id descendentes. A paginação ocorre no MongoDB.

Sem includeLinks retorna um array; includeLinks=true retorna items/links e consulta
COUNT no MongoDB para navegação. Não são anunciadas rotas individuais de eventos.
Parâmetros inválidos retornam 400; falta de autenticação 401; falta de permissão 403;
MongoDB indisponível retorna 503 com mensagem genérica.

## Testes

As fábricas HTTP usam databases GPTEST_<guid>, nunca a base padrão da aplicação.
GUARDIANPET_TEST_MONGO_CONNECTION pode apontar para outro MongoDB de testes local.
O schema Oracle continua isolado. A limpeza remove somente a base GPTEST criada
pela fixture, com validação do nome antes de qualquer remoção.
