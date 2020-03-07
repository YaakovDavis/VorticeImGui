using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
using Vortice.Win32;
using VorticeImGui;

namespace VorticeImGui
{
    class AppWindow
    {
        public Win32Window Win32Window;
        public ID3D11Device Device;
        public ID3D11DeviceContext DeviceContext;
        public IDXGISwapChain SwapChain;
        public ID3D11Texture2D BackBuffer;
        public ID3D11RenderTargetView RenderView;

        Format format = Format.R8G8B8A8_UNorm;

        ImGuiRenderer imGuiRenderer;
        ImGuiInputHandler imguiInputHandler;
        Stopwatch stopwatch = Stopwatch.StartNew();
        TimeSpan lastFrameTime;

        IntPtr imGuiContext;

        public AppWindow(Win32Window win32window, ID3D11Device device, ID3D11DeviceContext deviceContext)
        {
            Win32Window = win32window;
            Device = device;
            DeviceContext = deviceContext;

            imGuiContext = ImGui.CreateContext();
            ImGui.SetCurrentContext(imGuiContext);

            imGuiRenderer = new ImGuiRenderer(Device, DeviceContext);
            imguiInputHandler = new ImGuiInputHandler(Win32Window.Handle);

            ImGui.GetIO().DisplaySize = new Vector2(Win32Window.Width, Win32Window.Height);
        }

        public void Show()
        {
            User32.ShowWindow(Win32Window.Handle, ShowWindowCommand.Normal);
        }

        public virtual bool ProcessMessage(uint msg, UIntPtr wParam, IntPtr lParam)
        {
            ImGui.SetCurrentContext(imGuiContext);
            if (imguiInputHandler.ProcessMessage((WindowMessage)msg, wParam, lParam))
                return true;

            switch ((WindowMessage)msg)
            {
                case WindowMessage.Size:
                    switch ((SizeMessage)wParam)
                    {
                        case SizeMessage.SIZE_RESTORED:
                        case SizeMessage.SIZE_MAXIMIZED:
                            Win32Window.IsMinimized = false;

                            var lp = (int)lParam;
                            Win32Window.Width = Utils.Loword(lp);
                            Win32Window.Height = Utils.Hiword(lp);

                            resize();
                            break;
                        case SizeMessage.SIZE_MINIMIZED:
                            Win32Window.IsMinimized = true;
                            break;
                        default:
                            break;
                    }
                    break;
            }

            return false;
        }

        public void UpdateAndDraw()
        {
            UpdateImGui();
            render();
        }

        void resize()
        {
            if (RenderView == null)//first show
            {
                var dxgiFactory = Device.QueryInterface<IDXGIDevice>().GetParent<IDXGIAdapter>().GetParent<IDXGIFactory>();

                var swapchainDesc = new SwapChainDescription()
                {
                    BufferCount = 1,
                    BufferDescription = new ModeDescription(Win32Window.Width, Win32Window.Height, format),
                    IsWindowed = true,
                    OutputWindow = Win32Window.Handle,
                    SampleDescription = new SampleDescription(1, 0),
                    SwapEffect = SwapEffect.Discard,
                    Usage = Vortice.DXGI.Usage.RenderTargetOutput
                };

                SwapChain = dxgiFactory.CreateSwapChain(Device, swapchainDesc);
                dxgiFactory.MakeWindowAssociation(Win32Window.Handle, WindowAssociationFlags.IgnoreAll);

                BackBuffer = SwapChain.GetBuffer<ID3D11Texture2D>(0);
                RenderView = Device.CreateRenderTargetView(BackBuffer);
            }
            else
            {
                RenderView.Dispose();
                BackBuffer.Dispose();

                SwapChain.ResizeBuffers(1, Win32Window.Width, Win32Window.Height, format, SwapChainFlags.None);

                BackBuffer = SwapChain.GetBuffer<ID3D11Texture2D1>(0);
                RenderView = Device.CreateRenderTargetView(BackBuffer);
            }
        }

        public virtual void UpdateImGui()
        {
            ImGui.SetCurrentContext(imGuiContext);
            var io = ImGui.GetIO();

            var now = stopwatch.Elapsed;
            var delta = now - lastFrameTime;
            lastFrameTime = now;
            io.DeltaTime = (float)delta.TotalSeconds;

            imguiInputHandler.Update();

            ImGui.NewFrame();
        }

        void render()
        {
            ImGui.Render();

            var dc = DeviceContext;
            dc.ClearRenderTargetView(RenderView, new Color4(0, 0, 0));
            dc.OMSetRenderTargets(RenderView);
            dc.RSSetViewport(0, 0, Win32Window.Width, Win32Window.Height);

            imGuiRenderer.Render(ImGui.GetDrawData());
            DoRender();

            SwapChain.Present(0, PresentFlags.None);
        }

        public virtual void DoRender() { }
    }
}
