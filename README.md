# Clyvo Pet Care API

API REST para acompanhamento da jornada de saúde de cachorros — vacinas, consultas, retornos e prevenção. Construída no contexto do challenge FIAP 2026 em parceria com a Clyvo.

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12.0-239120?style=flat-square&logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![EF Core](https://img.shields.io/badge/EF%20Core-8.0-512BD4?style=flat-square)](https://learn.microsoft.com/ef/core/)
[![Oracle](https://img.shields.io/badge/Oracle-11g%20XE-F80000?style=flat-square&logo=oracle&logoColor=white)](https://www.oracle.com/database/)
[![OpenAPI](https://img.shields.io/badge/OpenAPI-3.0-85EA2D?style=flat-square&logo=swagger&logoColor=black)](https://swagger.io/)

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

## Stack

.NET 8 com ASP.NET Core (Controllers), Entity Framework Core 8 sobre Oracle 11g XE (servidor da FIAP) e documentação via Swagger/OpenAPI 3.

## Estrutura

```
Clyvo.PetCare.Api/
├── Controllers/          # endpoints REST
├── Application/
│   ├── DTOs/             # contratos da API
│   └── Services/         # regras de negócio
├── Domain/
│   ├── Entities/         # modelo de domínio puro
│   └── Enums/
├── Infrastructure/
│   └── Persistence/      # DbContext + mappings Fluent API
└── Program.cs
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

A API expõe 31 endpoints sob `/api/v1/`. Visão geral:

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

A documentação completa fica disponível em `/swagger` quando o projeto está rodando, com schemas de request/response e a possibilidade de testar cada rota direto pela interface.

## Banco de dados

O projeto usa Oracle. A connection string fica em `appsettings.json` apontando para o servidor da FIAP. As migrations do EF Core criam cinco tabelas:

- `TB_TUTOR`
- `TB_VETERINARIO`
- `TB_PET` (FK para tutor)
- `TB_CONSULTA` (FK para pet e veterinário)
- `TB_VACINA` (FK para pet, FK opcional para veterinário)

Para quem prefere ir direto pelo SQL, os scripts DDL e de seed estão em `/scripts` e podem ser executados via SQL Developer ou DBeaver.

## Como rodar

Precisa do .NET 8 SDK e de acesso ao Oracle FIAP (ou um Oracle local rodando).

Antes de subir, abra `Clyvo.PetCare.Api/appsettings.json` e troque o RM e a senha na connection string `OracleFiap`. Depois:

```bash
git clone https://github.com/SEU_USUARIO/clyvo-petcare-api.git
cd clyvo-petcare-api/Clyvo.PetCare.Api

dotnet tool install --global dotnet-ef   # se ainda não tiver
dotnet restore
dotnet ef database update                # aplica as migrations no Oracle
dotnet run
```

A API sobe em `http://localhost:5000` e o Swagger em `/swagger`. No Visual Studio, basta abrir a `.sln` e apertar F5.

Quem não quiser usar EF Core pode rodar os scripts de `/scripts` direto no SQL Developer — criam o schema e populam dados de exemplo equivalentes.

## Próximos passos

A ideia é evoluir o projeto nas próximas sprints com:

- Notificações automáticas (e-mail/WhatsApp) puxando dos campos de próxima dose e próximo retorno
- Score de risco por raça, idade e peso para alertas preditivos
- Módulo de medicamentos contínuos (já previsto no modelo de domínio)
- Autenticação JWT com perfis de tutor, veterinário e admin de clínica
- Dashboard analítico para clínicas (taxa de retorno, abandono de tratamento, LTV)

---

Challenge FIAP 2026 — Clyvo VET.
