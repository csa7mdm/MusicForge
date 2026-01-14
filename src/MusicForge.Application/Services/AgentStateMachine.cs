using Microsoft.Extensions.Logging;
using MusicForge.Application.Interfaces;
using MusicForge.Domain.Entities;

namespace MusicForge.Application.Services;

public class AgentStateMachine : IAgentStateMachine
{
    private AgentState _currentState = AgentState.Idle;
    private readonly List<(AgentState State, DateTime Timestamp)> _history = [];
    private readonly ILogger<AgentStateMachine> _logger;

    public AgentState CurrentState => _currentState;

    public IReadOnlyList<(AgentState State, DateTime Timestamp)> StateHistory => _history.AsReadOnly();

    public AgentStateMachine(ILogger<AgentStateMachine> logger)
    {
        _logger = logger;
        // Record initial state
        _history.Add((_currentState, DateTime.UtcNow));
    }

    public bool CanTransitionTo(AgentState targetState)
    {
        if (_currentState == targetState) return true; // Idempotent

        if (targetState == AgentState.Error) return true;
        if (targetState == AgentState.Idle) return true; // Reset allows going to Idle/Draft

        return _currentState switch
        {
            AgentState.Idle => targetState == AgentState.Understanding || targetState == AgentState.Planning,
            AgentState.Understanding => targetState == AgentState.Planning,
            AgentState.Planning => targetState == AgentState.Composing,
            AgentState.Composing => targetState == AgentState.GeneratingMidi,
            AgentState.GeneratingMidi => targetState == AgentState.SynthesizingAudio,
            AgentState.SynthesizingAudio => targetState == AgentState.SynthesizingVocals || targetState == AgentState.Mixing,
            AgentState.SynthesizingVocals => targetState == AgentState.Mixing,
            AgentState.Mixing => targetState == AgentState.Mastering,
            AgentState.Mastering => targetState == AgentState.Exporting || targetState == AgentState.Complete,
            AgentState.Exporting => targetState == AgentState.Complete,
            AgentState.Complete => targetState == AgentState.AwaitingFeedback || targetState == AgentState.Idle,
            AgentState.AwaitingFeedback => targetState == AgentState.Iterating,
            AgentState.Iterating => targetState == AgentState.Planning || targetState == AgentState.Composing || targetState == AgentState.Mixing || targetState == AgentState.GeneratingMidi,
            AgentState.Error => targetState == AgentState.Idle || targetState == AgentState.Planning,
            _ => false
        };
    }

    public Task<AgentState> TransitionAsync(AgentContext context, CancellationToken ct = default)
    {
        // Determine next state based on current state and context
        // This acts as the "Auto-Advance" logic for the pipeline
        
        var nextState = _currentState;

        switch (_currentState)
        {
            case AgentState.Idle:
                // If intent is set, move to Understanding, else stay Idle
                if (!string.IsNullOrEmpty(context.CurrentIntent))
                {
                    nextState = AgentState.Understanding;
                }
                break;

            case AgentState.Understanding:
                nextState = AgentState.Planning;
                break;

            case AgentState.Planning:
                nextState = AgentState.Composing;
                break;

            case AgentState.Composing:
                nextState = AgentState.GeneratingMidi;
                break;

            case AgentState.GeneratingMidi:
                nextState = AgentState.SynthesizingAudio;
                break;

            case AgentState.SynthesizingAudio:
                // Check if vocals are needed? For now, linear flow implies checking context/project specs
                // We'll assume yes if context implies specific intent, but here we just flow forward
                nextState = AgentState.SynthesizingVocals; 
                // Optimization: could skip to Mixing if instrumental
                break;

            case AgentState.SynthesizingVocals:
                nextState = AgentState.Mixing;
                break;

            case AgentState.Mixing:
                nextState = AgentState.Mastering;
                break;

            case AgentState.Mastering:
                nextState = AgentState.Complete; // Skip exporting as default step, usually explicit command
                break;
                
            case AgentState.Exporting:
                nextState = AgentState.Complete;
                break;

            case AgentState.Complete:
                nextState = AgentState.AwaitingFeedback;
                break;

            case AgentState.AwaitingFeedback:
               // Stay here until explicit "Iterate" command triggers a jump to Iterating
               // TransitionAsync usually called after a step completes.
               // If we are awaiting feedback, we stay awaiting.
               break;

            case AgentState.Iterating:
               // Where do we go after iterating? Depends on what we iterated.
               // Default to Mixing to verify? Or back to Complete?
               // Let's assume we go to Mixing to reintegrate changes.
               nextState = AgentState.Mixing;
               break;
               
            case AgentState.Error:
                // Manual intervention needed usually.
                break;
        }

        return TransitionToAsync(nextState, context, ct);
    }

    // Helper to perform the transition with validation
    public Task<AgentState> TransitionToAsync(AgentState targetState, AgentContext context, CancellationToken ct = default)
    {
        if (!CanTransitionTo(targetState))
        {
            _logger.LogWarning("Invalid state transition from {CurrentState} to {TargetState}", _currentState, targetState);
            throw new InvalidOperationException($"Cannot transition from {_currentState} to {targetState}");
        }

        if (_currentState != targetState)
        {
            _currentState = targetState;
            _history.Add((_currentState, DateTime.UtcNow));
            _logger.LogInformation("Agent transitioned to {State}", _currentState);
        }

        return Task.FromResult(_currentState);
    }

    public void Reset()
    {
        _currentState = AgentState.Idle;
        _history.Clear();
        _history.Add((_currentState, DateTime.UtcNow));
        _logger.LogInformation("Agent state machine reset");
    }
}
