﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using AutoMapper;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Activities
{
    public class List
    {
        public class ActivitiesEnvelope
        {
            public List<ActivityDto> Activities { get; set; }
            public int ActivityCount { get; set; }
        }
        public class Query : IRequest<ActivitiesEnvelope>
        {
            public Query(int? limit, int? offset, bool isGoing, bool isHost, DateTime? startDate)
            {
                Limit = limit;
                Offset = offset;
                IsGoing = isGoing;
                IsHost = isHost;
                StartDate = startDate ?? DateTime.Now;
            }

            public int? Limit { get; set; }
            public int? Offset { get; set; }
            public bool IsGoing { get; set; }
            public bool IsHost { get; set; }
            public DateTime? StartDate { get; set; }
        }
        public class Handler : IRequestHandler<Query, ActivitiesEnvelope>
        {
            private readonly DataContext _context;
            private readonly ILogger<List> _logger;
            private readonly IMapper _mapper;
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context, ILogger<List> logger, IMapper mapper,IUserAccessor userAccessor)
            {
                _context = context;
                _logger = logger;
                _mapper = mapper;
                _userAccessor = userAccessor;
            }
            /*
            public async Task<ActivitiesEnvelope> Handle(Query request, CancellationToken cancellationToken)
            {
                //try
                //{
                //    for (int i = 0; i < 10; i++)
                //    {
                //        cancellationToken.ThrowIfCancellationRequested();
                //        await Task.Delay(1000, cancellationToken);
                //        _logger.LogInformation($"Task {i} has completed");
                //    }
                //}
                //catch (Exception ex) when (ex is TaskCanceledException)
                //{
                //    _logger.LogInformation("Task was cancelled");
                //}
                var activities = await _context.Activities
                    //.Include(x=>x.UserActivities)
                    //.ThenInclude(x=>x.AppUser)
                    .ToListAsync(cancellationToken);
                return _mapper.Map<List<Activity>,List<ActivityDto>>(activities);
            }
            */
            public async Task<ActivitiesEnvelope> Handle(Query request, CancellationToken cancellationToken)
            {
                var queryable = _context.Activities
                    .Where(x => x.Date >= request.StartDate)
                    .OrderBy(x=>x.Date)
                    .AsQueryable();
                if (request.IsGoing && !request.IsHost)
                    queryable = queryable.Where(x => x.UserActivities.Any(a => a.AppUser.UserName == _userAccessor.GetCurrentUserName()));
                if(request.IsHost && !request.IsGoing)
                    queryable = queryable.Where(x => x.UserActivities.Any(a => a.AppUser.UserName == _userAccessor.GetCurrentUserName() && a.IsHost));
                var activities = await queryable
                    .Skip(request.Offset ?? 0)
                    .Take(request.Limit ?? 3)
                    .ToListAsync();
                return new ActivitiesEnvelope
                {
                    Activities = _mapper.Map<List<Activity>, List<ActivityDto>>(activities),
                    ActivityCount = queryable.Count()
                };
            }
        }
    }
}
