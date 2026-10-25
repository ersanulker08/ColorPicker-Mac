using System;
using System.Runtime.InteropServices;
using System.Threading;

while (true)
{
    IntPtr eventRef = Native.CGEventCreate(IntPtr.Zero);
    CGPoint location = Native.CGEventGetLocation(eventRef);
    Native.CFRelease(eventRef);

    Console.WriteLine($"X: {location.X}, Y: {location.Y}");

    if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Spacebar)
    {
        Console.WriteLine("Renk okuma başlıyor...");
        Console.WriteLine($"Ekran izni: {Native.CGPreflightScreenCaptureAccess()}");

        var (r, g, b) = Native.ReadPixel(location.X, location.Y);
        Console.WriteLine($"Renk: R={r}, G={g}, B={b}");
        break;
    }

    Thread.Sleep(500);
}

[StructLayout(LayoutKind.Sequential)]
public struct CGPoint
{
    public double X;
    public double Y;
}

[StructLayout(LayoutKind.Sequential)]
public struct CGSize
{
    public double Width;
    public double Height;
}

[StructLayout(LayoutKind.Sequential)]
public struct CGRect
{
    public CGPoint Origin;
    public CGSize Size;
}

static class Native
{
    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    public static extern IntPtr CGEventCreate(IntPtr source);

    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    public static extern CGPoint CGEventGetLocation(IntPtr evt);

    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    public static extern bool CGPreflightScreenCaptureAccess();

    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    public static extern uint CGMainDisplayID();

    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    public static extern IntPtr CGDisplayCreateImageForRect(uint displayId, CGRect rect);

    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    public static extern IntPtr CGImageGetDataProvider(IntPtr image);

    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    public static extern IntPtr CGDataProviderCopyData(IntPtr provider);

    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    public static extern void CGImageRelease(IntPtr image);

    [DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
    public static extern IntPtr CFDataGetBytePtr(IntPtr data);

    [DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
    public static extern void CFRelease(IntPtr obj);

    public static (byte r, byte g, byte b) ReadPixel(double x, double y)
    {
        CGRect area = new CGRect
        {
            Origin = new CGPoint { X = x, Y = y },
            Size = new CGSize { Width = 1, Height = 1 }
        };

        IntPtr imageRef = CGDisplayCreateImageForRect(CGMainDisplayID(), area);
        if (imageRef == IntPtr.Zero) return (0, 0, 0);

        IntPtr dataProvider = CGImageGetDataProvider(imageRef); // we don't own this, so we don't release it
        IntPtr data = CGDataProviderCopyData(dataProvider);     // we own this, so we must release it
        IntPtr bytePtr = CFDataGetBytePtr(data);

        // macOS usually stores pixels in BGRA order
        byte b = Marshal.ReadByte(bytePtr, 0);
        byte g = Marshal.ReadByte(bytePtr, 1);
        byte r = Marshal.ReadByte(bytePtr, 2);

        CFRelease(data);
        CGImageRelease(imageRef);

        return (r, g, b);
    }
}