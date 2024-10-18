using Microsoft.AspNetCore.Mvc;
using Moq;
using Prog6212Part2.Controllers;
using Prog6212Part2.Data;
using Prog6212Part2.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;

public class ManageClaimsControllerTests
{
    private readonly Mock<ApplicationDbContext> _contextMock;
    private readonly ManageClaimsController _controller;

    public ManageClaimsControllerTests()
    {
        _contextMock = new Mock<ApplicationDbContext>();
        _controller = new ManageClaimsController(_contextMock.Object);
    }

    [Fact]
    public async Task Index_ShouldReturnPendingClaims()
    {
        // Arrange
        var pendingClaims = new List<LecturerClaim>
        {
            new LecturerClaim { Id = 1, IsApproved = null },
            new LecturerClaim { Id = 2, IsApproved = null }
        }.AsQueryable();

        var mockSet = new Mock<DbSet<LecturerClaim>>();
        mockSet.As<IQueryable<LecturerClaim>>().Setup(m => m.Provider).Returns(pendingClaims.Provider);
        mockSet.As<IQueryable<LecturerClaim>>().Setup(m => m.Expression).Returns(pendingClaims.Expression);
        mockSet.As<IQueryable<LecturerClaim>>().Setup(m => m.ElementType).Returns(pendingClaims.ElementType);
        mockSet.As<IQueryable<LecturerClaim>>().Setup(m => m.GetEnumerator()).Returns(pendingClaims.GetEnumerator());

        _contextMock.Setup(c => c.LecturerClaims).Returns(mockSet.Object);

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<LecturerClaim>>(viewResult.Model);
        Assert.Equal(2, model.Count());
    }

    [Fact]
    public async Task ApproveClaim_ShouldSetIsApprovedToTrue()
    {
        // Arrange
        var claim = new LecturerClaim { Id = 1, IsApproved = null };
        _contextMock.Setup(c => c.LecturerClaims.FindAsync(1)).ReturnsAsync(claim);

        // Act
        var result = await _controller.ApproveClaim(1);

        // Assert
        Assert.True(claim.IsApproved);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<System.Threading.CancellationToken>()), Times.Once);
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
    }
}
