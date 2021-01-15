using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Snippet.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SnippetsController : ControllerBase
    {
        private static Dictionary<string, SnippetResponseWithPassword> snippetResponses = new Dictionary<string, SnippetResponseWithPassword>();
        private const int DEFAULT_INCREMENT=30;
        private readonly ILogger<SnippetsController> _logger;

        public SnippetsController(ILogger<SnippetsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("{name}")]
        public async Task<IActionResult> Get(string name)
        {
            if(!snippetResponses.ContainsKey(name))
            {
                //no response found 
                return NotFound();
            }
            else
            {
                //try and get a response
                SnippetResponseWithPassword snippetResponseWithPassword = snippetResponses[name];
                if(snippetResponseWithPassword.response.expires_at<DateTime.Now)
                {
                    //respose found but is expired
                    snippetResponses.Remove(name);
                    return NotFound();
                }

                //valid response found so update expires_at and return it
                snippetResponseWithPassword.response.expires_at = snippetResponseWithPassword.response.expires_at.AddSeconds(DEFAULT_INCREMENT);
                snippetResponses[name] = snippetResponseWithPassword;
                return Ok(snippetResponseWithPassword.response);
            }
        }
 
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Snippet snippet)
        {
            if(snippetResponses.ContainsKey(snippet.name))
            {
                //snippet exists
                SnippetResponseWithPassword snippetResponseWithPassword = snippetResponses[snippet.name];
                
                //check password
                if(snippetResponseWithPassword.password==snippet.password)
                {
                    snippetResponseWithPassword.response = new SnippetResponse{
                        url = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/snippet/{snippet.name}",
                        name = snippet.name,
                        expires_at = snippet.expires_in.HasValue? DateTime.Now.AddSeconds(snippet.expires_in.Value) : DateTime.Now.AddSeconds(DEFAULT_INCREMENT),
                        snippet = snippet.snippet
                    };
                    return Ok(snippetResponseWithPassword.response);
                }
                else
                {
                    return Unauthorized();
                }
            }
            else
            {
                //does not exist so create it
                SnippetResponseWithPassword snippetResponseWithPassword = new SnippetResponseWithPassword {
                    password = snippet.password,
                    response = new SnippetResponse{
                        url = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/snippet/{snippet.name}",
                        name = snippet.name,
                        expires_at = DateTime.Now.AddSeconds(DEFAULT_INCREMENT),
                        snippet = snippet.snippet
                    }
                };
                snippetResponses[snippet.name] = snippetResponseWithPassword;
                return Created(snippetResponseWithPassword.response.url, snippetResponseWithPassword.response);
            }
        }
    }
}
