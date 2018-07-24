using System;
using System.Threading.Tasks;

using PipServices.Commons.Commands;
using PipServices.Commons.Run;
using PipServices.Commons.Validate;
using PipServices.Commons.Data;

namespace PipServices.Rpc.Commands
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

        #region Commands

        private ICommand MakeGetPageByFilterCommand()
        {
            return new Command(
                "get_dummies",
                new ObjectSchema()
                    .WithOptionalProperty("correlation_id", typeof(string))
                    .WithOptionalProperty("filter", new FilterParamsSchema())
                    .WithOptionalProperty("paging", new PagingParamsSchema()),
                GetDummies);
        }

        private ICommand MakeGetOneByIdCommand()
        {
            return new Command(
                "get_dummy_by_id",
                new ObjectSchema()
                    .WithRequiredProperty("dummy_id", Commons.Convert.TypeCode.String),
                GetOneDummy);
        }

        private ICommand MakeCreateCommand()
        {
            return new Command(
                "create_dummy",
                new ObjectSchema()
                    .WithRequiredProperty("dummy", new DummySchema()),
                CreateDummy);
        }

        private ICommand MakeUpdateCommand()
        {
            return new Command(
                "update_dummy",
                new ObjectSchema()
                    .WithRequiredProperty("dummy", new DummySchema()),
                UpdateDummy);
        }

        private ICommand MakeDeleteByIdCommand()
        {
            return new Command(
                "delete_dummy",
                new ObjectSchema()
                    .WithRequiredProperty("dummy_id", Commons.Convert.TypeCode.String),
                DeleteDummy);
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
                (correlationId, parameters) =>
                {
                    _controller.RaiseException(correlationId);
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

        #endregion

        #region Helper Methods

        private Task<object> GetDummies(string correlationId, Parameters args)
        {
            var filter = FilterParams.FromValue(args.Get("filter"));
            var paging = PagingParams.FromValue(args.Get("paging"));

            return Convert(Task.FromResult(_controller.GetPageByFilter(correlationId, filter, paging)));
        }

        private Task<object> GetOneDummy(string correlationId, Parameters args)
        {
            var dummyId = args.GetAsString("dummy_id");

            return Convert(Task.FromResult(_controller.GetOneById(correlationId, dummyId)));
        }

        private Task<object> CreateDummy(string correlationId, Parameters args)
        {
            var dummy = ExtractDummy(args);

            return Convert(Task.FromResult(_controller.Create(correlationId, dummy)));
        }

        private Task<object> UpdateDummy(string correlationId, Parameters args)
        {
            var dummy = ExtractDummy(args);

            return Convert(Task.FromResult(_controller.Update(correlationId, dummy)));
        }

        private Task<object> DeleteDummy(string correlationId, Parameters args)
        {
            var dummyId = args.GetAsString("dummy_id");

            return Convert(Task.FromResult(_controller.DeleteById(correlationId, dummyId)));
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

        private async Task<object> Convert<T>(Task<T> task)
        {
            return await task;
        }

        private async Task<object> Convert(Task task)
        {
            return await task.ContinueWith(obj => { return new object(); });
        }

        #endregion

    }
}