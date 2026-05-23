using Microsoft.Extensions.Logging;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Enums;
using SportsLeague.Domain.Helpers;
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.Domain.Services
{
    public class MatchLineupService : IMatchLineupService
    {
        private readonly IMatchRepository _matchRepository;
        private readonly IMatchLineupRepository _matchLineupRepository;
        private readonly IPlayerRepository _playerRepository;
        private readonly MatchValidationHelper _validationHelper;
        private readonly ILogger<MatchLineupService> _logger;

        public MatchLineupService(
            IMatchRepository matchRepository,
            IMatchLineupRepository matchLineupRepository,
            IPlayerRepository playerRepository,
            MatchValidationHelper validationHelper,
            ILogger<MatchLineupService> logger)
        {
            _matchRepository = matchRepository;
            _matchLineupRepository = matchLineupRepository;
            _playerRepository = playerRepository;
            _validationHelper = validationHelper;
            _logger = logger;
        }

        // ═══ MatchLineup ═══

        public async Task<MatchLineup> RegisterLineupAsync(
            int matchId, MatchLineup lineup)
        {
            var match = await _matchRepository.GetByIdAsync(matchId);
            if (match == null)
                throw new KeyNotFoundException(
                    $"No se encontró el partido con ID {matchId}");

            var player = await _validationHelper.ValidatePlayerInMatchAsync(lineup.PlayerId, match);

            var existingLineup = await _matchLineupRepository.ExistsByMatchAndPlayerAsync(matchId, lineup.PlayerId);

            if (existingLineup)
                throw new InvalidOperationException(
                    "El jugador ya está registrado en la alineación de este partido");

            var teamLineup = await _matchLineupRepository.GetByMatchAndTeamAsync(matchId, player.TeamId);

            var startersCount = teamLineup.Count(ml => ml.IsStarter);
            if (startersCount >= 11 && lineup.IsStarter)
                throw new InvalidOperationException(
                    "El equipo ya tiene 11 titulares registrados en este partido");

            if (match.Status != MatchStatus.Scheduled)
                throw new InvalidOperationException(
                    "Solo se pueden registrar alineaciones en partidos Scheduled");

            lineup.MatchId = matchId;

            _logger.LogInformation(
                "Registering lineup for match {MatchId}: {player}",
                matchId, lineup.PlayerId);
            return await _matchLineupRepository.CreateAsync(lineup);
        }

        public async Task<IEnumerable<MatchLineup>> GetLineupByMatchAsync(int matchId)
        {
            var match = await _matchRepository.GetByIdAsync(matchId);
            if (match == null)
                throw new KeyNotFoundException(
                    $"No se encontró el partido con ID {matchId}");
            return await _matchLineupRepository.GetByMatchIdAsync(matchId);
        }

        public async Task<IEnumerable<MatchLineup>> GetLineupByMatchAndTeamAsync(int matchId, int teamId)
        {
            var match = await _matchRepository.GetByIdAsync(matchId);
            if (match == null)
                throw new KeyNotFoundException(
                    $"No se encontró el partido con ID {matchId}");
            var team = await _validationHelper.ValidateTeamInMatchAsync(teamId, match);

            return await _matchLineupRepository.GetByMatchAndTeamAsync(matchId, teamId);
        }

        public async Task DeleteLineupAsync(int lineupId)
        {
            var exists = await _matchLineupRepository.ExistsAsync(lineupId);
            if (!exists)
                throw new KeyNotFoundException(
                    $"No se encontró la alineación con ID {lineupId}");
            await _matchLineupRepository.DeleteAsync(lineupId);
        }
    }
}
