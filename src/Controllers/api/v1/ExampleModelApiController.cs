// TODO DELETE THIS FILE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NetCoreBootstrap.Models.Database;
using NetCoreBootstrap.Persistance;

namespace NetCoreBootstrap.Controllers.Api.V1
{
    [Route("api/v1/[controller]")]
    public class ExampleModelApiController : Controller
    {
        public ExampleModelApiController(DataBaseContext context)
        {
            _unitOfwork = new UnitOfWork(context);
        }

        private readonly UnitOfWork _unitOfwork;

        // GET api/v1/examplemodelapi/{id}
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            return Json(_unitOfwork.ExampleModelRepository.Get(id));
        }

        // POST api/v1/examplemodelapi
        [HttpPost]
        public void Post()
        {
            int value1 = 1;
            string value2 = "fafafa";
            bool value3 = true;
            _unitOfwork.ExampleModelRepository.Add(new ExampleModel { Attribute1 = value1, Attribute2 = value2, Attribute3 = value3 });
            _unitOfwork.Complete();
        }

        // PUT api/v1/examplemodelapi/5
        [HttpPut("{id}")]
        public void Put(int id)
        {
            int value1 = 2;
            string value2 = "fafafa2";
            bool value3 = false;
            var exampleModel = new ExampleModel { Attribute1 = value1, Attribute2 = value2, Attribute3 = value3 };
            exampleModel.Id = id;
            _unitOfwork.ExampleModelRepository.Update(exampleModel);
            _unitOfwork.Complete();
        }

        // DELETE api/v1/examplemodelapi/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            var exampleModel = _unitOfwork.ExampleModelRepository.Get(id);
            _unitOfwork.ExampleModelRepository.Remove(exampleModel);
            _unitOfwork.Complete();
        }
    }
}
