using System.Text.Json.Serialization;

namespace FFMpegCore;

[JsonSerializable(typeof(FFProbeAnalysis))]
[JsonSerializable(typeof(FFProbeFrames))]
[JsonSerializable(typeof(FFProbePackets))]
[JsonSerializable(typeof(FFOptions))]
public partial class JsonContext : JsonSerializerContext { }
