using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreCodeCamp.Controllers
{
    [ApiController]
    [Route("api/camps/{moniker}/talks")]
    public class TalksController : ControllerBase
    {
        private readonly ICampRepository repository;
        private readonly IMapper mapper;
        private readonly LinkGenerator linkGenerator;

        public TalksController(ICampRepository repository, IMapper mapper, LinkGenerator linkGenerator)
        {
            this.repository = repository;
            this.mapper = mapper;
            this.linkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<ActionResult<TalkModel[]>> Get(string moniker)
        {
            try
            {
                var talks = await repository.GetTalksByMonikerAsync(moniker, true);
                return mapper.Map<TalkModel[]>(talks);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to get Talks");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TalkModel>> Get(string moniker, int id)
        {
            try
            {
                var talk = await repository.GetTalkByMonikerAsync(moniker, id, true);
                if (talk == null)
                {
                    return NotFound();
                }
                return mapper.Map<TalkModel>(talk);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to get Talk");
            }
        }

        [HttpPost]
        public async Task<ActionResult<TalkModel>> Post(string moniker, TalkModel model)
        {
            try
            {
                var camp = await repository.GetCampAsync(moniker);
                if (camp == null)
                {
                    return BadRequest("moniker does not exist");
                }
                var talk = mapper.Map<Talk>(model);
                talk.Camp = camp;
                if (model.Speaker == null)
                {
                    return BadRequest("speaker.speakerId not specified");
                }
                var speaker = await repository.GetSpeakerAsync(model.Speaker.SpeakerId);
                if (speaker == null)
                {
                    return BadRequest("speaker.speakerID not found");
                }
                talk.Speaker = speaker;

                repository.Add(talk);

                if (await repository.SaveChangesAsync())
                {
                    var url = linkGenerator.GetPathByAction(HttpContext, "Get", values: new { moniker, id = talk.TalkId });
                    return Created(url, mapper.Map<TalkModel>(talk));
                }
                else
                {
                    return BadRequest("Error creating talk");
                }
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create Talk");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<TalkModel>> Put (string moniker, int id, TalkModel model)
        {
            try
            {
                var talk = await repository.GetTalkByMonikerAsync(moniker, id, true);
                if (talk == null) return NotFound("Talk not found");
                if (model.Speaker == null)
                {
                    return BadRequest("Speaker not specified");
                }
                else
                {
                    var speaker = await repository.GetSpeakerAsync(model.Speaker.SpeakerId);
                    if (speaker == null)
                    {
                        return BadRequest("Speaker not found");
                    }
                    else
                    {
                        talk.Speaker = speaker;
                    }
                }
                mapper.Map(model, talk);
                if (await repository.SaveChangesAsync())
                {
                    return mapper.Map<TalkModel>(talk);
                }
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to update Talk");
            }
            return BadRequest("Could not update talk");
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(string moniker, int id)
        {
            try
            {
                var talk = await repository.GetTalkByMonikerAsync(moniker, id);
                if (talk == null)
                {
                    return NotFound("Could not find talk to delete");
                }

                repository.Delete(talk);
                if (await repository.SaveChangesAsync())
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("Could not delete talk");
                }
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to delete Talk");
            }
        }
    }
}
