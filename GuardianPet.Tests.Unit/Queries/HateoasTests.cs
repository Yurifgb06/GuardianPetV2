using GuardianPet.Controllers;
using GuardianPet.DTOs.Query;
using GuardianPet.DTOs.Response;
using GuardianPet.Presentation;
using GuardianPet.Repositories;
using GuardianPet.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Identity;
using GuardianPet.Models;
using Moq;
using Xunit;

namespace GuardianPet.Tests.Unit.Queries;

public sealed class HateoasTests
{
    private static PetsController Controller()
    {
        var controller = new PetsController(Mock.Of<IPetService>());
        var context = new DefaultHttpContext();
        context.Request.QueryString = new QueryString("?species=Dog%20%26%20Cat&petSize=SMALL&sortBy=name&sortDirection=desc&page=2&pageSize=2&includeLinks=true");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = context,
            RouteData = new RouteData(),
            ActionDescriptor = new ControllerActionDescriptor { ControllerName = "Pets", ActionName = "FindAll" }
        };
        var url = new Mock<IUrlHelper>();
        url.SetupGet(x => x.ActionContext).Returns(controller.ControllerContext);
        url.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns("https://example.test/guardian/api/pets");
        controller.Url = url.Object;
        return controller;
    }

    [Theory]
    [InlineData(1, 5, "self,first,next,last")]
    [InlineData(2, 5, "self,first,prev,next,last")]
    [InlineData(3, 5, "self,first,prev,last")]
    [InlineData(1, 0, "self,first,last")]
    [InlineData(4, 5, "self,first,prev,last")]
    public async Task Pagination_CountsOnceAndBuildsApplicableLinks(int page, long total, string relations)
    {
        var controller = Controller();
        var calls = 0;
        var response = await Hateoas.CollectionAsync(controller, new List<PetResponseDTO>(), new PetQueryDTO { Page = page, PageSize = 2 },
            x => x.Id, () => { calls++; return Task.FromResult(total); });
        Assert.Equal(1, calls);
        Assert.Equal(relations.Split(','), response.Links.Select(x => x.Rel));
        foreach (var link in response.Links)
        {
            var query = QueryHelpers.ParseQuery(new Uri(link.Href).Query);
            Assert.Equal("Dog & Cat", query["species"].ToString());
            Assert.Equal("SMALL", query["petSize"].ToString());
            Assert.Equal("name", query["sortBy"].ToString());
            Assert.Equal("desc", query["sortDirection"].ToString());
            Assert.Equal("true", query["includeLinks"].ToString());
            Assert.Equal("2", query["pageSize"].ToString());
            Assert.Equal("GET", link.Method);
        }
    }

    [Fact]
    public async Task UnpagedHateoas_DoesNotCount()
    {
        var response = await Hateoas.CollectionAsync(Controller(), new List<PetResponseDTO> { new() { Id = 1 } }, new PetQueryDTO(),
            x => x.Id, () => throw new InvalidOperationException("Count must not run"));
        Assert.Equal("self", Assert.Single(response.Links).Rel);
        Assert.Equal(1, Assert.Single(response.Items).Data.Id);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Pet_LegacyPagedCollection_DoesNotCount(bool explicitFalse)
    {
        var service = new Mock<IPetService>(MockBehavior.Strict);
        var options = new PetQueryDTO { Page = 1, PageSize = 2 };
        var items = new List<PetResponseDTO> { new() { Id = 1 } };
        service.Setup(x => x.QueryAsync(options)).ReturnsAsync(items);
        var controller = new PetsController(service.Object);
        var result = explicitFalse ? await controller.FindAll(options, false) : await controller.FindAll(options);
        Assert.Same(items, Assert.IsType<OkObjectResult>(result).Value);
        service.Verify(x => x.QueryAsync(options), Times.Once);
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Pet_Count_DelegatesOnlyToCount()
    {
        var repository = new Mock<IPetRepository>(MockBehavior.Strict);
        var options = new PetQueryDTO { Page = 2, PageSize = 2 };
        repository.Setup(x => x.CountAsync(options)).ReturnsAsync(5);
        var total = await new PetService(repository.Object).CountAsync(options);
        Assert.Equal(5, total);
        repository.Verify(x => x.CountAsync(options), Times.Once);
        repository.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task User_LegacyPagedCollection_DoesNotCount(bool explicitFalse)
    {
        var service = new Mock<IUserService>(MockBehavior.Strict);
        var options = new UserQueryDTO { Page = 1, PageSize = 2 };
        var items = new List<UserResponseDTO> { new() { Id = 1 } };
        service.Setup(x => x.QueryAsync(options)).ReturnsAsync(items);
        var controller = new UsersController(service.Object);
        var result = explicitFalse ? await controller.FindAll(options, false) : await controller.FindAll(options);
        Assert.Same(items, Assert.IsType<OkObjectResult>(result).Value);
        service.Verify(x => x.QueryAsync(options), Times.Once);
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task User_Count_DelegatesOnlyToCount()
    {
        var repository = new Mock<IUserRepository>(MockBehavior.Strict);
        var options = new UserQueryDTO { Page = 2, PageSize = 2 };
        repository.Setup(x => x.CountAsync(options)).ReturnsAsync(5);
        var total = await new UserService(repository.Object, new PasswordHasher<User>()).CountAsync(options);
        Assert.Equal(5, total);
        repository.Verify(x => x.CountAsync(options), Times.Once);
        repository.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Veterinarian_LegacyPagedCollection_DoesNotCount(bool explicitFalse)
    {
        var service = new Mock<IVeterinarianService>(MockBehavior.Strict);
        var options = new VeterinarianQueryDTO { Page = 1, PageSize = 2 };
        var items = new List<VeterinarianResponseDTO> { new() { Id = 1 } };
        service.Setup(x => x.QueryAsync(options)).ReturnsAsync(items);
        var controller = new VeterinariansController(service.Object);
        var result = explicitFalse ? await controller.FindAll(options, false) : await controller.FindAll(options);
        Assert.Same(items, Assert.IsType<OkObjectResult>(result).Value);
        service.Verify(x => x.QueryAsync(options), Times.Once);
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Veterinarian_Count_DelegatesOnlyToCount()
    {
        var repository = new Mock<IVeterinarianRepository>(MockBehavior.Strict);
        var options = new VeterinarianQueryDTO { Page = 2, PageSize = 2 };
        repository.Setup(x => x.CountAsync(options)).ReturnsAsync(5);
        var total = await new VeterinarianService(repository.Object).CountAsync(options);
        Assert.Equal(5, total);
        repository.Verify(x => x.CountAsync(options), Times.Once);
        repository.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Consultation_LegacyPagedCollection_DoesNotCount(bool explicitFalse)
    {
        var service = new Mock<IConsultationService>(MockBehavior.Strict);
        var options = new ConsultationQueryDTO { Page = 1, PageSize = 2 };
        var items = new List<ConsultationResponseDTO> { new() { Id = 1 } };
        service.Setup(x => x.QueryAsync(options)).ReturnsAsync(items);
        var controller = new ConsultationsController(service.Object);
        var result = explicitFalse ? await controller.FindAll(options, false) : await controller.FindAll(options);
        Assert.Same(items, Assert.IsType<OkObjectResult>(result).Value);
        service.Verify(x => x.QueryAsync(options), Times.Once);
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Consultation_Count_DelegatesOnlyToCount()
    {
        var repository = new Mock<IConsultationRepository>(MockBehavior.Strict);
        var options = new ConsultationQueryDTO { Page = 2, PageSize = 2 };
        repository.Setup(x => x.CountAsync(options)).ReturnsAsync(5);
        var total = await new ConsultationService(repository.Object, Mock.Of<IUserRepository>(), Mock.Of<IPetRepository>(), Mock.Of<IVeterinarianRepository>()).CountAsync(options);
        Assert.Equal(5, total);
        repository.Verify(x => x.CountAsync(options), Times.Once);
        repository.VerifyNoOtherCalls();
    }
}
