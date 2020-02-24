# VorticeImGui

This code demostrates how to use [ImGui.Net](https://github.com/mellinoe/ImGui.NET) with [Vortice](https://github.com/amerkoleci/Vortice.Windows).

* Uses .Net Core 3.1.   
* Only dependencies are Vortice and ImGui. Input & windowing is handled using the Win32 API; can be trivially replaced to other APIs, like SDL.

Add your logic to `Update`, `DoUILayout` and `Draw`.  
