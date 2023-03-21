using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using IntPtr = System.IntPtr;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace Klak.Ndi {

[ExecuteInEditMode]
public sealed partial class NdiReceiver : MonoBehaviour
{
    #region Receiver objects

    Interop.Recv _recv;
    FormatConverter _converter;
    MaterialPropertyBlock _override;

    bool PrepareReceiverObjects()
    {
        if (_recv == null) _recv = RecvHelper.TryCreateRecv(ndiName);
        if (_converter == null) _converter = new FormatConverter(_resources);
        if (_override == null) _override = new MaterialPropertyBlock();
				return _recv != null;
    }

    void ReleaseReceiverObjects()
    {
        _recv?.Dispose();
        _recv = null;

        _converter?.Dispose();
        _converter = null;

        // We don't dispose _override because it's reusable.
    }

    #endregion

    #region Receiver implementation

    RenderTexture TryReceiveFrame()
    {
        if (! PrepareReceiverObjects()) return null;

        // Video frame capturing
        var frameOrNull = RecvHelper.TryCaptureVideoFrame(_recv);
        if (frameOrNull == null) return null;
        var frame = (Interop.VideoFrame)frameOrNull;

        // Pixel format conversion
        var rt = _converter.Decode
          (frame.Width, frame.Height, Util.HasAlpha(frame.FourCC), frame.Data);

        // Metadata retrieval
        if (frame.Metadata != IntPtr.Zero)
            metadata = Marshal.PtrToStringAnsi(frame.Metadata);
        else
            metadata = null;

        // Video frame release
        _recv.FreeVideoFrame(frame);

        return rt;
    }

    #endregion

    #region Component state controller

    internal void Restart() => ReleaseReceiverObjects();

    #endregion

    #region MonoBehaviour implementation

		public bool IsPtzSupported()
				=> PrepareReceiverObjects() && _recv.PtzIsSupported();

		public bool SetZoom(float zoom)
				=> PrepareReceiverObjects() && _recv.PtzZoom(zoom);

		public bool SetZoomSpeed(float zoom_speed)
				=> PrepareReceiverObjects() && _recv.PtzZoomSpeed(zoom_speed);

		public bool SetPanTilt(float pan, float tilt)
				=> PrepareReceiverObjects() && _recv.PtzPanTilt(pan, tilt);

		public bool SetPanTiltSpeed(float pan_speed, float tilt_speed)
				=> PrepareReceiverObjects() && _recv.PtzPanTiltSpeed(pan_speed, tilt_speed);

		public bool StorePtzPreset(int preset)
				=> PrepareReceiverObjects() && _recv.PtzStorePreset(preset);

		public bool RecallPtzPreset(int preset, float speed)
				=> PrepareReceiverObjects() && _recv.PtzRecallPreset(preset, speed);


    void OnDisable() => ReleaseReceiverObjects();

    void Update()
    {
        var rt = TryReceiveFrame();
        if (rt == null) return;

        // Material property override
        if (targetRenderer != null)
        {
            targetRenderer.GetPropertyBlock(_override);
            _override.SetTexture(targetMaterialProperty, rt);
            targetRenderer.SetPropertyBlock(_override);
        }

        // External texture update
        if (targetTexture != null) Graphics.Blit(rt, targetTexture);
    }

    #endregion
}

} // namespace Klak.Ndi
