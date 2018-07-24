using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using PipServices.Commons.Config;
using PipServices.Commons.Errors;
using PipServices.Commons.Refer;
using PipServices.Commons.Run;
using PipServices.Components.Count;
using PipServices.Components.Log;

namespace PipServices.Rpc.Clients
{
    public abstract class DirectClient<T> : IConfigurable, IOpenable, IReferenceable
    {
        protected T _controller;
        protected CompositeLogger _logger = new CompositeLogger();
        protected CompositeCounters _counters = new CompositeCounters();
        protected DependencyResolver _dependencyResolver = new DependencyResolver();
        protected bool _opened = false;

        public DirectClient()
        {
            _dependencyResolver.Put("controller", "none");
        }

        public virtual void Configure(ConfigParams config)
        {
            _dependencyResolver.Configure(config);
        }

        public virtual void SetReferences(IReferences references)
        {
            _logger.SetReferences(references);
            _counters.SetReferences(references);

            _dependencyResolver.SetReferences(references);
            _controller = _dependencyResolver.GetOneRequired<T>("controller");
        }

        public virtual bool IsOpened()
        {
            return _opened;
        }

        public virtual Task OpenAsync(string correlationId)
        {
            if (IsOpened())
            {
                return Task.Delay(0);
            }

            if (_controller == null)
            {
                throw new ConnectionException(correlationId, "NO_CONTROLLER", "Controller reference is missing");
            }

            _logger.Info(correlationId, "Opened Direct client {0}", GetType().Name);

            _opened = true;
            return Task.Delay(0);
        }

        public virtual Task CloseAsync(string correlationId)
        {
            if (IsOpened())
            {
                _logger.Debug(correlationId, "Closed Direct client {0}", this.GetType().Name);
            }

            _opened = false;

            return Task.Delay(0);
        }

        protected Timing Instrument(string correlationId, [CallerMemberName]string methodName = null)
        {
            var typeName = GetType().Name;
            _logger.Trace(correlationId, "Calling {0} method of {1}", methodName, typeName);
            return _counters.BeginTiming(typeName + "." + methodName + ".call_time");
        }
    }
}
