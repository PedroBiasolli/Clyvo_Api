using Clyvo.PetCare.Api.Application.DTOs.Common;
using Clyvo.PetCare.Api.Application.DTOs.Tutor;
using Clyvo.PetCare.Api.Application.Services.Interfaces;
using Clyvo.PetCare.Api.Domain.Entities;
using Clyvo.PetCare.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Clyvo.PetCare.Api.Application.Services;

public class TutorService : ITutorService
{
    private readonly AppDbContext _db;

    public TutorService(AppDbContext db) => _db = db;

    public async Task<PagedResultDto<TutorResponseDto>> ListarAsync(string? nome, string? cidade, int pagina, int tamanhoPagina)
    {
        pagina = pagina < 1 ? 1 : pagina;
        tamanhoPagina = tamanhoPagina is < 1 or > 100 ? 20 : tamanhoPagina;

        var query = _db.Tutores.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(nome))
            query = query.Where(t => t.Nome.ToUpper().Contains(nome.ToUpper()));

        if (!string.IsNullOrWhiteSpace(cidade))
            query = query.Where(t => t.Cidade != null && t.Cidade.ToUpper() == cidade.ToUpper());

        var total = await query.CountAsync();

        var itens = await query
            .OrderBy(t => t.Nome)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .Select(t => MapToDto(t))
            .ToListAsync();

        return new PagedResultDto<TutorResponseDto>
        {
            Itens = itens,
            Pagina = pagina,
            TamanhoPagina = tamanhoPagina,
            TotalRegistros = total
        };
    }

    public async Task<TutorResponseDto?> BuscarPorIdAsync(int id)
    {
        var tutor = await _db.Tutores
            .AsNoTracking()
            .Include(t => t.Pets)
            .FirstOrDefaultAsync(t => t.Id == id);

        return tutor is null ? null : MapToDto(tutor);
    }

    public async Task<TutorResponseDto?> BuscarPorCpfAsync(string cpf)
    {
        var tutor = await _db.Tutores
            .AsNoTracking()
            .Include(t => t.Pets)
            .FirstOrDefaultAsync(t => t.Cpf == cpf);

        return tutor is null ? null : MapToDto(tutor);
    }

    public async Task<TutorResponseDto> CriarAsync(CriarTutorDto dto)
    {
        var jaExiste = await _db.Tutores.AnyAsync(t => t.Cpf == dto.Cpf);
        if (jaExiste)
            throw new InvalidOperationException($"Já existe um tutor cadastrado com o CPF {dto.Cpf}.");

        var tutor = new Tutor
        {
            Nome = dto.Nome,
            Cpf = dto.Cpf,
            Email = dto.Email,
            Telefone = dto.Telefone,
            Endereco = dto.Endereco,
            Cidade = dto.Cidade,
            Uf = dto.Uf?.ToUpper(),
            AceitaNotificacoes = dto.AceitaNotificacoes,
            DataCadastro = DateTime.UtcNow
        };

        _db.Tutores.Add(tutor);
        await _db.SaveChangesAsync();

        return MapToDto(tutor);
    }

    public async Task<TutorResponseDto?> AtualizarAsync(int id, AtualizarTutorDto dto)
    {
        var tutor = await _db.Tutores.FirstOrDefaultAsync(t => t.Id == id);
        if (tutor is null) return null;

        tutor.Nome = dto.Nome;
        tutor.Email = dto.Email;
        tutor.Telefone = dto.Telefone;
        tutor.Endereco = dto.Endereco;
        tutor.Cidade = dto.Cidade;
        tutor.Uf = dto.Uf?.ToUpper();
        tutor.AceitaNotificacoes = dto.AceitaNotificacoes;

        await _db.SaveChangesAsync();
        return MapToDto(tutor);
    }

    public async Task<bool> RemoverAsync(int id)
    {
        var tutor = await _db.Tutores
            .Include(t => t.Pets)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tutor is null) return false;

        if (tutor.Pets.Any())
            throw new InvalidOperationException("Não é possível remover um tutor que possui pets cadastrados.");

        _db.Tutores.Remove(tutor);
        await _db.SaveChangesAsync();
        return true;
    }

    private static TutorResponseDto MapToDto(Tutor t) => new()
    {
        Id = t.Id,
        Nome = t.Nome,
        Cpf = t.Cpf,
        Email = t.Email,
        Telefone = t.Telefone,
        Endereco = t.Endereco,
        Cidade = t.Cidade,
        Uf = t.Uf,
        DataCadastro = t.DataCadastro,
        AceitaNotificacoes = t.AceitaNotificacoes,
        QuantidadePets = t.Pets?.Count ?? 0
    };
}
