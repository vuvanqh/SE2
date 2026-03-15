using StudentPlanner.Core.Application.PersonalEvents;
using System;
using System.Collections.Generic;
using System.Text;
using AutoFixture;
using Moq;
using StudentPlanner.Core.Domain.RepositoryContracts;
using StudentPlanner.Core.Domain;
using System.Runtime.CompilerServices;
using Xunit.Internal;


namespace StudentPlanner.Tests;

public class PersonalEventTests
{
    private readonly IFixture _fixture;
    private readonly IPersonalEventRepository _personalEventRepo;
    private readonly Mock<IPersonalEventRepository> _personalEventRepoMock;
    public PersonalEventTests()
    {
        _fixture = new Fixture();
        _personalEventRepoMock = new Mock<IPersonalEventRepository>();
        _personalEventRepo = _personalEventRepoMock.Object;
    }

    #region Create Personal Event Tests
    [Fact]
    public async Task CreatePersonalEvent_ShouldCreateEvent_WhenRequestIsValid() {
        PersonalEvent? result = null;
        CreatePersonalEventRequest request = _fixture.Build<CreatePersonalEventRequest>()
            .With(t => t.StartTime, DateTime.UtcNow)
            .With(t => t.EndTime, DateTime.UtcNow.AddDays(1))
            .Create();

        Guid dummyUser = Guid.NewGuid();

        _personalEventRepoMock.Setup(t => t.AddAsync(It.IsAny<PersonalEvent>()))
            .Callback<PersonalEvent>(e => result = e);

        PersonalEventService personalEventService = new PersonalEventService(_personalEventRepo);
        Guid eventId = await personalEventService.CreatePersonalEventAsync(dummyUser, request);

        Assert.NotNull(result);
        Assert.Equal(request.Title, result.EventDetails.Title);
        Assert.Equal(dummyUser, result.UserId);
    }
    [Fact]
    public async Task CreatePersonalEvent_ShouldThrow_WhenEndDateBeforeEqualStartDate()
    {
        PersonalEventService personalEventService = new PersonalEventService(_personalEventRepo);

        CreatePersonalEventRequest request = _fixture.Build<CreatePersonalEventRequest>()
            .With(t => t.StartTime, DateTime.UtcNow)
            .With(t => t.EndTime, DateTime.UtcNow.AddDays(-1))
            .Create();
        
        Exception exception = await Assert.ThrowsAsync<ArgumentException>(()=> personalEventService.CreatePersonalEventAsync(Guid.NewGuid(), request));
    }
    [Fact]
    public async Task CreatePersonalEvent_ShouldThrow_WhenTitleIsEmpty() {
        PersonalEventService personalEventService = new PersonalEventService(_personalEventRepo);

        CreatePersonalEventRequest request = _fixture.Build<CreatePersonalEventRequest>()
            .With(t => t.StartTime, DateTime.UtcNow)
            .With(t => t.EndTime, DateTime.UtcNow.AddDays(1))
            .With(t => t.Title, string.Empty)
            .Create();

        Exception exception = await Assert.ThrowsAsync<ArgumentException>(() => personalEventService.CreatePersonalEventAsync(Guid.NewGuid(), request));
    }
    [Fact]
    public async Task CreatePersonalEvent_ShouldThrow_WhenStartDateInPast() {
        PersonalEventService personalEventService = new PersonalEventService(_personalEventRepo);

        CreatePersonalEventRequest request = _fixture.Build<CreatePersonalEventRequest>()
            .With(t => t.StartTime, DateTime.UtcNow.AddDays(-1))
            .With(t => t.EndTime, DateTime.UtcNow.AddDays(1))
            .Create();

        Exception exception = await Assert.ThrowsAsync<ArgumentException>(() => personalEventService.CreatePersonalEventAsync(Guid.NewGuid(), request));
    }
    #endregion

    #region Update Personal Event Tests
    [Fact]
    public async Task UpdatePersonalEvent_ShouldUpdateEvent_WhenRequestIsValid()
    {
        Guid eventId = Guid.NewGuid();
        Guid dummyUser = Guid.NewGuid();

        PersonalEvent personalEvent = new PersonalEvent()
        {
            Id = eventId,
            UserId = dummyUser,
            EventDetails = _fixture.Build<EventDetails>()
                .With(t=>t.StartTime, DateTime.UtcNow)
                .With(t=>t.EndTime, DateTime.UtcNow.AddDays(1))
                .Create<EventDetails>()
        };

        UpdatePersonalEventRequest request = _fixture.Build<UpdatePersonalEventRequest>()
            .With(t => t.StartTime, DateTime.UtcNow)
            .With(t => t.EndTime, DateTime.UtcNow.AddDays(1))
            .Create();

        _personalEventRepoMock.Setup(t => t.GetEventByEventIdAsync(eventId))
            .ReturnsAsync(personalEvent);

        PersonalEventService personalEventService = new PersonalEventService(_personalEventRepo);
        await personalEventService.UpdatePersonalEventAsync(dummyUser, eventId, request);

        PersonalEvent updatedEvent = request.ToPersonalEvent(dummyUser, eventId);

        _personalEventRepoMock.Verify(r => r.UpdateAsync(It.IsAny<PersonalEvent>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePersonalEvent_ShouldThrow_WhenUserDoesNotOwnTheEvent()
    {
        Guid eventId = Guid.NewGuid();
        Guid dummyUser = Guid.NewGuid();
        Guid dummyUser2 = Guid.NewGuid();

        PersonalEvent personalEvent = new PersonalEvent()
        {
            Id = eventId,
            UserId = dummyUser,
            EventDetails = _fixture.Create<EventDetails>()
        };

        UpdatePersonalEventRequest request = _fixture.Create<UpdatePersonalEventRequest>();

        _personalEventRepoMock.Setup(t => t.GetEventByEventIdAsync(eventId))
            .ReturnsAsync(personalEvent);

        PersonalEventService personalEventService = new PersonalEventService(_personalEventRepo);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => personalEventService.UpdatePersonalEventAsync(dummyUser2, eventId, request));
    }
    [Fact]
    public async Task UpdatePersonalEvent_ShouldThrow_WhenEventDoesNotExist()
    {
        Guid eventId = Guid.NewGuid();
        Guid dummyUser = Guid.NewGuid();

        UpdatePersonalEventRequest request = _fixture.Create<UpdatePersonalEventRequest>();

        _personalEventRepoMock.Setup(t => t.GetEventByEventIdAsync(eventId))
            .ReturnsAsync((PersonalEvent?)null);

        PersonalEventService personalEventService = new PersonalEventService(_personalEventRepo);

        await Assert.ThrowsAsync<ArgumentException>(() => personalEventService.UpdatePersonalEventAsync(dummyUser, eventId, request));
    }
    [Fact]
    public async Task UpdatePersonalEvent_ShouldThrow_WhenEndDateBeforeEqualStartDate()
    {
        Guid dummyUser = Guid.NewGuid();

        PersonalEventService personalEventService = new PersonalEventService(_personalEventRepo);
        UpdatePersonalEventRequest request = _fixture.Build<UpdatePersonalEventRequest>()
            .With(t => t.StartTime, DateTime.UtcNow)
            .With(t => t.EndTime, DateTime.UtcNow.AddDays(-1))
            .Create();

        await Assert.ThrowsAsync<ArgumentException>(() => personalEventService.UpdatePersonalEventAsync(dummyUser, Guid.NewGuid(), request));
    }
    [Fact]
    public async Task UpdatePersonalEvent_ShouldThrow_WhenTitleIsEmpty()
    {
        Guid dummyUser = Guid.NewGuid();

        PersonalEventService personalEventService = new PersonalEventService(_personalEventRepo);
        UpdatePersonalEventRequest request = _fixture.Build<UpdatePersonalEventRequest>()
            .With(t => t.StartTime, DateTime.UtcNow)
            .With(t => t.EndTime, DateTime.UtcNow.AddDays(1))
            .With(t => t.Title, string.Empty)
            .Create();

        Exception exception = await Assert.ThrowsAsync<ArgumentException>(() => personalEventService.UpdatePersonalEventAsync(dummyUser, Guid.NewGuid(), request));
    }
    [Fact]
    public async Task UpdatePersonalEvent_ShouldThrow_WhenStartDateInPast()
    {
        Guid dummyUser = Guid.NewGuid();

        PersonalEventService personalEventService = new PersonalEventService(_personalEventRepo);
        UpdatePersonalEventRequest request = _fixture.Build<UpdatePersonalEventRequest>()
            .With(t => t.StartTime, DateTime.UtcNow.AddDays(-1))
            .With(t => t.EndTime, DateTime.UtcNow.AddDays(1))
            .Create();

        Exception exception = await Assert.ThrowsAsync<ArgumentException>(() => personalEventService.UpdatePersonalEventAsync(dummyUser, Guid.NewGuid(), request));
    }
    #endregion

    #region Delete Personal Event Tests
    [Fact]
    public async Task DeletePersonalEvent_ShouldDeleteEvent_WhenRequestIsValid()
    {
        Guid eventId = Guid.NewGuid();
        Guid dummyUser = Guid.NewGuid();

        PersonalEvent personalEvent = new PersonalEvent()
        {
            Id = eventId,
            UserId = dummyUser,
            EventDetails = _fixture.Build<EventDetails>()
                .With(t => t.StartTime, DateTime.UtcNow)
                .With(t => t.EndTime, DateTime.UtcNow.AddDays(1))
                .Create<EventDetails>()
        };

        _personalEventRepoMock.Setup(t => t.GetEventByEventIdAsync(eventId))
            .ReturnsAsync(personalEvent);

        PersonalEventService personalEventService = new PersonalEventService(_personalEventRepo);
        await personalEventService.DeletePersonalEventAsync(dummyUser, eventId);

        _personalEventRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task DeletePersonalEvent_ShouldThrow_WhenUserDoesNotOwnTheEvent()
    {
        Guid eventId = Guid.NewGuid();
        Guid dummyUser = Guid.NewGuid();
        Guid dummyUser2 = Guid.NewGuid();

        PersonalEvent personalEvent = new PersonalEvent()
        {
            Id = eventId,
            UserId = dummyUser,
            EventDetails = _fixture.Create<EventDetails>()
        };

        _personalEventRepoMock.Setup(t => t.GetEventByEventIdAsync(eventId))
            .ReturnsAsync(personalEvent);

        PersonalEventService personalEventService = new PersonalEventService(_personalEventRepo);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => personalEventService.DeletePersonalEventAsync(dummyUser2, eventId));
    }
    [Fact]
    public async Task DeletePersonalEvent_ShouldThrow_WhenEventDoesNotExist()
    {
        Guid eventId = Guid.NewGuid();
        Guid dummyUser = Guid.NewGuid();

        _personalEventRepoMock.Setup(t => t.GetEventByEventIdAsync(eventId))
            .ReturnsAsync((PersonalEvent?)null);

        PersonalEventService personalEventService = new PersonalEventService(_personalEventRepo);

        await Assert.ThrowsAsync<ArgumentException>(() => personalEventService.DeletePersonalEventAsync(dummyUser, eventId));
    }
    #endregion

    #region Get Event(s) Tests
    [Fact]
    public async Task GetPersonalEvent_ShouldReturnEvent_WhenRequestIsValid()
    {
        Guid eventId = Guid.NewGuid();
        Guid dummyUser = Guid.NewGuid();

        PersonalEvent personalEvent = new PersonalEvent()
        {
            Id = eventId,
            UserId = dummyUser,
            EventDetails = _fixture.Build<EventDetails>()
                .With(t => t.StartTime, DateTime.UtcNow)
                .With(t => t.EndTime, DateTime.UtcNow.AddDays(1))
                .Create<EventDetails>()
        };

        _personalEventRepoMock.Setup(t => t.GetEventByEventIdAsync(eventId))
            .ReturnsAsync(personalEvent);

        PersonalEventService personalEventService = new PersonalEventService(_personalEventRepo);
        PersonalEventResponse? resp = await personalEventService.GetEventByIdAsync(dummyUser, eventId);
      
        _personalEventRepoMock.Verify(r => r.GetEventByEventIdAsync(It.IsAny<Guid>()), Times.Once);
        Assert.True(resp != null);
    }

    [Fact]
    public async Task GetPersonalEvent_ShouldThrow_WhenUserDoesNotOwnTheEvent()
    {
        Guid eventId = Guid.NewGuid();
        Guid dummyUser = Guid.NewGuid();
        Guid dummyUser2 = Guid.NewGuid();

        PersonalEvent personalEvent = new PersonalEvent()
        {
            Id = eventId,
            UserId = dummyUser,
            EventDetails = _fixture.Create<EventDetails>()
        };

        _personalEventRepoMock.Setup(t => t.GetEventByEventIdAsync(eventId))
            .ReturnsAsync(personalEvent);

        PersonalEventService personalEventService = new PersonalEventService(_personalEventRepo);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => personalEventService.GetEventByIdAsync(dummyUser2, eventId));
    }
    [Fact]
    public async Task GetPersonalEvent_ShouldThrow_WhenEventDoesNotExist()
    {
        Guid eventId = Guid.NewGuid();
        Guid dummyUser = Guid.NewGuid();

        _personalEventRepoMock.Setup(t => t.GetEventByEventIdAsync(eventId))
            .ReturnsAsync((PersonalEvent?)null);

        PersonalEventService personalEventService = new PersonalEventService(_personalEventRepo);

        await Assert.ThrowsAsync<ArgumentException>(() => personalEventService.GetEventByIdAsync(dummyUser, eventId));
    }

    [Fact]
    public async Task GetPersonalEvents_ShouldReturnEmptyList_WhenUserDoesNotHavePersonalEvents()
    {
        Guid dummyUser = Guid.NewGuid();

        _personalEventRepoMock.Setup(t => t.GetEventsByUserIdAsync(dummyUser))
            .ReturnsAsync(new List<PersonalEvent>());

        PersonalEventService personalEventService = new PersonalEventService(_personalEventRepo);
        List<PersonalEventResponse> resp = await personalEventService.GetEventsAsync(dummyUser);

        _personalEventRepoMock.Verify(r => r.GetEventsByUserIdAsync(It.IsAny<Guid>()), Times.Once);
        Assert.True(resp != null);
        Assert.True(resp.Count == 0);
    }

    [Fact]
    public async Task GetPersonalEvents_ShouldReturnPersonalEventList_WhenUserOwnsPersonalEvents()
    {
        Guid eventId = Guid.NewGuid();
        Guid dummyUser = Guid.NewGuid();

        PersonalEvent personalEvent = new PersonalEvent()
        {
            Id = eventId,
            UserId = dummyUser,
            EventDetails = _fixture.Build<EventDetails>()
                .With(t => t.StartTime, DateTime.UtcNow)
                .With(t => t.EndTime, DateTime.UtcNow.AddDays(1))
                .Create<EventDetails>()
        };

        _personalEventRepoMock.Setup(t => t.GetEventsByUserIdAsync(dummyUser))
            .ReturnsAsync(new List<PersonalEvent>() { personalEvent });

        PersonalEventService personalEventService = new PersonalEventService(_personalEventRepo);
        List<PersonalEventResponse> resp = await personalEventService.GetEventsAsync(dummyUser);

        _personalEventRepoMock.Verify(r => r.GetEventsByUserIdAsync(It.IsAny<Guid>()), Times.Once);
        Assert.True(resp.Count>0);
        Assert.Equal(resp[0].Id, eventId);
    }
    #endregion

    #region helpers
    #endregion
}
