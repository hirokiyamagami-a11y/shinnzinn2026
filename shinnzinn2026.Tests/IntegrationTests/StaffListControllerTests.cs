using System.Threading.Tasks;
using System.Threading;
using System;
using Xunit;
using Microsoft.EntityFrameworkCore;
using shinnzinn2026.Data;
using shinnzinn2026.Models;
using shinnzinn2026.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace shinnzinn2026.Tests.IntegrationTests
{
    public class StaffListControllerTests
    {
        private ApplicationDbContext CreateInMemoryDb(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        private StaffListController CreateControllerWithSession(ApplicationDbContext db, string loginStaffCd)
        {
            var controller = new StaffListController(db);
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();
            if (!string.IsNullOrEmpty(loginStaffCd))
            {
                httpContext.Session.Set("LoginStaffCd", System.Text.Encoding.UTF8.GetBytes(loginStaffCd));
            }
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };
            return controller;
        }

        [Fact]
        public async Task StaffList_ReturnsViewWithStaffs_WhenDataExists()
        {
            var db = CreateInMemoryDb("test1");
            db.Staffs.AddRange(new List<StaffModel>
            {
                new StaffModel{ StaffCd = "A01", Password = "p", Name = "Alice", DeleteFlag = 0, ManagerFlag = 1 },
                new StaffModel{ StaffCd = "B02", Password = "p", Name = "Bob", DeleteFlag = 0, ManagerFlag = 0 },
                new StaffModel{ StaffCd = "C03", Password = "p", Name = "Carol", DeleteFlag = 0, ManagerFlag = 0 },
            });
            db.SaveChanges();

            var controller = CreateControllerWithSession(db, "A01");

            var result = await controller.StaffList();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<StaffModel>>(view.Model);
            Assert.Equal(3, model.Count());
        }

        [Fact]
        public async Task StaffList_Pagination_ReturnsOnlyPageItems()
        {
            var db = CreateInMemoryDb("test2");
            // create 12 staff records
            for (int i = 1; i <= 12; i++)
            {
                db.Staffs.Add(new StaffModel{ StaffCd = i.ToString("D2"), Password = "p", Name = $"N{i}", DeleteFlag = 0, ManagerFlag = (short)(i==1?1:0) });
            }
            db.SaveChanges();

            var controller = CreateControllerWithSession(db, "01");

            var result = await controller.StaffList(page: 2);

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<StaffModel>>(view.Model);
            // pageSize = 5 -> page 2 should have items 6..10 => 5 items
            Assert.Equal(5, model.Count());
            Assert.Equal("06", model.First().StaffCd);
        }

        [Fact]
        public async Task StaffList_InvalidPage_ClampsToValidRange()
        {
            var db = CreateInMemoryDb("test3");
            // only 3 items => totalPages = 1
            db.Staffs.AddRange(new List<StaffModel>
            {
                new StaffModel{ StaffCd = "A1", Password = "p", Name = "Alice", DeleteFlag = 0, ManagerFlag = 1 },
                new StaffModel{ StaffCd = "B2", Password = "p", Name = "Bob", DeleteFlag = 0, ManagerFlag = 0 },
                new StaffModel{ StaffCd = "C3", Password = "p", Name = "Carol", DeleteFlag = 0, ManagerFlag = 0 },
            });
            db.SaveChanges();

            var controller = CreateControllerWithSession(db, "A1");

            // request an out-of-range page
            var result = await controller.StaffList(page: 999);

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<StaffModel>>(view.Model);
            // should clamp to page 1 and return 3 items
            Assert.Equal(3, model.Count());
            Assert.Equal(1, controller.ViewBag.CurrentPage);
        }
    }

    // Simple in-memory ISession implementation for tests
    class TestSession : ISession
    {
        private Dictionary<string, byte[]> _store = new();
        public IEnumerable<string> Keys => _store.Keys;
        public string Id => "test";
        public bool IsAvailable => true;
        public void Clear() => _store.Clear();
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Remove(string key) => _store.Remove(key);
        public void Set(string key, byte[] value) => _store[key] = value;
        public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value);
    }

    // no extension helpers here to avoid ambiguity with other test files
}

