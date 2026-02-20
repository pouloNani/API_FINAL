using System.Security.Claims;
using Api.DTOs;
using Api.DTOs.Account;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("account")]
public class AccountController(SignInManager<AppUser> signInManager, ITokenService tokenService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterDto registerDto)
    {
        var user = new AppUser
        {
            FirstName   = registerDto.firstName,
            LastName    = registerDto.lastName,
            PhoneNumber = registerDto.phoneNumber,
            Email       = registerDto.email,
            UserName    = registerDto.email,
            Address     = registerDto.address
        };

        var result = await signInManager.UserManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded) return BadRequest(result.Errors);

        var roleName = char.ToUpper(registerDto.role[0]) + registerDto.role.Substring(1).ToLower();
        var roleResult = await signInManager.UserManager.AddToRoleAsync(user, roleName);
        if (!roleResult.Succeeded) return BadRequest(roleResult.Errors);

        await signInManager.SignInAsync(user, isPersistent: false);
        var token = tokenService.CreateToken(user);

        return Ok(new
        {
            message = $"Utilisateur créé avec le rôle {roleName}.",
            token,
            user = new
            {
                id    = user.Id,
                email = user.Email,
                role  = roleName
            }
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        if (loginDto == null)                        return BadRequest(new { message = "Request body is null" });
        if (string.IsNullOrEmpty(loginDto.email))    return BadRequest(new { message = "Email is required" });
        if (string.IsNullOrEmpty(loginDto.password)) return BadRequest(new { message = "Password is required" });

        var user = await signInManager.UserManager.FindByEmailAsync(loginDto.email);
        if (user == null) return Unauthorized(new { message = "Email ou mot de passe incorrect" });

        var passwordValid = await signInManager.UserManager.CheckPasswordAsync(user, loginDto.password);
        if (!passwordValid) return Unauthorized(new { message = "Mot de passe incorrect" });

        await signInManager.SignInAsync(user, isPersistent: true);
        var token = tokenService.CreateToken(user);
        var roles = await signInManager.UserManager.GetRolesAsync(user);

        return Ok(new
        {
            message = "Connexion réussie !",
            token,
            user = new
            {
                id          = user.Id,
                email       = user.Email,
                firstName   = user.FirstName,
                lastName    = user.LastName,
                phoneNumber = user.PhoneNumber,
                roles
            }
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("get-user")]
    public async Task<IActionResult> GetUserByEmailOrId(GetUserDto dto)
    {
        if (string.IsNullOrEmpty(dto.UserId) && string.IsNullOrEmpty(dto.Email))
            return BadRequest(new { message = "Fournir un UserId ou un Email." });

        AppUser? user = null;
        if (!string.IsNullOrEmpty(dto.UserId))
            user = await signInManager.UserManager.FindByIdAsync(dto.UserId);
        else if (!string.IsNullOrEmpty(dto.Email))
            user = await signInManager.UserManager.FindByEmailAsync(dto.Email);

        if (user is null) return NotFound(new { message = "Utilisateur introuvable." });

        var roles = await signInManager.UserManager.GetRolesAsync(user);

        return Ok(new
        {
            user.Id, user.FirstName, user.LastName,
            user.Email, user.PhoneNumber,
            user.EmailConfirmed, user.LockoutEnabled, user.LockoutEnd,
            user.Address, Roles = roles
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("change-role")]
    public async Task<ActionResult> ChangeRole(ChangeRoleDto dto)
    {
        var user = await signInManager.UserManager.FindByEmailAsync(dto.Email);
        if (user == null) return NotFound("User not found");

        var currentRoles = await signInManager.UserManager.GetRolesAsync(user);
        var removeResult = await signInManager.UserManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded) return BadRequest(removeResult.Errors);

        var addResult = await signInManager.UserManager.AddToRoleAsync(user, dto.Role.ToString());
        if (!addResult.Succeeded) return BadRequest(addResult.Errors);

        return Ok("Role updated successfully");
    }

    [Authorize]
    [HttpPut("update-user")]
    public async Task<ActionResult> UpdateUser(UpdateUserDto dto)
    {
        string? emailToUpdate = User.IsInRole("Admin") && !string.IsNullOrEmpty(dto.TargetEmail)
            ? dto.TargetEmail
            : User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(emailToUpdate)) return BadRequest("Cannot determine user's email");

        var user = await signInManager.UserManager.FindByEmailAsync(emailToUpdate);
        if (user == null) return NotFound($"User with email {emailToUpdate} not found");

        if (!string.IsNullOrEmpty(dto.FirstName))   user.FirstName   = dto.FirstName;
        if (!string.IsNullOrEmpty(dto.LastName))    user.LastName    = dto.LastName;
        if (!string.IsNullOrEmpty(dto.PhoneNumber)) user.PhoneNumber = dto.PhoneNumber;
        if (dto.Address != null)                    user.Address     = dto.Address;
        if (!string.IsNullOrEmpty(dto.Email))       { user.Email = dto.Email; user.UserName = dto.Email; }

        var result = await signInManager.UserManager.UpdateAsync(user);
        if (!result.Succeeded) return BadRequest(result.Errors);

        return Ok("User updated successfully");
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return Ok("Logged out successfully");
    }

    [Authorize]
    [HttpGet("my-role")]
    public async Task<ActionResult> GetMyRole()
    {
        var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email)) return BadRequest("Cannot determine your email from token");

        var user = await signInManager.UserManager.FindByEmailAsync(email);
        if (user == null) return NotFound("User not found");

        var roles = await signInManager.UserManager.GetRolesAsync(user);
        return Ok(new { Email = email, Roles = roles });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("delete")]
    public async Task<ActionResult> Delete(DeleteUserDto dto)
    {
        AppUser? user = null;
        if (dto.UserId != 0)
            user = await signInManager.UserManager.FindByIdAsync(dto.UserId.ToString());
        else if (!string.IsNullOrEmpty(dto.Email))
            user = await signInManager.UserManager.FindByEmailAsync(dto.Email);

        if (user is null) return NotFound(new { message = "Utilisateur introuvable." });

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (user.Id.ToString() != currentUserId && !User.IsInRole("Admin")) return Forbid();

        await signInManager.SignOutAsync();
        var result = await signInManager.UserManager.DeleteAsync(user);
        if (!result.Succeeded) return BadRequest(new { message = "Échec de la suppression.", errors = result.Errors.Select(e => e.Description) });

        return Ok(new { message = "Utilisateur supprimé avec succès." });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("delete-all")]
    public async Task<IActionResult> DeleteAllUsers()
    {
        var users  = signInManager.UserManager.Users.ToList();
        var errors = new List<string>();

        foreach (var user in users)
        {
            var result = await signInManager.UserManager.DeleteAsync(user);
            if (!result.Succeeded) errors.AddRange(result.Errors.Select(e => e.Description));
        }

        await signInManager.SignOutAsync();

        if (errors.Any()) return BadRequest(new { message = "Certains utilisateurs n'ont pas pu être supprimés.", errors });
        return Ok(new { message = $"{users.Count} utilisateur(s) supprimé(s) avec succès." });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users  = signInManager.UserManager.Users.ToList();
        var result = new List<object>();

        foreach (var user in users)
        {
            var roles = await signInManager.UserManager.GetRolesAsync(user);
            result.Add(new
            {
                user.Id, user.UserName, user.Email, user.PhoneNumber,
                user.EmailConfirmed, user.LockoutEnabled, user.LockoutEnd,
                Roles = roles
            });
        }

        return Ok(result);
    }
}