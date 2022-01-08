using Engine;
using Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Net;
namespace Game
{
    public static class Program
    {
        public static double m_frameBeginTime;

        public static double m_cpuEndTime;

        public static List<Uri> m_urisToHandle = new List<Uri>();

        public static float LastFrameTime
        {
            get;
            set;
        }

        public static float LastCpuFrameTime
        {
            get;
            set;
        }

        public static event Action<Uri> HandleUri;
        [STAThread]
        public static void Main()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            Log.RemoveAllLogSinks();
            Log.AddLogSink(new GameLogSink());
            Window.HandleUri += HandleUriHandler;
            Window.Deactivated += DeactivatedHandler;
            Window.Frame += FrameHandler;
            Display.DeviceReset += ContentManager.Display_DeviceReset;
            Window.UnhandledException += delegate (UnhandledExceptionInfo e)
            {
                ExceptionManager.ReportExceptionToUser("Unhandled exception.", e.Exception);
                e.IsHandled = true;
            };
            Window.Run(1920, 1080, WindowMode.Resizable, "����ս��2.2�����V" + ModsManager.APIVersion + "beta");
        }

        public static void HandleUriHandler(Uri uri)
        {
            m_urisToHandle.Add(uri);
        }

        public static void DeactivatedHandler()
        {
            GC.Collect();
        }

        public static void FrameHandler()
        {
            if (Time.FrameIndex < 0)
            {
                Display.Clear(Vector4.Zero, 1f);
            }
            else if (Time.FrameIndex == 0)
            {
                Initialize();
            }
            else
            {
                Run();
            }
        }

        public static void Initialize()
        {
            Log.Information($"Survivalcraft starting up at {DateTime.Now}, Version={VersionsManager.Version}, BuildConfiguration={VersionsManager.BuildConfiguration}, Platform={VersionsManager.Platform}, Storage.AvailableFreeSpace={Storage.FreeSpace / 1024 / 1024}MB, ApproximateScreenDpi={ScreenResolutionManager.ApproximateScreenDpi:0.0}, ApproxScreenInches={ScreenResolutionManager.ApproximateScreenInches:0.0}, ScreenResolution={Window.Size}, ProcessorsCount={Environment.ProcessorCount}, RAM={Utilities.GetTotalAvailableMemory() / 1024 / 1024}MB, 64bit={Marshal.SizeOf<IntPtr>() == 8}");
            try
            {
                SettingsManager.Initialize();
                VersionsManager.Initialize();
                ExternalContentManager.Initialize();                
                ScreensManager.Initialize();
                Log.Information("Program Initialize Success");
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }
        }

        public static void Run()
        {
            LastFrameTime = (float)(Time.RealTime - m_frameBeginTime);
            LastCpuFrameTime = (float)(m_cpuEndTime - m_frameBeginTime);
            m_frameBeginTime = Time.RealTime;
            if (Engine.Input.Keyboard.IsKeyDown(Engine.Input.Key.F11))
            {
                SettingsManager.WindowMode = SettingsManager.WindowMode == WindowMode.Fullscreen ? WindowMode.Resizable : WindowMode.Fullscreen;
            }            
            try
            {
                if (ExceptionManager.Error == null)
                {
                    while (m_urisToHandle.Count > 0)
                    {
                        Uri obj = m_urisToHandle[0];
                        m_urisToHandle.RemoveAt(0);
                        HandleUri?.Invoke(obj);
                    }
                    PerformanceManager.Update();
                    MotdManager.Update();
                    MusicManager.Update();
                    ScreensManager.Update();
                    DialogsManager.Update();
                }
                else
                {
                    ExceptionManager.UpdateExceptionScreen();
                }
            }
            catch (Exception e)
            {
                ModsManager.AddException(e);
                ScreensManager.SwitchScreen("MainMenu");
            }
            try
            {
                Display.RenderTarget = null;
                if (ExceptionManager.Error == null)
                {
                    ScreensManager.Draw();
                    PerformanceManager.Draw();
                    ScreenCaptureManager.Run();
                }
                else
                {
                    ExceptionManager.DrawExceptionScreen();
                }
                m_cpuEndTime = Time.RealTime;
            }
            catch (Exception e2)
            {
                if (GameManager.Project != null) GameManager.DisposeProject();
                ExceptionManager.ReportExceptionToUser(null, e2);
                ScreensManager.SwitchScreen("MainMenu");
            }
        }
    }
}
