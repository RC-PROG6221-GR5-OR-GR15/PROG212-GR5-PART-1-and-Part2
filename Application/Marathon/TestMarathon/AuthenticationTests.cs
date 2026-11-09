using System.IdentityModel.Tokens.Jwt;
using Marathon.Controllers;
using Marathon.Data;
using Marathon.Models;
using Marathon.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace TestMarathon
{
    public class AuthenticationTests
    {
        private static (AuthController controller, ApplicationDbcontext db) Build()
        {
            var db = TestSupport.CreateDb();
            var hasher = new PasswordHasher<UserAccount>();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = "THIS_IS_A_TEST_SIGNING_KEY_LONG_ENOUGH_32CHARS!",
                    ["Jwt:Issuer"] = "TestIssuer",
                    ["Jwt:Audience"] = "TestAudience"
                })
                .Build();

            return (new AuthController(db, config, hasher), db);
        }

        private static RegisterDTO NewParticipant(string email) => new()
        {
            Email = email,
            Password = "Passw0rd!",
            Role = "Participant",
            FirstName = "Test",
            LastName = "Runner",
            DateOfBirth = new DateOnly(1995, 1, 1),
            PhoneNumber = "0820000000"
        };

        [Fact]
        public async Task Register_Participant_CreatesAccountAndLinkedProfile()
        {
            var (controller, db) = Build();

            var result = await controller.Register(NewParticipant("new@raceday.com"));

            Assert.IsType<OkObjectResult>(result);
            var account = Assert.Single(db.accounts.ToList());
            var profile = Assert.Single(db.users.ToList());
            Assert.Equal("new@raceday.com", account.Email);
            Assert.Equal("Participant", account.Role);
            Assert.Equal("new@raceday.com", profile.Username);
            Assert.NotEqual("Passw0rd!", account.PasswordHash); // must be hashed
        }

        [Fact]
        public async Task Register_Organiser_DoesNotCreateParticipantProfile()
        {
            var (controller, db) = Build();
            var dto = new RegisterDTO { Email = "org@raceday.com", Password = "Passw0rd!", Role = "Organiser" };

            var result = await controller.Register(dto);

            Assert.IsType<OkObjectResult>(result);
            Assert.Single(db.accounts.ToList());
            Assert.Empty(db.users.ToList());
        }

        [Fact]
        public async Task Register_DuplicateEmail_ReturnsBadRequest()
        {
            var (controller, _) = Build();
            await controller.Register(NewParticipant("dupe@raceday.com"));

            var result = await controller.Register(NewParticipant("dupe@raceday.com"));

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Register_ParticipantWithoutProfileDetails_ReturnsBadRequest()
        {
            var (controller, _) = Build();
            var dto = new RegisterDTO { Email = "incomplete@raceday.com", Password = "Passw0rd!", Role = "Participant" };

            var result = await controller.Register(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Login_WithCorrectCredentials_ReturnsToken()
        {
            var (controller, _) = Build();
            await controller.Register(NewParticipant("login@raceday.com"));

            var result = await controller.Login(new LoginDTO { Email = "login@raceday.com", Password = "Passw0rd!" });

            var ok = Assert.IsType<OkObjectResult>(result);
            var token = ok.Value!.GetType().GetProperty("token")!.GetValue(ok.Value) as string;
            Assert.False(string.IsNullOrEmpty(token));

            var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);
            Assert.Contains(parsed.Claims, c => c.Value == "Participant");
        }

        [Fact]
        public async Task Login_WithWrongPassword_ReturnsUnauthorized()
        {
            var (controller, _) = Build();
            await controller.Register(NewParticipant("wrong@raceday.com"));

            var result = await controller.Login(new LoginDTO { Email = "wrong@raceday.com", Password = "nope" });

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Login_WithUnknownEmail_ReturnsUnauthorized()
        {
            var (controller, _) = Build();

            var result = await controller.Login(new LoginDTO { Email = "ghost@raceday.com", Password = "Passw0rd!" });

            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }
}
