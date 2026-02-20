using System;
using System.Security.Claims;
using AutoMapper;
using Core.DTOs.Schedule;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("schedules")]
public class ScheduleController(
    IScheduleRepository scheduleRepository,
    IShopRepository shopRepository,
    IMapper mapper) : BaseApiController
{
        private string? GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        private async Task<bool> CanAccessShop(int shopId)
        {
            var shop = await shopRepository.GetByIdAsync(shopId);
            if (shop is null) return false;
            if (User.IsInRole("Admin")) return true;
            return shop.OwnerId == GetCurrentUserId();
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ScheduleDto>>> GetSchedules(int shopId)
        {
            var shop = await shopRepository.GetByIdAsync(shopId);
            if (shop is null) return NotFound("Shop introuvable.");

            var schedules = await scheduleRepository.GetByShopIdAsync(shopId);
            return Ok(mapper.Map<IReadOnlyList<ScheduleDto>>(schedules));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<ActionResult> CreateSchedules(int shopId, List<CreateScheduleDto> dtos)
        {
            if (!await CanAccessShop(shopId)) return Forbid();

            if (dtos.Count == 0)
                return BadRequest("Vous devez fournir au moins un jour.");

            var existingSchedules = await scheduleRepository.GetByShopIdAsync(shopId);
            if (existingSchedules.Any())
                return BadRequest("Les horaires existent déjà. Utilisez PUT pour les modifier.");

            foreach (var dto in dtos)
            {
                if (!dto.IsClosed && dto.OpenTime >= dto.CloseTime)
                    return BadRequest($"Heure invalide pour le {dto.Day}.");

                var schedule = mapper.Map<Schedule>(dto);
                schedule.ShopId = shopId;
                await scheduleRepository.AddAsync(schedule);
            }

            await scheduleRepository.SaveChangesAsync();
            return Ok(new { message = "Horaires créés avec succès." });
        }
    [HttpPut]
    [Authorize(Roles = "Admin,Owner")]
    public async Task<ActionResult> UpdateSchedules(int shopId, List<UpdateScheduleDto> dtos)
    {
        if (!await CanAccessShop(shopId)) return Forbid();

        if (dtos.Count == 0)
            return BadRequest("Vous devez fournir au moins un jour.");

        var schedules = await scheduleRepository.GetByShopIdAsync(shopId);
        if (!schedules.Any())
            return NotFound("Aucun horaire trouvé. Utilisez POST pour les créer.");

        foreach (var dto in dtos)
        {
            var schedule = schedules.FirstOrDefault(s => s.Day == dto.Day);
            if (schedule is null) continue;

            if (!dto.IsClosed && dto.OpenTime >= dto.CloseTime)
                return BadRequest($"Heure invalide pour le {dto.Day}.");

            mapper.Map(dto, schedule);
            schedule.UpdatedAt = DateTime.UtcNow;
            await scheduleRepository.UpdateAsync(schedule);
        }

        await scheduleRepository.SaveChangesAsync();
        return Ok(new { message = "Horaires mis à jour avec succès." });
    }
}