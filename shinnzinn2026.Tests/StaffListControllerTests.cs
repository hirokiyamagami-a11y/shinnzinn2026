using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shinnzinn2026.Controllers;
using shinnzinn2026.Data;
using shinnzinn2026.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace shinnzinn2026.Tests
{
    public class StaffListControllerTests
    {
        private ApplicationDbContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        private ControllerContext CreateControllerContextWithSession(ISession session)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Session = session;
            return new ControllerContext()
            {
                HttpContext = httpContext
            };
        }

        private class TestSession : ISession
        {
            private readonly Dictionary<string, byte[]> _store = new Dictionary<string, byte[]>();
            public IEnumerable<string> Keys => _store.Keys;
            public string Id => "test";
            public bool IsAvailable => true;
            public void Clear() => _store.Clear();
            public Task CommitAsync(System.Threading.CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task LoadAsync(System.Threading.CancellationToken cancellationToken = default) => Task.CompletedTask;
            public void Remove(string key) => _store.Remove(key);
            public void Set(string key, byte[] value) => _store[key] = value;
            public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value);
        }

        [Fact]
        public async Task StaffList_RedirectsToLogin_WhenNoSession()
        {
            using var context = CreateInMemoryContext("no_session_db");
            var controller = new StaffListController(context);
            controller.ControllerContext = CreateControllerContextWithSession(new TestSession());

            var result = await controller.StaffList();

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirect.ActionName);
        }

        [Fact]
        public async Task StaffList_RedirectsToLogin_WhenNotManager()
        {
            using var context = CreateInMemoryContext("not_manager_db");
            context.Staffs.Add(new StaffModel { StaffCd = "S1", ManagerFlag = 0, DeleteFlag = 0, Name = "User" });
            await context.SaveChangesAsync();

            var session = new TestSession();
            session.Set("LoginStaffCd", System.Text.Encoding.UTF8.GetBytes("S1"));

            var controller = new StaffListController(context);
            controller.ControllerContext = CreateControllerContextWithSession(session);

            var result = await controller.StaffList();

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirect.ActionName);
        }

        [Fact]
        public async Task StaffList_ReturnsView_WithPaginationAndViewBag()
        {
            using var context = CreateInMemoryContext("pagination_db");

            for (int i = 1; i <= 12; i++)
            {
                context.Staffs.Add(new StaffModel { StaffCd = $"S{i}", ManagerFlag = (short)1, DeleteFlag = 0, Name = $"User{i}" });
            }
            await context.SaveChangesAsync();

            var session = new TestSession();
            session.Set("LoginStaffCd", System.Text.Encoding.UTF8.GetBytes("S1"));

            var controller = new StaffListController(context);
            controller.ControllerContext = CreateControllerContextWithSession(session);

            var result = await controller.StaffList(page: 2);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal("StaffList", view.ViewName);

            Assert.Equal(2, controller.ViewBag.CurrentPage);
            Assert.Equal(3, controller.ViewBag.TotalPages);

            var model = Assert.IsAssignableFrom<System.Collections.IEnumerable>(view.Model);
        }
    }
}
