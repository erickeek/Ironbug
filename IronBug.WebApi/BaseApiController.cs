using AutoMapper.QueryableExtensions;
using IronBug.Mappers;
using IronBug.Pagination;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace IronBug.WebApi
{
    public class BaseApiController : ApiController
    {
        protected IHttpActionResult Map<T>(object value)
        {
            if (value != null)
                return Ok(value.Map().To<T>());

            return NotFound();
        }

        protected IHttpActionResult Page<T>(IPagedQuery query, bool withoutProjection = false)
        {
            var viewModel = MapViewModel<IEnumerable<T>, T>(query, withoutProjection);

            return Ok(new
            {
                Result = viewModel,
                query?.Total
            });
        }

        private static TCollection MapViewModel<TCollection, T>(IPagedQuery query, bool withoutProjection)
        {
            if (query == null)
                return default;

            if (withoutProjection)
                return (TCollection)query.Map().To<IEnumerable<T>>();

            return (TCollection)query.ProjectTo<T>(AutoMapperService.Instance.Mapper.ConfigurationProvider);
        }

        protected IHttpActionResult Success(string message = null, object data = null)
        {
            return Ok(new JsonData(true, message) { Data = data });
        }

        protected IHttpActionResult Success(object data)
        {
            return Ok(new JsonData(data) { Status = true });
        }

        protected IHttpActionResult Error(string message)
        {
            return Ok(new JsonData(false, message));
        }

        protected IHttpActionResult Error(Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}