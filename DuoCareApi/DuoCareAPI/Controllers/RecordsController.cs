using DuoCareAPI.Data;
using DuoCareAPI.Dtos;
using DuoCareAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DuoCareAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // ✅ aplica a todos los endpoints
    public class RecordsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RecordsController> _logger;

        public RecordsController(ApplicationDbContext context, ILogger<RecordsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ✅ POST api/records
        // Crea un registro médico del usuario (niño o mascota)
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
                    ExtraDataJson = dto.ExtraDataJson ?? "",
                    UserId = userId
                };

                _context.Records.Add(record);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Registro médico creado: {RecordId} para usuario {UserId}", record.Id, userId);
                return Ok(record);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error de base de datos al crear registro para usuario {UserId}", userId);
                return StatusCode(500, "Error al crear el registro. Intenta más tarde.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear registro para usuario {UserId}", userId);
                return StatusCode(500, "Error inesperado. Intenta más tarde.");
            }
        }

        // ✅ GET api/records/me
        // Devuelve el primer registro del usuario autenticado (si solo manejas 1)
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

        // ✅ GET api/records/me/all
        // Devuelve lista de registros del usuario autenticado
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

        // ✅ Mantengo tu endpoint existente (admin o dueño)
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