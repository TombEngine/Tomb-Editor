using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TombLib.Utils
{
    public class FileSystemWatcherManager : IDisposable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public class ReloadArgs : EventArgs
        {
            public WatchedObj Obj { get; set; }
            public TimeSpan ElapsedTime { get; set; }
            public int AttemptIndex { get; set; }
        }

        public abstract class WatchedObj
        {
            public abstract void TryReload(FileSystemWatcherManager sender, ReloadArgs e);
            public abstract IEnumerable<string> Files { get; }
            public abstract IEnumerable<string> Directories { get; }
            public abstract string Name { get; }
            public abstract bool IsRepresentingSameObject(WatchedObj other);
        }

        private class DirectoryWatcher
        {
            public FileSystemWatcherManager _parent;
            public string _directory;
            public FileSystemWatcher _watcher;
            public volatile KeyValuePair<string, WatchedObj>[] _watchedObjsPerFile = null; // Readonly array object
            public volatile WatchedObj[] _watchedObjsPerDirectory = null; // Readonly array object
            public void NotificationReceived(object sender, FileSystemEventArgs e) => _parent.NotificationReceived(this, e);
            public void NotificationReceivedRenamed(object sender, RenamedEventArgs e)
            {
                _parent.NotificationReceived(this, e);
                _parent.NotificationReceived(this, new FileSystemEventArgs(e.ChangeType,
                    e.OldFullPath.Substring(0, e.OldFullPath.Length - e.OldName.Length), e.OldName));
            }
        }

        private class ReloadingObjInfo
        {
            public long ChangeTimestamp;
            public volatile int ReloadAttemptCount;
            public volatile string _lastChangedFile;
        };

        private const int _maxWaitTimeInMilliseconds = 2000;
        private const int _minWaitTimeInMilliseconds = 40;
        private const int _minAttempts = 2;
        private const int _threadCheckInternalInMilliseconds = 40;

        private readonly AutoResetEvent _threadWorkEvent = new AutoResetEvent(false);
        private readonly Thread _thread;
        private readonly List<DirectoryWatcher> _directoryWatchers = new List<DirectoryWatcher>();
        private readonly List<KeyValuePair<WatchedObj, ReloadingObjInfo>> _objsToReloadQueue =
            new List<KeyValuePair<WatchedObj, ReloadingObjInfo>>(); // No dictionary because we only have equality comparision via IsRepresentingSameObject, no hash code or anything.
        private volatile bool _threadShouldExit = false;
        private volatile bool _temporarilyStoppedReload = false;
        private volatile bool _restartReloadingBecauseListOfFilesChanged = false;
        private static double _toDataTimeTicks = (double)TimeSpan.TicksPerSecond / Stopwatch.Frequency;
        private object _reloadLock = new object();

        public FileSystemWatcherManager()
        {
            // Have an extra thread to avoid any work inside the file system watcher thread.
            // This is strongly advised against in the documentation, but we can also
            // much more easily retry reloading a couple times (the file may be locked for a while), if we do that in a seperate thread.
            _thread = new Thread(ThreadRun);
            _thread.IsBackground = true;
            _thread.Priority = ThreadPriority.BelowNormal;
            _thread.Start();
        }

        public void Dispose()
        {
            foreach (DirectoryWatcher directoryWatcher in _directoryWatchers)
            {
                directoryWatcher._watcher.EnableRaisingEvents = false;
                directoryWatcher._watcher.Dispose();
            }
            _directoryWatchers.Clear(); // Make double dispense work

            lock (_reloadLock)
                _threadShouldExit = true;
            try
            {
                _threadWorkEvent.Set();
            }
            catch (ObjectDisposedException) { }
        }

        public void UpdateAllFiles(IEnumerable<WatchedObj> watchedObjs, Action whileLocked = null)
        {
            // Avoid concurrent reload.
            lock (_reloadLock)
            {
                whileLocked?.Invoke();

                // Remove old related files
                foreach (DirectoryWatcher directoryWatcher in _directoryWatchers)
                {
                    directoryWatcher._watchedObjsPerFile = null;
                    directoryWatcher._watchedObjsPerDirectory = null;
                }

                // Add watchers
                foreach (WatchedObj watchedObj in watchedObjs)
                {
                    foreach (string file in watchedObj.Files ?? Enumerable.Empty<string>())
                        if (file != null)
                        {
                            var directoryWatcher = GetDirectoryWatcher(PathC.GetDirectoryNameTry(file));
                            if (directoryWatcher != null)
                                directoryWatcher._watchedObjsPerFile = AddToArray(directoryWatcher._watchedObjsPerFile,
                                    new KeyValuePair<string, WatchedObj>(file.ToLowerInvariant(), watchedObj));
                        }
                    foreach (string directory in watchedObj.Directories ?? Enumerable.Empty<string>())
                        if (directory != null)
                        {
                            var directoryWatcher = GetDirectoryWatcher(directory);
                            if (directoryWatcher != null)
                                directoryWatcher._watchedObjsPerDirectory = AddToArray(directoryWatcher._watchedObjsPerDirectory, watchedObj);
                        }
                }

                // Clean up watchers
                _directoryWatchers.RemoveAll((directoryWatcher) =>
                {
                    if ((directoryWatcher._watchedObjsPerFile != null && directoryWatcher._watchedObjsPerFile.Length > 0) ||
                        (directoryWatcher._watchedObjsPerDirectory != null && directoryWatcher._watchedObjsPerDirectory.Length > 0))
                        return false;

                    directoryWatcher._watcher.EnableRaisingEvents = false;
                    directoryWatcher._watcher.Dispose();
                    return true;
                });

                // Remove objects from the list to reload that are no longer necessary.
                lock (_objsToReloadQueue) // No risk of dead locks because it's the only place were we take 2 locks
                    _objsToReloadQueue.RemoveAll(reloadingWatchedObj =>
                    {
                        foreach (WatchedObj newObj in watchedObjs)
                            if (reloadingWatchedObj.Key.IsRepresentingSameObject(newObj))
                                return false;
                        return true;
                    });
                _restartReloadingBecauseListOfFilesChanged = true;
            }
        }

        public void StopReloading()
        {
            lock (_reloadLock) // We lock so that no reload can happen concurrently while we switch it off.
                _temporarilyStoppedReload = true;
        }

        public void RestartReloading()
        {
            _temporarilyStoppedReload = false;
            _threadWorkEvent.Set();
        }

        private DirectoryWatcher GetDirectoryWatcher(string directoryName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(directoryName) || !Directory.Exists(directoryName))
                    return null;

                // Unify directory name
                directoryName = directoryName.ToLowerInvariant();

                // Find directory watcher
                foreach (DirectoryWatcher directoryWatcher in _directoryWatchers)
                    if (directoryWatcher._directory == directoryName)
                        return directoryWatcher;

                // Create new directory watcher
                var newWatcher = new DirectoryWatcher();
                newWatcher._parent = this;
                newWatcher._directory = directoryName;
                newWatcher._watcher = new FileSystemWatcher(directoryName);
                newWatcher._watcher.Deleted += newWatcher.NotificationReceived;
                newWatcher._watcher.Created += newWatcher.NotificationReceived;
                newWatcher._watcher.Changed += newWatcher.NotificationReceived;
                newWatcher._watcher.Renamed += newWatcher.NotificationReceivedRenamed;
                newWatcher._watcher.EnableRaisingEvents = true;
                newWatcher._watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                _directoryWatchers.Add(newWatcher);
                return newWatcher;
            }
            catch (Exception exc)
            {
                logger.Warn(exc, "Unable to create FileSystemWatcher for directory '" + directoryName + "'.");
                return null;
            }
        }

        private static T[] AddToArray<T>(T[] oldArray, T newEntry)
        {
            if (oldArray == null)
                return new T[] { newEntry };
            T[] newArray = new T[oldArray.Length + 1];
            Array.Copy(oldArray, newArray, oldArray.Length);
            newArray[oldArray.Length] = newEntry;
            return newArray;
        }

        private void NotificationReceived(DirectoryWatcher watcher, FileSystemEventArgs e)
        {
            try
            {
                // Gather objects to reload
                string changedFile = e.FullPath.ToLowerInvariant();
                var watchedObjsPerFile = watcher._watchedObjsPerFile;
                var watchedObjsPerDirectory = watcher._watchedObjsPerDirectory;

                // Figure out which objects to reload
                List<WatchedObj> objsToReload = new List<WatchedObj>();
                if (watchedObjsPerDirectory != null)
                    foreach (var watchedDirectory in watchedObjsPerDirectory)
                        if (!objsToReload.Contains(watchedDirectory))
                            objsToReload.Add(watchedDirectory);

                if (watchedObjsPerFile != null)
                    foreach (var watchedFile in watchedObjsPerFile)
                        if (watchedFile.Key == changedFile)
                            if (!objsToReload.Contains(watchedFile.Value))
                                objsToReload.Add(watchedFile.Value);
                if (objsToReload.Count == 0)
                    return;

                // Add objects that need to be reloaded
                long timestamp = Stopwatch.GetTimestamp();
                lock (_objsToReloadQueue)
                {
                    foreach (var watchedObj in objsToReload)
                    {
                        // Create (or find) entry in queue
                        ReloadingObjInfo info;
                        for (int i = 0; i < _objsToReloadQueue.Count; ++i)
                            if(_objsToReloadQueue[i].Key != null && watchedObj != null)
                            if (watchedObj.IsRepresentingSameObject(_objsToReloadQueue[i].Key))
                            {
                                info = _objsToReloadQueue[i].Value;
                                _objsToReloadQueue[i] = new KeyValuePair<WatchedObj, ReloadingObjInfo>(watchedObj, info); // Update the key (the WatchedObj may have changed but compare equals)
                                goto FoundWatchedObj;
                            }
                        _objsToReloadQueue.Add(new KeyValuePair<WatchedObj, ReloadingObjInfo>(watchedObj, info = new ReloadingObjInfo()));
                        FoundWatchedObj:
                        ;

                        // Set timestamp
                        info.ReloadAttemptCount = 0;
                        info._lastChangedFile = e.FullPath;
                        Interlocked.Exchange(ref info.ChangeTimestamp, timestamp);
                    }
                }
                _threadWorkEvent.Set();
            }
            catch (Exception exc)
            {
                logger.Error(exc, "Queueing changed file \"" + e.FullPath + "\" failed.");
            }
        }

        private void ThreadRun()
        {
            try
            {
                do
                {
                    // Wait until there are file changes or we exit.
                    _threadWorkEvent.WaitOne();
                    if (_threadShouldExit)
                        break;

                    // Inner loop we are in as long as there is something to check for reloading.
                    do
                    {
                        _restartReloadingBecauseListOfFilesChanged = false; // No need to interrupt later.

                        // Wait a fixed time (we don't want a busy loop here)
                        // Also reloading immediately is pointless, most likely the file will still be locked because it's written still.
                        Thread.Sleep(_threadCheckInternalInMilliseconds);
                        if (_threadShouldExit || _temporarilyStoppedReload)
                            break;

                        // Figure out which object may need to be reloaded
                        List<KeyValuePair<WatchedObj, ReloadingObjInfo>> objsToReloadQueue;
                        lock (_objsToReloadQueue)
                            objsToReloadQueue = _objsToReloadQueue.ToList();
                        if (objsToReloadQueue.Count == 0)
                            break;

                        // Try reloading
                        long timestamp = Stopwatch.GetTimestamp();
                        var objsDone = new List<KeyValuePair<WatchedObj, ReloadingObjInfo>>();
                        foreach (var objToReload in objsToReloadQueue)
                        {
                            long elapsedTicks = timestamp - Interlocked.Read(ref objToReload.Value.ChangeTimestamp);
                            TimeSpan elapsedTime = new TimeSpan(unchecked((long)(elapsedTicks * _toDataTimeTicks)));
                            if (elapsedTime.TotalMilliseconds < _minWaitTimeInMilliseconds)
                                continue; // Let's skip the file for now, most likely the file will be locked anyway.

                            // Take the lock
                            lock (_reloadLock)
                            {
                                // If we are supposed to finish, stop immediately
                                // We check this inside the lock.
                                if (_threadShouldExit || _temporarilyStoppedReload)
                                    break;
                                if (_restartReloadingBecauseListOfFilesChanged)
                                {
                                    _restartReloadingBecauseListOfFilesChanged = false;
                                    objsDone.Clear();
                                    break;
                                }

                                // Don't waste reload attempts while the file is still being written to.
                                // Keep checking until the file is released, but don't wait longer than the maximum wait time.
                                if (IsFileLocked(objToReload.Value._lastChangedFile))
                                {
                                    if (elapsedTime.TotalMilliseconds < _maxWaitTimeInMilliseconds)
                                        continue;
                                    objsDone.Add(objToReload);
                                    continue;
                                }

                                int attemptIndex = Interlocked.Increment(ref objToReload.Value.ReloadAttemptCount);
                                if (attemptIndex > _minAttempts || elapsedTime.TotalMilliseconds >= _maxWaitTimeInMilliseconds)
                                {
                                    // We have tried it enough times, let's give up.
                                    objsDone.Add(objToReload);
                                    continue;
                                }

                                // Attempt to actually reload (Likely to fail because files may still be locked!)
                                if(objToReload.Key != null)
                                try
                                {
                                    objToReload.Key.TryReload(this, new ReloadArgs { Obj = objToReload.Key, ElapsedTime = elapsedTime, AttemptIndex = attemptIndex });
                                }
                                catch (Exception exc)
                                {
                                    logger.Info(exc, "Reloading of object " + (objToReload.Key.Name == null ? "" : "'" + objToReload.Key.Name + "' ") +
                                        "failed " + elapsedTime.TotalMilliseconds + "ms (" + (attemptIndex + 1) + "th attempt) after it changed. " +
                                        "Perhaps the file is still locked.");
                                }
                            }
                            objsDone.Add(objToReload);
                        }

                        // Remove objects from the list that have been processed.
                        lock (_objsToReloadQueue)
                            foreach (var objDone in objsDone)
                                if (objDone.Value.ChangeTimestamp <= timestamp)
                                    for (int i = _objsToReloadQueue.Count - 1; i >= 0; --i)
                                        if(objDone.Key != null)
                                        {
                                            if (objDone.Key.IsRepresentingSameObject(_objsToReloadQueue[i].Key))
                                                _objsToReloadQueue.RemoveAt(i);
                                        }
                                        else
                                            _objsToReloadQueue.RemoveAt(i);
                    } while (true);
                } while (true);
            }
            finally
            {
                _threadWorkEvent.Dispose();
            }
        }

        private static bool IsFileLocked(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return false;
                // Opening the file, if it files with a specific error code we know that it's locked.
                // If it's locked it's not worth trying to reload anything.
                // https://stackoverflow.com/questions/1304/how-to-check-for-file-lock
                try
                {
                    new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1).Dispose();
                }
                catch (IOException exc)
                {
                    int errorCode = Marshal.GetHRForException(exc) & ((1 << 16) - 1);
                    return errorCode == 32 || errorCode == 33;
                }
                catch (Exception) { }
            }
            catch (Exception exc)
            {
                logger.Error(exc, "'IsFileLocked' failed.");
            }
            return false;
        }
    }
}