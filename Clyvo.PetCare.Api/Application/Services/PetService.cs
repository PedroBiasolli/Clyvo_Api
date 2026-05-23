using Clyvo.PetCare.Api.Application.DTOs.Common;
using Clyvo.PetCare.Api.Application.DTOs.Pet;
using Clyvo.PetCare.Api.Application.Services.Interfaces;
using Clyvo.PetCare.Api.Domain.Entities;
using Clyvo.PetCare.Api.Domain.Enums;
using Clyvo.PetCare.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Clyvo.PetCare.Api.Application.Services;

public class PetService : IPetService
{
    private readonly AppDbContext _db;

    public PetService(AppDbContext db) => _db = db;

    public async Task<PagedResultDto<PetResponseDto>> ListarAsync(
        string? nome, string? raca, PorteCachorro? porte, int pagina, int tamanhoPagina)
    {
        pagina = pagina < 1 ? 1 : pagina;
        tamanhoPagina = tamanhoPagina is < 1 or > 100 ? 20 : tamanhoPagina;

        var query = _db.Pets.AsNoTracking().Include(p => p.Tutor).AsQueryable();

        if (!string.IsNullOrWhiteSpace(nome))
            query = query.Where(p => p.Nome.ToUpper().Contains(nome.ToUpper()));

        if (!string.IsNullOrWhiteSpace(raca))
            query = query.Where(p => p.Raca.ToUpper().Contains(raca.ToUpper()));

        if (porte.HasValue)
            query = query.Where(p => p.Porte == porte.Value);

        var total = await query.CountAsync();

        var itens = await query
            .OrderBy(p => p.Nome)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync();

        return new PagedResultDto<PetResponseDto>
        {
            Itens = itens.Select(MapToDto),
            Pagina = pagina,
            TamanhoPagina = tamanhoPagina,
            TotalRegistros = total
        };
    }

    public async Task<PetResponseDto?> BuscarPorIdAsync(int id)
    {
        var pet = await _db.Pets
            .AsNoTracking()
            .Include(p => p.Tutor)
            .FirstOrDefaultAsync(p => p.Id == id);

        return pet is null ? null : MapToDto(pet);
    }

    public async Task<IEnumerable<PetResponseDto>> BuscarPorTutorAsync(int tutorId)
    {
        var pets = await _db.Pets
            .AsNoTracking()
            .Include(p => p.Tutor)
            .Where(p => p.TutorId == tutorId)
            .OrderBy(p => p.Nome)
            .ToListAsync();

        return pets.Select(MapToDto);
    }

    public async Task<PetResponseDto?> BuscarPorMicrochipAsync(string microchip)
    {
        var pet = await _db.Pets
            .AsNoTracking()
            .Include(p => p.Tutor)
            .FirstOrDefaultAsync(p => p.Microchip == microchip);

        return pet is null ? null : MapToDto(pet);
    }

    public async Task<PetResponseDto?> CriarAsync(CriarPetDto dto)
    {
        var tutorExiste = await _db.Tutores.AnyAsync(t => t.Id == dto.TutorId);
        if (!tutorExiste) return null;

        if (dto.DataNascimento > DateTime.Today)
            throw new InvalidOperationException("Data de nascimento não pode ser futura.");

        var pet = new Pet
        {
            Nome = dto.Nome,
            Raca = dto.Raca,
            DataNascimento = dto.DataNascimento,
            Sexo = dto.Sexo,
            PesoKg = dto.PesoKg,
            Porte = dto.Porte,
            Microchip = dto.Microchip,
            Castrado = dto.Castrado,
            Observacoes = dto.Observacoes,
            TutorId = dto.TutorId,
            DataCadastro = DateTime.UtcNow
        };

        _db.Pets.Add(pet);
        await _db.SaveChangesAsync();

        await _db.Entry(pet).Reference(p => p.Tutor).LoadAsync();
        return MapToDto(pet);
    }

    public async Task<PetResponseDto?> AtualizarAsync(int id, AtualizarPetDto dto)
    {
        var pet = await _db.Pets.Include(p => p.Tutor).FirstOrDefaultAsync(p => p.Id == id);
        if (pet is null) return null;

        pet.Nome = dto.Nome;
        pet.Raca = dto.Raca;
        pet.PesoKg = dto.PesoKg;
        pet.Porte = dto.Porte;
        pet.Microchip = dto.Microchip;
        pet.Castrado = dto.Castrado;
        pet.Observacoes = dto.Observacoes;

        await _db.SaveChangesAsync();
        return MapToDto(pet);
    }

    public async Task<bool> RemoverAsync(int id)
    {
        var pet = await _db.Pets
            .Include(p => p.Consultas)
            .Include(p => p.Vacinas)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pet is null) return false;

        if (pet.Consultas.Any() || pet.Vacinas.Any())
            throw new InvalidOperationException("Não é possível remover um pet com histórico clínico (consultas ou vacinas).");

        _db.Pets.Remove(pet);
        await _db.SaveChangesAsync();
        return true;
    }

    private static PetResponseDto MapToDto(Pet p) => new()
    {
        Id = p.Id,
        Nome = p.Nome,
        Raca = p.Raca,
        DataNascimento = p.DataNascimento,
        IdadeAnos = p.IdadeAnos,
        Sexo = p.Sexo,
        PesoKg = p.PesoKg,
        Porte = p.Porte,
        Microchip = p.Microchip,
        Castrado = p.Castrado,
        Observacoes = p.Observacoes,
        DataCadastro = p.DataCadastro,
        TutorId = p.TutorId,
        TutorNome = p.Tutor?.Nome
    };
}
