using BlazorGrpcDemo.Client;
using Grpc.Core;

namespace BlazorGrpcDemo.Data;

public class PeopleService : People.PeopleBase
{
    PersonsManager _personsManager;

    public PeopleService(PersonsManager personsManager)
    {
        _personsManager = personsManager;
    }

    public override Task<PeopleReply> GetAll(GetAllPeopleRequest request,
        ServerCallContext context)
    {
        var reply = new PeopleReply();
        reply.People.AddRange(_personsManager.People);
        return Task.FromResult(reply);
    }

    public override async Task GetAllStream(GetAllPeopleRequest request,
    IServerStreamWriter<Person> responseStream, ServerCallContext context)
    {
        // Use this pattern to return a stream in a gRPC service.

        // retrieve the list
        var people = _personsManager.People;

        // write each item to the responseStream, which does the rest
        foreach (var person in people)
        {
            await responseStream.WriteAsync(person);
        }
    }

    public override Task<Person> GetPersonById(GetPersonByIdRequest request,
        ServerCallContext context)
    {
        var result = (from x in _personsManager.People
                      where x.Id == request.Id
                      select x).FirstOrDefault();
        return Task.FromResult(result);
    }
}