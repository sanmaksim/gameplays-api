using GameplaysApi.DTOs;
using GameplaysApi.Interfaces;
using GameplaysApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace GameplaysApi.Controllers
{
    [ApiController]
    [Route("api/v1/users")]
    public class UsersV1Controller : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUsersRepository _usersRepository;

        public UsersV1Controller(
            IAuthService authService,
            IUsersRepository usersRepository)
        {
            _authService = authService;
            _usersRepository = usersRepository;
        }

        [HttpPost("register")]
        [SwaggerOperation(
            Summary = "Creates a new user",
            Description = "Adds a new user to the database, " +
            "creates their access & refresh tokens",
            OperationId = "Register"
        )]
        public async Task<IActionResult> CreateUser([FromBody] RegisterDto registerDto)
        {
            var user = await _usersRepository.GetUserByNameAsync(registerDto.Username);
            if (user != null)
            {
                return BadRequest(new { message = "Username already exists." });
            }

            var email = await _usersRepository.GetUserByEmailAsync(registerDto.Email);
            if (email != null)
            {
                return BadRequest(new { message = "Email already exists." });
            }

            var newUser = new User
            {
                Username = registerDto.Username,
                Password = registerDto.Password, // auto-hashed upon save in User model
                Email = registerDto.Email
            };

            try
            {
                await _usersRepository.AddUserAsync(newUser);
                _authService.CreateAuthCookie(newUser, Response);
                await _authService.CreateRefreshTokenCookie(newUser, Request, Response);
                return CreatedAtAction(nameof(GetUser),
                                        new { id = newUser.Id },
                                        new
                                        {
                                            id = newUser.Id,
                                            username = newUser.Username,
                                            email = newUser.Email
                                        });
            }
            catch (DbUpdateException)
            {
                return Conflict("There was an issue creating the record. Please reload and try again.");
            }
        }
        
        [Authorize]
        [HttpGet("profile/{id}")]
        [SwaggerOperation(
            Summary = "Get user",
            Description = "Retrieves a user based on ID",
            OperationId = "GetUser"
        )]
        public async Task<IActionResult> GetUser(int id)
        {
            var jwtUserId = _authService.GetCurrentUserId();
            if (string.IsNullOrEmpty(jwtUserId) || id.ToString() != jwtUserId)
            {
                return Forbid();
            }

            var user = await _usersRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok();
        }

        [Authorize]
        [HttpPut("profile/{id}")]
        [SwaggerOperation(
            Summary = "Update user",
            Description = "Updates user data based on ID",
            OperationId = "UpdateUser"
        )]
        public async Task<IActionResult> UpdateUser([FromBody] UserDto userDto)
        {
            // Does the user's JWT sub match the provided user ID
            var jwtUserId = _authService.GetCurrentUserId();
            if (string.IsNullOrEmpty(jwtUserId) || userDto.UserId.ToString() != jwtUserId)
            {
                return Forbid();
            }

            // Does the user exist in the database
            var user = await _usersRepository.GetUserByIdAsync(userDto.UserId);
            if (user == null)
            {
                return NotFound();
            }

            // Flag to update only the provided user properties
            bool hasChanges = false;

            // Is the provided username different to the original
            if (userDto.Username != null && userDto.Username != user.Username)
            {
                // Does the provided username match an existing user's
                var existingUser = await _usersRepository.GetUserByNameAsync(userDto.Username);
                if (existingUser != null && userDto.Username == existingUser.Username)
                {
                    return BadRequest(new { message = "The username already exists." });
                }

                // Update username to the new string
                user.Username = userDto.Username;
                hasChanges = true;
            }

            // Is the provided email address different to the original
            if (userDto.Email != null && userDto.Email != user.Email)
            {
                // Does the provided email address match an existing user's
                var existingUser = await _usersRepository.GetUserByEmailAsync(userDto.Email);
                if (existingUser != null && userDto.Email == existingUser.Email)
                {
                    return BadRequest(new { message = "The email is already in use." });
                }

                // Update email to the new address
                user.Email = userDto.Email;
                hasChanges = true;
            }

            // Has the user provided a password string
            if (!string.IsNullOrEmpty(userDto.Password))
            {
                // Update password to the new string
                user.Password = userDto.Password;
                hasChanges = true;
            }

            if (hasChanges)
            {
                try
                {
                    await _usersRepository.UpdateUserAsync(user);
                    return Ok(new
                    {
                        id = user.Id,
                        username = user.Username,
                        email = user.Email
                    });
                }
                catch (DbUpdateConcurrencyException)
                {
                    return Conflict("There was an issue saving the record. Please reload and try again.");
                }
            }

            return Ok(new { message = "No changes detected." });
        }

        [Authorize]
        [HttpDelete("profile")]
        [SwaggerOperation(
            Summary = "Delete user",
            Description = "Deletes a user based on their access token identifier",
            OperationId = "DeleteUser"
        )]
        public async Task<IActionResult> DeleteUser()
        {
            var jwtUserId = _authService.GetCurrentUserId();
            if (string.IsNullOrEmpty(jwtUserId))
            {
                return Forbid();
            }

            if (!int.TryParse(jwtUserId, out int userId))
            {
                return BadRequest(new { message = "The token string is not a valid integer." });
            }

            var user = await _usersRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            await _usersRepository.DeleteUserAsync(user);

            _authService.DeleteAuthCookie(Response);
            _authService.DeleteRefreshTokenCookie(Response);

            return Ok(new { message = "User deleted." });
        }
    }
}
