using System.Reflection;
using Marathon.Controllers;
using Microsoft.AspNetCore.Authorization;

namespace TestMarathon
{
    /// <summary>
    /// Guards the role rules required by the brief: organisers manage events and
    /// categories, participants enrol, and only staff can read the full register.
    /// </summary>
    public class RoleBasedAccessTests
    {
        private static string? RolesFor(Type controller, string methodName)
        {
            var method = controller.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .First(m => m.Name == methodName);
            return method.GetCustomAttribute<AuthorizeAttribute>()?.Roles;
        }

        [Theory]
        [InlineData(nameof(EventsController.CreateEvent), "Organiser, Admin")]
        [InlineData(nameof(EventsController.UpdateEvent), "Organiser, Admin")]
        [InlineData(nameof(EventsController.RemoveEvent), "Organiser, Admin")]
        public void EventManagement_RequiresOrganiserOrAdmin(string method, string expected)
        {
            Assert.Equal(expected, RolesFor(typeof(EventsController), method));
        }

        [Theory]
        [InlineData(nameof(CategoryController.CreateCategory), "Organiser, Admin")]
        [InlineData(nameof(CategoryController.EditCatery), "Organiser, Admin")]
        [InlineData(nameof(CategoryController.DeleteCategory), "Organiser, Admin")]
        public void CategoryManagement_RequiresOrganiserOrAdmin(string method, string expected)
        {
            Assert.Equal(expected, RolesFor(typeof(CategoryController), method));
        }

        [Fact]
        public void Enrolling_IsParticipantOnly()
        {
            Assert.Equal("Participant", RolesFor(typeof(EnrolmentController), nameof(EnrolmentController.EnrolUserInCategory)));
        }

        [Theory]
        [InlineData(nameof(EnrolmentController.GetAllEnrolments))]
        [InlineData(nameof(EnrolmentController.UpdateEnrolmentStatus))]
        public void EnrolmentRegister_IsStaffOnly(string method)
        {
            Assert.Equal("Organiser, Admin", RolesFor(typeof(EnrolmentController), method));
        }

        [Fact]
        public void EventReadEndpoints_AreAnonymous()
        {
            Assert.Null(RolesFor(typeof(EventsController), nameof(EventsController.GetEvents)));
            Assert.Null(RolesFor(typeof(EventsController), nameof(EventsController.GetEventById)));
        }

        [Fact]
        public void UploadEndpoints_RequireAuthentication()
        {
            var type = typeof(UploadController);
            Assert.NotNull(type.GetCustomAttribute<AuthorizeAttribute>());
            Assert.Equal("Organiser, Admin",
                RolesFor(type, nameof(UploadController.UploadEventBanner)));
        }
    }
}
