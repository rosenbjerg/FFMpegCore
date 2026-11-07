namespace FFMpegCore.Enums;

public readonly struct HardwareAccelerationDevice
{
    private HardwareAccelerationDevice(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Value { get; }

    public static HardwareAccelerationDevice Auto => new("auto");
    public static HardwareAccelerationDevice VideoToolbox => new("videotoolbox");
    public static HardwareAccelerationDevice CUDA => new("cuda");
    public static HardwareAccelerationDevice QSV => new("qsv");
    public static HardwareAccelerationDevice VAAPI => new("vaapi");
    public static HardwareAccelerationDevice VDPAU => new("vdpau");
    public static HardwareAccelerationDevice D3D11VA => new("d3d11va");
    public static HardwareAccelerationDevice D3D12VA => new("d3d12va");
    public static HardwareAccelerationDevice DXVA2 => new("dxva2");
    public static HardwareAccelerationDevice Vulkan => new("vulkan");
    public static HardwareAccelerationDevice OpenCL => new("opencl");
    public static HardwareAccelerationDevice DRM => new("drm");

    public static implicit operator HardwareAccelerationDevice(string value)
    {
        return new HardwareAccelerationDevice(value);
    }

    public override string ToString()
    {
        return Value;
    }
}
