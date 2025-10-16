﻿using System.Diagnostics;
using System.IO.Pipes;
using FFMpegCore.Pipes;

namespace FFMpegCore.Arguments;

public abstract class PipeArgument
{
    private readonly PipeDirection _direction;

    protected PipeArgument(PipeDirection direction)
    {
        PipeName = PipeHelpers.GetUnqiuePipeName();
        _direction = direction;
    }

    private string PipeName { get; }
    public string PipePath => PipeHelpers.GetPipePath(PipeName);

    protected NamedPipeServerStream Pipe { get; private set; } = null!;
    public abstract string Text { get; }

    public void Pre()
    {
        if (Pipe != null)
        {
            throw new InvalidOperationException("Pipe already has been opened");
        }

        Pipe = new NamedPipeServerStream(PipeName, _direction, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
    }

    public void Post()
    {
        Debug.WriteLine($"Disposing NamedPipeServerStream on {GetType().Name}");
        Pipe?.Dispose();
        Pipe = null!;
    }

    public async Task During(CancellationToken cancellationToken = default)
    {
        try
        {
            await ProcessDataAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            Debug.WriteLine($"ProcessDataAsync on {GetType().Name} cancelled");
        }
        finally
        {
            Debug.WriteLine($"Disconnecting NamedPipeServerStream on {GetType().Name}");
            if (Pipe is { IsConnected: true })
            {
                Pipe.Disconnect();
            }
        }
    }

    protected abstract Task ProcessDataAsync(CancellationToken token);
}
