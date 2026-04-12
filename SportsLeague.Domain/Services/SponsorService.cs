using Microsoft.Extensions.Logging;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Interfaces.Services;
using System.ComponentModel.DataAnnotations;

namespace SportsLeague.Domain.Services
{
    public class SponsorService : ISponsorService
    {
        private readonly ISponsorRepository _sponsorRepository;
        private readonly ITournamentSponsorRepository _tournamentSponsorRepository;
        private readonly ITournamentRepository _tournamentRepository;
        private readonly ILogger<SponsorService> _logger;

        public SponsorService(
            ISponsorRepository sponsorRepository,
            ITournamentSponsorRepository tournamentSponsorRepository,
            ITournamentRepository tournamentRepository,
            ILogger<SponsorService> logger)
        {
            _sponsorRepository = sponsorRepository;
            _tournamentSponsorRepository = tournamentSponsorRepository;
            _tournamentRepository = tournamentRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Sponsor>> GetAllAsync()
        {
            _logger.LogInformation("Retrieving all sponsors");
            return await _sponsorRepository.GetAllAsync();
        }

        public async Task<Sponsor?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving sponsor with ID: {SponsorId}", id);
            var sponsor = await _sponsorRepository.GetByIdWithTournamentAsync(id);
            if (sponsor == null)
                _logger.LogWarning("Sponsor with ID {SponsorId} not found", id);
            return sponsor;
        }

        public async Task<Sponsor> CreateAsync(Sponsor sponsor)
        {
            // Validación de negocio: nombre único
            var existingSponsor = await _sponsorRepository.GetByNameAsync(sponsor.Name);
            if (existingSponsor != null)
            {
                _logger.LogWarning("Sponsor with name '{SponsorName}' already exists", sponsor.Name);
                throw new InvalidOperationException(
                    $"Ya existe un patrocinador con el nombre '{sponsor.Name}'");
            }

            // Validacion de formato de email
            var validator = new EmailAddressAttribute();
            bool esValido = validator.IsValid(sponsor.ContactEmail);
            if (!esValido)
            {
                _logger.LogWarning("Sponsor with email '{SponsorEmail}' invalid", sponsor.ContactEmail);
                throw new InvalidOperationException(
                    $"El formato de correo '{sponsor.ContactEmail}' no es valido");
            }


            _logger.LogInformation("Creating sponsor: {SponsorName}", sponsor.Name);
            return await _sponsorRepository.CreateAsync(sponsor);
        }

        public async Task UpdateAsync(int id, Sponsor sponsor)
        {
            var existingSponsor = await _sponsorRepository.GetByIdAsync(id);
            if (existingSponsor == null)
                throw new KeyNotFoundException($"No se encontró el patrocinador con ID {id}");

            
            // Validar nombre único (si cambió)
            if (existingSponsor.Name != sponsor.Name)
            {
                var sponsorWithSameName = await _sponsorRepository.GetByNameAsync(sponsor.Name);
                if (sponsorWithSameName != null)
                {
                    throw new InvalidOperationException(
                        $"Ya existe un patrocinador con el nombre '{sponsor.Name}'");
                }
            }

            // Validacion de formato de email
            var validator = new EmailAddressAttribute();
            bool esValido = validator.IsValid(sponsor.ContactEmail);
            if (!esValido)
            {
                _logger.LogWarning("Sponsor with email '{SponsorEmail}' invalid", sponsor.ContactEmail);
                throw new InvalidOperationException(
                    $"El formato de correo '{sponsor.ContactEmail}' no es valido");
            }

            existingSponsor.Name = sponsor.Name;
            existingSponsor.ContactEmail = sponsor.ContactEmail;
            existingSponsor.Phone = sponsor.Phone;
            existingSponsor.WebsiteUrl = sponsor.WebsiteUrl;
            existingSponsor.Category = sponsor.Category;

            _logger.LogInformation("Updating sponsor with ID: {SponsorId}", id);
            await _sponsorRepository.UpdateAsync(existingSponsor);
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await _sponsorRepository.GetByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException($"No se encontró el patrocinador con ID {id}");

            _logger.LogInformation("Deleting sponsor with ID: {SponsorId}", id);
            await _sponsorRepository.DeleteAsync(id);
        }


        public async Task RegisterTournamentAsync(int tournamentId, int sponsorId, decimal contractAmount)
        {
            // Validar que el patrocinador existe
            var sponsor = await _sponsorRepository.ExistsAsync(sponsorId);
            if (!sponsor)
                throw new KeyNotFoundException(
                    $"No se encontró el patrocinador con ID {sponsorId}");

            // Validar que el torneo existe
            var tournamentExists = await _tournamentRepository.ExistsAsync(tournamentId);
            if (!tournamentExists)
                throw new KeyNotFoundException(
                    $"No se encontró el torneo con ID {tournamentId}");

            // Validar que no esté ya inscrito
            var existing = await _tournamentSponsorRepository
                .GetByTournamentAndSponsorAsync(tournamentId, sponsorId);
            if (existing != null)
            {
                throw new InvalidOperationException(
                    "Este torneo ya está patrocinado");
            }

            //Validar que el monto del contrato es mayor a 0
            if (contractAmount <= 0)
            {
                throw new InvalidOperationException(
                    "El monto del contrato debe ser mayor a 0");
            }

            var tournamentSponsor = new TournamentSponsor
            {
                SponsorId = sponsorId,
                TournamentId = tournamentId,
                ContractAmount = contractAmount,
                JoinedAt = DateTime.UtcNow
            };

            _logger.LogInformation(
                "Registering tournament {TournamentId} in sponsor {SponsorId}",
                tournamentId, sponsorId);
            await _tournamentSponsorRepository.CreateAsync(tournamentSponsor);
        }

        public async Task<IEnumerable<Tournament>> GetTournamentsBySponsorAsync(int sponsorId)
        {
            var sponsor = await _sponsorRepository.GetByIdAsync(sponsorId);
            if (sponsor == null)
                throw new KeyNotFoundException(
                    $"No se encontró el patrocinador con ID {sponsorId}");

            var tournamentsSponsor = await _tournamentSponsorRepository
                .GetBySponsorAsync(sponsorId);

            return tournamentsSponsor.Select(tt => tt.Tournament);
        }

        public async Task UnregisterTournamentAsync(int tournamentId, int sponsorId)
        {
            // Validar que el patrocinador existe
            var sponsor = await _sponsorRepository.ExistsAsync(sponsorId);
            if (!sponsor)
                throw new KeyNotFoundException(
                    $"No se encontró el patrocinador con ID {sponsorId}");

            // Validar que el torneo existe
            var tournamentExists = await _tournamentRepository.ExistsAsync(tournamentId);
            if (!tournamentExists)
                throw new KeyNotFoundException(
                    $"No se encontró el torneo con ID {tournamentId}");

            // Validar que si esté ya inscrito
            var existing = await _tournamentSponsorRepository
                .GetByTournamentAndSponsorAsync(tournamentId, sponsorId);
            if (existing == null)
            {
                throw new InvalidOperationException(
                    "Este torneo no está patrocinado");
            }

            _logger.LogInformation("Sponsor {SponsorId} and tournament {TournamentId} relationship removed", sponsorId, tournamentId);
            await _tournamentSponsorRepository.UnregisterTournamentAsync(tournamentId, sponsorId);
        }
    }
}
