using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SportsLeague.API.DTOs.Request;
using SportsLeague.API.DTOs.Response;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Services;
using SportsLeague.Domain.Services;

namespace SportsLeague.API.Controllers
{
    [ApiController]
    [Route("api/match/{matchId}")]
    public class MatchLineupController : ControllerBase
    {
        private readonly IMatchLineupService _matchLineupService;
        private readonly IMapper _mapper;

        public MatchLineupController(
            IMatchLineupService matchLineupService, IMapper mapper)
        {
            _matchLineupService = matchLineupService;
            _mapper = mapper;
        }

        [HttpPost("lineup")]
        public async Task<ActionResult<MatchLineupResponseDTO>> RegisterLineup(
            int matchId, MatchLineupRequestDTO dto)
        {
            try
            {
                var result = _mapper.Map<MatchLineup>(dto);
                var created = await _matchLineupService.RegisterLineupAsync(matchId, result);
                return Ok(_mapper.Map<MatchLineupResponseDTO>(created));
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        }

        [HttpGet("lineup")]
        public async Task<ActionResult<IEnumerable<MatchLineupResponseDTO>>> GetLineup(
            int matchId)
        {
            try
            {
                var result = await _matchLineupService.GetLineupByMatchAsync(matchId);
                if (result == null)
                    return NotFound(new { message = "Este partido aún no tiene alineacion registrada" });
                return Ok(result.Select(_mapper.Map<MatchLineupResponseDTO>));
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpGet("lineup/team/{teamId}")]
        public async Task<ActionResult<IEnumerable<MatchLineupResponseDTO>>> GetLineupByTeam(
            int matchId, int teamId)
        {
            try
            {
                var result = await _matchLineupService.GetLineupByMatchAndTeamAsync(matchId, teamId);
                if (result == null)
                    return NotFound(new { message = "Este equipo aún no tiene alineacion registrada para este partido" });
                return Ok(result.Select(_mapper.Map<MatchLineupResponseDTO>));
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        }

        [HttpDelete("lineup/{id}")]
        public async Task<ActionResult> DeleteLineup(int id)
        {
            try
            {
                await _matchLineupService.DeleteLineupAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }
    }
}
