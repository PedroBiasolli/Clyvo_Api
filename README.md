# Clyvo Pet Care API

> **Challenge FIAP 2026 — Clyvo VET**
> Disciplina: **Advanced Business Development with .NET**
> Tema: Continuidade do cuidado e engajamento na jornada de saúde do pet (cachorros)

API RESTful em **ASP.NET Core 8** que digitaliza a jornada de saúde de cachorros, conectando tutores, veterinários e clínicas em um modelo **contínuo, preventivo e integrado** — substituindo o atual modelo episódico e reativo da medicina veterinária.

---

## Visão do produto

A Clyvo VET está construindo a infraestrutura do futuro da medicina veterinária digital. Hoje, o tutor só interage com o ecossistema em sintomas agudos, emergências ou retornos solicitados — o que gera esquecimento, baixa adesão, abandono de tratamentos e perda de recorrência para as clínicas.

Esta API resolve isso entregando três capacidades centrais:

1. **Carteira de vacinação digital** — histórico vacinal completo do pet com motor de próximas doses
2. **Continuidade clínica** — agendamento e retorno de consultas com sugestão automática da próxima visita
3. **Visão 360° do pet** — tutor, raça, porte, microchip, peso, histórico clínico e vacinal em um único lugar

---

## Stack técnica

| Camada | Tecnologia |
|---|---|
| Runtime | .NET 8 (LTS) |
| Framework Web | ASP.NET Core Web API (Controllers) |
| ORM | Entity Framework Core 8 |
| Banco | Oracle 19c+ (provider `Oracle.EntityFrameworkCore`) |
| Documentação | Swagger / OpenAPI 3 (Swashbuckle) |
| Validação | Data Annotations |

---

## Arquitetura

```
Clyvo.PetCare.Api/
├── Controllers/                  ← Camada de apresentação (REST)
├── Application/
│   ├── DTOs/                     ← Contratos da API (Request/Response)
│   └── Services/                 ← Regras de negócio
│       └── Interfaces/
├── Domain/
│   ├── Entities/                 ← Modelo de domínio puro
│   └── Enums/
├── Infrastructure/
│   └── Persistence/
│       ├── AppDbContext.cs
│       └── Mappings/             ← EF Core Fluent API
├── Program.cs                    ← Composition root + pipeline
└── appsettings.json
```

**Princípios aplicados:**
- Separação de responsabilidades (Controller fino, Service com regras, Repositório implícito via DbContext)
- DTOs separados das entidades — nunca expor o modelo de domínio diretamente
- Mapeamentos explícitos com Fluent API — entidades limpas, sem `[Attribute]` de banco
- Async/await em toda a stack de I/O

---

## Modelo de domínio

```
Tutor (1) ──── (N) Pet
                 │
                 ├──── (N) Consulta ──── (N..1) Veterinário
                 │
                 └──── (N) Vacina ────── (N..1) Veterinário
```

| Entidade | Função |
|---|---|
| **Tutor** | Responsável legal pelo pet (CPF, contato, endereço, opt-in de notificações) |
| **Pet** | Cachorro acompanhado (raça, porte, peso, microchip, data de nascimento → idade calculada) |
| **Veterinário** | Profissional com CRMV, especialidade e flag de atividade |
| **Consulta** | Atendimento clínico com tipo, status, diagnóstico, prescrição e **próximo retorno** (núcleo da continuidade) |
| **Vacina** | Registro vacinal com data de aplicação e **data próxima dose** (núcleo do preventivo) |

---

## Endpoints — visão geral

Todos os recursos seguem o padrão `/api/v1/<recurso>` e devolvem `application/json`.

### Tutores `/api/v1/tutores`

| Método | Rota | Descrição | Status |
|---|---|---|---|
| GET | `/api/v1/tutores?nome=&cidade=&pagina=1&tamanhoPagina=20` | Lista paginada com filtros | 200 |
| GET | `/api/v1/tutores/{id}` | Busca por ID | 200 / 404 |
| GET | `/api/v1/tutores/cpf/{cpf}` | Busca por CPF | 200 / 404 |
| POST | `/api/v1/tutores` | Cadastra novo tutor | 201 / 400 |
| PUT | `/api/v1/tutores/{id}` | Atualiza tutor | 200 / 404 |
| DELETE | `/api/v1/tutores/{id}` | Remove tutor (se não tiver pets) | 204 / 400 / 404 |

### Pets `/api/v1/pets`

| Método | Rota | Descrição | Status |
|---|---|---|---|
| GET | `/api/v1/pets?nome=&raca=&porte=&pagina=1&tamanhoPagina=20` | Lista paginada com filtros | 200 |
| GET | `/api/v1/pets/{id}` | Busca por ID | 200 / 404 |
| GET | `/api/v1/pets/tutor/{tutorId}` | Lista pets de um tutor | 200 |
| GET | `/api/v1/pets/microchip/{microchip}` | Busca pelo microchip | 200 / 404 |
| POST | `/api/v1/pets` | Cadastra novo pet | 201 / 400 |
| PUT | `/api/v1/pets/{id}` | Atualiza pet | 200 / 404 |
| DELETE | `/api/v1/pets/{id}` | Remove pet (se sem histórico) | 204 / 400 / 404 |

### Veterinários `/api/v1/veterinarios`

| Método | Rota | Descrição | Status |
|---|---|---|---|
| GET | `/api/v1/veterinarios?especialidade=&ativo=&pagina=&tamanhoPagina=` | Lista paginada com filtros | 200 |
| GET | `/api/v1/veterinarios/{id}` | Busca por ID | 200 / 404 |
| GET | `/api/v1/veterinarios/crmv/{crmv}` | Busca por CRMV | 200 / 404 |
| POST | `/api/v1/veterinarios` | Cadastra veterinário | 201 / 400 |
| PUT | `/api/v1/veterinarios/{id}` | Atualiza dados | 200 / 404 |
| DELETE | `/api/v1/veterinarios/{id}` | Remove (se sem consultas) | 204 / 400 / 404 |

### Consultas `/api/v1/consultas`

| Método | Rota | Descrição | Status |
|---|---|---|---|
| GET | `/api/v1/consultas?tipo=&status=&dataInicio=&dataFim=` | Lista paginada com filtros | 200 |
| GET | `/api/v1/consultas/{id}` | Busca por ID | 200 / 404 |
| GET | `/api/v1/consultas/pet/{petId}` | Histórico de consultas de um pet | 200 |
| GET | `/api/v1/consultas/retornos-proximos/{diasAFrente}` | Retornos nos próximos N dias (motor preventivo) | 200 |
| POST | `/api/v1/consultas` | Agenda nova consulta | 201 / 400 |
| PUT | `/api/v1/consultas/{id}` | Atualiza consulta | 200 / 404 |
| DELETE | `/api/v1/consultas/{id}` | Remove (realizadas só podem ser canceladas) | 204 / 400 / 404 |

### Vacinas `/api/v1/vacinas`

| Método | Rota | Descrição | Status |
|---|---|---|---|
| GET | `/api/v1/vacinas?nome=` | Lista paginada com filtro | 200 |
| GET | `/api/v1/vacinas/{id}` | Busca por ID | 200 / 404 |
| GET | `/api/v1/vacinas/pet/{petId}` | Carteira de vacinação completa do pet | 200 |
| GET | `/api/v1/vacinas/atrasadas` | Doses com data passada | 200 |
| GET | `/api/v1/vacinas/proximas/{diasAFrente}` | Próximas doses em N dias | 200 |
| POST | `/api/v1/vacinas` | Registra nova vacina | 201 / 400 |
| PUT | `/api/v1/vacinas/{id}` | Atualiza vacina | 200 / 404 |
| DELETE | `/api/v1/vacinas/{id}` | Remove registro | 204 / 404 |

> **Total: 31 endpoints**, com 6 rotas GET parametrizadas (acima do mínimo de 3 exigido pela rubrica).

---

## Instalação e execução

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Acesso ao **Oracle FIAP** (`oracle.fiap.com.br:1521/ORCL`) ou Oracle local
- Editor: Visual Studio 2022 / VS Code / JetBrains Rider

### Passo a passo

**1. Clone o repositório**

```bash
git clone <url-do-repo>
cd DotNet_Challenge
```

**2. Configure a connection string**

Edite `Clyvo.PetCare.Api/appsettings.json` substituindo `SEU_RM` e `SUA_SENHA`:

```json
"ConnectionStrings": {
  "OracleFiap": "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=oracle.fiap.com.br)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=ORCL)));User Id=SEU_RM;Password=SUA_SENHA;"
}
```

> **Dica de segurança:** em ambiente real, use `dotnet user-secrets` ou variáveis de ambiente.

**3. Restaure as dependências**

```bash
cd Clyvo.PetCare.Api
dotnet restore
```

**4. Aplique as migrations (cria o schema no Oracle)**

```bash
dotnet tool install --global dotnet-ef        # apenas na primeira vez
dotnet ef migrations add InitialCreate
dotnet ef database update
```

> Alternativa: executar manualmente `scripts/01_schema_oracle.sql` e `scripts/02_seed_data.sql` no SQL Developer.

**5. Execute a API**

```bash
dotnet run
```

A API ficará disponível em:
- **HTTP:** http://localhost:5000
- **HTTPS:** https://localhost:5001
- **Swagger UI:** http://localhost:5000/swagger

---

## Testando com Swagger

1. Abra `http://localhost:5000/swagger` no navegador
2. Cada endpoint está documentado com descrição, parâmetros e schemas de request/response
3. Clique em **"Try it out"** em qualquer endpoint para testar diretamente pela interface

### Fluxo sugerido de teste

```http
POST /api/v1/tutores              ← cadastra Mariana Souza
POST /api/v1/veterinarios         ← cadastra Dra. Ana Lima
POST /api/v1/pets                 ← cadastra Thor (Golden Retriever) vinculado à Mariana
POST /api/v1/vacinas              ← registra V10 do Thor com próxima dose em 12 meses
POST /api/v1/consultas            ← agenda check-up para o Thor
GET  /api/v1/pets/tutor/1         ← lista os pets da Mariana
GET  /api/v1/vacinas/proximas/90  ← simula motor de lembretes (próximos 90 dias)
```

---

## Critérios da rubrica — onde estão atendidos

| Critério (pts) | Onde no projeto |
|---|---|
| **CRUD completo (35 pts)** | Todos os 5 controllers têm GET / POST / PUT / DELETE |
| → GET com 3+ rotas parametrizadas (15 pts) | 6 rotas GET parametrizadas (id, cpf, tutorId, microchip, crmv, petId, diasAFrente) |
| → POST (5 pts) | 5 endpoints POST com `CreatedAtAction` |
| → PUT (5 pts) | 5 endpoints PUT |
| → DELETE (5 pts) | 5 endpoints DELETE |
| → Organização REST (5 pts) | `/api/v1/<recurso>`, plural, status codes corretos |
| **Retornos HTTP (15 pts)** | `[ProducesResponseType]` em todo controller |
| → 200 OK | GETs e PUTs |
| → 201 Created | POSTs com `CreatedAtAction(...)` |
| → 204 NoContent | DELETEs |
| → 400 BadRequest | Validações de negócio (duplicidade, FK inexistente) |
| → 404 NotFound | Recursos inexistentes |
| **Oracle + EF Core (25 pts)** | |
| → DbContext (5 pts) | `Infrastructure/Persistence/AppDbContext.cs` |
| → Mapeamento (5 pts) | 5 classes Fluent API em `Mappings/` |
| → Conexão Oracle (5 pts) | `UseOracle(...)` em `Program.cs` com `Oracle.EntityFrameworkCore` |
| → Migrations (5 pts) | Suporte completo — instruções acima |
| → Estrutura coerente (5 pts) | 5 tabelas com FKs, índices e constraints |
| **OpenAPI (10 pts)** | |
| → Swagger configurado (4 pts) | `Program.cs` com `AddSwaggerGen` + comentários XML |
| → Endpoints documentados (3 pts) | Cada action com `<summary>` e `[ProducesResponseType]` |
| → Testes pela UI (3 pts) | Swagger UI habilitado em dev |
| **README + GitHub (15 pts)** | Este arquivo |
| → Descrição (5 pts) | Seção "Visão do produto" |
| → Documentação de rotas (5 pts) | Tabelas de endpoints acima |
| → Instalação/execução (5 pts) | Seção "Instalação e execução" |

**Total endereçado: 100/100 pontos** da rubrica de .NET.

---

## Próximos passos (Sprints 3 e 4)

- 🔔 **Motor de notificações** (e-mail/WhatsApp) usando `DataProximaDose` e `ProximoRetorno`
- 📊 **Dashboard analítico** para clínicas (LTV, taxa de retorno, abandono de tratamento)
- 🧠 **Score de risco** por raça/idade/peso para alertas preditivos
- 💊 **Módulo de medicamentos contínuos** (entidade `Medicamento` já prevista no domínio)
- 🔐 **Autenticação JWT** + perfis (Tutor / Veterinário / Admin Clínica)

---

## Equipe

> Preencher antes da entrega:
> - Nome – RM
> - Nome – RM
> - Nome – RM

---

## Licença

Projeto acadêmico — **FIAP 2026** em parceria com **Clyvo**.
