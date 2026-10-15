using Marathon.Controllers;
using Marathon.Data;
using Marathon.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using Microsoft.Extensions.Configuration;

using System;
using System.Collections.Generic;

namespace Race_Day.Test.TestHelpers
{
    public class AuthFixture : IDisposable
    {
        public ApplicationDbcontext Db { get; }
        public IConfiguration Config { get; }
        public PasswordHasher<UserAccount> Hasher { get; }

        public AuthFixture()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbcontext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            Db = new ApplicationDbcontext(options);

            Config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"]      = "THIS_IS_A_TEST_KEY_THAT_IS_LONG_ENOUGH_32CHARS!",
                    ["Jwt:Issuer"]   = "TestIssuer",
                    ["Jwt:Audience"] = "TestAudience"
                })
                .Build();

            Hasher = new PasswordHasher<UserAccount>();
        }

        public AuthController CreateController() => new AuthController(Db, Config, Hasher);

        public void Dispose() => Db.Dispose();
    }
}