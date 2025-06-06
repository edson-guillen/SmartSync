﻿using Microsoft.AspNetCore.Mvc;
using SmartSync.Application.Interfaces;
using SmartSync.Domain.Entities;
using Microsoft.AspNetCore.OData.Query;

namespace SmartSync.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BaseController<TModel>(IBaseService<TModel> service, ILogger logger) : Controller where TModel : BaseModel
    {
        protected readonly IBaseService<TModel> _service = service;
        protected readonly ILogger _logger = logger;

        [EnableQuery(MaxExpansionDepth = 5)]
        [HttpGet]
        public virtual IActionResult Get()
        {
            return TryExecute(() =>
            {
                return Ok(_service.Get());
            });
        }

        [HttpGet("{id}")]
        public virtual IActionResult GetById(Guid id)
        {
            return TryExecute(() =>
            {
                return Ok(_service.Get(id).FirstOrDefault());
            });
        }

        [HttpPost]
        public virtual async Task<IActionResult> Post([FromBody] TModel model)
        {
            return await TryExecuteAsync(async () =>
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                await _service.Insert(model);
                return Ok(model);
            });
        }

        [HttpPut("{id}")]
        public virtual async Task<IActionResult> Put(Guid id, [FromBody] TModel model)
        {
            return await TryExecuteAsync(async () =>
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                model.Id = id;
                await _service.Update(model);
                return Ok(model);
            });
        }

        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> Delete(Guid id)
        {
            return await TryExecuteAsync(async () =>
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                await _service.Delete(id);
                return Ok(id);
            });
        }

        [NonAction]
        protected virtual IActionResult TryExecute(Func<IActionResult> execute)
        {
            try
            {
                return execute();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message, ex.InnerException);
                return BadRequest(ex);
            }
        }

        [NonAction]
        protected virtual async Task<IActionResult> TryExecuteAsync(Func<Task<IActionResult>> execute)
        {
            try
            {
                return await execute();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message, ex.InnerException);
                return BadRequest(ex);
            }
        }
    }
}