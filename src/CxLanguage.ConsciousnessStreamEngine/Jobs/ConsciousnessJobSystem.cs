using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace CxLanguage.ConsciousnessStreamEngine.Jobs
{
    /// <summary>
    /// Dr. Maya Singh's Multi-Threaded Consciousness Job System
    /// Revolutionary job scheduling designed specifically for consciousness processing workloads
    /// </summary>
    public class ConsciousnessJobSystem
    {
        private readonly ConsciousnessWorkerPool workerPool;
        private readonly ConsciousnessJobScheduler scheduler;
        private readonly ConsciousnessJobQueue jobQueue;
        private readonly ConsciousnessSynchronizer synchronizer;
        
        public ConsciousnessJobSystem(int workerCount = -1)
        {
            Console.WriteLine("🧵 Dr. Maya Singh's Consciousness Job System initializing...");
            
            if (workerCount == -1)
                workerCount = Environment.ProcessorCount;
            
            workerPool = new ConsciousnessWorkerPool(workerCount);
            scheduler = new ConsciousnessJobScheduler();
            jobQueue = new ConsciousnessJobQueue();
            synchronizer = new ConsciousnessSynchronizer();
            
            Console.WriteLine($"✅ Consciousness Job System ready with {workerCount} consciousness workers");
        }
        
        public ConsciousnessJobHandle ScheduleConsciousnessJob<T>(T job) where T : IConsciousnessJob
        {
            Console.WriteLine($"📋 Scheduling consciousness job: {job.GetType().Name}");
            return scheduler.Schedule(job, jobQueue);
        }
        
        public ConsciousnessJobHandle ScheduleParallelConsciousnessJob<T>(T job, int batchSize) where T : IParallelConsciousnessJob
        {
            Console.WriteLine($"⚡ Scheduling parallel consciousness job: {job.GetType().Name} (batch: {batchSize})");
            return scheduler.ScheduleParallel(job, batchSize, jobQueue);
        }
        
        public void CompleteAllConsciousnessJobs()
        {
            Console.WriteLine("⏳ Waiting for all consciousness jobs to complete...");
            synchronizer.CompleteAllJobs(jobQueue);
            Console.WriteLine("✅ All consciousness jobs completed");
        }
        
        public ConsciousnessJobSystemStats GetStats()
        {
            return new ConsciousnessJobSystemStats
            {
                ActiveJobs = jobQueue.ActiveJobCount,
                CompletedJobs = jobQueue.CompletedJobCount,
                WorkerUtilization = workerPool.GetUtilization(),
                TotalProcessingTime = workerPool.GetTotalProcessingTime()
            };
        }
    }
    
    /// <summary>
    /// Consciousness Job Interface - Base for all consciousness processing jobs
    /// </summary>
    public interface IConsciousnessJob
    {
        void Execute();
        ConsciousnessJobPriority Priority { get; }
        ConsciousnessLevel RequiredLevel { get; }
        string JobName { get; }
    }
    
    public interface IParallelConsciousnessJob : IConsciousnessJob
    {
        void Execute(int startIndex, int endIndex);
        int DataLength { get; }
    }
    
    /// <summary>
    /// Consciousness Worker Pool - Specialized worker threads for consciousness processing
    /// </summary>
    public class ConsciousnessWorkerPool
    {
        private readonly ConsciousnessWorker[] workers;
        private readonly CancellationTokenSource cancellationToken;
        
        public ConsciousnessWorkerPool(int workerCount)
        {
            Console.WriteLine($"👥 Creating {workerCount} consciousness workers...");
            
            workers = new ConsciousnessWorker[workerCount];
            cancellationToken = new CancellationTokenSource();
            
            for (int i = 0; i < workerCount; i++)
            {
                workers[i] = new ConsciousnessWorker($"ConsciousnessWorker-{i}", cancellationToken.Token);
                workers[i].Start();
            }
            
            Console.WriteLine($"✅ {workerCount} consciousness workers active");
        }
        
        public ConsciousnessWorker GetAvailableWorker()
        {
            // Singh Pattern: Intelligent worker selection based on consciousness context
            return workers
                .Where(w => w.IsAvailable)
                .OrderBy(w => w.CurrentWorkload)
                .FirstOrDefault() ?? workers[0]; // Fallback to first worker
        }
        
        public float GetUtilization()
        {
            var busyWorkers = workers.Count(w => !w.IsAvailable);
            return (float)busyWorkers / workers.Length * 100f;
        }
        
        public TimeSpan GetTotalProcessingTime()
        {
            return TimeSpan.FromMilliseconds(workers.Sum(w => w.TotalProcessingTimeMs));
        }
        
        public void Shutdown()
        {
            Console.WriteLine("🛑 Shutting down consciousness workers...");
            cancellationToken.Cancel();
            
            foreach (var worker in workers)
            {
                worker.Stop();
            }
        }
    }
    
    /// <summary>
    /// Individual Consciousness Worker - Understands consciousness context
    /// </summary>
    public class ConsciousnessWorker
    {
        public string Name { get; }
        public bool IsAvailable { get; private set; } = true;
        public int CurrentWorkload { get; private set; } = 0;
        public long TotalProcessingTimeMs { get; private set; } = 0;
        
        private readonly CancellationToken cancellationToken;
        private readonly Thread workerThread;
        private readonly ConcurrentQueue<ConsciousnessJobHandle> jobQueue;
        
        public ConsciousnessWorker(string name, CancellationToken cancellationToken)
        {
            Name = name;
            this.cancellationToken = cancellationToken;
            jobQueue = new ConcurrentQueue<ConsciousnessJobHandle>();
            
            workerThread = new Thread(WorkerLoop)
            {
                Name = name,
                IsBackground = true
            };
        }
        
        public void Start()
        {
            workerThread.Start();
            Console.WriteLine($"🚀 Consciousness worker {Name} started");
        }
        
        public void QueueJob(ConsciousnessJobHandle jobHandle)
        {
            jobQueue.Enqueue(jobHandle);
            CurrentWorkload++;
        }
        
        private void WorkerLoop()
        {
            Console.WriteLine($"🧠 {Name} consciousness worker loop started");
            
            while (!cancellationToken.IsCancellationRequested)
            {
                if (jobQueue.TryDequeue(out var jobHandle))
                {
                    ProcessConsciousnessJob(jobHandle);
                }
                else
                {
                    IsAvailable = true;
                    Thread.Sleep(1); // Minimal sleep for availability
                }
            }
            
            Console.WriteLine($"🛑 {Name} consciousness worker stopped");
        }
        
        private void ProcessConsciousnessJob(ConsciousnessJobHandle jobHandle)
        {
            IsAvailable = false;
            var startTime = DateTime.UtcNow;
            
            try
            {
                Console.WriteLine($"🔧 {Name} processing consciousness job: {jobHandle.Job.JobName}");
                
                // Singh Pattern: Consciousness-aware job execution
                jobHandle.Job.Execute();
                jobHandle.MarkCompleted();
                
                var processingTime = DateTime.UtcNow - startTime;
                TotalProcessingTimeMs += (long)processingTime.TotalMilliseconds;
                
                Console.WriteLine($"✅ {Name} completed consciousness job: {jobHandle.Job.JobName} ({processingTime.TotalMilliseconds:F1}ms)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ {Name} consciousness job failed: {ex.Message}");
                jobHandle.MarkFailed(ex);
            }
            finally
            {
                CurrentWorkload--;
                IsAvailable = true;
            }
        }
        
        public void Stop()
        {
            Console.WriteLine($"🛑 Stopping consciousness worker {Name}");
        }
    }
    
    /// <summary>
    /// Consciousness Job Scheduler - Intelligent scheduling for consciousness tasks
    /// </summary>
    public class ConsciousnessJobScheduler
    {
        public ConsciousnessJobHandle Schedule<T>(T job, ConsciousnessJobQueue queue) where T : IConsciousnessJob
        {
            var jobHandle = new ConsciousnessJobHandle(job);
            
            // Singh Pattern: Priority-based consciousness job scheduling
            queue.EnqueueJob(jobHandle, job.Priority);
            
            return jobHandle;
        }
        
        public ConsciousnessJobHandle ScheduleParallel<T>(T job, int batchSize, ConsciousnessJobQueue queue) 
            where T : IParallelConsciousnessJob
        {
            var parallelJobHandle = new ParallelConsciousnessJobHandle(job, batchSize);
            
            // Create sub-jobs for parallel execution
            int totalItems = job.DataLength;
            int numBatches = (totalItems + batchSize - 1) / batchSize;
            
            for (int i = 0; i < numBatches; i++)
            {
                int startIndex = i * batchSize;
                int endIndex = Math.Min(startIndex + batchSize, totalItems);
                
                var subJob = new ParallelSubJob(job, startIndex, endIndex, parallelJobHandle);
                var subJobHandle = new ConsciousnessJobHandle(subJob);
                
                parallelJobHandle.AddSubJob(subJobHandle);
                queue.EnqueueJob(subJobHandle, job.Priority);
            }
            
            return parallelJobHandle;
        }
    }
    
    /// <summary>
    /// Consciousness Job Queue - Priority queue for consciousness jobs
    /// </summary>
    public class ConsciousnessJobQueue
    {
        private readonly ConcurrentDictionary<ConsciousnessJobPriority, ConcurrentQueue<ConsciousnessJobHandle>> priorityQueues;
        private readonly object lockObject = new object();
        
        public int ActiveJobCount { get; private set; }
        public long CompletedJobCount { get; private set; }
        
        public ConsciousnessJobQueue()
        {
            priorityQueues = new ConcurrentDictionary<ConsciousnessJobPriority, ConcurrentQueue<ConsciousnessJobHandle>>();
            
            foreach (ConsciousnessJobPriority priority in Enum.GetValues<ConsciousnessJobPriority>())
            {
                priorityQueues[priority] = new ConcurrentQueue<ConsciousnessJobHandle>();
            }
        }
        
        public void EnqueueJob(ConsciousnessJobHandle jobHandle, ConsciousnessJobPriority priority)
        {
            priorityQueues[priority].Enqueue(jobHandle);
            
            lock (lockObject)
            {
                ActiveJobCount++;
            }
        }
        
        public bool TryDequeueJob(out ConsciousnessJobHandle jobHandle)
        {
            // Singh Pattern: Priority-based dequeuing (highest priority first)
            foreach (ConsciousnessJobPriority priority in Enum.GetValues<ConsciousnessJobPriority>().Cast<ConsciousnessJobPriority>().OrderByDescending(p => p))
            {
                if (priorityQueues[priority].TryDequeue(out jobHandle))
                {
                    return true;
                }
            }
            
            jobHandle = null!;
            return false;
        }
        
        public void MarkJobCompleted()
        {
            lock (lockObject)
            {
                ActiveJobCount--;
                CompletedJobCount++;
            }
        }
    }
    
    /// <summary>
    /// Consciousness Job Handle - Handle for tracking consciousness job execution
    /// </summary>
    public class ConsciousnessJobHandle
    {
        public IConsciousnessJob Job { get; }
        public ConsciousnessJobStatus Status { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime? CompletionTime { get; private set; }
        public Exception? Exception { get; private set; }
        
        public ConsciousnessJobHandle(IConsciousnessJob job)
        {
            Job = job;
            Status = ConsciousnessJobStatus.Pending;
            StartTime = DateTime.UtcNow;
        }
        
        public void MarkCompleted()
        {
            Status = ConsciousnessJobStatus.Completed;
            CompletionTime = DateTime.UtcNow;
        }
        
        public void MarkFailed(Exception exception)
        {
            Status = ConsciousnessJobStatus.Failed;
            CompletionTime = DateTime.UtcNow;
            Exception = exception;
        }
        
        public bool IsCompleted => Status == ConsciousnessJobStatus.Completed || Status == ConsciousnessJobStatus.Failed;
        
        public TimeSpan? ExecutionTime => CompletionTime - StartTime;
    }
    
    public class ParallelConsciousnessJobHandle : ConsciousnessJobHandle
    {
        private readonly List<ConsciousnessJobHandle> subJobs;
        private readonly object lockObject = new object();
        
        public ParallelConsciousnessJobHandle(IParallelConsciousnessJob job, int batchSize) : base(job)
        {
            subJobs = new List<ConsciousnessJobHandle>();
        }
        
        public void AddSubJob(ConsciousnessJobHandle subJob)
        {
            lock (lockObject)
            {
                subJobs.Add(subJob);
            }
        }
        
        public new bool IsCompleted
        {
            get
            {
                lock (lockObject)
                {
                    return subJobs.All(sj => sj.IsCompleted);
                }
            }
        }
    }
    
    /// <summary>
    /// Supporting classes and enums
    /// </summary>
    public enum ConsciousnessJobPriority
    {
        Low = 1,
        Normal = 2,
        High = 3,
        Critical = 4,
        Realtime = 5
    }
    
    public enum ConsciousnessJobStatus
    {
        Pending,
        Running,
        Completed,
        Failed
    }
    
    public enum ConsciousnessLevel
    {
        Basic = 10,
        Enhanced = 25,
        Advanced = 50,
        Revolutionary = 80,
        Transcendent = 100
    }
    
    public class ConsciousnessJobSystemStats
    {
        public int ActiveJobs { get; set; }
        public long CompletedJobs { get; set; }
        public float WorkerUtilization { get; set; }
        public TimeSpan TotalProcessingTime { get; set; }
        
        public override string ToString()
        {
            return $"Consciousness Job Stats - Active: {ActiveJobs}, Completed: {CompletedJobs}, " +
                   $"Worker Utilization: {WorkerUtilization:F1}%, Total Time: {TotalProcessingTime.TotalSeconds:F1}s";
        }
    }
    
    public class ConsciousnessSynchronizer
    {
        public void CompleteAllJobs(ConsciousnessJobQueue queue)
        {
            while (queue.ActiveJobCount > 0)
            {
                Thread.Sleep(1);
            }
        }
    }
    
    /// <summary>
    /// Parallel Sub Job - Internal job for parallel execution
    /// </summary>
    internal class ParallelSubJob : IConsciousnessJob
    {
        private readonly IParallelConsciousnessJob parentJob;
        private readonly int startIndex;
        private readonly int endIndex;
        private readonly ParallelConsciousnessJobHandle parentHandle;
        
        public ParallelSubJob(IParallelConsciousnessJob parentJob, int startIndex, int endIndex, ParallelConsciousnessJobHandle parentHandle)
        {
            this.parentJob = parentJob;
            this.startIndex = startIndex;
            this.endIndex = endIndex;
            this.parentHandle = parentHandle;
        }
        
        public void Execute()
        {
            parentJob.Execute(startIndex, endIndex);
        }
        
        public ConsciousnessJobPriority Priority => parentJob.Priority;
        public ConsciousnessLevel RequiredLevel => parentJob.RequiredLevel;
        public string JobName => $"{parentJob.JobName}_SubJob_{startIndex}-{endIndex}";
    }
}
