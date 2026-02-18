using Api.Controllers;
using AutoMapper;
using Core.DTOs.Shop;
using Core.Entities;
using Core.Helpers;
using Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace Tests;

public class ShopControllerTests
{
   
    private readonly Mock<IShopRepository> _shopRepo;
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<SignInManager<AppUser>> _signInManager;
    private readonly Mock<UserManager<AppUser>> _userManager;
    private readonly ShopController _controller;

    public ShopControllerTests()
    {
        _shopRepo = new Mock<IShopRepository>();
        _mapper = new Mock<IMapper>();

        _userManager = new Mock<UserManager<AppUser>>(
            Mock.Of<IUserStore<AppUser>>(), null, null, null, null, null, null, null, null);

        _signInManager = new Mock<SignInManager<AppUser>>(
            _userManager.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<AppUser>>(),
            null, null, null, null);

        _controller = new ShopController(
            _shopRepo.Object, 
            _mapper.Object, 
            _signInManager.Object,
            _userManager.Object);
    }


    // ── Helpers ──────────────────────────────────────────

    private void SetUser(string userId, string role)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "Test");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };
    }

    // ── GetAllShops ───────────────────────────────────────

    [Fact]
    public async Task GetAllShops_ReturnsOk()
    {
        _shopRepo.Setup(x => x.GetShopsAsync(It.IsAny<ShopParams>()))
            .ReturnsAsync(new PagedResult<ShopDto>());

        var result = await _controller.GetAllShops(new ShopParams());

        Assert.IsType<OkObjectResult>(result.Result);
    }

    // ── GetMyShops ────────────────────────────────────────

    [Fact]
    public async Task GetMyShops_ReturnsUnauthorized_WhenNoUser()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
        };

        var result = await _controller.GetMyShops(new PaginationParams());

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetMyShops_ReturnsOk_WhenOwner()
    {
        SetUser("owner-123", "Owner");
        _shopRepo.Setup(x => x.GetByOwnerIdAsync("owner-123", It.IsAny<PaginationParams>()))
            .ReturnsAsync(new PagedResult<ShopDto>());

        var result = await _controller.GetMyShops(new PaginationParams());

        Assert.IsType<OkObjectResult>(result.Result);
    }

    // ── CreateShop ────────────────────────────────────────

    [Fact]
    public async Task CreateShop_ReturnsUnauthorized_WhenNoUser()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
        };

        var result = await _controller.CreateShop(new CreateShopDto());

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateShop_ReturnsCreated_WhenOwner()
    {
        SetUser("owner-123", "Owner");
        var shop = new Shop { Id = 1 };
        var shopDto = new ShopDto { Id = 1 };

        _mapper.Setup(x => x.Map<Shop>(It.IsAny<CreateShopDto>())).Returns(shop);
        _mapper.Setup(x => x.Map<ShopDto>(shop)).Returns(shopDto);
        _shopRepo.Setup(x => x.AddAsync(shop)).ReturnsAsync(shop);
        _shopRepo.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _controller.CreateShop(new CreateShopDto());

        Assert.IsType<CreatedAtActionResult>(result.Result);
    }

    // ── CreateShopForOwner ────────────────────────────────

    [Fact]
    public async Task CreateShopForOwner_ReturnsNotFound_WhenOwnerDoesNotExist()
    {
        SetUser("admin-123", "Admin");
        _userManager.Setup(x => x.FindByIdAsync("unknown-id"))
            .ReturnsAsync((AppUser)null!);

        var result = await _controller.CreateShopForOwner(
            new CreateShopForOwnerDto { OwnerId = "unknown-id" });

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateShopForOwner_ReturnsBadRequest_WhenUserIsNotOwner()
    {
        SetUser("admin-123", "Admin");
        var user = new AppUser { Id = "user-123" };

        _userManager.Setup(x => x.FindByIdAsync("user-123")).ReturnsAsync(user);
        _userManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Client" });

        var result = await _controller.CreateShopForOwner(
            new CreateShopForOwnerDto { OwnerId = "user-123" });

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateShopForOwner_ReturnsCreated_WhenValid()
    {
        SetUser("admin-123", "Admin");
        var user = new AppUser { Id = "owner-123" };
        var shop = new Shop { Id = 1 };
        var shopDto = new ShopDto { Id = 1 };

        _userManager.Setup(x => x.FindByIdAsync("owner-123")).ReturnsAsync(user);
        _userManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Owner" });
        _mapper.Setup(x => x.Map<Shop>(It.IsAny<CreateShopForOwnerDto>())).Returns(shop);
        _mapper.Setup(x => x.Map<ShopDto>(shop)).Returns(shopDto);
        _shopRepo.Setup(x => x.AddAsync(shop)).ReturnsAsync(shop);
        _shopRepo.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _controller.CreateShopForOwner(
            new CreateShopForOwnerDto { OwnerId = "owner-123" });

        Assert.IsType<CreatedAtActionResult>(result.Result);
    }

    // ── DeleteShop ────────────────────────────────────────

    [Fact]
    public async Task DeleteShop_ReturnsNotFound_WhenShopDoesNotExist()
    {
        SetUser("owner-123", "Owner");
        _shopRepo.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((Shop)null!);

        var result = await _controller.DeleteShop(99, null);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task DeleteShop_ReturnsForbid_WhenNotOwner()
    {
        SetUser("other-user", "Owner");
        var shop = new Shop { Id = 1, OwnerId = "real-owner" };
        _shopRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(shop);

        var result = await _controller.DeleteShop(1, null);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task DeleteShop_ReturnsOk_WhenOwnerDeletesOwnShop()
    {
        SetUser("owner-123", "Owner");
        var shop = new Shop { Id = 1, OwnerId = "owner-123" };
        _shopRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(shop);
        _shopRepo.Setup(x => x.DeleteAsync(shop)).Returns(Task.CompletedTask);
        _shopRepo.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _controller.DeleteShop(1, null);

        Assert.IsType<OkObjectResult>(result);
    }
}