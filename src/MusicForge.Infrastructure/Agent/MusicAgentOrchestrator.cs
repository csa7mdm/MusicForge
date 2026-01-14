using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MusicForge.Application.Interfaces;
using MusicForge.Domain.Entities;
using MusicForge.Domain.Enums;

namespace MusicForge.Infrastructure.Agent;

/// <summary>
/// LLM-powered orchestrator for intelligent music generation.
/// </summary>
public sealed class MusicAgentOrchestrator : IAgentOrchestrator
{
    private readonly ILlmClientFactory _llmFactory;
    private readonly IMusicWorkerClient? _workerClient;
    private readonly IProjectRepository _repository;
    private readonly ILogger<MusicAgentOrchestrator> _logger;
    private readonly Dictionary<Guid, ProgressState> _progressStates = new();

    public MusicAgentOrchestrator(
        ILlmClientFactory llmFactory,
        IProjectRepository repository,
        ILogger<MusicAgentOrchestrator> logger,
        IMusicWorkerClient? workerClient = null)
    {
        _llmFactory = llmFactory;
        _workerClient = workerClient;
        _repository = repository;
        _logger = logger;
    }

    public async Task<GenerationResult> GenerateAsync(
        Project project,
        string prompt,
        CancellationToken ct = default)
    {
        var progressState = new ProgressState();
        _progressStates[project.Id.Value] = progressState;

        try
        {
            _logger.LogInformation("Starting generation for project {ProjectId}", project.Id.Value);

            // Phase 1: Analyze and plan
            progressState.Update(AgentState.Understanding, "Understanding request", 0.1f);
            var plan = await AnalyzeRequestAsync(project, prompt, ct);

            // Phase 2: Generate music theory
            progressState.Update(AgentState.Composing, "Creating chord progression", 0.2f);
            var theory = await GenerateTheoryAsync(project, plan, ct);
            project.Context.AddFocusCheckpoint($"Theory: {string.Join("-", theory.Chords)}");

            // Phase 3: Generate audio for each section
            progressState.Update(AgentState.SynthesizingAudio, "Generating audio", 0.4f);
            var stems = await GenerateAudioAsync(project, theory, progressState, ct);

            // Phase 4: Mix and master
            progressState.Update(AgentState.Mastering, "Mixing final track", 0.9f);
            var masterPath = await MixAndMasterAsync(project, stems, ct);

            // Complete
            project.SetMasterFile(masterPath);
            project.Complete();
            await _repository.SaveAsync(project, ct);

            progressState.Update(AgentState.Complete, "Generation finished", 1.0f);

            return new GenerationResult(true, null, masterPath, stems.Select(s => s.Path).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Generation failed for project {ProjectId}", project.Id.Value);
            project.Fail(ex.Message);
            await _repository.SaveAsync(project, ct);
            progressState.Update(AgentState.Error, ex.Message, 0f);
            return new GenerationResult(false, ex.Message, null, []);
        }
    }

    public async Task<GenerationResult> IterateAsync(
        Project project,
        string feedback,
        string? targetSection,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Iterating on project {ProjectId} with feedback", project.Id.Value);

        project.Context.AddTurn("user", feedback);

        var llm = _llmFactory.GetDefaultClient();
        var analysis = await llm.CompleteAsync(new LlmRequest(
            Prompt: $"Analyze this music feedback and determine what changes to make:\n\nFeedback: {feedback}\n\nTarget section: {targetSection ?? "entire track"}\n\nRespond with specific musical changes (tempo, energy, instruments, etc).",
            SystemPrompt: "You are a music producer assistant. Provide concise, actionable feedback analysis.",
            MaxTokens: 500
        ), ct);

        project.Context.AddTurn("assistant", analysis.Content);

        return await GenerateAsync(project, $"Apply changes: {analysis.Content}", ct);
    }

    public async IAsyncEnumerable<ProgressUpdate> StreamProgressAsync(
        ProjectId projectId,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (!_progressStates.TryGetValue(projectId.Value, out var state))
        {
            yield break;
        }

        var lastVersion = -1;
        while (!ct.IsCancellationRequested)
        {
            if (state.Version > lastVersion)
            {
                lastVersion = state.Version;
                yield return new ProgressUpdate(state.State, state.Component, state.Progress, state.Message);

                if (state.State == AgentState.Complete || state.State == AgentState.Error)
                {
                    break;
                }
            }
            await Task.Delay(100, ct);
        }
    }

    private async Task<GenerationPlan> AnalyzeRequestAsync(Project project, string prompt, CancellationToken ct)
    {
        var llm = _llmFactory.GetDefaultClient();

        var systemPrompt = """
You are an expert music producer and composer. Analyze the user's request and create a generation plan.
Return a JSON object with:
- style: detailed style description
- structure: array of section names (intro, verse, chorus, etc)
- instruments: array of instruments to use
- mood_progression: how energy should flow through the track
""";

        var userPrompt = $"""
Project: {project.Name}
Genre: {project.Specification.Genre}
Mood: {project.Specification.Mood}
Key: {project.Specification.Key}
Tempo: {project.Specification.Tempo.Value} BPM
Duration: {project.Specification.DurationSeconds} seconds

User request: {prompt}

Create a generation plan as JSON.
""";

        var response = await llm.CompleteAsync(new LlmRequest(
            Prompt: userPrompt,
            SystemPrompt: systemPrompt,
            MaxTokens: 1000,
            Temperature: 0.7f
        ), ct);

        project.Context.AddTurn("assistant", response.Content);

        try
        {
            var plan = JsonSerializer.Deserialize<GenerationPlan>(response.Content);
            return plan ?? new GenerationPlan("default", ["intro", "verse", "chorus", "outro"], ["synth", "drums", "bass"], "building");
        }
        catch
        {
            return new GenerationPlan(
                $"{project.Specification.Genre} {project.Specification.Mood}",
                ["intro", "verse", "chorus", "outro"],
                ["synth", "drums", "bass", "pad"],
                "building then relaxing"
            );
        }
    }

    private async Task<TheoryOutput> GenerateTheoryAsync(Project project, GenerationPlan plan, CancellationToken ct)
    {
        if (_workerClient is not null)
        {
            var request = new TheoryRequest(
                Genre: project.Specification.Genre.ToString(),
                Mood: project.Specification.Mood.ToString(),
                TempoBpm: project.Specification.Tempo.Value,
                Key: project.Specification.Key.Root.ToString(),
                Mode: project.Specification.Key.Mode.ToString(),
                DurationSeconds: project.Specification.DurationSeconds,
                StyleTags: project.Specification.StyleTags.ToList()
            );

            var result = await _workerClient.GenerateTheoryAsync(request, ct);
            return new TheoryOutput(result.ChordProgression.ToList(), result.Sections.ToList(), result.MidiData);
        }

        var llm = _llmFactory.GetDefaultClient();
        var response = await llm.CompleteAsync(new LlmRequest(
            Prompt: $"Generate a chord progression for {project.Specification.Genre} in {project.Specification.Key}. Return only the chords separated by dashes.",
            MaxTokens: 100
        ), ct);

        var chords = response.Content.Split('-').Select(c => c.Trim()).ToList();
        return new TheoryOutput(chords, [], []);
    }

    private async Task<List<StemOutput>> GenerateAudioAsync(
        Project project,
        TheoryOutput theory,
        ProgressState progressState,
        CancellationToken ct)
    {
        var stems = new List<StemOutput>();
        var components = new[] { "drums", "bass", "lead", "pad" };

        for (int i = 0; i < components.Length; i++)
        {
            var component = components[i];
            var progress = 0.4f + (0.4f * i / components.Length);
            progressState.Update(AgentState.SynthesizingAudio, $"Generating {component}", progress);

            var path = $"output/{project.Id.Value}/{component}.wav";

            if (_workerClient is not null)
            {
                var request = new AudioRequest(
                    Prompt: $"{project.Specification.Genre} {component}, {project.Specification.Mood} mood",
                    DurationSeconds: project.Specification.DurationSeconds,
                    Genre: project.Specification.Genre.ToString(),
                    EnergyLevel: 0.6f,
                    ConditioningAudio: null,
                    SectionName: component
                );

                var chunks = new List<byte>();
                await foreach (var chunk in _workerClient.SynthesizeAudioAsync(request, ct))
                {
                    chunks.AddRange(chunk.AudioData);
                }
                stems.Add(new StemOutput(component, path, chunks.ToArray()));
            }
            else
            {
                stems.Add(new StemOutput(component, path, []));
            }

            var stem = Stem.Create(component, ComponentType.Stems, path);
            project.AddStem(stem);
        }

        return stems;
    }

    private Task<string> MixAndMasterAsync(Project project, List<StemOutput> stems, CancellationToken ct)
    {
        var masterPath = $"output/{project.Id.Value}/master.wav";
        return Task.FromResult(masterPath);
    }

    private sealed class ProgressState
    {
        public AgentState State { get; private set; } = AgentState.Idle;
        public string Component { get; private set; } = "";
        public float Progress { get; private set; } = 0f;
        public string Message { get; private set; } = "Starting...";
        public int Version { get; private set; } = 0;

        public void Update(AgentState state, string message, float progress)
        {
            State = state;
            Message = message;
            Progress = progress;
            Component = state.ToString();
            Version++;
        }
    }

    private sealed record GenerationPlan(
        string Style,
        List<string> Structure,
        List<string> Instruments,
        string MoodProgression);

    private sealed record TheoryOutput(
        List<string> Chords,
        IReadOnlyList<SectionData> Sections,
        byte[] MidiData);

    private sealed record StemOutput(string Name, string Path, byte[] AudioData);
}
