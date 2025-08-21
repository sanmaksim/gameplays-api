using GameplaysApi.DTOs;
using GameplaysApi.Interfaces;
using GameplaysApi.Models;
using GameplaysApi.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace GameplaysApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IUsersRepository _usersRepository;

        public UsersController(
            IAuthService authService,
            IUserService userService,
            IUsersRepository usersRepository)
        {
            _authService = authService;
            _userService = userService;
            _usersRepository = usersRepository;
        }

        [HttpPost("register")]
        [SwaggerOperation(
            Summary = "Creates a new user",
            Description = "Adds a new user to the database, " +
            "creates their access & refresh tokens",
            OperationId = "Register"
        )]
        public async Task<IActionResult> CreateUser([FromBody] RegisterRequestDto registerRequestDto)
        {
            var validator = new RegisterRequestDtoValidator();
            var result = validator.Validate(registerRequestDto);
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                return BadRequest(ModelState);
            }

            var normalizedUsername = registerRequestDto.Username.ToLower();
            var user = await _usersRepository.GetUserByNameAsync(normalizedUsername);
            if (user != null)
            {
                return BadRequest(new { message = "Username already exists." });
            }

            var email = await _usersRepository.GetUserByEmailAsync(registerRequestDto.Email);
            if (email != null)
            {
                return BadRequest(new { message = "Email already exists." });
            }

            var newUser = new User
            {
                Username = registerRequestDto.Username,
                Password = _userService.HashPassword(registerRequestDto.Password),
                Email = registerRequestDto.Email
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
        [HttpGet("{id}")]
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
        [HttpPut("{id}")]
        [SwaggerOperation(
            Summary = "Update user",
            Description = "Updates user data based on ID",
            OperationId = "UpdateUser"
        )]
        public async Task<IActionResult> UpdateUser([FromBody] UserRequestDto userRequestDto)
        {
            var validator = new UserRequestDtoValidator();
            var result = validator.Validate(userRequestDto);
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                return BadRequest(ModelState);
            }

            // Does the user's JWT sub match the provided user ID
            var jwtUserId = _authService.GetCurrentUserId();
            if (string.IsNullOrEmpty(jwtUserId) || userRequestDto.UserId.ToString() != jwtUserId)
            {
                return Forbid();
            }

            // Does the user exist in the database
            var user = await _usersRepository.GetUserByIdAsync(userRequestDto.UserId);
            if (user == null)
            {
                return NotFound();
            }

            // Flag to update only the provided user properties
            bool hasChanges = false;

            // Is the provided username different to the original
            if (userRequestDto.Username != null && userRequestDto.Username != user.Username)
            {
                // Does the provided username match an existing user's
                var normalizedUsername = userRequestDto.Username.ToLower();
                var existingUser = await _usersRepository.GetUserByNameAsync(normalizedUsername);
                if (existingUser != null && userRequestDto.Username == existingUser.Username)
                {
                    return BadRequest(new { message = "The username already exists." });
                }

                // Update username to the new string
                user.Username = userRequestDto.Username;
                hasChanges = true;
            }

            // Is the provided email address different to the original
            if (userRequestDto.Email != null && userRequestDto.Email != user.Email)
            {
                // Does the provided email address match an existing user's
                var existingUser = await _usersRepository.GetUserByEmailAsync(userRequestDto.Email);
                if (existingUser != null && userRequestDto.Email == existingUser.Email)
                {
                    return BadRequest(new { message = "The email is already in use." });
                }

                // Update email to the new address
                user.Email = userRequestDto.Email;
                hasChanges = true;
            }

            // Has the user provided a password string
            if (!string.IsNullOrEmpty(userRequestDto.Password))
            {
                // Update password to the new string
                user.Password = _userService.HashPassword(userRequestDto.Password);
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
        [HttpDelete("{id}")]
        [SwaggerOperation(
            Summary = "Delete user",
            Description = "Deletes a user based on ID",
            OperationId = "DeleteUser"
        )]
        public async Task<IActionResult> DeleteUser(int id)
        {
            // Does the user's JWT sub match the provided user ID
            var jwtUserId = _authService.GetCurrentUserId();
            if (string.IsNullOrEmpty(jwtUserId) || id.ToString() != jwtUserId)
            {
                return Forbid();
            }

            // Does the user exist in the database
            var user = await _usersRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            // Delete the user and associated records
            await _usersRepository.DeleteUserAsync(user);

            // Delete the user access and refresh tokens
            _authService.DeleteAuthCookie(Response);
            _authService.DeleteRefreshTokenCookie(Response);

            return Ok(new { message = "User deleted." });
        }
    }
}
