﻿using HotChocolate.Subscriptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using ToDoQL.Data;
using ToDoQL.GraphQL;
using ToDoQL.Models;

namespace ToDoQL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITopicEventSender _topicEventSender;

        public ItemsController(
            AppDbContext context,
            ITopicEventSender topicEventSender)
        {
            _context = context;
            _topicEventSender = topicEventSender;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _context.Items
                .Select(x => new
                {
                    x.Id,
                    x.Title,
                    x.Description,
                    x.IsDone,
                    ItemList = new
                    {
                        x.ItemList.Id,
                        x.ItemList.Name,
                    }
                })
                .ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AddItemInput input)
        {
            var item = new Item
            {
                Title = input.Title,
                Description = input.Description,
                IsDone = input.IsDone,
                ListId = input.ListId,
            };
            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            var response = new AddItemResponse(
                item.Id,
                item.Title,
                item.Description,
                item.IsDone);

            await _topicEventSender.SendAsync(GraphQLConstants.ITEM_CREATION_TOPIC, response);

            return Ok(response);
        }
    }
}
