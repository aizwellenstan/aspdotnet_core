using DemoWebAPIModelValidationResponse.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace DemoWebAPIModelValidationResponse.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<int> Get(int id)
        {
            return id;
        }

        // POST api/values
        [HttpPost]
        public ActionResult Post([FromBody] ComplexModel complexModel)
        {
            return new JsonResult(complexModel);
        }
    }
}
