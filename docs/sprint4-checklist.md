# Sprint 4 — checklist acadêmico

Projeto: GuardianPet. Branch auditada: sprint-4. Data: 12/10/2026.

Esta revisão usa os critérios de Advanced Business Development with .NET informados
nos blocos da Sprint 4. O enunciado e a rubrica oficial não estão no repositório;
a matriz não substitui sua conferência nem presume nota. Atendido significa
evidência implementada e verificável; parcial significa alcance limitado;
pendente significa ausência de evidência ou validação necessária.

## Aplicação .NET

| Requisito | Evidência no projeto | Situação | Observações |
|---|---|---|---|
| Arquitetura em camadas | [Controllers](../Controllers), [Services](../Services), [Repositories](../Repositories), [AppDbContext](../Data/AppDbContext.cs) | Atendido | Controller → IService → Service → IRepository → Repository → EF Core → Oracle. Auditoria administrativa possui fluxo específico. |
| Inversão de dependência e interfaces específicas | [IAuthService](../Services/IAuthService.cs), demais IService/IRepository, [Program](../Program.cs) | Atendido | Cinco Services com interfaces e DI Scoped. Evidencia DIP/ISP e separação de responsabilidades; não constitui prova formal de todos os princípios SOLID. |
| Extensão do CRUD sem refatoração geral | [decoradores de auditoria](../Audit), [AuditRegistration](../Audit/AuditRegistration.cs) | Atendido | Decoradores implementam os contratos existentes e delegam ao Oracle; sem repositório genérico. |
| CRUD e recursos REST | [UsersController](../Controllers/UsersController.cs), [PetsController](../Controllers/PetsController.cs), [VeterinariansController](../Controllers/VeterinariansController.cs), [ConsultationsController](../Controllers/ConsultationsController.cs) | Atendido | POST/GET/PUT/DELETE; 201 com Location, 200 e 204. Validação e erros preservados. O relacionamento User/Pet não tem endpoint próprio. |
| Swagger/OpenAPI e XML | [ApiDocumentationFilter](../Documentation/ApiDocumentationFilter.cs), Controllers e [Program](../Program.cs) | Atendido | Operações, parâmetros, autorização e respostas documentados; schemas alternativos de HATEOAS. Exemplos estruturais não substituem todos os exemplos de DTO. |
| Paginação no banco | [CollectionQuery](../Repositories/CollectionQuery.cs), QueryAsync dos repositórios | Atendido | Skip/Take antes de ToListAsync; page >= 1, pageSize 1..100. Sem paginação solicitada, mantém a listagem anterior. |
| Ordenação segura e estável | [DTOs de consulta](../DTOs/Query), switches dos Repositories | Atendido | Campos permitidos explicitamente, sem SQL arbitrário. ID ascendente como desempate e ordem padrão da paginação. |
| Filtros combináveis | [DTOs de consulta](../DTOs/Query), [Repositories](../Repositories) | Atendido | Nome/email, espécie/porte, nome/especialidade; consultas por status, IDs e datas. GET por email continua individual. |
| HATEOAS | [Hateoas](../Presentation/Hateoas.cs), [representações](../DTOs/Response/HateoasResponses.cs), [testes](../GuardianPet.Tests.Integration/Endpoints/HateoasEndpointsTests.cs) | Atendido | **Opcional por includeLinks=true**, preservando contratos antigos. Recursos: data/links; coleções: items/links. self/first/prev/next/last quando aplicáveis; URLs respeitam esquema, host e PathBase. |
| Compatibilidade HTTP | [testes de coleções](../GuardianPet.Tests.Integration/Endpoints/CollectionQueryEndpointsTests.cs), [testes HATEOAS](../GuardianPet.Tests.Integration/Endpoints/HateoasEndpointsTests.cs) | Atendido | Sem includeLinks, arrays continuam arrays e objetos mantêm DTOs/enums. Autorização e erros preservados. Paginação não adiciona envelope automaticamente. |
| Oracle como banco principal | [AppDbContext](../Data/AppDbContext.cs), [Models](../Models), [Repositories](../Repositories) | Atendido | As quatro entidades permanecem no Oracle; MongoDB não as substitui. |
| EF Core, relacionamentos e migrations | [migration existente](../Migrations/20260516211024_InitialCreate.cs), [AppDbContext](../Data/AppDbContext.cs), [Program](../Program.cs) | Atendido | Migration existente aplicada no startup. Nenhuma entidade Oracle ou migration alterada na Sprint 4. |
| MongoDB funcional | [MongoAuditRepository](../Audit/MongoAuditRepository.cs), [AuditEvent](../Audit/AuditEvent.cs), [testes reais](../GuardianPet.Tests.Integration/Endpoints/AuditEndpointsTests.cs) | Atendido | InsertOneAsync, Find, filtros, Sort/Skip/Limit e COUNT usados em MongoDB real e isolado. |
| Auditoria mínima e falhas seguras | [AuditService](../Audit/AuditService.cs), [decoradores](../Audit), [documentação](mongodb-audit.md) | Atendido | Created/Updated/Deleted após sucesso Oracle. Falha MongoDB gera warning seguro, preservando CRUD. Best-effort, sem outbox, replay ou transação distribuída. |
| JWT e consulta administrativa | [AuthService](../Services/AuthService.cs), [AuditController](../Controllers/AuditController.cs), [Program](../Program.cs) | Atendido | JWT validado; ManageUsers exige users.manage. Sem refresh token, revogação ou autorização por proprietário nos demais CRUDs. |
| Health checks | [OracleHealthCheck](../HealthChecks/OracleHealthCheck.cs), [Program](../Program.cs) | Atendido | /health verifica aplicação; /health/ready consulta Oracle. MongoDB não participa do readiness. |
| Logs e correlação | [CorrelationIdMiddleware](../Middlewares/CorrelationIdMiddleware.cs), [ErrorHandlingMiddleware](../Middlewares/ErrorHandlingMiddleware.cs), Serilog em [Program](../Program.cs) | Atendido | Logs JSON em console/arquivo, retenção de 14 arquivos; correlação na resposta e auditoria. Eventos não incluem payloads sensíveis. |
| OpenTelemetry e métricas HTTP | [GuardianPetTelemetry](../Observability/GuardianPetTelemetry.cs), [MetricsMiddleware](../Middlewares/MetricsMiddleware.cs), [Program](../Program.cs) | Atendido | Instrumentação ASP.NET Core/HttpClient, latência e erros 400..599 por padrão de rota; console em Development. |
| Abrangência de tracing manual | [PetService](../Services/PetService.cs), [PetRepository](../Repositories/PetRepository.cs) | Parcial | Spans manuais no fluxo de Pets, sem instrumentação SQL dedicada ou coletor OTLP. Confirmar se a rubrica exige cobertura mais ampla. |
| Testes unitários e integração isolada | [Unit](../GuardianPet.Tests.Unit), [Integration](../GuardianPet.Tests.Integration), [fixtures](../GuardianPet.Tests.Integration/Fixtures) | Atendido | 149 unitários + 189 integração = 338 aprovados, 0 falhas/ignorados. Oracle/MongoDB reais, bases GPTEST exclusivas e limpeza com validação de nome. |
| Métrica de cobertura de código | Microsoft.CodeCoverage 17.14.1, disponível pelo Test SDK; relatório Cobertura gerado na auditoria do Bloco 6 | Atendido | API GuardianPet.dll: 84,25% de linhas (2.059/2.444) e 84,60% de branches (467/552), com as duas suítes e assemblies de testes excluídos. Não há limiar mínimo confirmado na rubrica; a medição não comprova atendimento a um percentual obrigatório. |
| Documentação, equipe e execução local | [README](../README.md), [auditoria](mongodb-audit.md), [.env.example](../.env.example) | Atendido | Integrantes/RMs e GitHub preservados; Mermaid, exemplos, configurações e limitações documentados; placeholders sem credenciais reais. |
| Conferência da rubrica oficial | Não localizada no repositório | Pendente | Confirmar critérios e evidências exigidas antes da entrega; não foram inventados requisitos obrigatórios ou resultados de avaliação. |

## DevOps/Azure — responsabilidade do colega

Os itens abaixo são separados da aplicação .NET e não foram implementados ou
considerados concluídos nesta revisão. A ausência de evidência neste repositório
não permite afirmar o estado de outros repositórios da equipe.

| Requisito | Evidência no projeto | Situação | Observações |
|---|---|---|---|
| Pipeline CI/CD | Nenhuma pipeline localizada nos arquivos do projeto | Pendente | Responsabilidade do colega de DevOps; nenhuma pipeline criada aqui. |
| Azure, ACR/ACI e publicação | Sem evidência de execução ou URL de implantação neste repositório | Pendente | Conferir com o colega; não há alegação de deploy. |
| Segredos/configuração em nuvem | Apenas opções locais e variáveis documentadas | Pendente | Compose local não comprova configuração de produção. |
| Evidências de deploy e monitoramento remoto | Health checks e logs locais disponíveis | Pendente | URLs, capturas e provas de implantação cabem ao responsável. |

## Validação e riscos para avaliação

Comandos: dotnet build GuardianPet.sln, dotnet test GuardianPet.sln e
git diff --check. Validação em 12/10/2026: build com 0 erros/avisos,
338 testes aprovados (149 unitários, 189 integração), 0 falhas e 0 ignorados;
diff sem problemas de whitespace.

Cobertura medida no Bloco 6 com Microsoft.CodeCoverage 17.14.1, sem instalação
de dependências adicionais: **84,25% de linhas** e **84,60% de branches** na API.
O relatório final inclui a execução dos 338 testes aprovados (149 unitários e
189 de integração) e 62 arquivos instrumentados de GuardianPet.dll. Assemblies
de testes foram excluídos; código gerado de migrations/snapshot permaneceu incluído.
Configuração e relatório ficaram em diretório temporário, fora do projeto.
Não há limiar mínimo de cobertura confirmado na rubrica oficial; esse critério
deve ser conferido antes da avaliação, sem presumir nota ou cobertura total.

- A demonstração HATEOAS precisa de includeLinks=true; a opção preserva a compatibilidade.
- Testes MongoDB reais são ignorados quando o serviço está indisponível; a execução registrada não ignorou nenhum teste.
- Auditoria best-effort pode perder eventos; não oferece consistência transacional entre bancos.
- Readiness Oracle e tracing manual de Pets têm escopo limitado, explicitado na documentação.
- Defaults públicos de desenvolvimento não devem ser usados em ambientes compartilhados; .env.example usa placeholders.
- .env não é carregado por dotnet; o Compose só encaminha variáveis previstas. Administração exige configuração do operador.
- A aplicação aplica migrations no startup: uma conexão inadequada pode alterar o schema apontado.
- AUTHENTICATION.md registra a Sprint 3; suas contagens históricas não são os totais atuais.
- Nenhum código funcional, configuração de infraestrutura, entidade ou migration foi alterado no Bloco 5.
- O Git contém alterações acumuladas dos blocos anteriores; este bloco não cria commit, push, merge ou alteração da main.
