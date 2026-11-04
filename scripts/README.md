# Scripts SQL — Clyvo Pet Care

Esta pasta contém scripts de criação de schema e dados de exemplo para o Oracle.

## Conteúdo

| Arquivo | Descrição |
|---------|-----------|
| `01_schema_oracle.sql` | DDL completo: tabelas, constraints, índices (inclui `TB_USUARIO` da Sprint 4) |
| `00_cleanup.sql` | Remove todas as tabelas (inclusive `__EFMigrationsHistory`) para recomeçar |
| `02_seed_data.sql` | Dados de exemplo (tutores, pets, veterinários, consultas, vacinas e usuários da API) |

## Quando usar

A forma **recomendada** de criar o schema é via EF Core Migrations (veja README principal). Estes scripts SQL servem como:

1. Referência de schema para a entrega de **Banco de Dados** (modelagem física)
2. Fallback caso `dotnet ef database update` falhe
3. Material para a disciplina **Mastering Relational and Non-Relational Database** (3FN, joins, procedures)

## Ordem de execução

```sql
-- 1) Criar schema
@01_schema_oracle.sql

-- 2) Carregar dados de exemplo
@02_seed_data.sql
```

> **Atenção:** Em ambiente de produção/avaliação, NÃO execute o `DROP TABLE` comentado no início do schema sem ter certeza do que está fazendo.

## Usuários de demonstração (seed)

| E-mail | Perfil | Senha |
|---|---|---|
| admin@clyvo.com.br | Admin | Clyvo@2026 |
| ana.lima@clyvo.com.br | Veterinario | Clyvo@2026 |
| recepcao@clyvo.com.br | Atendente | Clyvo@2026 |

> Pela rota de Migrations não é preciso rodar este seed: a API cria o administrador automaticamente na primeira subida (seção `SeedAdmin` do `appsettings.json`).

> Os scripts são uma alternativa às Migrations — use **um** caminho ou o outro no mesmo schema, nunca os dois.
