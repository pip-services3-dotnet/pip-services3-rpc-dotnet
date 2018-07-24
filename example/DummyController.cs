using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using PipServices.Commons.Data;
using PipServices.Commons.Commands;
using PipServices.Rpc.Commands;
using PipServices.Commons.Errors;

namespace PipServices.Rpc
{
    public sealed class DummyController : IDummyController, ICommandable
    {
        private readonly object _lock = new object();
        private readonly IList<Dummy> _entities = new List<Dummy>();

        private DummyCommandSet _commandSet;

        public CommandSet GetCommandSet()
        {
            if (_commandSet == null)
            {
                _commandSet = new DummyCommandSet(this);
            }

		    return _commandSet;
	    }

        public DataPage<Dummy> GetPageByFilter(string correlationId, FilterParams filter, PagingParams paging)
        {
            filter = filter != null ? filter : new FilterParams();
            var key = filter.GetAsNullableString("key");

            paging = paging != null ? paging : new PagingParams();
            var skip = paging.GetSkip(0);
            var take = paging.GetTake(100);

            var result = new List<Dummy>();

            lock(_lock)
            {
                foreach (var entity in _entities)
                {
                    if (key != null && !key.Equals(entity.Key))
                        continue;

                    skip--;
                    if (skip >= 0) continue;

                    take--;
                    if (take < 0) break;

                    result.Add(entity);
                }
            }
            return new DataPage<Dummy>(result);
        }

        public Dummy GetOneById(string correlationId, string id)
        {
            lock(_lock)
            {
                foreach(var entity in _entities)
                {
                    if (entity.Id.Equals(id))
                        return entity;
                }
            }
            return null;
        }

        public Dummy Create(string correlationId, Dummy entity)
        {
            lock(_lock)
            {
                if (entity.Id == null)
                    entity.Id = IdGenerator.NextLong();

                _entities.Add(entity);
            }
            return entity;
        }

        public Dummy Update(string correlationId, Dummy newEntity)
        {
            lock(_lock)
            {
                for (int index = 0; index < _entities.Count; index++)
                {
                    var entity = _entities[index];
                    if (entity.Id.Equals(newEntity.Id))
                    {
                        _entities[index] = newEntity;
                        return newEntity;
                    }
                }
            }
            return null;
        }

        public Dummy DeleteById(string correlationId, string id)
        {
            lock(_lock)
            {
                for (int index = 0; index < _entities.Count; index++)
                {
                    var entity = _entities[index];
                    if (entity.Id.Equals(id))
                    {
                        _entities.RemoveAt(index);
                        return entity;
                    }
                }
            }
            return null;
        }

        public void RaiseException(string correlationId)
        {
            throw new NotFoundException(correlationId, "TEST_ERROR", "Dummy error in controller!");
        }

        public async Task<bool> PingAsync()
        {
            return await Task.FromResult(true);
        }
    }
}
