# Clyvo Pet Care API

API REST para acompanhamento da jornada de saúde de cachorros — vacinas, consultas, retornos e prevenção. Construída no contexto do challenge FIAP 2026 em parceria com a Clyvo.

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12.0-239120?style=flat-square&logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![EF Core](https://img.shields.io/badge/EF%20Core-8.0-512BD4?style=flat-square)](https://learn.microsoft.com/ef/core/)
[![Oracle](https://img.shields.io/badge/Oracle-19c-F80000?style=flat-square&logo=oracle&logoColor=white)](https://www.oracle.com/database/)
[![OpenAPI](https://img.shields.io/badge/OpenAPI-3.0-85EA2D?style=flat-square&logo=swagger&logoColor=black)](https://swagger.io/)
[![Serilog](https://img.shields.io/badge/Serilog-8-1E90FF?style=flat-square)](https://serilog.net/)
[![OpenTelemetry](https://img.shields.io/badge/OpenTelemetry-1.9-4B5563?style=flat-square)](https://opentelemetry.io/)
[![xUnit](https://img.shields.io/badge/xUnit-2.9-512BD4?style=flat-square)](https://xunit.net/)

## O problema

Hoje o tutor só procura o veterinário quando algo dá errado: sintomas agudos, emergências, ou aquela vacina obrigatória que ele lembrou tarde demais. O resultado é uma jornada cheia de buracos — tratamento abandonado no meio, dose esquecida, agravamento evitável, vínculo frágil com a clínica.

A proposta aqui é inverter essa lógica. Em vez de só reagir, a API serve de base para uma jornada **contínua e preventiva**: a clínica tem visão completa do que cada pet precisa, e o tutor recebe os empurrões certos no momento certo.

## O que a API faz

- Cadastro de tutores, pets, veterinários, consultas e vacinas
- Carteira de vacinação digital com controle de próximas doses
- Agendamento de consultas com sugestão de retorno automática
- Endpoints dedicados ao motor preventivo (doses atrasadas, retornos próximos)
- Busca por microchip, CRMV, CPF, tutor — o que clínica precisa todo dia
- Paginação, filtros e ordenação em todas as listagens
- **Sprint 3:** observabilidade completa (Serilog + OpenTelemetry + Health Checks) e suíte de testes automatizados (xUnit + Moq + WebApplicationFactory)

## Stack

.NET 8 com ASP.NET Core (Controllers), Entity Framework Core 8 sobre Oracle 19c (servidor da FIAP) e documentação via Swagger/OpenAPI 3. Observabilidade com Serilog para logging estruturado e OpenTelemetry para tracing distribuído e métricas Prometheus. Testes com xUnit, Moq, EF InMemory e WebApplicationFactory.

## Estrutura

```
DotNet_Challenge/
├── Clyvo.PetCare.Api/
│   ├── Controllers/            # endpoints REST
│   ├── Application/
│   │   ├── DTOs/               # contratos da API
│   │   └── Services/           # regras de negócio
│   ├── Domain/
│   │   ├── Entities/           # modelo de domínio puro
│   │   └── Enums/
│   ├── Infrastructure/
│   │   ├── Persistence/        # DbContext + mappings Fluent API
│   │   └── Observability/      # middleware de CorrelationId
│   ├── Migrations/             # migrations do EF Core (versionadas)
│   └── Program.cs
├── tests/
│   ├── Clyvo.PetCare.UnitTests/          # testes de domínio, services e controllers
│   └── Clyvo.PetCare.IntegrationTests/   # testes HTTP com WebApplicationFactory
└── scripts/                    # DDL, seed e cleanup SQL
```

Camadas separadas, controllers finos, services concentrando a lógica e o domínio sem dependência de framework. Os DTOs ficam isolados das entidades — a API nunca devolve o modelo de domínio cru.

## Modelo de domínio

```
Tutor (1) ─── (N) Pet
                 │
                 ├─── (N) Consulta ─── Veterinario
                 │
                 └─── (N) Vacina   ─── Veterinario
```

Cinco entidades. Duas delas carregam o coração da continuidade: `Consulta.ProximoRetorno` e `Vacina.DataProximaDose`. É a partir desses dois campos que a clínica sabe quem precisa ser lembrado de quê — sem precisar puxar relatório nenhum.

## Endpoints

A API expõe 31 endpoints REST sob `/api/v1/`. Visão geral:

**Tutores** — `/api/v1/tutores`
GET (lista paginada, por id, por CPF), POST, PUT, DELETE

**Pets** — `/api/v1/pets`
GET (lista paginada, por id, por tutor, por microchip), POST, PUT, DELETE

**Veterinários** — `/api/v1/veterinarios`
GET (lista paginada, por id, por CRMV), POST, PUT, DELETE

**Consultas** — `/api/v1/consultas`
GET (lista paginada, por id, por pet, retornos próximos), POST, PUT, DELETE

**Vacinas** — `/api/v1/vacinas`
GET (lista paginada, por id, por pet, doses atrasadas, próximas doses), POST, PUT, DELETE

Além dos endpoints de negócio, a API expõe rotas dedicadas a observabilidade (detalhadas na próxima seção). A documentação interativa fica em `/swagger` com a API rodando.

## Observabilidade

A Sprint 3 trouxe uma stack completa de observabilidade — a API deixou de ser uma caixa preta.

### Health Checks

Três endpoints para diferentes propósitos (típicos de deployment em Kubernetes/container orchestrator):

| Endpoint | Uso | Verifica |
| --- | --- | --- |
| `GET /health` | check completo | `self` + banco Oracle |
| `GET /health/live` | liveness probe | apenas `self` (processo respondendo) |
| `GET /health/ready` | readiness probe | dependências (Oracle conectado) |

Resposta em JSON no formato do HealthChecks UI (`application/json`):

```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0123456",
  "entries": {
    "self":            { "status": "Healthy", "duration": "00:00:00.0000123" },
    "oracle-database": { "status": "Healthy", "duration": "00:00:00.0123333" }
  }
}
```

Basta apontar seu container orchestrator (ou UptimeRobot, Better Uptime, Grafana) para `/health/live` (liveness) e `/health/ready` (readiness). Status HTTP 200 = healthy, 503 = unhealthy.

### Logging estruturado — Serilog

Logs enriquecidos com `CorrelationId`, `MachineName`, `ThreadId` e `Application`. Dois sinks:

- **Console** — desenvolvimento e container logs
- **File** — arquivo diário rotativo em `logs/clyvo-api-YYYYMMDD.log`, retenção de 14 dias

Cada requisição HTTP gera um `X-Correlation-Id` único (o cliente pode enviar o header pra propagar de outra origem — a API preserva). Todos os logs emitidos durante o pipeline da requisição carregam esse ID, o que permite rastrear uma jornada inteira em qualquer agregador (Loki, Elastic, Seq).

Configuração de níveis, sinks e templates fica toda em `appsettings.json` sob a chave `Serilog` — pode ser ajustada em produção sem recompilar.

### Tracing e métricas — OpenTelemetry

Instrumentação automática para:

- **ASP.NET Core** (spans de request/response)
- **HttpClient** (chamadas HTTP externas)
- **Entity Framework Core** (queries no Oracle)
- **Runtime** (GC, threads, memória)
- **Process** (CPU, handles)

**Tracing**: exportado por padrão para o Console (útil pra dev). Em produção, basta trocar para OTLP e apontar para Jaeger/Tempo/Zipkin/Application Insights com uma linha em `Program.cs`.

**Métricas**: expostas no formato Prometheus em `GET /metrics`. Um Prometheus server ou Grafana Agent pode fazer scrape direto:

```yaml
scrape_configs:
  - job_name: clyvo-petcare-api
    static_configs:
      - targets: ['localhost:5000']
```

Métricas inclusas: `http.server.request.duration`, `http.client.request.duration`, `process.runtime.dotnet.gc.*`, `process.runtime.dotnet.thread_pool.*`, entre outras.

## Testes automatizados

Suíte separada em dois projetos, seguindo padrão AAA (Arrange, Act, Assert) e nomenclatura `Metodo_Cenario_ResultadoEsperado`.

### Estrutura

```
tests/
├── Clyvo.PetCare.UnitTests/            # xUnit + Moq + EF InMemory
│   ├── Domain/                         # testes de entidades (Pet.IdadeAnos, etc)
│   ├── Services/                       # testes das regras de negócio isolando o DB via InMemory
│   ├── Controllers/                    # controllers com Moq das interfaces de serviço
│   └── TestHelpers/                    # InMemoryDbContextFactory + EntityBuilder
│
└── Clyvo.PetCare.IntegrationTests/     # xUnit + WebApplicationFactory
    ├── Fixtures/                       # ClyvoWebApplicationFactory (swap Oracle → InMemory)
    ├── Collections/                    # ApiTestCollection (compartilha host entre testes)
    ├── Controllers/                    # POST/GET/PUT/DELETE percorrendo o pipeline HTTP inteiro
    └── Observability/                  # health checks, metrics, correlation id
```

São **73 testes** no total — 53 unitários e 20 de integração, todos passando. Cobertura:

- **Domínio**: cálculo de idade do pet (casos limite incluídos)
- **Services**: CRUD completo dos cinco agregados, com validações de duplicidade, integridade referencial e regras de negócio (tutor com pets não deleta, consulta realizada não deleta, doses futuras rejeitadas)
- **Controllers (unit)**: mapeamento HTTP correto — 200/201/204/400/404 conforme cenário, com Moq das dependências
- **Endpoints (integration)**: fluxos ponta-a-ponta via `HttpClient` — model binding, serialização JSON, persistência, resposta
- **Observabilidade**: `/health`, `/health/live`, `/health/ready`, `/metrics` e propagação do `X-Correlation-Id`

### Sobre autenticação nos testes

Os testes de integração cobrem o fluxo HTTP completo — model binding, validação, serviço, persistência e resposta — mas não exercitam autenticação, porque a API ainda não tem.

Isso é decisão de escopo, não lacuna. Nas sprints anteriores a Clyvo Pet Care foi definida como serviço de back-office da clínica, rodando atrás do gateway interno, com o controle de acesso vivendo na camada de borda. Introduzir JWT agora só para ter o que testar acrescentaria superfície sem resolver problema real.

A autenticação está no roadmap (ver "Próximos passos") com perfis de tutor, veterinário e admin de clínica. Quando entrar, os testes de integração ganham os cenários de 401 e 403 seguindo o mesmo padrão AAA já usado aqui — a `ClyvoWebApplicationFactory` já está preparada para injetar um handler de autenticação de teste.

### Como executar

Todos os testes:

```bash
dotnet test
```

Apenas unitários:

```bash
dotnet test tests/Clyvo.PetCare.UnitTests
```

Apenas integração:

```bash
dotnet test tests/Clyvo.PetCare.IntegrationTests
```

Com relatório de cobertura de código:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

Os arquivos `coverage.cobertura.xml` gerados podem ser abertos com o **Coverage Gutters** do VS Code ou convertidos em HTML com o ReportGenerator (`dotnet tool install -g dotnet-reportgenerator-globaltool`).

Os testes de integração **não** dependem do Oracle da FIAP — a `ClyvoWebApplicationFactory` substitui o provider por EF Core InMemory, garantindo execução rápida e determinística mesmo offline.

## Banco de dados

O projeto usa Oracle 19c (servidor da FIAP). A connection string fica em `appsettings.json` apontando para `oracle.fiap.com.br:1521/ORCL`. As migrations do EF Core criam cinco tabelas:

- `TB_TUTOR`
- `TB_VETERINARIO`
- `TB_PET` (FK para tutor)
- `TB_CONSULTA` (FK para pet e veterinário)
- `TB_VACINA` (FK para pet, FK opcional para veterinário)

Para quem prefere ir direto pelo SQL, os scripts DDL, seed e cleanup estão em `/scripts` e podem ser executados via SQL Developer ou DBeaver.

## Como rodar

Precisa do .NET 8 SDK e de acesso ao Oracle FIAP (ou um Oracle 19c local rodando).

Antes de subir, abra `Clyvo.PetCare.Api/appsettings.json` (ou crie `appsettings.Development.json`, que fica fora do Git) e troque o RM e a senha na connection string `OracleFiap`. Depois:

```bash
git clone https://github.com/SEU_USUARIO/clyvo-petcare-api.git
cd clyvo-petcare-api

dotnet tool install --global dotnet-ef   # se ainda não tiver
dotnet restore
dotnet ef database update --project Clyvo.PetCare.Api   # aplica as migrations no Oracle
dotnet run --project Clyvo.PetCare.Api
```

A API sobe em `http://localhost:5000` e o Swagger em `/swagger`. No Visual Studio, basta abrir a `.sln` e apertar F5.

Quem não quiser usar EF Core pode rodar os scripts de `/scripts` direto no SQL Developer — criam o schema e populam dados de exemplo equivalentes.

## Próximos passos

- OTLP exporter apontado para Jaeger/Grafana Tempo pra visualizar traces em ambiente compartilhado
- Alerting no Grafana em cima das métricas Prometheus (SLO de latência p95, error budget)
- Autenticação JWT com perfis de tutor, veterinário e admin de clínica
- Dashboard analítico para clínicas (taxa de retorno, abandono de tratamento, LTV)
- Módulo de medicamentos contínuos (já previsto no modelo de domínio)

---

Challenge FIAP 2026 — Clyvo VET.
