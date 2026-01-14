using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MusicForge.Application.Interfaces;
using MusicForge.Application.Services;
using MusicForge.Domain.Entities;
using Xunit;

namespace MusicForge.Application.Tests.Services;

public class AgentStateMachineTests
{
    private readonly Mock<ILogger<AgentStateMachine>> _mockLogger;
    private readonly AgentStateMachine _stateMachine;
    private readonly AgentContext _context;

    public AgentStateMachineTests()
    {
        _mockLogger = new Mock<ILogger<AgentStateMachine>>();
        _stateMachine = new AgentStateMachine(_mockLogger.Object);
        _context = new AgentContext();
    }

    [Fact]
    public void Should_Start_Idle()
    {
        _stateMachine.CurrentState.Should().Be(AgentState.Idle);
        _stateMachine.StateHistory.Should().HaveCount(1);
    }

    [Fact]
    public async Task Should_Transition_To_Valid_State()
    {
        // Act
        var result = await _stateMachine.TransitionToAsync(AgentState.Understanding, _context);

        // Assert
        result.Should().Be(AgentState.Understanding);
        _stateMachine.CurrentState.Should().Be(AgentState.Understanding);
        _stateMachine.StateHistory.Should().HaveCount(2);
    }

    [Fact]
    public async Task Should_Throw_On_Invalid_Transition()
    {
        // Act
        Func<Task> act = async () => await _stateMachine.TransitionToAsync(AgentState.Mastering, _context);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
           .WithMessage("*Cannot transition*");
    }

    [Fact]
    public async Task Should_Auto_Advance_Through_Pipeline()
    {
        _context.SetIntent("Create a song");

        // Idle -> Understanding
        await _stateMachine.TransitionAsync(_context);
        _stateMachine.CurrentState.Should().Be(AgentState.Understanding);

        // Understanding -> Planning
        await _stateMachine.TransitionAsync(_context);
        _stateMachine.CurrentState.Should().Be(AgentState.Planning);

        // Planning -> Composing
        await _stateMachine.TransitionAsync(_context);
        _stateMachine.CurrentState.Should().Be(AgentState.Composing);
    }

    [Fact]
    public async Task Reset_Should_Clear_History_And_Set_Idle()
    {
        await _stateMachine.TransitionToAsync(AgentState.Understanding, _context);

        _stateMachine.Reset();

        _stateMachine.CurrentState.Should().Be(AgentState.Idle);
        _stateMachine.StateHistory.Should().HaveCount(1);
    }
}
