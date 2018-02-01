﻿using System.Collections.Generic;
using System.Linq;
using Moq;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Data.Models;
using VirtoCommerce.ImageToolsModule.Data.Repositories;
using VirtoCommerce.ImageToolsModule.Data.Services;
using Xunit;

namespace VirtoCommerce.ImageToolsModule.Tests
{
    public class ThumbnailTaskSearchServiceTests
    {
        [Fact]
        public void SerchTasks_ThumbnailOptionSearchCriteria_ReturnsGenericSearchResponseOfTasksInExpectedOrder()
        {
            var taskEntitys = ThumbnailTaskEntitysDataSource.ToArray();
            var expectedTasks = ThumbnailTasksDataSource.OrderBy(t => t.Name).ThenByDescending(t => t.WorkPath).ToArray();

            var criteria = new ThumbnailOptionSearchCriteria {Sort = "Name:asc;WorkPath:desc"};

            var mock = new Mock<IThumbnailRepository>();
            mock.Setup(r => r.GetThumbnailTasksByIds(It.IsIn<string[]>()))
                .Returns((string[] ids) => { return taskEntitys.Where(t => ids.Contains(t.Id)).ToArray(); });

            var sut = new ThumbnailTaskSearchService(mock.Object);

            var resultTasks = sut.SerchTasks(criteria);
            
            Assert.Equal(resultTasks, expectedTasks);
        }
        
        [Fact]
        public void SerchTasks_KeywordString_ReturnsKeywordMatchingGenericSearchResponseOfTasks()
        {
            var keyword = "New Name";
            var taskEntitys = ThumbnailTaskEntitysDataSource.ToArray();
            var expectedTasks = ThumbnailTasksDataSource.Where(t => t.Name == keyword).ToArray();

            var mock = new Mock<IThumbnailRepository>();
            mock.Setup(r => r.GetThumbnailTasksByIds(It.IsIn<string[]>()))
                .Returns((string[] ids) => { return taskEntitys.Where(t => ids.Contains(t.Id)).ToArray(); });

            var sut = new ThumbnailTaskSearchService(mock.Object);

            var resultTasks = sut.SerchTasks(keyword);
            
            Assert.Equal(resultTasks, expectedTasks);
        }
        
        private static IEnumerable<ThumbnailTaskEntity> ThumbnailTaskEntitysDataSource
        {
            get
            {
                int i = 0;
                yield return new ThumbnailTaskEntity() {Id = $"Task {++i}"};
                yield return new ThumbnailTaskEntity() {Id = $"Task {++i}"};
                yield return new ThumbnailTaskEntity() {Id = $"Task {++i}"};
            }
        }
        
        private static IEnumerable<ThumbnailTask> ThumbnailTasksDataSource
        {
            get
            {
                int i = 0;
                yield return new ThumbnailTask() {Id = $"Task {++i}", Name = "New Name", WorkPath = "New Path"};
                yield return new ThumbnailTask() {Id = $"Task {++i}", Name = "New Name", WorkPath = "New Path"};
                yield return new ThumbnailTask() {Id = $"Task {++i}", Name = "New Name", WorkPath = "New Path"};
            }
        }
    }
}