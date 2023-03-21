using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Klak.Ndi.Interop {

// Bandwidth enumeration (equivalent to NDIlib_recv_bandwidth_e)
public enum Bandwidth
{
    MetadataOnly = -10,
    AudioOnly = 10,
    Lowest = 0,
    Highest = 100
}

// Color format enumeration (equivalent to NDIlib_recv_color_format_e)
public enum ColorFormat
{
    BGRX_BGRA = 0,
    UYVY_BGRA = 1,
    RGBX_RGBA = 2,
    UYVY_RGBA = 3,
    BGRX_BGRA_Flipped = 200,
    Fastest = 100,
    Best = 101
}

public class Recv : SafeHandleZeroOrMinusOneIsInvalid
{
    #region SafeHandle implementation

    Recv() : base(true) {}

    protected override bool ReleaseHandle()
    {
        _Destroy(handle);
        return true;
    }

    #endregion

    #region Public methods

    public static Recv Create(in Settings settings)
      => _Create(settings);

    public FrameType Capture
      (out VideoFrame video, IntPtr audio, IntPtr metadata, uint timeout)
      => _Capture(this, out video, audio, metadata, timeout);

    public void FreeVideoFrame(in VideoFrame frame)
      => _FreeVideo(this, frame);

    public bool SetTally(in Tally tally)
      => _SetTally(this, tally);

    public bool PtzIsSupported()
				=> _PtzIsSupported(this);

    public bool PtzZoom(float zoom)
				=> _PtzZoom(this, zoom);

    public bool PtzZoomSpeed(float zoom_speed)
				=> _PtzZoomSpeed(this, zoom_speed);

    public bool PtzPanTilt(float pan, float tilt)
				=> _PtzPanTilt(this, pan, tilt);

    public bool PtzPanTiltSpeed(float pan_speed, float tilt_speed)
				=> _PtzPanTiltSpeed(this, pan_speed, tilt_speed);

		public bool PtzStorePreset(int preset)
				=> _PtzStorePreset(this, preset);

		public bool PtzRecallPreset(int preset, float speed)
				=> _PtzRecallPreset(this, preset, speed);

    #endregion

    #region Unmanaged interface

    // Constructor options (equivalent to NDIlib_recv_create_v3_t)
    [StructLayout(LayoutKind.Sequential)]
    public struct Settings
    {
        public Source Source;
        public ColorFormat ColorFormat;
        public Bandwidth Bandwidth;
        [MarshalAs(UnmanagedType.U1)] public bool AllowVideoFields;
        public IntPtr Name;
    }

    [DllImport(Config.DllName, EntryPoint = "NDIlib_recv_create_v3")]
    static extern Recv _Create(in Settings Settings);

    [DllImport(Config.DllName, EntryPoint = "NDIlib_recv_destroy")]
    static extern void _Destroy(IntPtr recv);

    [DllImport(Config.DllName, EntryPoint = "NDIlib_recv_capture_v2")]
    static extern FrameType _Capture
      (Recv recv, out VideoFrame video,
       IntPtr audio, IntPtr metadata, uint timeout);

    [DllImport(Config.DllName, EntryPoint = "NDIlib_recv_free_video_v2")]
    static extern void _FreeVideo(Recv recv, in VideoFrame data);

    [DllImport(Config.DllName, EntryPoint = "NDIlib_recv_set_tally")]
    [return: MarshalAs(UnmanagedType.U1)]
    static extern bool _SetTally(Recv recv, in Tally tally);

		[DllImport(Config.DllName, EntryPoint = "NDIlib_recv_ptz_is_supported")]
    [return: MarshalAs(UnmanagedType.U1)]
    static extern bool _PtzIsSupported(Recv recv);

		[DllImport(Config.DllName, EntryPoint = "NDIlib_recv_ptz_zoom")]
    [return: MarshalAs(UnmanagedType.U1)]
    static extern bool _PtzZoom(Recv recv, float zoom);

		[DllImport(Config.DllName, EntryPoint = "NDIlib_recv_ptz_zoom_speed")]
    [return: MarshalAs(UnmanagedType.U1)]
    static extern bool _PtzZoomSpeed(Recv recv, float zoom_speed);

		[DllImport(Config.DllName, EntryPoint = "NDIlib_recv_ptz_pan_tilt")]
    [return: MarshalAs(UnmanagedType.U1)]
    static extern bool _PtzPanTilt(Recv recv, float pan, float tilt);

		[DllImport(Config.DllName, EntryPoint = "NDIlib_recv_ptz_pan_tilt_speed")]
    [return: MarshalAs(UnmanagedType.U1)]
    static extern bool _PtzPanTiltSpeed(Recv recv, float pan_speed, float tilt_speed);

		[DllImport(Config.DllName, EntryPoint = "NDIlib_recv_ptz_store_preset")]
    [return: MarshalAs(UnmanagedType.U1)]
		static extern bool _PtzStorePreset(Recv recv, int preset);

		[DllImport(Config.DllName, EntryPoint = "NDIlib_recv_ptz_recall_preset")]
    [return: MarshalAs(UnmanagedType.U1)]
		static extern bool _PtzRecallPreset(Recv recv, int preset, float speed);

#endregion
}

} // namespace Klak.Ndi.Interop
