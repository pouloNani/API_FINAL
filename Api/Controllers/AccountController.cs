

using System.Security.Claims;
using Api.DTOs;
using Api.DTOs.Account;
using Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
namespace Api.Controllers;

[ApiController]
public class AccountController(SignInManager<AppUser> signInManager) : ControllerBase
{

    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterDto registerDto)
    {

        var user = new AppUser
        {
            FirstName = registerDto.firstName,
            LastName = registerDto.lastName,
            PhoneNumber = registerDto.phoneNumber,
            Email = registerDto.email,
            Address = registerDto.address
            
        };

        var result = await signInManager.UserManager.CreateAsync(user,registerDto.Password);
        
        if(!result.Succeeded) return BadRequest(result.Errors);

        if (!string.IsNullOrEmpty(registerDto.role.ToString()))
    {
        var roleResult = await signInManager.UserManager.AddToRoleAsync(user, registerDto.role.ToString());

        if (!roleResult.Succeeded)
            return BadRequest(roleResult.Errors);
    }

        return Ok();

    }

    [HttpPost("login")]
    public async Task<IActionResult> Login( LoginDto loginDto)
    {
        

        if (loginDto == null)
        {
            
            return BadRequest(new { message = "Request body is null" });
        }

        if (string.IsNullOrEmpty(loginDto.email))
        {
            
            return BadRequest(new { message = "Email is required" });
        }

        if (string.IsNullOrEmpty(loginDto.password))
        {
        
            return BadRequest(new { message = "Password is required" });
        }

    ;
        var user = await signInManager.UserManager.FindByEmailAsync(loginDto.email);
        
        if (user == null)
        {
        
            return Unauthorized(new 
            { 
                message = "Email ou mot de passe incorrect",
                email = loginDto.email
            });
        }


    
    var passwordValid = await signInManager.UserManager.CheckPasswordAsync(user, loginDto.password);
    

    if (!passwordValid)
    {
       
        return Unauthorized(new { message = "Mot de  passe incorrect" });
    }

    await signInManager.SignInAsync(user, isPersistent: true);

    var roles = await signInManager.UserManager.GetRolesAsync(user);

    return Ok(new 
    { 
        message = "Connexion réussie !",
        user = new
        {
            id = user.Id,
            email = user.Email,
            firstName = user.FirstName,
            lastName = user.LastName,
            phoneNumber = user.PhoneNumber,
            role = user.Role.ToString(),
            roles = roles
        }
    });
}

    [Authorize("Admin")]
    [HttpPut("change-role")]
    public async Task<ActionResult> ChangeRole(ChangeRoleDto dto)
    {
        


        var user = await signInManager.UserManager.FindByEmailAsync(dto.Email);

        if (user == null)
            return NotFound("User not found");

        
        var currentRoles = await signInManager.UserManager.GetRolesAsync(user);

        
        var removeResult = await signInManager.UserManager.RemoveFromRolesAsync(user, currentRoles);

        if (!removeResult.Succeeded)
            return BadRequest(removeResult.Errors);

        
        var addResult = await signInManager.UserManager.AddToRoleAsync(user, dto.Role.ToString());

        if (!addResult.Succeeded)
            return BadRequest(addResult.Errors);

        return Ok("Role updated successfully");
    }

    [Authorize]
    [HttpPut("update-user")]
    public async Task<ActionResult> UpdateUser( UpdateUserDto dto)
    {
        string? emailToUpdate;

     
        if (User.IsInRole("Admin") && !string.IsNullOrEmpty(dto.TargetEmail))
        {
            
            emailToUpdate = dto.TargetEmail;
        }
        else
        {
            
            emailToUpdate = User.Claims.FirstOrDefault(c => c.Type==ClaimTypes.Email)?.Value; // ou claim "email"
        }

        if (string.IsNullOrEmpty(emailToUpdate))
                return BadRequest("Cannot determine user's email");

        var user = await signInManager.UserManager.FindByEmailAsync(emailToUpdate);
        if (user == null)
            return NotFound($"User with email {emailToUpdate} not found");

        if (!string.IsNullOrEmpty(dto.FirstName)) user.FirstName = dto.FirstName;
        if (!string.IsNullOrEmpty(dto.LastName)) user.LastName = dto.LastName;
        if (!string.IsNullOrEmpty(dto.PhoneNumber)) user.PhoneNumber = dto.PhoneNumber;
        if (dto.Address !=null) user.Address = dto.Address;

        
        if (!string.IsNullOrEmpty(dto.Email))
        {
            user.Email = dto.Email;
            user.UserName = dto.Email; 
        }

        var result = await signInManager.UserManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

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
        // Récupérer l'email depuis le JWT
        var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(email))
            return BadRequest("Cannot determine your email from token");

        // Récupérer l'utilisateur
        var user = await signInManager.UserManager.FindByEmailAsync(email);
        if (user == null)
            return NotFound("User not found");

        // Récupérer les rôles
        var roles = await signInManager.UserManager.GetRolesAsync(user);

        return Ok(new
        {
            Email = email,
            Roles = roles
        });
    }


        }