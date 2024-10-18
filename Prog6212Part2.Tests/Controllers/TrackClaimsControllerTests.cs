using Microsoft.AspNetCore.Mvc;
using Moq;
using Prog6212Part2.Controllers;
using Prog6212Part2.Data;
using Prog6212Part2.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class TrackClaimsControllerTests
{
    private readonly Mock<ApplicationDbContext> _contextMock;
    private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
    private readonly TrackClaimsController _controller;

    public TrackClaimsControllerTests()
    {
        _contextMock = new Mock<ApplicationDbContext>();
        _userManagerMock = new Mock<UserManager<IdentityUser>>(
            Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);
        _controller = new TrackClaimsController(_contextMock.Object, _userManagerMock.Object);
    }

    [Fact]
    public async Task Track_ShouldReturnUserClaims()
    {
        // Arrange
        var userId = "user1";
        var userClaims = new List<LecturerClaim>
        {
            new LecturerClaim { Id = 1, UserId = userId },
            new LecturerClaim { Id = 2, UserId = userId }
        }.AsQueryable();

        var mockSet = new Mock<DbSet<LecturerClaim>>();
        mockSet.As<IQueryable<LecturerClaim>>().Setup(m => m.Provider).Returns(userClaims.Provider);
        mockSet.As<IQueryable<LecturerClaim>>().Setup(m => m.Expression).Returns(userClaims.Expression);
        mockSet.As<IQueryable<LecturerClaim>>().Setup(m => m.ElementType).Returns(userClaims.ElementType);
        mockSet.As<IQueryable<LecturerClaim>>().Setup(m => m.GetEnumerator()).Returns(userClaims.GetEnumerator());

        _contextMock.Setup(c => c.LecturerClaims).Returns(mockSet.Object);
        _userManagerMock.Setup(um => um.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).Returns(userId);

        // Act
        var result = await _controller.Track();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<LecturerClaim>>(viewResult.Model);
        Assert.Equal(2, model.Count());
    }
}
