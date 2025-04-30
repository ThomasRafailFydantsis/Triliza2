using Azure.Core;
using Azure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using triliza.Data;
using triliza.Models;

namespace triliza.Controllers
{
  
        [ApiController]
        [Route("api/[controller]")]
        public class AccountController : ControllerBase
        {
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly IConfiguration _configuration;
            private readonly RoleManager<IdentityRole> roleManager;
            private readonly ApplicationDbContext _context;

            public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
            {
                _userManager = userManager;
                _configuration = configuration;
                this.roleManager = roleManager;
                this._context = context;
            }
            [HttpGet("roles")]
            public async Task<IActionResult> GetUserRoles()
            {

                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    return Unauthorized("User not found.");
                }
                var roles = await _userManager.GetRolesAsync(user);

                return Ok(roles);
            }
            [HttpGet]
            public async Task<IActionResult> GetUsers()
            {
                var users = _userManager.Users.ToList();

                var userList = new List<object>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);

             

                    userList.Add(new
                    {
                        user.Id,
                        user.UserName,
                        user.Email,
                        user.FirstName,
                        user.LastName,
                        Roles = roles,
                     
                    });
                }

                return Ok(userList);
            }

            [HttpGet("get-user-profile/{userId}")]
            public async Task<IActionResult> GetUserProfile(string userId)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                var roles = await _userManager.GetRolesAsync(user);

                var userData = new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    //user.ProfileImagePath,
                    Roles = roles
                };

                return Ok(userData);
            }
            [HttpGet("me")]
            public async Task<IActionResult> GetCurrentUserData()
            {
                var token = Request.Cookies["authToken"];
                if (string.IsNullOrEmpty(token))
                {
                    return Unauthorized("User is not authenticated.");
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

                var claimsPrincipal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                }, out SecurityToken validatedToken);

                var userId = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User is not authenticated.");
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                var roles = await _userManager.GetRolesAsync(user);

                //if (user.PhoneNumber == null && user.Address1 == null)
                //{
                //    user.IsAuth = false;
                //}
                //else
                //{
                //    user.IsAuth = true;
                //}

                var userData = new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    user.PhoneNumber,
                    Roles = roles
                };

                return Ok(userData);
            }
            [HttpDelete("{id}")]
            public async Task<IActionResult> DeleteUser(string id)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return NotFound("User not found.");

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                    return BadRequest("Failed to delete user.");

                return Ok("User deleted successfully.");
            }
            [HttpPut("update-role")]
            public async Task<IActionResult> UpdateUserRole([FromBody] RoleAssignmentDto model)
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user == null)
                    return NotFound("User not found.");

                var currentRoles = await _userManager.GetRolesAsync(user);

                // Remove user from all current roles
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                    return BadRequest("Failed to remove current roles.");

                // Add user to the new role
                var addResult = await _userManager.AddToRoleAsync(user, model.Role);
                if (!addResult.Succeeded)
                    return BadRequest("Failed to assign new role.");

                return Ok($"User role updated to {model.Role}.");
            }


            [HttpPost("register")]
            public async Task<IActionResult> Register([FromBody] RegisterDto model)
            {
                if (model == null)
                {
                    return BadRequest("Invalid client request");
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    return BadRequest("Email is already in use");
                }

                var user = new ApplicationUser
                {
                    UserName = model.Username,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }


                var roleResult = await _userManager.AddToRoleAsync(user, "User");

                if (!roleResult.Succeeded)
                {
                    return BadRequest("Failed to assign default role to user.");
                }

                return Ok("User registered successfully.");
            }

            [HttpPost("login")]
            public async Task<IActionResult> Login([FromBody] LoginDto model)
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var user = await _userManager.FindByNameAsync(model.Username);
                if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                    return Unauthorized("Invalid username or password.");

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim("Email", user.Email ?? ""),
                    new Claim(ClaimTypes.Role, "Admin"),
                    new Claim(ClaimTypes.Role, "User"),
                    new Claim(ClaimTypes.Role, "Marker"),
                }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    Issuer = _configuration["Jwt:Issuer"],
                    Audience = _configuration["Jwt:Audience"],
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                HttpContext.Response.Cookies.Append("authToken", tokenString, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddHours(1)
                });


                return Ok(new { message = "Login successful." });
            }

            [HttpGet("auth-status")]
            public IActionResult GetAuthStatus()
            {
                var token = HttpContext.Request.Cookies["authToken"];
                if (string.IsNullOrEmpty(token))
                {
                    return Unauthorized("Token is missing");
                }

                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
                    handler.ValidateToken(token, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidIssuer = _configuration["Jwt:Issuer"],
                        ValidAudience = _configuration["Jwt:Audience"],
                        ValidateLifetime = true
                    }, out SecurityToken validatedToken);

                    return Ok(new { isAuthenticated = true });
                }
                catch (Exception)
                {
                    return Unauthorized(new { error = "User is not authenticated.", code = "AUTH_ERROR" });
                }
            }

            [HttpPost("logout")]
            public IActionResult Logout()
            {
                Response.Cookies.Delete("authToken");
                return Ok(new { message = "Logged out successfully." });
            }

            [HttpPut("{id}")]
            public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto model)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return NotFound("User not found.");

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.UserName = model.Username;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                    return BadRequest("Failed to update user.");

                return Ok("User updated successfully.");
            }
        }

        public class LoginDto
        {
            [Required(ErrorMessage = "Username is required.")]
            public string Username { get; set; }

            [Required(ErrorMessage = "Password is required.")]
            public string Password { get; set; }
        }

        public class RegisterDto
        {
            [Required(ErrorMessage = "Username is required.")]
            [StringLength(20, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 20 characters.")]
            public string Username { get; set; }

            [Required(ErrorMessage = "Email is required.")]
            [EmailAddress(ErrorMessage = "Invalid email format.")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Password is required.")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
            public string Password { get; set; }

            [Required(ErrorMessage = "First Name is required.")]
            [StringLength(50, ErrorMessage = "First Name cannot exceed 50 characters.")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "Last Name is required.")]
            [StringLength(50, ErrorMessage = "Last Name cannot exceed 50 characters.")]
            public string LastName { get; set; }
        }
        public class RoleAssignmentDto
        {
            [Required]
            public string UserId { get; set; }

            [Required]
            public string Role { get; set; }
        }
        public class UpdateUserDto
        {
            [Required]
            public string FirstName { get; set; }

            [Required]
            public string LastName { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            public string Username { get; set; }

            
        }

    
}
