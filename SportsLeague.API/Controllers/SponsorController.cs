using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SportsLeague.API.DTOs.Request;
using SportsLeague.API.DTOs.Response;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SponsorController : ControllerBase
    {
        private readonly ISponsorService _SponsorService;
        private readonly IMapper _mapper;

        public SponsorController(
            ISponsorService SponsorService,
            IMapper mapper)
        {
            _SponsorService = SponsorService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SponsorResponseDTO>>> GetAll()
        {
            var Sponsors = await _SponsorService.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<SponsorResponseDTO>>(Sponsors));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SponsorResponseDTO>> GetById(int id)
        {
            var Sponsor = await _SponsorService.GetByIdAsync(id);
            if (Sponsor == null)
                return NotFound(new { message = $"Patrocinador con ID {id} no encontrado" });
            return Ok(_mapper.Map<SponsorResponseDTO>(Sponsor));
        }

        [HttpPost]
        public async Task<ActionResult<SponsorResponseDTO>> Create(SponsorRequestDTO dto)
        {
            try
            {
                var Sponsor = _mapper.Map<Sponsor>(dto);
                var created = await _SponsorService.CreateAsync(Sponsor);
                var responseDto = _mapper.Map<SponsorResponseDTO>(created);
                return CreatedAtAction(nameof(GetById), new { id = responseDto.Id }, responseDto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, SponsorRequestDTO dto)
        {
            try
            {
                var Sponsor = _mapper.Map<Sponsor>(dto);
                await _SponsorService.UpdateAsync(id, Sponsor);
                return NoContent();
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                await _SponsorService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        }

        [HttpPost("{id}/tournament")]
        public async Task<ActionResult> RegisterTournament(TournamentSponsorRequestDTO dto, int id)
        {
            try
            {
                await _SponsorService.RegisterTournamentAsync(dto.TournamentId, id, dto.ContractAmount);
                return Ok(new { message = "Torneo patrocinado exitosamente" });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        }

        [HttpGet("{id}/tournament")]
        public async Task<ActionResult<IEnumerable<TournamentResponseDTO>>> GetTournaments(int id)
        {
            try
            {
                var tournaments = await _SponsorService.GetTournamentsBySponsorAsync(id);
                return Ok(_mapper.Map<IEnumerable<TournamentResponseDTO>>(tournaments));
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpDelete("{id}/tournament")]
        public async Task<ActionResult> UnregisterTournament(DeleteTournamentSponsorRequestDTO dto, int id)
        {
            try
            {
                await _SponsorService.UnregisterTournamentAsync(dto.TournamentId, id);
                return Ok(new { message = "Torneo despatrocinado exitosamente" });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }

        }
    }
}