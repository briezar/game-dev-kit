using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameDevKit
{
    public class TaskLoader
    {
        public class TaskHandle
        {
            /// <summary> Weight of the task in the total progress calculation. </summary>
            public float Weight { get; init; } = 1;
            public Func<UniTask> TaskFunc { get; private set; }
            public Func<float> ProgressGetter { get; private set; } // Should return 0-1
            public Func<string> ProgressInfoGetter { get; set; }

            public string ProgressInfo => ProgressInfoGetter?.Invoke() ?? "";
            public float Progress => Math.Clamp(ProgressGetter(), 0f, 1f);
            public float WeightedProgress => Progress * Weight;

            public TaskHandle(Func<UniTask> taskFunc, float estimatedDuration)
            {
                var progress = 0f;
                ProgressGetter = () => progress;

                TaskFunc = async () =>
                {
                    var cts = new CancellationTokenSource();
                    UniTaskUtils.WaitForProgress(estimatedDuration, p => progress = p, token: cts.Token);
                    await taskFunc();
                    cts.Cancel();
                    progress = 1f;
                };
            }
            public TaskHandle(Func<UniTask> taskFunc, Func<float> progressGetter)
            {
                TaskFunc = taskFunc;
                ProgressGetter = progressGetter;
            }
        }

        private readonly List<TaskHandle> _taskHandles = new();
        public float Progress { get; private set; }
        public TaskHandle CurrentTask { get; private set; }

        public event Action OnProgressUpdated;

        public void AddTask(TaskHandle handle)
        {
            _taskHandles.Add(handle);
        }

        public async UniTask RunAllAsync()
        {
            var totalWeight = _taskHandles.Sum(t => t.Weight);
            var lastProgress = -1f;

            Debug.Log($"Running {nameof(TaskLoader)} with {_taskHandles.Count} tasks.\n" + _taskHandles.Select(t => $"{t.ProgressInfo} | Weight: {t.Weight / totalWeight:P2}").JoinToString("\n"));

            foreach (var handle in _taskHandles)
            {
                CurrentTask = handle;
                var task = handle.TaskFunc();
                UpdateTotalProgress();

                while (!task.Status.IsCompleted())
                {
                    await UniTask.Yield();
                    UpdateTotalProgress();
                }

                await task;
            }

            UpdateTotalProgress();
            CurrentTask = null;

            void UpdateTotalProgress()
            {
                var totalWeightedProgress = _taskHandles.Sum(t => t.WeightedProgress);
                Progress = totalWeight > 0f ? totalWeightedProgress / totalWeight : 1f;
                lastProgress = Progress;
                OnProgressUpdated?.Invoke();
            }
        }

    }

}