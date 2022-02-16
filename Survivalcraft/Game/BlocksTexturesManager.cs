using System;
using System.Collections.Generic;
using System.IO;
using Engine;
using Engine.Graphics;
using Engine.Media;

namespace Game
{
	public static class BlocksTexturesManager
	{
		private static List<string> m_blockTextureNames = new List<string>();

		public static Texture2D DefaultBlocksTexture { get; private set; }

		public static ReadOnlyList<string> BlockTexturesNames => new ReadOnlyList<string>(m_blockTextureNames);

		private static string BlockTexturesDirectoryName => "data:/TexturePacks";

		public static event Action<string> BlocksTextureDeleted;

		public static void Initialize()
		{
			Storage.CreateDirectory(BlockTexturesDirectoryName);
			DefaultBlocksTexture = ContentManager.Get<Texture2D>("Textures/Blocks");
		}

		public static bool IsBuiltIn(string name)
		{
			return string.IsNullOrEmpty(name);
		}

		public static string GetFileName(string name)
		{
			if (IsBuiltIn(name))
			{
				return null;
			}
			return Storage.CombinePaths(BlockTexturesDirectoryName, name);
		}

		public static string GetDisplayName(string name)
		{
			if (IsBuiltIn(name))
			{
				return "Survivalcraft";
			}
			return Storage.GetFileNameWithoutExtension(name);
		}

		public static DateTime GetCreationDate(string name)
		{
			try
			{
				if (!IsBuiltIn(name))
				{
					return Storage.GetFileLastWriteTime(GetFileName(name));
				}
			}
			catch
			{
			}
			return new DateTime(2000, 1, 1);
		}

		public static Texture2D LoadTexture(string name)
		{
			Texture2D texture2D = null;
			if (!IsBuiltIn(name))
			{
				try
				{
					Image image = Image.Load(GetFileName(name));
					ValidateBlocksTexture(image);
					texture2D = Texture2D.Load(image);
					texture2D.Tag = image;
				}
				catch (Exception ex)
				{
					Log.Warning(string.Format("Could not load blocks texture \"{0}\". Reason: {1}.", new object[2] { name, ex.Message }));
				}
			}
			if (texture2D == null)
			{
				texture2D = DefaultBlocksTexture;
			}
			return texture2D;
		}

		public static string ImportBlocksTexture(string name, Stream stream)
		{
			Exception ex = ExternalContentManager.VerifyExternalContentName(name);
			if (ex != null)
			{
				throw ex;
			}
			if (Storage.GetExtension(name) != ".scbtex")
			{
				name += ".scbtex";
			}
			Image image = Image.Load(stream);
			stream.Position = 0L;
			ValidateBlocksTexture(image);
			using (Stream destination = Storage.OpenFile(GetFileName(name), OpenFileMode.Create))
			{
				stream.CopyTo(destination);
				return name;
			}
		}

		public static void DeleteBlocksTexture(string name)
		{
			try
			{
				string fileName = GetFileName(name);
				if (!string.IsNullOrEmpty(fileName))
				{
					Storage.DeleteFile(fileName);
					BlocksTexturesManager.BlocksTextureDeleted?.Invoke(name);
				}
			}
			catch (Exception e)
			{
				ExceptionManager.ReportExceptionToUser($"Unable to delete blocks texture \"{name}\"", e);
			}
		}

		public static void UpdateBlocksTexturesList()
		{
			m_blockTextureNames.Clear();
			m_blockTextureNames.Add(string.Empty);
			foreach (string item in Storage.ListFileNames(BlockTexturesDirectoryName))
			{
				m_blockTextureNames.Add(item);
			}
		}

		private static void ValidateBlocksTexture(Image image)
		{
			if (image.Width > 1024 || image.Height > 1024)
			{
				throw new InvalidOperationException(string.Format("Blocks texture is larger than 1024x1024 pixels (size={0}x{1})", new object[2] { image.Width, image.Height }));
			}
			if (!MathUtils.IsPowerOf2(image.Width) || !MathUtils.IsPowerOf2(image.Height))
			{
				throw new InvalidOperationException(string.Format("Blocks texture does not have power-of-two size (size={0}x{1})", new object[2] { image.Width, image.Height }));
			}
		}
	}
}
