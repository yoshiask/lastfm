using System;
using System.Net.Http;
using System.Threading.Tasks;
using IF.Lastfm.Core.Api.Enums;
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IF.Lastfm.Core.Api.Commands.Track
{
    [ApiMethodName("track.getSimilar")]
    internal class GetSimilarCommand : GetAsyncCommandBase<PageResponse<LastTrack>>
    {
        public string ArtistName { get; set; }

        public int? Limit { get; set; }

        public bool Autocorrect { get; set; }

        public string TrackName { get; set; }

        public string TrackMbid { get; set; }

        public GetSimilarCommand(ILastAuth auth, string trackName, string artistName)
            : base(auth)
        {
            ArtistName = artistName;
            TrackName = trackName;
        }

        public GetSimilarCommand(ILastAuth auth, string trackMbid)
            : base(auth)
        {
            TrackMbid = trackMbid;
        }

        public override void SetParameters()
        {
            if (TrackMbid != null)
            {
                Parameters.Add("mbid", TrackMbid);
            }
            else
            {
                Parameters.Add("track", TrackName);
                Parameters.Add("artist", ArtistName);

                Parameters.Add("autocorrect", Convert.ToInt32(Autocorrect).ToString());
            }

            if (Limit != null)
            {
                Parameters.Add("limit", Limit.ToString());
            }
            
            DisableCaching();
        }

        public override async Task<PageResponse<LastTrack>> HandleResponse(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();

            LastResponseStatus status;
            if (LastFm.IsResponseValid(json, out status) && response.IsSuccessStatusCode)
            {
                var jtoken = JsonConvert.DeserializeObject<JToken>(json);
                var itemsToken = jtoken.SelectToken("similartracks").SelectToken("track");

                return PageResponse<LastTrack>.CreateSuccessResponse(itemsToken, LastTrack.ParseJToken);
            }
            else
            {
                return LastResponse.CreateErrorResponse<PageResponse<LastTrack>>(status);
            }
        }
    }
}
