using Clyvo.PetCare.Api.Application.DTOs.Common;
using Clyvo.PetCare.Api.Application.DTOs.Veterinario;
using Clyvo.PetCare.Api.Application.Services.Interfaces;
using Clyvo.PetCare.Api.Domain.Entities;
using Clyvo.PetCare.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Clyvo.PetCare.Api.Application.Services;

public class VeterinarioService : IVeterinarioService
{
    private readonly AppDbContext _db;

    public VeterinarioService(AppDbContext db) => _db = db;

    public async Task<PagedResultDto<VeterinarioResponseDto>> ListarAsync(string? especialidade, bool? ativo, int pagina, int tamanhoPagina)
    {
        pagina = pagina < 1 ? 1 : pagina;
        tamanhoPagina = tamanhoPagina is < 1 or > 100 ? 20 : tamanhoPagina;

        var query = _db.Veterinarios.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(especialidade))
            query = query.Where(v => v.Especialidade != null && v.Especialidade.ToUpper().Contains(especialidade.ToUpper()));

        if (ativo.HasValue)
            query = query.Where(v => v.Ativo == ativo.Value);

        var total = await query.CountAsync();

        // Materializa as entidades primeiro e só então mapeia para DTO —
        // EF Core não traduz chamada de método CLR para SQL.
        var entidades = await query
            .OrderBy(v => v.Nome)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync();

        return new PagedResultDto<VeterinarioResponseDto>
        {
            Itens = entidades.Select(MapToDto),
            Pagina = pagina,
            TamanhoPagina = tamanhoPagina,
            TotalRegistros = total
        };
    }

    public async Task<VeterinarioResponseDto?> BuscarPorIdAsync(int id)
    {
        var vet = await _db.Veterinarios.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);
        return vet is null ? null : MapToDto(vet);
    }

    public async Task<VeterinarioResponseDto?> BuscarPorCrmvAsync(string crmv)
    {
        var vet = await _db.Veterinarios.AsNoTracking().FirstOrDefaultAsync(v => v.Crmv == crmv);
        return vet is null ? null : MapToDto(vet);
    }

    public async Task<VeterinarioResponseDto> CriarAsync(CriarVeterinarioDto dto)
    {
        var jaExiste = await _db.Veterinarios.AnyAsync(v => v.Crmv == dto.Crmv);
        if (jaExiste)
            throw new InvalidOperationException($"Já existe um veterinário cadastrado com o CRMV {dto.Crmv}.");

        var vet = new Veterinario
        {
            Nome = dto.Nome,
            Crmv = dto.Crmv,
            Especialidade = dto.Especialidade,
            Email = dto.Email,
            Telefone = dto.Telefone,
            Ativo = true,
            DataCadastro = DateTime.UtcNow
        };

        _db.Veterinarios.Add(vet);
        await _db.SaveChangesAsync();

        return MapToDto(vet);
    }

    public async Task<VeterinarioResponseDto?> AtualizarAsync(int id, AtualizarVeterinarioDto dto)
    {
        var vet = await _db.Veterinarios.FirstOrDefaultAsync(v => v.Id == id);
        if (vet is null) return null;

        vet.Nome = dto.Nome;
        vet.Especialidade = dto.Especialidade;
        vet.Email = dto.Email;
        vet.Telefone = dto.Telefone;
        vet.Ativo = dto.Ativo;

        await _db.SaveChangesAsync();
        return MapToDto(vet);
    }

    public async Task<bool> RemoverAsync(int id)
    {
        var vet = await _db.Veterinarios
            .Include(v => v.Consultas)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vet is null) return false;

        if (vet.Consultas.Any())
            throw new InvalidOperationException("Não é possível remover um veterinário com consultas vinculadas. Marque-o como inativo.");

        _db.Veterinarios.Remove(vet);
        await _db.SaveChangesAsync();
        return true;
    }

    private static VeterinarioResponseDto MapToDto(Veterinario v) => new()
    {
        Id = v.Id,
        Nome = v.Nome,
        Crmv = v.Crmv,
        Especialidade = v.Especialidade,
        Email = v.Email,
        Telefone = v.Telefone,
        Ativo = v.Ativo,
        DataCadastro = v.DataCadastro
    };
}
