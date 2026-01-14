using System.Runtime.CompilerServices;
using Grpc.Core;
using MusicForge.Infrastructure.Grpc;
using App = MusicForge.Application.Interfaces;

namespace MusicForge.Infrastructure.Grpc;

/// <summary>
/// Implementation of IMusicWorkerClient using gRPC.
/// </summary>
public class GrpcMusicWorkerClient(MusicWorker.MusicWorkerClient client) : App.IMusicWorkerClient
{
    public async Task<App.TheoryResult> GenerateTheoryAsync(App.TheoryRequest request, CancellationToken ct = default)
    {
        var grpcReq = new Grpc.TheoryRequest
        {
            Genre = request.Genre,
            Mood = request.Mood,
            TempoBpm = request.TempoBpm,
            Key = request.Key,
            Mode = request.Mode,
            DurationSeconds = request.DurationSeconds
        };

        if (request.StyleTags != null)
        {
            grpcReq.StyleTags.AddRange(request.StyleTags);
        }

        var response = await client.GenerateTheoryAsync(grpcReq, cancellationToken: ct);

        return new App.TheoryResult(
            ChordProgression: response.ChordProgression.ToList(),
            Sections: response.Sections.Select(s => new App.SectionData(
                Name: s.Name,
                StartBar: s.StartBar,
                DurationBars: s.DurationBars,
                EnergyLevel: s.EnergyLevel,
                Elements: s.Elements.ToList()
            )).ToList(),
            MidiData: response.MidiData.ToByteArray()
        );
    }

    public async IAsyncEnumerable<App.AudioChunk> SynthesizeAudioAsync(App.AudioRequest request, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var grpcReq = new Grpc.AudioRequest
        {
            Prompt = request.Prompt,
            DurationSeconds = request.DurationSeconds,
            Genre = request.Genre,
            EnergyLevel = request.EnergyLevel,
            SectionName = request.SectionName
        };

        if (request.ConditioningAudio != null)
        {
            grpcReq.ConditioningAudio = Google.Protobuf.ByteString.CopyFrom(request.ConditioningAudio);
        }

        using var call = client.SynthesizeAudio(grpcReq, cancellationToken: ct);

        while (await call.ResponseStream.MoveNext(ct))
        {
            var chunk = call.ResponseStream.Current;
            yield return new App.AudioChunk(
                AudioData: chunk.AudioData.ToByteArray(),
                SampleRate: chunk.SampleRate,
                IsFinal: chunk.IsFinal,
                Progress: chunk.Progress
            );
        }
    }

    public async IAsyncEnumerable<App.AudioChunk> SynthesizeVocalsAsync(App.VocalRequest request, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var grpcReq = new Grpc.VocalRequest
        {
            Lyrics = request.Lyrics,
            VoiceType = request.VoiceType,
            Style = request.Style,
            TargetDurationMs = request.TargetDurationMs
        };

        using var call = client.SynthesizeVocals(grpcReq, cancellationToken: ct);

        while (await call.ResponseStream.MoveNext(ct))
        {
            var chunk = call.ResponseStream.Current;
            yield return new App.AudioChunk(
                AudioData: chunk.AudioData.ToByteArray(),
                SampleRate: chunk.SampleRate,
                IsFinal: chunk.IsFinal,
                Progress: chunk.Progress
            );
        }
    }

    public async Task<App.StemSeparationResult> SeparateStemsAsync(byte[] audioData, CancellationToken ct = default)
    {
        var grpcReq = new Grpc.StemRequest
        {
            AudioData = Google.Protobuf.ByteString.CopyFrom(audioData)
            // request.SampleRate if needed, protocol has it but IMusicWorkerClient signature doesn't pass it currently
            // assuming default or metadata
        };

        var response = await client.SeparateStemsAsync(grpcReq, cancellationToken: ct);

        return new App.StemSeparationResult(
            Drums: response.Drums.ToByteArray(),
            Bass: response.Bass.ToByteArray(),
            Vocals: response.Vocals.ToByteArray(),
            Other: response.Other.ToByteArray(),
            SampleRate: response.SampleRate
        );
    }

    public async Task<App.HealthStatus> HealthCheckAsync(CancellationToken ct = default)
    {
        var response = await client.HealthCheckAsync(new Grpc.Empty(), cancellationToken: ct);

        return new App.HealthStatus(
            Status: response.Status,
            GpuAvailable: response.GpuAvailable,
            GpuMemoryBytes: response.GpuMemoryBytes,
            ModelsLoaded: response.ModelsLoaded.ToList()
        );
    }
}
