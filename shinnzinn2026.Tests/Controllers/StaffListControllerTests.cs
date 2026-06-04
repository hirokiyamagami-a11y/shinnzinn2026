using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shinnzinn2026.Controllers;
using shinnzinn2026.Data;
using shinnzinn2026.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace shinnzinn2026.Tests.Controllers
{
    public class StaffListControllerTests
    {
        private static ApplicationDbContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        private static void SeedStaffs(ApplicationDbContext context)
        {
            context.Staffs.AddRange(new List<StaffModel>
            {
                new StaffModel { Id = 1, StaffCd = "A001", Name = "Taro", DeleteFlag = 0, ManagerFlag = 1 },
                new StaffModel { Id = 2, StaffCd = "A002", Name = "Jiro", DeleteFlag = 0, ManagerFlag = 0 },
                new StaffModel { Id = 3, StaffCd = "A003", Name = "Saburo", DeleteFlag = 0, ManagerFlag = 0 },
                new StaffModel { Id = 4, StaffCd = "A004", Name = "Shiro", DeleteFlag = 0, ManagerFlag = 0 },
                new StaffModel { Id = 5, StaffCd = "A005", Name = "Goro", DeleteFlag = 0, ManagerFlag = 0 },
                new StaffModel { Id = 6, StaffCd = "A006", Name = "Roku", DeleteFlag = 0, ManagerFlag = 0 },
            });
            context.SaveChanges();
        }

        [Fact]
        public async Task StaffList_RedirectsToLogin_WhenSessionMissing()
        {
            using var context = CreateInMemoryContext(nameof(StaffList_RedirectsToLogin_WhenSessionMissing));
            SeedStaffs(context);

            var controller = new StaffListController(context);
            // provide a TestSession without LoginStaffCd to simulate "no logged-in user"
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();
            controller.ControllerContext = new ControllerContext() { HttpContext = httpContext };

            var result = await controller.StaffList();

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirect.ActionName);
        }

        [Fact]
        public async Task StaffList_RedirectsToLogin_WhenNotManager()
        {
            using var context = CreateInMemoryContext(nameof(StaffList_RedirectsToLogin_WhenNotManager));
            SeedStaffs(context);

            var controller = new StaffListController(context);
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();
            httpContext.Session.Set("LoginStaffCd", System.Text.Encoding.UTF8.GetBytes("A002")); // A002 is not manager
            controller.ControllerContext = new ControllerContext() { HttpContext = httpContext };

            var result = await controller.StaffList();

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirect.ActionName);
        }

        [Fact]
        public async Task StaffList_ReturnsView_WithPagedResults()
        {
            using var context = CreateInMemoryContext(nameof(StaffList_ReturnsView_WithPagedResults));
            SeedStaffs(context);

            var controller = new StaffListController(context);
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();
            httpContext.Session.Set("LoginStaffCd", System.Text.Encoding.UTF8.GetBytes("A001")); // A001 is manager
            controller.ControllerContext = new ControllerContext() { HttpContext = httpContext };

            var result = await controller.StaffList(page: 2);

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<StaffModel>>(view.Model);
            // pageSize is 5, so page 2 should contain the 6th item only
            Assert.Single(model);
            Assert.Equal("A006", model.First().StaffCd);

            Assert.Equal(2, controller.ViewBag.CurrentPage);
            Assert.Equal(2, controller.ViewBag.TotalPages);
        }

        // Simple ISession implementation for tests
        private class TestSession : ISession
        {
            private readonly Dictionary<string, byte[]> _store = new Dictionary<string, byte[]>();
            public IEnumerable<string> Keys => _store.Keys;
            public string Id => "test";
            public bool IsAvailable => true;
            public void Clear() => _store.Clear();
            public Task CommitAsync(System.Threading.CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task LoadAsync(System.Threading.CancellationToken cancellationToken = default) { return Task.CompletedTask; }
            public void Remove(string key) => _store.Remove(key);
            public void Set(string key, byte[] value) => _store[key] = value;
            public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value);
        }

        // no extension helpers here; tests set session bytes directly
    }
}
