using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vortice.Win32;
using static Vortice.Win32.Kernel32;
using static Vortice.Win32.User32;
using System.Runtime.CompilerServices;

using Vortice.D3DCompiler;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using ImGuiNET;
using System.Diagnostics;
using Vortice.Mathematics;
using System.Numerics;

namespace VorticeImGui
{
    class Program
    {
        const uint PM_REMOVE = 1;

        [STAThread]
        static void Main()
        {
            new Program().Run();
        }

        bool quitRequested;

        Format format = Format.R8G8B8A8_UNorm;
        ImGuiRenderer imGuiRenderer;
        ImGuiInputHandler imguiInputHandler;
        Stopwatch stopwatch = Stopwatch.StartNew();
        TimeSpan lastFrameTime;

        ID3D11Device device;
        ID3D11DeviceContext deviceContext;
        IDXGISwapChain swapChain;
        ID3D11RenderTargetView renderView;

        Window window;

        void Run()
        {
            var moduleHandle = GetModuleHandle(null);

            var wndClass = new WNDCLASSEX
            {
                Size = Unsafe.SizeOf<WNDCLASSEX>(),
                Styles = WindowClassStyles.CS_HREDRAW | WindowClassStyles.CS_VREDRAW | WindowClassStyles.CS_OWNDC,
                WindowProc = WndProc,
                InstanceHandle = moduleHandle,
                CursorHandle = LoadCursor(IntPtr.Zero, SystemCursor.IDC_ARROW),
                BackgroundBrushHandle = IntPtr.Zero,
                IconHandle = IntPtr.Zero,
                ClassName = "WndClass",
            };

            RegisterClassEx(ref wndClass);

            window = new Window(wndClass.ClassName, "Vortice ImGui", 800, 600);
            window.Show();

            InitD3d(window, out device, out deviceContext, out swapChain, out renderView);

            InitImGui();
            
            MainLoop();
        }

        void MainLoop()
        {
            while (!quitRequested)
            {
                if (PeekMessage(out var msg, IntPtr.Zero, 0, 0, PM_REMOVE))
                {
                    TranslateMessage(ref msg);
                    DispatchMessage(ref msg);

                    if (msg.Value == (uint)WindowMessage.Quit)
                    {
                        quitRequested = true;
                        break;
                    }
                }

                OnFrame();
            }
        }

        void InitImGui()
        {
            ImGui.CreateContext();
            ImGuiInputHandler.InitKeyMap();

            imGuiRenderer = new ImGuiRenderer(device, deviceContext);
            imguiInputHandler = new ImGuiInputHandler(window);

            var io = ImGui.GetIO();
            io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors;
            io.BackendFlags |= ImGuiBackendFlags.HasSetMousePos;
            io.ImeWindowHandle = window.Handle;
            io.DisplaySize = new Vector2(window.Width, window.Height);
        }

        void OnFrame()
        {
            Update();
            Draw();
        }

        void Update()
        {
            //...
            UpdateImGui();
            //...
        }

        void UpdateImGui()
        {
            var io = ImGui.GetIO();

            var now = stopwatch.Elapsed;
            var delta = now - lastFrameTime;
            lastFrameTime = now;
            io.DeltaTime = (float)delta.TotalSeconds;

            imguiInputHandler.Update();

            ImGui.NewFrame();
            DoUILayout();
        }

        void DoUILayout()
        {
            ImGui.ShowDemoWindow();
        }

        void Draw()
        {
            ImGui.Render();

            deviceContext.OMSetRenderTargets(renderView);
            deviceContext.ClearRenderTargetView(renderView, new Color4(0, 0, 0));
            
            //...
            imGuiRenderer.Render(ImGui.GetDrawData());
            //...
            
            swapChain.Present(0, PresentFlags.None);
        }

        void InitD3d(Window window, out ID3D11Device device, out ID3D11DeviceContext deviceContext, out IDXGISwapChain swapChain, out ID3D11RenderTargetView renderView)
        {
            D3D11.D3D11CreateDevice(null, DriverType.Hardware, DeviceCreationFlags.None, null, out device, out deviceContext);

            var dxgiFactory = device.QueryInterface<IDXGIDevice>().GetParent<IDXGIAdapter>().GetParent<IDXGIFactory>();

            var swapchainDesc = new SwapChainDescription()
            {
                BufferCount = 1,
                BufferDescription = new ModeDescription(window.Width, window.Height, format),
                IsWindowed = true,
                OutputWindow = window.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Vortice.DXGI.Usage.RenderTargetOutput
            };

            swapChain = dxgiFactory.CreateSwapChain(device, swapchainDesc);
            dxgiFactory.MakeWindowAssociation(window.Handle, WindowAssociationFlags.IgnoreAll);

            var backBuffer = swapChain.GetBuffer<ID3D11Texture2D>(0);
            renderView = device.CreateRenderTargetView(backBuffer);
        }


        IntPtr WndProc(IntPtr hWnd, uint msg, UIntPtr wParam, IntPtr lParam)
        {
            if (imguiInputHandler?.ProcessMessage(hWnd, (WindowMessage)msg, wParam, lParam) ?? false)
                return IntPtr.Zero;

            //switch ((WindowMessage)msg)
            //{
            //    case WindowMessage.Destroy:
            //        PostQuitMessage(0);
            //        break;
            //}

            return DefWindowProc(hWnd, msg, wParam, lParam);
        }
    }
}
