using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Enums;
using SportsLeague.Domain.Interfaces.Repositories;

namespace SportsLeague.Domain.Helpers
{
    public class MatchValidationHelper
    {
        private readonly IMatchRepository _matchRepository;
        private readonly IPlayerRepository _playerRepository;
        private readonly ITeamRepository _teamRepository;

        public MatchValidationHelper(
            IMatchRepository matchRepository,
            IPlayerRepository playerRepository,
            ITeamRepository teamRepository)
        {
            _matchRepository = matchRepository;
            _playerRepository = playerRepository;
            _teamRepository = teamRepository;
        }

        public async Task<Match> ValidateMatchForEventAsync(int matchId)
        {
            var match = await _matchRepository.GetByIdAsync(matchId);
            if (match == null)
                throw new KeyNotFoundException(
                    $"No se encontró el partido con ID {matchId}");

            if (match.Status != MatchStatus.InProgress &&
                match.Status != MatchStatus.Finished)
                throw new InvalidOperationException(
                    "Solo se pueden registrar eventos en partidos InProgress o Finished");

            return match;
        }

        public async Task<Player> ValidatePlayerInMatchAsync(
            int playerId, Match match)
        {
            var player = await _playerRepository.GetByIdAsync(playerId);
            if (player == null)
                throw new KeyNotFoundException(
                    $"No se encontró el jugador con ID {playerId}");

            if (player.TeamId != match.HomeTeamId &&
                player.TeamId != match.AwayTeamId)
                throw new InvalidOperationException(
                    "El jugador no pertenece a ninguno de los equipos del partido");

            return player;
        }

        public static void ValidateMinute(int minute)
        {
            if (minute < 1 || minute > 120)
                throw new InvalidOperationException(
                    "El minuto debe estar entre 1 y 120");
        }

        public async Task<Team> ValidateTeamInMatchAsync(int teamId, Match match)
        {
            var team = await _teamRepository.GetByIdAsync(teamId);
            if (team == null)
                throw new KeyNotFoundException(
                    $"No se encontró el equipo con ID {teamId}");

            if (match.HomeTeamId != teamId && match.AwayTeamId != teamId)
                throw new InvalidOperationException(
                    "El equipo no pertenece al partido");

            return team;
        }
    }

}
