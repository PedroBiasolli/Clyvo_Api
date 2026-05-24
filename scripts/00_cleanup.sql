-- ============================================================================
-- Script de limpeza — usado quando uma migration do EF Core falhou no meio
-- e deixou o schema em estado parcial. Roda no SQL Developer ou DBeaver
-- conectado ao seu schema FIAP (RMxxxxxx) antes de re-executar:
--     dotnet ef database update
--
-- Cada bloco PL/SQL ignora "tabela não existe" para que o script seja idempotente.
-- ============================================================================

BEGIN EXECUTE IMMEDIATE 'DROP TABLE TB_VACINA CASCADE CONSTRAINTS'; EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN EXECUTE IMMEDIATE 'DROP TABLE TB_CONSULTA CASCADE CONSTRAINTS'; EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN EXECUTE IMMEDIATE 'DROP TABLE TB_PET CASCADE CONSTRAINTS'; EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN EXECUTE IMMEDIATE 'DROP TABLE TB_VETERINARIO CASCADE CONSTRAINTS'; EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN EXECUTE IMMEDIATE 'DROP TABLE TB_TUTOR CASCADE CONSTRAINTS'; EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN EXECUTE IMMEDIATE 'DROP TABLE "__EFMigrationsHistory" CASCADE CONSTRAINTS'; EXCEPTION WHEN OTHERS THEN NULL; END;
/

-- Sequences eventualmente deixadas pelo EF (caso a migration tenha chegado a criar alguma)
BEGIN
  FOR s IN (SELECT sequence_name FROM user_sequences WHERE sequence_name LIKE 'TB_%') LOOP
    EXECUTE IMMEDIATE 'DROP SEQUENCE ' || s.sequence_name;
  END LOOP;
END;
/

COMMIT;
