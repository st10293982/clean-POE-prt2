using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Prog6212Part2.Controllers;
using Prog6212Part2.Data;
using Prog6212Part2.Models;
using System.IO;
using System.Threading.Tasks;
using Xunit;

public class LecturerClaimsControllerTests
{
    private readonly Mock<ApplicationDbContext> _contextMock;
    private readonly Mock<IWebHostEnvironment> _environmentMock;
    private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
    private readonly LecturerClaimsController _controller;

    public LecturerClaimsControllerTests()
    {
        // Mock the ApplicationDbContext
        _contextMock = new Mock<ApplicationDbContext>();

        // Mock the IWebHostEnvironment
        _environmentMock = new Mock<IWebHostEnvironment>();

        // Mock the UserManager<IdentityUser>
        _userManagerMock = new Mock<UserManager<IdentityUser>>(
            Mock.Of<IUserStore<IdentityUser>>(),
            null, null, null, null, null, null, null, null
        );

        // Pass all the mocks into the LecturerClaimsController
        _controller = new LecturerClaimsController(
            _contextMock.Object,
            _environmentMock.Object,
            _userManagerMock.Object
        );
    }
    [Fact]
    public async Task Create_ValidClaim_ShouldSaveToDatabase()
    {
        // Arrange
        var claim = new LecturerClaim { HourlyRate = 100, HoursWorked = 5 };
        var fileMock = new Mock<IFormFile>();
        var content = "Fake file content";
        var fileName = "test.pdf";
        using (var ms = new MemoryStream())
        {
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;

            fileMock.Setup(_ => _.OpenReadStream()).Returns(ms);
            fileMock.Setup(_ => _.FileName).Returns(fileName);
            fileMock.Setup(_ => _.Length).Returns(ms.Length);
        }

        _environmentMock.Setup(env => env.WebRootPath).Returns("wwwroot");

        // Act
        var result = await _controller.Create(claim, fileMock.Object);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        _contextMock.Verify(c => c.Add(It.IsAny<LecturerClaim>()), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<System.Threading.CancellationToken>()), Times.Once);
    }
}
