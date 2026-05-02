using DuoCareAPI.Data;
using DuoCareAPI.Dtos;
using DuoCareAPI.Models;
using DuoCareAPI.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DuoCareAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AppointmentsController> _logger;

        public AppointmentsController(ApplicationDbContext context, ILogger<AppointmentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // =========================
        // CREATE
        // =========================
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AppointmentDto dto)
        {
            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (dto == null)
                return BadRequest("Datos de la cita inválidos.");

            if (userId == dto.ReceiverId)
                return BadRequest("El emisor y el receptor no pueden ser el mismo.");

            if (dto.Date <= DateTime.Now)
                return BadRequest("La cita no puede estar en el pasado.");

            if (dto.Latitude < -90 || dto.Latitude > 90 ||
                dto.Longitude < -180 || dto.Longitude > 180)
                return BadRequest("Coordenadas inválidas.");

            var receiver = await _context.Users.FindAsync(dto.ReceiverId);
            if (receiver == null)
                return BadRequest("El usuario receptor no existe.");

            var conflict = await _context.Appointments.AnyAsync(a =>
                (a.SenderId == userId || a.ReceiverId == userId ||
                 a.SenderId == dto.ReceiverId || a.ReceiverId == dto.ReceiverId) &&
                a.Date == dto.Date &&
                a.Status != AppointmentStatus.Rechazada);

            if (conflict)
                return BadRequest("Ya existe una cita en esa fecha.");

            var appointment = new Appointment
            {
                Date = dto.Date,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                SenderId = userId,
                ReceiverId = dto.ReceiverId,
                Status = AppointmentStatus.Pendiente,
                CreatedBy = userId
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return Ok(MapToDto(appointment));
        }

        // =========================
        // GET BY ID
        // =========================
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id);
            if (appointment == null)
                return NotFound();

            if (appointment.SenderId != userId &&
                appointment.ReceiverId != userId &&
                !User.IsInRole("Administrator"))
            {
                return Forbid();
            }

            if (appointment.ReceiverId == userId &&
                appointment.Status == AppointmentStatus.Pendiente)
            {
                appointment.Status = AppointmentStatus.Leido;
                await _context.SaveChangesAsync();
            }

            return Ok(MapToDto(appointment));
        }

        // =========================
        // GET MY APPOINTMENTS
        // =========================
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMine(int page = 1, int pageSize = 10)
        {
            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 50) pageSize = 50;

            var query = _context.Appointments
                .Where(a => a.SenderId == userId || a.ReceiverId == userId)
                .OrderByDescending(a => a.Date);

            var total = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            foreach (var a in items)
            {
                if (a.ReceiverId == userId && a.Status == AppointmentStatus.Pendiente)
                    a.Status = AppointmentStatus.Leido;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                total,
                page,
                pageSize,
                data = items.Select(MapToDto)
            });
        }

        // =========================
        // ACCEPT
        // =========================
        [Authorize]
        [HttpPut("{id}/accept")]
        public async Task<IActionResult> Accept(int id)
        {
            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound();

            if (appointment.ReceiverId != userId)
                return Forbid();

            appointment.Status = AppointmentStatus.Aceptado;
            await _context.SaveChangesAsync();

            return Ok(MapToDto(appointment));
        }

        // =========================
        // REJECT
        // =========================
        [Authorize]
        [HttpPut("{id}/reject")]
        public async Task<IActionResult> Reject(int id)
        {
            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound();

            if (appointment.ReceiverId != userId)
                return Forbid();

            appointment.Status = AppointmentStatus.Rechazada;
            appointment.RejectedByUserId = userId;
            appointment.RejectedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(MapToDto(appointment));
        }

        // =========================
        // COMPLETE
        // =========================
        [Authorize]
        [HttpPut("{id}/complete")]
        public async Task<IActionResult> Complete(int id)
        {
            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound();

            if (appointment.SenderId != userId && appointment.ReceiverId != userId)
                return Forbid();

            if (appointment.Status != AppointmentStatus.Aceptado)
                return BadRequest("Solo citas aceptadas.");

            appointment.Status = AppointmentStatus.Completado;
            await _context.SaveChangesAsync();

            return Ok(MapToDto(appointment));
        }

        // =========================
        // MAPPER
        // =========================
        private static AppointmentResponseDto MapToDto(Appointment a)
        {
            return new AppointmentResponseDto
            {
                Id = a.Id,
                Date = a.Date,
                Status = a.Status.ToString(),
                Latitude = a.Latitude,
                Longitude = a.Longitude,
                SenderId = a.SenderId,
                ReceiverId = a.ReceiverId
            };
        }
    }
}