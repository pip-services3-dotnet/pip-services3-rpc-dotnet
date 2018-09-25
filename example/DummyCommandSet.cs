using System;
using System.Threading.Tasks;

using PipServices.Commons.Commands;
using PipServices.Commons.Run;
using PipServices.Commons.Validate;
using PipServices.Commons.Data;

namespace PipServices.Rpc
{
    public class DummyCommandSet : CommandSet
    {
        private IDummyController _controller;

        public DummyCommandSet(IDummyController controller)
        {
            _controller = controller;

            AddCommand(MakeGetPageByFilterCommand());
            AddCommand(MakeGetOneByIdCommand());
            AddCommand(MakeCreateCommand());
            AddCommand(MakeUpdateCommand());
            AddCommand(MakeDeleteByIdCommand());
            // Commands for errors
            AddCommand(MakeCreateWithoutValidationCommand());
            AddCommand(MakeRaiseCommandSetExceptionCommand());
            AddCommand(MakeRaiseControllerExceptionCommand());

            // V2
            AddCommand(MakePingCommand());
        }

        private ICommand MakeGetPageByFilterCommand()
        {
            return new Command(
                "get_dummies",
                new ObjectSchema()
                    .WithOptionalProperty("correlation_id", typeof(string))
                    .WithOptionalProperty("filter", new FilterParamsSchema())
                    .WithOptionalProperty("paging", new PagingParamsSchema()),
                async (correlationId, args) => 
                {
                    var filter = FilterParams.FromValue(args.Get("filter"));
                    var paging = PagingParams.FromValue(args.Get("paging"));

                    return await _controller.GetPageByFilterAsync(correlationId, filter, paging);    
                });
        }

        private ICommand MakeGetOneByIdCommand()
        {
            return new Command(
                "get_dummy_by_id",
                new ObjectSchema()
                    .WithRequiredProperty("dummy_id", Commons.Convert.TypeCode.String),
                async (correlationId, args) => 
                {
                    var dummyId = args.GetAsString("dummy_id");
                    return await _controller.GetOneByIdAsync(correlationId, dummyId);                    
                });
        }

        private ICommand MakeCreateCommand()
        {
            return new Command(
                "create_dummy",
                new ObjectSchema()
                    .WithRequiredProperty("dummy", new DummySchema()),
                async (correlationId, args) => 
                {
                    var dummy = ExtractDummy(args);
                    return await _controller.CreateAsync(correlationId, dummy);
                });
        }

        private ICommand MakeUpdateCommand()
        {
            return new Command(
                "update_dummy",
                new ObjectSchema()
                    .WithRequiredProperty("dummy", new DummySchema()),
                async (correlationId, args) =>
                {
                    var dummy = ExtractDummy(args);
                    return await _controller.UpdateAsync(correlationId, dummy);
                });
        }

        private ICommand MakeDeleteByIdCommand()
        {
            return new Command(
                "delete_dummy",
                new ObjectSchema()
                    .WithRequiredProperty("dummy_id", Commons.Convert.TypeCode.String),
                async (correlationId, args) => 
                {
                    var dummyId = args.GetAsString("dummy_id");

                    return await _controller.DeleteByIdAsync(correlationId, dummyId);
                });
        }

        private ICommand MakeCreateWithoutValidationCommand()
        {
            return new Command(
                "create_dummy_without_validation",
                null,
                async (correlationId, parameters) => 
                {
                    await Task.Delay(0);
                    return null;
                });
        }

        private ICommand MakeRaiseCommandSetExceptionCommand()
        {
            return new Command(
                "raise_commandset_error",
                new ObjectSchema()
                    .WithRequiredProperty("dummy", new DummySchema()),
                (correlationId, parameters) =>
                {
                    throw new Exception("Dummy error in commandset!");
                });
        }

        private ICommand MakeRaiseControllerExceptionCommand()
        {
            return new Command(
                "raise_exception",
                new ObjectSchema(),
                async (correlationId, parameters) =>
                {
                    await _controller.RaiseExceptionAsync(correlationId);
                    return null;
                });
        }

        private ICommand MakePingCommand()
        {
            return new Command(
                "ping_dummy",
                null,
                async (correlationId, parameters) =>
                {
                    return await _controller.PingAsync();
                });
        }

        private static Dummy ExtractDummy(Parameters args)
        {
            var map = args.GetAsMap("dummy");

            var id = map.GetAsStringWithDefault("id", string.Empty);
            var key = map.GetAsStringWithDefault("key", string.Empty);
            var content = map.GetAsStringWithDefault("content", string.Empty);
            var flag = map.GetAsBooleanWithDefault("flag", false);

            var dummy = new Dummy(id, key, content, flag);
            return dummy;
        }

    }
}