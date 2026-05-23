# Scripts SQL — Clyvo Pet Care

Esta pasta contém scripts de criação de schema e dados de exemplo para o Oracle.

## Conteúdo

| Arquivo | Descrição |
|---------|-----------|
| `01_schema_oracle.sql` | DDL completo: tabelas, constraints, índices |
| `02_seed_data.sql` | Dados de exemplo (tutores, pets, veterinários, consultas, vacinas) |

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
