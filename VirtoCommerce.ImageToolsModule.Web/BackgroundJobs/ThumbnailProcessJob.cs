﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;
using VirtoCommerce.ImageToolsModule.Web.Models;
using VirtoCommerce.ImageToolsModule.Web.Models.PushNotifications;
using VirtoCommerce.Platform.Core.PushNotifications;

namespace VirtoCommerce.ImageToolsModule.Web.BackgroundJobs
{
    public class ThumbnailProcessJob
    {
        private readonly IThumbnailGenerationProcessor _thumbnailProcessor;
        private readonly IPushNotificationManager _pushNotifier;
        private readonly IThumbnailTaskService _taskService;
        private readonly IThumbnailTaskSearchService _taskSearchService;

        public ThumbnailProcessJob(IPushNotificationManager pushNotifier, IThumbnailGenerationProcessor thumbnailProcessor, IThumbnailTaskService taskService, IThumbnailTaskSearchService taskSearchService)
        {
            _pushNotifier = pushNotifier;
            _thumbnailProcessor = thumbnailProcessor;
            _taskService = taskService;
            _taskSearchService = taskSearchService;
        }

        /// <summary>
        /// Thumbnail generation process
        /// </summary>
        /// <param name="generateRequest"></param>
        /// <param name="notifyEvent"></param>
        /// <param name="cancellationToken">Hangfire sets the cancellation token</param>
        /// <param name="context">Hangfire sets the process context</param>
        [DisableConcurrentExecution(60 * 60 * 24)]
        public async Task Process(ThumbnailsTaskRunRequest generateRequest, ThumbnailProcessNotification notifyEvent, IJobCancellationToken cancellationToken, PerformContext context)
        {
            try
            {
                Action<ThumbnailTaskProgress> progressCallback = x =>
                {
                    notifyEvent.Description = x.Message;
                    notifyEvent.Errors = x.Errors;
                    notifyEvent.ErrorCount = notifyEvent.Errors.Count;
                    notifyEvent.TotalCount = x.TotalCount ?? 0;
                    notifyEvent.ProcessedCount = x.ProcessedCount ?? 0;
                    notifyEvent.JobId = context.BackgroundJob.Id;

                    _pushNotifier.Upsert(notifyEvent);
                };

                //wrap token 
                var tasks = _taskService.GetByIds(generateRequest.TaskIds);

                var cancellationTokenWrapper = new JobCancellationTokenWrapper(cancellationToken);
                await _thumbnailProcessor.ProcessTasksAsync(tasks, generateRequest.Regenerate, progressCallback, cancellationTokenWrapper);

                //update tasks in case of successful generation
                foreach (var task in tasks)
                {
                    task.LastRun = DateTime.UtcNow;
                }
                
                _taskService.SaveOrUpdate(tasks);
            }
            catch (JobAbortedException)
            {
                //do nothing
            }
            catch (Exception ex)
            {
                notifyEvent.Description = "Error";
                notifyEvent.ErrorCount++;
                notifyEvent.Errors.Add(ex.ToString());
            }
            finally
            {
                notifyEvent.Finished = DateTime.UtcNow;
                notifyEvent.Description = "Process finished" + (notifyEvent.Errors.Any() ? " with errors" : " successfully");
                _pushNotifier.Upsert(notifyEvent);
            }
        }

        /// <summary>
        /// Find all tasks and run them
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [DisableConcurrentExecution(60 * 60 * 24)]
        public async Task ProcessAll(IJobCancellationToken cancellationToken)
        {
            var totalCount = _taskSearchService.Search(new ThumbnailTaskSearchCriteria() { Take = 0, Skip = 0 }).TotalCount;
            var tasks = _taskSearchService.Search(new ThumbnailTaskSearchCriteria() { Take = totalCount, Skip = 0 }).Results.ToArray();

            Action<ThumbnailTaskProgress> progressCallback = x => { };
            var cancellationTokenWrapper = new JobCancellationTokenWrapper(cancellationToken);
            await _thumbnailProcessor.ProcessTasksAsync(tasks, false, progressCallback, cancellationTokenWrapper);

            //update tasks in case of successful generation
            foreach (var task in tasks)
            {
                task.LastRun = DateTime.UtcNow;
            }

            _taskService.SaveOrUpdate(tasks);
        }
    }
}