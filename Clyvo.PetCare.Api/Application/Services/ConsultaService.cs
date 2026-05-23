using Clyvo.PetCare.Api.Application.DTOs.Common;
using Clyvo.PetCare.Api.Application.DTOs.Consulta;
using Clyvo.PetCare.Api.Application.Services.Interfaces;
using Clyvo.PetCare.Api.Domain.Entities;
using Clyvo.PetCare.Api.Domain.Enums;
using Clyvo.PetCare.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Clyvo.PetCare.Api.Application.Services;

public class ConsultaService : IConsultaService
{
    private readonly AppDbContext _db;

    public ConsultaService(AppDbContext db) => _db = db;

    public async Task<PagedResultDto<ConsultaResponseDto>> ListarAsync(
        TipoConsulta? tipo, StatusConsulta? status, DateTime? dataInicio, DateTime? dataFim,
        int pagina, int tamanhoPagina)
    {
        pagina = pagina < 1 ? 1 : pagina;
        tamanhoPagina = tamanhoPagina is < 1 or > 100 ? 20 : tamanhoPagina;

        var query = _db.Consultas
            .AsNoTracking()
            .Include(c => c.Pet)
            .Include(c => c.Veterinario)
            .AsQueryable();

        if (tipo.HasValue) query = query.Where(c => c.Tipo == tipo.Value);
        if (status.HasValue) query = query.Where(c => c.Status == status.Value);
        if (dataInicio.HasValue) query = query.Where(c => c.DataHora >= dataInicio.Value);
        if (dataFim.HasValue) query = query.Where(c => c.DataHora <= dataFim.Value);

        var total = await query.CountAsync();

        var itens = await query
            .OrderByDescending(c => c.DataHora)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync();

        return new PagedResultDto<ConsultaResponseDto>
        {
            Itens = itens.Select(MapToDto),
            Pagina = pagina,
            TamanhoPagina = tamanhoPagina,
            TotalRegistros = total
        };
    }

    public async Task<ConsultaResponseDto?> BuscarPorIdAsync(int id)
    {
        var consulta = await _db.Consultas
            .AsNoTracking()
            .Include(c => c.Pet)
            .Include(c => c.Veterinario)
            .FirstOrDefaultAsync(c => c.Id == id);

        return consulta is null ? null : MapToDto(consulta);
    }

    public async Task<IEnumerable<ConsultaResponseDto>> BuscarPorPetAsync(int petId)
    {
        var consultas = await _db.Consultas
            .AsNoTracking()
            .Include(c => c.Pet)
            .Include(c => c.Veterinario)
            .Where(c => c.PetId == petId)
            .OrderByDescending(c => c.DataHora)
            .ToListAsync();

        return consultas.Select(MapToDto);
    }

    public async Task<IEnumerable<ConsultaResponseDto>> BuscarRetornosProximosAsync(int diasAFrente)
    {
        if (diasAFrente < 1) diasAFrente = 7;
        var hoje = DateTime.Today;
        var limite = hoje.AddDays(diasAFrente);

        var consultas = await _db.Consultas
            .AsNoTracking()
            .Include(c => c.Pet)
            .Include(c => c.Veterinario)
            .Where(c => c.ProximoRetorno != null
                        && c.ProximoRetorno >= hoje
                        && c.ProximoRetorno <= limite)
            .OrderBy(c => c.ProximoRetorno)
            .ToListAsync();

        return consultas.Select(MapToDto);
    }

    public async Task<ConsultaResponseDto?> CriarAsync(CriarConsultaDto dto)
    {
        var petExiste = await _db.Pets.AnyAsync(p => p.Id == dto.PetId);
        var vetExiste = await _db.Veterinarios.AnyAsync(v => v.Id == dto.VeterinarioId && v.Ativo);
        if (!petExiste || !vetExiste) return null;

        var consulta = new Consulta
        {
            DataHora = dto.DataHora,
            Tipo = dto.Tipo,
            Status = StatusConsulta.Agendada,
            MotivoConsulta = dto.MotivoConsulta,
            PetId = dto.PetId,
            VeterinarioId = dto.VeterinarioId,
            DataCriacao = DateTime.UtcNow
        };

        _db.Consultas.Add(consulta);
        await _db.SaveChangesAsync();

        await _db.Entry(consulta).Reference(c => c.Pet).LoadAsync();
        await _db.Entry(consulta).Reference(c => c.Veterinario).LoadAsync();
        return MapToDto(consulta);
    }

    public async Task<ConsultaResponseDto?> AtualizarAsync(int id, AtualizarConsultaDto dto)
    {
        var consulta = await _db.Consultas
            .Include(c => c.Pet)
            .Include(c => c.Veterinario)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (consulta is null) return null;

        consulta.DataHora = dto.DataHora;
        consulta.Tipo = dto.Tipo;
        consulta.Status = dto.Status;
        consulta.MotivoConsulta = dto.MotivoConsulta;
        consulta.Diagnostico = dto.Diagnostico;
        consulta.Prescricao = dto.Prescricao;
        consulta.Observacoes = dto.Observacoes;
        consulta.ProximoRetorno = dto.ProximoRetorno;
        consulta.ValorCobrado = dto.ValorCobrado;

        await _db.SaveChangesAsync();
        return MapToDto(consulta);
    }

    public async Task<bool> RemoverAsync(int id)
    {
        var consulta = await _db.Consultas.FirstOrDefaultAsync(c => c.Id == id);
        if (consulta is null) return false;

        if (consulta.Status == StatusConsulta.Realizada)
            throw new InvalidOperationException("Consultas realizadas não podem ser removidas — alterem o status para Cancelada.");

        _db.Consultas.Remove(consulta);
        await _db.SaveChangesAsync();
        return true;
    }

    private static ConsultaResponseDto MapToDto(Consulta c) => new()
    {
        Id = c.Id,
        DataHora = c.DataHora,
        Tipo = c.Tipo,
        Status = c.Status,
        MotivoConsulta = c.MotivoConsulta,
        Diagnostico = c.Diagnostico,
        Prescricao = c.Prescricao,
        Observacoes = c.Observacoes,
        ProximoRetorno = c.ProximoRetorno,
        ValorCobrado = c.ValorCobrado,
        DataCriacao = c.DataCriacao,
        PetId = c.PetId,
        PetNome = c.Pet?.Nome,
        VeterinarioId = c.VeterinarioId,
        VeterinarioNome = c.Veterinario?.Nome
    };
}
