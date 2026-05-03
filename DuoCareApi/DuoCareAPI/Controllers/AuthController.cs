using DuoCareAPI.Dtos;
using DuoCareAPI.Models;
using DuoCareAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using Asp.Versioning;

namespace DuoCareAPI.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly JwtService _jwtService;
        //  Mailtrap cambiado, maileroo basura
        private readonly MailtrapEmailService _email;

        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            JwtService jwtService,
            MailtrapEmailService email,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _email = email;
            _logger = logger;
        }

        // ================= REGISTER =================
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            try
            {
                if (dto.Password != dto.ConfirmPassword)
                    return BadRequest("Las contraseñas no coinciden.");

                var user = new User
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    FullName = dto.FullName
                };

                var result = await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                    return BadRequest(result.Errors);

                await _userManager.AddToRoleAsync(user, "User");

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var confirmationUrl =
                    $"{baseUrl}/api/auth/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

                // Envío de email con Mailtrap, aparecera en el sandbx
                await _email.SendEmailAsync(
                    user.Email!,
                    "Confirma tu cuenta",
                    $"Hola {user.FullName},<br><br>" +
                    $"Haz clic en el siguiente enlace para confirmar tu cuenta:<br>" +
                    $"<a href='{confirmationUrl}'>Confirmar cuenta</a><br><br>" +
                    $"Si no solicitaste esta cuenta, puedes ignorar este mensaje."
                );

                _logger.LogInformation("Usuario registrado: {Email}", user.Email);
                return Ok("Usuario registrado correctamente. Revisa tu correo para confirmar la cuenta.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar usuario");
                return StatusCode(500, ex.Message);
            }
        }

        // ================= CONFIRM EMAIL =================
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return BadRequest("Usuario no válido.");

                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (!result.Succeeded)
                    return BadRequest("Token no válido o expirado.");

                return Ok("Correo confirmado correctamente. Ya puedes iniciar sesión.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al confirmar email");
                return StatusCode(500, "Error al confirmar el email.");
            }
        }

        // ================= LOGIN =================
        [HttpPost("login")]
        [EnableRateLimiting("LoginRateLimit")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(dto.Email);
                if (user == null)
                    return Unauthorized("Credenciales incorrectas.");

                if (!user.EmailConfirmed)
                    return Unauthorized("Debes confirmar tu correo antes de iniciar sesión.");

                var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
                if (!result.Succeeded)
                    return Unauthorized("Credenciales incorrectas.");

                var jwt = await _jwtService.GenerateToken(user);

                return Ok(new
                {
                    token = jwt,
                    userId = user.Id,
                    email = user.Email,
                    expires = DateTime.Now.AddHours(2)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al hacer login");
                return StatusCode(500, "Error al iniciar sesión.");
            }
        }

        // ================= FORGOT PASSWORD =================
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(dto.Email);
                if (user == null)
                    return Ok("Si el correo existe, se enviará un enlace para restablecer la contraseña.");

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var resetUrl =
                    $"{baseUrl}/api/auth/reset-password?userId={user.Id}&token={Uri.EscapeDataString(token)}";

                // Envío de email con Mailtrap, aparecera en el sandbox
                await _email.SendEmailAsync(
                    user.Email!,
                    "Restablecer contraseña",
                    $"Hola {user.FullName},<br><br>" +
                    $"Haz clic aquí para restablecer tu contraseña:<br>" +
                    $"<a href='{resetUrl}'>Restablecer contraseña</a><br><br>"
                );

                return Ok("Si el correo existe, se enviará un enlace para restablecer la contraseña.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en forgot-password");
                return StatusCode(500, "Error al procesar la solicitud.");
            }
        }

        // ================= LOGOUT =================
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok("Sesión cerrada correctamente.");
        }

        // ================= CHANGE PASSWORD =================
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("Usuario no encontrado.");

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded) return BadRequest("La contraseña actual es incorrecta o la nueva no cumple los requisitos.");

            return Ok();
        }

        // ================= CHANGE NAME =================
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("change-name")]
        public async Task<IActionResult> ChangeName([FromBody] ChangeNameRequestDto dto)
        {
            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("Usuario no encontrado.");

            user.FullName = dto.NewName;
            await _userManager.UpdateAsync(user);

            return Ok(new { FullName = user.FullName });
        }

        // ================= CHANGE FOTO =================
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("change-photo")]
        public async Task<IActionResult> ChangePhoto([FromBody] ChangePhotoDto dto)
        {
            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("Usuario no encontrado.");

            user.ProfilePhotoBase64 = dto.Base64Image;
            await _userManager.UpdateAsync(user);

            return Ok();
        }
    }
}