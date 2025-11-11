using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TKV.Interface;
using TKV.Model.JsonModels;
using Xunit;

namespace TKV.Test;

public class MainController
{
    private readonly Mock<IMainServices> _mainServices = new();

    private TadbirKishViraSample.Controllers.MainController CreateSut() => new(_mainServices.Object);

    [Fact]
    public void Controller_Has_ApiController_And_Route_Attributes()
    {
        var type = typeof(TadbirKishViraSample.Controllers.MainController);

        var apiAttr = type.GetCustomAttribute<ApiControllerAttribute>();
        var routeAttr = type.GetCustomAttribute<RouteAttribute>();

        apiAttr.Should().NotBeNull();
        routeAttr.Should().NotBeNull();
        routeAttr!.Template.Should().Be("api/[controller]");
    }

    [Fact]
    public void CreateRequest_Has_HttpPost_Attribute_With_Template()
    {
        var method = typeof(TadbirKishViraSample.Controllers.MainController).GetMethod("CreateRequest");

        var httpPost = method!.GetCustomAttribute<HttpPostAttribute>();

        httpPost.Should().NotBeNull();
        httpPost!.Template.Should().Be("CreateRequest");
    }

    [Fact]
    public void GetRequests_Has_HttpGet_Attribute_With_Template()
    {
        var method = typeof(TadbirKishViraSample.Controllers.MainController).GetMethod("GetRequests");

        var httpGet = method!.GetCustomAttribute<HttpGetAttribute>();

        httpGet.Should().NotBeNull();
        httpGet!.Template.Should().Be("GetRequests");
    }

    [Fact]
    public async Task CreateRequest_Returns_Ok_With_Service_Result()
    {
        var expectedServiceResult =new JsonResponse
        {
            IsSuccess = true,
            Message = "Request has been saved successfully.",
        };
        var sampleRequest = new RequestModel();
        _mainServices
            .Setup(s => s.CreateRequest(sampleRequest))
            .ReturnsAsync(expectedServiceResult);

        var sut = CreateSut();

        var result = await sut.CreateRequest(sampleRequest);

        var ok = result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.StatusCode.Should().Be(200);
        ok.Value.Should().Be(expectedServiceResult);

        _mainServices.Verify(s => s.CreateRequest(sampleRequest), Times.Once);
        _mainServices.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetRequests_Returns_Ok_With_Service_Result()
    {
        var expected =new JsonResponse
        {
            IsSuccess = true,
            Message = "Request list has been loaded successfully.",
        };
        _mainServices
            .Setup(s => s.GetRequests())
            .ReturnsAsync(expected);

        var sut = CreateSut();

        var result = await sut.GetRequests();

        var ok = result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.StatusCode.Should().Be(200);
        ok.Value.Should().BeSameAs(expected); 
        _mainServices.Verify(s => s.GetRequests(), Times.Once);
        _mainServices.VerifyNoOtherCalls();
    }
}
