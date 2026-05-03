using DuoCareAPI.Data;
using DuoCareAPI.Dtos;
using DuoCareAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;

namespace DuoCareAPI.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class RecordsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RecordsController> _logger;

        public RecordsController(ApplicationDbContext context, ILogger<RecordsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RecordDto dto)
        {
            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Acceso no autorizado al crear registro");
                return Unauthorized();
            }

            try
            {
                var record = new Record
                {
                    Name = dto.Name,
                    Type = dto.Type,
                    Medication = dto.Medication,
                    MedicalData = dto.MedicalData,
                    Notes = dto.Notes,
                    ExtraDataJson = dto.ExtraDataJson ?? "[]",
                    UserId = userId
                };

                _context.Records.Add(record);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Registro médico creado para usuario {UserId} con ID {RecordId}", userId, record.Id);
                
                return Ok(record); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear registro");
                return StatusCode(500, "Error interno"); 
            }
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMine()
        {
            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var record = await _context.Records
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.Id)
                .FirstOrDefaultAsync();

            return Ok(record);
        }

        [HttpGet("me/all")]
        public async Task<IActionResult> GetMineAll()
        {
            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var records = await _context.Records
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            return Ok(records);
        }

        [HttpPut("{id}")]
public async Task<IActionResult> Update(int id, [FromBody] RecordDto dto)
{
    var userId = User.FindFirst("uid")?.Value;
    if (string.IsNullOrEmpty(userId)) return Unauthorized();

    var record = await _context.Records.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
    
    if (record == null) return NotFound();

    record.Name = dto.Name;
    record.Type = dto.Type ?? "Ambos";
    record.Medication = dto.Medication ?? "";
    record.MedicalData = dto.MedicalData ?? "";
    record.Notes = dto.Notes ?? "";
    record.ExtraDataJson = dto.ExtraDataJson ?? "[]";

    await _context.SaveChangesAsync();
    return Ok(record);
}

        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetByUser(string id)
        {
            var loggedUserId = User.FindFirst("uid")?.Value;

            if (!User.IsInRole("Administrator") && loggedUserId != id)
            {
                _logger.LogWarning("Acceso denegado a registros del usuario {UserId} por usuario {RequestedBy}", id, loggedUserId);
                return Forbid("No tienes permiso para ver este perfil.");
            }

            try
            {
                var record = await _context.Records
                    .FirstOrDefaultAsync(r => r.UserId == id);

                return Ok(record);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener registros del usuario {UserId}", id);
                return StatusCode(500, "Error al obtener los registros. Intenta más tarde.");
            }
        }
    }
}