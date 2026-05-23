using Clyvo.PetCare.Api.Application.DTOs.Common;
using Clyvo.PetCare.Api.Application.DTOs.Vacina;
using Clyvo.PetCare.Api.Application.Services.Interfaces;
using Clyvo.PetCare.Api.Domain.Entities;
using Clyvo.PetCare.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Clyvo.PetCare.Api.Application.Services;

public class VacinaService : IVacinaService
{
    private readonly AppDbContext _db;

    public VacinaService(AppDbContext db) => _db = db;

    public async Task<PagedResultDto<VacinaResponseDto>> ListarAsync(string? nome, int pagina, int tamanhoPagina)
    {
        pagina = pagina < 1 ? 1 : pagina;
        tamanhoPagina = tamanhoPagina is < 1 or > 100 ? 20 : tamanhoPagina;

        var query = _db.Vacinas
            .AsNoTracking()
            .Include(v => v.Pet)
            .Include(v => v.Veterinario)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(nome))
            query = query.Where(v => v.Nome.ToUpper().Contains(nome.ToUpper()));

        var total = await query.CountAsync();

        var itens = await query
            .OrderByDescending(v => v.DataAplicacao)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync();

        return new PagedResultDto<VacinaResponseDto>
        {
            Itens = itens.Select(MapToDto),
            Pagina = pagina,
            TamanhoPagina = tamanhoPagina,
            TotalRegistros = total
        };
    }

    public async Task<VacinaResponseDto?> BuscarPorIdAsync(int id)
    {
        var vacina = await _db.Vacinas
            .AsNoTracking()
            .Include(v => v.Pet)
            .Include(v => v.Veterinario)
            .FirstOrDefaultAsync(v => v.Id == id);

        return vacina is null ? null : MapToDto(vacina);
    }

    public async Task<IEnumerable<VacinaResponseDto>> BuscarPorPetAsync(int petId)
    {
        var vacinas = await _db.Vacinas
            .AsNoTracking()
            .Include(v => v.Pet)
            .Include(v => v.Veterinario)
            .Where(v => v.PetId == petId)
            .OrderByDescending(v => v.DataAplicacao)
            .ToListAsync();

        return vacinas.Select(MapToDto);
    }

    public async Task<IEnumerable<VacinaResponseDto>> BuscarDosesAtrasadasAsync()
    {
        var hoje = DateTime.Today;
        var vacinas = await _db.Vacinas
            .AsNoTracking()
            .Include(v => v.Pet)
            .Include(v => v.Veterinario)
            .Where(v => v.DataProximaDose != null && v.DataProximaDose < hoje)
            .OrderBy(v => v.DataProximaDose)
            .ToListAsync();

        return vacinas.Select(MapToDto);
    }

    public async Task<IEnumerable<VacinaResponseDto>> BuscarProximasDosesAsync(int diasAFrente)
    {
        if (diasAFrente < 1) diasAFrente = 30;
        var hoje = DateTime.Today;
        var limite = hoje.AddDays(diasAFrente);

        var vacinas = await _db.Vacinas
            .AsNoTracking()
            .Include(v => v.Pet)
            .Include(v => v.Veterinario)
            .Where(v => v.DataProximaDose != null
                        && v.DataProximaDose >= hoje
                        && v.DataProximaDose <= limite)
            .OrderBy(v => v.DataProximaDose)
            .ToListAsync();

        return vacinas.Select(MapToDto);
    }

    public async Task<VacinaResponseDto?> CriarAsync(CriarVacinaDto dto)
    {
        var petExiste = await _db.Pets.AnyAsync(p => p.Id == dto.PetId);
        if (!petExiste) return null;

        if (dto.VeterinarioId.HasValue)
        {
            var vetExiste = await _db.Veterinarios.AnyAsync(v => v.Id == dto.VeterinarioId.Value);
            if (!vetExiste) return null;
        }

        if (dto.DataAplicacao > DateTime.Today)
            throw new InvalidOperationException("Data de aplicação não pode ser futura.");

        var vacina = new Vacina
        {
            Nome = dto.Nome,
            DataAplicacao = dto.DataAplicacao,
            DataProximaDose = dto.DataProximaDose,
            Lote = dto.Lote,
            Fabricante = dto.Fabricante,
            Observacoes = dto.Observacoes,
            PetId = dto.PetId,
            VeterinarioId = dto.VeterinarioId,
            DataRegistro = DateTime.UtcNow
        };

        _db.Vacinas.Add(vacina);
        await _db.SaveChangesAsync();

        await _db.Entry(vacina).Reference(v => v.Pet).LoadAsync();
        if (vacina.VeterinarioId.HasValue)
            await _db.Entry(vacina).Reference(v => v.Veterinario).LoadAsync();

        return MapToDto(vacina);
    }

    public async Task<VacinaResponseDto?> AtualizarAsync(int id, AtualizarVacinaDto dto)
    {
        var vacina = await _db.Vacinas
            .Include(v => v.Pet)
            .Include(v => v.Veterinario)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vacina is null) return null;

        vacina.Nome = dto.Nome;
        vacina.DataAplicacao = dto.DataAplicacao;
        vacina.DataProximaDose = dto.DataProximaDose;
        vacina.Lote = dto.Lote;
        vacina.Fabricante = dto.Fabricante;
        vacina.Observacoes = dto.Observacoes;
        vacina.VeterinarioId = dto.VeterinarioId;

        await _db.SaveChangesAsync();
        return MapToDto(vacina);
    }

    public async Task<bool> RemoverAsync(int id)
    {
        var vacina = await _db.Vacinas.FirstOrDefaultAsync(v => v.Id == id);
        if (vacina is null) return false;

        _db.Vacinas.Remove(vacina);
        await _db.SaveChangesAsync();
        return true;
    }

    private static VacinaResponseDto MapToDto(Vacina v) => new()
    {
        Id = v.Id,
        Nome = v.Nome,
        DataAplicacao = v.DataAplicacao,
        DataProximaDose = v.DataProximaDose,
        Lote = v.Lote,
        Fabricante = v.Fabricante,
        Observacoes = v.Observacoes,
        DataRegistro = v.DataRegistro,
        PetId = v.PetId,
        PetNome = v.Pet?.Nome,
        VeterinarioId = v.VeterinarioId,
        VeterinarioNome = v.Veterinario?.Nome
    };
}
