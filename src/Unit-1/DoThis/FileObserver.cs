﻿using Akka.Actor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WinTail
{
    public class FileObserver :IDisposable
    {
        private readonly IActorRef _tailActor;
        private readonly string _absoluteFilePath;
        private FileSystemWatcher _watcher;
        private readonly string _fileDir;
        private readonly string _fileNameOnly;

        public FileObserver(IActorRef tailActor, string absoluteFilePath)
        {
            _tailActor = tailActor;
            _absoluteFilePath = absoluteFilePath;
            _fileDir = Path.GetDirectoryName(_absoluteFilePath);
            _fileNameOnly = Path.GetFileName(_absoluteFilePath);
        }

        /// <summary>
        /// Begin monitoring file
        /// </summary>
        public void Start()
        {
            //watcher to observer specific file
            _watcher = new FileSystemWatcher(_fileDir, _fileNameOnly);

            //Watch our file for changes to the file name or new messages being written to the file
            _watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;

            //assign callback for the event types
            _watcher.Changed += OnFileChanged;
            _watcher.Error += OnFileError;

            //start watching
            _watcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Stop monitoring file
        /// </summary>
        public void Dispose()
        {
            _watcher.Dispose();
        }

        /// <summary>
        /// Callback for <see cref="FileSystemWatcher"/> file error events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnFileError(object sender, ErrorEventArgs e)
        {
            _tailActor.Tell(new TailActor.FileError(_fileNameOnly,
                e.GetException().Message),
                ActorRefs.NoSender);
        }

        /// <summary>
        /// Callback for <see cref="FileSystemWatcher"/> file change events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                // here we use a special ActorRefs.NoSender
                // since this event can happen many times,
                // this is a little microoptimization
                _tailActor.Tell(new TailActor.FileWrite(e.Name), ActorRefs.NoSender);
            }
        }
    }
}
