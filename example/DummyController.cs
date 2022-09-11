using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using PipServices3.Commons.Data;
using PipServices3.Commons.Commands;
using PipServices3.Commons.Errors;

namespace PipServices3.Rpc
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

        public async Task<DataPage<Dummy>> GetPageByFilterAsync(string correlationId, FilterParams filter, PagingParams paging)
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
            return await Task.FromResult(new DataPage<Dummy>(result));
        }

        public async Task<Dummy> GetOneByIdAsync(string correlationId, string id)
        {
            await Task.Delay(0);

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

        public async Task<Dummy> CreateAsync(string correlationId, Dummy entity)
        {
            await Task.Delay(0);

            lock(_lock)
            {
                if (string.IsNullOrEmpty(entity.Id))
                    entity.Id = IdGenerator.NextLong();

                _entities.Add(entity);
            }
            return entity;
        }

        public async Task<Dummy> UpdateAsync(string correlationId, Dummy newEntity)
        {
            await Task.Delay(0);

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

        public async Task<Dummy> DeleteByIdAsync(string correlationId, string id)
        {
            await Task.Delay(0);

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

        public async Task<string> CheckCorrelationId(string correlationId)
        {
            return await Task.FromResult(correlationId);
        }

        public Task RaiseExceptionAsync(string correlationId)
        {
            throw new NotFoundException(correlationId, "TEST_ERROR", "Dummy error in controller!");
        }

        public async Task<bool> PingAsync()
        {
            return await Task.FromResult(true);
        }
    }
}
