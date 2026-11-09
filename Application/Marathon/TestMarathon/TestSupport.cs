using System.Security.Claims;
using Marathon.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TestMarathon
{
    /// <summary>Shared helpers for the controller unit tests.</summary>
    public static class TestSupport
    {
        /// <summary>A fresh, isolated in-memory database per test.</summary>
        public static ApplicationDbcontext CreateDb() =>
            new(new DbContextOptionsBuilder<ApplicationDbcontext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        /// <summary>Builds a claims principal that mimics a signed-in API caller.</summary>
        public static ClaimsPrincipal Principal(int accountId, string email, string role) =>
            new(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, accountId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role)
            }, authenticationType: "TestAuth"));

        /// <summary>Attaches the given principal to a controller's HttpContext.</summary>
        public static T WithUser<T>(this T controller, ClaimsPrincipal principal) where T : ControllerBase
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
            return controller;
        }
    }
}
