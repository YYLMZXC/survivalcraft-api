﻿using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Game
{
	public class DiskExternalContentProvider : IExternalContentProvider
	{
		public string DisplayName => LanguageControl.Get(fName, "DisplayName");

		public bool SupportsLinks => false;

		public bool SupportsListing => true;

		public bool RequiresLogin => false;

		public bool IsLoggedIn => true;

		public static string fName = "DiskExternalContentProvider";

		public static string LocalPath = AppDomain.CurrentDomain.BaseDirectory;

		public string Description => LanguageControl.Get(fName, "Description");

		public DiskExternalContentProvider()
		{
			if (!Directory.Exists(LocalPath))
			{
				Directory.CreateDirectory(LocalPath);
			}
		}
		public void Dispose()
		{
		}

		public void Download(string path, CancellableProgress progress, Action<Stream> success, Action<Exception> failure)
		{
			FileStream fileStream = null;
			if (!File.Exists(path))
			{
				failure(new FileNotFoundException());
				return;
			}
			else fileStream = File.OpenRead(path);
			Exception e = default;
			ThreadPool.QueueUserWorkItem(delegate
			{
				try
				{
					success(fileStream);
				}
				catch (Exception ex)
				{
					failure(ex);
				}
			});
		}

		public void Link(string path, CancellableProgress progress, Action<string> success, Action<Exception> failure)
		{
			failure(new NotSupportedException());
		}

		public void List(string path, CancellableProgress progress, Action<ExternalContentEntry> success, Action<Exception> failure)
		{
			ExternalContentEntry entry = default;
			Exception e = default;
			ThreadPool.QueueUserWorkItem(delegate
			{
				try
				{
					string internalPath = path;
					entry = GetDirectoryEntry(internalPath, scanContents: true);
					success(entry);
				}
				catch (Exception ex)
				{
					failure(ex);
				}
			});
		}

		public void Login(CancellableProgress progress, Action success, Action<Exception> failure)
		{
			failure(new NotSupportedException());
		}

		public void Logout()
		{
			throw new NotSupportedException();
		}

		public void Upload(string path, Stream stream, CancellableProgress progress, Action<string> success, Action<Exception> failure)
		{
			var saveFileDialog1 = new SaveFileDialog();
			try
			{
				FileStream fileStream = null;
				string nPath = Path.Combine(LocalPath, path);
				fileStream = !File.Exists(nPath) ? File.Create(nPath) : File.OpenWrite(nPath);
				stream.CopyTo(fileStream);
				fileStream.Close();
				success(null);
			}
			catch (Exception e)
			{
				failure(e);
			}
		}
		private ExternalContentEntry GetDirectoryEntry(string internalPath, bool scanContents)
		{
			var externalContentEntry = new ExternalContentEntry();
			externalContentEntry.Type = ExternalContentType.Directory;
			externalContentEntry.Path = internalPath;
			externalContentEntry.Time = new DateTime(1970, 1, 1);
			if (scanContents)
			{
				string[] directories = Directory.GetDirectories(internalPath);
				foreach (string internalPath2 in directories)
				{
					externalContentEntry.ChildEntries.Add(GetDirectoryEntry(internalPath2, scanContents: false));
				}
				directories = Directory.GetFiles(internalPath);
				foreach (string text in directories)
				{
					var fileInfo = new FileInfo(text);
					var externalContentEntry2 = new ExternalContentEntry();
					externalContentEntry2.Type = ExternalContentManager.ExtensionToType(Path.GetExtension(text));
					externalContentEntry2.Path = text;
					externalContentEntry2.Size = fileInfo.Length;
					externalContentEntry2.Time = fileInfo.CreationTime;
					externalContentEntry.ChildEntries.Add(externalContentEntry2);
				}
			}
			return externalContentEntry;
		}

	}
}
