using Engine.Media;
using OpenTK.Graphics.ES20;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Engine.Graphics
{
	public sealed class RenderTarget2D : Texture2D
	{
		internal int m_frameBuffer;

		internal int m_depthBuffer;

		public DepthFormat DepthFormat
		{
			get;
			private set;
		}

		public RenderTarget2D(int width, int height, int mipLevelsCount, ColorFormat colorFormat, DepthFormat depthFormat)
			: base(width, height, mipLevelsCount, colorFormat)
		{
			try
			{
				InitializeRenderTarget2D(width, height, mipLevelsCount, colorFormat, depthFormat);
				AllocateRenderTarget();
			}
			catch
			{
				Dispose();
				throw;
			}
		}

		public override void Dispose()
		{
			base.Dispose();
			DeleteRenderTarget();
		}

		public void GetData<T>(T[] target, int targetStartIndex, Rectangle sourceRectangle) where T : struct
		{
			VerifyParametersGetData(target, targetStartIndex, sourceRectangle);
			var gCHandle = GCHandle.Alloc(target, GCHandleType.Pinned);
			try
			{
				GLWrapper.BindFramebuffer(m_frameBuffer);
				GL.ReadPixels(sourceRectangle.Left, sourceRectangle.Top, sourceRectangle.Width, sourceRectangle.Height, PixelFormat.Rgba, PixelType.UnsignedByte, gCHandle.AddrOfPinnedObject());
			}
			finally
			{
				gCHandle.Free();
			}
		}

		public void GenerateMipMaps()
		{
			GLWrapper.BindTexture(All.Texture2D, m_texture, forceBind: false);
			GL.GenerateMipmap(All.Texture2D);
		}

		internal override void HandleDeviceLost()
		{
			DeleteRenderTarget();
		}

		internal override void HandleDeviceReset()
		{
			AllocateRenderTarget();
		}

        private void AllocateRenderTarget()
		{
			GL.GenFramebuffers(1, out m_frameBuffer);
			GLWrapper.BindFramebuffer(m_frameBuffer);
			GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferSlot.ColorAttachment0, TextureTarget.Texture2D, m_texture, 0);
			if (DepthFormat != DepthFormat.None)
			{
				GL.GenRenderbuffers(1, out m_depthBuffer);
				GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, m_depthBuffer);
				GL.RenderbufferStorage(All.Renderbuffer, GLWrapper.TranslateDepthFormat(DepthFormat), base.Width, base.Height);
				GL.FramebufferRenderbuffer(All.Framebuffer, All.DepthAttachment, All.Renderbuffer, m_depthBuffer);
				GL.FramebufferRenderbuffer(All.Framebuffer, All.StencilAttachment, All.Renderbuffer, 0);
			}
			else
			{
				GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferSlot.DepthAttachment, RenderbufferTarget.Renderbuffer, 0);
				GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferSlot.StencilAttachment, RenderbufferTarget.Renderbuffer, 0);
			}
			FramebufferErrorCode framebufferErrorCode = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
			if (framebufferErrorCode != FramebufferErrorCode.FramebufferComplete)
			{
				throw new InvalidOperationException($"Error creating framebuffer ({framebufferErrorCode}).");
			}
		}

		private void DeleteRenderTarget()
		{
			if (m_depthBuffer != 0)
			{
				GL.DeleteRenderbuffers(1, ref m_depthBuffer);
				m_depthBuffer = 0;
			}
			if (m_frameBuffer != 0)
			{
				GLWrapper.DeleteFramebuffer(m_frameBuffer);
				m_frameBuffer = 0;
			}
		}

		public static void Save(RenderTarget2D renderTarget, Stream stream, ImageFileFormat format, bool saveAlpha)
		{
			if (renderTarget.ColorFormat != 0)
			{
				throw new InvalidOperationException("Unsupported color format.");
			}
			var image = new Image(renderTarget.Width, renderTarget.Height);
			renderTarget.GetData(image.Pixels, 0, new Rectangle(0, 0, renderTarget.Width, renderTarget.Height));
			Image.Save(image, stream, format, saveAlpha);
		}

		public override int GetGpuMemoryUsage()
		{
			return base.GetGpuMemoryUsage() + DepthFormat.GetSize() * base.Width * base.Height;
		}

		private void InitializeRenderTarget2D(int width, int height, int mipLevelsCount, ColorFormat colorFormat, DepthFormat depthFormat)
		{
			DepthFormat = depthFormat;
		}

		private void VerifyParametersGetData<T>(T[] target, int targetStartIndex, Rectangle sourceRectangle) where T : struct
		{
			VerifyNotDisposed();
			int size = base.ColorFormat.GetSize();
			int num = Utilities.SizeOf<T>();
			if (target == null)
			{
				throw new ArgumentNullException(nameof(target));
			}
			if (num > size)
			{
				throw new ArgumentNullException("Target array element size is larger than pixel size.");
			}
			if (size % num != 0)
			{
				throw new ArgumentNullException("Pixel size is not an integer multiple of target array element size.");
			}
			if (sourceRectangle.Left < 0 || sourceRectangle.Width <= 0 || sourceRectangle.Top < 0 || sourceRectangle.Height <= 0 || sourceRectangle.Left + sourceRectangle.Width > base.Width || sourceRectangle.Top + sourceRectangle.Height > base.Height)
			{
				throw new ArgumentOutOfRangeException(nameof(sourceRectangle));
			}
			if (targetStartIndex < 0 || targetStartIndex >= target.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(targetStartIndex));
			}
			if ((target.Length - targetStartIndex) * num < sourceRectangle.Width * sourceRectangle.Height * size)
			{
				throw new InvalidOperationException("Not enough space in target array.");
			}
		}
	}
}
