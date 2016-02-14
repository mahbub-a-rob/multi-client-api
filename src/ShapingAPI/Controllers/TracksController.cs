﻿using AutoMapper;
using Microsoft.AspNet.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShapingAPI.Entities;
using ShapingAPI.Infrastructure.Core;
using ShapingAPI.Infrastructure.Data.Repositories;
using ShapingAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ShapingAPI.Controllers
{
    [Route("api/[controller]")]
    public class TracksController : Controller
    {
        #region Properties
        private readonly ITrackRepository _trackRepository;
        private List<string> _properties = new List<string>();
        private Expression<Func<Track, object>>[] includeProperties;
        private const int maxSize = 50;
        #endregion

        #region Constructor
        public TracksController(ITrackRepository trackRepository)
        {
            _trackRepository = trackRepository;

            _properties = new List<string>();
            includeProperties = Expressions.LoadTrackNavigations();
        }
        #endregion

        #region Actions
        public ActionResult Get(string props = null, int page = 1, int pageSize = maxSize)
        {
            try
            {
                var _tracks = _trackRepository.GetAll(includeProperties).Skip(page).Take(pageSize);

                var _tracksVM = Mapper.Map<IEnumerable<Track>, IEnumerable<TrackViewModel>>(_tracks);

                string _serializedTracks = JsonConvert.SerializeObject(_tracksVM, Formatting.None,
                    new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });

                JToken _jtoken = JToken.Parse(_serializedTracks);
                if (!string.IsNullOrEmpty(props))
                    Utils.FilterProperties(_jtoken, props.ToLower().Split(',').ToList());

                return Ok(_jtoken);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500);
            }
        }

        [Route("{trackId}")]
        public ActionResult Get(int trackId, string props = null)
        {
            try
            {


                var _track = _trackRepository.Get(t => t.TrackId == trackId, includeProperties);

                if (_track == null)
                {
                    return HttpNotFound();
                }

                var _trackVM = Mapper.Map<Track, TrackViewModel>(_track);

                string _serializedTrack = JsonConvert.SerializeObject(_trackVM, Formatting.None,
                    new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });

                JToken _jtoken = JToken.Parse(_serializedTrack);
                if (!string.IsNullOrEmpty(props))
                    Utils.FilterProperties(_jtoken, props.ToLower().Split(',').ToList());
                return Ok(_jtoken);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500);
            }
        }
        #endregion
    }
}