using GameplaysApi.Models;
using GameplaysApi.DTOs;
using GameplaysApi.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameplaysApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUsersRepository _usersRepository;

        public UsersController(
            IAuthService authService,
            IUsersRepository usersRepository)
        {
            _authService = authService;
            _usersRepository = usersRepository;
        }

        // @desc Register user/set token
        // route POST /api/users/register
        // @access Public
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
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

        
        // @desc Auth user/create auth cookie
        // route GET /api/users/login
        // @access Public
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthDto authDto)
        {
            var validator = new AuthDtoValidator();
            var result = validator.Validate(authDto);
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                return BadRequest(ModelState);
            }

            User? user;
            if (authDto.Username != null)
            {
                user = await _usersRepository.GetUserByNameAsync(authDto.Username);
            }
            else if (authDto.Email != null)
            {
                user = await _usersRepository.GetUserByEmailAsync(authDto.Email);
            }
            else
            {
                return BadRequest(new { message = "Please enter a username or email." });
            }
             
            if (user == null || authDto.Password != null && !user.VerifyPassword(authDto.Password, user.Password))
            {
                return Unauthorized(new { message = "Invalid username or password." });
                
            }

            _authService.CreateAuthCookie(user, Response);
            await _authService.CreateRefreshTokenCookie(user, Request, Response);

            return Ok(new
            {
                id = user.Id,
                username = user.Username,
                email = user.Email
            });
        }

        // @desc Delete auth cookie
        // route POST /api/users/logout
        // @access Private
        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            _authService.DeleteAuthCookie(Response);
            return Ok( new { message = "Logged out successfully." });
        }

        // @desc Get user
        // route GET /api/users/profile/{id}
        // @access Private
        [Authorize]
        [HttpGet("profile/{id}")]
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

        // @desc Update user
        // route PUT /api/users/profile/{id}
        // @access Private
        [Authorize]
        [HttpPut("profile/{id}")]
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

        // @desc Delete user
        // route DELETE /api/users/profile
        // @access Private
        [Authorize]
        [HttpDelete("profile")]
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

            return Ok(new { message = "User deleted." });
        }
    }
}
